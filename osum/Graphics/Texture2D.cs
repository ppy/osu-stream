// CocosNet, Cocos2D in C#
// Copyright 2009 Matthew Greer
// See LICENSE file for license, and README and AUTHORS for more info

using System;
using OpenTK.Graphics.ES11;
using System.Drawing;
using System.ComponentModel;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using System.Runtime.InteropServices;

namespace osum.Graphics {

	public enum Texture2DPixelFormat {
		Automatic = 0,
		RGBA8888,
		RGB565,
		A8,
		RGBA4444,
		RGB5A1,
		Default = RGBA8888
	}

	public class TexParams {
		public All MinFilter { get; set; }
		public All MagFilter { get; set; }
		public All WrapS { get; set; }
		public All WrapT { get; set; }
	}

	/// <summary>
	/// This is the same Texture2D class that Apple
	/// shipped as sample code early on.
	/// </summary>
	public class Texture2D : IDisposable {
		public const int MaxTextureSize = 1024;
		private uint _name;

		public SizeF ContentSize { get; private set; }
		public Texture2DPixelFormat PixelFormat { get; private set; }
		public int PixelsWide { get; private set; }
		public int PixelsHigh { get; private set; }
		public float MaxS { get; private set; }
		public float MaxT { get; private set; }
		public bool HasPremultipliedAlpha { get; private set; }
		public uint Name {
			get { return _name; }
			set { _name = value; }
		}

		public void Dispose() {
			if (Name != 0) {
				GL.DeleteTextures(1, ref _name);
			}
			GC.SuppressFinalize(this);
		}
		
