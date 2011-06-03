using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace osum.Support
{
    class Benchmarker : IDisposable
    {
        Stopwatch sw = new Stopwatch();
        public Benchmarker()
        {
            sw.Start();
        }
        #region IDisposable Members

        public void Dispose()
        {
            Console.WriteLine("operation took " + sw.ElapsedTicks + "ms");
        }

        #endregion
    }
}
