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
#else
using OpenTK.Graphics.OpenGL;
#endif
using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using osum.Graphics.Sprites;

namespace osum.Graphics
{
    public class TextureGl : IDisposable
    {
        internal int potHeight;
        internal int potWidth;

        private int textureHeight;

        internal int TextureHeight
        {
            get => textureHeight;
            set
            {
                textureHeight = value;
                potHeight = GetPotDimension(value);
            }
        }

        private int textureWidth;

        internal int TextureWidth
        {
            get => textureWidth;
            set
            {
                textureWidth = value;
                potWidth = GetPotDimension(value);
            }
        }

        public int Id;
        public bool Loaded => Id > 0;

#if !NO_PIN_SUPPORT
        private readonly float[] coordinates;
        private readonly float[] vertices;

        private GCHandle handle_vertices;
        private GCHandle handle_coordinates;
#endif

        private readonly IntPtr handle_vertices_pointer;
        private readonly IntPtr handle_coordinates_pointer;

#if IPHONE
        static readonly internal bool SUPPORTS_DRAWTEXTURE_EXT;
        static TextureGl()
        {
            SUPPORTS_DRAWTEXTURE_EXT = GL.GetString(All.Extensions).Contains("OES_draw_texture");

        }
#endif


        public TextureGl(int width, int height)
        {
            Id = -1;
            TextureWidth = width;
            TextureHeight = height;

#if !NO_PIN_SUPPORT
            coordinates = new float[8];
            vertices = new float[8];

            handle_vertices = GCHandle.Alloc(vertices, GCHandleType.Pinned);
            handle_coordinates = GCHandle.Alloc(coordinates, GCHandleType.Pinned);

            handle_vertices_pointer = handle_vertices.AddrOfPinnedObject();
            handle_coordinates_pointer = handle_coordinates.AddrOfPinnedObject();
#else
            handle_vertices_pointer = Marshal.AllocHGlobal(8 * sizeof(float));
            handle_coordinates_pointer = Marshal.AllocHGlobal(8 * sizeof(float));
#endif
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        /// <summary>
        /// Removes texture from GL memory.
        /// </summary>
        public void Delete()
        {
            if (Id == -1)
                return;

            try
            {
                if (GL.IsTexture(Id))
                    GL.DeleteTextures(1, new[] { Id });
            }
            catch
            {
            }

            Id = -1;
        }

        private bool IsDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

#if !NO_PIN_SUPPORT
            handle_vertices.Free();
            handle_coordinates.Free();
#else
            Marshal.FreeHGlobal(handle_coordinates_pointer);
            Marshal.FreeHGlobal(handle_vertices_pointer);
#endif
            IsDisposed = true;

            Delete();
        }

        private void checkGlError()
        {
            ErrorCode error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                Console.WriteLine("GL Error: " + error);
            }
        }

#if iOS || ANDROID
        int fbo;

        internal unsafe void drawToTexture(bool begin)
        {
            if (begin)
            {
                fixed (int* p = &fbo)
                    GLES.GenFramebuffers(1, p);

                /*glGenFramebuffersOES(1, &fbo);
                glBindFramebufferOES(GL_FRAMEBUFFER_OES, fbo);
                glGenRenderbuffersOES(1, &renderbuffer);
                glBindRenderbufferOES(GL_RENDERBUFFER_OES, renderbuffer);

                glRenderbufferStorageOES(GL_RENDERBUFFER_OES, GL_DEPTH_COMPONENT16_OES, _width, _height);
                glFramebufferRenderbufferOES(GL_FRAMEBUFFER_OES, GL_DEPTH_ATTACHMENT_OES, GL_RENDERBUFFER_OES, renderbuffer);

                glBindTexture(GL_TEXTURE_2D, _name);
                glFramebufferTexture2DOES(GL_FRAMEBUFFER_OES, GL_COLOR_ATTACHMENT0_OES, GL_TEXTURE_2D, _name, 0);

                glGetIntegerv(GL_VIEWPORT,(int*)viewport);
                glViewport(0, 0, _width, _height);

                glPushMatrix();
                glScalef(320.0f/_width, 480.0f/_height, 1.0f);

                glClearColor(0.0f, 0.0f, 0.0f, 0.0f);
                glClear(GL_COLOR_BUFFER_BIT);
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);*/
            }
            else
            {
                //save data to texture using glCopyTexImage2D
                /*glPopMatrix();

                glBindFramebufferOES(GL_FRAMEBUFFER_OES, 0);
                glDeleteFramebuffersOES(1, &fbo);
                glDeleteRenderbuffersOES(1, &renderbuffer);*/

                //restore viewport
                //glViewport(viewport[0],viewport[1],viewport[2],viewport[3]);
                GameBase.Instance.SetViewport();
            }
        }

#endif


