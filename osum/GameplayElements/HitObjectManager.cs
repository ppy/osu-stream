#region Using Statements

using System;
using System.Collections.Generic;
using osum.GameplayElements.HitObjects;
using osum.GameplayElements.HitObjects.Osu;
using osum.Graphics.Renderers;
using osum.GameplayElements.Beatmaps;
using osum.Graphics.Skins;
using osum.Graphics.Sprites;
using osum.Helpers;
using osum.GameModes;
using osu_common.Helpers;
using osum.Support;
using OpenTK.Graphics;
using osum.Graphics.Primitives;
using OpenTK;
using osum.Audio;

#endregion

namespace osum.GameplayElements
{
    /// <summary>
    /// Class that handles loading of content from a Beatmap, and general handling of anything that involves hitObjects as a group.
    /// </summary>
    internal partial class HitObjectManager : IDrawable, IDisposable
    {
        /// <summary>
        /// The loaded beatmap.
        /// </summary>
        Beatmap beatmap;

        /// <summary>
        /// A factory to create necessary hitObjects.
        /// </summary>
        HitFactory hitFactory;

        internal pList<HitObject>[] StreamHitObjects = new pList<HitObject>[4];
        internal SpriteManager[] streamSpriteManagers = new SpriteManager[4];

        private int processFrom;

        /// <summary>
        /// Internal spriteManager for drawing all hitObject related content.
        /// </summary>
        internal SpriteManager spriteManager = new SpriteManager();

        internal SliderTrackRenderer sliderTrackRenderer = new SliderTrackRenderer();

        public HitObjectManager(Beatmap beatmap)
        {
            this.beatmap = beatmap;
            hitFactory = new HitFactoryOsu(this);

            sliderTrackRenderer.Initialize();

            ResetComboCounts();

            GameBase.OnScreenLayoutChanged += GameBase_OnScreenLayoutChanged;
        }

        void GameBase_OnScreenLayoutChanged()
        {
            foreach (HitObject h in ActiveStreamObjects.FindAll(h => h is Slider))
                ((Slider)h).DisposePathTexture();
        }

        public void Dispose()
        {
            spriteManager.Dispose();

            foreach (SpriteManager sm in streamSpriteManagers)
                if (sm != null) sm.Dispose();

            GameBase.OnScreenLayoutChanged -= GameBase_OnScreenLayoutChanged;

            OnScoreChanged = null;
            OnStreamChanged = null;
        }

        internal int nextStreamChange;

        internal bool StreamChanging { get { return nextStreamChange > 0 && nextStreamChange + 1000 >= Clock.AudioTime; } }

        /// <summary>
        /// Sets the current stream to the best match found.
        /// This is a temporary solution until we have all difficulties mapped for all maps.
        /// </summary>
        /// <returns></returns>
        internal int SetActiveStream()
        {
            if (StreamHitObjects[(int)Difficulty.Normal] != null)
                return SetActiveStream(Difficulty.Normal);

            for (int i = 2; i >= 0; i--)
                if (StreamHitObjects[i] != null)
                    return SetActiveStream((Difficulty)i);

            return -1;
        }


