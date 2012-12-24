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
using osu_common.Helpers;
using osum.GameplayElements;
using System.IO;
namespace osum.GameModes
{
    public class ResultTransition : Transition
    {
        const float fill_height = 5;
        const int end_bouncing = 600;
        const int colour_change_length = 500;
        const int time_between_fills = 300;

        List<pDrawable> fillSprites = new List<pDrawable>();

        public override bool SkipScreenClear
        {
            get
            {
                return true;
            }
        }

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
            fill.Clocking = ClockTypes.Game;
            fill.Scale.X *= (float)Results.RankableScore.count300 / Results.RankableScore.totalHits + 0.001f;
            fill.Colour = new Color4(1, 0.63f, 0.01f, 1);
            fillSprites.Add(fill);

            fill = pSprite.FullscreenWhitePixel;
            fill.Clocking = ClockTypes.Game;
            fill.Scale.X *= (float)Results.RankableScore.count100 / Results.RankableScore.totalHits + 0.001f;
            fill.Colour = new Color4(0.55f, 0.84f, 0, 1);
            fillSprites.Add(fill);

            fill = pSprite.FullscreenWhitePixel;
            fill.Clocking = ClockTypes.Game;
            fill.Scale.X *= (float)Results.RankableScore.count50 / Results.RankableScore.totalHits + 0.001f;
            fill.Colour = new Color4(0.50f, 0.29f, 0.635f, 1);
            fillSprites.Add(fill);

            fill = pSprite.FullscreenWhitePixel;
            fill.Clocking = ClockTypes.Game;
            fill.Scale.X *= (float)Results.RankableScore.countMiss / Results.RankableScore.totalHits + 0.001f;
            fill.Colour = new Color4(0.10f, 0.10f, 0.10f, 1);
            fillSprites.Add(fill);

            int i = 0;

            foreach (pDrawable p in fillSprites)
            {
                p.Alpha = 1;
                //p.Additive = true;
                p.DrawDepth = 0.98f;
                p.AlwaysDraw = true;

                int offset = Clock.Time + i++ * time_between_fills;

                p.Transform(new TransformationC(new Color4(23, 51, 71, 255), new Color4(23, 51, 71, 255), Clock.Time, Clock.Time + 1400));
                p.Transform(new TransformationC(Color4.White, p.Colour, Clock.Time + 1400, Clock.Time + 3000));
                //force the initial colour to be an ambiguous gray.

                p.Transform(new TransformationBounce(offset, offset + end_bouncing * 2, p.Scale.X, p.Scale.X, 5));
            }

            GameBase.Scheduler.Add(delegate { AudioEngine.PlaySample(OsuSamples.RankBling); }, 1400);

            spriteManager.Add(fillSprites);

            base.Initialize();
        }

        internal override void FadeIn()
        {
            spriteManager.MoveTo(new Vector2(0, -GameBase.BaseSize.Y), 1300, EasingTypes.InOut);
            AudioEngine.PlaySample(OsuSamples.RankWhoosh);
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

                fill.Scale.Y = GameBase.BaseSizeFixedWidth.Y + 1;

                if (lastPos != 0) fill.Position.X = lastPos;
                lastPos = fill.Position.X + fill.Scale.X;

                fill.UpdateFieldPosition();
                fill.UpdateFieldScale();
            }

            float widthOffset = -background.FieldPosition.X / GameBase.BaseToNativeRatio / GameBase.SpriteToBaseRatio;
            background.DrawWidth = (int)(widthOffset + (background.Texture.Width - widthOffset * 2) * (lastPos / GameBase.BaseSize.X));
        }
    }
}

