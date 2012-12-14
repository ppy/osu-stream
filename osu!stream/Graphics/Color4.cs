namespace OpenTK.Graphics
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Represents a color with 4 floating-point components (R, G, B, A).
    /// </summary>
    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct Color4 : IEquatable<Color4>
    {
        /// <summary>
        /// The red component of this Color4 structure.
        /// </summary>
        public float R;
        /// <summary>
        /// The green component of this Color4 structure.
        /// </summary>
        public float G;
        /// <summary>
        /// The blue component of this Color4 structure.
        /// </summary>
        public float B;
        /// <summary>
        /// The alpha component of this Color4 structure.
        /// </summary>
        public float A;
        /// <summary>
        /// Constructs a new Color4 structure from the specified components.
        /// </summary>
        /// <param name="r">The red component of the new Color4 structure.</param>
        /// <param name="g">The green component of the new Color4 structure.</param>
        /// <param name="b">The blue component of the new Color4 structure.</param>
        /// <param name="a">The alpha component of the new Color4 structure.</param>
        public Color4(float r, float g, float b, float a)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }

        /// <summary>
        /// Constructs a new Color4 structure from the specified components.
        /// </summary>
        /// <param name="r">The red component of the new Color4 structure.</param>
        /// <param name="g">The green component of the new Color4 structure.</param>
        /// <param name="b">The blue component of the new Color4 structure.</param>
        /// <param name="a">The alpha component of the new Color4 structure.</param>
        public Color4(byte r, byte g, byte b, byte a)
        {
            this.R = ((float) r) / 255f;
            this.G = ((float) g) / 255f;
            this.B = ((float) b) / 255f;
            this.A = ((float) a) / 255f;
        }

        /// <summary>
        /// Constructs a new Color4 structure from the specified System.Drawing.Color.
        /// </summary>
        /// <param name="color">The System.Drawing.Color containing the component values.</param>
        [Obsolete("Use new Color4(r, g, b, a) instead.")]
        public Color4(Color color) : this(color.R, color.G, color.B, color.A)
        {
        }

        /// <summary>
        /// Converts this color to an integer representation with 8 bits per channel.
        /// </summary>
        /// <returns>A <see cref="T:System.Int32" /> that represents this instance.</returns>
        /// <remarks>This method is intended only for compatibility with System.Drawing. It compresses the color into 8 bits per channel, which means color information is lost.</remarks>
        public int ToArgb()
        {
            uint num = (((((uint) (this.A * 255f)) << 0x18) | (((uint) (this.R * 255f)) << 0x10)) | (((uint) (this.G * 255f)) << 8)) | ((uint) (this.B * 255f));
            return (int) num;
        }

        /// <summary>
        /// Compares whether this Color4 structure is equal to the specified object.
        /// </summary>
        /// <param name="obj">An object to compare to.</param>
        /// <returns>True obj is a Color4 structure with the same components as this Color4; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            return ((obj is Color4) && this.Equals((Color4) obj));
        }

        /// <summary>
        /// Calculates the hash code for this Color4 structure.
        /// </summary>
        /// <returns>A System.Int32 containing the hashcode of this Color4 structure.</returns>
        public override int GetHashCode()
        {
            return this.ToArgb();
        }

        /// <summary>
        /// Creates a System.String that describes this Color4 structure.
        /// </summary>
        /// <returns>A System.String that describes this Color4 structure.</returns>
        public override string ToString()
        {
            object[] args = new object[] { this.R.ToString(), this.G.ToString(), this.B.ToString(), this.A.ToString() };
            return string.Format("{{(R, G, B, A) = ({0}, {1}, {2}, {3})}}", args);
        }

        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 255, 255, 0).
        /// </summary>
        public static Color4 Transparent
        {
            get
            {
                return new Color4(0xff, 0xff, 0xff, 0);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (240, 248, 255, 255).
        /// </summary>
        public static Color4 AliceBlue
        {
            get
            {
                return new Color4(240, 0xf8, 0xff, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (250, 235, 215, 255).
        /// </summary>
        public static Color4 AntiqueWhite
        {
            get
            {
                return new Color4(250, 0xeb, 0xd7, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (0, 255, 255, 255).
        /// </summary>
        public static Color4 Aqua
        {
            get
            {
                return new Color4(0, 0xff, 0xff, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (127, 255, 212, 255).
        /// </summary>
        public static Color4 Aquamarine
        {
            get
            {
                return new Color4(0x7f, 0xff, 0xd4, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (240, 255, 255, 255).
        /// </summary>
        public static Color4 Azure
        {
            get
            {
                return new Color4(240, 0xff, 0xff, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (245, 245, 220, 255).
        /// </summary>
        public static Color4 Beige
        {
            get
            {
                return new Color4(0xf5, 0xf5, 220, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 228, 196, 255).
        /// </summary>
        public static Color4 Bisque
        {
            get
            {
                return new Color4(0xff, 0xe4, 0xc4, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (0, 0, 0, 255).
        /// </summary>
        public static Color4 Black
        {
            get
            {
                return new Color4(0, 0, 0, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 235, 205, 255).
        /// </summary>
        public static Color4 BlanchedAlmond
        {
            get
            {
                return new Color4(0xff, 0xeb, 0xcd, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (0, 0, 255, 255).
        /// </summary>
        public static Color4 Blue
        {
            get
            {
                return new Color4(0, 0, 0xff, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (138, 43, 226, 255).
        /// </summary>
        public static Color4 BlueViolet
        {
            get
            {
                return new Color4(0x8a, 0x2b, 0xe2, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (165, 42, 42, 255).
        /// </summary>
        public static Color4 Brown
        {
            get
            {
                return new Color4(0xa5, 0x2a, 0x2a, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (222, 184, 135, 255).
        /// </summary>
        public static Color4 BurlyWood
        {
            get
            {
                return new Color4(0xde, 0xb8, 0x87, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (95, 158, 160, 255).
        /// </summary>
        public static Color4 CadetBlue
        {
            get
            {
                return new Color4(0x5f, 0x9e, 160, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (127, 255, 0, 255).
        /// </summary>
        public static Color4 Chartreuse
        {
            get
            {
                return new Color4(0x7f, 0xff, 0, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (210, 105, 30, 255).
        /// </summary>
        public static Color4 Chocolate
        {
            get
            {
                return new Color4(210, 0x69, 30, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 127, 80, 255).
        /// </summary>
        public static Color4 Coral
        {
            get
            {
                return new Color4(0xff, 0x7f, 80, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (100, 149, 237, 255).
        /// </summary>
        public static Color4 CornflowerBlue
        {
            get
            {
                return new Color4(100, 0x95, 0xed, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 248, 220, 255).
        /// </summary>
        public static Color4 Cornsilk
        {
            get
            {
                return new Color4(0xff, 0xf8, 220, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (220, 20, 60, 255).
        /// </summary>
        public static Color4 Crimson
        {
            get
            {
                return new Color4(220, 20, 60, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (0, 255, 255, 255).
        /// </summary>
        public static Color4 Cyan
        {
            get
            {
                return new Color4(0, 0xff, 0xff, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (0, 0, 139, 255).
        /// </summary>
        public static Color4 DarkBlue
        {
            get
            {
                return new Color4(0, 0, 0x8b, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (0, 139, 139, 255).
        /// </summary>
        public static Color4 DarkCyan
        {
            get
            {
                return new Color4(0, 0x8b, 0x8b, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (184, 134, 11, 255).
        /// </summary>
        public static Color4 DarkGoldenrod
        {
            get
            {
                return new Color4(0xb8, 0x86, 11, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (169, 169, 169, 255).
        /// </summary>
        public static Color4 DarkGray
        {
            get
            {
                return new Color4(0xa9, 0xa9, 0xa9, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (0, 100, 0, 255).
        /// </summary>
        public static Color4 DarkGreen
        {
            get
            {
                return new Color4(0, 100, 0, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (189, 183, 107, 255).
        /// </summary>
        public static Color4 DarkKhaki
        {
            get
            {
                return new Color4(0xbd, 0xb7, 0x6b, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (139, 0, 139, 255).
        /// </summary>
        public static Color4 DarkMagenta
        {
            get
            {
                return new Color4(0x8b, 0, 0x8b, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (85, 107, 47, 255).
        /// </summary>
        public static Color4 DarkOliveGreen
        {
            get
            {
                return new Color4(0x55, 0x6b, 0x2f, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 140, 0, 255).
        /// </summary>
        public static Color4 DarkOrange
        {
            get
            {
                return new Color4(0xff, 140, 0, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (153, 50, 204, 255).
        /// </summary>
        public static Color4 DarkOrchid
        {
            get
            {
                return new Color4(0x99, 50, 0xcc, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (139, 0, 0, 255).
        /// </summary>
        public static Color4 DarkRed
        {
            get
            {
                return new Color4(0x8b, 0, 0, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (233, 150, 122, 255).
        /// </summary>
        public static Color4 DarkSalmon
        {
            get
            {
                return new Color4(0xe9, 150, 0x7a, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (143, 188, 139, 255).
        /// </summary>
        public static Color4 DarkSeaGreen
        {
            get
            {
                return new Color4(0x8f, 0xbc, 0x8b, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (72, 61, 139, 255).
        /// </summary>
        public static Color4 DarkSlateBlue
        {
            get
            {
                return new Color4(0x48, 0x3d, 0x8b, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (47, 79, 79, 255).
        /// </summary>
        public static Color4 DarkSlateGray
        {
            get
            {
                return new Color4(0x2f, 0x4f, 0x4f, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (0, 206, 209, 255).
        /// </summary>
        public static Color4 DarkTurquoise
        {
            get
            {
                return new Color4(0, 0xce, 0xd1, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (148, 0, 211, 255).
        /// </summary>
        public static Color4 DarkViolet
        {
            get
            {
                return new Color4(0x94, 0, 0xd3, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 20, 147, 255).
        /// </summary>
        public static Color4 DeepPink
        {
            get
            {
                return new Color4(0xff, 20, 0x93, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (0, 191, 255, 255).
        /// </summary>
        public static Color4 DeepSkyBlue
        {
            get
            {
                return new Color4(0, 0xbf, 0xff, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (105, 105, 105, 255).
        /// </summary>
        public static Color4 DimGray
        {
            get
            {
                return new Color4(0x69, 0x69, 0x69, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (30, 144, 255, 255).
        /// </summary>
        public static Color4 DodgerBlue
        {
            get
            {
                return new Color4(30, 0x90, 0xff, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (178, 34, 34, 255).
        /// </summary>
        public static Color4 Firebrick
        {
            get
            {
                return new Color4(0xb2, 0x22, 0x22, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 250, 240, 255).
        /// </summary>
        public static Color4 FloralWhite
        {
            get
            {
                return new Color4(0xff, 250, 240, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (34, 139, 34, 255).
        /// </summary>
        public static Color4 ForestGreen
        {
            get
            {
                return new Color4(0x22, 0x8b, 0x22, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 0, 255, 255).
        /// </summary>
        public static Color4 Fuchsia
        {
            get
            {
                return new Color4(0xff, 0, 0xff, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (220, 220, 220, 255).
        /// </summary>
        public static Color4 Gainsboro
        {
            get
            {
                return new Color4(220, 220, 220, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (248, 248, 255, 255).
        /// </summary>
        public static Color4 GhostWhite
        {
            get
            {
                return new Color4(0xf8, 0xf8, 0xff, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 215, 0, 255).
        /// </summary>
        public static Color4 Gold
        {
            get
            {
                return new Color4(0xff, 0xd7, 0, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (218, 165, 32, 255).
        /// </summary>
        public static Color4 Goldenrod
        {
            get
            {
                return new Color4(0xda, 0xa5, 0x20, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (128, 128, 128, 255).
        /// </summary>
        public static Color4 Gray
        {
            get
            {
                return new Color4(0x80, 0x80, 0x80, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (0, 128, 0, 255).
        /// </summary>
        public static Color4 Green
        {
            get
            {
                return new Color4(0, 0x80, 0, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (173, 255, 47, 255).
        /// </summary>
        public static Color4 GreenYellow
        {
            get
            {
                return new Color4(0xad, 0xff, 0x2f, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (240, 255, 240, 255).
        /// </summary>
        public static Color4 Honeydew
        {
            get
            {
                return new Color4(240, 0xff, 240, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 105, 180, 255).
        /// </summary>
        public static Color4 HotPink
        {
            get
            {
                return new Color4(0xff, 0x69, 180, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (205, 92, 92, 255).
        /// </summary>
        public static Color4 IndianRed
        {
            get
            {
                return new Color4(0xcd, 0x5c, 0x5c, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (75, 0, 130, 255).
        /// </summary>
        public static Color4 Indigo
        {
            get
            {
                return new Color4(0x4b, 0, 130, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 255, 240, 255).
        /// </summary>
        public static Color4 Ivory
        {
            get
            {
                return new Color4(0xff, 0xff, 240, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (240, 230, 140, 255).
        /// </summary>
        public static Color4 Khaki
        {
            get
            {
                return new Color4(240, 230, 140, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (230, 230, 250, 255).
        /// </summary>
        public static Color4 Lavender
        {
            get
            {
                return new Color4(230, 230, 250, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 240, 245, 255).
        /// </summary>
        public static Color4 LavenderBlush
        {
            get
            {
                return new Color4(0xff, 240, 0xf5, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (124, 252, 0, 255).
        /// </summary>
        public static Color4 LawnGreen
        {
            get
            {
                return new Color4(0x7c, 0xfc, 0, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 250, 205, 255).
        /// </summary>
        public static Color4 LemonChiffon
        {
            get
            {
                return new Color4(0xff, 250, 0xcd, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (173, 216, 230, 255).
        /// </summary>
        public static Color4 LightBlue
        {
            get
            {
                return new Color4(0xad, 0xd8, 230, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (240, 128, 128, 255).
        /// </summary>
        public static Color4 LightCoral
        {
            get
            {
                return new Color4(240, 0x80, 0x80, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (224, 255, 255, 255).
        /// </summary>
        public static Color4 LightCyan
        {
            get
            {
                return new Color4(0xe0, 0xff, 0xff, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (250, 250, 210, 255).
        /// </summary>
        public static Color4 LightGoldenrodYellow
        {
            get
            {
                return new Color4(250, 250, 210, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (144, 238, 144, 255).
        /// </summary>
        public static Color4 LightGreen
        {
            get
            {
                return new Color4(0x90, 0xee, 0x90, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (211, 211, 211, 255).
        /// </summary>
        public static Color4 LightGray
        {
            get
            {
                return new Color4(0xd3, 0xd3, 0xd3, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 182, 193, 255).
        /// </summary>
        public static Color4 LightPink
        {
            get
            {
                return new Color4(0xff, 0xb6, 0xc1, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 160, 122, 255).
        /// </summary>
        public static Color4 LightSalmon
        {
            get
            {
                return new Color4(0xff, 160, 0x7a, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (32, 178, 170, 255).
        /// </summary>
        public static Color4 LightSeaGreen
        {
            get
            {
                return new Color4(0x20, 0xb2, 170, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (135, 206, 250, 255).
        /// </summary>
        public static Color4 LightSkyBlue
        {
            get
            {
                return new Color4(0x87, 0xce, 250, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (119, 136, 153, 255).
        /// </summary>
        public static Color4 LightSlateGray
        {
            get
            {
                return new Color4(0x77, 0x88, 0x99, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (176, 196, 222, 255).
        /// </summary>
        public static Color4 LightSteelBlue
        {
            get
            {
                return new Color4(0xb0, 0xc4, 0xde, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 255, 224, 255).
        /// </summary>
        public static Color4 LightYellow
        {
            get
            {
                return new Color4(0xff, 0xff, 0xe0, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (0, 255, 0, 255).
        /// </summary>
        public static Color4 Lime
        {
            get
            {
                return new Color4(0, 0xff, 0, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (50, 205, 50, 255).
        /// </summary>
        public static Color4 LimeGreen
        {
            get
            {
                return new Color4(50, 0xcd, 50, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (250, 240, 230, 255).
        /// </summary>
        public static Color4 Linen
        {
            get
            {
                return new Color4(250, 240, 230, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 0, 255, 255).
        /// </summary>
        public static Color4 Magenta
        {
            get
            {
                return new Color4(0xff, 0, 0xff, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (128, 0, 0, 255).
        /// </summary>
        public static Color4 Maroon
        {
            get
            {
                return new Color4(0x80, 0, 0, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (102, 205, 170, 255).
        /// </summary>
        public static Color4 MediumAquamarine
        {
            get
            {
                return new Color4(0x66, 0xcd, 170, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (0, 0, 205, 255).
        /// </summary>
        public static Color4 MediumBlue
        {
            get
            {
                return new Color4(0, 0, 0xcd, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (186, 85, 211, 255).
        /// </summary>
        public static Color4 MediumOrchid
        {
            get
            {
                return new Color4(0xba, 0x55, 0xd3, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (147, 112, 219, 255).
        /// </summary>
        public static Color4 MediumPurple
        {
            get
            {
                return new Color4(0x93, 0x70, 0xdb, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (60, 179, 113, 255).
        /// </summary>
        public static Color4 MediumSeaGreen
        {
            get
            {
                return new Color4(60, 0xb3, 0x71, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (123, 104, 238, 255).
        /// </summary>
        public static Color4 MediumSlateBlue
        {
            get
            {
                return new Color4(0x7b, 0x68, 0xee, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (0, 250, 154, 255).
        /// </summary>
        public static Color4 MediumSpringGreen
        {
            get
            {
                return new Color4(0, 250, 0x9a, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (72, 209, 204, 255).
        /// </summary>
        public static Color4 MediumTurquoise
        {
            get
            {
                return new Color4(0x48, 0xd1, 0xcc, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (199, 21, 133, 255).
        /// </summary>
        public static Color4 MediumVioletRed
        {
            get
            {
                return new Color4(0xc7, 0x15, 0x85, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (25, 25, 112, 255).
        /// </summary>
        public static Color4 MidnightBlue
        {
            get
            {
                return new Color4(0x19, 0x19, 0x70, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (245, 255, 250, 255).
        /// </summary>
        public static Color4 MintCream
        {
            get
            {
                return new Color4(0xf5, 0xff, 250, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 228, 225, 255).
        /// </summary>
        public static Color4 MistyRose
        {
            get
            {
                return new Color4(0xff, 0xe4, 0xe1, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 228, 181, 255).
        /// </summary>
        public static Color4 Moccasin
        {
            get
            {
                return new Color4(0xff, 0xe4, 0xb5, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 222, 173, 255).
        /// </summary>
        public static Color4 NavajoWhite
        {
            get
            {
                return new Color4(0xff, 0xde, 0xad, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (0, 0, 128, 255).
        /// </summary>
        public static Color4 Navy
        {
            get
            {
                return new Color4(0, 0, 0x80, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (253, 245, 230, 255).
        /// </summary>
        public static Color4 OldLace
        {
            get
            {
                return new Color4(0xfd, 0xf5, 230, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (128, 128, 0, 255).
        /// </summary>
        public static Color4 Olive
        {
            get
            {
                return new Color4(0x80, 0x80, 0, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (107, 142, 35, 255).
        /// </summary>
        public static Color4 OliveDrab
        {
            get
            {
                return new Color4(0x6b, 0x8e, 0x23, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 165, 0, 255).
        /// </summary>
        public static Color4 Orange
        {
            get
            {
                return new Color4(0xff, 0xa5, 0, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 69, 0, 255).
        /// </summary>
        public static Color4 OrangeRed
        {
            get
            {
                return new Color4(0xff, 0x45, 0, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (218, 112, 214, 255).
        /// </summary>
        public static Color4 Orchid
        {
            get
            {
                return new Color4(0xda, 0x70, 0xd6, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (238, 232, 170, 255).
        /// </summary>
        public static Color4 PaleGoldenrod
        {
            get
            {
                return new Color4(0xee, 0xe8, 170, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (152, 251, 152, 255).
        /// </summary>
        public static Color4 PaleGreen
        {
            get
            {
                return new Color4(0x98, 0xfb, 0x98, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (175, 238, 238, 255).
        /// </summary>
        public static Color4 PaleTurquoise
        {
            get
            {
                return new Color4(0xaf, 0xee, 0xee, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (219, 112, 147, 255).
        /// </summary>
        public static Color4 PaleVioletRed
        {
            get
            {
                return new Color4(0xdb, 0x70, 0x93, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 239, 213, 255).
        /// </summary>
        public static Color4 PapayaWhip
        {
            get
            {
                return new Color4(0xff, 0xef, 0xd5, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 218, 185, 255).
        /// </summary>
        public static Color4 PeachPuff
        {
            get
            {
                return new Color4(0xff, 0xda, 0xb9, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (205, 133, 63, 255).
        /// </summary>
        public static Color4 Peru
        {
            get
            {
                return new Color4(0xcd, 0x85, 0x3f, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 192, 203, 255).
        /// </summary>
        public static Color4 Pink
        {
            get
            {
                return new Color4(0xff, 0xc0, 0xcb, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (221, 160, 221, 255).
        /// </summary>
        public static Color4 Plum
        {
            get
            {
                return new Color4(0xdd, 160, 0xdd, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (176, 224, 230, 255).
        /// </summary>
        public static Color4 PowderBlue
        {
            get
            {
                return new Color4(0xb0, 0xe0, 230, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (128, 0, 128, 255).
        /// </summary>
        public static Color4 Purple
        {
            get
            {
                return new Color4(0x80, 0, 0x80, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 0, 0, 255).
        /// </summary>
        public static Color4 Red
        {
            get
            {
                return new Color4(0xff, 0, 0, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (188, 143, 143, 255).
        /// </summary>
        public static Color4 RosyBrown
        {
            get
            {
                return new Color4(0xbc, 0x8f, 0x8f, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (65, 105, 225, 255).
        /// </summary>
        public static Color4 RoyalBlue
        {
            get
            {
                return new Color4(0x41, 0x69, 0xe1, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (139, 69, 19, 255).
        /// </summary>
        public static Color4 SaddleBrown
        {
            get
            {
                return new Color4(0x8b, 0x45, 0x13, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (250, 128, 114, 255).
        /// </summary>
        public static Color4 Salmon
        {
            get
            {
                return new Color4(250, 0x80, 0x72, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (244, 164, 96, 255).
        /// </summary>
        public static Color4 SandyBrown
        {
            get
            {
                return new Color4(0xf4, 0xa4, 0x60, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (46, 139, 87, 255).
        /// </summary>
        public static Color4 SeaGreen
        {
            get
            {
                return new Color4(0x2e, 0x8b, 0x57, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 245, 238, 255).
        /// </summary>
        public static Color4 SeaShell
        {
            get
            {
                return new Color4(0xff, 0xf5, 0xee, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (160, 82, 45, 255).
        /// </summary>
        public static Color4 Sienna
        {
            get
            {
                return new Color4(160, 0x52, 0x2d, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (192, 192, 192, 255).
        /// </summary>
        public static Color4 Silver
        {
            get
            {
                return new Color4(0xc0, 0xc0, 0xc0, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (135, 206, 235, 255).
        /// </summary>
        public static Color4 SkyBlue
        {
            get
            {
                return new Color4(0x87, 0xce, 0xeb, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (106, 90, 205, 255).
        /// </summary>
        public static Color4 SlateBlue
        {
            get
            {
                return new Color4(0x6a, 90, 0xcd, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (112, 128, 144, 255).
        /// </summary>
        public static Color4 SlateGray
        {
            get
            {
                return new Color4(0x70, 0x80, 0x90, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 250, 250, 255).
        /// </summary>
        public static Color4 Snow
        {
            get
            {
                return new Color4(0xff, 250, 250, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (0, 255, 127, 255).
        /// </summary>
        public static Color4 SpringGreen
        {
            get
            {
                return new Color4(0, 0xff, 0x7f, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (70, 130, 180, 255).
        /// </summary>
        public static Color4 SteelBlue
        {
            get
            {
                return new Color4(70, 130, 180, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (210, 180, 140, 255).
        /// </summary>
        public static Color4 Tan
        {
            get
            {
                return new Color4(210, 180, 140, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (0, 128, 128, 255).
        /// </summary>
        public static Color4 Teal
        {
            get
            {
                return new Color4(0, 0x80, 0x80, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (216, 191, 216, 255).
        /// </summary>
        public static Color4 Thistle
        {
            get
            {
                return new Color4(0xd8, 0xbf, 0xd8, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 99, 71, 255).
        /// </summary>
        public static Color4 Tomato
        {
            get
            {
                return new Color4(0xff, 0x63, 0x47, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (64, 224, 208, 255).
        /// </summary>
        public static Color4 Turquoise
        {
            get
            {
                return new Color4(0x40, 0xe0, 0xd0, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (238, 130, 238, 255).
        /// </summary>
        public static Color4 Violet
        {
            get
            {
                return new Color4(0xee, 130, 0xee, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (245, 222, 179, 255).
        /// </summary>
        public static Color4 Wheat
        {
            get
            {
                return new Color4(0xf5, 0xde, 0xb3, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 255, 255, 255).
        /// </summary>
        public static Color4 White
        {
            get
            {
                return new Color4(0xff, 0xff, 0xff, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (245, 245, 245, 255).
        /// </summary>
        public static Color4 WhiteSmoke
        {
            get
            {
                return new Color4(0xf5, 0xf5, 0xf5, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (255, 255, 0, 255).
        /// </summary>
        public static Color4 Yellow
        {
            get
            {
                return new Color4(0xff, 0xff, 0, 0xff);
            }
        }
        /// <summary>
        /// Gets the system color with (R, G, B, A) = (154, 205, 50, 255).
        /// </summary>
        public static Color4 YellowGreen
        {
            get
            {
                return new Color4(0x9a, 0xcd, 50, 0xff);
            }
        }
        /// <summary>
        /// Compares whether this Color4 structure is equal to the specified Color4.
        /// </summary>
        /// <param name="other">The Color4 structure to compare to.</param>
        /// <returns>True if both Color4 structures contain the same components; false otherwise.</returns>
        public bool Equals(Color4 other)
        {
            return ((((this.R == other.R) && (this.G == other.G)) && (this.B == other.B)) && (this.A == other.A));
        }

        /// <summary>
        /// Compares the specified Color4 structures for equality.
        /// </summary>
        /// <param name="left">The left-hand side of the comparison.</param>
        /// <param name="right">The right-hand side of the comparison.</param>
        /// <returns>True if left is equal to right; false otherwise.</returns>
        public static bool operator ==(Color4 left, Color4 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares the specified Color4 structures for inequality.
        /// </summary>
        /// <param name="left">The left-hand side of the comparison.</param>
        /// <param name="right">The right-hand side of the comparison.</param>
        /// <returns>True if left is not equal to right; false otherwise.</returns>
        public static bool operator !=(Color4 left, Color4 right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Converts the specified System.Drawing.Color to a Color4 structure.
        /// </summary>
        /// <param name="color">The System.Drawing.Color to convert.</param>
        /// <returns>A new Color4 structure containing the converted components.</returns>
        public static implicit operator Color4(Color color)
        {
            return new Color4(color.R, color.G, color.B, color.A);
        }

        /// <summary>
        /// Converts the specified Color4 to a System.Drawing.Color structure.
        /// </summary>
        /// <param name="color">The Color4 to convert.</param>
        /// <returns>A new System.Drawing.Color structure containing the converted components.</returns>
        public static explicit operator Color(Color4 color)
        {
            return Color.FromArgb((int) (color.A * 255f), (int) (color.R * 255f), (int) (color.G * 255f), (int) (color.B * 255f));
        }
    }
}

