using System;
using System.Collections.Generic;
using System.Text;
using Color = OpenTK.Graphics.Color4;
using osum.Helpers;

namespace osu.Helpers
{
    internal static class ColourHelper
    {
        /// <summary>
        /// Returns a lightened version of the colour.
        /// </summary>
        /// <param name="color">Original colour</param>
        /// <param name="amount">Decimal light addition</param>
        /// <returns></returns>
        internal static Color Lighten(Color color, float amount)
        {
            return new Color(
                (byte)Math.Min(1, color.R * (1+amount)),
                (byte)Math.Min(1, color.G * (1+amount)),
                (byte)Math.Min(1, color.B * (1+amount)),
                color.A);
        }

        /// <summary>
        /// Lightens a colour in a way more friendly to dark or strong colours.
        /// </summary>
        internal static Color Lighten2(Color color, float amount)
        {
            amount *= 0.5f;
            return new Color(
                (byte)Math.Min(1, color.R * (1 + 0.5f * amount) + amount),
                (byte)Math.Min(1, color.G * (1 + 0.5f * amount) + amount),
                (byte)Math.Min(1, color.B * (1 + 0.5f * amount) + amount),
                color.A);
        }

        /// <summary>
        /// Returns a darkened version of the colour.
        /// </summary>
        /// <param name="color">Original colour</param>
        /// <param name="amount">Percentage light reduction</param>
        /// <returns></returns>
        internal static Color Darken(Color color, float amount)
        {
            return new Color(
                (byte)Math.Min(1, color.R * (1-amount)),
                (byte)Math.Min(1, color.G * (1-amount)),
                (byte)Math.Min(1, color.B * (1-amount)),
                color.A);
        }

        /// <summary>
        /// Hurr derp
        /// </summary>
        internal static Color ColourLerp(Color first, Color second, float weight)
        {
            return new Color((byte)pMathHelper.Lerp((float)first.R, (float)second.R, weight),
                             (byte)pMathHelper.Lerp((float)first.G, (float)second.G, weight),
                             (byte)pMathHelper.Lerp((float)first.B, (float)second.B, weight),
                             (byte)pMathHelper.Lerp((float)first.A, (float)second.A, weight));
        }

    }
}
