using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using osum.GameplayElements.Beatmaps;
using osum.GameplayElements;
using osu_common.Libraries.Osz2;
using System.Globalization;
using osum.GameModes;
using osum.Helpers;
using osum.GameplayElements.Scoring;
using osum;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using osum.Support;
using osum.Audio;
using osum.GameModes.Play;
using OpenTK;
using osum.GameplayElements.HitObjects.Osu;

namespace osum
{
    public class BeatmapDifficulty : Beatmap
    {
        internal string VersionName;
        internal List<HitObjectLine> HitObjectLines = new List<HitObjectLine>();
        internal List<string> HeaderLines = new List<string>();

        internal double VelocityAt(int time)
        {
            return (100000.0f * DifficultySliderMultiplier / beatLengthAt(time, true));
        }

        internal double ScoringDistanceAt(int time)
        {
            return ((100 * DifficultySliderMultiplier / bpmMultiplierAt(time)) / DifficultySliderTickRate);
        }
    }

    public class HitObjectLine : IComparable<HitObjectLine>
    {
        internal string StringRepresentation;
        internal int Time;

        public int CompareTo(HitObjectLine h)
        {
            return Time.CompareTo(h.Time);
        }
    }

    public class BeatmapCombinator
    {
        internal static readonly NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
        private static List<string> headerContent;
        private static double healthMultiplier;

        static bool DistBuild;
        public static bool Analysis;

        /// <summary>
        /// Combines many .osu files into one .osc
        /// </summary>
        /// <param name="args">Directory containing many .osu files</param>
        static void Main(string[] args)
        {
            try
            {
                if (args.Length > 0)
                {
                    DistBuild = true;
                    Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().GetName().CodeBase.Replace("file:///", ""));
                    Process(args[0], true, true, true, true);
                    Process(args[0], false, true, true, false);
                }
                else
                {
                    Console.WriteLine("Please drag a beatmap folder onto this app's icon!");
                    Console.WriteLine("Note that it must contain at least one of the following difficulties:");
                    Console.WriteLine();
                    Console.WriteLine("Easy | Normal | Hard | Expert");
                    Console.WriteLine("All other difficulty names will be ignored!");
                    Console.ReadLine();
                }
            }
            catch (Exception e)
            {
                Console.Write("An error occurred during combination:\n" + e.ToString() + "\n");
                Console.ReadLine();
            }

            Thread.Sleep(2000);
        }

