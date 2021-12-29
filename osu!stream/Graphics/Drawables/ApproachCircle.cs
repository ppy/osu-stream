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
using osum.Graphics.Sprites;


namespace osum.Graphics.Drawables
{
    internal class ApproachCircle : pDrawable
    {
        internal float Radius;
        internal float Width = 2 / 20f;

        private GCHandle handle_vertices;
        private readonly IntPtr handle_vertices_pointer;

        public ApproachCircle(Vector2 position, float radius, bool alwaysDraw, float drawDepth, Color4 colour)
        {
            AlwaysDraw = alwaysDraw;
            DrawDepth = drawDepth;
            Field = FieldTypes.GamefieldExact;
            Position = position;
            Radius = radius;
            Colour = colour;
            parts = GameBase.IsSlowDevice ? 36 : 48;


#if !NO_PIN_SUPPORT
            vertices = new float[parts * 4 + 4];
            handle_vertices = GCHandle.Alloc(vertices, GCHandleType.Pinned);
            handle_vertices_pointer = handle_vertices.AddrOfPinnedObject();
#else
            handle_vertices_pointer = Marshal.AllocHGlobal((parts * 4 + 4) * sizeof(float));
#endif
        }

        public override void Dispose()
        {
            base.Dispose();

#if !NO_PIN_SUPPORT
            if (handle_vertices.IsAllocated) handle_vertices.Free();
#else
            Marshal.FreeHGlobal(handle_vertices_pointer);
#endif
        }

        private readonly int parts = 48;

        private static float[] precalculatedAngles;
#if !NO_PIN_SUPPORT
        private readonly float[] vertices;
#endif

        public override bool Draw()
        {
            if (base.Draw())
            {
                float scale = FieldScale.X;

                float rad1 = (Radius * Scale.X + Width * 0.5f) * (scale / Scale.X);
                float rad2 = (Radius * Scale.X - Width * 0.5f) * (scale / Scale.X);

                Vector2 pos = FieldPosition;
                Color4 c = AlphaAppliedColour;

#if NO_PIN_SUPPORT
                    float* vertices = (float*)handle_vertices_pointer.ToPointer();
#endif
                if (precalculatedAngles == null)
                {
                    precalculatedAngles = new float[parts * 2 + 2];

                    for (int v = 0; v < parts; v++)
                    {
                        precalculatedAngles[v * 2] = (float)Math.Cos(v * 2.0f * MathHelper.Pi / parts);
                        precalculatedAngles[v * 2 + 1] = (float)Math.Sin(v * 2.0f * MathHelper.Pi / parts);
                    }

                    precalculatedAngles[parts * 2] = vertices[0];
                    precalculatedAngles[parts * 2 + 1] = vertices[1];
                }

                for (int v = 0; v < parts; v++)
                {
                    float angle1 = precalculatedAngles[v * 2];
                    float angle2 = precalculatedAngles[v * 2 + 1];

                    vertices[v * 4] = pos.X + angle1 * rad1;
                    vertices[v * 4 + 1] = pos.Y + angle2 * rad1;
                    vertices[v * 4 + 2] = pos.X + angle1 * rad2;
                    vertices[v * 4 + 3] = pos.Y + angle2 * rad2;
                }

                vertices[parts * 4] = vertices[0];
                vertices[parts * 4 + 1] = vertices[1];
                vertices[parts * 4 + 2] = vertices[2];
                vertices[parts * 4 + 3] = vertices[3];

                SpriteManager.TexturesEnabled = false;

                SpriteManager.SetColour(c);

                GL.VertexPointer(2, VertexPointerType.Float, 0, handle_vertices_pointer);

                GL.DrawArrays(BeginMode.TriangleStrip, 0, parts * 2 + 2);

                return true;
            }

            return false;
        }
    }
}