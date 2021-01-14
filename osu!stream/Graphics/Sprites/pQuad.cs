#if iOS || ANDROID
using OpenTK.Graphics.ES11;
#if iOS
using Foundation;
using ObjCRuntime;
using OpenGLES;
#endif
using TextureTarget = OpenTK.Graphics.ES11.All;
using TextureParameterName = OpenTK.Graphics.ES11.All;
using EnableCap = OpenTK.Graphics.ES11.All;
using ArrayCap = OpenTK.Graphics.ES11.All;
using BlendingFactorSrc = OpenTK.Graphics.ES11.All;
using BlendingFactorDest = OpenTK.Graphics.ES11.All;
using PixelStoreParameter = OpenTK.Graphics.ES11.All;
using VertexPointerType = OpenTK.Graphics.ES11.All;
using ColorPointerType = OpenTK.Graphics.ES11.All;
using ClearBufferMask = OpenTK.Graphics.ES11.All;
using TexCoordPointerType = OpenTK.Graphics.ES11.All;
using BeginMode = OpenTK.Graphics.ES11.All;
using MatrixMode = OpenTK.Graphics.ES11.All;
using PixelInternalFormat = OpenTK.Graphics.ES11.All;
using PixelFormat = OpenTK.Graphics.ES11.All;
using PixelType = OpenTK.Graphics.ES11.All;
using ShaderType = OpenTK.Graphics.ES11.All;
using VertexAttribPointerType = OpenTK.Graphics.ES11.All;
using ProgramParameter = OpenTK.Graphics.ES11.All;
using ShaderParameter = OpenTK.Graphics.ES11.All;
using ErrorCode = OpenTK.Graphics.ES11.All;
using TextureEnvParameter = OpenTK.Graphics.ES11.All;
using TextureEnvTarget = OpenTK.Graphics.ES11.All;
#else
using OpenTK.Graphics.OpenGL;
#endif
using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using osum.Helpers;


namespace osum.Graphics.Sprites
{
    internal class pQuad : pDrawable
    {
        public Vector2 p1, p2, p3, p4;
        public pQuad(Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight, bool alwaysDraw, float drawDepth, Color4 colour)
        {
            AlwaysDraw = alwaysDraw;
            Alpha = alwaysDraw ? 1 : 0;
            DrawDepth = drawDepth;
            Colour = colour;
            Clocking = ClockTypes.Mode;
            Field = FieldTypes.Standard;

            p1 = topLeft;
            p2 = topRight;
            p3 = bottomLeft;
            p4 = bottomRight;

#if !NO_PIN_SUPPORT
            vertices = new Vector2[4];
            colours = new Color4[4];

            handle_vertices = GCHandle.Alloc(vertices, GCHandleType.Pinned);
            handle_colours = GCHandle.Alloc(colours, GCHandleType.Pinned);

            handle_vertices_pointer = handle_vertices.AddrOfPinnedObject();
            handle_colours_pointer = handle_colours.AddrOfPinnedObject();
#else
            unsafe
            {
                handle_vertices_pointer = Marshal.AllocHGlobal(4 * sizeof(Vector2));
                handle_colours_pointer = Marshal.AllocHGlobal(4 * sizeof(Color4));
            }
#endif
        }
#if !NO_PIN_SUPPORT
        private float[] coordinates;
        private readonly Vector2[] vertices;
        private readonly Color4[] colours;


        private GCHandle handle_vertices;
        private GCHandle handle_coordinates;
        private GCHandle handle_colours;
#endif
        private IntPtr handle_vertices_pointer;
        private IntPtr handle_coordinates_pointer;
        private readonly IntPtr handle_colours_pointer;


        public Color4[] Colours;
        public bool IsDisposed { get; private set; }

        public override void Dispose()
        {
            if (IsDisposed)
                return;
#if !NO_PIN_SUPPORT
            if (coordinates != null) handle_coordinates.Free();
            handle_colours.Free();
            handle_vertices.Free();
#else
            if (handle_coordinates_pointer != IntPtr.Zero)
                Marshal.FreeHGlobal(handle_coordinates_pointer);

            Marshal.FreeHGlobal(handle_vertices_pointer);
            Marshal.FreeHGlobal(handle_colours_pointer);
#endif
            IsDisposed = true;
            base.Dispose();
        }


        public pTexture Texture;

        protected override bool checkHover(Vector2 position)
        {
            unsafe
            {
                return PointInPolygon(position * GameBase.BaseToNativeRatio,
                    (Vector2*)handle_vertices_pointer.ToPointer(), 4);
            }
        }

        private static unsafe bool PointInPolygon(Vector2 p, Vector2* poly, int length)
        {
            Vector2 p1, p2;

            bool inside = false;

            if (length < 3)
            {
                return inside;
            }

            Vector2 oldVector2 = new Vector2(
                poly[length - 1].X, poly[length - 1].Y);

            for (int i = 0; i < length; i++)
            {
                Vector2 newVector2 = new Vector2(poly[i].X, poly[i].Y);

                if (newVector2.X > oldVector2.X)
                {
                    p1 = oldVector2;
                    p2 = newVector2;
                }
                else
                {
                    p1 = newVector2;
                    p2 = oldVector2;
                }

                if ((newVector2.X < p.X) == (p.X <= oldVector2.X)
                    && ((long)p.Y - (long)p1.Y) * (long)(p2.X - p1.X)
                    < ((long)p2.Y - (long)p1.Y) * (long)(p.X - p1.X))
                {
                    inside = !inside;
                }

                oldVector2 = newVector2;
            }

            return inside;
        }

