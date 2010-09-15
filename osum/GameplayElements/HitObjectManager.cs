#region Using Statements

using System;
using System.Collections.Generic;
using osum.GameplayElements.HitObjects;
using osum.GameplayElements.HitObjects.Osu;
using osum.Graphics.Renderers;
using osum.GameplayElements.Beatmaps;
using osum.GameplayElements.HitObjects;
using osum.Graphics.Skins;
using osum.Graphics.Sprites;
using osum.Graphics.Renderers;
using osum.Helpers;

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

        /// <summary>
        /// The complete list of hitObjects.
        /// </summary>
        internal List<HitObject> hitObjects = new List<HitObject>();

        /// <summary>
        /// Internal spriteManager for drawing all hitObject related content.
        /// </summary>
        internal SpriteManager spriteManager = new SpriteManager();


        //todo: pull this from a support class or something, not #if
#if IPHONE
        internal SliderTrackRenderer sliderTrackRenderer = new SliderTrackRendererIphone();
#else
        internal SliderTrackRenderer sliderTrackRenderer = new SliderTrackRendererDesktop();
#endif

        public HitObjectManager(Beatmap beatmap)
        {
            this.beatmap = beatmap;
            hitFactory = new HitFactoryOsu(this);

            sliderTrackRenderer.Initialize();
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
            h.ColourIndex = colourIndex;

            hitObjects.Add(h);

            spriteManager.Add(h);
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

            //todo: optimise for only visible
            foreach (HitObject h in hitObjects)
                if (h.IsVisible)
                {
                    h.Update();

                    TriggerScoreChange(h.CheckScoring(), h);
                }
        }

        #endregion

        /// <summary>
        /// Finds an object at the specified window-space location.
        /// </summary>
        /// <returns>Found object, null on no object found.</returns>
        internal HitObject FindObjectAt(TrackingPoint tracking)
        {
            foreach (HitObject h in hitObjects)
            {
                if (h.HitTest(tracking))
                    return h;
            }

            return null;
        }

        internal void HandlePressAt(TrackingPoint point)
        {
            HitObject found = FindObjectAt(point);
            if (found != null)
                TriggerScoreChange(found.Hit(), found);
        }

        public event ScoreChangeDelegate OnScoreChanged;
        private void TriggerScoreChange(ScoreChange change, HitObject hitObject)
        {
            if (change == ScoreChange.Ignore) return;

            if (OnScoreChanged != null)
                OnScoreChanged(change, hitObject);
        }

        internal double SliderScoringPointDistance
        {
            get
            {
                return ((100 * beatmap.DifficultySliderMultiplier) / beatmap.DifficultySliderTickRate);
            }

        }


        internal double VelocityAt(int time)
        {
            return (SliderScoringPointDistance * beatmap.DifficultySliderTickRate * (1000F / beatmap.beatLengthAt(time)));
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
        Variables
    } ;
}