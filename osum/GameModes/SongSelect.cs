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

namespace osum.GameModes
{
    public class SongSelectMode : GameMode
    {
        private const string BEATMAP_DIRECTORY = "Beatmaps";
        private static List<Beatmap> availableMaps;
        private readonly List<BeatmapPanel> panels = new List<BeatmapPanel>();

        private float offset;
        private float offset_min { get { return panels.Count * -70 + GameBase.BaseSize.Height - s_Header.DrawHeight; } }
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
        private pSprite s_Footer;

        internal override void Initialize()
        {
            InitializeBeatmaps();

            Player.SetDifficulty(Difficulty.Normal);

            InputManager.OnMove += InputManager_OnMove;


            InitializeBgm();

            s_Header = new pSprite(TextureManager.Load(OsuTexture.songselect_header), new Vector2(0, 0));

            s_Header.Transform(new Transformation(new Vector2(-60, 0), Vector2.Zero, 0, 500, EasingTypes.In));
            s_Header.Transform(new Transformation(TransformationType.Rotation, -0.06f, 0, 0, 500, EasingTypes.In));

            spriteManager.Add(s_Header);

            s_Footer = new pSprite(TextureManager.Load(OsuTexture.songselect_footer), FieldTypes.StandardSnapBottomLeft, OriginTypes.BottomLeft, ClockTypes.Mode, new Vector2(0, -100), 1, true, Color4.White);
            s_Footer.OnClick += gameStart;
            spriteManager.Add(s_Footer);

            InitializePostSelectionOptions();
        }

        private void InitializeBgm()
        {
            //Start playing song select BGM.
#if iOS
            AudioEngine.Music.Load(File.ReadAllBytes("Skins/Default/select.m4a"), true);
#else
            AudioEngine.Music.Load(File.ReadAllBytes("Skins/Default/select.mp3"), true);
#endif
            AudioEngine.Music.Play();
        }

        private pSpriteCollection spritesDifficultySelection = new pSpriteCollection();

        private pButton s_ButtonEasy;
        private pButton s_ButtonStandard;
        private pButton s_ButtonExpert;
        private pDrawable s_ButtonExpertUnlock;

        private pRectangle s_DifficultySelectionRectangle;

        private void InitializePostSelectionOptions()
        {
            Vector2 border = new Vector2(4, 4);

            int ypos = 94;
            float spacing = border.X;

            Vector2 buttonSize = new Vector2((GameBase.BaseSize.Width - spacing * 4) / 3f, 100);

            float currX = spacing;

            s_ButtonEasy = new pButton("Easy", new Vector2(currX, ypos), buttonSize, PlayfieldBackground.COLOUR_EASY, difficultySelected);

            s_ButtonEasy.Sprites.Add(new pText("- For Beginners\n- Locked to Easy stream\n- No Fail", 13, new Vector2(currX, ypos + 40), buttonSize, 0.55f, true, Color4.White, false));

            spritesDifficultySelection.Add(s_ButtonEasy);

            currX += buttonSize.X + spacing;

            s_ButtonStandard = new pButton("Standard", new Vector2(currX, ypos), buttonSize, PlayfieldBackground.COLOUR_STANDARD, difficultySelected);

            s_ButtonStandard.Sprites.Add(new pText("- Standard gameplay\n- Three streams\n- Can Fail", 13, new Vector2(currX, ypos + 40), buttonSize, 0.55f, true, Color4.White, false));

            spritesDifficultySelection.Add(s_ButtonStandard);

            s_DifficultySelectionRectangle = new pRectangle(new Vector2(0, ypos - border.Y), new Vector2(GameBase.BaseSize.Width, buttonSize.Y + border.Y * 2), true, 0.3f, Color4.Gray);
            spritesDifficultySelection.Add(s_DifficultySelectionRectangle);

            s_DifficultySelectionRectangle = new pRectangle(new Vector2(currX, ypos), buttonSize + border * 2, true, 0.4f, Color4.LightGray) { Offset = -border };
            spritesDifficultySelection.Add(s_DifficultySelectionRectangle);

            currX += buttonSize.X + spacing;

            s_ButtonExpert = new pButton("Expert", new Vector2(currX, ypos), buttonSize, PlayfieldBackground.COLOUR_WARNING, difficultySelected);

            s_ButtonExpertUnlock = new pText("Unlock by passing on standard play first!", 13, new Vector2(currX, ypos + 40), buttonSize, 0.55f, true, Color4.LightGray, false);
            s_ButtonExpert.Sprites.Add(s_ButtonExpertUnlock);

            spritesDifficultySelection.Add(s_ButtonExpert);

            currX += buttonSize.X + spacing;

            s_ButtonStart = new pButton("Start!", new Vector2(GameBase.BaseSize.Width * 0.675f, ypos + 120), new Vector2(140, 40), Color4.DarkViolet, gameStart);
            s_ButtonStart.s_Text.Offset = new Vector2(0, 8);
            spritesDifficultySelection.Add(s_ButtonStart);

            s_ButtonBack = new pButton("Back", new Vector2(GameBase.BaseSize.Width * 0.125f, ypos + 120), new Vector2(140, 40), Color4.DarkViolet, backToSelect);
            s_ButtonBack.s_Text.Offset = new Vector2(0, 8);
            spritesDifficultySelection.Add(s_ButtonBack);

            spriteManager.Add(spritesDifficultySelection);
            spritesDifficultySelection.Sprites.ForEach(s => s.Alpha = 0);
        }

