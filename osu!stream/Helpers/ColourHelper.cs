using System;
using System.Drawing;
using OpenTK.Graphics;

namespace osum.Helpers
{
    internal static class ColourHelper
    {
        /// <summary>
        /// Returns a lightened version of the colour.
        /// </summary>
        /// <param name="color">Original colour</param>
        /// <param name="amount">Decimal light addition</param>
        /// <returns></returns>
        internal static Color4 Lighten(Color4 color, float amount)
        {
            return new Color4(
                Math.Min(1.0f, color.R * (1 + amount)),
                Math.Min(1.0f, color.G * (1 + amount)),
                Math.Min(1.0f, color.B * (1 + amount)),
                color.A);
        }

        /// <summary>
        /// Lightens a colour in a way more friendly to dark or strong colours.
        /// </summary>
        internal static Color4 Lighten2(Color4 color, float amount)
        {
            amount *= 0.5f;
            return new Color4(
                Math.Min(1.0f, color.R * (1 + 0.5f * amount) + amount),
                Math.Min(1.0f, color.G * (1 + 0.5f * amount) + amount),
                Math.Min(1.0f, color.B * (1 + 0.5f * amount) + amount),
                color.A);
        }

        /// <summary>
        /// Returns a darkened version of the colour.
        /// </summary>
        /// <param name="color">Original colour</param>
        /// <param name="amount">Percentage light reduction</param>
        /// <returns></returns>
        internal static Color4 Darken(Color4 color, float amount)
        {
            return new Color4(
                Math.Min(1.0f, color.R * (1 - amount)),
                Math.Min(1.0f, color.G * (1 - amount)),
                Math.Min(1.0f, color.B * (1 - amount)),
                color.A);
        }

        /// <summary>
        /// Hurr derp
        /// </summary>
        internal static Color4 ColourLerp(Color4 first, Color4 second, float weight)
        {
            return new Color4((byte)pMathHelper.Lerp(first.R, second.R, weight),
                             (byte)pMathHelper.Lerp(first.G, second.G, weight),
                             (byte)pMathHelper.Lerp(first.B, second.B, weight),
                             (byte)pMathHelper.Lerp(first.A, second.A, weight));
        }

        internal static Color CConvert(Color4 c)
        {
            return Color.FromArgb((int)(c.A * 255), (int)(c.R * 255), (int)(c.G * 255), (int)(c.B * 255));
        }

    }
}
