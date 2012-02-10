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
using osum.Resources;
using osu_common.Helpers;

namespace osum.GameModes
{
    public partial class SongSelectMode : GameMode
    {
#if iOS
#if MAPPER
        public static string BeatmapPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.Personal); } }
#else
        public static string BeatmapPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/../Library/Caches"; } }
#endif
#else
        public static string BeatmapPath { get { return @"Beatmaps"; } }
#endif

        private pList<Beatmap> maps = new pList<Beatmap>();
        private readonly List<BeatmapPanel> panels = new List<BeatmapPanel>();

        SpriteManager topmostSpriteManager = new SpriteManager();
        SpriteManager spriteManagerDifficultySelect = new SpriteManager();
        SpriteManager songInfoSpriteManager = new SpriteManager();

        private pSprite s_Header;
        private pSprite s_Footer;
        private BeatmapPanel SelectedPanel;
        private BeatmapPanel PreviewingPanel;

        private pDrawable s_ButtonBack;

        SelectState State;

        bool pendingModeChange;
        bool isBound;
        private BeatmapPanel panelDownloadMore;
        private pSprite background;

        /// <summary>
        /// Input is being handled by back button.
        /// </summary>
        private bool inputStolen;

        float lastIntOffset;
        private float songSelectOffset;

        private float difficultySelectOffset;

        private float offset_min { get { return panels.Count * -70 + GameBase.BaseSizeFixedWidth.Height - s_Header.DrawHeight - 80; } }
        private float offset_max = 0;
        private float velocity;

        /// <summary>
        /// True after the first touch on the song select screen. Changes the way the panels move.
        /// </summary>
        bool touchingBegun;

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

        public override void Initialize()
        {
            spriteManager.CheckSpritesAreOnScreenBeforeRendering = true;

            GameBase.Config.SaveConfig();

            InputManager.OnMove += InputManager_OnMove;
            InputManager.OnDown += InputManager_OnDown;

            InitializeBeatmaps();

            background =
                new pSprite(TextureManager.Load(OsuTexture.songselect_background), FieldTypes.StandardSnapCentre, OriginTypes.Centre,
                            ClockTypes.Mode, Vector2.Zero, 0, true, new Color4(56, 56, 56, 255));
            background.AlphaBlend = false;
            spriteManager.Add(background);

            s_Header = new pSprite(TextureManager.Load(OsuTexture.songselect_header), new Vector2(0, 0));
            s_Header.OnClick += delegate { };
            topmostSpriteManager.Add(s_Header);

            s_Footer = new pSprite(TextureManager.Load(OsuTexture.songselect_footer), FieldTypes.StandardSnapBottomRight, OriginTypes.BottomRight, ClockTypes.Mode, new Vector2(0, -100), 0.98f, true, Color4.White)
            {
                Alpha = 0,
                Rotation = 0.04f,
                Position = new Vector2(-60, -85)
            };

            s_Footer.OnClick += footer_onClick;

            spriteManager.Add(s_Footer);

            topmostSpriteManager.Add(s_ButtonBack = new BackButton(onBackPressed, Director.LastOsuMode == OsuMode.MainMenu));

            if (Player.Beatmap != null)
                showDifficultySelection(panels.Find(p => p.Beatmap != null && p.Beatmap.ContainerFilename == Player.Beatmap.ContainerFilename), true);
            else
            {
                InitializeBgm();

                s_Header.Transform(new TransformationV(new Vector2(0, -15), Vector2.Zero, 0, 800, EasingTypes.In));
                s_Header.Transform(new TransformationF(TransformationType.Rotation, -0.06f, 0, 0, 800, EasingTypes.In));

                Vector2 pos = new Vector2(400, 0);
                foreach (BeatmapPanel p in panels)
                {
                    p.MoveTo(pos, 0);
                    pos.Y += BeatmapPanel.PANEL_HEIGHT + 10;
                    pos.X += 300;
                }
            }
        }

        private void onBackPressed(object sender, EventArgs args)
        {
            switch (State)
            {
                default:
                    break;
                case SelectState.SongSelect:
                    State = SelectState.Exiting;
                    Director.ChangeMode(OsuMode.MainMenu);
                    break;
                case SelectState.DifficultySelect:
                case SelectState.LoadingPreview:
                    leaveDifficultySelection(sender, args);
                    break;
                case SelectState.SongInfo:
                    SongInfo_Hide();
                    showDifficultySelection2();
                    break;
                case SelectState.RankingDisplay:
                    Ranking_Hide();
                    showDifficultySelection2();
                    break;
            }
        }

        public static bool ForceBeatmapRefresh = false;

        /// <summary>
        /// Load beatmaps from the database, or by parsing the directory structure in fallback cases.
        /// </summary>
        private void InitializeBeatmaps()
        {
            BeatmapDatabase.Initialize();

#if MAPPER
            //desktop/mapper builds.
            recursiveBeatmaps(BeatmapPath);
#else

            if (BeatmapDatabase.BeatmapInfo.Count > 0 && !ForceBeatmapRefresh)
            {
                bool hasMissingMaps = false;
                foreach (BeatmapInfo bmi in BeatmapDatabase.BeatmapInfo)
                {
                    Beatmap b = bmi.GetBeatmap();
                    if (!File.Exists(b.ContainerFilename))
                    {
                        hasMissingMaps = true;
                        continue;
                    }

                    maps.Add(b);
                }

                if (hasMissingMaps)
                {
                    if (!GameBase.Config.GetValue<bool>("AppleScrewedUp1", false))
                    {
                        //prompt the user that apple just fucked their ass without permission.
                        //we only prompt once per install now for simplicity, but this should be managed on a databsae level.
                        GameBase.Notify(LocalisationManager.GetString(OsuString.AppleReallyScrewedThisUp1), delegate { GameBase.Config.SetValue<bool>("AppleScrewedUp1", true); });
                    }
                }
                else
                {
                    //do this in case we have recovered maps and need to reset the warning (it might happen again!)
                    GameBase.Config.SetValue<bool>("AppleScrewedUp1", false);
                }
            }
            else
            {
                ForceBeatmapRefresh = false;

#if DIST && iOS
                foreach (string s in Directory.GetFiles("Beatmaps/"))
                {
                    //bundled maps
                    Beatmap b = new Beatmap(s);

#if DEBUG
                    Console.WriteLine("Attempting to load " + s);
#endif

                    BeatmapDatabase.PopulateBeatmap(b);
                    maps.AddInPlace(b);
                }
#endif

                foreach (string s in Directory.GetFiles(BeatmapPath, "*.os*"))
                {
                    Beatmap b = new Beatmap(s);

#if DEBUG
                    Console.WriteLine("Attempting to load " + s);
#endif

                    if (b.Package == null)
                        continue;

#if DEBUG
                    Console.WriteLine("Loaded beatmap " + s + " (difficulty " + b.DifficultyStars + ")");
#endif

                    BeatmapDatabase.PopulateBeatmap(b);
                    maps.AddInPlace(b);
                }

                BeatmapDatabase.Write();
            }
#endif

            int index = 0;

            foreach (Beatmap b in maps)
            {
                if (b.Package == null)
                    continue;

                BeatmapPanel panel = new BeatmapPanel(b, panelSelected, index++);
                topmostSpriteManager.Add(panel);
                panels.Add(panel);
            }

            panelDownloadMore = new BeatmapPanel(null, delegate { AudioEngine.PlaySample(OsuSamples.MenuHit); Director.ChangeMode(OsuMode.Store); }, index++);
            panelDownloadMore.s_Text.Text = LocalisationManager.GetString(OsuString.DownloadMoreSongs);
            panelDownloadMore.s_Text.Colour = new Color4(151, 227, 255, 255);
            panels.Add(panelDownloadMore);
            topmostSpriteManager.Add(panelDownloadMore);
        }

        private void recursiveBeatmaps(string subdir)
        {
            if (subdir.Contains("Abandoned"))
                return;

            foreach (string ss in Directory.GetDirectories(subdir))
                recursiveBeatmaps(ss);

            foreach (string s in Directory.GetFiles(subdir, "*.osz2"))
            {
                Beatmap b = new Beatmap(s);
                BeatmapDatabase.PopulateBeatmap(b);
                maps.AddInPlace(b);
            }
        }

        void panelSelected(object sender, EventArgs args)
        {
            AudioEngine.PlaySample(OsuSamples.MenuHit);

            BeatmapPanel panel = ((pDrawable)sender).Tag as BeatmapPanel;

            if (panel == null || State != SelectState.SongSelect) return;

            if (panel == panelDownloadMore)
            {
                Director.ChangeMode(OsuMode.Store);
                return;
            }

            showDifficultySelection(panel);
        }

        /// <summary>
        /// Initializes the song select BGM and starts playing. Static for now so it can be triggered from anywhere.
        /// </summary>
        internal static void InitializeBgm()
        {
            //Start playing song select BGM.
#if iOS
            if (AudioEngine.Music.Load("Skins/Default/songselect.m4a", true))
#else
            if (AudioEngine.Music.Load("Skins/Default/songselect.mp3", true))
#endif
                AudioEngine.Music.Play();
        }

        public override void Dispose()
        {
            base.Dispose();

            topmostSpriteManager.Dispose();
            spriteManagerDifficultySelect.Dispose();
            songInfoSpriteManager.Dispose();
            if (rankingSpriteManager != null) rankingSpriteManager.Dispose();

            foreach (Beatmap b in maps)
                if (b != Player.Beatmap) b.Dispose();

            if (State == SelectState.Exiting)
                Player.Beatmap = null;

            InputManager.OnMove -= InputManager_OnMove;
            InputManager.OnDown -= InputManager_OnDown;
        }

        private void footerHide()
        {
            s_Footer.Transformations.Clear();
            s_Footer.Transform(new TransformationV(s_Footer.Position, new Vector2(-60, -85), Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
            s_Footer.Transform(new TransformationF(TransformationType.Rotation, s_Footer.Rotation, 0.04f, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
            s_Footer.Transform(new TransformationF(TransformationType.Fade, 1, 0, Clock.ModeTime + 500, Clock.ModeTime + 500));
        }

        int lastDownTime;
        void InputManager_OnDown(InputSource source, TrackingPoint trackingPoint)
        {
            lastDownTime = Clock.ModeTime;
        }

        private void InputManager_OnMove(InputSource source, TrackingPoint trackingPoint)
        {
            if (!InputManager.IsPressed || InputManager.PrimaryTrackingPoint == null || inputStolen) return;

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
                        float bound = Math.Min(mode_button_width, Math.Max(-mode_button_width, difficultySelectOffset));

                        velocity = change * 4;

                        if ((difficultySelectOffset - bound < 0 && change < 0) || (difficultySelectOffset - bound > 0 && change > 0))
                            change *= Math.Min(1, 10 / Math.Max(0.1f, Math.Abs(difficultySelectOffset - bound)));
                        difficultySelectOffset = difficultySelectOffset + change;
                    }
                    break;
            }
        }

        public override bool Draw()
        {
            base.Draw();

            if (rankingSpriteManager != null) rankingSpriteManager.Draw();

            spriteManagerDifficultySelect.Draw();
            if (songInfoSpriteManager != null) songInfoSpriteManager.Draw();
            topmostSpriteManager.Draw();
            return true;
        }

        public override void Update()
        {
            base.Update();

            spriteManagerDifficultySelect.Update();
            if (songInfoSpriteManager != null) songInfoSpriteManager.Update();
            if (rankingSpriteManager != null) rankingSpriteManager.Update();
            topmostSpriteManager.Update();

            inputStolen = InputManager.PrimaryTrackingPoint != null && InputManager.PrimaryTrackingPoint.HoveringObject == s_ButtonBack;

            //handle touch scrolling
            switch (State)
            {
                case SelectState.DifficultySelect:
                    if (!AudioEngine.Music.IsElapsing)
                        playFromPreview();

                    if (InputManager.IsPressed && !inputStolen)
                        pendingModeChange = true;
                    else if (pendingModeChange)
                    {
                        difficultySelectOffset += velocity;

                        if (difficultySelectOffset > mode_button_width / 2)
                            SetDifficulty(Difficulty.Easy);
                        else if (difficultySelectOffset < -mode_button_width / 2)
                            SetDifficulty(Difficulty.Expert);
                        else
                            SetDifficulty(Difficulty.Normal);

                        pendingModeChange = false;
                    }

                    if (Director.PendingOsuMode == OsuMode.Unknown)
                    {
                        Vector2 pos = new Vector2(difficultySelectOffset, 0);
                        if (Math.Abs(pos.X - s_ModeButtonEasy.Position.X) > 10)
                        {
                            s_ModeButtonEasy.MoveTo(pos, 200, EasingTypes.In);
                            s_ModeButtonStream.MoveTo(pos, 200, EasingTypes.In);
                            s_ModeButtonExpert.MoveTo(pos, 200, EasingTypes.In);
                        }

                        s_ModeButtonEasy.ScaleScalar = (float)Math.Sqrt(1 - 0.002f * Math.Abs(s_ModeButtonEasy.Offset.X + s_ModeButtonEasy.Position.X));
                        s_ModeButtonStream.ScaleScalar = (float)Math.Sqrt(1 - 0.002f * Math.Abs(s_ModeButtonStream.Offset.X + s_ModeButtonStream.Position.X));
                        s_ModeButtonExpert.ScaleScalar = (float)Math.Sqrt(1 - 0.002f * Math.Abs(s_ModeButtonExpert.Offset.X + s_ModeButtonExpert.Position.X));

                        s_ModeButtonEasy.Update();
                        s_ModeButtonStream.Update();
                        s_ModeButtonExpert.Update();
                    }

                    break;
                case SelectState.SongSelect:

                    float bound = offsetBound;
                    bool wasBound = isBound;
                    isBound = songSelectOffset == bound;

                    if (!InputManager.IsPressed)
                    {
                        float lastOffset = songSelectOffset;
                        songSelectOffset = songSelectOffset * 0.8f + bound * 0.2f + velocity;

                        if (songSelectOffset != bound)
                            velocity *= 0.7f;
                        else
                            velocity *= 0.94f;
                    }

                    float panelHeightPadded = BeatmapPanel.PANEL_HEIGHT + 10;

                    float newIntOffset = isBound ? (int)Math.Round(songSelectOffset / panelHeightPadded) : songSelectOffset / panelHeightPadded;

                    if (Director.PendingOsuMode == OsuMode.Unknown)
                    {
                        if (InputManager.PrimaryTrackingPoint != null && InputManager.IsPressed)
                        {
                            const int time_to_hover = 800;

                            pSprite sprite = InputManager.PrimaryTrackingPoint.HoveringObject as pSprite;

                            if (sprite != null)
                            {
                                BeatmapPanel panel = sprite.Tag as BeatmapPanel;

                                //check for beatmap present; the store link doesn't have one.
                                if (panel != null && panel.Beatmap != null)
                                {
                                    if (SelectedPanel != panel)
                                    {
                                        cancelHoverPreview();

                                        SelectedPanel = panel;
                                        Player.Beatmap = panel.Beatmap;

                                        SelectedPanelHoverGlow = panel.s_BackingPlate2.AdditiveFlash(0, 1, true);
                                        SelectedPanelHoverGlow.FadeIn(time_to_hover);

                                        GameBase.Scheduler.Add(delegate
                                        {
                                            if (panel != SelectedPanel || panel == PreviewingPanel || State != SelectState.SongSelect) return;

                                            cancelLockedHoverPreview();

                                            if (AudioEngine.Music != null && (AudioEngine.Music.lastLoaded != panel.Beatmap.PackIdentifier))
                                            {
                                                AudioEngine.Music.Load(panel.Beatmap.GetFileBytes(panel.Beatmap.AudioFilename), false, panel.Beatmap.PackIdentifier);
                                                if (!AudioEngine.Music.IsElapsing)
                                                    playFromPreview();

                                                SelectedPanelHoverGlow.Alpha = 1;
                                                SelectedPanelHoverGlow.FadeOut(500, 0.8f);
                                                SelectedPanelHoverGlow.Transformations[0].Looping = true;
                                                SelectedPanelHoverGlowLockedIn = SelectedPanelHoverGlow;
                                                SelectedPanelHoverGlow = null;
                                                PreviewingPanel = panel;
                                                PreviewingPanel.Add(SelectedPanelHoverGlowLockedIn);
                                            }
                                        }, time_to_hover);
                                    }
                                    else
                                    {
                                        if (SelectedPanelHoverGlow != null && Math.Abs(SelectedPanelHoverGlow.Position.Y - panel.s_BackingPlate2.Position.Y) > 3)
                                            cancelHoverPreview();
                                    }
                                }
                                else
                                    cancelHoverPreview();
                            }
                        }
                        else
                            cancelHoverPreview();

                        if (newIntOffset != lastIntOffset)
                        {
                            if (isBound && wasBound)
                            {
                                AudioEngine.PlaySample(OsuSamples.MenuClick);
                                background.FlashColour(new Color4(140, 140, 140, 255), 400);
                            }

                            lastIntOffset = newIntOffset;
                        }

                        Vector2 pos = new Vector2(0, 60 + (newIntOffset * panelHeightPadded) * 0.5f + songSelectOffset * 0.5f);

                        foreach (BeatmapPanel p in panels)
                        {
                            if (Math.Abs(p.s_BackingPlate.Position.Y - pos.Y) > 1 || Math.Abs(p.s_BackingPlate.Position.X - pos.X) > 1)
                                //todo: change this to use a draggable spritemanager instead. better performance and will move smoother on lower fps.
                                p.MoveTo(pos, touchingBegun ? 50 : 300);
                            pos.Y += 70;
                        }
                    }
                    break;
            }
        }
        
        /// <summary>
        /// Once a song preview starts from a hover event, a flashing effect is displayed on its panel.
        /// This sprite holds that effect. The reference is used later to cancel it when necessary.
        /// </summary>
        private pDrawable SelectedPanelHoverGlowLockedIn;

        /// <summary>
        /// Cancel the flashing effect on the currently previewing song's panel.
        /// </summary>
        private void cancelLockedHoverPreview()
        {
            if (SelectedPanelHoverGlowLockedIn != null)
            {
                SelectedPanelHoverGlowLockedIn.Transformations.Clear();
                SelectedPanelHoverGlowLockedIn.FadeOut(400);
                SelectedPanelHoverGlowLockedIn = null;
                PreviewingPanel = null;
            }
        }

        /// <summary>
        /// WHen a hover event is started on a particular beatmap panel, it begins to glow brighter.
        /// This holds that glow effect. It is either cancelled if the user moves their finger or when it turns into a locked glow (and the preview plays).
        /// </summary>
        private pDrawable SelectedPanelHoverGlow;

        /// <summary>
        /// Cances the increasing glow effect during a hover even over a particular beatmap panel.
        /// </summary>
        private void cancelHoverPreview()
        {
            SelectedPanel = null;
            if (SelectedPanelHoverGlow != null)
            {
                SelectedPanelHoverGlow.AlwaysDraw = false;
                SelectedPanelHoverGlow.Transformations.Clear();

                SelectedPanelHoverGlow.FadeOut(200);
                SelectedPanelHoverGlow = null;
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
        Exiting,
        SongInfo
    }
}