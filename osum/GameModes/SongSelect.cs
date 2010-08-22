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

            HitCircle c = new HitCircle(new Vector2(150, 150), 1000, false, HitObjectSoundType.Clap);
            spriteManager.Add(c);
		}
		
		public override void Draw ()
		{
            Console.WriteLine(Clock.AudioTime);
            base.Draw();
		}
	}
}

