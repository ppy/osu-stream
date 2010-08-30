using System;
using osum.Support;
namespace osum
{
    public class Transition : IUpdateable
    {
        public Transition()
        {
        }

        public virtual bool FadeOutDone
        {
            get { return true; }
        }

        public virtual bool FadeInDone
        {
            get { return true; }
        }

        public virtual void Update()
        {

        }

        internal virtual void FadeIn()
        {
        }
    }
}

