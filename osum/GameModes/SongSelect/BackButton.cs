using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Sprites;
using osum.Graphics.Drawables;
using OpenTK;
using OpenTK.Graphics;
using osum.Graphics.Skins;
using osum.Helpers;

namespace osum.GameModes.SongSelect
{
    class BackButton : pDrawable
    {
        pSprite background;
        pSprite arrow;

        SpriteManager sm = new SpriteManager();

        EventHandler Action;

        public BackButton(EventHandler action)
        {
            AlwaysDraw = true;
            Alpha = 1;
            DrawDepth = 1;
            Action = action;

            float offset = 30;

            background = new pSprite(TextureManager.Load(OsuTexture.songselect_back_hexagon), FieldTypes.StandardSnapBottomLeft, OriginTypes.Centre, ClockTypes.Mode, new Vector2(offset, offset), 0.99f, true, new Color4(200,200,200,255));
            background.OnClick += OnBackgroundOnClick;
            background.OnHover += delegate { background.FadeColour(new Color4(255, 255, 255, 255), 100); };
            background.OnHoverLost += delegate { background.FadeColour(new Color4(200, 200, 200, 255), 100); };

            arrow = new pSprite(TextureManager.Load(OsuTexture.songselect_back_arrow), FieldTypes.StandardSnapBottomLeft, OriginTypes.Centre, ClockTypes.Mode, new Vector2(offset + 15, offset + 15), 1, true, Color4.White);
            sm.Add(background);
            sm.Add(arrow);
        }

        void OnBackgroundOnClick(object sender, EventArgs e)
        {
            background.Transform(new TransformationBounce(Clock.ModeTime - 300, Clock.ModeTime + 700, 1, 1, 2));
            arrow.Transform(new TransformationBounce(Clock.ModeTime - 300, Clock.ModeTime + 700, 1, 1, 2));

            Action(sender, e);
        }

        public override void Dispose()
        {
            sm.Dispose();
            base.Dispose();
        }

        public override bool Draw()
        {
            if (!base.Draw())
                return false;

            sm.Draw();

            return true;
        }

        internal override bool IsOnScreen
        {
            get
            {
                return true;
            }
        }

        public override void Update()
        {
            base.Update();

            sm.Update();

            background.Rotation += (float)GameBase.ElapsedMilliseconds * 0.0005f;
        }
    }
}
