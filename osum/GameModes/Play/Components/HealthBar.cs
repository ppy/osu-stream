using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Sprites;
using osum.Graphics;
using OpenTK;
using osum.Helpers;
using osum.Graphics.Skins;
using OpenTK.Graphics;
using osum.GameModes;

namespace osum.GameplayElements.Scoring
{
    internal class HealthBar : GameComponent
    {
        protected pAnimation s_barFill;
        protected internal pSprite s_barBg;
        protected pSprite s_kiIcon;

        protected pTexture t_kiDanger;
        protected pTexture t_kiDanger2;
        protected pTexture t_kiNormal;

        const int HP_BAR_MAXIMUM = 200;

        /// <summary>
        /// Are we currently doing the initial "fill" stage?
        /// </summary>
        internal bool InitialIncrease = true;

        /// <summary>
        /// Time in Audio milliseconds to start the initial HP increase.
        /// </summary>
        internal int InitialIncreaseStartTime;

        /// <summary>
        /// Rate of initial HP increase.
        /// </summary>
        internal double InitialIncreaseRate = 0.02;

        /// <summary>
        /// The rate at which HP will naturally drop.
        /// </summary>
        internal double HpDropRate;

        /// <summary>
        /// DisplayHp lags behind current hp due to smooth movement.  Handled internally.
        /// </summary>
        internal double DisplayHp { get; private set; }

        /// <summary>
        /// Current and accurate HP counter.
        /// </summary>
        internal double CurrentHp { get; private set; }

        /// <summary>
        /// Current HP with no upper limiter.
        /// </summary>
        internal double CurrentHpUncapped { get; private set; }