        /// <summary>
        /// Call at the point of judgement. Will switch stream to new difficulty as soon as possible (next new combo).
        /// </summary>
        /// <param name="newDifficulty">The new stream difficulty.</param>
        /// <returns>The time at which the switch will take place. -1 on failure.</returns>
        internal int SetActiveStream(Difficulty newDifficulty)
        {
            Difficulty oldActiveStream = ActiveStream;

            if (ActiveStream == newDifficulty || Clock.AudioTime < nextStreamChange)
                return -1;

            pList<HitObject> oldStreamObjects = ActiveStreamObjects;

            if (StreamHitObjects[(int)newDifficulty] == null)
                return -1;

            ActiveStream = newDifficulty;

            pList<HitObject> newStreamObjects = ActiveStreamObjects;
            SpriteManager newSpriteManager = ActiveStreamSpriteManager;

            int switchTime = Clock.AudioTime;

            if (oldStreamObjects != null)
            {
                int removeBeforeObjectIndex = 0;

                int mustBeAfterTime = Clock.AudioTime + 2000;

                if (beatmap.StreamSwitchPoints != null)
                {
                    bool foundPoint = false;
                    int c = beatmap.StreamSwitchPoints.Count;
                    for (int i = 0; i < c; i++)
                        if (beatmap.StreamSwitchPoints[i] > mustBeAfterTime)
                        {
                            mustBeAfterTime = beatmap.StreamSwitchPoints[i];
                            foundPoint = true;
                            break;
                        }

                    if (!foundPoint)
                        return -1;
                }

                //find a good point to stream switch. this will be mapper set later.
                for (int i = processFrom; i < oldStreamObjects.Count; i++)
                {
                    if (oldStreamObjects[i].NewCombo && oldStreamObjects[i].StartTime > mustBeAfterTime)
                    {
                        removeBeforeObjectIndex = i;
                        switchTime = i > 0 ? oldStreamObjects[i - 1].EndTime : oldStreamObjects[i].StartTime;
                        break;
                    }

                    newSpriteManager.Add(oldStreamObjects[i]);
                }

                if (removeBeforeObjectIndex == 0)
                    //failed to find a suitable stream switch point.
                    return -1;

                if (removeBeforeObjectIndex - processFrom > 0)
                {
                    int removeBeforeIndex = 0;
                    for (int i = 0; i < newStreamObjects.Count; i++)
                    {
                        HitObject h = newStreamObjects[i];

                        if (h.StartTime > switchTime && h.NewCombo)
                        {
                            removeBeforeIndex = i;
                            break;
                        }

                        h.Sprites.ForEach(s => s.Bypass = true);
                        h.Dispose();
                    }

                    newStreamObjects.RemoveRange(0, removeBeforeIndex);
                    newStreamObjects.InsertRange(0, oldStreamObjects.GetRange(processFrom, removeBeforeObjectIndex - processFrom));
                }
            }

#if DEBUG
            Console.WriteLine("Changed stream to " + ActiveStream);
#endif

            processFrom = 0;

            if (oldActiveStream == Difficulty.None)
                return 0; //loading a stream from nothing, not switching.

            nextStreamChange = switchTime;
            return switchTime;
        }

        internal Difficulty ActiveStream = Difficulty.None;

        internal SpriteManager ActiveStreamSpriteManager
        {
            get
            {
                if (ActiveStream == Difficulty.None)
                    return null;

                return streamSpriteManagers[(int)ActiveStream];
            }
        }

        internal pList<HitObject> ActiveStreamObjects
        {
            get
            {
                if (ActiveStream == Difficulty.None)
                    return null;

                return StreamHitObjects[(int)ActiveStream];
            }
        }

        /// <summary>
        /// Adds a new hitObject to be managed by this manager.
        /// </summary>
        /// <param name="h">The hitObject to manage.</param>
        internal void Add(HitObject h, Difficulty difficulty)
        {
            pList<HitObject> diffObjects = StreamHitObjects[(int)difficulty];

            if (diffObjects == null)
            {
                diffObjects = new pList<HitObject>() { UseBackwardsSearch = true };
                StreamHitObjects[(int)difficulty] = diffObjects;
                streamSpriteManagers[(int)difficulty] = new SpriteManager() { ForwardPlayOptimisedAdd = true };
            }

            HitObject lastDiffObject = diffObjects.Count > 0 ? diffObjects[diffObjects.Count - 1] : null;

            int currentComboNumber = 1;

            int colourIndex = lastDiffObject != null ? lastDiffObject.ColourIndex : 0;

            bool sameTimeAsLastAdded = lastDiffObject != null && Math.Abs(h.StartTime - lastDiffObject.StartTime) < 5;

            if (h.NewCombo)
            {
                currentComboNumber = 1;
                if (!sameTimeAsLastAdded) //don't change colour if this is a connceted note
                    colourIndex = (colourIndex + 1 + h.ComboOffset) % TextureManager.DefaultColours.Length;
            }
            else
                currentComboNumber = lastDiffObject == null ? 1 : lastDiffObject.ComboNumber + (lastDiffObject.IncrementCombo ? 1 : 0);

            if (sameTimeAsLastAdded)
            {
                currentComboNumber = Math.Max(1, --currentComboNumber);
                HitObject hLast = diffObjects[diffObjects.Count - 1];
                Connect(hLast, h);
            }

            h.ComboNumber = currentComboNumber;
            h.ColourIndex = colourIndex;
            h.Difficulty = difficulty;
            h.Index = diffObjects.Count;

            diffObjects.AddInPlace(h);

            streamSpriteManagers[(int)difficulty].Add(h);
        }

