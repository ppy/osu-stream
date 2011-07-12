using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.GameModes;
using osum.GameplayElements.Scoring;

namespace osum.GameplayElements
{
    public static class DifficultyManager
    {
        public const float HitObjectRadiusSolid = 59;
        public const float HitObjectRadiusSprite = 64;
        //these are values as found on the spritesheet
        //they are @2x sizes. half them for gamefield radius

        public static float HitObjectRadiusGamefield { get { return HitObjectRadiusSprite * HitObjectSizeModifier * GameBase.SpriteToBaseRatio; } }
        public static float HitObjectRadiusSolidGamefield { get { return HitObjectRadiusSolid * HitObjectSizeModifier * GameBase.SpriteToBaseRatio; } }
        public static float HitObjectRadiusSolidGamefieldHittable
        {
            get
            {
                float leniency = 1.4f;

                switch (Player.Difficulty)
                {
                    case Difficulty.Easy:
                        leniency = 1.6f;
                        break;
                    case Difficulty.Expert:
                        leniency = 1.3f;
                        break;
                }

                return HitObjectRadiusSolid * HitObjectSizeModifier * GameBase.SpriteToBaseRatio * leniency;
            }
        }

        public static float HitObjectSizeModifier = 1f;

        /// <summary>
        /// Radius of hitObjects in the native field.
        /// </summary>
        public static float HitObjectRadius { get { return HitObjectRadiusSolid * HitObjectSizeModifier * GameBase.SpriteToNativeRatio; } }
        public static float HitObjectRadiusFull { get { return HitObjectRadiusSprite * HitObjectSizeModifier * GameBase.SpriteToNativeRatio; } }

        public static int SliderVelocity = 300;

        public static int PreEmpt
        {
            get
            {
                float adjustment = 1;
                switch (Player.Difficulty)
                {
                    case Difficulty.Easy:
                        adjustment = 1.4f;
                        break;
                    case Difficulty.Expert:
                        adjustment = 0.8f;
                        break;
                }

                return (int)(1000 * adjustment);
            }
        }

        // at what time does the snaking animation of a LONG slider begin?
        public static int SnakeStart { get { return PreEmpt * 9 / 10; } }

        // at what time does the snaking animation of a SHORT slider end?
        public static int SnakeEndDesired { get { return PreEmpt / 2; } }

        // at what time does the snaking animation of a LONG slider end?
        public static int SnakeEndLimit { get { return PreEmpt * 3 / 10; } }

        // at what speed does the snaking animation of a SHORT slider go? (milliseconds per osupixel)
        public static double SnakeSpeedInverse { get { return 3.0d; } }

        public static int HitWindow50
        {
            get
            {
                switch (Player.Difficulty)
                {
                    case Difficulty.Easy:
                        return 240;
                    default:
                        return 152;
                    case Difficulty.Expert:
                        return 88;
                }
            }
        }

        public static int HitWindow100
        {
            get
            {
                switch (Player.Difficulty)
                {
                    case Difficulty.Easy:
                        return 120;
                    default:
                        return 76;
                    case Difficulty.Expert:
                        return 44;
                }
            }
        }

        public static int HitWindow300
        {
            get
            {
                switch (Player.Difficulty)
                {
                    case Difficulty.Easy:
                        return 60;
                    default:
                        return 38;
                    case Difficulty.Expert:
                        return 22;
                }
            }
        }

        public static float HpAdjustment
        {
            get
            {
                switch (Player.Difficulty)
                {
                    case Difficulty.Expert:
                        return 1.3f;
                    default:
                        return 1;
                }
            }
        }

        public static int FadeIn { get { return 400; } }
        public static int FadeOut { get { return 300; } }
        public static int SpinnerRotationRatio
        {
            get
            {
                switch (Player.Difficulty)
                {
                    case Difficulty.Expert:
                        return 3;
                    default:
                        return 2;
                }
            }
        }

        public static int DistanceBetweenTicks { get { return 30; } }

        /// <summary>
        /// Distance between consecutive follow-line sprites.
        /// </summary>
        public static int FollowLineDistance = 32;

        /// <summary>
        /// Number of milliseconds to preempt the follow line.  Higher will make the line appear earlier.
        /// </summary>
        public static int FollowLinePreEmpt = 800;

        public static double InitialHp { get { return Player.Difficulty == Difficulty.Normal ? HealthBar.HP_BAR_MAXIMUM / 2 : HealthBar.HP_BAR_MAXIMUM; } }
    }
}