        public override bool Draw()
        {
            if (base.Draw())
            {
                Color4 c = AlphaAppliedColour;
                Vector2 pos = FieldPosition;
                Vector2 scale = FieldScale;
                Vector2 origin = OriginVector * GameBase.BaseToNativeRatio;

                if (Colours == null)
                    SpriteManager.SetColour(c);
                else
                {
#if NO_PIN_SUPPORT
                        Color4* colours = (Color4*)handle_colours_pointer.ToPointer();
#endif
                    for (int i = 0; i < Colours.Length; i++)
                    {
                        Color4 col = Colours[i];

                        if (SpriteManager.UniversalDim > 0)
                        {
                            float multi = 1 - SpriteManager.UniversalDim;
                            colours[i] = new Color4(col.R * multi, col.G * multi, col.B * multi, c.A);
                        }
                        else
                            colours[i] = new Color4(col.R, col.G, col.B, c.A);

                        //todo: optimise
                    }

                    GL.EnableClientState(ArrayCap.ColorArray);

                    GL.ColorPointer(4, ColorPointerType.Float, 0, handle_colours_pointer);
                }

                //first move everything so it is centered on (0,0)
                /*float vLeft = -origin.X;
                float vTop = -origin.Y;
                float vRight = -origin.X + scale.X;
                float vBottom = -origin.Y + scale.Y;

                if (Rotation != 0)
                {
                    float cos = (float)Math.Cos(Rotation);
                    float sin = (float)Math.Sin(Rotation);

                    vertices[0] = vLeft * cos - vTop * sin + pos.X;
                    vertices[1] = vLeft * sin + vTop * cos + pos.Y;
                    vertices[2] = vRight * cos - vTop * sin + pos.X;
                    vertices[3] = vRight * sin + vTop * cos + pos.Y;
                    vertices[4] = vRight * cos - vBottom * sin + pos.X;
                    vertices[5] = vRight * sin + vBottom * cos + pos.Y;
                    vertices[6] = vLeft * cos - vBottom * sin + pos.X;
                    vertices[7] = vLeft * sin + vBottom * cos + pos.Y;
                }
                else*/
                /*vLeft += pos.X;
                    vRight += pos.X;
                    vTop += pos.Y;
                    vBottom += pos.Y;*/
#if NO_PIN_SUPPORT
                    Vector2* vertices = (Vector2*)handle_vertices_pointer.ToPointer();
#endif
                vertices[0].X = pos.X + p1.X * scale.X - origin.X;
                vertices[0].Y = pos.Y + p1.Y * scale.Y - origin.Y;
                vertices[1].X = pos.X + p2.X * scale.X - origin.X;
                vertices[1].Y = pos.Y + p2.Y * scale.Y - origin.Y;
                vertices[2].X = pos.X + p4.X * scale.X - origin.X;
                vertices[2].Y = pos.Y + p4.Y * scale.Y - origin.Y;
                vertices[3].X = pos.X + p3.X * scale.X - origin.X;
                vertices[3].Y = pos.Y + p3.Y * scale.Y - origin.Y;

                if (Texture != null && Texture.TextureGl != null)
                {
                    SpriteManager.TexturesEnabled = true;
                    Texture.TextureGl.Bind();
#if !NO_PIN_SUPPORT
                    if (coordinates == null)
                    {
                        coordinates = new[]
                        {
                            (float)Texture.X / Texture.TextureGl.potWidth,
                            (float)Texture.Y / Texture.TextureGl.potHeight,
                            (float)(Texture.X + Texture.Width) / Texture.TextureGl.potWidth,
                            (float)Texture.Y / Texture.TextureGl.potHeight,
                            (float)(Texture.X + Texture.Width) / Texture.TextureGl.potWidth,
                            (float)(Texture.Y + Texture.Height) / Texture.TextureGl.potHeight,
                            (float)Texture.X / Texture.TextureGl.potWidth,
                            (float)(Texture.Y + Texture.Height) / Texture.TextureGl.potHeight
                        };

                        handle_coordinates = GCHandle.Alloc(coordinates, GCHandleType.Pinned);
                        handle_coordinates_pointer = handle_coordinates.AddrOfPinnedObject();
                    }
#else
                    if(handle_coordinates_pointer == IntPtr.Zero)
                    {
                        unsafe
                        {
                            Color4* colours = (Color4*)handle_colours_pointer.ToPointer();
                            handle_coordinates_pointer = Marshal.AllocHGlobal(8 * sizeof(float));

                            float* coordinates = (float*)handle_coordinates_pointer.ToPointer();

                            coordinates[0] = (float)Texture.X / Texture.TextureGl.potWidth;
                            coordinates[1] = (float)Texture.Y / Texture.TextureGl.potHeight;
                            coordinates[2] = (float)(Texture.X + Texture.Width) / Texture.TextureGl.potWidth;
                            coordinates[3] = (float)Texture.Y / Texture.TextureGl.potHeight;
                            coordinates[4] = (float)(Texture.X + Texture.Width) / Texture.TextureGl.potWidth;
                            coordinates[5] = (float)(Texture.Y + Texture.Height) / Texture.TextureGl.potHeight;
                            coordinates[6] = (float)Texture.X / Texture.TextureGl.potWidth;
                            coordinates[7] = (float)(Texture.Y + Texture.Height) / Texture.TextureGl.potHeight;
                        }

                    }
#endif
                    GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, handle_coordinates_pointer);
                }
                else
                    SpriteManager.TexturesEnabled = false;

                GL.VertexPointer(2, VertexPointerType.Float, 0, handle_vertices_pointer);

                GL.DrawArrays(BeginMode.TriangleFan, 0, 4);

                if (Colours != null)
                    GL.DisableClientState(ArrayCap.ColorArray);

                return true;
            }

            return false;
        }
    }
}