        public static string Process(string dir, bool quick = false, bool usem4a = true, bool free = false, bool previewMode = false)
        {
            Console.WriteLine("Combinating beatmap: " + dir.Split('\\').Last(s => s == s));
            Console.WriteLine();

            if (dir.Length < 1)
            {
                Console.WriteLine("No path specified!");
                return null;
            }

            List<string> osuFiles = new List<string>(Directory.GetFiles(dir, "*.osu"));

            if (osuFiles.Count < 1)
            {
                Console.WriteLine("No .osu files found!");
                return null;
            }

            List<string> orderedDifficulties = new List<string>();

            orderedDifficulties.AddRange(osuFiles);
            
            //orderedDifficulties.Add(osuFiles.Find(f => f.EndsWith("[Easy].osu")));
            //orderedDifficulties.Add(osuFiles.Find(f => f.EndsWith("[Normal].osu")));
            //orderedDifficulties.Add(osuFiles.Find(f => f.EndsWith("[Hard].osu")));
            //orderedDifficulties.Add(osuFiles.Find(f => f.EndsWith("[Expert].osu")));

            if (orderedDifficulties.FindAll(t => t != null).Count < 1) return null;

            Console.WriteLine("Files found:");
            foreach (string s in orderedDifficulties)
                Console.WriteLine("    * " + Path.GetFileName(s));
            Console.WriteLine();
            Console.WriteLine();

            List<BeatmapDifficulty> difficulties = new List<BeatmapDifficulty>();

            foreach (string f in orderedDifficulties)
            {
                if (f == null)
                {
                    difficulties.Add(null);
                    continue;
                }

                BeatmapDifficulty bd = new BeatmapDifficulty();
                difficulties.Add(bd);

                string currentSection = "";

                foreach (string line in File.ReadAllLines(f))
                {
                    string writeLine = line;

                    if (line.StartsWith("Version:"))
                    {
                        bd.VersionName = line.Replace("Version:", "");
                        continue;
                    }

                    if (line.StartsWith("["))
                        currentSection = line.Replace("[", "").Replace("]", "");
                    else if (line.Length > 0)
                    {
                        string[] split = line.Split(',');
                        string[] var = line.Split(':');
                        string key = string.Empty;
                        string val = string.Empty;
                        if (var.Length > 1)
                        {
                            key = var[0].Trim();
                            val = var[1].Trim();
                        }

                        switch (currentSection)
                        {
                            case "General":
                                switch (key)
                                {
                                    case "AudioFilename":
                                        writeLine = "AudioFilename: audio.mp3";
                                        break;
                                }
                                break;
                            case "Difficulty":
                                switch (key)
                                {
                                    case "HPDrainRate":
                                        bd.DifficultyHpDrainRate = Math.Min((byte)10, Math.Max((byte)0, byte.Parse(val)));
                                        break;
                                    case "CircleSize":
                                        bd.DifficultyCircleSize = Math.Min((byte)10, Math.Max((byte)0, byte.Parse(val)));
                                        break;
                                    case "OverallDifficulty":
                                        bd.DifficultyOverall = Math.Min((byte)10, Math.Max((byte)0, byte.Parse(val)));
                                        //if (!hasApproachRate) DifficultyApproachRate = DifficultyOverall;
                                        break;
                                    case "SliderMultiplier":
                                        bd.DifficultySliderMultiplier =
                                            Math.Max(0.4, Math.Min(3.6, Double.Parse(val, nfi)));
                                        break;
                                    case "SliderTickRate":
                                        bd.DifficultySliderTickRate =
                                            Math.Max(0.5, Math.Min(8, Double.Parse(val, nfi)));
                                        break;
                                }
                                break;
                            case "HitObjects":
                                {
                                    HitObjectType type = (HitObjectType)Int32.Parse(split[3]) & ~HitObjectType.ColourHax;
                                    bool slider = (type & HitObjectType.Slider) > 0;
                                    bool spinner = (type & HitObjectType.Spinner) > 0;
                                    int time = (int)Decimal.Parse(split[2], nfi);
                                    int endTime = spinner ? endTime = (int)Decimal.Parse(split[5], nfi) : time;

                                    int repeatCount = 0; double length = 0; bool hadEndpointSamples = false;
                                    bool hold = false;
                                    SampleSet ss = SampleSet.None, ssa = SampleSet.None;
                                    string[] samplestring = null;

                                    if (slider)
                                    {
                                        repeatCount = Int32.Parse(split[6], nfi);
                                        length = double.Parse(split[7], nfi);
                                        hadEndpointSamples = split.Length >= 9;

                                        hold = (repeatCount > 1 && length < 50) ||
                                               (repeatCount > 4 && length < 100) ||
                                               (hadEndpointSamples && split[4] == "4");

                                        if (split.Length > 10)
                                        {
                                            samplestring = split[10].Split(':');
                                        }
                                    }
                                    else if (spinner)
                                    {
                                        if (split.Length > 6)
                                        {
                                            samplestring = split[6].Split(':');
                                        }
                                    }
                                    else
                                    {
                                        if (split.Length > 5)
                                        {
                                            samplestring = split[5].Split(':');
                                        }
                                    }

                                    if (samplestring != null)
                                    {
                                        ss = (SampleSet)Convert.ToInt32(samplestring[0]);
                                        if (samplestring.Length > 0)
                                            ssa = (SampleSet)Convert.ToInt32(samplestring[1]);
                                    }

                                    // take the slider's slide sampleset from 20ms after the head in case the head has a different sampleset
                                    ControlPoint cp = bd.controlPointAt(slider ? time + 20 : endTime + 5);

                                    StringBuilder builder = new StringBuilder();
                                    builder.Append(MakeSampleset(cp, ss, ssa));

                                    // Object commons
                                    builder.Append(',');
                                    builder.Append(split[0]); // X

                                    builder.Append(',');
                                    builder.Append(split[1]); // Y

                                    builder.Append(',');
                                    builder.Append(time.ToString(nfi)); // time

                                    HitObjectType type2 = (HitObjectType)Int32.Parse(split[3]);
                                    builder.Append(',');
                                    builder.Append(hold ? (int)(type2 | HitObjectType.Hold) : (int)type2); // object type

                                    string soundAdditions = MakeSoundAdditions(split[4]);
                                    builder.Append(',');
                                    builder.Append(soundAdditions); // sound additions

                                    //add addition difficulty-specific information
                                    if (slider)
                                    {
                                        builder.Append(',');
                                        builder.Append(split[5]); // curve type, all control points

                                        builder.Append(',');
                                        builder.Append(repeatCount.ToString(nfi)); // repeat count

                                        builder.Append(',');
                                        builder.Append(length.ToString(nfi)); // curve length

                                        string[] additions;
                                        if (hadEndpointSamples) additions = split[8].Split('|');
                                        else additions = new string[0];

                                        // nodal hitsamples
                                        builder.Append(',');
                                        for (int repeatNo = 0; repeatNo <= repeatCount; repeatNo++)
                                        {
                                            if (repeatNo > 0) builder.Append('|');
                                            if (repeatNo < additions.Length) builder.Append(MakeSoundAdditions(additions[repeatNo]));
                                            else builder.Append(soundAdditions);
                                        }

                                        double velocity = bd.VelocityAt(time);

                                        //velocity and scoring distance.
                                        builder.Append(',');
                                        builder.Append(velocity.ToString(nfi));

                                        builder.Append(',');
                                        builder.Append(bd.ScoringDistanceAt(time).ToString(nfi));

                                        double ReboundTime = 1000 * length / velocity;

                                        double currTime = time;
                                        cp = bd.controlPointAt(currTime + 5);

                                        string[] node_samples;
                                        if (split.Length > 9)
                                        {
                                            // osu!'s separator is different
                                            node_samples = split[9].Split('|');
                                        }
                                        else
                                        {
                                            node_samples = new string[0];
                                        }
                                        // nodal samplesets
                                        for (int repeatNo = 0; repeatNo <= repeatCount; repeatNo++)
                                        {
                                            SampleSet node_ss = ss;
                                            SampleSet node_ssa = ssa;

                                            if (repeatNo < node_samples.Length)
                                            {
                                                string[] pair = node_samples[repeatNo].Split(':');
                                                node_ss = (SampleSet)Convert.ToInt32(pair[0]);
                                                if (pair.Length > 0)
                                                    node_ssa = (SampleSet)Convert.ToInt32(pair[1]);
                                            }

                                            cp = bd.controlPointAt(currTime + 5);
                                            builder.Append(repeatNo == 0 ? ',' : ':');
                                            builder.Append(MakeSampleset(cp, node_ss, node_ssa));
                                            currTime += ReboundTime;
                                        }
                                    }

                                    if (spinner)
                                    {
                                        builder.Append(',');
                                        builder.Append(split[5]); // end time
                                    }

                                    bd.HitObjectLines.Add(new HitObjectLine() { StringRepresentation = builder.ToString(), Time = Int32.Parse(line.Split(',')[2]) });
                                    continue; //skip direct output
                                }
                            case "TimingPoints":
                                {
                                    ControlPoint cp = new ControlPoint(Double.Parse(split[0], nfi),
                                                                 Double.Parse(split[1], nfi),
                                                                 split[2][0] == '0' ? TimeSignatures.SimpleQuadruple :
                                                                 (TimeSignatures)Int32.Parse(split[2]),
                                                                 (SampleSet)Int32.Parse(split[3]),
                                                                 split.Length > 4
                                                                     ? (CustomSampleSet)Int32.Parse(split[4])
                                                                     : CustomSampleSet.Default,
                                                                 Int32.Parse(split[5]),
                                                                 split.Length > 6 ? split[6][0] == '1' : true,
                                                                 split.Length > 7 ? split[7][0] == '1' : false);
                                    bd.ControlPoints.Add(cp);
                                    break;
                                }
                        }
                    }

                    bd.HeaderLines.Add(writeLine);
                }
            }


            string metadata = dir + "\\metadata.txt";
            string Artist = "", Title = "", Creator = "";
            if (File.Exists(metadata))
            {
                foreach (string line in File.ReadAllLines(metadata))
                {
                    if (line.Length == 0) continue;

                    string[] var = line.Split(':');
                    string key = string.Empty;
                    string val = string.Empty;
                    if (var.Length > 1)
                    {
                        key = line.Substring(0, line.IndexOf(':'));
                        val = line.Substring(line.IndexOf(':') + 1).Trim();
                        switch (key)
                        {
                            case "Artist":
                                Artist = val;
                                break;
                            case "Title":
                                Title = val;
                                break;
                            case "Creator":
                                Creator = val;
                                break;
                        }
                    }
                }
            }

            string baseName = Artist + " - " + Title + " (" + Creator + ")";
            string oscFilename = baseName + ".osc";

            foreach (BeatmapDifficulty d in difficulties)
                if (d != null) ListHelper.StableSort(d.HitObjectLines);

            headerContent = difficulties.Find(d => d != null).HeaderLines;

            string[] splitdir = dir.Split('\\');
            string upOneDir = string.Join("\\", splitdir, 0, splitdir.Length - 1);

            string osz2Filename;

            string baseFileWithLocation = baseName.Substring(baseName.LastIndexOf("\\") + 1);

            if (free && DistBuild)
                osz2Filename = baseFileWithLocation + ".osf2";
            else
                osz2Filename = baseFileWithLocation + (usem4a && !DistBuild ? ".m4a.osz2" : ".osz2");

            string audioFilename = null;

            if (usem4a)
            {
                audioFilename = "";
                foreach (string s in Directory.GetFiles(dir, "*.m4a"))
                {
                    if (s.Contains("_lq")) continue;

                    audioFilename = s;
                    break;
                }
            }
            else
                audioFilename = Directory.GetFiles(dir, "*.mp3")[0];

            File.Delete(osz2Filename);

            //write the package initially so we can use it for score testing purposes.
            writePackage(oscFilename, osz2Filename, audioFilename, difficulties, orderedDifficulties);

            //scoring

            Player.Beatmap = new Beatmap(osz2Filename);
            Player.Autoplay = true;

            //Working on the scoring algorithm for osu!s
            //Basically I need to calculate the total possible score from hitobjects before any multipliers kick in...
            //but I need to know this before the beatmap is loaded.

            //So this means running through the beatmap as if it was being played at the time of package creation
            //(inside BeatmapCombinator).  After I find the score that can be achieved, I can figure out what multiplier
            //i need in order to pad it out to a fixed 1,000,000 max score.

            //I am sure I will run into some rounding issues once I get that far, but we'll see how things go :p.

            ITimeSource oldTimeSource = Clock.AudioTimeSource;
            FakeAudioTimeSource source = new FakeAudioTimeSource();
            Clock.AudioTimeSource = source;

            SoundEffectPlayer oldEffect = AudioEngine.Effect;
            BackgroundAudioPlayer oldMusic = AudioEngine.Music;

            AudioEngine.Music = null;
            AudioEngine.Effect = null;

            headerContent.Remove("[HitObjects]");

            headerContent.Add(string.Empty);
            headerContent.Add("[ScoringMultipliers]");

            if (quick)
            {
                processDifficulty(Difficulty.Easy, true);
                processDifficulty(Difficulty.Normal, true);
                processDifficulty(Difficulty.Hard, true);
                processDifficulty(Difficulty.Expert, true);
            }
            else
            {
                if (orderedDifficulties[(int)Difficulty.Easy] != null)
                    headerContent.Add("0: " + processDifficulty(Difficulty.Easy).ToString("G17", nfi));
                if (orderedDifficulties[(int)Difficulty.Normal] != null)
                    headerContent.Add("1: " + processDifficulty(Difficulty.Normal).ToString("G17", nfi));
                if (orderedDifficulties[(int)Difficulty.Expert] != null)
                    headerContent.Add("3: " + processDifficulty(Difficulty.Expert).ToString("G17", nfi));
            }

            if (healthMultiplier != 0)
                headerContent.Add("HP:" + healthMultiplier.ToString("G17", nfi));

            headerContent.Add(string.Empty);
            headerContent.Add("[HitObjects]");

            Player.Beatmap.Dispose();

            Clock.AudioTimeSource = oldTimeSource;
            AudioEngine.Effect = oldEffect;
            AudioEngine.Music = oldMusic;

            //only change the filename here so it is not treated as a preview above (else previewpoints will not be filled before necessary).
            if (previewMode) osz2Filename = osz2Filename.Replace(".osf2", "_preview.osf2");

            //write the package a second time with new multiplier header data.
            writePackage(oscFilename, osz2Filename, audioFilename, difficulties, orderedDifficulties);

            return osz2Filename;
        }

