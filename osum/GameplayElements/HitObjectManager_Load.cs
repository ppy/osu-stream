using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Sprites;
using System.IO;
using OpenTK;
using osum.GameplayElements.HitObjects.Osu;
using osum.GameplayElements.Beatmaps;
using osum.Graphics;
using osum.Helpers;
using osum.Graphics.Skins;
using osum.GameplayElements.HitObjects;
using OpenTK.Graphics;
using osum.GameplayElements.Events;
using osum.GameModes;

namespace osum.GameplayElements
{
    /// <summary>
    /// Class that handles loading of content from a Beatmap, and general handling of anything that involves hitObjects as a group.
    /// </summary>
    public partial class HitObjectManager : IDrawable, IDisposable
    {
        public void LoadFile()
        {
            spriteManager.ForwardPlayOptimisedAdd = true;

            beatmap.ControlPoints.Clear();

            FileSection currentSection = FileSection.Unknown;

            //Check file just before load -- ensures no modifications have occurred.
            //BeatmapManager.Current.UpdateChecksum();

            List<string> readableFiles = new List<string>();

            readableFiles.Add(beatmap.BeatmapFilename);

            string storyBoardFile = beatmap.StoryboardFilename;

            //if (beatmap.CheckFileExists(storyBoardFile))
            //    readableFiles.Add(storyBoardFile);

            //bool hasCustomColours = false;
            //bool firstColour = true;
            bool hitObjectPreInit = false;
            bool lastAddedSpinner = false;

            //bool verticalFlip = (GameBase.Mode == OsuModes.Play && Player.currentScore != null &&
            //                     ModManager.CheckActive(Player.currentScore.enabledMods, Mods.HardRock));

            int linenumber;

            //Variables = new Dictionary<string, string>();

            //if (osqEngine == null) osqEngine = new osq.Encoder();

            //The first file will be the actual .osu file.
            //The second file is the .osb for now.
            for (int fn = 0; fn < readableFiles.Count; fn++)
            {
                if (fn > 0)
                {
                    break;
                    //don't handle storyboarding yet.

                    //baseReader = new StreamReader(BeatmapManager.Current.GetFileStream(readableFiles[fn]));
                    //LocatedTextReaderWrapper ltr = new LocatedTextReaderWrapper(baseReader);
                    //osqEngine = new osq.Encoder(ltr);
                }

                //using (TextReader reader = (fn == 0 ? (TextReader)new StreamReader(BeatmapManager.Current.GetFileStream(readableFiles[fn])) : new StringReader(osqEngine.Encode())))
                using (TextReader reader = (TextReader)new StreamReader(beatmap.GetFileStream(readableFiles[fn])))
                {
                    linenumber = 0;

                    string line = null;

                    bool readNew = true;
                    int objnumber = 0;

                    while (true)
                    {
                        if (readNew)
                        {
                            line = reader.ReadLine();

                            if (line == null) break;

                            linenumber++;
                        }

                        readNew = true;

                        if (line.Length == 0 || line.StartsWith(" ") || line.StartsWith("_") || line.StartsWith("//"))
                            continue;

                        //if (currentSection == FileSection.Events) ParseVariables(ref line);

                        string[] split = line.Split(',');
                        string[] var = line.Split(':');
                        string key = string.Empty;
                        string val = string.Empty;
                        if (var.Length > 1)
                        {
                            key = var[0].Trim();
                            val = var[1].Trim();
                        }

                        if (line[0] == '[')
                        {
                            try
                            {
                                currentSection =
                                    (FileSection)Enum.Parse(typeof(FileSection), line.Trim(new[] { '[', ']' }));
                            }
                            catch (Exception)
                            {
                            }
                            continue;
                        }

                        switch (currentSection)
                        {
                            case FileSection.ScoringMultipliers:
                                Difficulty diff = (Difficulty)Int32.Parse(key);
                                beatmap.DifficultyInfo[diff] = new BeatmapDifficultyInfo(diff) { ComboMultiplier = double.Parse(val, GameBase.nfi) };
                                break;
                            case FileSection.TimingPoints:
                                if (split.Length > 2)
                                    beatmap.ControlPoints.Add(
                                        new ControlPoint(Double.Parse(split[0], GameBase.nfi),
                                                         Double.Parse(split[1], GameBase.nfi),
                                                         split[2][0] == '0' ? TimeSignatures.SimpleQuadruple :
                                                         (TimeSignatures)Int32.Parse(split[2]),
                                                         (SampleSet)Int32.Parse(split[3]),
                                                         split.Length > 4
                                                             ? (CustomSampleSet)Int32.Parse(split[4])
                                                             : CustomSampleSet.Default,
                                                         Int32.Parse(split[5]),
                                                         split.Length > 6 ? split[6][0] == '1' : true,
                                                         split.Length > 7 ? split[7][0] == '1' : false));
                                break;
                            case FileSection.General:
                                //todo: reimplement?
                                switch (key)
                                {
                                    case "AudioFilename":
                                        if (val.Length > 0)
                                            beatmap.AudioFilename = val;
                                        break;
                                }
                                break;
                            case FileSection.Editor:
                                switch (key)
                                {
                                    case "Bookmarks":
                                        if (val.Length > 0)
                                        {
                                            beatmap.StreamSwitchPoints = new List<int>();
                                            string[] points = val.Split(',');
                                            foreach (string point in points)
                                                beatmap.StreamSwitchPoints.Add(Int32.Parse(point.Trim()));
                                        }
                                        break;
                                }
                                //not relevant
                                continue;
                            case FileSection.Colours:
                                //not implemented yet.
                                break;
                            case FileSection.Variables:
                                //not implemented yet.
                                break;
                            case FileSection.Events:
                                //not implemented yet.
                                break;
                            case FileSection.Difficulty:
                                switch (key)
                                {
                                    case "HPDrainRate":
                                        beatmap.DifficultyHpDrainRate = Math.Min((byte)10, Math.Max((byte)0, byte.Parse(val)));
                                        break;
                                    case "CircleSize":
                                        beatmap.DifficultyCircleSize = Math.Min((byte)10, Math.Max((byte)0, byte.Parse(val)));
                                        break;
                                    case "OverallDifficulty":
                                        beatmap.DifficultyOverall = Math.Min((byte)10, Math.Max((byte)0, byte.Parse(val)));
                                        //if (!hasApproachRate) DifficultyApproachRate = DifficultyOverall;
                                        break;
                                    case "SliderMultiplier":
                                        beatmap.DifficultySliderMultiplier =
                                            Math.Max(0.4, Math.Min(3.6, Double.Parse(val, GameBase.nfi)));
                                        break;
                                    case "SliderTickRate":
                                        beatmap.DifficultySliderTickRate =
                                            Math.Max(0.5, Math.Min(8, Double.Parse(val, GameBase.nfi)));
                                        break;
                                    /*case "ApproachRate":
                                        beatmap.DifficultyApproachRate = Math.Min((byte)10, Math.Max((byte)0, byte.Parse(val)));
                                        hasApproachRate = true;
                                        break;*/
                                }
                                break;
                            case FileSection.HitObjects:
                                if (fn > 0)
                                    continue;

                                if (!hitObjectPreInit)
                                {
                                    //ComboColoursReset();
                                    hitObjectPreInit = true;
                                }

                                int offset = 0;

                                Difficulty difficulty = (Difficulty)Int32.Parse(split[offset++]);

                                switch (Player.Difficulty)
                                {
                                    case Difficulty.Easy:
                                        if (difficulty != Difficulty.Easy)
                                            continue;
                                        break;
                                    case Difficulty.Normal:
                                        if (difficulty == Difficulty.Expert)
                                            continue;
                                        break;
                                    case Difficulty.Expert:
                                        if (difficulty != Difficulty.Expert)
                                            continue;
                                        break;
                                }

                                string sample = split[offset++];

                                //most optimal way. need to rewrite if there are ever more samplesets :p.
                                SampleSet sampleSet = sample[0] == '2' ? SampleSet.Soft : SampleSet.Normal;
                                float volume = 1;

                                if (sample.Length > 1)
                                    volume = Int32.Parse(sample.Substring(2)) / 100f;

                                int x = (int)Math.Max(0, Math.Min(512, Decimal.Parse(split[offset++], GameBase.nfi)));
                                int y = (int)Math.Max(0, Math.Min(512, Decimal.Parse(split[offset++], GameBase.nfi)));
                                int time = (int)Decimal.Parse(split[offset++], GameBase.nfi);
                                HitObjectType type = (HitObjectType)(Int32.Parse(split[offset], GameBase.nfi) & 15);
                                int comboOffset = (Convert.ToInt32(split[offset++], GameBase.nfi) >> 4) & 7; // mask out bits 5-7 for combo offset.
                                HitObjectSoundType soundType = (HitObjectSoundType)Int32.Parse(split[offset++], GameBase.nfi);

                                Vector2 pos = new Vector2(x, y);

                                bool newCombo = (type & HitObjectType.NewCombo) > 0 || lastAddedSpinner || objnumber == 0;

                                HitObject h = null;

                                //used for new combo forcing after a spinner.
                                lastAddedSpinner = h is Spinner;

                                if ((type & HitObjectType.Circle) > 0)
                                {
                                    h = hitFactory.CreateHitCircle(pos, time, newCombo, soundType, newCombo ? comboOffset : 0);
                                }
                                else if ((type & HitObjectType.Slider) > 0)
                                {
                                    CurveTypes curveType = CurveTypes.Bezier;
                                    int repeatCount = 0;
                                    double length = 0;
                                    List<Vector2> points = new List<Vector2>();
                                    List<HitObjectSoundType> sounds = null;

                                    string[] pointsplit = split[offset++].Split('|');
                                    for (int i = 0; i < pointsplit.Length; i++)
                                    {
                                        if (pointsplit[i].Length == 1)
                                        {
                                            switch (pointsplit[i])
                                            {
                                                case "C":
                                                    curveType = CurveTypes.Catmull;
                                                    break;
                                                case "B":
                                                    curveType = CurveTypes.Bezier;
                                                    break;
                                                case "L":
                                                    curveType = CurveTypes.Linear;
                                                    break;
                                            }
                                            continue;
                                        }

                                        string[] temp = pointsplit[i].Split(':');
                                        Vector2 v = new Vector2((float)Convert.ToDouble(temp[0], GameBase.nfi),
                                                                (float)Convert.ToDouble(temp[1], GameBase.nfi));
                                        points.Add(v);
                                    }

                                    repeatCount = Convert.ToInt32(split[offset++], GameBase.nfi);

                                    length = Convert.ToDouble(split[offset++], GameBase.nfi);

                                    //Per-endpoint Sample Additions
                                    if (split[offset].Length > 0)
                                    {
                                        string[] adds = split[offset++].Split('|');
                                        if (adds.Length > 0)
                                        {
                                            sounds = new List<HitObjectSoundType>();
                                            for (int i = 0; i < adds.Length; i++)
                                            {
                                                int sound;
                                                Int32.TryParse(adds[i], out sound);
                                                sounds.Add((HitObjectSoundType)sound);
                                            }
                                        }
                                    }
                                    else
                                        offset++;

                                    if ((repeatCount > 1 && length < 50) || (repeatCount > 4 && length < 100))
                                    {
                                        h = hitFactory.CreateHoldCircle(pos, time, newCombo, soundType, repeatCount, length, sounds, newCombo ? comboOffset : 0, Convert.ToDouble(split[offset++], GameBase.nfi), Convert.ToDouble(split[offset++], GameBase.nfi));
                                    }
                                    else
                                    {
                                        h = hitFactory.CreateSlider(pos, time, newCombo, soundType, curveType, repeatCount, length, points, sounds, newCombo ? comboOffset : 0, Convert.ToDouble(split[offset++], GameBase.nfi), Convert.ToDouble(split[offset++], GameBase.nfi));
                                    }
                                }
                                else if ((type & HitObjectType.Spinner) > 0)
                                {
                                    h = hitFactory.CreateSpinner(time, Convert.ToInt32(split[offset++], GameBase.nfi), soundType);
                                }

                                //Make sure we have a valid  hitObject and actually add it to this manager.
                                if (h != null)
                                {
                                    h.SampleSet = sampleSet;
                                    h.Volume = volume;
                                    Add(h, difficulty);
                                }
                                objnumber++;

                                break;
                            case FileSection.Unknown:
                                continue; //todo: readd this?  not sure if we need it anymore.
                        }
                    }
                }
            }

            PostProcessing();
        }

