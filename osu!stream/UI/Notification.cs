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
using osum.Resources;
using osum.Audio;

namespace osum.UI
{
    public class Notification : SpriteManager
    {
        public bool Dismissed;
        private BoolDelegate Action;

        NotificationStyle Style;

        pSprite okayButton;
        pSprite cancelButton;

        public Notification(string title, string description, NotificationStyle style, BoolDelegate action = null)
        {
            Clocking = ClockTypes.Game;

            pSprite back = new pSprite(TextureManager.Load(OsuTexture.notification_background), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Game, Vector2.Zero, 0.98f, true, Color4.White)
            {
                DimImmune = true,
            };

            pText titleText = new pText(title, 36, new Vector2(0, -130), new Vector2(600 * GameBase.SpriteToBaseRatio, 0), 1, true, Color4.White, true)
            {
                Field = FieldTypes.StandardSnapCentre,
                Origin = OriginTypes.Centre,
                TextAlignment = TextAlignment.Centre,
                Clocking = ClockTypes.Game,
                DimImmune = true
            };

            pText descText = new pText(description, 24, new Vector2(0, -90), new Vector2(600 * GameBase.SpriteToBaseRatio, 0), 1, true, Color4.White, false)
            {
                Field = FieldTypes.StandardSnapCentre,
                Origin = OriginTypes.TopCentre,
                TextAlignment = TextAlignment.Centre,
                Clocking = ClockTypes.Game,
                DimImmune = true
            };

            Alpha = 0;
            DrawDepth = 1;

            Action = action;


            Style = style;

            AddControls(style);

            Add(back);
            Add(descText);
            Add(titleText);
        }

        private void AddControls(NotificationStyle style)
        {
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
                            dismiss(true);
                        };

                        Add(okayButton);

                        pText okayText = new pText(LocalisationManager.GetString(OsuString.Okay), 24, new Vector2(0, button_height), Vector2.Zero, 1, true, Color4.White, true)
                        {
                            Field = FieldTypes.StandardSnapCentre,
                            Origin = OriginTypes.Centre,
                            Clocking = ClockTypes.Game,
                            DimImmune = true
                        };

                        Add(okayText);
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
                            dismiss(true);
                        };

                        Add(okayButton);

                        pText okayText = new pText(LocalisationManager.GetString(OsuString.Yes), 24, new Vector2(-140, button_height), Vector2.Zero, 1, true, Color4.White, true)
                        {
                            Field = FieldTypes.StandardSnapCentre,
                            Origin = OriginTypes.Centre,
                            Clocking = ClockTypes.Game,
                            DimImmune = true
                        };

                        Add(okayText);
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
                            dismiss(false);
                        };

                        Add(cancelButton);

                        pText cancelText = new pText(LocalisationManager.GetString(OsuString.No), 24, new Vector2(140, button_height), Vector2.Zero, 1, true, Color4.White, true)
                        {
                            Field = FieldTypes.StandardSnapCentre,
                            Origin = OriginTypes.Centre,
                            Clocking = ClockTypes.Game,
                            DimImmune = true
                        };

                        Add(cancelText);
                    }
                    break;
            }
        }

        private void dismiss(bool completed)
        {
            AudioEngine.PlaySample(OsuSamples.ButtonTap);

            GameBase.Scheduler.Add(delegate
            {
                if (Action != null) Action(completed);
                Dismissed = true;
                AlwaysDraw = false;
            }, 300);

            FadeOut(300);
            ScaleTo(0.95f, 300, EasingTypes.Out);
            RotateTo(0.05f, 300, EasingTypes.Out);
            
        }

        internal virtual void Display()
        {
            Transformation bounce = new TransformationBounce(Clock.Time, Clock.Time + 800, 1, 0.1f, 8);
            Transform(bounce);

            FadeIn(200);

            AudioEngine.PlaySample(OsuSamples.Notify);

        }
    }

    public enum NotificationStyle
    {
        Okay,
        YesNo
    }
}
