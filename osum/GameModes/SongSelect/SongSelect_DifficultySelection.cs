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
    public partial class SongSelectMode : GameMode
    {
        private pSpriteCollection spritesDifficultySelection = new pSpriteCollection();

        private pButton s_ButtonEasy;
        private pButton s_ButtonStandard;
        private pButton s_ButtonExpert;
        private pDrawable s_ButtonExpertUnlock;

        private pRectangle s_DifficultySelectionRectangle;
        private pSprite s_TabBarBackground;

        private void showDifficultySelection()
        {
            if (s_ButtonEasy == null)
            {
                Vector2 border = new Vector2(4, 4);

                int ypos = 140;
                float spacing = border.X;

                Vector2 buttonSize = new Vector2((GameBase.BaseSize.Width - spacing * 4) / 3f, 100);

                float currX = spacing;

                s_ButtonEasy = new pButton("Easy", new Vector2(currX, ypos), buttonSize, PlayfieldBackground.COLOUR_EASY, onDifficultyButtonPressed);
                s_ButtonEasy.Sprites.Add(new pText("- For Beginners\n- Locked to Easy stream\n- No Fail", 13, new Vector2(currX, ypos + 40), buttonSize, 0.55f, true, Color4.White, false));
                spritesDifficultySelection.Add(s_ButtonEasy);

                currX += buttonSize.X + spacing;

                s_ButtonStandard = new pButton("Standard", new Vector2(currX, ypos), buttonSize, PlayfieldBackground.COLOUR_STANDARD, onDifficultyButtonPressed);
                s_ButtonStandard.Sprites.Add(new pText("- Standard gameplay\n- Three streams\n- Can Fail", 13, new Vector2(currX, ypos + 40), buttonSize, 0.55f, true, Color4.White, false));
                spritesDifficultySelection.Add(s_ButtonStandard);

                s_DifficultySelectionRectangle = new pRectangle(new Vector2(0, ypos - border.Y), new Vector2(GameBase.BaseSize.Width, buttonSize.Y + border.Y * 2), true, 0.3f, Color4.Gray);
                spritesDifficultySelection.Add(s_DifficultySelectionRectangle);

                s_DifficultySelectionRectangle = new pRectangle(new Vector2(currX, ypos), buttonSize + border * 2, true, 0.4f, Color4.LightGray) { Offset = -border };
                spritesDifficultySelection.Add(s_DifficultySelectionRectangle);

                currX += buttonSize.X + spacing;

                s_ButtonExpert = new pButton("Expert", new Vector2(currX, ypos), buttonSize, PlayfieldBackground.COLOUR_WARNING, onDifficultyButtonPressed);
                s_ButtonExpertUnlock = new pText("Unlock by passing on standard play first!", 13, new Vector2(currX, ypos + 40), buttonSize, 0.55f, true, Color4.LightGray, false);
                s_ButtonExpert.Sprites.Add(s_ButtonExpertUnlock);
                spritesDifficultySelection.Add(s_ButtonExpert);

                currX += buttonSize.X + spacing;

                s_TabBarBackground = new pSprite(TextureManager.Load(OsuTexture.songselect_tab_bar), FieldTypes.StandardSnapTopCentre, OriginTypes.TopCentre, ClockTypes.Mode, new Vector2(0, -100), 0.4f, true, Color4.White);
                spritesDifficultySelection.Add(s_TabBarBackground);

                spriteManager.Add(spritesDifficultySelection);
                spritesDifficultySelection.Sprites.ForEach(s => s.Alpha = 0);
            }

            //preview has finished loading.
            State = SelectState.DifficultySelect;

            foreach (pDrawable s in SelectedPanel.Sprites)
                s.MoveTo(new Vector2(0, 0), 500, EasingTypes.InDouble);

            s_TabBarBackground.Transform(new Transformation(new Vector2(0, -100), new Vector2(0, -100), Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
            s_TabBarBackground.Transform(new Transformation(new Vector2(0, 0), new Vector2(0, BeatmapPanel.PANEL_HEIGHT), Clock.ModeTime + 400, Clock.ModeTime + 1000, EasingTypes.In));

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

            s_Header.Transform(new Transformation(Vector2.Zero, new Vector2(0, -59), Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
            s_Header.Transform(new Transformation(TransformationType.Rotation, s_Header.Rotation, 0.03f, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));

            s_Footer.Transform(new Transformation(new Vector2(-60, -105), Vector2.Zero, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
            s_Footer.Transform(new Transformation(TransformationType.Rotation, 0.04f, 0, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
        }

        private void leaveDifficultySelection(object sender, EventArgs args)
        {
            State = SelectState.SongSelect;

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

                s_Header.Transform(new Transformation(s_Header.Position, Vector2.Zero, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
                s_Header.Transform(new Transformation(TransformationType.Rotation, s_Header.Rotation, 0, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));

                s_Footer.Transform(new Transformation(s_Footer.Position, new Vector2(-60, -105), Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
                s_Footer.Transform(new Transformation(TransformationType.Rotation, 0, 0.04f, Clock.ModeTime, Clock.ModeTime + 500, EasingTypes.In));
            }, true);
        }

        private void onDifficultyButtonPressed(object sender, EventArgs args)
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

        private void onStartButtonPressed(object sender, EventArgs args)
        {
            if (State == SelectState.Starting)
                return;

            State = SelectState.Starting;

            if (sender != s_ButtonEasy) s_ButtonEasy.Sprites.ForEach(s => s.FadeOut(200));
            if (sender != s_ButtonStandard) s_ButtonStandard.Sprites.ForEach(s => s.FadeOut(200));
            if (sender != s_ButtonExpert) s_ButtonExpert.Sprites.ForEach(s => s.FadeOut(200));

            GameBase.Scheduler.Add(delegate
            {
                Director.ChangeMode(OsuMode.Play);
            }, 900);
        }
    }
}