        internal static int lastDrawTexture;

        public void Bind()
        {
            if (lastDrawTexture != Id)
            {
                lastDrawTexture = Id;
                GL.BindTexture(SURFACE_TYPE, Id);
            }
        }

        public unsafe void DrawTo(float* coordinates, float* vertices, int startIndex, Vector2 currentPos, Vector2 origin, Vector2 scaleVector, float rotation,
            Box2 drawRect)
        {
            float left = drawRect.Left / potWidth;
            float right = drawRect.Right / potWidth;
            float top = drawRect.Top / potHeight;
            float bottom = drawRect.Bottom / potHeight;

            startIndex *= 12;

            coordinates[startIndex + 0] = left;
            coordinates[startIndex + 1] = top;
            coordinates[startIndex + 2] = right;
            coordinates[startIndex + 3] = top;
            coordinates[startIndex + 4] = right;
            coordinates[startIndex + 5] = bottom;

            coordinates[startIndex + 6] = right;
            coordinates[startIndex + 7] = bottom;
            coordinates[startIndex + 8] = left;
            coordinates[startIndex + 9] = bottom;
            coordinates[startIndex + 10] = left;
            coordinates[startIndex + 11] = top;

            //first move everything so it is centered on (0,0)
            float vLeft = -(origin.X * scaleVector.X);
            float vTop = -(origin.Y * scaleVector.Y);
            float vRight = vLeft + drawRect.Width * scaleVector.X;
            float vBottom = vTop + drawRect.Height * scaleVector.Y;

            if (rotation != 0)
            {
                //only being used for spritetext atm, which doesn't need rotation.
                //todo: implement if necessary

                /*float cos = (float)Math.Cos(rotation);
                    float sin = (float)Math.Sin(rotation);

                    vertices[startIndex + 0] = vLeft * cos - vTop * sin + currentPos.X;
                    vertices[startIndex + 1] = vLeft * sin + vTop * cos + currentPos.Y;
                    vertices[startIndex + 2] = vRight * cos - vTop * sin + currentPos.X;
                    vertices[startIndex + 3] = vRight * sin + vTop * cos + currentPos.Y;
                    vertices[startIndex + 4] = vRight * cos - vBottom * sin + currentPos.X;
                    vertices[startIndex + 5] = vRight * sin + vBottom * cos + currentPos.Y;
                    vertices[startIndex + 6] = vLeft * cos - vBottom * sin + currentPos.X;
                    vertices[startIndex + 7] = vLeft * sin + vBottom * cos + currentPos.Y;*/
            }
            else
            {
                vLeft += currentPos.X;
                vRight += currentPos.X;
                vTop += currentPos.Y;
                vBottom += currentPos.Y;

                vertices[startIndex + 0] = vLeft;
                vertices[startIndex + 1] = vTop;
                vertices[startIndex + 2] = vRight;
                vertices[startIndex + 3] = vTop;
                vertices[startIndex + 4] = vRight;
                vertices[startIndex + 5] = vBottom;

                vertices[startIndex + 6] = vRight;
                vertices[startIndex + 7] = vBottom;
                vertices[startIndex + 8] = vLeft;
                vertices[startIndex + 9] = vBottom;
                vertices[startIndex + 10] = vLeft;
                vertices[startIndex + 11] = vTop;
            }
        }

