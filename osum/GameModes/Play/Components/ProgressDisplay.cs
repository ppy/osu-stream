using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Sprites;
using osum.Graphics.Drawables;
using OpenTK.Graphics;
using OpenTK;
using osum.GameplayElements;
using osum.Helpers;

namespace osum.GameModes.Play.Components
{
    public class ProgressDisplay : SpriteManager
    {
        const int HEIGHT = 5;
        pRectangle progressRect;
        pRectangle progressRectBg;
        public ProgressDisplay()
        {
            progressRectBg = new pRectangle(Vector2.Zero, new Vector2(GameBase.BaseSize.Width, HEIGHT), true, 0.99f, Color4.Black);
            progressRectBg.Field = FieldTypes.StandardSnapBottomLeft;
            progressRectBg.Origin = OriginTypes.BottomLeft;
            Add(progressRectBg);

            progressRect = new pRectangle(Vector2.Zero, new Vector2(0, HEIGHT - 1), true, 1, Color4.Gray);
            progressRect.Field = FieldTypes.StandardSnapBottomLeft;
            progressRect.Origin = OriginTypes.BottomLeft;
            progressRect.Additive = true;
            Add(progressRect);
        }

        ScoreChange lastDisplayedChange;
        float lastProgressStart;

        internal void SetProgress(float progress, ScoreChange latestChange)
        {
            progressRect.Scale.X = GameBase.BaseSize.Width * (progress - lastProgressStart);

            if (latestChange != lastDisplayedChange)
            {
                lastDisplayedChange = latestChange;

                Color4 displayColour;

                switch (lastDisplayedChange)
                {
                    case ScoreChange.Hit300:
                        displayColour = new Color4(255, 156, 55, 255);
                        break;
                    case ScoreChange.Hit100:
                        displayColour = new Color4(117, 204, 65, 255);
                        break;
                    case ScoreChange.Hit50:
                        displayColour = new Color4(118, 65, 143, 255);
                        break;
                    default:
                    case ScoreChange.Miss:
                        displayColour = new Color4(144, 0, 16, 255);
                        break;
                }

                progressRect.FlashColour(Color4.White, 1000);
                progressRect.Transform(new TransformationV(TransformationType.VectorScale, new Vector2(progressRect.Scale.X, progressRect.Scale.Y * 2), progressRect.Scale, Clock.ModeTime, Clock.ModeTime + 1000, EasingTypes.In));

                progressRect = new pRectangle(new Vector2(progressRect.Scale.X + progressRect.Position.X, 0), new Vector2(0, HEIGHT - 1), true, 1, displayColour);
                progressRect.Field = FieldTypes.StandardSnapBottomLeft;
                progressRect.Origin = OriginTypes.BottomLeft;
                Add(progressRect);

                lastProgressStart = progress;
            }
        }
    }
}
