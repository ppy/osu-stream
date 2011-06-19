using System;
using osum.GameModes;
using osum.Graphics.Sprites;
using osum.Graphics.Skins;
using osum.Support;
using osum.Helpers;
using osum.Audio;
using osum.GameModes.Store;
using osum.GameModes.Play;

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
        internal static OsuMode LastOsuMode { get; private set; }

        /// <summary>
        /// The next game mode to be displayed (after a possible transition). OsuMode.Unknown when no mode is pending
        /// </summary>
        internal static OsuMode PendingOsuMode { get; private set; }

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

        private static void TriggerOnTransitionEnded()
        {
            if (OnTransitionEnded != null)
            {
                OnTransitionEnded();
                OnTransitionEnded = null;
            }
        }

        /// <summary>
        /// Changes the active game mode to a new requested mode, with a possible transition.
        /// </summary>
        /// <param name="mode">The new mode.</param>
        /// <param name="transition">The transition (null for instant switching).</param>
        /// <returns></returns>
        internal static bool ChangeMode(OsuMode mode, Transition transition)
        {
            if (mode == OsuMode.Unknown) return false;

            LastOsuMode = CurrentOsuMode;

            if (transition == null)
            {
                changeMode(mode);

                //force a transition-end in this case.
                TriggerOnTransitionEnded();

                return true;
            }

            PendingOsuMode = mode;
            ActiveTransition = transition;

            return true;
        }

        /// <summary>
        /// Handles switching to a new OsuMode. Acts as a fatory to create the material GameMode instance and dispose of any previous mode.
        /// </summary>
        /// <param name="newMode">The new mode specification.</param>
        private static void changeMode(OsuMode newMode)
        {
            if (PendingMode == null)
                loadNewMode(newMode);

            if (CurrentMode != null)
                CurrentMode.Dispose();

            TextureManager.DisposeAll(false);

            AudioEngine.Reset();

            CurrentMode = PendingMode;
            PendingMode = null;

            Clock.ModeTimeReset();

            CurrentMode.Initialize();

            if (PendingOsuMode != OsuMode.Unknown) //can be unknown on first startup
            {
                if (PendingOsuMode != newMode)
                {
                    //we got a new request to load a *different* mode during initialisation...
                    return;
                }

                modeChangePending = true;
            }

            PendingOsuMode = OsuMode.Unknown;
            CurrentOsuMode = newMode;

            GC.Collect(); //force a full collect before we start displaying the new mode.
        }

        private static void loadNewMode(OsuMode newMode)
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
                case OsuMode.Tutorial:
                    mode = new Tutorial();
                    break;
            }

            PendingMode = mode;
        }

        static bool modeChangePending;
        private static GameMode PendingMode;


        /// <summary>
        /// Updates the director, along with current game mode.
        /// </summary>
        internal static bool Update()
        {
            if (modeChangePending)
            {
                //There was a mode change last frame.
                //See below for where this is set.
                Clock.ModeTimeReset();
                if (ActiveTransition != null)
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
                    if (PendingOsuMode != OsuMode.Unknown)
                        changeMode(PendingOsuMode);
                    else if (ActiveTransition.FadeInDone)
                    {
                        TriggerOnTransitionEnded();

                        ActiveTransition.Dispose();
                        ActiveTransition = null;
                    }
                }
            }
            else if (GameBase.ActiveNotification != null && GameBase.ActiveNotification.Alpha > 0)
                SpriteManager.UniversalDim = GameBase.ActiveNotification.Alpha * 0.7f;

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

            if (ActiveTransition != null)
                ActiveTransition.Draw();
            return true;
        }

        public static bool IsTransitioning { get { return ActiveTransition != null; } }
    }
}

