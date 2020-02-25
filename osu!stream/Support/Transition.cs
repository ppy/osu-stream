using osum.GameModes;

namespace osum.Support
{
    public class Transition : GameComponent
    {
        public virtual float CurrentValue => 0;

        public virtual bool FadeOutDone => true;

        public virtual bool FadeInDone => true;

        internal virtual void FadeIn()
        {
        }

        public override void Initialize()
        {
        }

        public virtual bool SkipScreenClear => false;
    }
}