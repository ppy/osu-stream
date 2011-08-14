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

namespace BeatmapCombinator
{
    class BeatmapDifficulty : Beatmap
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

    class HitObjectLine
    {
        internal string StringRepresentation;
        internal int Time;
    }

    class BeatmapCombinator
    {
        internal static readonly NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
        private static List<string> headerContent;
        private static double healthMultiplier;

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
                    ProcessBeatmap(args[0]);
#if !DEBUG
                Console.ReadLine();
#endif

                    Console.WriteLine();
                    Console.WriteLine("All done :)");
                    Thread.Sleep(2000);
                    return;
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
        }

        private static void ProcessBeatmap(string dir)
        {
            Console.WriteLine("Combinating beatmap: " + dir.Split('\\').Last(s => s == s));
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            
            if (dir.Length < 1)
            {
                Console.WriteLine("No path specified!");
                return;
            }

            List<string> osuFiles = new List<string>(Directory.GetFiles(dir, "*.osu"));

            if (osuFiles.Count < 1)
            {
                Console.WriteLine("No .osu files found!");
                return;
            }

            string baseName = osuFiles[0].Remove(osuFiles[0].LastIndexOf('[') - 1);

            string oscFilename = baseName + ".osc";

            List<string> orderedDifficulties = new List<string>();

            orderedDifficulties.Add(osuFiles.Find(f => f.EndsWith("[Easy].osu")));
            orderedDifficulties.Add(osuFiles.Find(f => f.EndsWith("[Normal].osu")));
            orderedDifficulties.Add(osuFiles.Find(f => f.EndsWith("[Hard].osu")));
            orderedDifficulties.Add(osuFiles.Find(f => f.EndsWith("[Expert].osu")));

            if (orderedDifficulties.FindAll(t => t != null).Count < 1) return;

            Console.WriteLine("Files found:");
            foreach (string s in orderedDifficulties)
                Console.WriteLine("    * " + Path.GetFileName(s));
            Console.WriteLine();
            Console.WriteLine();

            List<BeatmapDifficulty> difficulties = new List<BeatmapDifficulty>();

            string Artist = string.Empty, Creator = string.Empty, Source = string.Empty, Title = string.Empty;

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
                                    int time = (int)Decimal.Parse(split[2]);
                                    int endTime = spinner ? endTime = (int)Decimal.Parse(split[5]) : time;

                                    int repeatCount = 0; double length = 0; bool hadEndpointSamples = false;
                                    bool hold = false;
                                    if (slider)
                                    {
                                        repeatCount = Convert.ToInt32(split[6]);
                                        length = Convert.ToDouble(split[7]);
                                        hadEndpointSamples = split.Length >= 9;

                                        hold = (repeatCount > 1 && length < 50) ||
                                               (repeatCount > 4 && length < 100) ||
                                               (hadEndpointSamples && split[4] == "4");
                                    }

                                    // take the slider's slide sampleset from 20ms after the head in case the head has a different sampleset
                                    ControlPoint cp = bd.controlPointAt(slider ? time + 20 : endTime + 5);

                                    StringBuilder builder = new StringBuilder();
                                    builder.Append(MakeSampleset(cp));

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

                                        // nodal samplesets
                                        builder.Append(',');
                                        builder.Append(MakeSampleset(cp));

                                        for (int repeatNo = 0; repeatNo < repeatCount; repeatNo++)
                                        {
                                            currTime += ReboundTime;
                                            cp = bd.controlPointAt(currTime + 5);
                                            builder.Append(':');
                                            builder.Append(MakeSampleset(cp));
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
                                                                 split.Length > 7 ? split[7][0] == '1' : false,
                                                                 split.Length > 8 ? (SampleSet)Int32.Parse(split[8]) : (SampleSet)Int32.Parse(split[3]));
                                    bd.ControlPoints.Add(cp);
                                    break;
                                }
                        }
                    }

                    bd.HeaderLines.Add(writeLine);
                }
            }

            foreach (BeatmapDifficulty d in difficulties)
                if (d != null) d.HitObjectLines.Sort(delegate(HitObjectLine h1, HitObjectLine h2) { return h1.Time.CompareTo(h2.Time); });

            headerContent = difficulties.Find(d => d != null).HeaderLines;

            string[] splitdir = dir.Split('\\');
            string upOneDir = string.Join("\\", splitdir, 0, splitdir.Length - 1);

