using System.Threading;
using osu.Helpers;

namespace osu_common.Helpers
{
    public class SleepScheduler : Scheduler
    {
        private SleepHandle sleeper;
        public SleepScheduler(SleepHandle sleeper)
            : base()
        {
            this.sleeper = sleeper;
        }
        public override void Add(VoidDelegate d, bool forceDelayed)
        {
            if (!sleeper.IsSleeping || Thread.CurrentThread.ManagedThreadId == mainThreadID)
                base.Add(d, forceDelayed);
            else
                ThreadPool.QueueUserWorkItem(State => 
                { 
	                if (sleeper.IsSleeping)
                		sleeper.Invoke(new VoidDelegate(d));
	                else
                        Add(d, forceDelayed);
                });
        }
        public override void Add(VoidDelegate d)
        {
            Add(d, false);
        }
    }
}