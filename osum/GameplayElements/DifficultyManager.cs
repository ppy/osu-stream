using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.GameModes;

namespace osum.GameplayElements
{
    internal static class DifficultyManager
    {
        internal const float HitObjectRadiusSolid = 59;
        internal const float HitObjectRadiusSprite = 64;
        //these are values as found on the spritesheet
        //they are @2x sizes. half them for gamefield radius

        internal static float HitObjectRadiusGamefield { get { return HitObjectRadiusSprite * HitObjectSizeModifier * GameBase.SpriteToBaseRatio; } }
        internal static float HitObjectRadiusSolidGamefield { get { return HitObjectRadiusSolid * HitObjectSizeModifier * GameBase.SpriteToBaseRatio; } }
        internal static float HitObjectRadiusSolidGamefieldHittable { get { return HitObjectRadiusSolid * HitObjectSizeModifier * GameBase.SpriteToBaseRatio * 1.3f; } }

        public static float HitObjectSizeModifier = 1f;

        /// <summary>
        /// Radius of hitObjects in the native field.
        /// </summary>
        public static float HitObjectRadius { get { return HitObjectRadiusSolid * HitObjectSizeModifier * GameBase.SpriteToNativeRatio; } }
        public static float HitObjectRadiusFull { get { return HitObjectRadiusSprite * HitObjectSizeModifier * GameBase.SpriteToNativeRatio; } }

        public static int SliderVelocity = 300;

        internal static int PreEmpt
        {
            get
            {
                float adjustment = 1;
                switch (Player.Difficulty)
                {
                    case Difficulty.Easy:
                        adjustment = 1.4f;
                        break;
                }

                return (int)(1000 * adjustment);
            }
        }
        // TODO: PreEmptSnakeStart should depend on the slider length.
        // For very short sliders, it should be around 50% of PreEmpt,
        // whereas for long ones, it should be as large as (but never larger than) PreEmpt.
        internal static int PreEmptSnakeStart { get { return PreEmpt; } }
        internal static int PreEmptSnakeEnd { get { return 500; } }
        internal static int HitWindow50 { get { return 150; } }
        internal static int HitWindow100 { get { return 100; } }
        internal static int HitWindow300 { get { return 33; } }
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
