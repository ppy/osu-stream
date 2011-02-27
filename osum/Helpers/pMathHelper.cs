using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using System.Drawing;
using osum.Graphics.Primitives;

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

        public static float Distance(Vector2 value1, Vector2 value2)
        {
            float num2 = value1.X - value2.X;
            float num = value1.Y - value2.Y;
            float num3 = (num2 * num2) + (num * num);
            return (float)Math.Sqrt((double)num3);
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

        internal static List<Vector2> CreateBezier(List<Vector2> input, int points)
        {
            int count = input.Count;

            Vector2[] working = new Vector2[count];
            List<Vector2> output = new List<Vector2>();

            for (int i = 0; i < points; i++)
            {
                for (int j = 0; j < count; j++)
                    working[j] = input[j];

                for (int level = 0; level < count; level++)
                    for (int j = 0; j < count - level - 1; j++)
                        Vector2.Lerp(ref working[j], ref working[j + 1], (float)i / points, out working[j]);
                output.Add(working[0]);
            }

            return output;
        }

        public static Vector2 CatmullRom(Vector2 value1, Vector2 value2, Vector2 value3, Vector2 value4, float amount)
        {
            Vector2 vector;
            float num = amount * amount;
            float num2 = amount * num;
            vector.X = 0.5f * ((((2f * value2.X) + ((-value1.X + value3.X) * amount)) + (((((2f * value1.X) - (5f * value2.X)) + (4f * value3.X)) - value4.X) * num)) + ((((-value1.X + (3f * value2.X)) - (3f * value3.X)) + value4.X) * num2));
            vector.Y = 0.5f * ((((2f * value2.Y) + ((-value1.Y + value3.Y) * amount)) + (((((2f * value1.Y) - (5f * value2.Y)) + (4f * value3.Y)) - value4.Y) * num)) + ((((-value1.Y + (3f * value2.Y)) - (3f * value3.Y)) + value4.Y) * num2));
            return vector;
        }


        internal static List<Vector2> CreateCatmull(List<Vector2> controlPoints, int detailLevel)
        {
            List<Vector2> output = new List<Vector2>();

            for (int j = 0; j < controlPoints.Count - 1; j++)
            {
                Vector2 v1 = (j - 1 >= 0 ? controlPoints[j - 1] : controlPoints[j]);
                Vector2 v2 = controlPoints[j];
                Vector2 v3 = (j + 1 < controlPoints.Count
                                  ? controlPoints[j + 1]
                                  : v2 + (v2 - v1));
                Vector2 v4 = (j + 2 < controlPoints.Count
                                  ? controlPoints[j + 2]
                                  : v3 + (v3 - v2));

                for (int k = 0; k < detailLevel; k++)
                {
                    Vector2 vector;

                    float amount = (float)k / detailLevel;
                    float num = amount * amount;
                    float num2 = amount * num;

                    vector.X = 0.5f * ((((2f * v2.X) + ((-v1.X + v3.X) * amount)) + (((((2f * v1.X) - (5f * v2.X)) + (4f * v3.X)) - v4.X) * num)) + ((((-v1.X + (3f * v2.X)) - (3f * v3.X)) + v4.X) * num2));
                    vector.Y = 0.5f * ((((2f * v2.Y) + ((-v1.Y + v3.Y) * amount)) + (((((2f * v1.Y) - (5f * v2.Y)) + (4f * v3.Y)) - v4.Y) * num)) + ((((-v1.Y + (3f * v2.Y)) - (3f * v3.Y)) + v4.Y) * num2));

                    output.Add(vector);
                }
            }

            return output;
        }

        internal static List<Vector2> CreateLinear(List<Vector2> controlPoints, int detailLevel)
        {
            List<Vector2> output = new List<Vector2>();

            for (int i = 1; i < controlPoints.Count; i++)
            {
                Line l = new Line(controlPoints[i - 1], controlPoints[i]);
                int segments = (int)(l.rho / detailLevel);
                for (int j = 0; j < segments; j++)
                    output.Add(l.p1 + (l.p2 - l.p1) * ((float)j / segments));
            }

            return output;
        }

        internal static float ClampToOne(float p)
        {
            return Math.Max(0, Math.Min(1, p));
        }


    }
}
