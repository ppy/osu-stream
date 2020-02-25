using System;
using OpenTK;
using OpenTK.Graphics;
using osum.Graphics;
using osum.Graphics.Sprites;
using osum.Helpers;

namespace osum.GameModes.Play.Components
{
    public class HealthBar : GameComponent
    {
        internal pSprite s_barFill;
        internal pSprite s_barBg;
        internal pSprite s_kiIcon;
        private pSprite s_kiExplode;

        protected pTexture t_kiNormal;

        public const int HP_BAR_MAXIMUM = 200;
        public const int HP_BAR_INITIAL = 100;

        /// <summary>
        /// Are we currently doing the initial "fill" stage?
        /// </summary>
        internal bool InitialIncrease = true;

        /// <summary>
        /// Rate of initial HP increase.
        /// </summary>
        internal double InitialIncreaseRate = 0.04;

        /// <summary>
        /// DisplayHp lags behind current hp due to smooth movement.  Handled internally.
        /// </summary>
        internal double DisplayHp { get; private set; }

        /// <summary>
        /// Current and accurate HP counter.
        /// </summary>
        public double CurrentHp { get; private set; }

        /// <summary>
        /// Current HP with no upper limiter.
        /// </summary>
        public double CurrentHpUncapped { get; private set; }

        private bool visible = true;

        internal bool Visible
        {
            get => visible;
            set
            {
                if (visible == value) return;

                visible = value;

                if (visible)
                    spriteManager.Sprites.ForEach(s => s.FadeIn(100));
                else
                    spriteManager.Sprites.ForEach(s => s.FadeOut(100));
            }
        }

        internal float CurrentXPosition => s_barFill.DisplayRectangle.Right / GameBase.InputToFixedWidthAlign;

        internal HealthBar()
        {
        }

        /*internal virtual void SlideOut()
        {
            s_barFill.FadeOut(500);
            s_barBg.FadeOut(500);
            s_kiIcon.FadeOut(500);

            Vector2 off = new Vector2(0, 20);

            s_barFill.MoveTo(s_barFill.StartPosition - off, 500);
            s_barBg.MoveTo(s_barBg.StartPosition - off, 500);

            s_kiIcon.StartPosition = new Vector2(s_kiIcon.Position.X, s_kiIcon.StartPosition.Y);
            s_kiIcon.Transform(new TransformationF(TransformationType.Scale, 1, 1.6f, Clock.Time, Clock.Time + 500));
        }

        internal virtual void SlideIn()
        {
            s_barFill.FadeIn(500);
            s_barBg.FadeIn(500);
            s_kiIcon.FadeIn(500);

            s_barFill.MoveTo(s_barFill.StartPosition, 500);
            s_barBg.MoveTo(s_barBg.StartPosition, 500);
            s_kiIcon.Transform(new TransformationF(TransformationType.Scale, 1.6f, 1, Clock.Time, Clock.Time + 500));
        }*/

        private Transformation initialAppearTransformation;

        public override void Update()
        {
            base.Update();

            if (DisplayHp != CurrentHp)
            {
                if (DisplayHp < CurrentHp)
                {
                    if (InitialIncrease)
                    {
                        DisplayHp = Math.Min(HP_BAR_MAXIMUM, DisplayHp + InitialIncreaseRate * Clock.ElapsedMilliseconds);
                        if (s_kiIcon.Transformations.Count == 0)
                        {
                            if (initialAppearTransformation == null)
                                initialAppearTransformation = new TransformationF(TransformationType.Scale, 1.2F, 0.8F, Clock.Time, Clock.Time + 150);
                            else
                            {
                                initialAppearTransformation.StartTime = Clock.Time;
                                initialAppearTransformation.EndTime = Clock.Time + 150;
                            }

                            s_kiIcon.Transform(initialAppearTransformation);
                        }
                    }
                    else
                        DisplayHp = Math.Min(HP_BAR_MAXIMUM, DisplayHp + Math.Abs(CurrentHp - DisplayHp) / 4 * Clock.ElapsedMilliseconds * 0.03);
                }
                else if (DisplayHp > CurrentHp)
                {
                    InitialIncrease = false;
                    DisplayHp = Math.Max(0, DisplayHp - Math.Abs(DisplayHp - CurrentHp) / 4 * Clock.ElapsedMilliseconds * 0.1);
                }

                s_barFill.DrawWidth = (int)Math.Min(s_barFill.TextureWidth, Math.Max(0, (s_barFill.TextureWidth * (DisplayHp / HP_BAR_MAXIMUM))));

                //Sync Ki icon position with the end of the scorebar fill.
                s_kiIcon.Position.X = CurrentXPosition;
                s_kiExplode.Position = s_kiIcon.Position;
            }
        }

