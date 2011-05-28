using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Sprites;

namespace osum.Support
{
    class FadeTransition : Transition
    {
        private int FadeOutTime;
        private int FadeInTime;

        public FadeTransition()
            : this(300, 300)
        { }

        public FadeTransition(int fadeOut, int fadeIn)
            : base()
        {
            FadeOutTime = fadeOut;
            FadeInTime = fadeIn;
        }

        FadeState fadeState = FadeState.FadeOut;
		
		float currentValue; //todo: yucky.
		public override float CurrentValue {
			get {
				return currentValue;
			}
		}

        public override void Update()
        {
            switch (fadeState)
            {
                case FadeState.FadeIn:
                    if (FadeInTime == 0)
						SpriteManager.UniversalDim = 0;
                	else
						SpriteManager.UniversalDim = (float)Math.Max(0, SpriteManager.UniversalDim - GameBase.ElapsedMilliseconds / FadeInTime);
					break;
                case FadeState.FadeOut:
					if (FadeOutTime == 0)
                    	SpriteManager.UniversalDim = 1;
					else
						SpriteManager.UniversalDim = (float)Math.Min(1, SpriteManager.UniversalDim + GameBase.ElapsedMilliseconds / FadeOutTime);
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
                return SpriteManager.UniversalDim == 0 && fadeState == FadeState.FadeIn;
            }
        }

        public override bool FadeOutDone
        {
            get
            {
                return SpriteManager.UniversalDim == 1 && fadeState == FadeState.FadeOut;
            }
        }
    }

    enum FadeState
    {
        FadeOut,
        FadeIn
    }
}