		private void SetTexParameters(TexParams texParams) {
			GL.BindTexture(All.Texture2D, _name);
			GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int)texParams.MinFilter);
			GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int)texParams.MagFilter);
			GL.TexParameter(All.Texture2D, All.TextureWrapS, (int)texParams.WrapS);
			GL.TexParameter(All.Texture2D, All.TextureWrapT, (int)texParams.WrapT);
		}

		private void SetAntiAliasTexParameters() {
			TexParams texParams = new TexParams();
			texParams.MinFilter = All.Nearest;
			texParams.MagFilter = All.Nearest;
			texParams.WrapS = All.ClampToEdge;
			texParams.WrapT = All.ClampToEdge;
			
			SetTexParameters(texParams);
		}

		private void InitWithData(byte[] data, Texture2DPixelFormat pixelFormat, int pixelsWide, int pixelsHigh, SizeF contentSize) {
			GL.GenTextures(1, ref _name);

            Console.WriteLine("Texture2D assigned: " + _name);
			
			GL.BindTexture(All.Texture2D, _name);
			
			SetAntiAliasTexParameters();
			
			switch (pixelFormat) {
				case Texture2DPixelFormat.RGBA8888:
					GL.TexImage2D(All.Texture2D, 0, (int)All.Rgba, pixelsWide, pixelsHigh, 0, All.Rgba, All.UnsignedByte, data);
					break;
				case Texture2DPixelFormat.RGBA4444:
					GL.TexImage2D(All.Texture2D, 0, (int)All.Rgba, pixelsWide, pixelsHigh, 0, All.Rgba, All.UnsignedShort4444, data);
					break;
				case Texture2DPixelFormat.RGB5A1:
					GL.TexImage2D(All.Texture2D, 0, (int)All.Rgba, pixelsWide, pixelsHigh, 0, All.Rgba, All.UnsignedShort5551, data);
					break;
				case Texture2DPixelFormat.RGB565:
					GL.TexImage2D(All.Texture2D, 0, (int)All.Rgb, pixelsWide, pixelsHigh, 0, All.Rgb, All.UnsignedShort565, data);
					break;
				case Texture2DPixelFormat.A8:
					GL.TexImage2D(All.Texture2D, 0, (int)All.Alpha, pixelsWide, pixelsHigh, 0, All.Alpha, All.UnsignedByte, data);
					break;
				default:
					throw new InvalidEnumArgumentException("pixelFormat", (int)pixelFormat, typeof(Texture2DPixelFormat));
			}
			
			ContentSize = contentSize;
			PixelsWide = pixelsWide;
			PixelsHigh = pixelsHigh;
			PixelFormat = pixelFormat;
			MaxS = ContentSize.Width / PixelsWide;
			MaxT = ContentSize.Height / PixelsHigh;
			
			HasPremultipliedAlpha = false;
		}

		public Texture2D(string text, string fontName, float fontSize) : this(text, /*text.SizeWithFont(UIFont.FromName(fontName, fontSize))*/ new SizeF(1,2), UITextAlignment.Center, fontName, fontSize) {
		}

		public Texture2D(string text, SizeF dim, UITextAlignment alignment, string fontName, float fontSize) {
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
			
			CGColorSpace colorSpace = CGColorSpace.CreateDeviceGray();
			
			byte[] data = new byte[width * height];
			
			unsafe {
				fixed (byte* dataPb = data) {
					using (CGContext context = new CGBitmapContext((IntPtr)dataPb, width, height, 8, width, colorSpace, CGImageAlphaInfo.None)) {
						context.SetGrayFillColor(1f, 1f);
						context.TranslateCTM(0f, height);
						context.ScaleCTM(1f, -1f);
						UIGraphics.PushContext(context);
						//todo: fix
						//text.DrawInRect(new RectangleF(0, 0, dim.Width, dim.Height), font, UILineBreakMode.WordWrap, alignment);
						UIGraphics.PopContext();
					}
				}
			}
			colorSpace.Dispose();
			
			InitWithData(data, Texture2DPixelFormat.A8, width, height, dim);
		}

		public Texture2D(UIImage uiImage) {
			if (uiImage == null) {
				throw new ArgumentNullException("uiImage");
			}
			
			CGImage image = uiImage.CGImage;
			
			if (image == null) {
				throw new InvalidOperationException("Attempted to create a Texture2D from UIImage, but resulting CGImage is null");
			}
			
			CGImageAlphaInfo info = image.AlphaInfo;
			bool hasAlpha = info == CGImageAlphaInfo.PremultipliedLast || info == CGImageAlphaInfo.PremultipliedFirst || info == CGImageAlphaInfo.Last || info == CGImageAlphaInfo.First;
			
			int bpp = image.BitsPerComponent;
			
			Texture2DPixelFormat pixelFormat;
			
			if (image.ColorSpace != null) {
				if (hasAlpha || bpp >= 8) {
					pixelFormat = Texture2DPixelFormat.Default;
				} else {
					pixelFormat = Texture2DPixelFormat.RGB565;
				}
			} else {
				pixelFormat = Texture2DPixelFormat.A8;
			}
			
			int width = image.Width;
			if (width != 1 && (width & (width - 1)) != 0) {
				int i = 1;
				while (i < width) {
					i *= 2;
				}
				
				width = i;
			}
			
			int height = image.Height;
			if (height != 1 && (height & (height - 1)) != 0) {
				int i = 1;
				while (i < height) {
					i *= 2;
				}
				height = i;
			}
			
			if (width > MaxTextureSize || height > MaxTextureSize) {
				throw new InvalidOperationException("Image is too large. Width or height larger than MaxTextureSize");
			}
			
			CGColorSpace colorSpace = null;
			CGContext context;
			byte[] data;
			
			unsafe {
				// all formats require w*h*4, except A8 requires just w*h
				int dataSize = width * height * 4;
				if (pixelFormat == Texture2DPixelFormat.A8) {
					dataSize = width * height;
				}
				
				data = new byte[dataSize];
				fixed (byte* dp = data) {
					switch (pixelFormat) {
						case Texture2DPixelFormat.RGBA8888:
						case Texture2DPixelFormat.RGBA4444:
						case Texture2DPixelFormat.RGB5A1:
							colorSpace = CGColorSpace.CreateDeviceRGB();
							context = new CGBitmapContext((IntPtr)dp, (int)width, (int)height, 8, 4 * (int)width, colorSpace, CGImageAlphaInfo.PremultipliedLast);
							break;
						case Texture2DPixelFormat.RGB565:
							colorSpace = CGColorSpace.CreateDeviceRGB();
							context = new CGBitmapContext((IntPtr)dp, (int)width, (int)height, 8, 4 * (int)width, colorSpace, CGImageAlphaInfo.NoneSkipLast);
							break;
						case Texture2DPixelFormat.A8:
							context = new CGBitmapContext((IntPtr)dp, (int)width, (int)height, 8, (int)width, null, CGImageAlphaInfo.Only);
							break;
						default:
							throw new InvalidEnumArgumentException("pixelFormat", (int)pixelFormat, typeof(Texture2DPixelFormat));
					}
					
					if (colorSpace != null) {
						colorSpace.Dispose();
					}
					
					context.ClearRect(new RectangleF(0, 0, width, height));
					context.TranslateCTM(0, height - image.Height);
					
					// why is this here? make an identity transform, then immediately not use it? Need to look into this
					CGAffineTransform transform = CGAffineTransform.MakeIdentity();
					
					if (!transform.IsIdentity) {
						context.ConcatCTM(transform);
					}
					
					context.DrawImage(new RectangleF(0, 0, image.Width, image.Height), image);
				}
			}
			
			if (pixelFormat == Texture2DPixelFormat.RGB565) {
				//Convert "RRRRRRRRGGGGGGGGBBBBBBBBAAAAAAAA" to "RRRRRGGGGGGBBBBB"
				byte[] tempData = new byte[height * width * 2];
				
				
				unsafe {
					fixed (byte* inPixel32b = data) {
						uint* inPixel32 = (uint*)inPixel32b;
						fixed (byte* outPixel16b = tempData) {
							ushort* outPixel16 = (ushort*)outPixel16b;
							for (int i = 0; i < width * height; ++i,++inPixel32) {
								uint tempInt32 = ((((*inPixel32 >> 0) & 0xff) >> 3) << 11) | ((((*inPixel32 >> 8) & 0xff) >> 2) << 5) | ((((*inPixel32 >> 16) & 0xff) >> 3) << 0);
								*outPixel16++ = (ushort)tempInt32;
							}
						}
					}
				}
				data = tempData;
				
			} else if (pixelFormat == Texture2DPixelFormat.RGBA4444) {
				//Convert "RRRRRRRRGGGGGGGGBBBBBBBBAAAAAAAA" to "RRRRGGGGBBBBAAAA"
				byte[] tempData = new byte[height * width * 2];
				
				unsafe {
					fixed (byte* inPixel32b = data) {
						uint* inPixel32 = (uint*)inPixel32b;
						fixed (byte* outPixel16b = tempData) {
							ushort* outPixel16 = (ushort*)outPixel16b;
							for (int i = 0; i < width * height; ++i,++inPixel32) {
								uint tempInt32 = ((((*inPixel32 >> 0) & 0xff) >> 4) << 12) | ((((*inPixel32 >> 8) & 0xff) >> 4) << 8) | ((((*inPixel32 >> 16) & 0xff) >> 4) << 4) | ((((*inPixel32 >> 24) & 0xff) >> 4) << 0);
								*outPixel16++ = (ushort)tempInt32;
							}
						}
					}
				}
				data = tempData;
				
			} else if (pixelFormat == Texture2DPixelFormat.RGB5A1) {
				//Convert "RRRRRRRRGGGGGGGGBBBBBBBBAAAAAAAA" to "RRRRRGGGGGBBBBBA"
				byte[] tempData = new byte[height * width * 2];
				
				unsafe {
					fixed (byte* inPixel32b = data) {
						uint* inPixel32 = (uint*)inPixel32b;
						fixed (byte* outPixel16b = tempData) {
							ushort* outPixel16 = (ushort*)outPixel16b;
							for (int i = 0; i < width * height; ++i,++inPixel32) {
								uint tempInt32 = ((((*inPixel32 >> 0) & 0xff) >> 3) << 11) | ((((*inPixel32 >> 8) & 0xff) >> 3) << 6) | ((((*inPixel32 >> 16) & 0xff) >> 3) << 1) | ((((*inPixel32 >> 24) & 0xff) >> 7) << 0);
								*outPixel16++ = (ushort)tempInt32;
							}
						}
					}
				}
				data = tempData;
				
			}
			
			InitWithData(data, pixelFormat, width, height, new SizeF(image.Width, image.Height));
			
			HasPremultipliedAlpha = info == CGImageAlphaInfo.PremultipliedLast || info == CGImageAlphaInfo.PremultipliedFirst;
			
			context.Dispose();
		}
		
