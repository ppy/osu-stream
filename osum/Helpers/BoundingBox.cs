using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace osum.Helpers
{
    internal class BoundingBox
    {
        private bool has_points = false;
        private float x_min, x_max, y_min, y_max;

        internal BoundingBox()
        {
            has_points = false;
        }

        internal BoundingBox(params Vector2[] points)
            : this()
        {
            foreach (Vector2 point in points)
            {
                Add(point);
            }
        }

        internal void Reset()
        {
            has_points = false;
        }

        internal float? XMin
        {
            get
            {
                return has_points ? x_min : (float?)null;
            }
        }

        internal float? XMax
        {
            get
            {
                return has_points ? x_max : (float?)null;
            }
        }

        internal float? YMin
        {
            get
            {
                return has_points ? y_min : (float?)null;
            }
        }

        internal float? YMax
        {
            get
            {
                return has_points ? y_max : (float?)null;
            }
        }

        internal float? Width
        {
            get
            {
                return has_points ? (x_max - x_min) : (float?)null;
            }
        }

        internal float? Height
        {
            get
            {
                return has_points ? (y_max - y_min) : (float?)null;
            }
        }

        internal void Add(Vector2 point)
        {
            if (has_points)
            {
                x_min = Math.Min(x_min, point.X);
                x_max = Math.Max(x_max, point.X);
                y_min = Math.Min(y_min, point.Y);
                y_max = Math.Max(y_max, point.Y);
            }
            else
            {
                x_min = point.X;
                x_max = point.X;
                y_min = point.Y;
                y_max = point.Y;
                has_points = true;
            }
        }
    }
}
