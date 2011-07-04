using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Sprites;
using OpenTK.Graphics;
using osum.Helpers;
using osum.GameplayElements;

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
        const int line_count = 5;

        float[] vertices = new float[(line_count + 1) * 4 * 2];
        float[] colours = new float[(line_count + 1) * 4 * 4];

        internal static Color4 COLOUR_INTRO = new Color4(25, 25, 25, 255);
        internal static Color4 COLOUR_EASY = new Color4(122, 172, 37, 255);
        internal static Color4 COLOUR_STANDARD = new Color4(0, 78, 206, 255);
        internal static Color4 COLOUR_HARD = new Color4(133, 25, 0, 255);
        internal static Color4 COLOUR_EXPERT = new Color4(77, 0, 105, 255);
        internal static Color4 COLOUR_WARNING = new Color4(174, 17, 17, 255);
        private Color4 currentColour;

        public PlayfieldBackground()
            : base()
        {
            initialize();

            DrawDepth = 0;
            AlwaysDraw = true;
            Alpha = 1;
            currentColour = Colour = COLOUR_INTRO;

            curentXOffset = -lineWidth;

            GameBase.OnScreenLayoutChanged += initialize;
        }

        float curentXOffset;
        float lineWidth;

        private void initialize()
        {
            lineWidth = GameBase.NativeSize.Width * 0.2f;

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
            
            calculateDiagonals();
        }

        private void calculateDiagonals()
        {
            int j = 8;

            float diagonalY = curentXOffset * GameBase.BaseToNativeRatio - lineWidth;
            float diagonalX = curentXOffset * GameBase.BaseToNativeRatio - lineWidth;

            for (int k = 0; k < line_count; k++)
            {
                vertices[j++] = diagonalX;
                vertices[j++] = 0;

                vertices[j++] = diagonalX + lineWidth;
                vertices[j++] = 0;

                vertices[j++] = 0;
                vertices[j++] = diagonalY + lineWidth;

                vertices[j++] = 0;
                vertices[j++] = diagonalY;

                diagonalY += lineWidth * 2;
                diagonalX += lineWidth * 2;
            }
        }

        public override void Dispose()
        {
            GameBase.OnScreenLayoutChanged -= initialize;

            base.Dispose();
        }

        internal void Move(float amount)
        {
            curentXOffset += amount * 0.7f;

            float nativeX = curentXOffset * GameBase.BaseToNativeRatio;


            if (nativeX - 2 * lineWidth > 0)
                curentXOffset -= lineWidth / GameBase.BaseToNativeRatio * 2;
            else if (0.5f * lineWidth - nativeX > 0)
                curentXOffset += lineWidth / GameBase.BaseToNativeRatio * 2;

            calculateDiagonals();
        }

        internal float Velocity;

        public override void Update()
        {
            base.Update();

            Color4 colourTop = Colour;
            Color4 colourBottom = ColourHelper.Darken(Colour, 0.85f);

            if (Velocity != 0)
            {
                Move(Velocity);
                Velocity *= 0.9f;
                if (Math.Abs(Velocity) < 0.01f) Velocity = 0;
            }

            Color4 col = Colour;
            for (int i = 0; i < (line_count + 1) * 4; i++)
            {
                //change to the darker colour for bottom vertices and diagonals
                if (i == 2) col = ColourHelper.Darken(Colour, 0.85f);

                if (SpriteManager.UniversalDim > 0)
                {
                    float mult = 1 - SpriteManager.UniversalDim;
                    colours[i * 4] = col.R * mult;
                    colours[i * 4 + 1] = col.G * mult;
                    colours[i * 4 + 2] = col.B * mult;
                    colours[i * 4 + 3] = col.A;
                }
                else
                {
                    colours[i * 4] = col.R;
                    colours[i * 4 + 1] = col.G;
                    colours[i * 4 + 2] = col.B;
                    colours[i * 4 + 3] = col.A;
                }
            }
        }

        public override bool Draw()
        {
            if (!base.Draw()) return false;

            SpriteManager.TexturesEnabled = false;

            GL.EnableClientState(ArrayCap.ColorArray);

            SpriteManager.AlphaBlend = false;

            GL.VertexPointer(2, VertexPointerType.Float, 0, vertices);
            GL.ColorPointer(4, ColorPointerType.Float, 0, colours);
            GL.DrawArrays(BeginMode.TriangleFan, 0, 4);

            SpriteManager.AlphaBlend = true;

            SpriteManager.SetBlending(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);

            //todo: this can definitely be further optimised into a single call.
            for (int i = 0; i < line_count; i++)
                GL.DrawArrays(BeginMode.TriangleFan, (i + 1) * 4, 4);

            GL.DisableClientState(ArrayCap.ColorArray);

            return true;
        }

        Difficulty lastDifficulty;
        internal void ChangeColour(Difficulty difficulty, bool flash = true)
        {
            if (difficulty != lastDifficulty && flash)
            {
                if (difficulty > lastDifficulty)
                    Velocity = 50;
                else
                    Velocity = -50;
            }

            lastDifficulty = difficulty;

            switch (difficulty)
            {
                case Difficulty.Easy:
                    ChangeColour(COLOUR_EASY, flash);
                    return;
                case Difficulty.Normal:
                    ChangeColour(COLOUR_STANDARD, flash);
                    return;
                case Difficulty.Hard:
                    ChangeColour(COLOUR_HARD, flash);
                    return;
                case Difficulty.Expert:
                    ChangeColour(COLOUR_EXPERT, flash);
                    return;
            }
        }

        internal void ChangeColour(Color4 colour, bool flash = true)
        {
            if (colour == currentColour)
                return;

            if (flash)
            {
                Transformations.RemoveAll(t => t.Type == TransformationType.Colour);

                Colour = colour;

                if (currentColour == COLOUR_INTRO)
                    FlashColour(Color4.LightGray, 400);
                else
                    FlashColour(Color4.White, 400);
            }
            else
            {
                FadeColour(colour, 400);
            }

            currentColour = colour;
        }
    }
}
