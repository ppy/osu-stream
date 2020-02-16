#region Using Statements

using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using osum.GameModes.Play;
using osum.GameplayElements.Beatmaps;
using osum.GameplayElements.HitObjects;
using osum.GameplayElements.HitObjects.Osu;
using osum.Graphics;
using osum.Graphics.Renderers;
using osum.Graphics.Sprites;
using osum.Helpers;
using osum.Input;
using osum.Support;

#endregion

namespace osum.GameplayElements
{
    /// <summary>
    /// Class that handles loading of content from a Beatmap, and general handling of anything that involves hitObjects as a group.
    /// </summary>
    public partial class HitObjectManager : IDrawable, IDisposable
    {
        /// <summary>
        /// The loaded beatmap.
        /// </summary>
        protected Beatmap beatmap;

        /// <summary>
        /// A factory to create necessary hitObjects.
        /// </summary>
        private readonly HitFactory hitFactory;

        public pList<HitObject>[] StreamHitObjects = new pList<HitObject>[4];
        internal SpriteManager[] streamSpriteManagers = new SpriteManager[4];

        internal int ProcessFrom;
        internal int ProcessTo = -1;

        /// <summary>
        /// Internal spriteManager for drawing all hitObject related content.
        /// </summary>
        internal SpriteManager spriteManager = new SpriteManager();

        internal SliderTrackRenderer sliderTrackRenderer;

        internal int CountdownTime;

        public HitObjectManager(Beatmap beatmap)
        {
            this.beatmap = beatmap;
            hitFactory = new HitFactoryOsu(this);

            if (GameBase.Instance != null)
            {
                sliderTrackRenderer = new SliderTrackRenderer();
                sliderTrackRenderer.Initialize();
            }

            ResetComboCounts();

            GameBase.OnScreenLayoutChanged += GameBase_OnScreenLayoutChanged;
        }

        private void GameBase_OnScreenLayoutChanged()
        {
            if (ActiveStreamObjects != null)
            {
                foreach (HitObject h in ActiveStreamObjects.FindAll(h => h is Slider))
                    ((Slider)h).DisposePathTexture();
            }
        }

        private HitObject drawBelowOverlayActiveSpinner;

        /// <summary>
        /// When we are spinning, we still want to show the score on top of the spinner display.
        /// This should allow for that.
        /// </summary>
        internal bool DrawBelowOverlay
        {
            get
            {
                if (NextObject is Spinner)
                    drawBelowOverlayActiveSpinner = NextObject;

                if (drawBelowOverlayActiveSpinner != null)
                {
                    if (!drawBelowOverlayActiveSpinner.IsVisible && drawBelowOverlayActiveSpinner.IsHit)
                    {
                        drawBelowOverlayActiveSpinner = null; //the spinner has finished fading out.
                        return false;
                    }

                    return true;
                }

                return false;
            }
        }

        public void Dispose()
        {
            spriteManager.Dispose();

            if (sliderTrackRenderer != null) sliderTrackRenderer.Dispose();

            if (streamSpriteManagers != null)
            {
                foreach (SpriteManager sm in streamSpriteManagers)
                    if (sm != null)
                        sm.Dispose();
                streamSpriteManagers = null;
            }

            List<HitObject> objects = ActiveStreamObjects;
            if (objects != null)
                foreach (HitObject h in objects)
                    h.Dispose();

            GameBase.OnScreenLayoutChanged -= GameBase_OnScreenLayoutChanged;

            OnScoreChanged = null;
            OnStreamChanged = null;
        }

        internal int nextStreamChange;

        internal bool StreamChanging
        {
            get { return nextStreamChange > 0 && nextStreamChange + 1000 >= Clock.AudioTime; }
        }

        /// <summary>
        /// Sets the current stream to the best match found.
        /// This is a temporary solution until we have all difficulties mapped for all maps.
        /// </summary>
        /// <returns></returns>
        public int SetActiveStream()
        {
            if (StreamHitObjects[(int)Difficulty.Normal] != null)
                return SetActiveStream(Difficulty.Normal);

            for (int i = 2; i >= 0; i--)
                if (StreamHitObjects[i] != null)
                    return SetActiveStream((Difficulty)i);

            return -1;
        }


        private int nextPossibleSwitchTime;
        private int removeBeforeObjectIndex;

