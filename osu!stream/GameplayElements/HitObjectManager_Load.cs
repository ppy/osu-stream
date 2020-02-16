using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using osum.GameModes.Play;
using osum.GameplayElements.Beatmaps;
using osum.GameplayElements.HitObjects;
using osum.GameplayElements.HitObjects.Osu;
using osum.Graphics;
using osum.Graphics.Sprites;
using osum.Helpers;
using osum.Support;

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
                TextReader reader = new StreamReader(beatmap.GetFileStream(readableFiles[fn]));
                {
                    linenumber = 0;

                    string line = null;

                    bool readNew = true;
                    int objnumber = 0;

                    bool headerReadFinished = false;

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

                        string key = string.Empty;
                        string val = string.Empty;

                        if (!headerReadFinished)
                        {
                            string[] var = line.Split(':');

                            if (var.Length > 1)
                            {
                                key = var[0].Trim();
                                val = var[1].Trim();
                            }
                        }

                        if (line[0] == '[')
                        {
                            try
                            {
                                currentSection =
                                    (FileSection)Enum.Parse(typeof(FileSection), line.Trim('[', ']'));
                                if (currentSection == FileSection.HitObjects)
                                    headerReadFinished = true;
                            }
                            catch (Exception)
                            {
                            }

                            continue;
                        }

                        switch (currentSection)
                        {
                            case FileSection.ScoringMultipliers:
                                if (key == "HP")
                                {
                                    beatmap.HpStreamAdjustmentMultiplier = double.Parse(val, GameBase.nfi);
                                }
                                else
                                {
                                    Difficulty diff = (Difficulty)int.Parse(key);
                                    beatmap.DifficultyInfo[diff] = new BeatmapDifficultyInfo(diff) { ComboMultiplier = double.Parse(val, GameBase.nfi) };
                                }

                                break;
                            case FileSection.General:
                                switch (key)
                                {
                                    case "CountdownOffset":
                                        if (val.Length > 0)
                                        {
                                            beatmap.CountdownOffset = int.Parse(val);
                                        }

                                        break;
                                }

                                break;
                            case FileSection.TimingPoints:
                            {
                                string[] split = line.Split(',');

                                if (split.Length > 2)
                                    beatmap.ControlPoints.Add(
                                        new ControlPoint(double.Parse(split[0], GameBase.nfi),
                                            double.Parse(split[1], GameBase.nfi),
                                            split[2][0] == '0' ? TimeSignatures.SimpleQuadruple : (TimeSignatures)int.Parse(split[2]),
                                            (SampleSet)int.Parse(split[3]),
                                            split.Length > 4
                                                ? (CustomSampleSet)int.Parse(split[4])
                                                : CustomSampleSet.Default,
                                            int.Parse(split[5]),
                                            split.Length > 6 ? split[6][0] == '1' : true,
                                            split.Length > 7 ? split[7][0] == '1' : false));
                                break;
                            }
                            case FileSection.Editor:
                                switch (key)
                                {
                                    case "Bookmarks":
                                        if (val.Length > 0)
                                        {
                                            beatmap.StreamSwitchPoints = new List<int>();
                                            string[] points = val.Split(',');
                                            foreach (string point in points)
                                                beatmap.StreamSwitchPoints.Add(int.Parse(point.Trim()));
                                        }

                                        break;
                                }

                                //not relevant
                                continue;
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
                                            Math.Max(0.4, Math.Min(3.6, double.Parse(val, GameBase.nfi)));
                                        break;
                                    case "SliderTickRate":
                                        beatmap.DifficultySliderTickRate =
                                            Math.Max(0.5, Math.Min(8, double.Parse(val, GameBase.nfi)));
                                        break;
                                    /*case "ApproachRate":
                                        beatmap.DifficultyApproachRate = Math.Min((byte)10, Math.Max((byte)0, byte.Parse(val)));
                                        hasApproachRate = true;
                                        break;*/
                                }

                                break;
                            case FileSection.HitObjects:
                            {
                                if (fn > 0)
                                    continue;

                                if (!hitObjectPreInit)
                                {
                                    //ComboColoursReset();
                                    hitObjectPreInit = true;
                                }

                                string[] split = line.Split(',');

                                int offset = 0;

                                Difficulty difficulty = (Difficulty)int.Parse(split[offset++]);


                                SampleSetInfo ssi = parseSampleSet(split[offset++]);

                                int x = (int)Math.Max(0, Math.Min(512, decimal.Parse(split[offset++], GameBase.nfi)));
                                int y = (int)Math.Max(0, Math.Min(512, decimal.Parse(split[offset++], GameBase.nfi)));
                                int time = (int)decimal.Parse(split[offset++], GameBase.nfi);

                                if (objnumber == 0) CountdownTime = time;
                                else CountdownTime = Math.Min(CountdownTime, time);
                                objnumber++;

                                if (!shouldLoadDifficulty(difficulty))
                                    continue;

                                HitObjectType type = (HitObjectType)int.Parse(split[offset], GameBase.nfi) & ~HitObjectType.ColourHax;
                                int comboOffset = (Convert.ToInt32(split[offset++], GameBase.nfi) >> 4) & 7; // mask out bits 5-7 for combo offset.
                                HitObjectSoundType soundType = (HitObjectSoundType)int.Parse(split[offset++], GameBase.nfi);

                                Vector2 pos = new Vector2(x, y);

                                bool newCombo = (type & HitObjectType.NewCombo) > 0 || lastAddedSpinner || StreamHitObjects[(int)difficulty] == null || StreamHitObjects[(int)difficulty].Count == 0;

                                HitObject h = null;

                                //used for new combo forcing after a spinner.
                                lastAddedSpinner = h is Spinner;

                                if ((type & HitObjectType.Circle) > 0)
                                {
                                    h = hitFactory.CreateHitCircle(pos, time, newCombo, soundType, newCombo ? comboOffset : 0);
                                }
                                else if ((type & (HitObjectType.Slider | HitObjectType.Hold)) > 0)
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
                                                case "P":
                                                    curveType = CurveTypes.PerfectCurve;
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

                                    List<SampleSetInfo> listSampleSets = null;

                                    //Per-endpoint Sample Additions
                                    if (split[offset].Length > 0)
                                    {
                                        string[] adds = split[offset++].Split('|');

                                        if (adds.Length > 0)
                                        {
                                            sounds = new List<HitObjectSoundType>(adds.Length);
                                            for (int i = 0; i < adds.Length; i++)
                                            {
                                                int sound;
                                                int.TryParse(adds[i], out sound);
                                                sounds.Add((HitObjectSoundType)sound);
                                            }
                                        }
                                    }
                                    else
                                        offset += 1;

                                    if (split.Length > 13)
                                    {
                                        string[] samplesets = split[13].Split(':');
                                        listSampleSets = new List<SampleSetInfo>(samplesets.Length);
                                        for (int i = 0; i < samplesets.Length; i++)
                                        {
                                            SampleSetInfo node_ssi = parseSampleSet(samplesets[i]);
                                            listSampleSets.Add(node_ssi);
                                        }
                                    }

                                    if ((repeatCount > 1 && length < 50) ||
                                        (repeatCount > 4 && length < 100) ||
                                        (type & HitObjectType.Hold) > 0)
                                    {
                                        h = hitFactory.CreateHoldCircle(pos, time, newCombo, soundType, repeatCount, length, sounds, newCombo ? comboOffset : 0, Convert.ToDouble(split[offset++], GameBase.nfi), Convert.ToDouble(split[offset++], GameBase.nfi), listSampleSets);
                                    }
                                    else
                                    {
                                        h = hitFactory.CreateSlider(pos, time, newCombo, soundType, curveType, repeatCount, length, points, sounds, newCombo ? comboOffset : 0, Convert.ToDouble(split[offset++], GameBase.nfi), Convert.ToDouble(split[offset++], GameBase.nfi), listSampleSets);
                                    }
                                }
                                else if ((type & HitObjectType.Spinner) > 0)
                                {
                                    h = hitFactory.CreateSpinner(time, Convert.ToInt32(split[offset++], GameBase.nfi), soundType);
                                }

                                //Make sure we have a valid  hitObject and actually add it to this manager.
                                if (h != null)
                                {
                                    h.SampleSet = ssi;
                                    Add(h, difficulty);
                                }
                            }
                                break;
                            case FileSection.Unknown:
                                continue; //todo: readd this?  not sure if we need it anymore.
                        }
                    }
                }
            }

            PostProcessing();
        }

        protected virtual bool shouldLoadDifficulty(Difficulty difficulty)
        {
            switch (Player.Difficulty)
            {
                case Difficulty.Easy:
                    if (difficulty != Difficulty.Easy)
                        return false;
                    break;
                case Difficulty.Normal:
                    if (difficulty == Difficulty.Expert)
                        return false;
                    break;
                case Difficulty.Expert:
                    if (difficulty != Difficulty.Expert)
                        return false;
                    break;
            }

            return true;
        }

        internal SampleSetInfo parseSampleSet(string sample)
        {
            //most optimal way. need to rewrite if there are ever more samplesets :p.
            //like there are now. >_<
            string[] split = sample.Split('|');

            SampleSet sampleSet = (SampleSet)Convert.ToInt32(split[0]);
            SampleSet normalSampleSet = SampleSet.None;
            float volume = 1;

            if (split.Length > 1)
                volume = int.Parse(split[1]) / 100f;
            if (split.Length > 2 && split[2].Length > 0)
                normalSampleSet = (SampleSet)Convert.ToInt32(split[2]);

            if (normalSampleSet == SampleSet.None) normalSampleSet = sampleSet;

            return new SampleSetInfo { SampleSet = sampleSet, CustomSampleSet = CustomSampleSet.Default, Volume = volume, AdditionSampleSet = normalSampleSet };
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

                float StackOffset = 4.0f;

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

                HitObject last = null, secondlast = null;
                HitObject lastLeft = null, lastRight = null;

                int currentComboNumber = 0;
                int colourIndex = -1;

                for (int i = 0; i < objects.Count; i++)
                {
                    HitObject currHitObject = objects[i];

                    if (currHitObject.StackCount != 0)
                        //add the previously calculated stack offset here.
                        currHitObject.Position = currHitObject.Position - currHitObject.StackCount * stackVector;

                    // two starts connected
                    bool sameTimeAsLastAdded = last != null && Math.Abs(currHitObject.StartTime - last.StartTime) < 10;

                    // one start connected to a slider end
                    bool sameTimeAsLastAdded2 = !sameTimeAsLastAdded && (last is Slider && !(last is HoldCircle)) && Math.Abs(currHitObject.StartTime - last.EndTime) < 10;

                    if (currHitObject.NewCombo)
                    {
                        currentComboNumber = 0;
                        if (!sameTimeAsLastAdded) //don't change colour if this is a connceted note
                        {
                            colourIndex += currHitObject.ComboOffset;
                            if ((currHitObject.Type & HitObjectType.Spinner) == 0) colourIndex++;
                            colourIndex %= TextureManager.DefaultColours.Length;
                        }
                    }
                    else if (last == null) colourIndex = 0;
                    else if (((last.Type & HitObjectType.Spinner) != 0) &&
                             ((currHitObject.Type & HitObjectType.Spinner) == 0))
                    {
                        colourIndex++;
                        colourIndex %= TextureManager.DefaultColours.Length;
                    }

                    if (colourIndex < 0)
                        colourIndex = 0;

                    if (currHitObject.IncrementCombo)
                    {
                        if (!sameTimeAsLastAdded || currentComboNumber == 0) currentComboNumber++;
                    }

                    currHitObject.ComboNumber = currentComboNumber;
                    currHitObject.ColourIndex = colourIndex;

                    if (sameTimeAsLastAdded || sameTimeAsLastAdded2)
                    {
                        //connect multitouch beats
                        diffSpriteManager.Add(Connect(last, currHitObject, sameTimeAsLastAdded2));
                    }
                    else if (last != null && lastLeft == null)
                    {
                        // draw a followpoint line to adjacent object if the previous isn't a multi pair
                        // and the objects to be joined aren't multi wrt. each other
                        // the case where it's a pair down to a single is handled later
                        // single up to a pair is handled here
                        FollowConnect(last, currHitObject, diffSpriteManager, fptexture);
                    }

                    if (sameTimeAsLastAdded && lastLeft != null)
                    {
                        // add pairs of followpoints between pairs of objects on each side
                        float x1 = last.Position.X;
                        float x2 = currHitObject.Position.X;
                        HitObject currLeft, currRight;
                        if (x1 <= x2)
                        {
                            currLeft = last;
                            currRight = currHitObject;
                        }
                        else
                        {
                            currLeft = currHitObject;
                            currRight = last;
                        }

                        if (!currLeft.NewCombo && !currRight.NewCombo)
                        {
                            FollowConnect(lastLeft, currLeft, diffSpriteManager, fptexture);
                            FollowConnect(lastRight, currRight, diffSpriteManager, fptexture);
                        }
                    }

                    if (last != null)
                    {
                        if (Math.Abs(currHitObject.EndTime - last.EndTime) < 10)
                        {
                            // current is pair
                            float x1 = last.EndPosition.X;
                            float x2 = currHitObject.EndPosition.X;

                            if (x1 <= x2)
                            {
                                lastLeft = last;
                                lastRight = currHitObject;
                            }
                            else
                            {
                                lastLeft = currHitObject;
                                lastRight = last;
                            }
                        }
                        else if (lastLeft != null && (Math.Abs(last.EndTime - lastLeft.EndTime) >= 10 || Math.Abs(last.EndTime - lastRight.EndTime) >= 10))
                        {
                            // curr/last not a pair but pair still formed

                            // wipe last pair if the last object isn't a member of this pair
                            lastLeft = null;
                            lastRight = null;

                            if (secondlast != null)
                                FollowConnect(secondlast, last, diffSpriteManager, fptexture);

                            // connect this pass's objects here since the left/right pair is still remembered above
                            FollowConnect(last, currHitObject, diffSpriteManager, fptexture);
                        }
                    }

                    secondlast = last;
                    last = currHitObject;
                }

                diffSpriteManager.ForwardPlayOptimisedAdd = false;

                difficultyIndex++;
            }

            spriteManager.ForwardPlayOptimisedAdd = false;
        }

        private void FollowConnect(HitObject first, HitObject second, SpriteManager diffSpriteManager, pTexture fptexture)
        {
            if (!ShouldFollow(first, second)) return;

            int time1 = first.EndTime;
            int time2 = second.StartTime;

            // followpoint code
            int time3 = first.StartTime - DifficultyManager.PreEmpt;
            // only allow followpoints to start their trek once their starting slider has finished snaking
            if (first.Type == HitObjectType.Slider)
                time3 = Math.Max(time3, ((Slider)first).snakingEnd);

            float hitRadius = DifficultyManager.HitObjectRadiusSolid / 2;

            Vector2 pos1 = first.EndPosition;
            Vector2 pos2 = second.Position;

            float distance = pMathHelper.Distance(pos1, pos2);
            distance -= hitRadius * 2;

            Vector2 distanceVector = pos2 - pos1;
            Vector2 unitVector = distanceVector;
            unitVector.Normalize();
            unitVector *= (hitRadius - DifficultyManager.FollowLineDistance * 0.125f); // this lets the followpoints get closer to their circles, for a better looking effect.

            // these two now represent the very edges of the two circles to be joined.
            pos1 += unitVector;
            pos2 -= unitVector;

            // number of spaces between followpoints, including the spaces between
            // the first and last followpoints and their joining circles
            int count = (int)(distance / DifficultyManager.FollowLineDistance + 0.5f);

            if (count > 1)
            {
                float countf = count;

                // the first followpoint appears moments before the destination object appears.
                float expandStart = Math.Max(time3, time2 - DifficultyManager.PreEmpt - DifficultyManager.FollowLinePreEmptStart);
                // it should reach its target just in time for that circle to appear
                // the line's speed is limited so it may arrive a bit late if the slider it leaves from is still snaking.
                float expandEnd = Math.Max(time2 - DifficultyManager.PreEmpt, expandStart + DifficultyManager.FollowPointSpeedLimit);

                // begin contracting followpoints as soon as the first object is done
                float contractStart = Math.Max(time1, expandStart + DifficultyManager.FollowPointScreenTime);
                // try to contract at the same speed but always finish just as the later object needs to be hit
                float contractEnd = Math.Max(Math.Min(time2, time1 + DifficultyManager.FollowLinePreEmptEnd), expandEnd + DifficultyManager.FollowPointScreenTime);

                // exclude j=0 and j=count, since they represent the two circles
                for (int j = 1; j < count; j++)
                {
                    float progress = j / countf;
                    Vector2 pos = Vector2.Lerp(pos1, pos2, progress);

                    int fadein = (int)pMathHelper.Lerp(expandStart, expandEnd, progress);
                    int fadeout = (int)pMathHelper.Lerp(contractStart, contractEnd, progress);

                    pSprite dot =
                        new pSprite(fptexture,
                            FieldTypes.GamefieldSprites, OriginTypes.Centre, ClockTypes.Audio, pos,
                            0.005f, false, Color4.White);

                    dot.Transform(
                        new TransformationF(TransformationType.Fade, 0, 1, fadein, fadein + DifficultyManager.FadeIn));
                    dot.Transform(
                        new TransformationBounce(fadein, fadein + DifficultyManager.FadeIn, 1, 2f, 2));
                    dot.Transform(
                        new TransformationF(TransformationType.Fade, 1, 0, fadeout, fadeout + DifficultyManager.FadeOut));
                    diffSpriteManager.Add(dot);
                    second.Sprites.Add(dot);
                }
            }
        }

        internal static bool ShouldFollow(HitObject first, HitObject second)
        {
            return (first.EndTime < second.StartTime) && !(first is Spinner) && !(second is Spinner) && !(first.connectedObject == second) && !second.NewCombo;
        }
    }
}