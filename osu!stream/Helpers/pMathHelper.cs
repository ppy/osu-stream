using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;

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
            return (float)Math.Sqrt(num3);
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

        private static readonly List<Vector2> working = new List<Vector2>();

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
        private const float MAX_ANGLE_SIN = 0.1305261922f; // sine of maximum angle permitted between segments

        internal static List<Vector2> CreateBezier(List<Vector2> input)
        {
            List<Vector2> output = new List<Vector2>();
            // linear formula
            if (input.Count == 2)
            {
                LinearPart(output, input[0], input[1], MAX_LENGTH);
                output.Add(input[1]);
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
                        Vector2 v0 = p1 - p0;
                        Vector2 v1 = p2 - p1;

                        // find angle between using cross product since sin(x) has more accuracy near 0 than cos(x).
                        float r = v0.LengthSquared;
                        angle1 = Math.Abs(v0.X * v1.Y - v0.Y * v1.X) * MathHelper.InverseSqrtFast(r * v1.LengthSquared);

                        // todo: the dependency on angle should be a weighted function of length instead of all/nothing
                        if (r > MIN_LENGTH_SQUARED && (angle0 > MAX_ANGLE_SIN || angle1 > MAX_ANGLE_SIN || r > MAX_LENGTH_SQUARED))
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
                    if (_r > MIN_LENGTH_SQUARED && (angle0 > MAX_ANGLE_SIN || _r > MAX_LENGTH_SQUARED))
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
                LinearPart(output, controlPoints[i - 1], controlPoints[i], detailLevel);

            output.Add(controlPoints[controlPoints.Count - 1]);

            return output;
        }

        private static void LinearPart(List<Vector2> output, Vector2 p0, Vector2 p1, int detailLevel)
        {
            int segments = (int)((p1 - p0).Length / detailLevel + 1);
            for (int j = 0; j < segments; j++)
                output.Add(p0 + (p1 - p0) * ((float)j / segments));
        }

        internal static float ClampToOne(float p)
        {
            return Math.Max(0, Math.Min(1, p));
        }

        internal static void CircleThroughPoints(Vector2 A, Vector2 B, Vector2 C,
            out Vector2 centre, out float radius, out double t_initial, out double t_final)
        {
            // Circle through 3 points
            // https://en.wikipedia.org/wiki/Circumscribed_circle#Cartesian_coordinates
            float D = 2 * (A.X * (B.Y - C.Y) + B.X * (C.Y - A.Y) + C.X * (A.Y - B.Y));
            float AMagSq = A.LengthSquared;
            float BMagSq = B.LengthSquared;
            float CMagSq = C.LengthSquared;
            centre = new Vector2(
                (AMagSq * (B.Y - C.Y) + BMagSq * (C.Y - A.Y) + CMagSq * (A.Y - B.Y)) / D,
                (AMagSq * (C.X - B.X) + BMagSq * (A.X - C.X) + CMagSq * (B.X - A.X)) / D);
            radius = Distance(centre, A);

            t_initial = CircleTAt(A, centre);
            double t_mid = CircleTAt(B, centre);
            t_final = CircleTAt(C, centre);

            while (t_mid < t_initial) t_mid += 2 * Math.PI;
            while (t_final < t_initial) t_final += 2 * Math.PI;
            if (t_mid > t_final)
            {
                t_final -= 2 * Math.PI;
            }
        }

        internal static double CircleTAt(Vector2 pt, Vector2 centre)
        {
            return Math.Atan2(pt.Y - centre.Y, pt.X - centre.X);
        }

        internal static Vector2 CirclePoint(Vector2 centre, float radius, double t)
        {
            return new Vector2((float)(Math.Cos(t) * radius), (float)(Math.Sin(t) * radius)) + centre;
        }
    }
}