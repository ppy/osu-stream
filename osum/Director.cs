using System;
using osum.GameModes;
using osum.Graphics.Sprites;
using osum.Graphics.Skins;
using osum.Support;
using osum.Helpers;
using osum.Audio;
using osum.GameModes.Store;

namespace osum
{
    /// <summary>
    /// Handles display and transitioning of game modes.
    /// </summary>
    public static class Director
    {
        /// <summary>
        /// The active game mode, which is being drawn to screen.
        /// </summary>
        internal static GameMode CurrentMode { get; private set; }
        internal static OsuMode CurrentOsuMode { get; private set; }

        /// <summary>
        /// The next game mode to be displayed (after a possible transition). OsuMode.Unknown when no mode is pending
        /// </summary>
        internal static OsuMode PendingMode { get; private set; }

        /// <summary>
        /// The transition being used to introduce a pending mode.
        /// </summary>
        internal static Transition ActiveTransition;

        internal static bool ChangeMode(OsuMode mode)
        {
            return ChangeMode(mode, new FadeTransition());
        }

        /// <summary>
        /// Actions to perform when transition finishes. NOTE: Is cleared after each transition.
        /// </summary>
        public static event VoidDelegate OnTransitionEnded;

        /// <summary>
        /// Changes the active game mode to a new requested mode, with a possible transition.
        /// </summary>
        /// <param name="mode">The new mode.</param>
        /// <param name="transition">The transition (null for instant switching).</param>
        /// <returns></returns>
        internal static bool ChangeMode(OsuMode mode, Transition transition)
        {
            if (mode == OsuMode.Unknown) return false;

            if (transition == null)
            {
                changeMode(mode);
                return true;
            }

            PendingMode = mode;
            ActiveTransition = transition;

            return true;
        }

        /// <summary>
        /// Handles switching to a new OsuMode. Acts as a fatory to create the material GameMode instance and dispose of any previous mode.
        /// </summary>
        /// <param name="newMode">The new mode specification.</param>
        private static void changeMode(OsuMode newMode)
        {
            //Create the actual mode
            GameMode mode = null;

            switch (newMode)
            {
                case OsuMode.MainMenu:
                    mode = new MainMenu();
                    break;
                case OsuMode.SongSelect:
                    mode = new SongSelectMode();
                    break;
                case OsuMode.Ranking:
                    mode = new Ranking();
                    break;
                case OsuMode.Play:
                    mode = new Player();
                    break;
                case OsuMode.Store:
                    mode = new StoreMode();
                    break;
            }

            if (CurrentMode != null)
                CurrentMode.Dispose();

            TextureManager.DisposeAll(false);

            CurrentMode = mode;
            CurrentMode.Initialize();

            if (PendingMode != OsuMode.Unknown) //can be unknown on first startup
            {
                if (PendingMode != newMode)
                {
                    //we got a new request to load a *different* mode during initialisation...
                    return;
                }

                modeChangePending = true;
            }

            PendingMode = OsuMode.Unknown;
            CurrentOsuMode = newMode;

            GC.Collect(); //force a full collect before we start displaying the new mode.
        }

        static bool modeChangePending;


        /// <summary>
        /// Updates the director, along with current game mode.
        /// </summary>
        internal static bool Update()
        {
            if (modeChangePending)
            {
                //There was a mode change last frame.
                //See below for where this is set.
                Clock.ModeLoadComplete();
                ActiveTransition.FadeIn();
                CurrentMode.OnFirstUpdate();

                modeChangePending = false;
            }

            if (ActiveTransition != null)
            {
                ActiveTransition.Update();

                AudioEngine.Music.Volume = 0.5f + Director.ActiveTransition.CurrentValue * 0.5f;

                if (ActiveTransition.FadeOutDone)
                {
                    while (PendingMode != OsuMode.Unknown)
                        changeMode(PendingMode);
                }

                if (ActiveTransition.FadeInDone)
                {
                    if (OnTransitionEnded != null)
                    {
                        OnTransitionEnded();
                        OnTransitionEnded = null;
                    }

                    ActiveTransition = null;
                }
            }
            else if (GameBase.ActiveNotification != null && GameBase.ActiveNotification.Alpha > 0)
                SpriteManager.UniversalDim = GameBase.ActiveNotification.Alpha * 0.7f;
            else
                SpriteManager.UniversalDim = 0;

            if (modeChangePending) return true;
            //Save the first mode updates after we purge this frame away.
            //Initialising a mode usually takes a fair amount of time and will throw off timings,
            //so we count this as a null frame.

            if (CurrentMode != null)
                CurrentMode.Update();

            return false;
        }

        /// <summary>
        /// Draws the current game mode.
        /// </summary>
        internal static bool Draw()
        {
            if (CurrentMode == null)
                return false;

            CurrentMode.Draw();
            return true;
        }

        public static bool IsTransitioning { get { return ActiveTransition != null; } }
    }
}

