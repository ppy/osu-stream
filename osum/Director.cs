using System;
using osum.GameModes;
using osum.Graphics.Sprites;

namespace osum
{	
	public static class Director
	{
        internal static GameMode CurrentMode {get; private set;}
		internal static OsuMode PendingMode {get; private set;}
		
		private static Transition ActiveTransition;
		
		internal static bool ChangeMode(OsuMode mode, Transition transition)
        {
            if (mode == null) return false;

            if (CurrentMode != null)
                CurrentMode.Dispose();

			if (transition == null)
			{
				changeMode(mode);
				return true;
			}
			
			PendingMode = mode;
			ActiveTransition = transition;
            return true;
        }
		
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
			}
			
			if (mode != null)
			{
				PendingMode = OsuMode.Unknown;
				ActiveTransition = null;
				CurrentMode = mode;
				
				CurrentMode.Initialize();
			}
		}
		
		internal static void Update()
		{
			if (ActiveTransition != null)
			{
				ActiveTransition.Update();
				
				if (ActiveTransition.IsDone)
					changeMode(PendingMode);
			}
			
			CurrentMode.Update();
		}
		
		internal static void Draw()
		{
			CurrentMode.Draw();
		}
	}
}

