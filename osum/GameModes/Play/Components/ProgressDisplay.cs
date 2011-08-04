using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Sprites;
using osum.Graphics.Drawables;
using OpenTK.Graphics;
using OpenTK;

namespace osum.GameModes.Play.Components
{
    class ProgressDisplay : SpriteManager
    {
        const int HEIGHT = 5;
        pRectangle progressRect;
        pRectangle progressRectBg;
        public ProgressDisplay()
        {
            progressRectBg = new pRectangle(Vector2.Zero, new Vector2(GameBase.BaseSize.Width, HEIGHT), true, 1, new Color4(3, 50, 255, 100));
            progressRectBg.Field = FieldTypes.StandardSnapBottomLeft;
            progressRectBg.Origin = OriginTypes.BottomLeft;
            this.Add(progressRectBg);

            progressRect = new pRectangle(Vector2.Zero, new Vector2(0, HEIGHT), true, 1, new Color4(3, 205, 255, 80));
            progressRect.Field = FieldTypes.StandardSnapBottomLeft;
            progressRect.Origin = OriginTypes.BottomLeft;
            progressRect.Additive = true;
            this.Add(progressRect);
        }

        internal void SetProgress(float progress)
        {
            progressRect.Scale.X = GameBase.BaseSize.Width * progress;
        }
    }
}
