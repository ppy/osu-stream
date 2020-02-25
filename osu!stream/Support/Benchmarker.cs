using System;
using System.Diagnostics;

namespace osum.Support
{
    internal class Benchmarker : IDisposable
    {
        private readonly Stopwatch sw = new Stopwatch();

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