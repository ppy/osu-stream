using System;
using osum.GameModes;
using osum.Graphics.Sprites;
using osum.Graphics.Skins;
using osum.Support;
using osum.Helpers;

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
        internal static GameMode CurrentMode {get; private set;}
        internal static OsuMode CurrentOsuMode { get; private set; }

        /// <summary>
        /// The next game mode to be displayed (after a possible transition). OsuMode.Unknown when no mode is pending
        /// </summary>
        internal static OsuMode PendingMode {get; private set;}

        /// <summary>
        /// The transition being used to introduce a pending mode.
        /// </summary>
        private static Transition ActiveTransition;

        internal static bool ChangeMode(OsuMode mode)
        {
            return ChangeMode(mode, new FadeTransition());
        }

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
                    mode = new SongSelect();
                    break;
				case OsuMode.Ranking:
					mode = new Ranking();
					break;
                case OsuMode.Play:
                    mode = new Player();
                    break;
            }
            
            //Can we ever fail to create a mode?
            if (mode != null)
            {
                if (CurrentMode != null)
                    CurrentMode.Dispose();

                TextureManager.DisposeAll();

                CurrentMode = mode;
                CurrentMode.Initialize();

                if (PendingMode != newMode)
                {
                    //we got a new request to load a *different* mode during initialisation...
                    return;
                }

                modeChangePending = true;
            }

            PendingMode = OsuMode.Unknown;
            CurrentOsuMode = newMode;
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
        internal static void Draw()
        {
            if (CurrentMode != null)
                CurrentMode.Draw();
        }

        public static bool IsTransitioning { get { return ActiveTransition != null; } }
    }
}

