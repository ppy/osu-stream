using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;

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
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
#else
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using osum.Input;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
#endif

using System.Text;
using OpenTK;
using osum.Graphics.Skins;


namespace osum.Graphics
{
    public class pTexture : IDisposable
    {
        public string assetName;
        public bool fromResourceStore;
        internal int Width;
        internal int Height;
        internal int X;
        internal int Y;
        internal bool Permanent;
#if DEBUG
        internal int id;
        internal static int staticid = 1;
#endif
        //public SkinSource Source;

        public pTexture(TextureGl textureGl, int width, int height)
        {
            TextureGl = textureGl;
            Width = width;
            Height = height;

#if DEBUG
            id = staticid++;
#endif
        }

        private pTexture()
        {
#if DEBUG
            id = staticid++;
#endif
        }

        internal TextureGl TextureGl;
        internal OsuTexture OsuTextureInfo;

        public bool IsDisposed { get; private set; }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of textures but leaves the GC finalizer in place.
        /// This is used to temporarily unload sprites which are not accessed in a while.
        /// </summary>
        internal void TemporalDispose()
        {
            if (TextureGl != null)
            {
                TextureGl.Dispose();
                TextureGl = null;
            }
        }

        private void Dispose(bool isDisposing)
        {
            if (TextureGl != null)
            {
                if (fboId >= 0)
                {
#if iOS
	                GL.Oes.DeleteFramebuffers(1,ref fboId);
	                fboId = -1;
#else
                    GL.DeleteFramebuffers(1, ref fboId);
                    fboId = -1;
#endif
                }

                TextureGl.Dispose();
                TextureGl = null;
            }

            IsDisposed = true;
        }

        /// <summary>
        /// Unloads texture without fully disposing. It may be able to be restored by calling ReloadIfPossible
        /// </summary>
        internal void UnloadTexture()
        {
            if (TextureGl != null)
            {
                TextureGl.Dispose();
                //TextureGl = null;
            }
        }

        internal bool ReloadIfPossible()
        {
            if (TextureGl == null || TextureGl.Id == -1)
            {
                if (assetName != null)
                {
                    
                    pTexture reloadedTexture = OsuTextureInfo != OsuTexture.None ? TextureManager.Load(OsuTextureInfo) : FromFile(assetName);
                    if (TextureGl == null)
                        TextureGl = reloadedTexture.TextureGl;
                    else
                    {
                        TextureGl.Id = reloadedTexture.TextureGl.Id;
                    }

                    reloadedTexture.TextureGl = null; //deassociate with temporary pTexture to avoid disposal.

                    return true;
                }

            }

            return false;
        }

        #endregion

        public void SetData(byte[] data)
        {
            SetData(data, 0, 0);
        }

        public void SetData(byte[] data, int level, PixelFormat format)
        {
            if (TextureGl != null)
            {
                if ((int)format != 0)
                    TextureGl.SetData(data, level, format);
                else
                    TextureGl.SetData(data, level);
            }
        }

        public static pTexture FromFile(string filename)
        {
            return FromFile(filename, false);
        }