        private Transformation burstScale;
        private Transformation burstFade;

        internal virtual void KiExplode()
        {
            if (!visible) return;

            burstScale.StartTime = burstFade.StartTime = Clock.Time;
            burstScale.EndTime = burstFade.EndTime = Clock.Time + 180;
        }

        internal virtual void SetCurrentHp(double amount, bool initial = false)
        {
            if (InitialIncrease && !initial) InitialIncrease = false;

            CurrentHp = Math.Max(0, Math.Min(HP_BAR_MAXIMUM, amount));
            CurrentHpUncapped = amount;
        }

        internal virtual void ReduceCurrentHp(double amount)
        {
            if (InitialIncrease) InitialIncrease = false;

            CurrentHpUncapped = Math.Max(0, CurrentHpUncapped - amount);
            CurrentHp = Math.Max(0, CurrentHp - amount);
        }

        internal virtual void IncreaseCurrentHp(double amount)
        {
            if (InitialIncrease) InitialIncrease = false;

            KiExplode();

            CurrentHpUncapped += amount;
            CurrentHp = Math.Max(0, Math.Min(HP_BAR_MAXIMUM, CurrentHp + amount));
        }

        public override void Initialize()
        {
            s_barFill = new pSprite(TextureManager.Load(OsuTexture.scorebar_colour), FieldTypes.Standard, OriginTypes.TopLeft,
                ClockTypes.Game, new Vector2(4, 10f), 0.965F, true, Color4.White);

            s_kiIcon =
                new pSprite(TextureManager.Load(OsuTexture.scorebar_marker), FieldTypes.Standard, OriginTypes.Centre, ClockTypes.Game,
                    new Vector2(0, 14), 0.97F, true, Color4.White);

            s_barBg = new pSprite(TextureManager.Load(OsuTexture.scorebar_background), FieldTypes.Standard, OriginTypes.TopLeft,
                ClockTypes.Game,
                Vector2.Zero, 0.96F, true, Color4.White);

            s_kiExplode =
                new pSprite(TextureManager.Load(OsuTexture.scorebar_marker_hit), FieldTypes.Standard, OriginTypes.Centre, ClockTypes.Game,
                    Vector2.Zero, 1, true, Color4.White);
            s_kiExplode.Alpha = 0;
            s_kiExplode.RemoveOldTransformations = false;
            s_kiExplode.Additive = true;

            burstScale = new TransformationF(TransformationType.Scale, 1, 2F, 0, 0, EasingTypes.In);
            burstFade = new TransformationF(TransformationType.Fade, 1, 0, 0, 0);

            s_kiExplode.Transform(burstScale);
            s_kiExplode.Transform(burstFade);

            spriteManager.Add(s_barBg);
            spriteManager.Add(s_barFill);
            spriteManager.Add(s_kiIcon);
            spriteManager.Add(s_kiExplode);

            CurrentHp = HP_BAR_INITIAL;
            CurrentHpUncapped = HP_BAR_INITIAL;

            DisplayHp = 0;
        }
    }
}