#if DIST
            string osz2Filename = upOneDir + "\\" + baseName.Substring(baseName.LastIndexOf("\\") + 1) + ".osf2";
#elif M4A
            string osz2Filename = upOneDir + "\\" + baseName.Substring(baseName.LastIndexOf("\\") + 1) + ".osz2";
#else
            string osz2Filename = upOneDir + "\\" + baseName.Substring(baseName.LastIndexOf("\\") + 1) + ".osz2";
#endif

#if M4A
            string audioFilename = Directory.GetFiles(dir, "*.m4a")[0];
#else
            string audioFilename = Directory.GetFiles(dir, "*.mp3")[0];
#endif

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

            FakeAudioTimeSource source = new FakeAudioTimeSource();
            Clock.AudioTimeSource = source;

            headerContent.Remove("[HitObjects]");

            headerContent.Add(string.Empty);
            headerContent.Add("[ScoringMultipliers]");

            if (orderedDifficulties[(int)Difficulty.Easy] != null)
                headerContent.Add("0: " + calculateMultiplier(Difficulty.Easy).ToString("G17", nfi));
            if (orderedDifficulties[(int)Difficulty.Normal] != null)
                headerContent.Add("1: " + calculateMultiplier(Difficulty.Normal).ToString("G17", nfi));
            if (orderedDifficulties[(int)Difficulty.Expert] != null)
                headerContent.Add("3: " + calculateMultiplier(Difficulty.Expert).ToString("G17", nfi));

            if (healthMultiplier != 0)
                headerContent.Add("HP:" + healthMultiplier);

            headerContent.Add(string.Empty);
            headerContent.Add("[HitObjects]");

            Player.Beatmap.Dispose();

            //write the package a second time with new multiplier header data.
            writePackage(oscFilename, osz2Filename, audioFilename, difficulties, orderedDifficulties);
        }

        private static void writePackage(string oscFilename, string osz2Filename, string audioFilename, List<BeatmapDifficulty> difficulties, List<string> ordered)
        {
            using (StreamWriter output = new StreamWriter(oscFilename))
            {
                //write headers first (use first difficulty as arbitrary source)
                foreach (string l in headerContent)
                    output.WriteLine(l);

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

                        if (line.Time >= currentTime && (bestMatchLine == null || line.Time < bestMatchLine.Time))
                        {
                            bestMatchDifficulty = i;
                            bestMatchLine = line;
                        }
                    }

                    output.WriteLine(bestMatchDifficulty + "," + bestMatchLine.StringRepresentation);

                    linesRemaining[bestMatchDifficulty]--;
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

                package.AddFile(Path.GetFileName(oscFilename), oscFilename, DateTime.MinValue, DateTime.MinValue);
#if M4A
                package.AddFile("audio.m4a", audioFilename, DateTime.MinValue, DateTime.MinValue);
#else
                package.AddFile("audio.mp3", audioFilename, DateTime.MinValue, DateTime.MinValue);
#endif

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
                            key = line.Substring(0,line.IndexOf(':'));
                            val = line.Substring(line.IndexOf(':')+1).Trim();

                            MapMetaType t = (MapMetaType)Enum.Parse(typeof(MapMetaType), key, true);
                            package.AddMetadata(t, val);
                        }
                    }
                }

                string thumb = dir + "\\thumb-128.jpg";
                if (File.Exists(thumb))
                    package.AddFile("thumb-128.jpg", thumb, DateTime.MinValue, DateTime.MinValue);
                thumb = Path.GetDirectoryName(audioFilename) + "\\thumb-256.jpg";
                if (File.Exists(thumb))
                    package.AddFile("thumb-256.jpg", thumb, DateTime.MinValue, DateTime.MinValue);

                package.Save();
            }
        }

        private static string MakeSampleset(ControlPoint cp)
        {
            string result = ((int)cp.sampleSet).ToString();
            bool b1 = cp.volume != 100;
            bool b2 = cp.normalSampleSet != SampleSet.None && cp.normalSampleSet != cp.sampleSet;

            if (b1 || b2)
            {
                result += "|" + cp.volume.ToString();
                if (b2)
                {
                    result += "|" + ((int)cp.normalSampleSet).ToString();
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

        private static double calculateMultiplier(Difficulty difficulty)
        {
            Console.Write("Processing " + difficulty);

            double comboMultiplier = 1;

            Player.Difficulty = difficulty;
            Player.Autoplay = true;

            using (Player p = new Player())
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
                    int index = p.HitObjectManager.ActiveStreamObjects.FindIndex(h => { return h.StartTime > testStreamSwitch; });
                    if (index == 0)
                        throw new Exception("Bookmark exists before first object! Please only use bookmarks for stream switch points.");
                    switchHpObject = p.HitObjectManager.ActiveStreamObjects[index - 1];
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
                        double currentHp = p.healthBar.CurrentHp;
                        //Console.WriteLine("HP at required stream switch point (" + switchHpObject.EndTime + ") is " + currentHp);

                        if (currentHp < HealthBar.HP_BAR_MAXIMUM)
                        {
                            healthMultiplier = (HealthBar.HP_BAR_MAXIMUM - HealthBar.HP_BAR_INITIAL) / (currentHp - HealthBar.HP_BAR_INITIAL);
                            //Console.WriteLine("Need a multiplier of " + healthMultiplier);
                        }
                        switchHpObject = null;
                    }

                    if (p.Completed)
                    {
                        Score s = p.CurrentScore;

                        int excess = s.totalScore - s.spinnerBonusScore - 1000000;

                        double testMultiplier = (double)(s.comboBonusScore - excess) / s.comboBonusScore;

                        comboMultiplier = testMultiplier;
                        break;
                    }
                }
            }

            int finalScore = 0;
            double adjustment = 0;

            while (finalScore < 1000000)
            {
                Console.Write(".");

                Player.Difficulty = difficulty;
                Player.Autoplay = true;

                Player.Beatmap.DifficultyInfo[difficulty] = new BeatmapDifficultyInfo(difficulty) { ComboMultiplier = comboMultiplier };

                //let's do some test runs
                using (Player p = new Player())
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
                            Score s = p.CurrentScore;

                            finalScore = (s.totalScore - s.spinnerBonusScore);

                            if (finalScore < 1000000)
                            {
                                int fellShortBy = 1000000 - finalScore;
                                adjustment = (double)fellShortBy / s.comboBonusScore;
                                comboMultiplier += adjustment;
                            }
                            else
                            {
                                Console.WriteLine(" Done");
                                Console.WriteLine();
                                Console.WriteLine("HP multiplier: ".PadRight(25) + healthMultiplier);
                                Console.WriteLine("Using combo multiplier: ".PadRight(25) + comboMultiplier);
                                Console.WriteLine("Hitobject score: ".PadRight(25) + s.hitScore);
                                Console.WriteLine("Combo score: ".PadRight(25) + s.comboBonusScore);
                                Console.WriteLine("Spin score: ".PadRight(25) + s.spinnerBonusScore);
                                Console.WriteLine("Accuracy score: ".PadRight(25) + s.accuracyBonusScore);
                                Console.WriteLine("Total score: ".PadRight(25) + s.totalScore);
                                Console.WriteLine("Total score (no spin): ".PadRight(25) + finalScore);
                                Console.WriteLine();
                                Console.WriteLine();
                            }

                            break;
                        }
                    }
                }
            }

            //i guess the best thing to do might be to aim slightly above 1m and ignore the excess...
            //okay now we have numbers roughly around 1mil (always higher or equal to).
            //need to do something about this static, then load them up in osu!s.
            return comboMultiplier;
        }
    }
}
