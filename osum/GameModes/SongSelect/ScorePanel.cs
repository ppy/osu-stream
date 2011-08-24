using System;
using osum.Graphics.Sprites;
using osum.GameplayElements.Beatmaps;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using osum.Helpers;
using System.Text.RegularExpressions;
using osum.Graphics.Drawables;
using osum.Graphics.Renderers;
using osum.Graphics.Skins;
using osum.Audio;
using osum.Graphics;
using osum.GameplayElements;
using System.Collections.Generic;
using osum.GameplayElements.Scoring;
namespace osum.GameModes.SongSelect
{
    internal class ScorePanel : pSpriteCollection
    {
        internal pDrawable s_BackingPlate;
        internal pText s_Text;
        internal pText s_TextArtist;

        float base_depth = 0.4f;

        static Color4 colourNormal = new Color4(50, 50, 50, 255);
        static Color4 colourHover = new Color4(28, 139, 242, 255);

        internal const int PANEL_HEIGHT = 34;
        public static Color4 BACKGROUND_COLOUR = new Color4(255, 255, 255, 240);

        public Score Score;

        internal ScorePanel(Score score, EventHandler action, int rank)
        {
            Score = score;

            base_depth += 0.0001f * rank;

            s_BackingPlate = new pRectangle(Vector2.Zero, new Vector2(GameBase.BaseSize.Width,PANEL_HEIGHT), true, base_depth, new Color4(0,0,0,40))
            {
                Tag = this
            };

            Sprites.Add(s_BackingPlate);

            if (action != null)
                s_BackingPlate.OnClick += action;

            s_BackingPlate.HandleClickOnUp = true;

            pSpriteText rankNumber = new pSpriteText(rank.ToString(), "score", 0, FieldTypes.Standard, OriginTypes.CentreRight, ClockTypes.Mode, Vector2.Zero, 0.9f, true, new Color4(100, 100, 100, 255));
            rankNumber.Offset = new Vector2(20, PANEL_HEIGHT / 2);
            rankNumber.TextConstantSpacing = true;
            rankNumber.SpacingOverlap = 8;
            rankNumber.ZeroAlpha = 0.5f;
            rankNumber.ScaleScalar = 0.5f;
            Sprites.Add(rankNumber);

            pSprite rankingSprite = new pSprite(score.RankingTextureSmall, Vector2.Zero)
            {
                Origin = OriginTypes.CentreLeft,
                DrawDepth = base_depth + 0.06f,
                Offset = new Vector2(23, PANEL_HEIGHT / 2),
                ScaleScalar = 0.8f
            };
            Sprites.Add(rankingSprite);

            if (score.Username != @"Guest")
            {
                pSpriteWeb avatar = new pSpriteWeb(@"http://api.twitter.com/1/users/profile_image/" + score.Username)
                {
                    Offset = new Vector2(80, PANEL_HEIGHT / 2),
                    Origin = OriginTypes.Centre
                };
                Sprites.Add(avatar);
            }


            s_Text = new pText(score.Username, 26, Vector2.Zero, Vector2.Zero, 0.5f, true, Color4.White, false);
            s_Text.Bold = true;
            s_Text.Offset = new Vector2(100, 0);
            Sprites.Add(s_Text);

            pSpriteText scoreText = new pSpriteText("000000", "score", 0, FieldTypes.StandardSnapRight, OriginTypes.CentreRight, ClockTypes.Mode, Vector2.Zero, 0.9f, true, new Color4(255, 166, 0, 255));
            scoreText.Offset = new Vector2(200, PANEL_HEIGHT / 2);
            scoreText.ShowInt(score.totalScore, 6, true);
            scoreText.TextConstantSpacing = true;
            scoreText.ZeroAlpha = 0.5f;
            scoreText.ScaleScalar = 0.8f;
            Sprites.Add(scoreText);

            pSpriteText accuracy = new pSpriteText((score.accuracy * 100).ToString("00.00", GameBase.nfi) + "%", "score", 0, FieldTypes.StandardSnapRight, OriginTypes.CentreRight, ClockTypes.Mode, Vector2.Zero, 0.9f, true, new Color4(0, 180, 227, 255));
            accuracy.Offset = new Vector2(20, PANEL_HEIGHT / 2);
            accuracy.ScaleScalar = 0.8f;
            accuracy.TextConstantSpacing = true;
            Sprites.Add(accuracy);
        }
    }
}