        /// <summary>
        /// Call at the point of judgement. Will switch stream to new difficulty as soon as possible (next new combo).
        /// </summary>
        /// <param name="newDifficulty">The new stream difficulty.</param>
        /// <returns>The time at which the switch will take place. -1 on failure.</returns>
        public virtual int SetActiveStream(Difficulty newDifficulty, bool instant = false)
        {
            Difficulty oldActiveStream = ActiveStream;

            if (ActiveStream == newDifficulty || (Clock.AudioTime > 0 && Clock.AudioTime < nextStreamChange))
                return -1; //already switching stream

            pList<HitObject> oldStreamObjects = ActiveStreamObjects;

            if (oldActiveStream == Difficulty.None || instant)
            {
                //loading a new stream.
                ActiveStream = newDifficulty;
                ProcessFrom = 0;
                ProcessTo = -1;

                return 0;
            }

            if (StreamHitObjects[(int)newDifficulty] == null)
                return -1; //no difficulty is mapped for the target stream.

            int switchTime = Clock.AudioTime + DifficultyManager.PreEmpt;

            if (oldStreamObjects != null)
            {
                if (nextPossibleSwitchTime < switchTime)
                {
                    //need to find a new switch time.
                    removeBeforeObjectIndex = 0;

                    if (beatmap.StreamSwitchPoints != null)
                    {
                        bool foundPoint = false;
                        int c = beatmap.StreamSwitchPoints.Count;
                        for (int i = 0; i < c; i++)
                            if (beatmap.StreamSwitchPoints[i] > switchTime)
                            {
                                switchTime = beatmap.StreamSwitchPoints[i];
                                foundPoint = true;
                                break;
                            }

                        if (!foundPoint)
                        {
                            //exhausted all stream switch points.
                            nextPossibleSwitchTime = Int32.MaxValue;
                            return -1;
                        }
                    }


                    //find a good point to stream switch. this will be mapper set later.
                    for (int i = ProcessFrom; i < oldStreamObjects.Count; i++)
                        if (oldStreamObjects[i].NewCombo && oldStreamObjects[i].StartTime > switchTime)
                        {
                            removeBeforeObjectIndex = i;
                            switchTime = i > 0 ? oldStreamObjects[i - 1].EndTime : oldStreamObjects[i].StartTime;
                            break;
                        }

                    nextPossibleSwitchTime = switchTime;
                }

                if (removeBeforeObjectIndex == 0)
                {
                    //failed to find a suitable stream switch point.
                    nextPossibleSwitchTime = Int32.MaxValue;
                    return -1;
                }

                switchTime = nextPossibleSwitchTime;

                int judgementStart = (int)(switchTime - Player.Beatmap.beatLengthAt(Clock.AudioTime) * 8);

                //check we are close enough to the switch time to actually judge this
                if (newDifficulty > oldActiveStream && Clock.AudioTime < judgementStart)
                {
#if FULL_DEBUG
                    DebugOverlay.AddLine("Waiting for next judgement section starting at " + judgementStart + "...");
#endif

                    return -1;
                }

                nextPossibleSwitchTime = 0;

                ActiveStream = newDifficulty;

                pList<HitObject> newStreamObjects = ActiveStreamObjects;
                SpriteManager newSpriteManager = ActiveStreamSpriteManager;

                for (int i = ProcessFrom; i < removeBeforeObjectIndex; i++)
                    newSpriteManager.Add(oldStreamObjects[i]);

                if (removeBeforeObjectIndex - ProcessFrom > 0)
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

                        h.Sprites.ForEach(s =>
                        {
                            s.Transformations.Clear();
                            s.Alpha = 0;
                        });
                        h.Dispose();
                    }

                    newStreamObjects.RemoveRange(0, removeBeforeIndex);
                    newStreamObjects.InsertRange(0, oldStreamObjects.GetRange(ProcessFrom, removeBeforeObjectIndex - ProcessFrom));
                }
            }

            ProcessFrom = 0;
            ProcessTo = -1;

