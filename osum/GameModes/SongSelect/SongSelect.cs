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
using OpenTK.Graphics;
using osum.GameModes.Play.Components;
using osum.Graphics.Drawables;
using osum.GameplayElements;
using System.Threading;
using osum.GameplayElements.Scoring;

namespace osum.GameModes
{
    public partial class SongSelectMode : GameMode
    {
#if iOS
        public static string BeatmapPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.Personal); } }
#else
        public static string BeatmapPath { get { return "Beatmaps"; } }
#endif

        private static List<Beatmap> availableMaps;
        private readonly List<BeatmapPanel> panels = new List<BeatmapPanel>();

        private float songSelectOffset;
        private float difficultySelectOffset;



        SelectState State;


        private float offset_min { get { return panels.Count * -70 + GameBase.BaseSizeFixedWidth.Height - s_Header.DrawHeight - 80; } }
        private float offset_max = 0;
        private float velocity;
        /// <summary>
        /// Offset bound to visible limits.
        /// </summary>
        private float offsetBound
        {
            get
            {
                return Math.Min(offset_max, Math.Max(offset_min, songSelectOffset));
            }
        }

        private pSprite s_Header;
        private pSprite s_Footer;
        private BeatmapPanel SelectedPanel;

        private pDrawable s_ButtonBack;

        internal override void Initialize()
        {
            Player.Difficulty = Difficulty.Normal;

            InputManager.OnMove += InputManager_OnMove;

            InitializeBgm();

            InitializeBeatmaps();

            background =
                new pSprite(TextureManager.Load(OsuTexture.songselect_background), FieldTypes.StandardSnapCentre, OriginTypes.Centre,
                            ClockTypes.Mode, Vector2.Zero, 0, true, Color4.White);
            spriteManager.Add(background);

            s_Header = new pSprite(TextureManager.Load(OsuTexture.songselect_header), new Vector2(0, 0));
            s_Header.Transform(new Transformation(new Vector2(-80, -10), Vector2.Zero, 0, 800, EasingTypes.In));
            s_Header.Transform(new Transformation(TransformationType.Rotation, -0.06f, 0, 0, 800, EasingTypes.In));
            s_Header.OnClick += delegate {};
            spriteManager.Add(s_Header);

            s_Footer = new pSprite(TextureManager.Load(OsuTexture.songselect_footer), FieldTypes.StandardSnapBottomRight, OriginTypes.BottomRight, ClockTypes.Mode, new Vector2(0, -100), 0.98f, true, new Color4(200, 200, 200, 255));
            s_Footer.OnHover += delegate { s_Footer.FadeColour(new Color4(255, 255, 255, 255), 100); };
            s_Footer.OnHoverLost += delegate { s_Footer.FadeColour(new Color4(200, 200, 200, 255), 100); };
            s_Footer.OnClick += onStartButtonPressed;
            spriteManager.Add(s_Footer);

            s_ButtonBack = new BackButton(onBackPressed);

            spriteManager.Add(s_ButtonBack);
        }

        private void onBackPressed(object sender, EventArgs args)
        {
            switch (State)
            {
                case SelectState.SongSelect:
                    State = SelectState.Exiting;
                    Director.ChangeMode(OsuMode.MainMenu);
                    break;
                case SelectState.DifficultySelect:
                case SelectState.LoadingPreview:
                    leaveDifficultySelection(sender, args);
                    break;
            }
        }

        private void InitializeBeatmaps()
        {
            availableMaps = new List<Beatmap>();

            int index = 0;

            string docs = BeatmapPath;

            foreach (string s in Directory.GetFiles(docs, "*.osz2"))
            {
                Beatmap map = new Beatmap(s);

                //string[] files;

                //try
                //{
                //    if (map.Package == null)
                //        files = new string[] { s };
                //    else
                //        files = map.Package.MapFiles;
                //}
                //catch
                //{
                //    //possibly corrupt file. just delete for now.
                //    try
                //    {
                //        File.Delete(s);
                //    }
                //    catch { }
                //    continue;
                //}

                //foreach (string file in files)
                {
                    Beatmap b = new Beatmap(s);

                    BeatmapPanel panel = new BeatmapPanel(b, this, index++);
                    spriteManager.Add(panel);

                    availableMaps.Add(b);
                    panels.Add(panel);
                }
            }

            panelDownloadMore = new BeatmapPanel(null, this, index++);
            panelDownloadMore.s_Text.Text = "Download more songs...";
            panelDownloadMore.s_Text.Colour = new Color4(151, 227, 255, 255);
            panels.Add(panelDownloadMore);
            spriteManager.Add(panelDownloadMore);

            Vector2 pos = new Vector2(400, 0);
            foreach (BeatmapPanel p in panels)
            {
                p.MoveTo(pos, 0);
                pos.Y += 70;
                pos.X += 300;
            }

            /*if (panels.Count > 1)
            {
                onSongSelected(panels[panels.Count - 2], null);
                Ranking.RankableScore = new Score()
                {
                    count100 = 80,
                    count300 = 20,
                    count50 = 35,
                    countGeki = 4,
                    countKatu = 8,
                    countMiss = 12,
                    date = DateTime.Now,
                    maxCombo = 60,
                    totalScore = 1,
                };
    
    
                GameBase.Scheduler.Add(delegate
                {
                    Director.ChangeMode(OsuMode.Ranking);
                }, 500);
            }*/

        }

        /// <summary>
        /// Initializes the song select BGM and starts playing. Static for now so it can be triggered from anywhere.
        /// </summary>
        internal static void InitializeBgm()
        {
            //Start playing song select BGM.
#if iOS
            AudioEngine.Music.Load(File.ReadAllBytes("Skins/Default/songselect.m4a"), true);
#else
            AudioEngine.Music.Load(File.ReadAllBytes("Skins/Default/songselect.mp3"), true);
#endif
            AudioEngine.Music.Play();
        }

        /// <summary>
        /// Called when a panel has been selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        internal void onSongSelected(object sender, EventArgs args)
        {
            AudioEngine.PlaySample(OsuSamples.MenuHit);
            
            BeatmapPanel panel = sender as BeatmapPanel;
            if (panel == null || State != SelectState.SongSelect) return;

            if (panel == panelDownloadMore)
            {
                Director.ChangeMode(OsuMode.Store);
                return;
            }

            Player.Beatmap = panel.Beatmap;

            SelectedPanel = panel;
            State = SelectState.LoadingPreview;

            foreach (BeatmapPanel p in panels)
            {
                p.s_BackingPlate.HandleInput = false;

                if (p == panel) continue;

                foreach (pDrawable s in p.Sprites)
                {
                    s.FadeOut(400);
                    s.MoveTo(new Vector2(200, s.Position.Y), 500, EasingTypes.In);
                }
            }

            panel.s_BackingPlate2.Alpha = 1;
            panel.s_BackingPlate2.AdditiveFlash(400, 1);
            panel.s_BackingPlate2.FadeColour(Color4.White,0);

            GameBase.Scheduler.Add(delegate
            {
                if (State != SelectState.LoadingPreview) return;

                AudioEngine.Music.Load(panel.Beatmap.GetFileBytes(panel.Beatmap.AudioFilename), false);

                GameBase.Scheduler.Add(showDifficultySelection, true);
            }, 400);
        }

        public override void Dispose()
        {
            base.Dispose();

            if (State == SelectState.Exiting)
                Player.Beatmap = null;

            if (tabController != null) tabController.Dispose();

            InputManager.OnMove -= InputManager_OnMove;
        }

        bool touchingBegun;
        private void InputManager_OnMove(InputSource source, TrackingPoint trackingPoint)
        {
            if (!InputManager.IsPressed || InputManager.PrimaryTrackingPoint == null) return;

            touchingBegun = true;

            switch (State)
            {
                case SelectState.SongSelect:
                    {
                        float change = InputManager.PrimaryTrackingPoint.WindowDelta.Y;
                        float bound = offsetBound;

                        if ((songSelectOffset - bound < 0 && change < 0) || (songSelectOffset - bound > 0 && change > 0))
                            change *= Math.Min(1, 10 / Math.Max(0.1f, Math.Abs(songSelectOffset - bound)));
                        songSelectOffset = songSelectOffset + change;
                        velocity = change;
                    }
                    break;
                case SelectState.DifficultySelect:
                    {
                        float change = InputManager.PrimaryTrackingPoint.WindowDelta.X;
                        float bound = Math.Min(mode_button_width, Math.Max(mapRequiresUnlock ? 0 : -mode_button_width, difficultySelectOffset));

                        if ((difficultySelectOffset - bound < 0 && change < 0) || (difficultySelectOffset - bound > 0 && change > 0))
                            change *= Math.Min(1, 10 / Math.Max(0.1f, Math.Abs(difficultySelectOffset - bound)));
                        difficultySelectOffset = difficultySelectOffset + change;
                        velocity = change * 4;
                    }
                    break;
            }
        }

        public override bool Draw()
        {
            base.Draw();

            if (tabController != null) tabController.Draw();

            return true;
        }

        bool pendingModeChange;
        private BeatmapPanel panelDownloadMore;
        private pSprite background;
        public override void Update()
        {
            base.Update();

            if (tabController != null) tabController.Update();

            //handle touch scrolling
            switch (State)
            {
                case SelectState.DifficultySelect:
                    if (tabController.SelectedTab == s_TabBarPlay)
                    {
                        if (InputManager.IsPressed)
                            pendingModeChange = true;
                        else if (pendingModeChange)
                        {
                            difficultySelectOffset += velocity;


                            if (difficultySelectOffset > mode_button_width / 2)
                                SetDifficulty(Difficulty.Easy);
                            else if (!mapRequiresUnlock && difficultySelectOffset < -mode_button_width / 2)
                                SetDifficulty(Difficulty.Expert);
                            else
                                SetDifficulty(Difficulty.Normal);

                            pendingModeChange = false;
                        }

                        if (Director.PendingMode == OsuMode.Unknown)
                        {
                            Vector2 pos = new Vector2(difficultySelectOffset, 0);
                            s_ModeButtonEasy.MoveTo(pos, 200, EasingTypes.In);
                            s_ModeButtonEasy.ScaleScalar = (float)Math.Sqrt(1 - 0.002f * Math.Abs(s_ModeButtonEasy.Offset.X + s_ModeButtonEasy.Position.X));
                            s_ModeButtonStream.MoveTo(pos, 200, EasingTypes.In);
                            s_ModeButtonStream.ScaleScalar = (float)Math.Sqrt(1 - 0.002f * Math.Abs(s_ModeButtonStream.Offset.X + s_ModeButtonStream.Position.X));
                            s_ModeButtonExpert.MoveTo(pos, 200, EasingTypes.In);
                            s_ModeButtonExpert.ScaleScalar = (float)Math.Sqrt(1 - 0.002f * Math.Abs(s_ModeButtonExpert.Offset.X + s_ModeButtonExpert.Position.X));
                        }
                    }

                    break;
                case SelectState.SongSelect:
                    if (!InputManager.IsPressed)
                    {
                        float bound = offsetBound;

                        songSelectOffset = songSelectOffset * 0.8f + bound * 0.2f + velocity;

                        if (songSelectOffset != bound)
                            velocity *= 0.7f;
                        else
                            velocity *= 0.94f;
                    }

                    if (Director.PendingMode == OsuMode.Unknown)
                    {
                        Vector2 pos = new Vector2(0, 60 + songSelectOffset);
                        foreach (BeatmapPanel p in panels)
                        {
                            if (Math.Abs(p.s_BackingPlate.Position.Y - pos.Y) > 1 || Math.Abs(p.s_BackingPlate.Position.X - pos.X) > 1)
                                p.MoveTo(pos, touchingBegun ? 40 : 300);
                            pos.Y += 70;
                        }
                    }
                    break;
            }

            //handle audio adjustments
            switch (State)
            {
                case SelectState.LoadingPreview:
                    if (AudioEngine.Music.Volume > 0)
                        AudioEngine.Music.Volume = Math.Max(0.5f, AudioEngine.Music.Volume * 0.97f);
                    break;
                case SelectState.RankingDisplay:
                case SelectState.DifficultySelect:
                    if (AudioEngine.Music.Volume < 1)
                        AudioEngine.Music.Volume = Math.Min(1, AudioEngine.Music.Volume + 0.02f);
                    break;
                default:
                    break;
            }
        }
    }

    enum SelectState
    {
        SongSelect,
        DifficultySelect,
        LoadingPreview,
        RankingDisplay,
        Starting,
        Exiting
    }
}