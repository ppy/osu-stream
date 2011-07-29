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

        private static List<Vector2> working = new List<Vector2>();

        internal static Vector2 BezierPoint(ref List<Vector2> input, float t)
        {
            int count = input.Count;

            lock (working)
            {
                working.Clear();
                foreach (Vector2 v in input)
                    working.Add(v);

                for (int level = 0; level < count; level++)
                    for (int j = 0; j < count - level - 1; j++)
                        working[j] = Vector2.Lerp(working[j], working[j + 1], t);

                return working[0];
            }
        }

        private const int MAX_LENGTH = 12;
        private const float MAX_LENGTH_SQUARED = 144.0f; // square of maximum distance permitted between segments
        private const float MIN_LENGTH_SQUARED = 4.0f; // square of distance at which subdivision should stop anyway
        private const float MAX_ANGLE_TAN = 0.01f; // tan of maximum angle permitted between segments

        internal static List<Vector2> CreateBezier(List<Vector2> input)
        {
            List<Vector2> output = new List<Vector2>();
            // linear formula
            if (input.Count == 2)
            {
                Vector2 p0 = input[0];
                Vector2 p1 = input[1];
                output.Add(p0);
                float r = (input[1] - input[0]).Length;
                for (float x = 0; x < r; x += MAX_LENGTH)
                {
                    output.Add(Vector2.Lerp(p0, p1, x / r));
                }
                output.Add(p1);
                return output;
            }

            SortedList<float, Vector2> points = new SortedList<float, Vector2>();
            
            points.Add(0.0f, input[0]);
            points.Add(1.0f, input[input.Count - 1]);

            SortedList<float, Vector2> addedPoints = new SortedList<float, Vector2>();

            do
            {
                addedPoints.Clear();

                float angle0 = 0.0f, angle1;
                if (points.Count < 3)
                    addedPoints.Add(0.5f, BezierPoint(ref input, 0.5f));
                else
                {
                    for (int x = 0; x < points.Count - 2; x++)
                    {
                        Vector2 p0 = points.Values[x];
                        Vector2 p1 = points.Values[x + 1];
                        Vector2 p2 = points.Values[x + 2];

                        angle1 = (p1.X * p2.Y - p1.Y * p2.X) / (p1.X * p2.Y + p1.Y * p2.X);
                        float r = (p1 - p0).LengthSquared;

                        // todo: the dependency on angle should be a weighted function of length instead of all/nothing
                        if (r > MIN_LENGTH_SQUARED && (angle0 > MAX_ANGLE_TAN || angle1 > MAX_ANGLE_TAN || r > MAX_LENGTH_SQUARED))
                        {
                            float t = (points.Keys[x] + points.Keys[x + 1]) * 0.5f;
                            if (!addedPoints.ContainsKey(t))
                                addedPoints.Add(t, BezierPoint(ref input, t));
                        }

                        angle0 = angle1;
                    }

                    Vector2 _p0 = points.Values[points.Count - 2];
                    Vector2 _p1 = points.Values[points.Count - 1];
                    float _r = (_p1 - _p0).LengthSquared;
                    if (_r > MIN_LENGTH_SQUARED && (angle0 > MAX_ANGLE_TAN || _r > MAX_LENGTH_SQUARED))
                    {
                        float t = (points.Keys[points.Count - 2] + 1.0f) * 0.5f;
                        if (!addedPoints.ContainsKey(t))
                            addedPoints.Add(t, BezierPoint(ref input, t));
                    }
                }

                foreach (KeyValuePair<float, Vector2> k in addedPoints)
                    points.Add(k.Key, k.Value);

            } while (addedPoints.Count > 0);

            output.AddRange(points.Values);
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
