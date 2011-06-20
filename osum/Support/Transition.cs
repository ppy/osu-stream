using System;
using osum.Support;
using osum.GameModes;
namespace osum
{
    public class Transition : GameComponent
    {
        public Transition()
        {
        }
        
        public virtual float CurrentValue
        {
            get { return 0; }
        }
        
        public virtual bool FadeOutDone
        {
            get { return true; }
        }

        public virtual bool FadeInDone
        {
            get { return true; }
        }

        internal virtual void FadeIn()
        {
        }

        public override void Initialize()
        {
        }
    }
}

