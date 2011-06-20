using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Support;

namespace BeatmapCombinator
{
    public class FakeAudioTimeSource : ITimeSource
    {
        public double InternalTime;

        #region ITimeSource Members

        public double CurrentTime
        {
            get { return InternalTime; }
        }

        public bool IsElapsing
        {
            get { return true; }
        }

        #endregion
    }
}
