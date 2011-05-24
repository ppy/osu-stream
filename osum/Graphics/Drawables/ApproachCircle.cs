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
#else
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
#endif


namespace osum.Graphics.Drawables
{
    internal class ApproachCircle : pDrawable
    {
        internal float Radius;
        internal float Width = 2 / 20f;

        public ApproachCircle(Vector2 position, float radius, bool alwaysDraw, float drawDepth, Color4 colour)
        {
            AlwaysDraw = alwaysDraw;
            DrawDepth = drawDepth;
            Field = FieldTypes.GamefieldExact;
            StartPosition = position;
            Position = position;
            Radius = radius;
            Colour = colour;
        }

        public override void Dispose()
        {
        }

        const int PARTS = 48;
        float[] vertices = new float[PARTS * 4 + 4];

        public override bool Draw()
        {
            if (base.Draw())
            {
                float scale = FieldScale.X;

                float rad1 = (Radius * Scale.X + Width * 0.5f) * (scale / Scale.X);
                float rad2 = (Radius * Scale.X - Width * 0.5f) * (scale / Scale.X);

                Vector2 pos = FieldPosition;
                Color4 c = AlphaAppliedColour;

                for (int v = 0; v < PARTS; v++)
                {
                    vertices[v * 4] = (float)(pos.X + Math.Cos(v * 2.0f * Math.PI / PARTS) * rad1);
                    vertices[v * 4 + 1] = (float)(pos.Y + Math.Sin(v * 2.0f * Math.PI / PARTS) * rad1);
                    vertices[v * 4 + 2] = (float)(pos.X + Math.Cos(v * 2.0f * Math.PI / PARTS) * rad2);
                    vertices[v * 4 + 3] = (float)(pos.Y + Math.Sin(v * 2.0f * Math.PI / PARTS) * rad2);
                }

                vertices[PARTS * 4] = vertices[0];
                vertices[PARTS * 4 + 1] = vertices[1];
                vertices[PARTS * 4 + 2] = vertices[2];
                vertices[PARTS * 4 + 3] = vertices[3];

                SpriteManager.TexturesEnabled = false;

                GL.Color4(c.R, c.G, c.B, c.A);
                GL.VertexPointer(2, VertexPointerType.Float, 0, vertices);
                GL.DrawArrays(BeginMode.TriangleStrip, 0, PARTS * 2 + 2);

                return true;
            }

            return false;

        }
    }
}

