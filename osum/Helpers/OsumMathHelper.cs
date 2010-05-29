using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace osum.Helpers
{
    public static class OsumMathHelper
    {
        public static float ToDegrees(float radians)
        {
            return (radians * 57.29578f);
        }

        public static float ToRadians(float degrees)
        {
            return (degrees * 0.01745329f);
        }

        public static float Lerp(float start, float end, float amount)
        {
            return start + ((end - start) * amount);
        }

        public static float DistanceSquared(Vector2 value1, Vector2 value2)
        {
            float num2 = value1.X - value2.X;
            float num = value1.Y - value2.Y;
            return ((num2 * num2) + (num * num));
        }
    }
}
