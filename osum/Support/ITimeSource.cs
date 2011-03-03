using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osum.Support
{
    public interface ITimeSource
    {
        double CurrentTime { get; }
        bool IsElapsing { get; }
    }
}
