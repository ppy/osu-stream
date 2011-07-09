using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Sprites;
using osum.Helpers;
using osum.Graphics.Skins;
using OpenTK;
using OpenTK.Graphics;

namespace osum.UI
{
    class SliderControl : SpriteManager
    {
        private FloatDelegate action;
        private pSprite s_BackingPlate;

        public SliderControl(string text, float initialValue, Vector2 position, FloatDelegate onValueChanged)
        {
            action = onValueChanged;

            s_BackingPlate = new pSprite(TextureManager.Load(OsuTexture.sliderbar), position) { Colour = new Color4(50,50,50,255) };
            Add(s_BackingPlate);

            s_FrontPlate = new pSprite(TextureManager.Load(OsuTexture.sliderbar), position)
            {
                DrawDepth = s_BackingPlate.DrawDepth + 0.01f,
                Colour = new Color4(97,159,0,255),
                Additive = true
            };
            Add(s_FrontPlate);

            Vector2 offset = new Vector2(-s_BackingPlate.DisplayRectangle.Width / 2, -s_BackingPlate.DisplayRectangle.Height / 2);

            s_BackingPlate.Offset = offset;
            s_FrontPlate.Offset = offset;

            s_Text = new pText(text, 24, position, s_BackingPlate.DrawDepth + 0.02f, true, Color4.White)
            {
                Origin = OriginTypes.Centre,
                TextShadow = true
            };
            Add(s_Text);

            s_BackingPlate.OnClick += new EventHandler(SliderControl_OnClick);

            UpdateValue(initialValue);
        }

        bool wasClicked;
        private pText s_Text;
        private pSprite s_FrontPlate;

        void SliderControl_OnClick(object sender, EventArgs e)
        {
            wasClicked = true;
        }

        internal override void HandleInputManagerOnDown(InputSource source, TrackingPoint trackingPoint)
        {
            base.HandleInputManagerOnDown(source, trackingPoint);

            UpdatePosition(trackingPoint);
        }

        private void UpdatePosition(TrackingPoint trackingPoint)
        {
            if (wasClicked)
            {
                Box2 displayRect = s_BackingPlate.DisplayRectangle;
                float fill = pMathHelper.ClampToOne((trackingPoint.BasePosition.X - displayRect.Left) / displayRect.Width);
                UpdateValue(fill);
            }
        }

        private void UpdateValue(float value)
        {
            s_FrontPlate.DrawWidth = (int)(s_BackingPlate.TextureWidth * value);
            s_FrontPlate.FlashColour(new Color4(131,240,0,255), 150);
        }

        internal override void HandleOnMove(InputSource source, TrackingPoint trackingPoint)
        {
            UpdatePosition(trackingPoint);
            base.HandleOnMove(source, trackingPoint);
        }

        internal override void HandleOnUp(InputSource source, TrackingPoint trackingPoint)
        {
            wasClicked = false;

            base.HandleOnUp(source, trackingPoint);
        }
    }
}