        private bool visible = true;
        internal bool Visible
        {
            get { return visible; }
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

        internal float CurrentXPosition
        {
            get { return s_barFill.Position.X + s_barFill.DrawWidth * GameBase.SpriteRatioToWindowBase; }
        }

        internal HealthBar()
        {
        }

        internal virtual void SlideOut()
        {
            s_barFill.FadeOut(500);
            s_barBg.FadeOut(500);
            s_kiIcon.FadeOut(500);

            Vector2 off = new Vector2(0, 20);

            s_barFill.MoveTo(s_barFill.StartPosition - off, 500);
            s_barBg.MoveTo(s_barBg.StartPosition - off, 500);

            s_kiIcon.StartPosition = new Vector2(s_kiIcon.Position.X, s_kiIcon.StartPosition.Y);
            s_kiIcon.Transform(new Transformation(TransformationType.Scale, 1, 1.6f, Clock.Time, Clock.Time + 500));
        }

        internal virtual void SlideIn()
        {
            s_barFill.FadeIn(500);
            s_barBg.FadeIn(500);
            s_kiIcon.FadeIn(500);

            s_barFill.MoveTo(s_barFill.StartPosition, 500);
            s_barBg.MoveTo(s_barBg.StartPosition, 500);
            s_kiIcon.Transform(new Transformation(TransformationType.Scale, 1.6f, 1, Clock.Time, Clock.Time + 500));
        }

        public override void Update()
        {
            base.Update();

            if (DisplayHp < HP_BAR_MAXIMUM * 0.2)
                s_kiIcon.Texture = t_kiDanger2;
            else if (DisplayHp < HP_BAR_MAXIMUM * 0.5)
                s_kiIcon.Texture = t_kiDanger;
            else if (s_kiIcon.Texture != t_kiNormal)
                s_kiIcon.Texture = t_kiNormal;

            //HP Bar
            if (DisplayHp < CurrentHp)
            {
                if (InitialIncrease)
                {
                    //if (InitialIncreaseStartTime < AudioEngine.Time && (Player.Recovering || AudioEngine.AudioState == AudioStates.Playing))
                    {
                        DisplayHp = Math.Min(HP_BAR_MAXIMUM, DisplayHp + InitialIncreaseRate * GameBase.ElapsedMilliseconds);
                        if (s_kiIcon.Transformations.Count == 0)
                        {
                            s_kiIcon.Transform(
                                new Transformation(TransformationType.Scale, 1.2F, 0.8F, Clock.Time,
                                                   Clock.Time + 150));
                        }
                    }
                }
                else
                    DisplayHp = Math.Min(HP_BAR_MAXIMUM, DisplayHp + Math.Abs(CurrentHp - DisplayHp) / 4 * GameBase.ElapsedMilliseconds * 0.03);
            }
            else if (DisplayHp > CurrentHp)
            {
                InitialIncrease = false;
                DisplayHp = Math.Max(0, DisplayHp - Math.Abs(DisplayHp - CurrentHp) / 6 * GameBase.ElapsedMilliseconds * 0.1);
            }

            s_barFill.DrawWidth = (int)Math.Min(s_barFill.Width, Math.Max(0, (s_barFill.Width * (DisplayHp / HP_BAR_MAXIMUM))));

            //Sync Ki icon position with the end of the scorebar fill.
            s_kiIcon.Position = new Vector2(CurrentXPosition, s_kiIcon.Position.Y);

        }

        internal virtual void Draw()
        {
            spriteManager.Draw();
        }

        internal virtual void KiBulge()
        {
            s_kiIcon.Transformations.RemoveAll(
                    t => t.Type == TransformationType.Scale);
            s_kiIcon.Transform(new Transformation(TransformationType.Scale, 1.2F, 0.8F, Clock.Time,
                                                             Clock.Time + 150));
        }

        internal virtual void KiExplode()
        {
            if (!visible) return;

            pSprite p =
                    new pSprite(t_kiNormal, FieldTypes.NativeStandardScale, OriginTypes.Centre, ClockTypes.Game,
                                s_kiIcon.Position, 1, false, Color4.White);
            Transformation t =
                new Transformation(TransformationType.Scale, 1, 1.6F, Clock.Time, Clock.Time + 120);
            t.Easing = EasingTypes.In;
            p.Transform(t);
            t =
                new Transformation(TransformationType.Fade, 1, 0, Clock.Time, Clock.Time + 120);
            t.Easing = EasingTypes.In;
            p.Transform(t);

            if (spriteManager != null)
                spriteManager.Add(p);
        }

        internal virtual void SetCurrentHp(double amount)
        {
            CurrentHp = Math.Max(0, Math.Min(HP_BAR_MAXIMUM, amount));
            CurrentHpUncapped = amount;
        }

        internal virtual void ReduceCurrentHp(double amount)
        {
            if (InitialIncrease) InitialIncrease = false;

            //if (Player.Playing && InitialIncrease) InitialIncrease = false;

            CurrentHpUncapped = Math.Max(0, CurrentHpUncapped - amount);
            CurrentHp = Math.Max(0, CurrentHp - amount);
        }

        internal virtual void IncreaseCurrentHp(double amount)
        {
            if (InitialIncrease) InitialIncrease = false;

            //if (Player.Playing && InitialIncrease) InitialIncrease = false;

            CurrentHpUncapped += amount;
            CurrentHp = Math.Max(0, Math.Min(HP_BAR_MAXIMUM, CurrentHp + amount));
        }

        internal void SetDisplayHp(double amount)
        {
            DisplayHp = amount;
        }

        internal override void Initialize()
        {
            s_barFill =
    new pAnimation(TextureManager.LoadAll("scorebar-colour"), FieldTypes.Standard, OriginTypes.TopLeft,
                   ClockTypes.Game, new Vector2(3, 10), 0.965F, true, Color4.White);
            s_barFill.SetFramerateFromSkin();
            s_barFill.DrawDimensionsManualOverride = true;


            t_kiNormal = TextureManager.Load("scorebar-ki");
            t_kiDanger = TextureManager.Load("scorebar-kidanger");
            t_kiDanger2 = TextureManager.Load("scorebar-kidanger2");

            s_kiIcon =
                new pSprite(t_kiNormal, FieldTypes.Standard, OriginTypes.Centre, ClockTypes.Game,
                            new Vector2(0, 10), 0.97F, true, Color4.White);

            s_barBg = new pSprite(TextureManager.Load("scorebar-bg"), FieldTypes.Standard, OriginTypes.TopLeft,
                                    ClockTypes.Game,
                                    Vector2.Zero, 0.96F, true, Color4.White);

            spriteManager.Add(s_barBg);
            spriteManager.Add(s_barFill);
            spriteManager.Add(s_kiIcon);

            CurrentHp = HP_BAR_MAXIMUM;
            CurrentHpUncapped = HP_BAR_MAXIMUM;
            DisplayHp = 0;
        }
    }
}
