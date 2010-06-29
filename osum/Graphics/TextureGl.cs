using System;
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using osum.Helpers;
using osum.Graphics.Sprites;

namespace osum.Graphics
{
    public class TextureGl : IDisposable
    {
        private readonly int potHeight;
        private readonly int potWidth;
        private readonly int textureHeight;
        private readonly int textureWidth;
        private int textureId;
        public bool Loaded { get { return textureId > 0; } }

        public TextureGl(int width, int height)
        {
            textureId = -1;
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
            if (textureId == -1)
                return;

            try
            {
                if (GL.IsTexture(textureId))
                {
                    int[] textures = new[] {textureId};
                    GL.DeleteTextures(1, textures);
                }
            }
            catch
            {
            }

            textureId = -1;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Delete();
            }
        }

        /// <summary>
        /// Blits sprite to OpenGL display with specified parameters.
        /// </summary>
        public void Draw(Vector2 currentPos, Vector2 origin, Color4 drawColour, Vector2 scaleVector, float rotation,
                         Box2? srcRect, SpriteEffect effect)
        {
            if (textureId < 0)
                return;

            Box2 drawRect = srcRect == null ? new Box2(0, 0, textureWidth, textureHeight) : srcRect.Value;

            float drawHeight = drawRect.Height*scaleVector.Y;
            float drawWidth = drawRect.Width*scaleVector.X;

            Vector2 originVector = new Vector2(origin.X*drawWidth/drawRect.Width, origin.Y*drawHeight/drawRect.Height);

            bool verticalFlip = (effect & SpriteEffect.FlipVertically) > 0;
            bool horizontalFlip = (effect & SpriteEffect.FlipHorizontally) > 0;
            
            GL.Color4(drawColour);

            //GL.PushMatrix();
            GL.LoadIdentity();

            GL.Translate(currentPos.X, currentPos.Y, 0);
            GL.Rotate(OsumMathHelper.ToDegrees(rotation), 0, 0, 1.0f);
            
            if (originVector.X != 0 || originVector.Y != 0)
                GL.Translate(-originVector.X, -originVector.Y, 0);

            GL.BindTexture(SURFACE_TYPE, textureId);

            GL.Begin(BeginMode.Quads);

            if (SURFACE_TYPE == TextureTarget.Texture2D)
            {
                float left = (float) drawRect.Left/potWidth;
                float right = (float) drawRect.Right/potWidth;
                float top = (float) drawRect.Top/potHeight;
                float bottom = (float) drawRect.Bottom/potHeight;

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

            //GL.PopMatrix();
        }

        internal static void DisableTexture()
        {
            switch (SURFACE_TYPE)
            {
                case TextureTarget.Texture2D:
                    GL.Disable(EnableCap.Texture2D);
                    break;
                case TextureTarget.TextureRectangle:
                    GL.Disable(EnableCap.TextureRectangleArb);
                    break;
            }
        }

        internal static void EnableTexture()
        {
            switch (SURFACE_TYPE)
            {
                case TextureTarget.Texture2D:
                    GL.Enable(EnableCap.Texture2D);
                    break;
                case TextureTarget.TextureRectangle:
                    GL.Enable(EnableCap.TextureRectangleArb);
                    break;
            }
        }


        public void SetData(int textureId)
        {
            this.textureId = textureId;
        }

        public void SetData(byte[] data)
        {
            SetData(data, 0, PixelFormat.Bgra);
        }

        public void SetData(byte[] data, int level)
        {
            SetData(data, level, PixelFormat.Bgra);
        }

        /// <summary>
        /// Load texture data from a raw byte array (BGRA 32bit format)
        /// </summary>
        public void SetData(byte[] data, int level, PixelFormat format)
        {
            GCHandle h0 = GCHandle.Alloc(data, GCHandleType.Pinned);
            SetData(h0.AddrOfPinnedObject(), level, format);
            h0.Free();
        }

        internal static int GetPotDimension(int size)
        {
            int pot = 1;
            while (pot < size)
                pot *= 2;
            return pot;
        }

        const TextureTarget SURFACE_TYPE = TextureTarget.TextureRectangle;

        /// <summary>
        /// Load texture data from a raw IntPtr location (BGRA 32bit format)
        /// </summary>
        public void SetData(IntPtr dataPointer, int level, PixelFormat format)
        {
            if (format == 0)
                format = PixelFormat.Bgra;

            GL.GetError(); //Clear errors.

            bool newTexture = false;

            if (level == 0 && textureId < 0)
            {
                Delete();
                newTexture = true;
                int[] textures = new int[1];
                GL.GenTextures(1, textures);
                textureId = textures[0];
            }

            if (level > 0)
                return;

            GL.BindTexture(SURFACE_TYPE, textureId);

            //GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float)TextureEnvMode.Replace);

            if (newTexture)
            {
                if (SURFACE_TYPE == TextureTarget.Texture2D)
                {
                    if (potWidth == textureWidth && potHeight == textureHeight)
                    {
                        GL.TexImage2D(SURFACE_TYPE, level, PixelInternalFormat.Rgba, potWidth, potHeight, 0, format,
                                        PixelType.UnsignedByte, dataPointer);
                    }
                    else
                    {
                        byte[] temp = new byte[potWidth*potHeight*4];
                        GCHandle h0 = GCHandle.Alloc(temp, GCHandleType.Pinned);
                        IntPtr pinnedDataPointer = h0.AddrOfPinnedObject();
                        GL.TexImage2D(SURFACE_TYPE, level, PixelInternalFormat.Rgba, potWidth, potHeight, 0, format,
                                        PixelType.UnsignedByte, pinnedDataPointer);
                        h0.Free();

                        GL.TexSubImage2D(SURFACE_TYPE, level, 0, 0, textureWidth, textureHeight, format,
                                           PixelType.UnsignedByte, dataPointer);
                    }
                }
                else
                {
                    GL.TexImage2D(SURFACE_TYPE, level, PixelInternalFormat.Rgba, textureWidth, textureHeight, 0, format,
                                    PixelType.UnsignedByte, dataPointer);
                }
            }
            else
            {
                GL.TexSubImage2D(SURFACE_TYPE, level, 0, 0, textureWidth, textureHeight, format,
                                   PixelType.UnsignedByte, dataPointer);
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Nearest);

            if (GL.GetError() != 0)
            {
                //error occurred - rollback texture
                Delete();
            }
        }
    }
}