        private static void writePackage(string oscFilename, string osz2Filename, string audioFilename, List<BeatmapDifficulty> difficulties, List<string> ordered)
        {
            bool isPreview = osz2Filename.Contains("_preview");

            int hitObjectCutoff = 0;

            using (StreamWriter output = new StreamWriter(oscFilename))
            {
                //write headers first (use first difficulty as arbitrary source)
                foreach (string l in headerContent)
                {
                    if (isPreview)
                    {
                        if (l.StartsWith("Bookmarks:") && osz2Filename.Contains("_preview"))
                        {
                            //may need to double up on bookmarks if they don't occur often often

                            List<int> switchPoints = Player.Beatmap.StreamSwitchPoints;

                            if (switchPoints.Count < 10 || switchPoints[9] > 60000)
                            {
                                string switchString = "Bookmarks:";

                                foreach (int s in switchPoints)
                                {
                                    switchString += s.ToString(nfi) + ",";
                                    switchString += s.ToString(nfi) + ","; //double bookmark hack for previews
                                }

                                output.WriteLine(switchString.Trim(','));

                                hitObjectCutoff = switchPoints.Count < 10 ? switchPoints[4] : switchPoints[9];
                                continue;
                            }
                        }
                    }
                    output.WriteLine(l);
                }

                //keep track of how many hitObject lines are remaining for each difficulty
                int[] linesRemaining = new int[difficulties.Count];
                for (int i = 0; i < difficulties.Count; i++)
                {
                    linesRemaining[i] = difficulties[i] == null ? 0 : difficulties[i].HitObjectLines.Count;
                }

                int currentTime = 0;

                while (!linesRemaining.All(i => i == 0))
                {
                    int bestMatchDifficulty = -1;
                    HitObjectLine bestMatchLine = null;

                    for (int i = 0; i < difficulties.Count; i++)
                    {
                        if (linesRemaining[i] == 0)
                            continue;

                        int holOffset = difficulties[i].HitObjectLines.Count - linesRemaining[i];

                        HitObjectLine line = difficulties[i].HitObjectLines[holOffset];

                        if (isPreview && hitObjectCutoff > 0 && line.Time > hitObjectCutoff)
                        {
                            linesRemaining[i]--;
                            continue;
                        }

                        if (line.Time >= currentTime && (bestMatchLine == null || line.Time < bestMatchLine.Time))
                        {
                            bestMatchDifficulty = i;
                            bestMatchLine = line;
                        }
                    }

                    if (bestMatchLine != null)
                    {
                        output.WriteLine(bestMatchDifficulty + "," + bestMatchLine.StringRepresentation);
                        linesRemaining[bestMatchDifficulty]--;
                    }
                }
            }

            using (MapPackage package = new MapPackage(osz2Filename, true))
            {
                package.AddMetadata(MapMetaType.BeatmapSetID, "0");

                string versionsAvailable = "";
                if (ordered[0] != null) versionsAvailable += "|Easy";
                if (ordered[1] != null) versionsAvailable += "|Normal";
                if (ordered[2] != null) versionsAvailable += "|Hard";
                if (ordered[3] != null) versionsAvailable += "|Expert";
                package.AddMetadata(MapMetaType.Version, versionsAvailable.Trim('|'));

                if (string.IsNullOrEmpty(audioFilename))
                    throw new Exception("FATAL ERROR: audio file not found");

                package.AddFile(Path.GetFileName(oscFilename), oscFilename, DateTime.MinValue, DateTime.MinValue);
                if (isPreview)
                {
                    if (!File.Exists(audioFilename.Replace(".m4a", "_lq.m4a")))
                    {
                        Console.WriteLine("WARNING: missing preview audio file (_lq.m4a)");
                        return;
                    }
                    package.AddFile("audio.m4a", audioFilename.Replace(".m4a", "_lq.m4a"), DateTime.MinValue, DateTime.MinValue);
                }
                else
                    package.AddFile(audioFilename.EndsWith(".m4a") ? "audio.m4a" : "audio.mp3", audioFilename, DateTime.MinValue, DateTime.MinValue);

                string dir = Path.GetDirectoryName(audioFilename);

                string metadata = dir + "\\metadata.txt";
                if (File.Exists(metadata))
                {
                    foreach (string line in File.ReadAllLines(metadata))
                    {
                        if (line.Length == 0) continue;

                        string[] var = line.Split(':');
                        string key = string.Empty;
                        string val = string.Empty;
                        if (var.Length > 1)
                        {
                            key = line.Substring(0, line.IndexOf(':'));
                            val = line.Substring(line.IndexOf(':') + 1).Trim();

                            MapMetaType t = (MapMetaType)Enum.Parse(typeof(MapMetaType), key, true);
                            package.AddMetadata(t, val);
                        }
                    }
                }

                if (isPreview)
                    package.AddMetadata(MapMetaType.Revision, "preview");

                string thumb = dir + "\\thumb-128.jpg";
                if (File.Exists(thumb))
                    package.AddFile("thumb-128.jpg", thumb, DateTime.MinValue, DateTime.MinValue);
                thumb = Path.GetDirectoryName(audioFilename) + "\\thumb-256.jpg";
                if (File.Exists(thumb))
                    package.AddFile("thumb-256.jpg", thumb, DateTime.MinValue, DateTime.MinValue);

                package.Save();
            }
        }

