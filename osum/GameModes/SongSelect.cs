using System;
using osum.GameModes;
using OpenTK.Graphics.ES11;
using osum.GameplayElements;
using OpenTK;
using osum.Helpers;
using System.IO;
using osum.Graphics.Skins;
using osum.GameplayElements.Beatmaps;
using System.Collections.Generic;
namespace osum
{
    public class SongSelect : GameMode
    {
        public SongSelect()
            : base()
        {
        }

        static List<Beatmap> availableMaps;

        internal override void Initialize()
        {
            if (availableMaps == null)
                InitializeBeatmaps();
        }

        private void InitializeBeatmaps()
        {
            availableMaps = new List<Beatmap>();

            foreach (string s in Directory.GetFiles("Beatmaps"))
            {
                //pSprite song = new pSprite(TextureManager.Load);
                Console.WriteLine("Loading file " + s);

                Beatmap b = new Beatmap(s);

                foreach (string file in b.Package.MapFiles)
                {
                    Console.WriteLine(" -" + file);
                }
                
                availableMaps.Add(b);

            }
        }

        public override void Draw()
        {
            base.Draw();
        }
    }
}

