#region Using Statements

using System;
using Color = OpenTK.Graphics.Color4;
using OpenTK;
using osum.Helpers;

#endregion

namespace osu.Graphics.Primitives
{
    /// <summary>
    /// Represents a single line segment.  Drawing is handled by the LineManager class.
    /// </summary>
    internal class Line
    {
        internal Color color = Color.White;
        internal Vector2 p1; // Begin point of the line
        internal Vector2 p2; // End point of the line
        internal float radius = 0.1f; // The line's total thickness is twice its radius
        internal Vector2 rhoTheta; // Length and angle of the line


        internal Line(Vector2 p1, Vector2 p2)
        {
            this.p1 = p1;
            this.p2 = p2;
            Recalc();
        }


        internal Line(Vector2 p1, Vector2 p2, float radius)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.radius = radius;
            Recalc();
        }


        internal Line(Vector2 p1, Vector2 p2, float radius, Color color)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.radius = radius;
            this.color = color;
            Recalc();
        }

        internal float rho
        {
            get { return rhoTheta.X; }
        }

        internal float theta
        {
            get { return rhoTheta.Y; }
        }


        internal void Move(Vector2 p1, Vector2 p2)
        {
            this.p1 = p1;
            this.p2 = p2;
            Recalc();
        }


        internal void Recalc()
        {
            Vector2 delta = p2 - p1;
            float rho = delta.Length;
            float theta = (float) Math.Atan2(delta.Y, delta.X);
            rhoTheta = new Vector2(rho, theta);
        }


        /// <summary>
        /// Distance squared from an arbitrary point p to this line.
        /// </summary>
        /// <remarks>
        /// See http://geometryalgorithms.com/Archive/algorithm_0102/algorithm_0102.htm, near the bottom.
        /// </remarks>
        internal float DistanceSquaredToPoint(Vector2 p)
        {
            Vector2 v = p2 - p1; // Vector from line's p1 to p2
            Vector2 w = p - p1; // Vector from line's p1 to p

            // See if p is closer to p1 than to the segment
            float c1 = Vector2.Dot(w, v);
            if (c1 <= 0)
                return pMathHelper.DistanceSquared(p, p1);

            // See if p is closer to p2 than to the segment
            float c2 = Vector2.Dot(v, v);
            if (c2 <= c1)
                return pMathHelper.DistanceSquared(p, p2);

            // p is closest to point pB, between p1 and p2
            float b = c1/c2;
            Vector2 pB = p1 + b*v;
            return pMathHelper.DistanceSquared(p, pB);
        }


        internal Matrix4 WorldMatrix()
        {
            // todo: Optimize. There should be no trig here.
            Matrix4 rotate = Matrix4.CreateRotationZ(theta);
            Matrix4 translate = Matrix4.CreateTranslation(p1.X, p1.Y, 0);
            // For some reason, in osu!m, I'm getting flipped results with the same world matrix as before.
            // Probably related to peppy's flipping the ortho matrix?
            Matrix4 flip = new Matrix4(-1, 0, 0, 0,
                                       0, -1, 0, 0,
                                       0, 0, 1, 0,
                                       0, 0, 0, 1);
            return flip * rotate * translate;
        }

        /// <summary>
        /// It's the end of the world as we know it
        /// </summary>
        internal Matrix4 EndWorldMatrix()
        {
            // todo: ^
            Matrix4 rotate = Matrix4.CreateRotationZ(theta);
            Matrix4 translate = Matrix4.CreateTranslation(p2.X, p2.Y, 0);
            Matrix4 flip = new Matrix4(-1, 0, 0, 0,
                                       0, -1, 0, 0,
                                       0, 0, 1, 0,
                                       0, 0, 0, 1);
            return flip * rotate * translate;
        }

    } ;
}