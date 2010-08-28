using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Drawing;

namespace osum.Helpers
{
    public static class pMathHelper
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
		
		public static Vector2 Point2Vector(PointF point)
		{
			return new Vector2(point.X, point.Y);	
		}

        internal static List<Vector2> CreateBezier(List<Vector2> input, int detailLevel)
        {
            int count = input.Count;

            Vector2[] working = new Vector2[count];
            List<Vector2> output = new List<Vector2>();

            //todo: make detail based on line length rather than point count?
            int points = detailLevel * count;

            for (int iteration = 0; iteration < points; iteration++)
            {
                for (int i = 0; i < count; i++)
                    working[i] = input[i];

                for (int level = 0; level < count; level++)
                    for (int i = 0; i < count - level - 1; i++)
                        Vector2.Lerp(ref working[i], ref working[i + 1], (float)iteration / points, out working[i]);
                output.Add(working[0]);
            }

            return output;
        }

        internal static float ClampToOne(float p)
        {
            return Math.Max(0, Math.Min(1, p));
        }
    }
}
