using System;
using OpenTK;
using OpenTK.Graphics;
using osum.Graphics;
using osum.Graphics.Sprites;
using osum.Helpers;
using osum.Input;
using osum.Input.Sources;

namespace osum.UI
{
    internal class SliderControl : SpriteManager
    {
        private readonly FloatDelegate action;
        private readonly pSprite BackingPlate;

        public SliderControl(string text, float initialValue, Vector2 position, FloatDelegate onValueChanged)
        {
            action = onValueChanged;

            BackingPlate = new pSprite(TextureManager.Load(OsuTexture.sliderbar), position) { Colour = new Color4(50,50,50,255) };
            Add(BackingPlate);

            FrontPlate = new pSprite(TextureManager.Load(OsuTexture.sliderbar), position)
            {
                DrawDepth = BackingPlate.DrawDepth + 0.01f,
                Colour = new Color4(97,159,0,255),
                Additive = true
            };
            Add(FrontPlate);

            Vector2 offset = new Vector2(-BackingPlate.DisplayRectangle.Width / 2, -BackingPlate.DisplayRectangle.Height / 2);

            BackingPlate.Offset = offset;
            FrontPlate.Offset = offset;

            Text = new pText(text, 24, position, BackingPlate.DrawDepth + 0.02f, true, Color4.White)
            {
                Origin = OriginTypes.Centre,
                TextShadow = true
            };
            Add(Text);

            BackingPlate.OnClick += SliderControl_OnClick;

            UpdateValue(initialValue);
        }

        private bool wasClicked;
        internal pText Text;
        private readonly pSprite FrontPlate;

        private void SliderControl_OnClick(object sender, EventArgs e)
        {
            wasClicked = true;
        }

        private TrackingPoint trackingPoint;

        internal override void HandleInputManagerOnDown(InputSource source, TrackingPoint trackingPoint)
        {
            if (wasClicked) return; //already tracking

            base.HandleInputManagerOnDown(source, trackingPoint);

            if (wasClicked)
                this.trackingPoint = trackingPoint.originalTrackingPoint;

            UpdatePosition(trackingPoint);
        }

        private void UpdatePosition(TrackingPoint trackingPoint)
        {
            if (trackingPoint.originalTrackingPoint == this.trackingPoint)
            {
                Box2 displayRect = BackingPlate.DisplayRectangle;
                float fill = pMathHelper.ClampToOne((trackingPoint.BasePosition.X - displayRect.Left) / displayRect.Width);
                UpdateValue(fill);
            }
        }

        public float Value;

        private void UpdateValue(float value)
        {
            Value = value;
            FrontPlate.DrawWidth = (int)(BackingPlate.TextureWidth * value);
            FrontPlate.FlashColour(new Color4(131,240,0,255), 150);
            action(value);
        }

        internal override void HandleOnMove(InputSource source, TrackingPoint trackingPoint)
        {
            UpdatePosition(trackingPoint);
            base.HandleOnMove(source, trackingPoint);
        }

        internal override void HandleOnUp(InputSource source, TrackingPoint trackingPoint)
        {
            if (trackingPoint.originalTrackingPoint == this.trackingPoint)
            {
                wasClicked = false;
                this.trackingPoint = null;
            }

            base.HandleOnUp(source, trackingPoint);
        }
    }
}
