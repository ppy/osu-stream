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
    public partial class HitObjectManager : IDrawable, IDisposable
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

        internal SliderTrackRenderer sliderTrackRenderer;

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

        void GameBase_OnScreenLayoutChanged()
        {
            if (ActiveStreamObjects != null)
            {
                foreach (HitObject h in ActiveStreamObjects.FindAll(h => h is Slider))
                    ((Slider)h).DisposePathTexture();
            }
        }

        public void Dispose()
        {
            spriteManager.Dispose();

            if (sliderTrackRenderer != null) sliderTrackRenderer.Dispose();

            foreach (SpriteManager sm in streamSpriteManagers)
                if (sm != null) sm.Dispose();

            List<HitObject> objects = ActiveStreamObjects;
            if (objects != null)
                foreach (HitObject h in objects)
                    h.Dispose();

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


        int nextPossibleSwitchTime;
        int removeBeforeObjectIndex;

        /// <summary>
        /// Call at the point of judgement. Will switch stream to new difficulty as soon as possible (next new combo).
        /// </summary>
        /// <param name="newDifficulty">The new stream difficulty.</param>
        /// <returns>The time at which the switch will take place. -1 on failure.</returns>
        internal int SetActiveStream(Difficulty newDifficulty)
        {
            Difficulty oldActiveStream = ActiveStream;

            if (ActiveStream == newDifficulty || Clock.AudioTime < nextStreamChange)
                return -1; //already switching stream

            pList<HitObject> oldStreamObjects = ActiveStreamObjects;

            if (oldActiveStream == Difficulty.None)
            {
                //loading a new stream.
                ActiveStream = newDifficulty;
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
                    for (int i = processFrom; i < oldStreamObjects.Count; i++)
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

                for (int i = processFrom; i < removeBeforeObjectIndex; i++)
                    newSpriteManager.Add(oldStreamObjects[i]);

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

            processFrom = 0;

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
                diffObjects = new pList<HitObject>() { UseBackwardsSearch = true };
                StreamHitObjects[diffIndex] = diffObjects;
                streamSpriteManagers[diffIndex] = new SpriteManager() { ForwardPlayOptimisedAdd = true };
            }

            h.Difficulty = difficulty;
            h.Index = diffObjects.Count;

            diffObjects.AddInPlace(h);
            streamSpriteManagers[diffIndex].Add(h);
        }

        /// <summary>
        /// Connect two objects that occur at the same time with a line.
        /// </summary>
        pSprite Connect(HitObject h1, HitObject h2)
        {
            Vector2 p1 = h1.Position;
            Vector2 p2 = h2.Position;

            HitObject firstObject = h1 is HitCircle || h1.EndTime < h2.EndTime ? h1 : h2;

            Vector2 p3 = (p2 + p1) / 2;
            float length = ((p2 - p1).Length - DifficultyManager.HitObjectRadiusSolidGamefield * 1.96f) / DifficultyManager.HitObjectSizeModifier;

            pSprite connectingLine = new pSprite(TextureManager.Load(OsuTexture.connectionline), FieldTypes.GamefieldSprites, OriginTypes.Centre,
                firstObject.Sprites[0].Clocking, p3, SpriteManager.drawOrderBwd(firstObject.EndTime - 15), false, Color4.White);

            //a small hack to allow for texel boundaries to be the correct colour.
            connectingLine.DrawLeft++;
            connectingLine.DrawWidth -= 2;

            connectingLine.Scale = new Vector2(length / 2 * (1 / GameBase.SpriteToBaseRatio), 1);
            connectingLine.Rotation = (float)Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
            connectingLine.Transform(firstObject.Sprites[0].Transformations);

            h2.Sprites.Add(connectingLine);

            h1.connectedObject = h2;
            h2.connectedObject = h1;

            h1.connectionSprite = connectingLine;
            h2.connectionSprite = connectingLine;

            return connectingLine;
        }

        #region IDrawable Members

        public bool Draw()
        {
            if (ActiveStream != Difficulty.None)
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
            NextObject = null;

            for (int i = processFrom; i < activeObjects.Count; i++)
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
                        if (NextObject == null)
                            NextObject = h;
                    }

                    if (!AllowSpinnerOptimisation)
                        AllowSpinnerOptimisation |= h is Spinner && h.Sprites[0].Alpha == 1;

                    if (Player.Autoplay && !h.IsHit && hitObjectNow >= h.StartTime)
                        TriggerScoreChange(h.Hit(), h);
                    if (Clock.AudioTimeSource.IsElapsing || (Clock.AudioTime < 0 && Clock.AudioLeadingIn))
                        TriggerScoreChange(h.CheckScoring(), h);

                    if (lowestActiveObject < 0)
                        lowestActiveObject = i;
                }
                else
                {
                    Slider s = h as Slider;
                    if (s != null && s.EndTime < hitObjectNow)
                        s.DisposePathTexture();
                }

                if (h.StartTime > hitObjectNow + 4000 && !h.IsVisible)
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

            if (objects == null) return null;

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
        Variables,
        ScoringMultipliers
    } ;
}