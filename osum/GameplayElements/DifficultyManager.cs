using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osum.GameplayElements
{
    internal static class DifficultyManager
    {
        public static float HitObjectRadiusDefault { get { return 64 * GameBase.SpriteRatioToWindowBase; } }
		
		/// <summary>
        /// Radius of hitObjects in a gamefield.
        /// </summary>
        public static float HitObjectRadius { get { return 90 * GameBase.SpriteRatioToWindowBase; } }

        public static int SliderVelocity = 300;

        internal static int PreEmpt { get { return 1000; } }
        // TODO: PreEmptSnakeStart should depend on the slider length.
        // For very short sliders, it should be around 50% of PreEmpt,
        // whereas for long ones, it should be as large as (but never larger than) PreEmpt.
        internal static int PreEmptSnakeStart { get { return 1000; } }
        internal static int PreEmptSnakeEnd { get { return 500; } }
        internal static int HitWindow50 { get { return 150; } }
        internal static int HitWindow100 { get { return 100; } }
        internal static int HitWindow300 { get { return 50; } }
        internal static int FadeIn { get { return 400; } }
        internal static int FadeOut { get { return 300; } }
        internal static int SpinnerRotationRatio { get { return 5; } }
        internal static int DistanceBetweenTicks { get { return 30; } }

        /// <summary>
        /// Distance between consecutive follow-line sprites.
        /// </summary>
        internal static int FollowLineDistance = 32;

        /// <summary>
        /// Number of milliseconds to preempt the follow line.  Higher will make the line appear earlier.
        /// </summary>
        internal static int FollowLinePreEmpt = 800;
    }
}
