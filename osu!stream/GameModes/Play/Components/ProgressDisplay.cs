using OpenTK;
using OpenTK.Graphics;
using osum.GameplayElements;
using osum.Graphics.Sprites;

namespace osum.GameModes.Play.Components
{
    public class ProgressDisplay : SpriteManager
    {
        private const int HEIGHT = 6;
        private pRectangle progressRect;
        private readonly pRectangle progressRectBg;

        public ProgressDisplay()
        {
            progressRectBg = new pRectangle(Vector2.Zero, new Vector2(GameBase.BaseSize.X, HEIGHT), true, 0.99f, Color4.Black);
            progressRectBg.Field = FieldTypes.StandardSnapBottomLeft;
            progressRectBg.Origin = OriginTypes.BottomLeft;
            Add(progressRectBg);

            progressRect = new pRectangle(Vector2.Zero, new Vector2(0, HEIGHT - 1), true, 1, gray_colour);
            progressRect.Field = FieldTypes.StandardSnapBottomLeft;
            progressRect.Origin = OriginTypes.BottomLeft;
            progressRect.Additive = true;
            Add(progressRect);
        }

        private ScoreChange lastDisplayedChange;
        private float lastProgressStart;

        private readonly Color4 gray_colour = new Color4(80, 80, 80, 255);

        internal void ExtendHeight(int duration, float extent)
        {
            Sprites.ForEach(s =>
            {
                Transformation t = new TransformationV(TransformationType.VectorScale, s.Scale, new Vector2(s.Scale.X, s == progressRectBg ? extent : extent - 1),
                    s.ClockingNow, s.ClockingNow + duration, EasingTypes.In);
                s.Transform(t);
            });
        }

        internal void SetProgress(float progress, ScoreChange latestChange)
        {
            progressRect.Scale.X = GameBase.BaseSize.X * (progress - lastProgressStart);

            if (latestChange != lastDisplayedChange)
            {
                lastDisplayedChange = latestChange;

                Color4 displayColour = gray_colour;

                float heightMultiplier = 1;

                switch (lastDisplayedChange)
                {
                    case ScoreChange.Hit300:
                        displayColour = new Color4(255, 156, 55, 255);
                        break;
                    case ScoreChange.Hit100:
                        displayColour = new Color4(117, 204, 65, 255);
                        heightMultiplier = 0.9f;
                        break;
                    case ScoreChange.Hit50:
                        displayColour = new Color4(118, 65, 143, 255);
                        heightMultiplier = 0.8f;
                        break;
                    case ScoreChange.Miss:
                        displayColour = new Color4(144, 0, 16, 255);
                        heightMultiplier = 0.7f;
                        break;
                }

                progressRect.FlashColour(Color4.White, 1000);
                //progressRect.Transform(new TransformationV(TransformationType.VectorScale, new Vector2(progressRect.Scale.X, progressRect.Scale.Y * 2), progressRect.Scale, Clock.ModeTime, Clock.ModeTime + 1000, EasingTypes.In));

                progressRect = new pRectangle(new Vector2(progressRect.Scale.X + progressRect.Position.X, 0), new Vector2(0, HEIGHT * heightMultiplier - 1), true, 1, displayColour);
                progressRect.Field = FieldTypes.StandardSnapBottomLeft;
                progressRect.Origin = OriginTypes.BottomLeft;
                Add(progressRect);

                lastProgressStart = progress;
            }
        }
    }
}