#region Using Statements

using System;
using System.Collections.Generic;
using osu.GameplayElements.HitObjects;
using osu.GameplayElements.HitObjects.Osu;
using osu.Graphics.Renderers;
using osum.GameplayElements.Beatmaps;
using osum.GameplayElements.HitObjects;
using osum.Graphics.Skins;
using osum.Graphics.Sprites;

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

        internal SliderTrackRenderer sliderTrackRenderer = new SliderTrackRenderer();


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
            h.Colour = SkinManager.DefaultColours[colourIndex];

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

                    HitObjectSpannable s = h as HitObjectSpannable;
                    if (s != null)
                        s.CheckScoring(); //todo: do this in another loop maybe?
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