        /// <summary>
        /// Connect two objects that occur at the same time with a line.
        /// </summary>
        void Connect(HitObject h1, HitObject h2)
        {
            Vector2 p1 = h1.Position;
            Vector2 p2 = h2.Position;

            Vector2 p3 = (p2 + p1) / 2;
            float length = ((p2 - p1).Length - DifficultyManager.HitObjectRadiusSolidGamefield * 1.96f) / DifficultyManager.HitObjectSizeModifier;

            pSprite connectingLine = new pSprite(TextureManager.Load(OsuTexture.connectionline), FieldTypes.GamefieldSprites, OriginTypes.Centre,
                ClockTypes.Audio, p3, SpriteManager.drawOrderBwd(h1.EndTime + 15), false, Color4.White);
            
            //a small hack to allow for texel boundaries to be the correct colour.
            connectingLine.DrawLeft++;
            connectingLine.DrawWidth -= 2;

            connectingLine.Scale = new Vector2(length / 2 * (1 / GameBase.SpriteToBaseRatio), 1);
            connectingLine.Rotation = (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
            connectingLine.Transform(h1.Sprites[0].Transformations);

            Box2 rect = connectingLine.DisplayRectangle;

            h2.Sprites.Add(connectingLine);

            h1.connectedObject = h2;
            h2.connectedObject = h1;

            h1.connectionSprite = connectingLine;
            h2.connectionSprite = connectingLine;
        }

        #region IDrawable Members

        public bool Draw()
        {
            streamSpriteManagers[(int)ActiveStream].Draw();

            spriteManager.Draw();

            return true;
        }

        #endregion

        #region IUpdateable Members

        int processedTo;

        public bool AllowSpinnerOptimisation;

        public void Update()
        {
            AllowSpinnerOptimisation = false;

            spriteManager.Update();

            List<HitObject> activeObjects = ActiveStreamObjects;

            if (activeObjects == null) return;

            streamSpriteManagers[(int)(ActiveStream)].Update();

            int lowestActiveObject = -1;

            processedTo = activeObjects.Count - 1;
            //initialise to the last object. if we don't find an earlier one below, this wil be used.

            ActiveObject = null;

            for (int i = processFrom; i < activeObjects.Count; i++)
            {
                HitObject h = activeObjects[i];

                if (h.IsVisible || !h.IsHit)
                {
                    h.Update();

                    if (h.StartTime <= Clock.AudioTime && h.EndTime > Clock.AudioTime)
                        ActiveObject = h;

                    if (!AllowSpinnerOptimisation)
                        AllowSpinnerOptimisation |= h is Spinner && h.Sprites[0].Alpha == 1;

                    if (Player.Autoplay && !h.IsHit && Clock.AudioTime >= h.StartTime)
                        TriggerScoreChange(h.Hit(), h);

                    if (AudioEngine.Music.IsElapsing)
                        TriggerScoreChange(h.CheckScoring(), h);

                    if (lowestActiveObject < 0)
                        lowestActiveObject = i;
                }
                else
                {
                    Slider s = h as Slider;
                    if (s != null && s.EndTime < Clock.AudioTime)
                        s.DisposePathTexture();
                }

                if (h.StartTime > Clock.AudioTime + 4000 && !h.IsVisible)
                {
                    processedTo = i;
                    break; //stop processing after a decent amount of leeway...
                }
            }

            if (lowestActiveObject >= 0)
                processFrom = lowestActiveObject;

            if (nextStreamChange > 0 && nextStreamChange <= Clock.AudioTime)
            {
                if (OnStreamChanged != null)
                    OnStreamChanged(ActiveStream);

                nextStreamChange = 0;
            }
        }

        #endregion

        /// <summary>
        /// True when all notes have been hit in the current stream (to the end of the beatmap).
        /// </summary>
        internal bool AllNotesHit { get { return ActiveStreamObjects[ActiveStreamObjects.Count - 1].IsHit; } }

        /// <summary>
        /// Finds an object at the specified window-space location.
        /// </summary>
        /// <returns>Found object, null on no object found.</returns>
        internal HitObject FindObjectAt(TrackingPoint tracking)
        {
            List<HitObject> objects = ActiveStreamObjects;
            for (int i = processFrom; i < processedTo + 1; i++)
            {
                HitObject h = objects[i];
                h.Index = i;

                if (h.HitTestInitial(tracking))
                    return h;
            }

            return null;
        }

        internal bool HandlePressAt(TrackingPoint point)
        {
            HitObject found = FindObjectAt(point);

            if (found == null) return false;

            if (found.Index > 0)
            {
                if (Clock.AudioTime < found.StartTime - DifficultyManager.HitWindow300)
                {
                    //check last hitObject has been hit already and isn't still active
                    HitObject last = ActiveStreamObjects[found.Index - 1];
                    if (!last.IsHit && Clock.AudioTime < last.StartTime - DifficultyManager.HitWindow100)
                    {
                        found.Shake();
                        return true;
                    }
                }
            }

            TriggerScoreChange(found.Hit(), found);
            return true;
        }

        Dictionary<ScoreChange, int> ComboScoreCounts = new Dictionary<ScoreChange, int>();

        public event ScoreChangeDelegate OnScoreChanged;
        public event StreamChangeDelegate OnStreamChanged;

        /// <summary>
        /// Cached value of the first beat length for the current beatmap. Used for general calculations (circle dimming).
        /// </summary>
        public double FirstBeatLength;
        public HitObject ActiveObject;

        private void TriggerScoreChange(ScoreChange change, HitObject hitObject)
        {
            if (change == ScoreChange.Ignore) return;

            ScoreChange hitAmount = change & ScoreChange.HitValuesOnly;

            if (hitAmount != ScoreChange.Ignore)
            {
                //handle combo additions here
                ComboScoreCounts[hitAmount] += 1;

                List<HitObject> objects = ActiveStreamObjects;

                int index = objects.IndexOf(hitObject);

                //is next hitObject the end of a combo?
                if (objects.Count - 1 == index || objects[index + 1].NewCombo)
                {
                    //apply combo addition
                    if (ComboScoreCounts[ScoreChange.Hit100] == 0 && ComboScoreCounts[ScoreChange.Hit50] == 0 && ComboScoreCounts[ScoreChange.Miss] == 0)
                        change |= ScoreChange.GekiAddition;
                    else if (ComboScoreCounts[ScoreChange.Hit50] == 0 && ComboScoreCounts[ScoreChange.Miss] == 0)
                        change |= ScoreChange.KatuAddition;
                    else
                        change |= ScoreChange.MuAddition;

                    ResetComboCounts();
                }
            }

            hitObject.HitAnimation(change);

            if (OnScoreChanged != null)
                OnScoreChanged(change, hitObject);
        }

        private void ResetComboCounts()
        {
            ComboScoreCounts[ScoreChange.Miss] = 0;
            ComboScoreCounts[ScoreChange.Hit50] = 0;
            ComboScoreCounts[ScoreChange.Hit100] = 0;
            ComboScoreCounts[ScoreChange.Hit300] = 0;
        }

        public bool IsLowestStream { get { return ActiveStream == Difficulty.Easy || ActiveStream == Difficulty.Expert; } }
        public bool IsHighestStream { get { return ActiveStream == Difficulty.Hard || ActiveStream == Difficulty.Expert; } } //todo: support easy mode
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