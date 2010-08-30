using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osum.GameplayElements
{
    internal static class DifficultyManager
    {
        public static float HitObjectRadius = 128; //todo: implement/fix
        public static int SliderVelocity = 300;

        internal static int PreEmpt { get { return 1500; } }
        internal static int PreEmptSnakeStart { get { return 1000; } }
        internal static int PreEmptSnakeEnd { get { return 500; } }
        internal static int HitWindow50 { get { return 150; } }
        internal static int HitWindow100 { get { return 100; } }
        internal static int HitWindow300 { get { return 50; } }
        internal static int FadeIn { get { return 400; } }
        internal static int FadeOut { get { return 380; } }
        internal static int SpinnerRotationRatio { get { return 5; } }
        internal static int DistanceBetweenTicks { get { return 30; } }
    }
}