            nextStreamChange = switchTime;
            return switchTime;
        }

        internal Difficulty ActiveStream = Difficulty.None;

        internal SpriteManager ActiveStreamSpriteManager
        {
            get
            {
                if (ActiveStream == Difficulty.None || streamSpriteManagers == null)
                    return null;
                return streamSpriteManagers[(int)ActiveStream];
            }
        }

        public pList<HitObject> ActiveStreamObjects
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
            int diffIndex = (int)difficulty;

            pList<HitObject> diffObjects = StreamHitObjects[diffIndex];

            if (diffObjects == null)
            {
                diffObjects = new pList<HitObject> { UseBackwardsSearch = true, InsertAfterOnEqual = true };
                StreamHitObjects[diffIndex] = diffObjects;
                streamSpriteManagers[diffIndex] = new SpriteManager { ForwardPlayOptimisedAdd = true };
            }

            diffObjects.AddInPlace(h);
            streamSpriteManagers[diffIndex].Add(h);
        }

        /// <summary>
        /// Connect two objects that occur at the same time with a line.
        /// </summary>
        private pSprite Connect(HitObject h1, HitObject h2, bool useEnd = false)
        {
            Vector2 p1 = useEnd ? h1.EndPosition : h1.Position;
            Vector2 p2 = h2.Position;

            HitObject firstObject = h1.CompareTo(h2) <= 0 ? h1 : h2;

            float length = ((p2 - p1).Length - DifficultyManager.HitObjectRadiusSolidGamefield * 1.96f) / DifficultyManager.HitObjectSizeModifier;

            pSprite connectingLine = new pSprite(TextureManager.Load(OsuTexture.connectionline), FieldTypes.GamefieldSprites, OriginTypes.Centre,
                firstObject.Sprites[0].Clocking, (p2 + p1) / 2, SpriteManager.drawOrderBwd(firstObject.EndTime - 15), false, Color4.White);

            //a small hack to allow for texel boundaries to be the correct colour.
            connectingLine.DrawLeft++;
            connectingLine.DrawWidth -= 2;
            connectingLine.ExactCoordinates = false;

            connectingLine.Scale = new Vector2(length / 2 * (1 / GameBase.SpriteToBaseRatio), 1);
            connectingLine.Rotation = (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);

            int startTime = (useEnd && h1 is Slider) ? ((Slider)h1).snakingEnd : h1.StartTime - DifficultyManager.PreEmpt;
            startTime = Math.Max(startTime, h2.StartTime - DifficultyManager.PreEmpt);

            connectingLine.Transform(new TransformationF(TransformationType.Fade, 0, 1,
                startTime, startTime + DifficultyManager.FadeIn));

            foreach (Transformation t in (h1.EndTime < h2.EndTime ? h1.Sprites[0].Transformations : h2.Sprites[0].Transformations))
            {
                if (t is TransformationF tf && tf.EndFloat == 0)
                    connectingLine.Transform(t);
            }

            h2.Sprites.Add(connectingLine);
            connectingLine.TagNumeric = HitObject.DIMMABLE_TAG;

            h1.connectedObject = h2;
            h2.connectedObject = h1;

            h1.connectionSprite = connectingLine;
            h2.connectionSprite = connectingLine;

            return connectingLine;
        }

        #region IDrawable Members

        public bool Draw()
        {
            float gameplayScale = GameBase.IsSuperWide ? 0.75f : 1;

            if (ActiveStream != Difficulty.None)
            {
                var currentStreamManager = streamSpriteManagers[(int)ActiveStream];

                currentStreamManager.ScaleScalar = gameplayScale;
                currentStreamManager.Draw();
            }

            spriteManager.ScaleScalar = gameplayScale;
            spriteManager.Draw();

            return true;
        }

        #endregion

        #region IUpdateable Members

        public bool AllowSpinnerOptimisation;

        public void Update()
        {
            AllowSpinnerOptimisation = false;

            List<HitObject> activeObjects = ActiveStreamObjects;

            if (activeObjects == null) return;

            int lowestActiveObject = -1;

            ProcessTo = activeObjects.Count - 1;
            //initialise to the last object. if we don't find an earlier one below, this will be used.

            ActiveObject = null;
            NextObject = null;

            for (int i = ProcessFrom; i < activeObjects.Count; i++)
            {
                HitObject h = activeObjects[i];

                int hitObjectNow = h.ClockingNow;

                if (h.IsVisible || !h.IsHit)
                {
                    h.Update();

                    if (h.StartTime <= hitObjectNow && h.EndTime > hitObjectNow)
                        ActiveObject = h;
                    else if (h.StartTime > hitObjectNow)
                    {
                        if (NextObject == null && !h.IsHit)
                            NextObject = h;
                    }

                    if (!AllowSpinnerOptimisation)
                        AllowSpinnerOptimisation |= h is Spinner && h.Sprites[0].Alpha == 1;

                    if (Player.Autoplay && !h.IsHit && hitObjectNow >= h.StartTime)
                        TriggerScoreChange(h.Hit(), h);
                    if (Clock.AudioTimeSource.IsElapsing)
                        TriggerScoreChange(h.CheckScoring(), h);

                    if (lowestActiveObject < 0)
                        lowestActiveObject = i;
                }
                else
                {
                    if (h is Slider s && s.EndTime < hitObjectNow)
                        s.DisposePathTexture();
                }

                if (h.StartTime > hitObjectNow + 4000 && !h.IsVisible)
                {
                    ProcessTo = i;
                    break; //stop processing after a decent amount of leeway...
                }
            }

            if (lowestActiveObject >= 0)
                ProcessFrom = lowestActiveObject;

            if (nextStreamChange > 0 && nextStreamChange <= Clock.AudioTime)
            {
                if (OnStreamChanged != null)
                    OnStreamChanged(ActiveStream);

                nextStreamChange = 0;
            }

            streamSpriteManagers[(int)(ActiveStream)].Update();

            spriteManager.Update();
        }

        #endregion

        /// <summary>
        /// True when all notes have been hit in the current stream (to the end of the beatmap).
        /// </summary>
        internal bool AllNotesHit
        {
            get
            {
                List<HitObject> objects = ActiveStreamObjects;

                if (objects == null) return false;
                if (objects.Count == 0) return true;

                return objects[objects.Count - 1].IsHit;
            }
        }

        /// <summary>
        /// Finds an object at the specified window-space location.
        /// </summary>
        /// <returns>Found object, null on no object found.</returns>
        internal HitObject FindObjectAt(TrackingPoint tracking)
        {
            List<HitObject> objects = ActiveStreamObjects;

            if (objects == null) return null;

            for (int i = ProcessFrom; i <= ProcessTo; i++)
            {
                HitObject h = objects[i];
                if (h.HitTestInitial(tracking))
                    return h;
            }

            return null;
        }

        internal bool HandlePressAt(TrackingPoint point)
        {
            HitObject found = FindObjectAt(point);

            if (found == null) return false;

            if (Clock.AudioTime < found.StartTime - DifficultyManager.HitWindow300)
            {
                List<HitObject> objects = ActiveStreamObjects;

                int index = objects.IndexOf(found);

                if (index > 0)
                {
                    //check last hitObject has been hit already and isn't still active
                    HitObject last = ActiveStreamObjects[index - 1];
                    if (found.connectedObject != last && !last.IsHit && Clock.AudioTime < last.StartTime)
                    {
                        found.Shake();
                        return true;
                    }
                }
            }

            TriggerScoreChange(found.Hit(), found);
            return true;
        }

        private readonly Dictionary<ScoreChange, int> ComboScoreCounts = new Dictionary<ScoreChange, int>();

        public event ScoreChangeDelegate OnScoreChanged;
        public event StreamChangeDelegate OnStreamChanged;

        /// <summary>
        /// Cached value of the first beat length for the current beatmap. Used for general calculations (circle dimming).
        /// </summary>
        public double FirstBeatLength;

        public HitObject ActiveObject;
        public HitObject NextObject;

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
                int count = objects.Count;

                bool multitouchSameEndTime = hitObject.connectedObject != null && Math.Abs(hitObject.connectedObject.EndTime - hitObject.EndTime) < 10;

                //is next hitObject the end of a combo?
                if (index == count - 1 //last object in the song.
                    || objects[index + 1].NewCombo //next object is a new combo.
                    || (multitouchSameEndTime && index < count - 2 && objects[index + 1] == hitObject.connectedObject && objects[index + 2].NewCombo) //this is part of a multitouch sequence with a new combo following.
                )
                {
                    //apply combo addition
                    if (ComboScoreCounts[ScoreChange.Hit100] == 0 && ComboScoreCounts[ScoreChange.Hit50] == 0 && ComboScoreCounts[ScoreChange.Miss] == 0)
                        change |= ScoreChange.GekiAddition;
                    else if (ComboScoreCounts[ScoreChange.Hit50] == 0 && ComboScoreCounts[ScoreChange.Miss] == 0)
                        change |= ScoreChange.KatuAddition;
                    else
                        change |= ScoreChange.MuAddition;

                    if (!(multitouchSameEndTime && !hitObject.connectedObject.IsHit))
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

        public virtual bool IsLowestStream
        {
            get { return ActiveStream == Difficulty.Easy || ActiveStream == Difficulty.Expert; }
        }

        public virtual bool IsHighestStream
        {
            get { return ActiveStream == Difficulty.Hard || ActiveStream == Difficulty.Expert; }
        } //todo: support easy mode

        internal void StopAllSounds()
        {
            if (ActiveObject != null)
                ActiveObject.StopSound();
        }
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
        Variables,
        ScoringMultipliers
    }

    [Flags]
    public enum ScoreChange
    {
        Ignore = 0,
        MissMinor = 1 << 0,
        Miss = 1 << 1,
        MuAddition = 1 << 3,
        KatuAddition = 1 << 4,
        GekiAddition = 1 << 5,
        SliderTick = 1 << 6,
        SliderRepeat = 1 << 7,
        SliderEnd = 1 << 8,
        Hit50 = 1 << 9,
        Hit100 = 1 << 10,
        Hit300 = 1 << 11,
        SpinnerSpinPoints = 1 << 12,
        SpinnerBonus = 1 << 13,
        Hit50m = Hit50 | MuAddition,
        Hit100m = Hit100 | MuAddition,
        Hit300m = Hit300 | MuAddition,
        Hit100k = Hit100 | KatuAddition,
        Hit300k = Hit300 | KatuAddition,
        Hit300g = Hit300 | GekiAddition,
        HitValuesOnly = Miss | Hit50 | Hit100 | Hit300 | GekiAddition | KatuAddition,
        ComboAddition = MuAddition | KatuAddition | GekiAddition
    }

    public enum Difficulty
    {
        None = -1,
        Easy = 0,
        Normal = 1,
        Hard = 2,
        Expert = 3
    }
}