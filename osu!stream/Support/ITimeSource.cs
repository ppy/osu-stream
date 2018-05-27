using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Text;

namespace osum.Support
{
    public interface ITimeSource
    {
        double CurrentTime { get; }
        bool IsElapsing { get; }
    }
}
