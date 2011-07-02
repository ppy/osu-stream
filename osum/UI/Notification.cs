using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.GameModes;
using osum.Graphics.Sprites;
using osum.Graphics.Skins;
using osum.Helpers;
using OpenTK;
using OpenTK.Graphics;
using osum.Graphics.Renderers;

namespace osum.UI
{
    public class Notification : GameComponent
    {
        public bool Dismissed;
        private BoolDelegate Action;

        pSprite okayButton;
        pSprite cancelButton;

        public override void Dispose()
        {
            Action = null;
            base.Dispose();
        }

        public Notification(string title, string description, NotificationStyle style, BoolDelegate action = null)
        {
            pSprite back = new pSprite(TextureManager.Load(OsuTexture.notification_background), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Game, Vector2.Zero, 0.98f, true, Color4.White)
            {
                DimImmune = true,
            };

            pText titleText = new pText(title, 36, new Vector2(0, -130), new Vector2(GameBase.BaseSizeFixedWidth.Width * 0.8f, 0), 1, true, Color4.White, true)
            {
                Field = FieldTypes.StandardSnapCentre,
                Origin = OriginTypes.Centre,
                TextAlignment = TextAlignment.Centre,
                Clocking = ClockTypes.Game,
                DimImmune = true
            };

            pText descText = new pText(description, 24, new Vector2(0, -80), new Vector2(GameBase.BaseSizeFixedWidth.Width * 0.8f, 0), 1, true, Color4.White, false)
            {
                Field = FieldTypes.StandardSnapCentre,
                Origin = OriginTypes.TopCentre,
                TextAlignment = TextAlignment.Centre,
                Clocking = ClockTypes.Game,
                DimImmune = true
            };

            spriteManager.Alpha = 0;

            Action = action;

            const int button_height = 95;

            switch (style)
            {
                case NotificationStyle.Okay:
                    {
                        pDrawable additiveButton = null;

                        okayButton = new pSprite(TextureManager.Load(OsuTexture.notification_button_ok), new Vector2(0, button_height))
                        {
                            Field = FieldTypes.StandardSnapCentre,
                            Origin = OriginTypes.Centre,
                            Clocking = ClockTypes.Game,
                            DimImmune = true,
                            DrawDepth = 0.99f,
                            HandleClickOnUp = true
                        };
                        okayButton.OnHover += delegate
                        {
                            additiveButton = okayButton.AdditiveFlash(10000, 0.4f);
                        };

                        okayButton.OnHoverLost += delegate
                        {
                            if (additiveButton != null) additiveButton.FadeOut(100);
                        };

                        okayButton.OnClick += delegate
                        {
                            if (Action != null) Action(true);
                            dismiss();
                        };

                        spriteManager.Add(okayButton);

                        pText okayText = new pText(osum.Resources.General.Okay, 24, new Vector2(0, button_height), Vector2.Zero, 1, true, Color4.White, true)
                        {
                            Field = FieldTypes.StandardSnapCentre,
                            Origin = OriginTypes.Centre,
                            Clocking = ClockTypes.Game,
                            DimImmune = true
                        };

                        spriteManager.Add(okayText);
                    }
                    break;
                case NotificationStyle.YesNo:
                    {
                        pDrawable additiveButton = null;

                        okayButton = new pSprite(TextureManager.Load(OsuTexture.notification_button_yes), new Vector2(-140, button_height))
                        {
                            Field = FieldTypes.StandardSnapCentre,
                            Origin = OriginTypes.Centre,
                            Clocking = ClockTypes.Game,
                            DimImmune = true,
                            DrawDepth = 0.99f,
                            HandleClickOnUp = true
                        };

                        okayButton.OnHover += delegate
                        {
                            additiveButton = okayButton.AdditiveFlash(10000, 0.4f);
                        };

                        okayButton.OnHoverLost += delegate
                        {
                            if (additiveButton != null) additiveButton.FadeOut(100);
                        };

                        okayButton.OnClick += delegate
                        {
                            if (Action != null) Action(true);
                            dismiss();
                        };

                        spriteManager.Add(okayButton);

                        pText okayText = new pText(osum.Resources.General.Yes, 24, new Vector2(-140, button_height), Vector2.Zero, 1, true, Color4.White, true)
                        {
                            Field = FieldTypes.StandardSnapCentre,
                            Origin = OriginTypes.Centre,
                            Clocking = ClockTypes.Game,
                            DimImmune = true
                        };

                        spriteManager.Add(okayText);
                    }
                    {
                        pDrawable additiveButton = null;

                        cancelButton = new pSprite(TextureManager.Load(OsuTexture.notification_button_no), new Vector2(140, button_height))
                        {
                            Field = FieldTypes.StandardSnapCentre,
                            Origin = OriginTypes.Centre,
                            Clocking = ClockTypes.Game,
                            DimImmune = true,
                            DrawDepth = 0.99f,
                            HandleClickOnUp = true

                        };
                        cancelButton.OnHover += delegate
                        {
                            additiveButton = cancelButton.AdditiveFlash(10000, 0.4f);
                        };

                        cancelButton.OnHoverLost += delegate
                        {
                            if (additiveButton != null) additiveButton.FadeOut(100);
                        };

                        cancelButton.OnClick += delegate
                        {
                            if (Action != null) Action(false);
                            dismiss();
                        };

                        spriteManager.Add(cancelButton);

                        pText cancelText = new pText(osum.Resources.General.No, 24, new Vector2(140, button_height), Vector2.Zero, 1, true, Color4.White, true)
                        {
                            Field = FieldTypes.StandardSnapCentre,
                            Origin = OriginTypes.Centre,
                            Clocking = ClockTypes.Game,
                            DimImmune = true
                        };

                        spriteManager.Add(cancelText);
                    }
                    break;
            }

            spriteManager.Add(back);
            spriteManager.Add(descText);
            spriteManager.Add(titleText);
        }

        private void dismiss()
        {
            spriteManager.FadeOut(250);
            spriteManager.ScaleTo(0.95f, 250, EasingTypes.Out);
            spriteManager.RotateTo(0.05f, 250, EasingTypes.Out);
            Dismissed = true;
        }

        public float Alpha { get { return spriteManager.Sprites[0].Alpha; } }

        internal void Display()
        {
            Transformation bounce = new TransformationBounce(Clock.Time, Clock.Time + 800, 1, 0.1f, 8);
            Transformation fadeIn = new Transformation(TransformationType.Fade, 0, 1, Clock.Time, Clock.Time + 200);

            spriteManager.Transform(bounce);
            spriteManager.Transform(fadeIn);
        }
    }

    public enum NotificationStyle
    {
        Okay,
        YesNo
    }
}
