using osum.GameModes;

namespace osum.Support
{
    public class Transition : GameComponent
    {
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

        public virtual bool SkipScreenClear
        {
            get { return false; }
        }
    }
}