        private static string MakeSampleset(ControlPoint cp, SampleSet ss, SampleSet ssa)
        {
            SampleSet _ss = ss == SampleSet.None ? cp.sampleSet : ss;
            SampleSet _ssa = ssa == SampleSet.None ? ss : ssa;

            string result = ((int)_ss).ToString();
            bool b1 = cp.volume != 100;
            bool b2 = _ssa != SampleSet.None && _ssa != _ss;

            if (b1 || b2)
            {
                result += "|" + cp.volume.ToString();
                if (b2)
                {
                    result += "|" + ((int)_ssa).ToString();
                }
            }
            return result;
        }

        private static string MakeSoundAdditions(string rep)
        {
            HitObjectSoundType sounds = (HitObjectSoundType)Convert.ToInt32(rep);
            sounds |= HitObjectSoundType.Normal;
            return ((int)sounds).ToString(nfi);
        }

        /// <summary>
        /// Does various calculations for each difficulty.
        /// </summary>
        /// <param name="difficulty">The difficulty to process</param>
        /// <param name="quick">Skips score normalisation if true</param>
        /// <returns>Score multiplier if quick is not set to true, else 0</returns>
        private static double processDifficulty(Difficulty difficulty, bool quick = false)
        {
            Console.Write("Processing " + difficulty);

            double comboMultiplier = 1;

            Player.Difficulty = difficulty;
            Player.Autoplay = true;

            Score s = null;

            using (Player p = new PlayCombinate())
            {
                p.Initialize();

                if (p.HitObjectManager.ActiveStreamObjects == null || p.HitObjectManager.ActiveStreamObjects.Count == 0)
                {
                    Console.WriteLine(" Failed!");
                    return 0;
                }

                HitObject switchHpObject = null;
                if (difficulty == Difficulty.Normal && Player.Beatmap.StreamSwitchPoints != null && Player.Beatmap.StreamSwitchPoints.Count > 0)
                {
                    //stream mode specific. make sure we have enough hp to hit the first stream switch
                    int testStreamSwitch = Player.Beatmap.StreamSwitchPoints[0] - DifficultyManager.PreEmpt;
                    int index = p.HitObjectManager.ActiveStreamObjects.FindIndex(h => { return h.StartTime > testStreamSwitch; }) - 1;
                    //take one from the index. we need to be at max HP *before* the preempt-take-switch.

                    while (p.HitObjectManager.ActiveStreamObjects[index].EndTime > testStreamSwitch)
                        index--;

                    if (index <= 0)
                        throw new Exception("Bookmark exists before first object! Please only use bookmarks for stream switch points.");
                    switchHpObject = p.HitObjectManager.ActiveStreamObjects[index];
                    
                }

                FakeAudioTimeSource source = new FakeAudioTimeSource();
                Clock.AudioTimeSource = source;

                while (true)
                {
                    if (source.InternalTime % 10 < 0.01)
                        Console.Write(".");

                    Clock.UpdateCustom(0.01);
                    source.InternalTime += 0.01;
                    Clock.ElapsedMilliseconds = 10;

                    p.Update();

                    if (switchHpObject != null && switchHpObject.IsHit)
                    {
                        double currentHp = p.healthBar.CurrentHpUncapped;

                        healthMultiplier = (HealthBar.HP_BAR_MAXIMUM - HealthBar.HP_BAR_INITIAL + 5) / (currentHp - HealthBar.HP_BAR_INITIAL);
                        Player.Beatmap.HpStreamAdjustmentMultiplier = healthMultiplier;

                        switchHpObject = null;
                    }

                    if (p.Completed)
                    {
                        s = p.CurrentScore;
                        s.UseAccuracyBonus = true;

                        int excess = s.totalScore - s.spinnerBonusScore - 1000000;

                        double testMultiplier = (double)(s.comboBonusScore - excess) / s.comboBonusScore;

                        comboMultiplier = testMultiplier * 0.97f;
                        break;
                    }
                }
            }

            Console.WriteLine();

            if (Analysis)
                checkOverlaps(difficulty);

            int finalScore = 0;

            if (!quick)
            {
                double adjustment = 0;

                while (finalScore < 1000000)
                {
                    Console.Write(".");

                    Player.Difficulty = difficulty;
                    Player.Autoplay = true;

                    Player.Beatmap.DifficultyInfo[difficulty] = new BeatmapDifficultyInfo(difficulty) { ComboMultiplier = comboMultiplier };

                    //let's do some test runs
                    using (Player p = new PlayCombinate())
                    {
                        p.Initialize();

                        FakeAudioTimeSource source = new FakeAudioTimeSource();
                        Clock.AudioTimeSource = source;

                        while (true)
                        {
                            Clock.UpdateCustom(0.01);
                            source.InternalTime += 0.01;

                            p.Update();

                            if (p.Completed)
                            {
                                s = p.CurrentScore;
                                s.UseAccuracyBonus = true;

                                finalScore = (s.totalScore - s.spinnerBonusScore);

                                if (finalScore < 1000000)
                                {
                                    int fellShortBy = 1000000 - finalScore;
                                    adjustment = (double)fellShortBy / s.comboBonusScore;
                                    comboMultiplier += adjustment;
                                }
                                break;
                            }
                        }
                    }
                }
            }


            Console.WriteLine("Done");
            Console.WriteLine();

            if (!quick)
            {
                Console.WriteLine("HP multiplier: ".PadRight(25) + healthMultiplier);
                Console.WriteLine("Using combo multiplier: ".PadRight(25) + comboMultiplier);
                Console.WriteLine("Hitobject score: ".PadRight(25) + s.hitScore);
                Console.WriteLine("Combo score: ".PadRight(25) + s.comboBonusScore);
                Console.WriteLine("Spin score: ".PadRight(25) + s.spinnerBonusScore);
                Console.WriteLine("Accuracy score: ".PadRight(25) + s.accuracyBonusScore);
                Console.WriteLine("Total score: ".PadRight(25) + s.totalScore);
                Console.WriteLine("Total score (no spin): ".PadRight(25) + finalScore);
            }

            //i guess the best thing to do might be to aim slightly above 1m and ignore the excess...
            //okay now we have numbers roughly around 1mil (always higher or equal to).
            //need to do something about this static, then load them up in osu!s.
            return comboMultiplier;
        }

