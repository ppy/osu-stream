using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osum.Helpers
{
    public static class Clock
    {
        // measured in milliseconds
        private static int time = 0;

        public static int Time
        {
            get { return time; }
        }

        public static void Update(double elasped)
        {
            time += (int)(elasped * 1000);
        }
    }
}
