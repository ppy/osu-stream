using System;
using osum.GameModes;
using OpenTK.Graphics.ES11;
using osum.GameplayElements;
using OpenTK;
using osum.Helpers;
using System.IO;
using osum.Graphics.Skins;
namespace osum
{
    public class SongSelect : GameMode
    {
        public SongSelect() : base()
        {
        }
        
        internal override void Initialize ()
        {
            foreach (string s in Directory.GetDirectories("Beatmaps"))
            {
                //pSprite song = new pSprite(TextureManager.Load);

            }
        }
        
        public override void Draw ()
        {
            Console.WriteLine(Clock.AudioTime);
            base.Draw();
        }
    }
}