        private static void checkOverlaps(Difficulty difficulty)
        {
            Console.Write("Searching for overlaps", difficulty);
            checkOverlaps(difficulty, Difficulty.None);
            Console.WriteLine();

            if (difficulty == Difficulty.Normal) //do stream overlap checks here
            {
                Console.Write("Searching for stream switch overlaps");

                bool first = true;
                foreach (int switchTime in Player.Beatmap.StreamSwitchPoints)
                {
                    if (!first)
                    {
                        checkOverlaps(Difficulty.Easy, Difficulty.Normal, switchTime);
                        checkOverlaps(Difficulty.Hard, Difficulty.Normal, switchTime);
                    }

                    checkOverlaps(Difficulty.Normal, Difficulty.Easy, switchTime);
                    checkOverlaps(Difficulty.Hard, Difficulty.Normal, switchTime);

                    first = false;
                }

                Console.WriteLine();
            }
        }

        private static void checkOverlaps(Difficulty s1, Difficulty s2 = Difficulty.None, int switchTime = 0)
        {
            bool streamSwitch = s2 != Difficulty.None;
            PlayTest.AllowStreamSwitch = streamSwitch;

            PlayTest.StartTime = switchTime > 0 ? switchTime - 10000 : 0;
            int endTime = switchTime > 0 ? switchTime + 10000 : 0;
            PlayTest.InitialDifficulty = s1;

            Player.Autoplay = true;

            if (s2 > s1)
                PlayTest.InitialHp = 200;
            else
                PlayTest.InitialHp = 0;

            using (PlayTest p = new PlayTest())
            {
                p.Initialize();

                FakeAudioTimeSource source = new FakeAudioTimeSource();
                source.InternalTime = Math.Max((PlayTest.StartTime - 10000) / 1000f, 0);
                Clock.AudioTimeSource = source;

                HitObjectManager hitObjectManager = p.HitObjectManager;

                List<HitObjectPair> pairs = new List<HitObjectPair>();

                while ((switchTime == 0 && p.Progress < 1) || Clock.AudioTime < switchTime + 10000)
                {
                    if (source.InternalTime % 20 < 0.01)
                        Console.Write(".");

                    Clock.UpdateCustom(0.01);
                    source.InternalTime += 0.01;

                    List<HitObject> objects = hitObjectManager.ActiveStreamObjects.FindAll(h => h.IsVisible);
                    foreach (HitObject h1 in objects)
                        foreach (HitObject h2 in objects)
                        {
                            if (h1 == h2) continue;

                            HitObjectPair hop = new HitObjectPair(h1, h2);

                            if (pairs.IndexOf(hop) >= 0) continue;

                            //hack in the current snaking point for added security.
                            Vector2 h1pos2 = h1 is Slider ? ((Slider)h1).SnakingEndPosition : h1.Position2;
                            Vector2 h2pos2 = h2 is Slider ? ((Slider)h2).SnakingEndPosition : h2.Position2;

                            if (pMathHelper.Distance(h1.Position, h2.Position) < DifficultyManager.HitObjectRadiusSprite ||
                                pMathHelper.Distance(h1.Position, h2pos2) < DifficultyManager.HitObjectRadiusSprite ||
                                pMathHelper.Distance(h1pos2, h2.Position) < DifficultyManager.HitObjectRadiusSprite ||
                                pMathHelper.Distance(h1pos2, h2pos2) < DifficultyManager.HitObjectRadiusSprite)
                            {
                                pairs.Add(hop);
                                if (s2 != Difficulty.None)
                                    Console.WriteLine("[mod] [{1}->{2}] Overlap at {0}", Clock.AudioTime, s1.ToString()[0], s2.ToString()[0]);
                                else
                                    Console.WriteLine("[mod] [{1}] Overlap at {0}", Clock.AudioTime, s1, s2);
                            }
                        }
                }
            }
        }

        internal class HitObjectPair : IEquatable<HitObjectPair>
        {
            internal HitObject h1;
            internal HitObject h2;

            public HitObjectPair(HitObject h1, HitObject h2)
            {
                this.h1 = h1;
                this.h2 = h2;
            }

            #region IEquatable<HitObjectPair> Members

            public bool Equals(HitObjectPair other)
            {
                return (h1 == other.h1 || h1 == other.h2) && (h2 == other.h1 || h2 == other.h2);
            }

            #endregion
        }
    }
}