        internal virtual void PostProcessing()
        {
            int difficultyIndex = 0;
            foreach (List<HitObject> objects in StreamHitObjects)
            {
                if (objects == null)
                {
                    difficultyIndex++;
                    continue;
                }

                SpriteManager diffSpriteManager = streamSpriteManagers[difficultyIndex];

                FirstBeatLength = beatmap.beatLengthAt(0);

                float StackOffset = DifficultyManager.HitObjectRadiusGamefield / 10;

                pTexture fptexture = TextureManager.Load(OsuTexture.followpoint);

                Vector2 stackVector = new Vector2(StackOffset, StackOffset);

                const int STACK_LENIENCE = 3;

                //Reverse pass for stack calculation.
                for (int i = objects.Count - 1; i > 0; i--)
                {
                    int n = i;
                    /* We should check every note which has not yet got a stack.
                     * Consider the case we have two interwound stacks and this will make sense.
                     * 
                     * o <-1      o <-2
                     *  o <-3      o <-4
                     * 
                     * We first process starting from 4 and handle 2,
                     * then we come backwards on the i loop iteration until we reach 3 and handle 1.
                     * 2 and 1 will be ignored in the i loop because they already have a stack value.
                     */

                    HitObject objectI = objects[i];

                    if (objectI.StackCount != 0 || objectI is Spinner) continue;

                    /* If this object is a hitcircle, then we enter this "special" case.
                     * It either ends with a stack of hitcircles only, or a stack of hitcircles that are underneath a slider.
                     * Any other case is handled by the "is Slider" code below this.
                     */
                    if (objectI is HitCircle)
                    {
                        while (--n >= 0)
                        {
                            HitObject objectN = objects[n];

                            if (objectN is Spinner) continue;

                            HitObjectSpannable spanN = objectN as HitObjectSpannable;

                            if (objectI.StartTime - (DifficultyManager.PreEmpt * beatmap.StackLeniency) > objectN.EndTime)
                                //We are no longer within stacking range of the previous object.
                                break;

                            /* This is a special case where hticircles are moved DOWN and RIGHT (negative stacking) if they are under the *last* slider in a stacked pattern.
                             *    o==o <- slider is at original location
                             *        o <- hitCircle has stack of -1
                             *         o <- hitCircle has stack of -2
                             */
                            if (spanN != null && pMathHelper.Distance(spanN.EndPosition, objectI.Position) < STACK_LENIENCE)
                            {
                                int offset = objectI.StackCount - objectN.StackCount + 1;
                                for (int j = n + 1; j <= i; j++)
                                {
                                    //For each object which was declared under this slider, we will offset it to appear *below* the slider end (rather than above).
                                    if (pMathHelper.Distance(spanN.EndPosition, objects[j].Position) < STACK_LENIENCE)
                                        objects[j].StackCount -= offset;
                                }

                                //We have hit a slider.  We should restart calculation using this as the new base.
                                //Breaking here will mean that the slider still has StackCount of 0, so will be handled in the i-outer-loop.
                                break;
                            }

                            if (pMathHelper.Distance(objectN.Position, objectI.Position) < STACK_LENIENCE)
                            {
                                //Keep processing as if there are no sliders.  If we come across a slider, this gets cancelled out.
                                //NOTE: Sliders with start positions stacking are a special case that is also handled here.

                                objectN.StackCount = objectI.StackCount + 1;
                                objectI = objectN;
                            }
                        }
                    }
                    else if (objectI is Slider)
                    {
                        /* We have hit the first slider in a possible stack.
                         * From this point on, we ALWAYS stack positive regardless.
                         */
                        while (--n >= 0)
                        {
                            HitObject objectN = objects[n];

                            if (objectN is Spinner) continue;

                            HitObjectSpannable spanN = objectN as HitObjectSpannable;

                            if (objectI.StartTime - (DifficultyManager.PreEmpt * beatmap.StackLeniency) > objectN.StartTime)
                                //We are no longer within stacking range of the previous object.
                                break;

                            if (pMathHelper.Distance((spanN != null ? spanN.EndPosition : objectN.Position), objectI.Position) < STACK_LENIENCE)
                            {
                                objectN.StackCount = objectI.StackCount + 1;
                                objectI = objectN;
                            }
                        }
                    }

                }

                HitObject last = null;

                for (int i = 0; i < objects.Count; i++)
                {
                    HitObject currHitObject = objects[i];

                    if (currHitObject.StackCount != 0)
                        currHitObject.Position = currHitObject.Position - currHitObject.StackCount * stackVector;

                    if (last != null && last.StartTime == currHitObject.StartTime)
                        diffSpriteManager.Add(Connect(last, currHitObject));

                    //Draw follow lines
                    if (last != null && !currHitObject.NewCombo && !(last is Spinner))
                    {
                        Vector2 pos1 = last.EndPosition;
                        int time1 = last.EndTime;
                        Vector2 pos2 = currHitObject.Position;
                        int time2 = currHitObject.StartTime;

                        int distance = (int)pMathHelper.Distance(pos1, pos2);
                        Vector2 distanceVector = pos2 - pos1;
                        int length = time2 - time1;

                        int buffer_size = (int)(DifficultyManager.FollowLineDistance * 1.5);

                        if (distance >= DifficultyManager.FollowLineDistance * 4 && last.connectedObject != currHitObject)
                        {
                            //find out how many points we can place (evenly)
                            int count = (int)Math.Round((double)(distance - buffer_size * 2) / DifficultyManager.FollowLineDistance);

                            float usableDistance = (distance - buffer_size * 2) / (count);

                            for (int j = 0; j < count + 1; j++)
                            {
                                float fraction = (buffer_size + usableDistance * j) / distance;
                                Vector2 pos = pos1 + fraction * distanceVector;
                                int fadein = (int)(time1 + fraction * length) - DifficultyManager.FollowLinePreEmpt;
                                int fadeout = (int)(time1 + fraction * length);

                                pSprite dot =
                                    new pSprite(fptexture,
                                                   FieldTypes.GamefieldSprites, OriginTypes.Centre, ClockTypes.Audio, pos,
                                                   0.01f, false, Color4.White);

                                dot.Transform(
                                    new Transformation(TransformationType.Fade, 0, 1, fadein, fadein + DifficultyManager.FadeIn));
                                dot.Transform(
                                    new Transformation(TransformationType.Scale, 0.5f, 1, fadein, fadein + DifficultyManager.FadeIn));
                                dot.Transform(
                                    new Transformation(TransformationType.Fade, 1, 0, fadeout, fadeout + DifficultyManager.FadeIn));
                                diffSpriteManager.Add(dot);
                                currHitObject.Sprites.Add(dot);
                            }
                        }
                    }

                    last = currHitObject;
                }

                diffSpriteManager.ForwardPlayOptimisedAdd = false;

                difficultyIndex++;
            }

            spriteManager.ForwardPlayOptimisedAdd = false;
        }
    }
}
