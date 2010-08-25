using System;
using osum.GameModes;
using OpenTK.Graphics.ES11;
using osum.GameplayElements;
using OpenTK;
using osum.Helpers;
namespace osum
{
	public class SongSelect : GameMode
	{
		public SongSelect() : base()
		{
		}
		
		internal override void Initialize ()
		{
		}
		
		public override void Draw ()
		{
            Console.WriteLine(Clock.AudioTime);
            base.Draw();
		}
	}
}

