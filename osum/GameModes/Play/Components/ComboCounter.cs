using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Sprites;
using OpenTK;
using osum.Helpers;
using OpenTK.Graphics;

namespace osum.GameModes.Play.Components
{
    class ComboCounter : GameComponent
    {
        internal pSpriteText s_hitCombo;
        internal pSpriteText s_hitCombo_Incoming;
        internal int displayCombo;
        internal int currentCombo;

        public ComboCounter()
            : base()
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            s_hitCombo = new pSpriteText("0x", "score", -2,
                    FieldTypes.StandardSnapBottomLeft, OriginTypes.BottomLeft, ClockTypes.Game,
                    new Vector2(2, 2), 0.92F, true, Color4.White);
            s_hitCombo.Alpha = 0;
            //s_hitCombo.OriginVector = new Vector2(3, 40);
            s_hitCombo.ScaleScalar = 1.28F;

            s_hitCombo_Incoming =
                new pSpriteText("0x", "score", -2,
                                FieldTypes.StandardSnapBottomLeft, OriginTypes.BottomLeft, ClockTypes.Game,
                                new Vector2(2, 2), 0.91F, true, Color4.White);
            s_hitCombo_Incoming.Alpha = 0;
            //s_hitCombo_Incoming.OriginVector = new Vector2(3, 40);
            s_hitCombo_Incoming.Additive = true;

            spriteManager.Add(s_hitCombo);
            spriteManager.Add(s_hitCombo_Incoming);
        }

        /*internal virtual void SlideOut()
        {
            s_hitCombo.FadeOut(1000);
            s_hitCombo.MoveTo(s_hitCombo.StartPosition - new Vector2(80, 0), 1000, EasingTypes.Out);
        }

        internal virtual void SlideIn()
        {
            s_hitCombo.FadeIn(1000);
            s_hitCombo.MoveTo(s_hitCombo.StartPosition, 1000, EasingTypes.In);
        }*/

        internal virtual void EnsureVisible()
        {
            if (displayComboMainCounter != 0 && s_hitCombo.Alpha == 0)
                s_hitCombo.FadeIn(0);
            else if (displayCombo == 0 && currentCombo == 0 && s_hitCombo.Alpha == 1)
                s_hitCombo.FadeOut(120);
        }

        internal void Reset()
        {
            currentCombo = 0;
            displayCombo = 0;
        }

        public override void Update()
        {
            base.Update();

            EnsureVisible();

            //Hit combo display (bottom-left)
            if (displayCombo != currentCombo)
            {
                if (displayCombo > currentCombo)
                    OnDecrease(currentCombo);
                else if (displayCombo < currentCombo)
                    OnIncrease(currentCombo);

                s_hitCombo_Incoming.TagNumeric = displayCombo;

                s_hitCombo_Incoming.ShowInt(displayCombo, 0, false, 'x');
            }

            if (s_hitCombo_Incoming.Transformations.Count > 0)
            {
                if (s_hitCombo_Incoming.Transformations[0].EndTime < Clock.Time + 140 && s_hitCombo.TagNumeric != s_hitCombo_Incoming.TagNumeric)
                    transferToMainCounter();
            }
            else if (s_hitCombo.TagNumeric != s_hitCombo_Incoming.TagNumeric)
            {
                s_hitCombo.TextArray = s_hitCombo_Incoming.TextArray;
                s_hitCombo.TagNumeric = s_hitCombo_Incoming.TagNumeric;
            }
        }

        internal void SetCombo(int combo)
        {
            currentCombo = combo;
        }

        int displayComboMainCounter = 0;

        private void transferToMainCounter()
        {
            displayComboMainCounter = s_hitCombo_Incoming.TagNumeric;

            s_hitCombo.TagNumeric = s_hitCombo_Incoming.TagNumeric;
            s_hitCombo.TextArray = s_hitCombo_Incoming.TextArray;

            s_hitCombo.Transformations.RemoveAll(tr => tr.Type == TransformationType.Scale);
            Transformation t1 = new TransformationF(TransformationType.Scale, 1.28F, 1.4F, Clock.Time, Clock.Time + 50);
            t1.Easing = EasingTypes.Out;
            s_hitCombo.Transformations.Add(t1);
            t1 = new TransformationF(TransformationType.Scale, 1.4f, 1.28F, Clock.Time + 50, Clock.Time + 100);
            t1.Easing = EasingTypes.In;
            s_hitCombo.Transformations.Add(t1);
        }

        protected virtual void OnIncrease(int currentCombo)
        {
            displayCombo++;

            if (s_hitCombo.TagNumeric != s_hitCombo_Incoming.TagNumeric)
                transferToMainCounter();

            s_hitCombo_Incoming.Transformations.Clear();
            Transformation t1 =
                new TransformationF(TransformationType.Scale, 2F, 1.28F, Clock.Time, Clock.Time + 300);
            Transformation t2 =
                new TransformationF(TransformationType.Fade, 0.6F, 0, Clock.Time, Clock.Time + 300);
            s_hitCombo_Incoming.Transform(t1);
            s_hitCombo_Incoming.Transform(t2);
        }

        protected virtual void OnDecrease(int currentCombo)
        {
            if (currentCombo == 0)
                displayCombo -= 1;
            else
                displayCombo = 0;

            displayComboMainCounter = displayCombo;
        }

        internal void IncreaseCombo()
        {
            SetCombo(currentCombo + 1);
        }
    }
}
