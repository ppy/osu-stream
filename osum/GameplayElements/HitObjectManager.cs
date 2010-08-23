#region Using Statements

using System;
using System.Collections.Generic;
using osum.GameplayElements.Beatmaps;
using System.IO;
using OpenTK;
using osu.GameplayElements.HitObjects;
using osu.GameplayElements.HitObjects.Osu;
using osum.Graphics.Skins;
using osum.Graphics.Sprites;

#endregion

namespace osum.GameplayElements
{
    /// <summary>
    /// Class that handles loading of content from a Beatmap, and general handling of anything that involves hitObjects as a group.
    /// </summary>
    internal class HitObjectManager : IDrawable, IDisposable
    {
        /// <summary>
        /// The loaded beatmap.
        /// </summary>
        Beatmap beatmap;

        /// <summary>
        /// A factory to create necessary hitObjects.
        /// </summary>
        HitFactory hitFactory;

        /// <summary>
        /// The complete list of hitObjects.
        /// </summary>
        internal List<HitObject> hitObjects = new List<HitObject>();

        /// <summary>
        /// Internal spriteManager for drawing all hitObject related content.
        /// </summary>
        internal SpriteManager spriteManager = new SpriteManager();

        public HitObjectManager(Beatmap beatmap)
        {
            this.beatmap = beatmap;
            hitFactory = new HitFactoryOsu(this);
        }

        public void Dispose()
        {
            spriteManager.Dispose();
        }

        /// <summary>
        /// Counter for assigning combo numbers to hitObjects during load-time.
        /// </summary>
        int currentComboNumber = 1;

        /// <summary>
        /// Index counter for assigning combo colours during load-time.
        /// </summary>
        int colourIndex = 0;

        /// <summary>
        /// Adds a new hitObject to be managed by this manager.
        /// </summary>
        /// <param name="h">The hitObject to manage.</param>
        void Add(HitObject h)
        {
            if (h.NewCombo)
            {
                currentComboNumber = 1;
                colourIndex = (colourIndex + 1) % SkinManager.DefaultColours.Length;
            }

            h.ComboNumber = currentComboNumber++;
            h.SetColour(SkinManager.DefaultColours[colourIndex]);

            hitObjects.Add(h);

            spriteManager.Add(h);
        }

        public void LoadFile()
        {
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
                StreamReader baseReader = null;

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
                            case FileSection.General:
                                //todo: reimplement?
                                /*switch (key)
                                {
                                    case "EditorBookmarks":
                                        string[] strlist = val.Split(',');
                                        foreach (string s in strlist)
                                            if (s.Length > 0)
                                            {
                                                int bm = Int32.Parse(s);
                                                if (!Bookmarks.Contains(bm))
                                                    Bookmarks.Add(bm);
                                            }
                                        break;
                                    case "EditorDistanceSpacing":
                                        ConfigManager.sDistanceSpacing = Convert.ToDouble(val, GameBase.nfi);
                                        break;
                                    case "StoryFireInFront":
                                        BeatmapManager.Current.StoryFireInFront = val[0] == '1';
                                        break;
                                    case "UseSkinSprites":
                                        BeatmapManager.Current.UseSkinSpritesInSB = val[0] == '1';
                                        break;
                                }*/
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

                                int combo_offset = (Convert.ToInt32(split[3], GameBase.nfi) >> 4) & 7; // mask out bits 5-7 for combo offset.
                                bool new_combo = (type & HitObjectType.NewCombo) > 0;

                                HitObject h = null;

                                if ((type & HitObjectType.Normal) > 0)
                                {
                                    h = hitFactory.CreateHitCircle(pos, time,
                                                                             lastAddedSpinner ||
                                                                             new_combo,
                                                                             soundType, new_combo ? combo_offset : 0);
                                    lastAddedSpinner = false;
                                }
                                else if ((type & HitObjectType.Slider) > 0)
                                {
                                    /*CurveTypes curveType = CurveTypes.Catmull;
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
                                                                (int)
                                                                (verticalFlip
                                                                     ? 384 - Convert.ToDouble(temp[1], GameBase.nfi)
                                                                     : Convert.ToDouble(temp[1], GameBase.nfi)));
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

                                    //todo: implement sliders
                                    Slider s = hitFactory.CreateSlider(pos, time,
                                                                       lastAddedSpinner ||
                                                                       new_combo, soundType,
                                                                       curveType, repeatCount, length, points, sounds, new_combo ? combo_offset : 0);
                                    lastAddedSpinner = false;
                                    AddSlider(s);*/
                                }
                                else if ((type & HitObjectType.Spinner) > 0)
                                {
                                    h = hitFactory.CreateSpinner(time, Convert.ToInt32(split[5], GameBase.nfi), soundType);
                                    lastAddedSpinner = true;
                                }

                                if (h != null)
                                {
                                    Add(h);
                                }


                                break;
                            case FileSection.Unknown:
                                continue; //todo: readd this?  not sure if we need it anymore.
                        }
                    }
                }

                if (baseReader != null)
                    baseReader.Dispose(); //clumsy stream cleanup (osq)
            }
        }


        #region IDrawable Members

        public void Draw()
        {
            spriteManager.Draw();
        }

        #endregion

        #region IUpdateable Members

        public void Update()
        {
            spriteManager.Update();
        }

        #endregion
    }

    internal enum FileSection
    {
        Unknown,
        General,
        Colours,
        Editor,
        Metadata,
        TimingPoints,
        Events,
        HitObjects,
        Difficulty,
        Variables
    } ;
}