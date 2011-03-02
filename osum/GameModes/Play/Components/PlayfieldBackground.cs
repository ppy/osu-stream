using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Sprites;
using OpenTK.Graphics;
using osum.Helpers;

#if iOS
using OpenTK.Graphics.ES11;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;

using TextureTarget = OpenTK.Graphics.ES11.All;
using TextureParameterName = OpenTK.Graphics.ES11.All;
using EnableCap = OpenTK.Graphics.ES11.All;
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
using ArrayCap =  OpenTK.Graphics.ES11.All;
#else
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using osum.Input;
#endif

namespace osum.GameModes.Play.Components
{
    /// <summary>
    /// Vector implementation of a gradient + diagonal sprites background.
    /// Hopefully uses less resources than drawing a massive texture!
    /// </summary>
    class PlayfieldBackground : pDrawable
    {
        float[] vertices = new float[20 * 2];
        float[] colours = new float[20 * 4];

        internal static Color4 COLOUR_INTRO = new Color4(25, 25, 25, 255);
        internal static Color4 COLOUR_STANDARD = new Color4(18, 78, 143, 255);
        internal static Color4 COLOUR_HARD = new Color4(215, 122, 12, 255);
        internal static Color4 COLOUR_WARNING = new Color4(237, 29, 29, 255);
        private Color4 currentColour;

        public PlayfieldBackground()
            : base()
        {
            initialize();

            DrawDepth = 0;
            AlwaysDraw = true;
            Alpha = 1;
            currentColour = Colour = COLOUR_INTRO;

            GameBase.OnScreenLayoutChanged += initialize;
        }

        private void initialize()
        {
            float left = 0;
            float right = GameBase.NativeSize.Width;
            float top = 0;
            float bottom = GameBase.NativeSize.Height;

            int j = 0;

            //main background
            vertices[j++] = left;
            vertices[j++] = top;
            vertices[j++] = right;
            vertices[j++] = top;
            vertices[j++] = right;
            vertices[j++] = bottom;
            vertices[j++] = left;
            vertices[j++] = bottom;

            //diagonal lines
            float diagonalWidth = GameBase.NativeSize.Width * 0.2f;

            float diagonalY = diagonalWidth;
            float diagonalX = diagonalWidth;

            for (int k = 0; k < 4; k++)
            {
                vertices[j++] = diagonalX;
                vertices[j++] = 0;

                vertices[j++] = diagonalX + diagonalWidth;
                vertices[j++] = 0;

                vertices[j++] = 0;
                vertices[j++] = diagonalY + diagonalWidth;

                vertices[j++] = 0;
                vertices[j++] = diagonalY;

                diagonalY += diagonalWidth * 2;
                diagonalX += diagonalWidth * 2;
            }
        }

        public override void Dispose()
        {
            GameBase.OnScreenLayoutChanged -= initialize;

            base.Dispose();
        }

        public override void Update()
        {
            base.Update();

            Color4 colourTop = Colour;
            Color4 colourBottom = ColourHelper.Darken(Colour, 0.85f);

            Color4 col = Colour;
            for (int i = 0; i < 20; i++)
            {
                //change to the darker colour for bottom vertices and diagonals
                if (i == 2) col = ColourHelper.Darken(Colour, 0.85f);

                colours[i * 4] = col.R;
                colours[i * 4 + 1] = col.G;
                colours[i * 4 + 2] = col.B;
                colours[i * 4 + 3] = col.A * (1 - SpriteManager.UniversalDim);
            }
        }

        public override bool Draw()
        {
            if (!base.Draw()) return false;

            SpriteManager.TexturesEnabled = false;

            GL.EnableClientState(ArrayCap.ColorArray);

            GL.VertexPointer(2, VertexPointerType.Float, 0, vertices);
            GL.ColorPointer(4, ColorPointerType.Float, 0, colours);
            GL.DrawArrays(BeginMode.TriangleFan, 0, 4);

            SpriteManager.BlendingMode = BlendingFactorDest.One;

            //todo: this can definitely be further optimised into a single call.
            GL.DrawArrays(BeginMode.TriangleFan, 4, 4);
            GL.DrawArrays(BeginMode.TriangleFan, 8, 4);
            GL.DrawArrays(BeginMode.TriangleFan, 12, 4);
            GL.DrawArrays(BeginMode.TriangleFan, 16, 4);

            GL.DisableClientState(ArrayCap.ColorArray);

            return true;
        }

        internal void ChangeColour(Color4 colour)
        {
            if (colour == currentColour)
                return;

            Colour = colour;

            if (currentColour == COLOUR_INTRO)
                FlashColour(Color4.LightGray, 400);
            else
                FlashColour(Color4.White, 400);

            currentColour = colour;
        }
    }
}
