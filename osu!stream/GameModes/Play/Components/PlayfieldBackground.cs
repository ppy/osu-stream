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
using ArrayCap = OpenTK.Graphics.ES11.All;
#else
using OpenTK.Graphics.OpenGL;
#endif
using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics;
using osum.GameplayElements;
using osum.Graphics.Sprites;
using osum.Helpers;

namespace osum.GameModes.Play.Components
{
    /// <summary>
    /// Vector implementation of a gradient + diagonal sprites background.
    /// Hopefully uses less resources than drawing a massive texture!
    /// </summary>
    internal class PlayfieldBackground : pDrawable
    {
        private const int line_count = 5;
#if !NO_PIN_SUPPORT
        private readonly float[] vertices;
        private readonly float[] colours;

        private GCHandle handle_vertices;
        private GCHandle handle_colours;
#endif
        private readonly IntPtr handle_vertices_pointer;
        private readonly IntPtr handle_colours_pointer;

        internal static Color4 COLOUR_INTRO = new Color4(25, 25, 25, 255);
        internal static Color4 COLOUR_EASY = new Color4(90, 135, 42, 255);

        internal static Color4 COLOUR_STANDARD = new Color4(43, 80, 136, 255);

        //internal static Color4 COLOUR_HARD = new Color4(150, 0, 95, 255);
        internal static Color4 COLOUR_HARD = new Color4(135, 42, 101, 255);
        internal static Color4 COLOUR_EXPERT = new Color4(111, 43, 136, 255);
        internal static Color4 COLOUR_WARNING = new Color4(174, 17, 17, 255);
        private Color4 currentColour;

        public PlayfieldBackground()
        {
#if !NO_PIN_SUPPORT
            vertices = new float[(line_count + 1) * 4 * 2];
            colours = new float[(line_count + 1) * 4 * 4];


            handle_vertices = GCHandle.Alloc(vertices, GCHandleType.Pinned);
            handle_colours = GCHandle.Alloc(colours, GCHandleType.Pinned);

            handle_vertices_pointer = handle_vertices.AddrOfPinnedObject();
            handle_colours_pointer = handle_colours.AddrOfPinnedObject();
#else
            handle_vertices_pointer = Marshal.AllocHGlobal((line_count + 1) * 4 * 2 * sizeof(float) );
            handle_colours_pointer = Marshal.AllocHGlobal((line_count + 1) * 4 * 4 * sizeof(float) );
#endif

            initialize();

            DrawDepth = 0.002f;
            AlwaysDraw = true;
            Alpha = 1;
            currentColour = Colour = COLOUR_INTRO;

            curentXOffset = -lineWidth;

            GameBase.OnScreenLayoutChanged += initialize;
        }

        private float curentXOffset;
        private float lineWidth;

        private void initialize()
        {
            lineWidth = GameBase.NativeSize.Width * 0.2f;

            float left = 0;
            float right = GameBase.NativeSize.Width;
            float top = 0;
            float bottom = GameBase.NativeSize.Height;

            int j = 0;
#if NO_PIN_SUPPORT
                float* vertices = (float*)handle_vertices_pointer;
#endif

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

#if NO_PIN_SUPPORT
                float* vertices = (float*)handle_vertices_pointer;
#endif
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
#if !NO_PIN_SUPPORT
            if (handle_vertices.IsAllocated)
            {
                handle_vertices.Free();
                handle_colours.Free();
            }
#else
            if (handle_colours_pointer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(handle_colours_pointer);
                Marshal.FreeHGlobal(handle_vertices_pointer);
                handle_colours_pointer = IntPtr.Zero;
            }
#endif
            GameBase.OnScreenLayoutChanged -= initialize;

            base.Dispose();
        }

        internal void Move(float amount)
        {
            curentXOffset += amount * 0.7f * Clock.ElapsedRatioToSixty;

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

            if (Velocity != 0)
            {
                Move(Velocity);
                Velocity *= 1 - (0.1f * Clock.ElapsedRatioToSixty);
                if (Math.Abs(Velocity) < 0.01f) Velocity = 0;
            }

#if NO_PIN_SUPPORT
                float* colours = (float*)handle_colours_pointer;
#endif
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

            GL.VertexPointer(2, VertexPointerType.Float, 0, handle_vertices_pointer);
            GL.ColorPointer(4, ColorPointerType.Float, 0, handle_colours_pointer);

            GL.DrawArrays(BeginMode.TriangleFan, 0, 4);

            SpriteManager.AlphaBlend = true;

            SpriteManager.SetBlending(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);

            //todo: this can definitely be further optimised into a single call.
            for (int i = 0; i < line_count; i++)
                GL.DrawArrays(BeginMode.TriangleFan, (i + 1) * 4, 4);

            GL.DisableClientState(ArrayCap.ColorArray);

            return true;
        }

        private Difficulty lastDifficulty;

        internal void ChangeColour(Difficulty difficulty, bool flash = true)
        {
            if (difficulty != lastDifficulty && flash)
            {
                if (difficulty > lastDifficulty)
                    Velocity = 100;
                else
                    Velocity = -100;
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

        internal void ChangeColour(Difficulty difficulty, float dimAmount)
        {
            Color4 colour;
            switch (difficulty)
            {
                default:
                case Difficulty.Easy:
                    colour = COLOUR_EASY;
                    break;
                case Difficulty.Normal:
                    colour = COLOUR_STANDARD;
                    break;
                case Difficulty.Hard:
                    colour = COLOUR_HARD;
                    break;
                case Difficulty.Expert:
                    colour = COLOUR_EXPERT;
                    break;
            }

            colour = ColourHelper.Darken(colour, dimAmount * 0.5f);
            if (currentColour == colour)
                return;
            currentColour = colour;

            FadeColour(currentColour, 300);
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