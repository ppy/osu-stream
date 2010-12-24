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
using osum.Graphics.Sprites;
using OpenTK.Graphics;
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
            InitializeBeatmaps();
        }
		
		const string BEATMAP_DIRECTORY = "Beatmaps";
		
        private void InitializeBeatmaps()
        {
            availableMaps = new List<Beatmap>();

            Vector2 currentPosition = new Vector2(10,10);

            if (Directory.Exists(BEATMAP_DIRECTORY))
			foreach (string s in Directory.GetDirectories(BEATMAP_DIRECTORY))
            {
                Beatmap reader = new Beatmap(s);

                foreach (string file in reader.Package == null ? Directory.GetFiles(s,"*.osu") : reader.Package.MapFiles)
                {
                    Beatmap b = new Beatmap(s);
                    b.BeatmapFilename = Path.GetFileName(file);
					
                    pText pt = new pText(string.Format("{0}", b.BeatmapFilename), 18, currentPosition, Vector2.Zero, 1, true, Color4.White, false);
                    
                    pt.OnClick += delegate {
                        pt.UnbindAllEvents();

                        pt.MoveTo(pt.Position + new Vector2(20, 0), 1000, EasingTypes.In);

                        Player.SetBeatmap(b);
                        Director.ChangeMode(OsuMode.Play);
                    };

                    pt.OnHover += delegate { pt.Colour = Color4.OrangeRed; };
                    pt.OnHoverLost += delegate {pt.Colour = Color4.White; };

                    spriteManager.Add(pt);

                    currentPosition.Y += 30;

                    availableMaps.Add(b);
                }
            }
			
#if IPHONE
			string docs = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			
			pText ptx = new pText(string.Format("documents folder:"), 18, currentPosition, Vector2.Zero, 1, true, Color4.BlueViolet, false);
			spriteManager.Add(ptx);
			currentPosition.Y += 30;
			
			foreach (string file in Directory.GetFiles(docs,"*.osu"))
            {
                Beatmap b = new Beatmap(docs);
                b.BeatmapFilename = Path.GetFileName(file);
				
                pText pt = new pText(string.Format("{0}", b.BeatmapFilename), 18, currentPosition, Vector2.Zero, 1, true, Color4.White, false);
                
                pt.OnClick += delegate {
                    pt.UnbindAllEvents();

                    pt.MoveTo(pt.Position + new Vector2(20, 0), 1000, EasingTypes.In);

                    Player.SetBeatmap(b);
                    Director.ChangeMode(OsuMode.Play);
                };

                pt.OnHover += delegate { pt.Colour = Color4.OrangeRed; };
                pt.OnHoverLost += delegate {pt.Colour = Color4.White; };

                spriteManager.Add(pt);

                currentPosition.Y += 30;

                availableMaps.Add(b);
			}
#endif
        }

        public override void Draw()
        {
            base.Draw();
        }

        public override void Update()
        {
            base.Update();
        }
    }
}