/*		public Texture2D(string pvrFile) {
			PVRTexture pvr = new PVRTexture(pvrFile);
			pvr.RetainName = true;
			
			Name = pvr.Name;
			MaxS = 1.0f;
			MaxT = 1.0f;
			PixelsWide = (int)pvr.Width;
			PixelsHigh = (int)pvr.Height;
			this.ContentSize = new SizeF(pvr.Width, pvr.Height);
			
			SetAntiAliasTexParameters();
		}*/

		~Texture2D() {
			Dispose();
		}

		public void DrawAtPoint(PointF point) {
			float[] coordinates = {
				0f,
				MaxT,
				MaxS,
				MaxT,
				0f,
				0f,
				MaxS,
				0f
			};
			float width = (float)PixelsWide * MaxS, height = (float)PixelsHigh * MaxT;
			
			float[] vertices = {
				point.X,
				point.Y,
				0f,
				width + point.X,
				point.Y,
				0f,
				point.X,
				height + point.Y,
				0f,
				width + point.X,
				height + point.Y,
				0f
			};
			
			GL.BindTexture(All.Texture2D, _name);
			GL.VertexPointer(3, All.Float, 0, vertices);
			GL.TexCoordPointer(2, All.Float, 0, coordinates);
			GL.DrawArrays(All.TriangleStrip, 0, 4);
		}

		public void DrawInRect(RectangleF rect) {
			float[] coordinates = {
				0f,
				MaxT,
				MaxS,
				MaxT,
				0f,
				0f,
				MaxS,
				0f
			};
			
			float[] vertices = {
				rect.Location.X,
				rect.Location.Y,
				rect.Location.X + rect.Width,
				rect.Location.Y,
				rect.Location.X,
				rect.Location.Y + rect.Height,
				rect.Location.X + rect.Width,
				rect.Location.Y + rect.Height
			};
			
			GL.BindTexture(All.Texture2D, Name);
			GL.VertexPointer(2, All.Float, 0, vertices);
			GL.TexCoordPointer(2, All.Float, 0, coordinates);
			GL.DrawArrays(All.TriangleStrip, 0, 4);
		}
	}
}