        /// <summary>
        /// Read a pTexture from an arbritrary file.
        /// </summary>
        public static pTexture FromFile(string filename, bool mipmap)
        {
            //load base texture first...

            if (!File.Exists(filename)) return null;

            pTexture tex = null;

            try
            {
#if iOS
				using (UIImage image = UIImage.FromFile(filename))
                    tex = FromUIImage(image,filename);
#else
                using (Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                    tex = FromStream(stream, filename);
#endif
            }
            catch
            {
                return null;
            }

            if (mipmap)
            {
                int mipmapLevel = 1;

                int width = tex.Width;
                int height = tex.Height;

                do
                {
                    string mmfilename = filename.Replace(".", mipmapLevel + ".");
                    if (!File.Exists(mmfilename))
                        break;

                    width /= 2;
                    height /= 2;

#if iOS
					using (UIImage textureImage = UIImage.FromFile(mmfilename))
					{
						IntPtr pTextureData = Marshal.AllocHGlobal(width * height * 4);
				
						using (CGBitmapContext textureContext = new CGBitmapContext(pTextureData,
			                        width, height, 8, width * 4,
			                        textureImage.CGImage.ColorSpace, CGImageAlphaInfo.PremultipliedLast))
			            	textureContext.DrawImage(new RectangleF (0, 0, width, height), textureImage.CGImage);
						
			            tex.TextureGl.SetData(pTextureData,mipmapLevel,0);
						
						Marshal.FreeHGlobal(pTextureData);
					}
#endif


                    mipmapLevel++;
                } while (true);

            }

            return tex;


        }

        public static pTexture FromStream(Stream stream, string assetname)
        {
            return FromStream(stream, assetname, false);
        }

#if iOS
		public unsafe static pTexture FromUIImage(UIImage textureImage, string assetname)
		{
            if (textureImage == null)
                return null;

            int width = (int)textureImage.Size.Width;
            int height = (int)textureImage.Size.Height;

			IntPtr pTextureData = Marshal.AllocHGlobal(width * height * 4);
			
			
#if SIMULATOR
			//on the simulator we get texture corruption without this.
			byte[] bytes = new byte[width * height * 4];
			Marshal.Copy(bytes, 0, pTextureData,bytes.Length);
#endif
			
			
			using (CGBitmapContext textureContext = new CGBitmapContext(pTextureData,
                        width, height, 8, width * 4,
                        textureImage.CGImage.ColorSpace, CGImageAlphaInfo.PremultipliedLast))
            	textureContext.DrawImage(new RectangleF (0, 0, width, height), textureImage.CGImage);
			
            pTexture tex = FromRawBytes(pTextureData, width, height);
			
			Marshal.FreeHGlobal(pTextureData);
			
			tex.assetName = assetname;
			return tex;
		}
#endif

        /// <summary>
        /// Read a pTexture from an arbritrary file.
        /// </summary>
        public unsafe static pTexture FromStream(Stream stream, string assetname, bool saveToFile)
        {
            try
            {

                pTexture pt;
#if iOS
				pt = FromUIImage(UIImage.LoadFromData(NSData.FromStream(stream)),assetname);
#else
                using (Bitmap b = (Bitmap)Image.FromStream(stream, false, false))
                {
                    BitmapData data = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadOnly,
                                                 System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    if (saveToFile)
                    {
                        byte[] bitmap = new byte[b.Width * b.Height * 4];
                        Marshal.Copy(data.Scan0, bitmap, 0, bitmap.Length);
                        File.WriteAllBytes(assetname, bitmap);
                    }

                    pt = FromRawBytes(data.Scan0, b.Width, b.Height);
                    pt.assetName = assetname;
                    b.UnlockBits(data);

                }
#endif

                //This makes sure we are always at the correct sprite resolution.
                //Fucking hack, or fucking hax?
                pt.Width = (int)(pt.Width * 960f / GameBase.SpriteSheetResolution);
                pt.Height = (int)(pt.Height * 960f / GameBase.SpriteSheetResolution);
                pt.TextureGl.TextureWidth = pt.Width;
                pt.TextureGl.TextureHeight = pt.Height;

                return pt;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        public static pTexture FromBytes(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
                return FromStream(ms, "");
        }

        public static pTexture FromBytesSaveRawToFile(byte[] data, string filename)
        {
            using (MemoryStream ms = new MemoryStream(data))
                return FromStream(ms, filename, true);
        }

        public static pTexture FromRawBytes(IntPtr location, int width, int height)
        {
            pTexture pt = new pTexture();

            pt.Width = width;
            pt.Height = height;

            try
            {
                pt.TextureGl = new TextureGl(pt.Width, pt.Height);
                pt.TextureGl.SetData(location, 0, 0);
            }
            catch
            {
            }

            return pt;
        }

        public static pTexture FromRawBytes(byte[] bitmap, int width, int height)
        {
            pTexture pt = new pTexture();
            pt.Width = width;
            pt.Height = height;

            try
            {
                pt.TextureGl = new TextureGl(pt.Width, pt.Height);
                pt.SetData(bitmap);
            }
            catch
            {
            }

            return pt;
        }

        /*public static pTexture FromText(string text, SizeF dim, UITextAlignment alignment, string fontName, float fontSize) {
            UIFont font = UIFont.FromName(fontName, fontSize);
			
            int width = (int)dim.Width;
            if (width != 1 && (width & (width - 1)) != 0) {
                int i = 1;
                while (i < width) {
                    i *= 2;
                }
				
                width = i;
            }
			
            int height = (int)dim.Height;
            if (height != 1 && (height & (height - 1)) != 0) {
                int i = 1;
                while (i < height) {
                    i *= 2;
                }
                height = i;
            }
			
            CGColorSpace colorSpace = CGColorSpace.CreateDeviceRGB(); //CGColorSpace.CreateDeviceGray();
			
            byte[] data = new byte[width * height];
			
            unsafe {
                fixed (byte* dataPb = data) {
                    using (CGContext context = new CGBitmapContext((IntPtr)dataPb, width, height, 8, width, colorSpace, CGImageAlphaInfo.None)) {
                        context.SetGrayFillColor(1f, 1f);
                        context.TranslateCTM(0f, height);
                        context.ScaleCTM(1f, -1f);
                        UIGraphics.PushContext(context);
                        //text.DrawInRect(new RectangleF(0, 0, dim.Width, dim.Height), font, UILineBreakMode.WordWrap, alignment);
                        UIGraphics.PopContext();
                    }
                }
            }
            colorSpace.Dispose();
			
            return null;
            //FromRawBytes(
            //InitWithData(data, Texture2DPixelFormat.A8, width, height, dim);
        }*/

        internal int fboId = -1;
        internal int fboDepthBuffer = -1;

        internal int BindFramebuffer()
        {
            if (fboId >= 0)
                return fboId;

#if iOS
            int oldFBO = 0;
			GL.GetInteger(All.FramebufferBindingOes, ref oldFBO);
			
			// create framebuffer
            GL.Oes.GenFramebuffers(1, ref fboId);
            GL.Oes.BindFramebuffer(All.FramebufferOes, fboId);

            // attach renderbuffer
            GL.Oes.FramebufferTexture2D(All.FramebufferOes, All.ColorAttachment0Oes, All.Texture2D, TextureGl.Id, 0);

            // unbind frame buffer
            GL.Oes.BindFramebuffer(All.FramebufferOes, oldFBO);
#else
            // make depth buffer
            GL.GenRenderbuffers(1, out fboDepthBuffer);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, fboDepthBuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent16, Width, Height);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

            GL.GenFramebuffers(1, out fboId);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fboId);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureGl.SURFACE_TYPE, TextureGl.Id, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, fboDepthBuffer);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
#endif

            return fboId;
        }

        internal pTexture Clone()
        {
            return (pTexture)this.MemberwiseClone();
        }
    }

    /// <summary>
    /// BinaryWriter exposing protected MS function.
    /// </summary>
    internal class HaxBinaryReader : BinaryReader
    {
        public HaxBinaryReader(Stream input)
            : base(input)
        {
        }

        public HaxBinaryReader(Stream input, Encoding encoding)
            : base(input, encoding)
        {
        }

        public new int Read7BitEncodedInt()
        {
            byte num3;
            int num = 0;
            int num2 = 0;
            do
            {
                if (num2 == 0x23)
                {
                    return -1;
                }
                num3 = ReadByte();
                num |= (num3 & 0x7f) << num2;
                num2 += 7;
            } while ((num3 & 0x80) != 0);
            return num;
        }
    }
}