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

namespace osum.GameplayElements
{
    /// <summary>
    /// Class that handles loading of content from a Beatmap, and general handling of anything that involves hitObjects as a group.
    /// </summary>
    internal partial class HitObjectManager : IDrawable, IDisposable
    {
        public void LoadFile()
        {
            beatmap.ControlPoints.Clear();

            FileSection currentSection = FileSection.Unknown;

            //Check file just before load -- ensures no modifications have occurred.
            //BeatmapManager.Current.UpdateChecksum();

            List<string> readableFiles = new List<string>();

            readableFiles.Add(beatmap.BeatmapFilename);

            string storyBoardFile = beatmap.StoryboardFilename;

            //if (beatmap.CheckFileExists(storyBoardFile))
            //    readableFiles.Add(storyBoardFile);

            bool hasCustomColours = false;
            bool hitObjectPreInit = false;
            bool lastAddedSpinner = false;
            bool firstColour = true;

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

                        string[] split = line.Trim().Split(',');
                        string[] var = line.Trim().Split(':');
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
                            case FileSection.TimingPoints:
                                if (split.Length > 2)
                                    beatmap.ControlPoints.Add(
                                        new ControlPoint(Double.Parse(split[0].Trim(), GameBase.nfi),
                                                         Double.Parse(split[1].Trim(), GameBase.nfi),
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

                                }
                                break;
                            case FileSection.Editor:
                                //We only need to read this section if we are in the editor.
                                continue;
                            case FileSection.Colours:
                                /*if (!hasCustomColours)
                                {
                                    hasCustomColours = true;
                                    SkinManager.BeatmapColours["Combo1"] = Color.TransparentWhite;
                                    SkinManager.BeatmapColours["Combo2"] = Color.TransparentWhite;
                                    SkinManager.BeatmapColours["Combo3"] = Color.TransparentWhite;
                                    SkinManager.BeatmapColours["Combo4"] = Color.TransparentWhite;
                                    SkinManager.BeatmapColours["Combo5"] = Color.TransparentWhite;
                                }
                                string[] splitn = val.Split(',');
                                SkinManager.BeatmapColours[key] =
                                    new Color((byte)Convert.ToInt32(splitn[0]), (byte)Convert.ToInt32(splitn[1]),
                                              (byte)Convert.ToInt32(splitn[2]));*/
                                break;
                            case FileSection.Variables:
                                /*string[] varSplit = line.Split('=');
                                if (varSplit.Length != 2) continue;
                                Variables.Add(varSplit[0], varSplit[1]);*/
                                break;
                            case FileSection.Events:
                                //todo: implement this
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

                                // Mask out the first 4 bits for HitObjectType. This is redundant because they're masked out in the checks below, but still wise.
                                HitObjectType type = (HitObjectType)(Int32.Parse(split[3], GameBase.nfi) & 15);
                                HitObjectSoundType soundType = (HitObjectSoundType)Int32.Parse(split[4], GameBase.nfi);
                                int x = (int)Math.Max(0, Math.Min(512, Decimal.Parse(split[0], GameBase.nfi)));
                                int y = (int)Math.Max(0, Math.Min(512, Decimal.Parse(split[1], GameBase.nfi)));
                                Vector2 pos = new Vector2(x, y);
                                int time = (int)Decimal.Parse(split[2], GameBase.nfi);
                                //+ BeatmapManager.Current.VersionOffset;

                                int comboOffset = (Convert.ToInt32(split[3], GameBase.nfi) >> 4) & 7; // mask out bits 5-7 for combo offset.
                                bool newCombo = (type & HitObjectType.NewCombo) > 0 || lastAddedSpinner;

                                HitObject h = null;

                                //used for new combo forcing after a spinner.
                                lastAddedSpinner = h is Spinner;

                                if ((type & HitObjectType.Circle) > 0)
                                {
                                    h = hitFactory.CreateHitCircle(pos, time, newCombo, soundType, newCombo ? comboOffset : 0);
                                }
                                else if ((type & HitObjectType.Slider) > 0)
                                {
                                    CurveTypes curveType = CurveTypes.Catmull;
                                    int repeatCount = 0;
                                    double length = 0;
                                    List<Vector2> points = new List<Vector2>();
                                    List<HitObjectSoundType> sounds = null;

                                    string[] pointsplit = split[5].Split('|');
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
                                        Vector2 v = new Vector2((int)Convert.ToDouble(temp[0], GameBase.nfi),
                                                                (int)Convert.ToDouble(temp[1], GameBase.nfi));
                                        //if (i > 1 || v != points[0]) fixed with new constructor
                                        //old maps stored the start point of a slider in this list.
                                        //newer ones don't but we should check anyway.
                                        points.Add(v);
                                    }

                                    repeatCount = Convert.ToInt32(split[6], GameBase.nfi);

                                    if (split.Length > 7)
                                        length = Convert.ToDouble(split[7], GameBase.nfi);

                                    if (split.Length > 8)
                                    {
                                        //Per-endpoint Sample Additions
                                        string[] adds = split[8].Split('|');
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

                                    h = hitFactory.CreateSlider(pos, time, newCombo, soundType, curveType, repeatCount, length, points, sounds, newCombo ? comboOffset : 0);
                                }
                                else if ((type & HitObjectType.Spinner) > 0)
                                {
                                    h = hitFactory.CreateSpinner(time, Convert.ToInt32(split[5], GameBase.nfi), soundType);
                                }

                                //Make sure we have a valid  hitObject and actually add it to this manager.
                                if (h != null)
                                    Add(h);
                                break;
                            case FileSection.Unknown:
                                continue; //todo: readd this?  not sure if we need it anymore.
                        }
                    }
                }
            }

            PostProcessing();
        }

        protected virtual void PostProcessing()
        {
            float StackOffset = DifficultyManager.HitObjectRadius / 10;
            
            pTexture[] fptextures = SkinManager.LoadAll("followpoint");

            Vector2 stackVector = new Vector2(StackOffset, StackOffset);

            hitObjectsCount = hitObjects.Count;

            const int STACK_LENIENCE = 3;

            //Reverse pass for stack calculation.
            for (int i = hitObjectsCount - 1; i > 0; i--)
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

                HitObject objectI = hitObjects[i];

                if (objectI.StackCount != 0 || objectI is Spinner) continue;

                /* If this object is a hitcircle, then we enter this "special" case.
                 * It either ends with a stack of hitcircles only, or a stack of hitcircles that are underneath a slider.
                 * Any other case is handled by the "is Slider" code below this.
                 */
                if (objectI is HitCircle)
                {
                    while (--n >= 0)
                    {
                        HitObject objectN = hitObjects[n];

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
                                if (pMathHelper.Distance(spanN.EndPosition, hitObjects[j].Position) < STACK_LENIENCE)
                                    hitObjects[j].StackCount -= offset;
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
                        HitObject objectN = hitObjects[n];

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

            for (int i = 0; i < hitObjectsCount; i++)
            {
                HitObject currHitObject = hitObjects[i];

                if (currHitObject.StackCount != 0)
                    currHitObject.Position = currHitObject.Position - currHitObject.StackCount * stackVector;

                //Draw connection lines
                if (i > 0 && (currHitObject.Type & HitObjectType.NewCombo) == 0 &&
                    (hitObjects[i - 1].Type & HitObjectType.Spinner) == 0)
                {
                    Vector2 pos1 = hitObjects[i - 1].EndPosition;
                    int time1 = hitObjects[i - 1].EndTime;
                    Vector2 pos2 = currHitObject.Position;
                    int time2 = currHitObject.StartTime;

                    int distance = (int)pMathHelper.Distance(pos1, pos2);
                    Vector2 distanceVector = pos2 - pos1;
                    int length = time2 - time1;

                    for (int j = (int)(DifficultyManager.FollowLineDistance * 1.5);
                         j < distance - DifficultyManager.FollowLineDistance;
                         j += DifficultyManager.FollowLineDistance)
                    {
                        float fraction = ((float)j / distance);
                        Vector2 pos = pos1 + fraction * distanceVector;
                        int fadein = (int)(time1 + fraction * length) - DifficultyManager.FollowLinePreEmpt;
                        int fadeout = (int)(time1 + fraction * length);

                        pAnimation dot =
                            new pAnimation(fptextures,
                                           FieldTypes.Gamefield512x384, OriginTypes.Centre, ClockTypes.Audio, pos,
                                           0, false, Color4.White);
                        dot.SetFramerateFromSkin();

                        dot.Transformations.Add(
                            new Transformation(TransformationType.Fade, 0, 1, fadein, fadein + DifficultyManager.FadeIn));
                        dot.Transformations.Add(
                            new Transformation(TransformationType.Fade, 1, 0, fadeout, fadeout + DifficultyManager.FadeIn));
                        spriteManager.Add(dot);
                    }
                }
            }
        }
    }
}
