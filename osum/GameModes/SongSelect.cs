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

        internal override void Initialize()
        {
            InitializeBeatmaps();

            Player.SetDifficulty(Difficulty.Normal);

            InputManager.OnMove += InputManager_OnMove;


            //Start playing song select BGM.
#if iOS
            AudioEngine.Music.Load(File.ReadAllBytes("Skins/Default/select.m4a"), true);
#else
            AudioEngine.Music.Load(File.ReadAllBytes("Skins/Default/select.mp3"), true);
#endif
            AudioEngine.Music.Play();

            s_Header = new pSprite(TextureManager.Load(OsuTexture.songselect_header), new Vector2(0, 0));
            spriteManager.Add(s_Header);

            InitializePostSelectionOptions();
        }

        private pSpriteCollection spritesDifficultySelection = new pSpriteCollection();

        private pButton s_ButtonEasy;
        private pButton s_ButtonStandard;
        private pButton s_ButtonExpert;
        private pButton s_ButtonStart;
        private pDrawable s_ButtonExpertUnlock;

        private pRectangle s_DifficultySelectionRectangle;

        private void InitializePostSelectionOptions()
        {
            const int ypos = 140;
            const int spacing = 20;

            Vector2 buttonSize = new Vector2(185, 100);

            float currX = spacing;

            s_ButtonEasy = new pButton("Easy", new Vector2(currX, ypos), buttonSize, PlayfieldBackground.COLOUR_EASY, difficultySelected);
            
            s_ButtonEasy.Sprites.Add(new pText("Recommended for beginners! You can't fail!", 13, new Vector2(currX, ypos + 40), buttonSize, 0.55f, true, Color4.White, false));

            spritesDifficultySelection.Add(s_ButtonEasy);

            currX += buttonSize.X + spacing;

            s_ButtonStandard = new pButton("Standard", new Vector2(currX, ypos), buttonSize, PlayfieldBackground.COLOUR_STANDARD, difficultySelected);

            s_ButtonStandard.Sprites.Add(new pText("Stream-based challenge!", 13, new Vector2(currX, ypos + 40), buttonSize, 0.55f, true, Color4.White, false));

            spritesDifficultySelection.Add(s_ButtonStandard);

            Vector2 border = new Vector2(4, 4);

            s_DifficultySelectionRectangle = new pRectangle(new Vector2(currX, ypos), buttonSize + border * 2, true, 0.4f, Color4.OrangeRed) { Offset = -border };
            spritesDifficultySelection.Add(s_DifficultySelectionRectangle);

            currX += buttonSize.X + spacing;

            s_ButtonExpert = new pButton("Expert", new Vector2(currX, ypos), buttonSize, PlayfieldBackground.COLOUR_WARNING, difficultySelected);

            s_ButtonExpertUnlock = new pText("Unlock by passing on standard play first!", 13, new Vector2(currX, ypos + 40), buttonSize, 0.55f, true, Color4.White, false);
            s_ButtonExpert.Sprites.Add(s_ButtonExpertUnlock);

            spritesDifficultySelection.Add(s_ButtonExpert);

            currX += buttonSize.X + spacing;

            s_ButtonStart = new pButton("Start!", new Vector2(GameBase.BaseSizeHalf.Width * 0.5f, ypos + 120), new Vector2(GameBase.BaseSizeHalf.Width, 40), Color4.MistyRose, gameStart);
            s_ButtonStart.s_Text.Offset = new Vector2(0, 8);
            spritesDifficultySelection.Add(s_ButtonStart);

            spriteManager.Add(spritesDifficultySelection);
            spritesDifficultySelection.Sprites.ForEach(s => s.Alpha = 0);
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

            Player.SetBeatmap(panel.Beatmap);

            hasSelected = true;

            foreach (BeatmapPanel p in panels)
            {
                if (p == panel)
                {
                    panel.s_BackingPlate.UnbindAllEvents();
                    panel.s_BackingPlate.FlashColour(Color4.White, 600);

                    foreach (pSprite s in p.Sprites)
                    {
                        s.MoveTo(new Vector2(0, 60), 500, EasingTypes.InDouble);
                    }
                }
                else
                {
                    foreach (pSprite s in p.Sprites)
                        s.FadeOut(100);
                }
            }

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

            AudioEngine.Music.Unload();

            InputManager.OnMove -= InputManager_OnMove;
        }

        private void InitializeBeatmaps()
        {
            availableMaps = new List<Beatmap>();

#if iOS
            string docs = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            
            foreach (string s in Directory.GetFiles(docs,"*.osc"))
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

                    string[] files = reader.Package == null ? Directory.GetFiles(s, "*.osc") : reader.Package.MapFiles;
                    foreach (string file in files)
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