        bool hasSelected;
        bool previewLoaded;

        /// <summary>
        /// Called when a panel has been selected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        internal void SongSelected(object sender, EventArgs args)
        {
            BeatmapPanel panel = sender as BeatmapPanel;
            if (panel == null || hasSelected) return;

            Player.SetBeatmap(panel.Beatmap);

            hasSelected = true;

            foreach (BeatmapPanel p in panels)
            {
                p.s_BackingPlate.HandleInput = false;

                if (p == panel) continue;

                foreach (pDrawable s in p.Sprites)
                {
                    //s.MoveTo(s.Position + new Vector2(-400, 0), 500, EasingTypes.InDouble);
                    s.FadeOut(100);
                }
            }

            panel.s_BackingPlate.FlashColour(Color4.White, 500);

            GameBase.Scheduler.Add(delegate
            {
                AudioEngine.Music.Load(panel.Beatmap.GetFileBytes(panel.Beatmap.AudioFilename), false);
                AudioEngine.Music.Play();
                AudioEngine.Music.Volume = 0;
                AudioEngine.Music.SeekTo(30000);
                previewLoaded = true;

                GameBase.Scheduler.Add(delegate
                {
                    foreach (pDrawable s in panel.Sprites)
                        s.MoveTo(new Vector2(0, 30), 500, EasingTypes.InDouble);

                    spritesDifficultySelection.Sprites.ForEach(s => s.FadeIn(200));

                    bool requiresUnlock = true;

                    if (!requiresUnlock)
                    {
                        s_ButtonExpert.Colour = PlayfieldBackground.COLOUR_WARNING;
                        s_ButtonExpertUnlock.Transformations.Clear();
                        s_ButtonExpert.Enabled = true;
                    }
                    else
                    {
                        s_ButtonExpert.Colour = Color4.Gray;
                        s_ButtonExpert.Enabled = false;
                    }

                    s_Header.Transform(new Transformation(Vector2.Zero, new Vector2(0, -19), Clock.ModeTime, Clock.ModeTime + 300, EasingTypes.In));
                    s_Header.Transform(new Transformation(TransformationType.Rotation, 0, 0.03f, Clock.ModeTime, Clock.ModeTime + 300, EasingTypes.In));

                    s_Footer.Transform(new Transformation(new Vector2(-60, -35), Vector2.Zero, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
                    s_Footer.Transform(new Transformation(TransformationType.Rotation, 0.06f, 0, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
                }, true);
            }, 400);
        }

        private void difficultySelected(object sender, EventArgs args)
        {
            pButton button = sender as pButton;
            if (button == null) return;

            if (button == s_ButtonEasy)
                Player.SetDifficulty(Difficulty.Easy);
            else if (button == s_ButtonExpert)
                Player.SetDifficulty(Difficulty.Expert);
            else
                Player.SetDifficulty(Difficulty.Normal);

            s_DifficultySelectionRectangle.MoveTo(((pButton)sender).Position, 500, EasingTypes.In);
        }

        bool hasStarted;
        private pButton s_ButtonStart;
        private pButton s_ButtonBack;

        private void backToSelect(object sender, EventArgs args)
        {
            hasSelected = false;
            previewLoaded = false;

            InitializeBgm();

            GameBase.Scheduler.Add(delegate
            {
                foreach (BeatmapPanel p in panels)
                {
                    p.s_BackingPlate.HandleInput = true;

                    foreach (pDrawable d in p.Sprites)
                        d.FadeIn(200);
                }

                spritesDifficultySelection.Sprites.ForEach(s => s.FadeOut(50));

                s_Header.Transform(new Transformation(new Vector2(0, -19), Vector2.Zero, Clock.ModeTime, Clock.ModeTime + 300, EasingTypes.In));
                s_Header.Transform(new Transformation(TransformationType.Rotation, s_Header.Rotation, 0, Clock.ModeTime, Clock.ModeTime + 300, EasingTypes.In));

                s_Footer.Transform(new Transformation(s_Footer.Position, new Vector2(-60, -35), Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
                s_Footer.Transform(new Transformation(TransformationType.Rotation, 0, 0.06f, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
            }, true);
        }

        private void gameStart(object sender, EventArgs args)
        {
            if (hasStarted) return;
            hasStarted = true;

            if (sender != s_ButtonEasy) s_ButtonEasy.Sprites.ForEach(s => s.FadeOut(200));
            if (sender != s_ButtonStandard) s_ButtonStandard.Sprites.ForEach(s => s.FadeOut(200));
            if (sender != s_ButtonExpert) s_ButtonExpert.Sprites.ForEach(s => s.FadeOut(200));

            GameBase.Scheduler.Add(delegate
            {
                Director.ChangeMode(OsuMode.Play);
            }, 900);
        }


        public override void Dispose()
        {
            base.Dispose();

            InputManager.OnMove -= InputManager_OnMove;
        }

        private void InitializeBeatmaps()
        {
            availableMaps = new List<Beatmap>();

            int index = 0;

#if iOS
            string docs = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            
            foreach (string s in Directory.GetFiles(docs,"*.osz2"))
            {
                Beatmap reader = new Beatmap(s);

                string[] files = reader.Package == null ? new string[]{s} : reader.Package.MapFiles;
                foreach (string file in files)
                {
                    Beatmap b = new Beatmap(s);
                    b.BeatmapFilename = Path.GetFileName(file);

                    BeatmapPanel panel = new BeatmapPanel(b, this, index++);
                    spriteManager.Add(panel);

                    availableMaps.Add(b);
                    panels.Add(panel);
                }
            }
#endif

            if (Directory.Exists(BEATMAP_DIRECTORY))
                foreach (string s in Directory.GetFiles(BEATMAP_DIRECTORY))
                {
                    Beatmap reader = new Beatmap(s);

                    string[] files = reader.Package == null ? Directory.GetFiles(s, "*.osc") : reader.Package.MapFiles;
                    foreach (string file in files)
                    {
                        Beatmap b = new Beatmap(s);
                        b.BeatmapFilename = Path.GetFileName(file);

                        BeatmapPanel panel = new BeatmapPanel(b, this, index++);
                        spriteManager.Add(panel);

                        availableMaps.Add(b);
                        panels.Add(panel);
                    }
                }
        }

        private void InputManager_OnMove(InputSource source, TrackingPoint trackingPoint)
        {
            if (InputManager.IsPressed)
            {
                float change = InputManager.PrimaryTrackingPoint.WindowDelta.Y;
                float bound = offsetBound;

                if ((offset - bound < 0 && change < 0) || (offset - bound > 0 && change > 0))
                    change *= Math.Min(1, 10 / Math.Max(0.1f, Math.Abs(offset - bound)));
                offset = offset + change;
                velocity = change;
            }
        }

        public override void Update()
        {
            base.Update();

            if (hasSelected && !previewLoaded)
            {
                if (AudioEngine.Music.Volume > 0)
                    AudioEngine.Music.Volume -= 0.05f;
            }
            if (AudioEngine.Music.Volume < 1)
                AudioEngine.Music.Volume += 0.005f;

            if (hasSelected)
            {

            }
            else
            {
                if (!InputManager.IsPressed)
                {
                    float bound = offsetBound;

                    if (offset != bound)
                        velocity = 0;

                    offset = offset * 0.8f + bound * 0.2f + velocity;
                    velocity *= 0.9f;
                }

                if (Director.PendingMode == OsuMode.Unknown)
                {
                    Vector2 pos = new Vector2(0, 60 + offset);
                    foreach (BeatmapPanel p in panels)
                    {
                        p.MoveTo(pos);
                        pos.Y += 70;
                    }
                }
            }
        }
    }
}