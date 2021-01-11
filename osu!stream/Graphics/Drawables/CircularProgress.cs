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
using osum.Helpers;


namespace osum.Graphics.Drawables
{
    internal class CircularProgress : pDrawable
    {
        internal float Progress;
        internal float Radius;
        internal bool EvenShading;

        private readonly int parts = 48;
#if !NO_PIN_SUPPORT
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly float[] vertices;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly float[] colours;
#endif

        private GCHandle handle_vertices;
        private GCHandle handle_colours;

        private IntPtr handle_vertices_pointer;
        private IntPtr handle_colours_pointer;

        public CircularProgress(Vector2 position, float radius, bool alwaysDraw, float drawDepth, Color4 colour)
        {
            parts = GameBase.IsSlowDevice ? 36 : 48;
            AlwaysDraw = alwaysDraw;
            Alpha = alwaysDraw ? 1 : 0;
            DrawDepth = drawDepth;
            Position = position;
            Radius = radius;
            Colour = colour;
            Field = FieldTypes.Standard;
#if !NO_PIN_SUPPORT
            vertices = new float[parts * 2 + 2];
            colours = new float[parts * 4 + 4];


            handle_vertices = GCHandle.Alloc(vertices, GCHandleType.Pinned);
            handle_colours = GCHandle.Alloc(colours, GCHandleType.Pinned);

            handle_vertices_pointer = handle_vertices.AddrOfPinnedObject();
            handle_colours_pointer = handle_colours.AddrOfPinnedObject();
#else
            handle_vertices_pointer = Marshal.AllocHGlobal((parts * 2 + 2) * sizeof(float));
            handle_colours_pointer = Marshal.AllocHGlobal((parts * 4 + 4) * sizeof(float));
#endif
        }

        public override void Dispose()
        {
#if !NO_PIN_SUPPORT
            if (handle_colours.IsAllocated)
            {
                handle_colours.Free();
                handle_vertices.Free();
            }
#else
            Marshal.FreeHGlobal(handle_vertices_pointer);
            Marshal.FreeHGlobal(handle_colours_pointer);
#endif

            base.Dispose();
        }

        public override bool Draw()
        {
            if (base.Draw())
            {
                Color4 c = AlphaAppliedColour;

                float startAngle = -MathHelper.Pi / 2;
                float cappedProgress = pMathHelper.ClampToOne(Progress);

                float endAngle = cappedProgress * MathHelper.Pi * 2f + startAngle;

                float da = (endAngle - startAngle) / (parts - 1);

                float radius = Radius * FieldScale.X;
                Vector2 pos = FieldPosition;
                unsafe
                {
                    float* l_vertices = (float*)handle_vertices_pointer.ToPointer();
                    float* l_colours = (float*)handle_colours_pointer.ToPointer();

                    l_vertices[0] = pos.X;
                    l_vertices[1] = pos.Y;

                    l_colours[0] = c.R;
                    l_colours[1] = c.G;
                    l_colours[2] = c.B;
                    l_colours[3] = c.A * Progress;

                    float a = startAngle;
                    for (int v = 1; v <= parts; v++)
                    {
                        l_vertices[v * 2] = (float)(pos.X + Math.Cos(a) * radius);
                        l_vertices[v * 2 + 1] = (float)(pos.Y + Math.Sin(a) * radius);
                        a += da;

                        l_colours[v * 4] = c.R;
                        l_colours[v * 4 + 1] = c.G;
                        l_colours[v * 4 + 2] = c.B;
                        l_colours[v * 4 + 3] = c.A * (EvenShading ? 0.6f : (0.2f + 0.4f * ((float)v / parts)));
                    }
                }

                GL.EnableClientState(ArrayCap.ColorArray);

                GL.VertexPointer(2, VertexPointerType.Float, 0, handle_vertices_pointer);
                GL.ColorPointer(4, ColorPointerType.Float, 0, handle_colours_pointer);

                GL.DrawArrays(BeginMode.TriangleFan, 0, parts + 1);

                GL.DisableClientState(ArrayCap.ColorArray);

                return true;
            }

            return false;
        }
    }
}