        /// <summary>
        /// Blits sprite to OpenGL display with specified parameters.
        /// </summary>
        public void Draw(Vector2 currentPos, Vector2 origin, Color4 drawColour, Vector2 scaleVector, float rotation,
            Box2 drawRect)
        {
            if (Id < 0) return;

            SpriteManager.SetColour(drawColour);

            float left = drawRect.Left / potWidth;
            float right = drawRect.Right / potWidth;
            float top = drawRect.Top / potHeight;
            float bottom = drawRect.Bottom / potHeight;


/*#if ANDROID
            if (rotation == 0 && SUPPORTS_DRAWTEXTURE_EXT)
            {
                Bind();
                GL.Oes.DrawTex(left, top, 0, (right - left) * scaleVector.X, (bottom - top) * scaleVector.Y);
                return;
            }
#endif*/

#if NO_PIN_SUPPORT
                float* coordinates = (float*)handle_coordinates_pointer;
                float* vertices = (float*)handle_vertices_pointer;
#endif
            coordinates[0] = left;
            coordinates[1] = top;
            coordinates[2] = right;
            coordinates[3] = top;
            coordinates[4] = right;
            coordinates[5] = bottom;
            coordinates[6] = left;
            coordinates[7] = bottom;

            //first move everything so it is centered on (0,0)
            float vLeft = -(origin.X * scaleVector.X);
            float vTop = -(origin.Y * scaleVector.Y);
            float vRight = vLeft + drawRect.Width * scaleVector.X;
            float vBottom = vTop + drawRect.Height * scaleVector.Y;

            if (rotation != 0)
            {
                float cos = (float)Math.Cos(rotation);
                float sin = (float)Math.Sin(rotation);

                vertices[0] = vLeft * cos - vTop * sin + currentPos.X;
                vertices[1] = vLeft * sin + vTop * cos + currentPos.Y;
                vertices[2] = vRight * cos - vTop * sin + currentPos.X;
                vertices[3] = vRight * sin + vTop * cos + currentPos.Y;
                vertices[4] = vRight * cos - vBottom * sin + currentPos.X;
                vertices[5] = vRight * sin + vBottom * cos + currentPos.Y;
                vertices[6] = vLeft * cos - vBottom * sin + currentPos.X;
                vertices[7] = vLeft * sin + vBottom * cos + currentPos.Y;
            }
            else
            {
                vLeft += currentPos.X;
                vRight += currentPos.X;
                vTop += currentPos.Y;
                vBottom += currentPos.Y;

                vertices[0] = vLeft;
                vertices[1] = vTop;
                vertices[2] = vRight;
                vertices[3] = vTop;
                vertices[4] = vRight;
                vertices[5] = vBottom;
                vertices[6] = vLeft;
                vertices[7] = vBottom;
            }

            Bind();

            GL.VertexPointer(2, VertexPointerType.Float, 0, handle_vertices_pointer);
            GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, handle_coordinates_pointer);
            GL.DrawArrays(BeginMode.TriangleFan, 0, 4);
        }

        public void SetData(int textureId)
        {
            Id = textureId;
        }

        public void SetData(byte[] data)
        {
            SetData(data, 0, PIXEL_FORMAT);
        }

        public void SetData(byte[] data, int level)
        {
            SetData(data, level, PIXEL_FORMAT);
        }

        /// <summary>
        /// Load texture data from a raw byte array (BGRA 32bit format)
        /// </summary>
        public unsafe void SetData(byte[] data, int level, PixelFormat format)
        {
            fixed (byte* dataPtr = data)
                SetData((IntPtr)dataPtr, level, format);
        }

        internal static int GetPotDimension(int size)
        {
            int pot = 1;
            while (pot < size)
                pot *= 2;
            return pot;
        }

        public const TextureTarget SURFACE_TYPE = TextureTarget.Texture2D;

