using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using osum.Audio;
using osum.GameModes.Play;
using osum.GameplayElements;
using osum.GameplayElements.Beatmaps;
using osum.Graphics;
using osum.Graphics.Sprites;
using osum.Helpers;
using osum.Input;
using osum.Input.Sources;
using osum.Localisation;

namespace osum.GameModes.SongSelect
{
    public partial class SongSelectMode
    {
#if iOS
    #if !DIST
            public static string BeatmapPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.Personal); } }
    #else
            public static string BeatmapPath { get { return Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/../Library/Caches"; } }
    #endif
#else
        public static string BeatmapPath => @"/sdcard/Beatmaps/";
#endif

        private readonly pList<Beatmap> maps = new pList<Beatmap>(new BeatmapPackComparer(), false);
        private readonly List<BeatmapPanel> panels = new List<BeatmapPanel>();

        private readonly SpriteManager topmostSpriteManager = new SpriteManager();
        private readonly SpriteManager spriteManagerDifficultySelect = new SpriteManager();
        private readonly SpriteManager songInfoSpriteManager = new SpriteManager();

        private pSprite s_Header;
        private pSprite s_Footer;
        private BeatmapPanel SelectedPanel;
        private BeatmapPanel PreviewingPanel;

        private pDrawable s_ButtonBack;

        private SelectState State;

        private bool pendingModeChange;
        private bool isBound;
        private BeatmapPanel panelDownloadMore;
        private pSprite background;

        /// <summary>
        /// Input is being handled by back button.
        /// </summary>
        private bool inputStolen;

        private float lastIntOffset;
        private static float songSelectOffset;

        private float difficultySelectOffset;

        private float offset_min => panels.Count * -70 + GameBase.BaseSizeFixedWidth.Y - s_Header.DrawHeight - 80;

        private readonly float offset_max = 0;
        private float velocity;

        /// <summary>
        /// True after the first touch on the song select screen. Changes the way the panels move.
        /// </summary>
        private bool touchingBegun;

        /// <summary>
        /// Offset bound to visible limits.
        /// </summary>
        private float offsetBound => Math.Min(offset_max, Math.Max(offset_min, songSelectOffset));

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
            background.Scale.X = background.DrawWidth / GameBase.BaseSize.X;
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
                    break;
                case SelectState.RankingDisplay:
                    Ranking_Hide();
                    showDifficultySelection2();
                    break;
            }
        }

        public static bool ForceBeatmapRefresh;

        /// <summary>
        /// Load beatmaps from the database, or by parsing the directory structure in fallback cases.
        /// </summary>
        private void InitializeBeatmaps()
        {
            string udid = GameBase.Instance.DeviceIdentifier; //cache the udid before possibly writing the database out.

            BeatmapDatabase.Initialize();

#if !DIST
            if (GameBase.Mapper)
            {
                //desktop/mapper builds.
                recursiveBeatmaps(BeatmapPath);
            }
            else
#endif
            if (BeatmapDatabase.BeatmapInfo.Count > 0 && !ForceBeatmapRefresh && BeatmapDatabase.Version == BeatmapDatabase.DATABASE_VERSION)
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

                    maps.AddInPlace(b);
                }
            }
            else
            {
#if !DIST
                Console.WriteLine("Regenerating database!");
#endif

                ForceBeatmapRefresh = false;

#if iOS
                    //bundled maps
                    foreach (string s in Directory.GetFiles("Beatmaps/"))
                    {

                        Beatmap b = new Beatmap(s);

                        BeatmapDatabase.PopulateBeatmap(b);
                        maps.AddInPlace(b);
                    }
#endif

                foreach (string s in Directory.GetFiles(BeatmapPath, "*.os*"))
                {
                    Beatmap b = new Beatmap(s);

                    if (b.Package == null)
                        continue;

                    BeatmapDatabase.PopulateBeatmap(b);
                    maps.AddInPlace(b);
                }

                BeatmapDatabase.Write();
            }

            int index = 0;

            string lastPackId = null;
            foreach (Beatmap b in maps)
            {
                if (b.Package == null)
                    continue;

                BeatmapPanel panel = new BeatmapPanel(b, panelSelected, index++);
                if (b.PackId != lastPackId)
                {
                    panel.NewSection = true;
                    lastPackId = b.PackId;
                }

                topmostSpriteManager.Add(panel);
                panels.Add(panel);
            }

            panelDownloadMore = new BeatmapPanel(null, delegate
            {
                AudioEngine.PlaySample(OsuSamples.MenuHit);
                State = SelectState.Exiting;
                Director.ChangeMode(OsuMode.Store);
            }, index++) { NewSection = true };

            panelDownloadMore.s_Text.Text = LocalisationManager.GetString(OsuString.DownloadMoreSongs);
            panelDownloadMore.s_Text.Colour = Color4.White;
            panelDownloadMore.s_Text.Offset.Y += 16;
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

        private void panelSelected(object sender, EventArgs args)
        {
            if (!(((pDrawable)sender).Tag is BeatmapPanel panel) || State != SelectState.SongSelect) return;

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

            cancelHoverPreview();
            cancelLockedHoverPreview();

            topmostSpriteManager.Dispose();
            spriteManagerDifficultySelect.Dispose();
            songInfoSpriteManager.Dispose();
            if (rankingSpriteManager != null) rankingSpriteManager.Dispose();

            foreach (Beatmap b in maps)
                if (b != Player.Beatmap)
                    b.Dispose();

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

        private int lastDownTime;

        private void InputManager_OnDown(InputSource source, TrackingPoint trackingPoint)
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

        private const int time_to_hover = 400;

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
                        songSelectOffset = songSelectOffset * (1 - 0.2f * Clock.ElapsedRatioToSixty) + bound * 0.2f * Clock.ElapsedRatioToSixty + velocity * Clock.ElapsedRatioToSixty;

                        if (songSelectOffset != bound)
                            velocity *= (1 - 0.3f * Clock.ElapsedRatioToSixty);
                        else
                            velocity *= (1 - 0.06f * Clock.ElapsedRatioToSixty);
                    }

                    float panelHeightPadded = BeatmapPanel.PANEL_HEIGHT + 10;

                    float newIntOffset = isBound ? (int)Math.Round(songSelectOffset / panelHeightPadded) : songSelectOffset / panelHeightPadded;

                    if (Director.PendingOsuMode == OsuMode.Unknown)
                    {
                        if (InputManager.PrimaryTrackingPoint != null && InputManager.IsPressed)
                        {
                            if (InputManager.PrimaryTrackingPoint.HoveringObject is pSprite sprite)
                            {
                                //check for beatmap present; the store link doesn't have one.
                                if (sprite.Tag is BeatmapPanel panel && panel.Beatmap != null)
                                {
                                    if (SelectedPanel != panel && PreviewingPanel != panel)
                                    {
                                        cancelHoverPreview();

                                        SelectedPanel = panel;
                                        Player.Beatmap = panel.Beatmap;

                                        SelectedPanelHoverGlow = panel.s_BackingPlate2.AdditiveFlash(0, 1, true);
                                        SelectedPanelHoverGlow.FadeIn(time_to_hover);

                                        //cancel any previously scheduled preview that was not activated yet.
                                        GameBase.Scheduler.Cancel(previewDelegate);

                                        previewDelegate = GameBase.Scheduler.Add(delegate
                                        {
                                            if (panel != SelectedPanel || panel == PreviewingPanel || State != SelectState.SongSelect) return;

                                            cancelLockedHoverPreview();

                                            if (AudioEngine.Music != null && (AudioEngine.Music.lastLoaded != panel.Beatmap.PackageIdentifier))
                                            {
                                                AudioEngine.Music.Load(panel.Beatmap.GetFileBytes(panel.Beatmap.AudioFilename), false, panel.Beatmap.PackageIdentifier);
                                                if (!AudioEngine.Music.IsElapsing)
                                                    playFromPreview();

                                                SelectedPanelHoverGlow.Alpha = 1;
                                                SelectedPanelHoverGlow.Colour = Color4.White;
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
                        {
                            cancelHoverPreview();
                            if (!AudioEngine.Music.IsElapsing)
                                InitializeBgm();
                        }

                        if (newIntOffset != lastIntOffset)
                        {
                            if (isBound && wasBound)
                            {
                                AudioEngine.PlaySample(OsuSamples.MenuClick);
                                background.FlashColour(new Color4(140, 140, 140, 255), 400);
                            }

                            lastIntOffset = newIntOffset;
                        }

                        if (SelectedPanelHoverGlow != null)
                            AudioEngine.Music.DimmableVolume = 1 - SelectedPanelHoverGlow.Alpha;

                        Vector2 pos = new Vector2(0, 60 + (newIntOffset * panelHeightPadded) * 0.5f + songSelectOffset * 0.5f);

                        foreach (BeatmapPanel p in panels)
                        {
                            if (p.NewSection)
                                pos.Y += 20;

                            if (Math.Abs(p.s_BackingPlate.Position.Y - pos.Y) > 1 || Math.Abs(p.s_BackingPlate.Position.X - pos.X) > 1)
                                //todo: change this to use a draggable spritemanager instead. better performance and will move smoother on lower fps.
                                p.MoveTo(pos, touchingBegun ? 50 : 300);
                            pos.Y += 63;
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
        /// Holds the scheduled preview. We keep a reference to this so we can cancel previously scheduled previews that have not yet been activated.
        /// </summary>
        private ScheduledDelegate previewDelegate;

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

    internal enum SelectState
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