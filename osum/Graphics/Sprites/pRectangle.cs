using System;
using osum.Graphics.Sprites;
using osum.Graphics.Drawables;
using osum.Helpers;
using System.Runtime.InteropServices;
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
#else
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
#endif


namespace osum.Graphics.Drawables
{
    internal class pRectangle : pDrawable
    {
        public pRectangle(Vector2 position, Vector2 size, bool alwaysDraw, float drawDepth, Color4 colour)
        {
            AlwaysDraw = alwaysDraw;
            Alpha = alwaysDraw ? 1 : 0;
            DrawDepth = drawDepth;
            Position = position;
            Colour = colour;
            Clocking = ClockTypes.Mode;
            Field = FieldTypes.Standard;
            Scale = size;

            coordinates = new float[8];
            vertices = new float[8];

            handle_vertices = GCHandle.Alloc(vertices, GCHandleType.Pinned);
            handle_coordinates = GCHandle.Alloc(coordinates, GCHandleType.Pinned);

            handle_vertices_pointer = handle_vertices.AddrOfPinnedObject();
            handle_coordinates_pointer = handle_coordinates.AddrOfPinnedObject();
        }

        float[] coordinates;
        float[] vertices;

        GCHandle handle_vertices;
        GCHandle handle_coordinates;

        IntPtr handle_vertices_pointer;
        IntPtr handle_coordinates_pointer;

        public override void Dispose()
        {
            handle_vertices.Free();
            handle_coordinates.Free();
            base.Dispose();
        }

        public override bool Draw()
        {
            if (base.Draw())
            {

                Color4 c = AlphaAppliedColour;
                Vector2 pos = FieldPosition;
                Vector2 scale = FieldScale;
                Vector2 origin = OriginVector * GameBase.BaseToNativeRatio;

                SpriteManager.SetColour(c);

                //first move everything so it is centered on (0,0)
                float vLeft = -origin.X;
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
                else
                {
                    vLeft += pos.X;
                    vRight += pos.X;
                    vTop += pos.Y;
                    vBottom += pos.Y;

                    vertices[0] = vLeft;
                    vertices[1] = vTop;
                    vertices[2] = vRight;
                    vertices[3] = vTop;
                    vertices[4] = vRight;
                    vertices[5] = vBottom;
                    vertices[6] = vLeft;
                    vertices[7] = vBottom;
                }

                SpriteManager.TexturesEnabled = false;

                GL.VertexPointer(2, VertexPointerType.Float, 0, handle_vertices_pointer);
                GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, handle_coordinates_pointer);
                GL.DrawArrays(BeginMode.TriangleFan, 0, 4);

                return true;
            }

            return false;

        }
    }
}