#if iOS || ANDROID
        public const PixelFormat PIXEL_FORMAT = PixelFormat.Rgba;
#else
        public const PixelFormat PIXEL_FORMAT = PixelFormat.Bgra;
#endif

        public bool usedSinceLastModeChange;

        /// <summary>
        /// Load texture data from a raw IntPtr location (BGRA 32bit format)
        /// </summary>
        public void SetData(IntPtr dataPointer, int level, PixelFormat format)
        {
            if (format == 0)
                format = PIXEL_FORMAT;

            SpriteManager.TexturesEnabled = true;

            bool newTexture = false;

            if (level == 0 && Id < 0)
            {
                Delete();
                newTexture = true;
                int[] textures = new int[1];
                GL.GenTextures(1, textures);
                Id = textures[0];
            }

            GL.BindTexture(SURFACE_TYPE, Id);

            if (level > 0)
            {
                GL.TexParameter(SURFACE_TYPE, TextureParameterName.TextureMinFilter, (int)All.LinearMipmapNearest);
                GL.TexParameter(SURFACE_TYPE, TextureParameterName.TextureMagFilter, (int)All.LinearMipmapNearest);
            }
            else
            {
                GL.TexParameter(SURFACE_TYPE, TextureParameterName.TextureMinFilter, (int)All.Linear);
                GL.TexParameter(SURFACE_TYPE, TextureParameterName.TextureMagFilter, (int)All.Linear);
            }

            //can't determine if this helps
            GL.TexParameter(SURFACE_TYPE, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(SURFACE_TYPE, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);

            //doesn't seem to help much at all? maybe best to test once more...
            //GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float)All.Replace);

#if iOS || ANDROID
            int internalFormat = (int)PixelInternalFormat.Rgba;
            switch (format)
            {
                case PixelFormat.Alpha:
                    internalFormat = (int)PixelInternalFormat.Alpha;
                    break;
            }
#else
            PixelInternalFormat internalFormat = PixelInternalFormat.Rgba;
            switch (format)
            {
                case PixelFormat.Alpha:
                    internalFormat = PixelInternalFormat.Alpha;
                    break;
            }
#endif

            if (newTexture)
            {
                if (SURFACE_TYPE == TextureTarget.Texture2D)
                {
                    if (potWidth == TextureWidth && potHeight == TextureHeight || dataPointer == IntPtr.Zero)
                    {
                        GL.TexImage2D(SURFACE_TYPE, level, internalFormat, potWidth, potHeight, 0, format,
                            PixelType.UnsignedByte, dataPointer);
                    }
                    else
                    {
                        GL.TexImage2D(SURFACE_TYPE, level, internalFormat, potWidth, potHeight, 0, format,
                            PixelType.UnsignedByte, IntPtr.Zero);

                        GL.TexSubImage2D(SURFACE_TYPE, level, 0, 0, TextureWidth, TextureHeight, format,
                            PixelType.UnsignedByte, dataPointer);
                    }
                }
                else
                {
                    GL.TexImage2D(SURFACE_TYPE, level, internalFormat, TextureWidth, TextureHeight, 0, format,
                        PixelType.UnsignedByte, dataPointer);
                }
            }
            else
            {
                GL.TexImage2D(SURFACE_TYPE, level, internalFormat, TextureWidth / (int)Math.Pow(2, level), TextureHeight / (int)Math.Pow(2, level), 0, format,
                    PixelType.UnsignedByte, dataPointer);
            }

            Reset();
        }

        internal static void Reset()
        {
            lastDrawTexture = -1;
        }
    }
}