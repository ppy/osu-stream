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
                        return 1400;
                    default:
                        return 1000;
                    case Difficulty.Expert:
                        return 800;

                }
            }
        }

        // at what time does the snaking animation of a LONG slider begin?
        public static int SnakeStart { get { return PreEmpt * 8 / 10; } }

        // at what time does the snaking animation of a SHORT slider end?
        public static int SnakeEndDesired { get { return PreEmpt * 5 / 10; } }

        // at what time does the snaking animation of a LONG slider end?
        public static int SnakeEndLimit { get { return PreEmpt * 3 / 10; } }

        // at what speed does the snaking animation of a SHORT slider go? (milliseconds per osupixel)
        public static double SnakeSpeedInverse { get
        {
            switch (Player.Difficulty)
            {
                case Difficulty.Easy:
                    return 3.2d;
                default:
                    return 2.8d;
                case Difficulty.Expert:
                    return 2.4d;

            }
        } }

        const int HIT_EXPERT = 25;
        const int HIT_STREAM = 44;
        const int HIT_EASY = 70;

        public static int HitWindow50
        {
            get
            {
                int window = 0;
                switch (Player.Difficulty)
                {
                    case Difficulty.Easy:
                        window = HIT_EASY;
                        break;
                    default:
                        window = HIT_STREAM;
                        break;
                    case Difficulty.Expert:
                        window = HIT_EXPERT;
                        break;
                }

                return (window * 5);
            }
        }

        public static int HitWindow100
        {
            get
            {
                int window = 0;
                switch (Player.Difficulty)
                {
                    case Difficulty.Easy:
                        window = HIT_EASY;
                        break;
                    default:
                        window = HIT_STREAM;
                        break;
                    case Difficulty.Expert:
                        window = HIT_EXPERT;
                        break;
                }

                return (window * 5)/2;
            }
        }

        public static int HitWindow300
        {
            get
            {
                switch (Player.Difficulty)
                {
                    case Difficulty.Easy:
                        return HIT_EASY;
                    default:
                        return HIT_STREAM;
                    case Difficulty.Expert:
                        return HIT_EXPERT;
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
        /// This stacks on top of regular PreEmpt to make follow lines begin appearing just before the object they connect to.
        /// </summary>
        public static int FollowLinePreEmptStart = 500;

        /// <summary>
        /// Number of milliseconds after the follow line's starting object got hit before they've finished disappearing.
        /// </summary>
        public static int FollowLinePreEmptEnd = 300;

        /// <summary>
        /// Shortest time a followpoint can spend onscreen.
        /// </summary>
        public static int FollowPointScreenTime = 200;

        public static double InitialHp { get { return Player.Difficulty == Difficulty.Normal ? HealthBar.HP_BAR_MAXIMUM / 2 : HealthBar.HP_BAR_MAXIMUM; } }
    }
}
