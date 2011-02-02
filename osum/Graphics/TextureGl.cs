using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using osum.Helpers;
using osum.Graphics.Sprites;

#if IPHONE
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
#else
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using osum.Input;
#endif

namespace osum.Graphics
{
    public class TextureGl : IDisposable
    {
        internal readonly int potHeight;
        internal readonly int potWidth;
        internal readonly int textureHeight;
        internal readonly int textureWidth;
        public int Id;
        public bool Loaded { get { return Id > 0; } }

        public TextureGl(int width, int height)
        {
            Id = -1;
            textureWidth = width;
            textureHeight = height;

            if (SURFACE_TYPE == TextureGl.SURFACE_TYPE)
            {
                potWidth = GetPotDimension(width);
                potHeight = GetPotDimension(height);
            }
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
                {
                    int[] textures = new[] { Id };
                    GL.DeleteTextures(1, textures);
                }
            }
            catch
            {
            }

            Id = -1;
        }

        protected virtual void Dispose(bool disposing)
        {
            Delete();
        }

        private void checkGlError ()
        {
        	ErrorCode error = GL.GetError ();
        	if (error != ErrorCode.NoError)
            {
        		Console.WriteLine ("GL Error: " + error);
        	}
        }

        int fbo;// = new int[1];
        //int fixed renderbuffer;

#if IPHONE
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


        static int lastDrawTexture;

        /// <summary>
        /// Blits sprite to OpenGL display with specified parameters.
        /// </summary>
        public void Draw(Vector2 currentPos, Vector2 origin, Color4 drawColour, Vector2 scaleVector, float rotation,
                         Box2? srcRect)
        {
			if (Id < 0)
                return;

			GL.PushMatrix();

            Box2 drawRect = srcRect == null ? new Box2(0, 0, textureWidth, textureHeight) : srcRect.Value;

            float drawHeight = drawRect.Height * scaleVector.Y;
            float drawWidth = drawRect.Width * scaleVector.X;

            Vector2 originVector = new Vector2(origin.X * drawWidth / drawRect.Width, origin.Y * drawHeight / drawRect.Height);

            GL.Color4(drawColour.R,drawColour.G,drawColour.B,drawColour.A);
			GL.Translate(currentPos.X, currentPos.Y, 0);
			
			if (rotation != 0) GL.Rotate(pMathHelper.ToDegrees(rotation), 0, 0, 1.0f);

            if (originVector.X != 0 || originVector.Y != 0)
                GL.Translate(-originVector.X, -originVector.Y, 0);
			
			float left = (float)drawRect.Left / potWidth;
            float right = (float)drawRect.Right / potWidth;
            float top = (float)drawRect.Top / potHeight;
            float bottom = (float)drawRect.Bottom / potHeight;
			
            float[] coordinates = { left, top,
									right, top,
									right, bottom,
									left, bottom };

			float[] vertices = {0, 0, 0,
							drawWidth, 0, 0,
							drawWidth, drawHeight, 0,
							0, drawHeight, 0 };

            if (lastDrawTexture != Id)
            {
                lastDrawTexture = Id;
                GL.BindTexture(TextureGl.SURFACE_TYPE, Id);
            }
						
			GL.VertexPointer(3, VertexPointerType.Float, 0, vertices);
			GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, coordinates);
			

			GL.DrawArrays(BeginMode.TriangleFan, 0, 4);

            GL.PopMatrix();
        }

        public void SetData(int textureId)
        {
            this.Id = textureId;
        }

        public void SetData(byte[] data)
        {
            SetData(data, 0, PixelFormat.Bgra);
        }

        public void SetData(byte[] data, int level)
        {
#if IPHONE
			SetData(data, level, PixelFormat.Rgba);
#else
			SetData(data, level, PixelFormat.Bgra);
#endif
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
		
        /// <summary>
        /// Load texture data from a raw IntPtr location (BGRA 32bit format)
        /// </summary>
        public void SetData (IntPtr dataPointer, int level, PixelFormat format)
        {
        	if (format == 0)
        		format = PixelFormat.Bgra;

            GL.GetError();
        	//Clear errors.
			
			SpriteManager.TexturesEnabled = true;

            bool newTexture = false;

            if (level == 0 && Id < 0)
            {
        		Delete();
        		newTexture = true;
        		int[] textures = new int[1];
        		GL.GenTextures (1, textures);
        		Id = textures[0];
        	}

            if (level > 0)
        		return;

       		GL.BindTexture(SURFACE_TYPE, Id);

            GL.TexParameter(TextureGl.SURFACE_TYPE, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(TextureGl.SURFACE_TYPE, TextureParameterName.TextureMagFilter, (int)All.Linear);

            //can't determine if this helps
			GL.TexParameter(TextureGl.SURFACE_TYPE, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(TextureGl.SURFACE_TYPE, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);

			//doesn't seem to help much at all? maybe best to test once more...
            //GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float)All.Replace);

            PixelInternalFormat internalFormat = PixelInternalFormat.Rgba;
            switch (format)
            {
                case PixelFormat.Alpha:
                    internalFormat = PixelInternalFormat.Alpha;
                    break;
            }
			
			if (dataPointer != IntPtr.Zero)
			{
	            if (newTexture)
	            {
	                if (SURFACE_TYPE == TextureGl.SURFACE_TYPE)
	                {
	                    if (potWidth == textureWidth && potHeight == textureHeight || dataPointer == IntPtr.Zero)
	                    {
	#if IPHONE
	                        GL.TexImage2D(SURFACE_TYPE, level, (int)internalFormat, potWidth, potHeight, 0, format,
	                                        PixelType.UnsignedByte, dataPointer);
	#else
	                        GL.TexImage2D(SURFACE_TYPE, level, internalFormat, potWidth, potHeight, 0, format,
	                                        PixelType.UnsignedByte, dataPointer);
	#endif
	                    }
	                    else
	                    {
	                        byte[] temp = new byte[potWidth * potHeight * 4];
	                        GCHandle h0 = GCHandle.Alloc(temp, GCHandleType.Pinned);
	                        IntPtr pinnedDataPointer = h0.AddrOfPinnedObject();
	#if IPHONE
	                        GL.TexImage2D(SURFACE_TYPE, level, (int)internalFormat, potWidth, potHeight, 0, format,
	                                        PixelType.UnsignedByte, pinnedDataPointer);
	#else
	                        GL.TexImage2D(SURFACE_TYPE, level, internalFormat, potWidth, potHeight, 0, format,
	                                        PixelType.UnsignedByte, pinnedDataPointer);
	#endif
	                        h0.Free();
	
	                        GL.TexSubImage2D(SURFACE_TYPE, level, 0, 0, textureWidth, textureHeight, format,
	                                          PixelType.UnsignedByte, dataPointer);
	                    }
	                }
	                else
	                {
	#if IPHONE
	                    GL.TexImage2D(SURFACE_TYPE, level, (int)internalFormat, textureWidth, textureHeight, 0, format,
	                                    PixelType.UnsignedByte, dataPointer);
	#else
	                    GL.TexImage2D(SURFACE_TYPE, level, internalFormat, textureWidth, textureHeight, 0, format,
	                                    PixelType.UnsignedByte, dataPointer);
	#endif
	                }
	            }
	            else
	            {
	                GL.TexSubImage2D(SURFACE_TYPE, level, 0, 0, textureWidth, textureHeight, format,
	                                   PixelType.UnsignedByte, dataPointer);
	            }
			}
            

            if (GL.GetError() != 0)
            {
                Console.WriteLine("something go wrong!");
                //error occurred - rollback texture
                Delete();
            }
        }
    }
}