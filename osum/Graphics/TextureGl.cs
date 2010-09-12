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

            if (SURFACE_TYPE == TextureTarget.Texture2D)
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
            if (disposing)
            {
                Delete();
            }
        }

        private void checkGlError ()
        {
        	ErrorCode error = GL.GetError ();
        	if (error != ErrorCode.NoError)
            {
        		Console.WriteLine ("GL Error: " + error);
        	}
        }
		
        /// <summary>
        /// Blits sprite to OpenGL display with specified parameters.
        /// </summary>
        public void Draw(Vector2 currentPos, Vector2 origin, Color4 drawColour, Vector2 scaleVector, float rotation,
                         Box2? srcRect, SpriteEffect effect)
        {
			if (Id < 0)
                return;
                
			GL.PushMatrix();

            Box2 drawRect = srcRect == null ? new Box2(0, 0, textureWidth, textureHeight) : srcRect.Value;

            float drawHeight = drawRect.Height * scaleVector.Y;
            float drawWidth = drawRect.Width * scaleVector.X;

            Vector2 originVector = new Vector2(origin.X * drawWidth / drawRect.Width, origin.Y * drawHeight / drawRect.Height);

            bool verticalFlip = (effect & SpriteEffect.FlipVertically) > 0;
            bool horizontalFlip = (effect & SpriteEffect.FlipHorizontally) > 0;

#if IPHONE
            GL.Color4(drawColour.R,drawColour.G,drawColour.B,drawColour.A);
			
			GL.Translate(currentPos.X, currentPos.Y, 0);
			
			if (rotation != 0)
		        GL.Rotate(pMathHelper.ToDegrees(rotation), 0, 0, 1.0f);

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

			GL.BindTexture(TextureTarget.Texture2D, Id);
						
			GL.VertexPointer(3, All.Float, 0, vertices);
			GL.TexCoordPointer(2, All.Float, 0, coordinates);
			
			GL.DrawArrays (All.TriangleFan, 0, 4);
#else
            GL.Color4(drawColour);

            checkGlError();

            //GL.PushMatrix();
            GL.LoadIdentity();

            GL.Translate(currentPos.X, currentPos.Y, 0);
            GL.Rotate(pMathHelper.ToDegrees(rotation), 0, 0, 1.0f);

            if (originVector.X != 0 || originVector.Y != 0)
                GL.Translate(-originVector.X, -originVector.Y, 0);

            GL.BindTexture(SURFACE_TYPE, Id);

            GL.Begin(BeginMode.Quads);

            if (SURFACE_TYPE == TextureTarget.Texture2D)
            {
                float left = (float)drawRect.Left / potWidth;
                float right = (float)drawRect.Right / potWidth;
                float top = (float)drawRect.Top / potHeight;
                float bottom = (float)drawRect.Bottom / potHeight;

                GL.TexCoord2(horizontalFlip ? right : left, verticalFlip ? top : bottom);
                GL.Vertex2(0, drawHeight);

                GL.TexCoord2(horizontalFlip ? left : right, verticalFlip ? top : bottom);
                GL.Vertex2(drawWidth, drawHeight);

                GL.TexCoord2(horizontalFlip ? left : right, verticalFlip ? bottom : top);
                GL.Vertex2(drawWidth, 0);

                GL.TexCoord2(horizontalFlip ? right : left, verticalFlip ? bottom : top);
                GL.Vertex2(0, 0);
            }
            else
            {
                GL.TexCoord2(horizontalFlip ? drawRect.Right : drawRect.Left,
                                verticalFlip ? drawRect.Top : drawRect.Bottom);
                GL.Vertex2(0, drawHeight);

                GL.TexCoord2(horizontalFlip ? drawRect.Left : drawRect.Right,
                                verticalFlip ? drawRect.Top : drawRect.Bottom);
                GL.Vertex2(drawWidth, drawHeight);

                GL.TexCoord2(horizontalFlip ? drawRect.Left : drawRect.Right,
                                verticalFlip ? drawRect.Bottom : drawRect.Top);
                GL.Vertex2(drawWidth, 0);

                GL.TexCoord2(horizontalFlip ? drawRect.Right : drawRect.Left,
                                verticalFlip ? drawRect.Bottom : drawRect.Top);
                GL.Vertex2(0, 0);
            }

            GL.End();

            checkGlError();
#endif

            GL.PopMatrix();
        }

        internal static void DisableTexture()
        {
            switch (SURFACE_TYPE)
            {
                case TextureTarget.Texture2D:
                    GL.Disable(EnableCap.Texture2D);
                    break;
#if !IPHONE
                case TextureTarget.TextureRectangle:
                    GL.Disable(EnableCap.Texture2D);
                    break;
#endif
            }
        }

        internal static void EnableTexture()
        {
            switch (SURFACE_TYPE)
            {
                case TextureTarget.Texture2D:
                    GL.Enable(EnableCap.Texture2D);
                    break;
#if !IPHONE
                case TextureTarget.TextureRectangle:
                    GL.Enable(EnableCap.Texture2D);
                    break;
#endif
            }
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

            GL.GetError ();
        	//Clear errors.

            bool newTexture = false;

            if (level == 0 && Id < 0)
            {
        		Delete ();
        		newTexture = true;
        		int[] textures = new int[1];
        		GL.GenTextures (1, textures);
        		Id = textures[0];
#if DEBUG
        		Console.WriteLine ("TextureGl assigned: " + Id);
#endif
        	}

            if (level > 0)
        		return;

       		GL.BindTexture (SURFACE_TYPE, Id);

			//Nearest gives ~30% more draw performance, but looks a bit shitty.
            GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
        	GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
			
			//can't determine if this helps
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);

			//doesn't seem to help much at all? maybe best to test once more...
            //GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float)All.Replace);

            if (newTexture)
            {
                if (SURFACE_TYPE == TextureTarget.Texture2D)
                {
                    if (potWidth == textureWidth && potHeight == textureHeight)
                    {
#if IPHONE
                        GL.TexImage2D(SURFACE_TYPE, level, (int)PixelInternalFormat.Rgba, potWidth, potHeight, 0, format,
                                        PixelType.UnsignedByte, dataPointer);
#else
                        GL.TexImage2D(SURFACE_TYPE, level, PixelInternalFormat.Rgba, potWidth, potHeight, 0, format,
                                        PixelType.UnsignedByte, dataPointer);
#endif
                    }
                    else
                    {
                        byte[] temp = new byte[potWidth * potHeight * 4];
                        GCHandle h0 = GCHandle.Alloc(temp, GCHandleType.Pinned);
                        IntPtr pinnedDataPointer = h0.AddrOfPinnedObject();
#if IPHONE
                        GL.TexImage2D(SURFACE_TYPE, level, (int)PixelInternalFormat.Rgba, potWidth, potHeight, 0, format,
                                        PixelType.UnsignedByte, pinnedDataPointer);
#else
                        GL.TexImage2D(SURFACE_TYPE, level, PixelInternalFormat.Rgba, potWidth, potHeight, 0, format,
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
                    GL.TexImage2D(SURFACE_TYPE, level, (int)PixelInternalFormat.Rgba, textureWidth, textureHeight, 0, format,
                                    PixelType.UnsignedByte, dataPointer);
#else
                    GL.TexImage2D(SURFACE_TYPE, level, PixelInternalFormat.Rgba, textureWidth, textureHeight, 0, format,
                                    PixelType.UnsignedByte, dataPointer);
#endif
                }
            }
            else
            {
                GL.TexSubImage2D(SURFACE_TYPE, level, 0, 0, textureWidth, textureHeight, format,
                                   PixelType.UnsignedByte, dataPointer);
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