using System;
using osum.Graphics.Sprites;
using osum.Graphics.Drawables;
using osum.Helpers;
using OpenTK.Graphics;
using OpenTK;
#if iOS
using OpenTK.Graphics.ES11;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;
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
using TextureEnvTarget =  OpenTK.Graphics.ES11.All;
using System.Runtime.InteropServices;
#else
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
#endif


namespace osum.Graphics.Drawables
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

            vertices = new Vector2[4];
            colours = new Color4[4];

            handle_vertices = GCHandle.Alloc(vertices, GCHandleType.Pinned);
            handle_colours = GCHandle.Alloc(colours, GCHandleType.Pinned);

            handle_vertices_pointer = handle_vertices.AddrOfPinnedObject();
            handle_colours_pointer = handle_colours.AddrOfPinnedObject();
        }

        private float[] coordinates;
        private Vector2[] vertices;
        private Color4[] colours;

        GCHandle handle_vertices;
        GCHandle handle_coordinates;
        GCHandle handle_colours;

        IntPtr handle_vertices_pointer;
        IntPtr handle_coordinates_pointer;
        IntPtr handle_colours_pointer;

        public Color4[] Colours;


        public override void Dispose ()
        {
            if (coordinates != null) handle_coordinates.Free();
            handle_colours.Free();
            handle_vertices.Free();

            base.Dispose();
        }


        public pTexture Texture;

        protected override bool checkHover(Vector2 position)
        {
            return PointInPolygon(position * GameBase.BaseToNativeRatio, vertices);
        }

        static bool PointInPolygon(Vector2 p, Vector2[] poly)
        {
            Vector2 p1, p2;

            bool inside = false;

            if (poly.Length < 3)
            {
                return inside;
            }

            Vector2 oldVector2 = new Vector2(
            poly[poly.Length - 1].X, poly[poly.Length - 1].Y);

            for (int i = 0; i < poly.Length; i++)
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
                {
                    /*vLeft += pos.X;
                    vRight += pos.X;
                    vTop += pos.Y;
                    vBottom += pos.Y;*/

                    vertices[0].X = pos.X + p1.X * scale.X - origin.X;
                    vertices[0].Y = pos.Y + p1.Y * scale.Y - origin.Y;
                    vertices[1].X = pos.X + p2.X * scale.X - origin.X;
                    vertices[1].Y = pos.Y + p2.Y * scale.Y - origin.Y;
                    vertices[2].X = pos.X + p4.X * scale.X - origin.X;
                    vertices[2].Y = pos.Y + p4.Y * scale.Y - origin.Y;
                    vertices[3].X = pos.X + p3.X * scale.X - origin.X;
                    vertices[3].Y = pos.Y + p3.Y * scale.Y - origin.Y;
                }

                if (Texture != null)
                {
                    SpriteManager.TexturesEnabled = true;
                    Texture.TextureGl.Bind();
                    if (coordinates == null)
                    {
                        coordinates = new float[] {
                            (float)Texture.X / Texture.TextureGl.potWidth,
                            (float)Texture.Y / Texture.TextureGl.potHeight,
                            (float)(Texture.X + Texture.Width) / Texture.TextureGl.potWidth,
                            (float)Texture.Y / Texture.TextureGl.potHeight,
                            (float)(Texture.X + Texture.Width) / Texture.TextureGl.potWidth,
                            (float)(Texture.Y + Texture.Height) / Texture.TextureGl.potHeight,
                            (float)Texture.X / Texture.TextureGl.potWidth,
                            (float)(Texture.Y + Texture.Height) / Texture.TextureGl.potHeight};

                        handle_coordinates = GCHandle.Alloc(coordinates, GCHandleType.Pinned);
                        handle_coordinates_pointer = handle_coordinates.AddrOfPinnedObject();
                    }
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

