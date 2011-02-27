using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using osum.Audio;
using osum.GameModes;
using osum.GameplayElements.Beatmaps;
using osum.Graphics.Sprites;
using osum.Graphics.Skins;
using osum.Helpers;
using osum.GameModes.SongSelect;

namespace osum.GameModes
{
    public class SongSelectMode : GameMode
    {
        private const string BEATMAP_DIRECTORY = "Beatmaps";
        private static List<Beatmap> availableMaps;
        private readonly List<BeatmapPanel> panels = new List<BeatmapPanel>();
        
        private float offset;
        private float offset_min { get { return panels.Count * -80 + GameBase.BaseSize.Height - s_Header.DrawHeight; } }
        private float offset_max = 0;

        private float velocity;
        
        /// <summary>
        /// Offset bound to visible limits.
        /// </summary>
        private float offsetBound
        {
            get
            {
                return Math.Min(offset_max, Math.Max(offset_min, offset));
            }
        }


        private pSprite s_Header;

        internal override void Initialize()
        {
            InitializeBeatmaps();

            InputManager.OnMove += InputManager_OnMove;


            //Start playing song select BGM.
#if iOS
            AudioEngine.Music.Load(File.ReadAllBytes("Skins/Default/select.m4a"), true);
#else
            AudioEngine.Music.Load(File.ReadAllBytes("Skins/Default/select.mp3"), true);
#endif
            AudioEngine.Music.Play();

            s_Header = new pSprite(TextureManager.Load(OsuTexture.songselect_header), new Vector2(0,0));
            spriteManager.Add(s_Header);
        }

        public override void Dispose()
        {
            base.Dispose();

            AudioEngine.Music.Unload();

            InputManager.OnMove -= InputManager_OnMove;
        }

        private void InitializeBeatmaps()
        {
            availableMaps = new List<Beatmap>();

#if iOS
            string docs = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            
            foreach (string s in Directory.GetFiles(docs,"*.osu"))
            {
                Beatmap b = new Beatmap(docs);
                b.BeatmapFilename = Path.GetFileName(s);
                
                BeatmapPanel panel = new BeatmapPanel(b, this);
                spriteManager.Add(panel);

                availableMaps.Add(b);
                panels.Add(panel);
            }
#endif

            if (Directory.Exists(BEATMAP_DIRECTORY))
                foreach (string s in Directory.GetDirectories(BEATMAP_DIRECTORY))
                {
                    Beatmap reader = new Beatmap(s);

                    foreach (
                        string file in reader.Package == null ? Directory.GetFiles(s, "*.osu") : reader.Package.MapFiles
                        )
                    {
                        Beatmap b = new Beatmap(s);
                        b.BeatmapFilename = Path.GetFileName(file);

                        BeatmapPanel panel = new BeatmapPanel(b, this);
                        spriteManager.Add(panel);

                        availableMaps.Add(b);
                        panels.Add(panel);
                    }
                }
        }

        bool hasSelected;

        /// <summary>
        /// Called when a panel has been selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        internal void SongSelected(object sender, EventArgs args)
        {
            BeatmapPanel panel = sender as BeatmapPanel;
            if (panel == null || hasSelected) return;

            hasSelected = true;

            foreach (BeatmapPanel p in panels)
            {
                if (p == panel)
                {
                    foreach (pSprite s in p.SpriteCollection)
                    {
                        s.MoveTo(new Vector2(0, 60), 500, EasingTypes.InDouble);
                    }
                }
                else
                {
                    foreach (pSprite s in p.SpriteCollection)
                        s.FadeOut(100);
                }
            }

            GameBase.Scheduler.Add(delegate
            {
                Player.SetBeatmap(panel.Beatmap);
                Director.ChangeMode(OsuMode.Play);
            }, 900);
        }

        private void InputManager_OnMove(InputSource source, TrackingPoint trackingPoint)
        {
            if (InputManager.IsPressed)
            {
                float change = InputManager.PrimaryTrackingPoint.WindowDelta.Y;
                if ((offset - offsetBound < 0 && change < 0) || (offset - offsetBound > 0 && change > 0))
                    change *= Math.Min(1, 10 / Math.Max(0.1f, Math.Abs(offset - offsetBound)));
                offset = offset + change;
                velocity = change;
            }
        }

        public override void Update()
        {
            base.Update();


            if (!hasSelected)
            {
                if (!InputManager.IsPressed)
                {
                    offset = offset * 0.9f + offsetBound * 0.1f + velocity;
                    velocity *= 0.9f;
                }


                if (Director.PendingMode == OsuMode.Unknown)
                {
                    Vector2 pos = new Vector2(0, 60 + offset);
                    foreach (BeatmapPanel p in panels)
                    {
                        p.MoveTo(pos);
                        pos.Y += 80;
                    }
                }
            }
        }
    }
}