using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenTK.Graphics.OpenGL;

namespace osu.Graphics.OpenGl
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

            if (SURFACE_TYPE == GL._TEXTURE_2D)
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
                if (GL.IsTexture(textureId) > 0)
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
        public void Draw(Vector2 currentPos, Vector2 origin, Color drawColour, Vector2 scaleVector, float rotation,
                         Rectangle? srcRect, SpriteEffects effect)
        {
            if (textureId < 0)
                return;

            //GL.Enable(SURFACE_TYPE);

            Rectangle drawRect = srcRect == null ? new Rectangle(0, 0, textureWidth, textureHeight) : srcRect.Value;


            float drawHeight = drawRect.Height*scaleVector.Y;
            float drawWidth = drawRect.Width*scaleVector.X;

            Vector2 originVector = new Vector2(origin.X*drawWidth/drawRect.Width, origin.Y*drawHeight/drawRect.Height);

            bool verticalFlip = (effect & SpriteEffects.FlipVertically) > 0;
            bool horizontalFlip = (effect & SpriteEffects.FlipHorizontally) > 0;
            
            GL.Color4ub(drawColour.R, drawColour.G, drawColour.B, drawColour.A);

            GL.BindTexture(SURFACE_TYPE, textureId);

            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Translatef(currentPos.X, currentPos.Y, 0);
            GL.Rotatef(MathHelper.ToDegrees(rotation), 0, 0, 1.0f);
            GL.Translatef(-originVector.X, -originVector.Y, 0);

            GL.Begin(GL._QUADS);

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

            GL.PopMatrix();

            //GL.Disable(SURFACE_TYPE);
        }


        public void SetData(int textureId)
        {
            this.textureId = textureId;
        }

        public void SetData(byte[] data)
        {
            SetData(data, 0, GL._BGRA);
        }

        public void SetData(byte[] data, int level)
        {
            SetData(data, level, GL.);
        }

        /// <summary>
        /// Load texture data from a raw byte array (BGRA 32bit format)
        /// </summary>
        public void SetData(byte[] data, int level, int format)
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
        public void SetData(IntPtr dataPointer, int level, int format)
        {
            if (format == 0)
                format = GL._BGRA;

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

            //GL.Enable(EnableCap.Text);

            GL.BindTexture(SURFACE_TYPE, textureId);
            GL.TexParameterI(SURFACE_TYPE, TextureParameterName.TextureMinFilter, new int[]{(int)TextureMinFilter.Linear});
            GL.TexParameteri(SURFACE_TYPE, GL._TEXTURE_MAG_FILTER, (int) GL._LINEAR);

            if (newTexture)
            {
                if (SURFACE_TYPE == GL._TEXTURE_2D)
                {
                    if (potWidth == textureWidth && potHeight == textureHeight)
                    {
                        GL.TexImage2D(SURFACE_TYPE, level, GL._RGBA, potWidth, potHeight, 0, format,
                                        GL._UNSIGNED_BYTE, dataPointer);
                    }
                    else
                    {
                        byte[] temp = new byte[potWidth*potHeight*4];
                        GCHandle h0 = GCHandle.Alloc(temp, GCHandleType.Pinned);
                        GL.TexImage2D(SURFACE_TYPE, level, GL._RGBA, potWidth, potHeight, 0, format,
                                        GL._UNSIGNED_BYTE, h0.AddrOfPinnedObject());
                        h0.Free();

                        GL.TexSubImage2D(SURFACE_TYPE, level, 0, 0, textureWidth, textureHeight, format,
                                           GL._UNSIGNED_BYTE, dataPointer);
                    }
                }
                else
                {
                    GL.TexImage2D(SURFACE_TYPE, level, GL._RGBA, textureWidth, textureHeight, 0, format,
                                    GL._UNSIGNED_BYTE, dataPointer);
                }
            }
            else
            {
                GL.TexSubImage2D(SURFACE_TYPE, level, 0, 0, textureWidth, textureHeight, format,
                                   GL._UNSIGNED_BYTE, dataPointer);
            }

            //GL.Disable(SURFACE_TYPE);

            if (GL.GetError() != 0)
            {
                //error occurred - rollback texture
                Delete();
            }
        }
    }
}