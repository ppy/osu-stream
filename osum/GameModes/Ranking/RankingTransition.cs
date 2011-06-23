using System;
using osum.GameplayElements.Scoring;
using osum.Graphics.Sprites;
using osum.Graphics.Skins;
using osum.Helpers;
using OpenTK;
using OpenTK.Graphics;
using System.Collections;
using System.Collections.Generic;
using osum.Audio;
using osum.GameModes.SongSelect;
using osum.Graphics;
using osum.Online;
using osu_common.Helpers;
using osum.GameplayElements;
using System.IO;
namespace osum.GameModes
{
    public class RankingTransition : Transition
    {
        float count_height = 80;
        const float fill_height = 5;
        const int end_bouncing = 600;
        const int colour_change_length = 500;
        const int time_between_fills = 300;

        List<pDrawable> fillSprites = new List<pDrawable>();
        List<pDrawable> countSprites = new List<pDrawable>();

        public override bool FadeOutDone
        {
            get
            {
                return Clock.Time - startTime > 3000;
            }
        }

        public override bool FadeInDone
        {
            get
            {
                return spriteManager.Transformations.Count == 0;
            }
        }

        int startTime;
        private pDrawable flash;

        pSprite background;

        public override void Initialize()
        {
            startTime = Clock.Time;

            background = new pSprite(TextureManager.Load(OsuTexture.cleared), FieldTypes.StandardSnapCentre, OriginTypes.CentreLeft,
                            ClockTypes.Mode, Vector2.Zero, 1, true, Color4.White);
            background.Position.X -= background.DrawWidth * GameBase.SpriteToBaseRatio / 2;

            background.Additive = true;
            spriteManager.Add(background);

            pDrawable fill = pSprite.FullscreenWhitePixel;
            fill.AlignToSprites = true;
            fill.Clocking = ClockTypes.Game;
            fill.Scale.X *= (float)Ranking.RankableScore.count300 / Ranking.RankableScore.totalHits;
            fill.Colour = new Color4(1, 0.63f, 0.01f, 1);
            fillSprites.Add(fill);

            pSpriteText count = new pSpriteText(Ranking.RankableScore.count300.ToString(), "default", 0, FieldTypes.Standard, OriginTypes.BottomRight, ClockTypes.Game, new Vector2(0, count_height), 1, false, Color4.White);
            countSprites.Add(count);

            count_height += 80;

            fill = pSprite.FullscreenWhitePixel;
            fill.AlignToSprites = true;
            fill.Clocking = ClockTypes.Game;
            fill.Scale.X *= (float)Ranking.RankableScore.count100 / Ranking.RankableScore.totalHits;
            fill.Colour = new Color4(0.55f, 0.84f, 0, 1);
            fillSprites.Add(fill);

            count = new pSpriteText(Ranking.RankableScore.count100.ToString(), "default", 0, FieldTypes.Standard, OriginTypes.BottomRight, ClockTypes.Game, new Vector2(0, count_height), 1, false, Color4.White);
            countSprites.Add(count);

            count_height += 80;

            fill = pSprite.FullscreenWhitePixel;
            fill.AlignToSprites = true;
            fill.Clocking = ClockTypes.Game;
            fill.Scale.X *= (float)Ranking.RankableScore.count50 / Ranking.RankableScore.totalHits;
            fill.Colour = new Color4(0.50f, 0.29f, 0.635f, 1);
            fillSprites.Add(fill);

            count = new pSpriteText(Ranking.RankableScore.count50.ToString(), "default", 0, FieldTypes.Standard, OriginTypes.BottomRight, ClockTypes.Game, new Vector2(0, count_height), 1, false, Color4.White);
            countSprites.Add(count);

            count_height += 80;

            fill = pSprite.FullscreenWhitePixel;
            fill.AlignToSprites = true;
            fill.Clocking = ClockTypes.Game;
            fill.Scale.X *= (float)Ranking.RankableScore.countMiss / Ranking.RankableScore.totalHits;
            fill.Colour = new Color4(0.10f, 0.10f, 0.10f, 1);
            fillSprites.Add(fill);

            count = new pSpriteText(Ranking.RankableScore.countMiss.ToString(), "default", 0, FieldTypes.Standard, OriginTypes.BottomRight, ClockTypes.Game, new Vector2(0, count_height), 1, false, Color4.White);
            countSprites.Add(count);

            int i = 0;

            foreach (pDrawable p in countSprites)
            {
                p.Alpha = 0;
                p.AlwaysDraw = true;
                p.Additive = true;

                int offset = Clock.Time + i++ * time_between_fills;
                //p.Transform(new Transformation(TransformationType.Fade, 0, 0.5f, offset - 50, offset + 200));
                //p.Transform(new Transformation(TransformationType.Fade, 1, 0, offset + 100, offset + 800));
            }


            i = 0;

            foreach (pDrawable p in fillSprites)
            {
                p.Alpha = 1;
                //p.Additive = true;
                p.DrawDepth = 0.98f;
                p.AlwaysDraw = true;

                int offset = Clock.Time + i++ * time_between_fills;

                p.Transform(new Transformation(new Color4(23, 51, 71, 255), new Color4(23, 51, 71, 255), Clock.Time, Clock.Time + 1400));
                p.Transform(new Transformation(Color4.White, p.Colour, Clock.Time + 1400, Clock.Time + 3000));
                //force the initial colour to be an ambiguous gray.

                p.Transform(new TransformationBounce(offset, offset + end_bouncing * 2, p.Scale.X, p.Scale.X, 5));
            }

            spriteManager.Add(fillSprites);
            spriteManager.Add(countSprites);

            base.Initialize();
        }

        internal override void FadeIn()
        {
            //spriteManager.Sprites.ForEach(s =>
            //{
            //    s.AlwaysDraw = false;
            //});

            //background.FadeOut(100);
            //fillSprites.ForEach(s => s.FadeOut(800));

            spriteManager.MoveTo(new Vector2(0, -GameBase.BaseSize.Height), 1000, EasingTypes.InOut);

            /*flash = pSprite.FullscreenWhitePixel;
            flash.Clocking = ClockTypes.Game;
            flash.FadeOutFromOne(800);
            spriteManager.Add(flash);*/
            base.FadeIn();
        }

        public override void Update()
        {
            base.Update();

            //set the x scale back to the default value (override the bounce transformation).
            float lastPos = 0;

            for (int i = 0; i < fillSprites.Count; i++)
            {
                pDrawable fill = fillSprites[i];
                pDrawable count = countSprites[i];

                fill.Scale.Y = GameBase.BaseSize.Height;

                if (lastPos != 0) fill.Position.X = lastPos;
                lastPos = fill.Position.X + fill.Scale.X;

                count.Position.X = lastPos - 3;
            }

            float widthOffset = -background.FieldPosition.X / GameBase.BaseToNativeRatio / GameBase.SpriteToBaseRatio;
            background.DrawWidth = (int)(widthOffset + (background.Texture.Width - widthOffset * 2) * (lastPos / GameBase.BaseSize.Width));
        }
    }
}

