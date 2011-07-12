using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Sprites;
using osum.Helpers;

namespace osum.Support
{
    class FadeTransition : Transition
    {
        public const int DEFAULT_FADE_OUT = 400;
        public const int DEFAULT_FADE_IN = 400;

        private int FadeOutTime;
        private int FadeInTime;

        public FadeTransition()
            : this(DEFAULT_FADE_OUT, DEFAULT_FADE_IN)
        { }

        public FadeTransition(int fadeOut, int fadeIn)
            : base()
        {
            FadeOutTime = fadeOut;
            FadeInTime = fadeIn;
        }

        FadeState fadeState = FadeState.FadeOut;
        
        float currentValue; //todo: yucky.
        private float drawDim;
        public override float CurrentValue {
            get {
                return currentValue;
            }
        }

        public override bool Draw()
        {
            drawDim = SpriteManager.UniversalDim;

            return base.Draw();
        }

        public override void Update()
        {
            switch (fadeState)
            {
                case FadeState.FadeIn:
                    if (FadeInTime == 0)
                        SpriteManager.UniversalDim = 0;
                    else
                        SpriteManager.UniversalDim = (float)Math.Max(0, SpriteManager.UniversalDim - Clock.ElapsedMilliseconds / FadeInTime);
                    break;
                case FadeState.FadeOut:
                    if (FadeOutTime == 0)
                        SpriteManager.UniversalDim = 1;
                    else
                        SpriteManager.UniversalDim = (float)Math.Min(1, SpriteManager.UniversalDim + Clock.ElapsedMilliseconds / FadeOutTime);
                    break;
            }
            
            currentValue = 1 - SpriteManager.UniversalDim; //todo: yucky.

            base.Update();
        }

        internal override void FadeIn()
        {
            fadeState = FadeState.FadeIn;
            base.FadeIn();
        }

        public override bool FadeInDone
        {
            get
            {
                return drawDim == 0 && fadeState == FadeState.FadeIn;
            }
        }

        public override bool FadeOutDone
        {
            get
            {
                return (drawDim == 1 && fadeState == FadeState.FadeOut) || fadeState == FadeState.FadeIn;
            }
        }
    }

    enum FadeState
    {
        FadeOut,
        FadeIn
    }
}
