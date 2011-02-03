using System;
using osum.GameModes;
using osum.GameplayElements;
using OpenTK;
using osum.Helpers;
using System.IO;
using osum.Graphics.Skins;
using osum.GameplayElements.Beatmaps;
using System.Collections.Generic;
using osum.Graphics.Sprites;
using OpenTK.Graphics;
using osum.GameModes.SongSelect;
using osum.Audio;
namespace osum
{
    public class SongSelect : GameMode
    {
        public SongSelect() : base()
        {
        }

        static List<Beatmap> availableMaps;

        internal override void Initialize()
        {
            InitializeBeatmaps();
			
			InputManager.OnMove += InputManager_OnMove;

#if IPHONE
			AudioEngine.Music.Load(File.ReadAllBytes("Skins/Default/select.m4a"), true);
#else
			AudioEngine.Music.Load(File.ReadAllBytes("Skins/Default/select.mp3"), true);
#endif
			
			AudioEngine.Music.Play();
        }

        void InputManager_OnMove(InputSource source, TrackingPoint trackingPoint)
        {
			if (InputManager.IsPressed)
                offset = offset + InputManager.PrimaryTrackingPoint.WindowDelta.Y;
        }
		
		public override void Dispose()
		{
			base.Dispose();

            AudioEngine.Music.Unload();
			
			InputManager.OnMove -= InputManager_OnMove;
		}
		
		const string BEATMAP_DIRECTORY = "Beatmaps";
		
		List<BeatmapPanel> panels = new List<BeatmapPanel>();
		
        private void InitializeBeatmaps()
        {
            availableMaps = new List<Beatmap>();

#if IPHONE
			string docs = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			
			foreach (string s in Directory.GetFiles(docs,"*.osu"))
            {
				Beatmap b = new Beatmap(docs);
                b.BeatmapFilename = Path.GetFileName(s);
				
				BeatmapPanel panel = new BeatmapPanel(b);
				spriteManager.Add(panel);

                availableMaps.Add(b);
				panels.Add(panel);
			}
#endif
            if (Directory.Exists(BEATMAP_DIRECTORY))
			foreach (string s in Directory.GetDirectories(BEATMAP_DIRECTORY))
            {
                Beatmap reader = new Beatmap(s);

                foreach (string file in reader.Package == null ? Directory.GetFiles(s,"*.osu") : reader.Package.MapFiles)
                {
                    
					Beatmap b = new Beatmap(s);
                    b.BeatmapFilename = Path.GetFileName(file);
					
					BeatmapPanel panel = new BeatmapPanel(b);
					spriteManager.Add(panel);

                    availableMaps.Add(b);
					panels.Add(panel);
                }
			}
        }
		
		float offset;
		
        public override void Update()
        {
            base.Update();
			
            
			if (!InputManager.IsPressed)
				offset = offset * 0.9f + Math.Min(0,Math.Max(panels.Count * -80 + GameBase.BaseSize.Height, offset)) * 0.1f;
			
			if (Director.PendingMode == OsuMode.Unknown)
			{
				Vector2 pos = new Vector2(0,10 + offset);
				foreach (BeatmapPanel p in panels)
				{
					p.MoveTo(pos);
					pos.Y += 80;
				}
			}
        }
    }
}


