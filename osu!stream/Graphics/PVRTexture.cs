using System;
using System.Collections.Generic;
using System.IO;
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
using TextureEnvTarget = OpenTK.Graphics.ES11.All;
using System.Runtime.InteropServices;
#else
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using osum.Input;
#endif

namespace osum.Graphics
{

	public class PVRTexture
	{
		private static readonly char[] PVRTexIdentifier = "PVR!".ToCharArray();
		private const int PvrTextureFlagTypeMask = 0xff;
		private const int PVRTextureFlagTypePVRTC_2 = 24;
		private const int PVRTextureFlagTypePVRTC_4 = 25;

		[StructLayout(LayoutKind.Explicit)]
		private struct PVRTexHeader
		{
			[FieldOffset(0)]
			public uint headerLength;
			[FieldOffset(4)]
			public uint height;
			[FieldOffset(8)]
			public uint width;
			[FieldOffset(12)]
			public uint numMipmaps;
			[FieldOffset(16)]
			public uint flags;
			[FieldOffset(20)]
			public uint dataLength;
			[FieldOffset(24)]
			public uint bpp;
			[FieldOffset(28)]
			public uint bitmaskRed;
			[FieldOffset(32)]
			public uint bitmaskGreen;
			[FieldOffset(36)]
			public uint bitmaskBlue;
			[FieldOffset(40)]
			public uint bitmaskAlpha;
			[FieldOffset(44)]
			public uint pvrTag;
			[FieldOffset(48)]
			public uint numSurfs;
		}

		private List<byte[]> _imageData;

		public uint Name { get; private set; }
		public uint Width { get; private set; }
		public uint Height { get; private set; }
		public All InternalFormat { get; private set; }
		public bool HasAlpha { get; private set; }
		public bool RetainName { get; set; }

		private void CreateGLTexture()
		{
			if (_imageData.Count > 0) {
				uint name = Name;
				
				if (Name != 0) {
					GL.DeleteTextures(1, ref name);
					
				}
				
				GL.GenTextures(1, ref name);
				Name = name;
				GL.BindTexture(All.Texture2D, Name);
			}
			
			uint currentWidth = Width;
			uint currentHeight = Height;
			
			for (int i = 0; i < _imageData.Count; ++i) {
				byte[] data = _imageData[i];
				
				GL.CompressedTexImage2D(All.Texture2D, i, InternalFormat, (int)currentWidth, (int)currentHeight, 0, data.Length, data);
				
				All error = GL.GetError();
				
				if (error != All.NoError) {
					throw new InvalidOperationException(string.Format("Error uploading compressed texture level: {0}, glError: {1}", i, error.ToString()));
				}
				
				currentWidth = Math.Max(currentWidth >> 1, 1);
				currentHeight = Math.Max(currentHeight >> 1, 1);
			}
			
			_imageData.Clear();
			
		}

		private void UnpackPVRData(byte[] data)
		{
			unsafe {
				fixed (byte* dataP = data) {
					PVRTexHeader* header = (PVRTexHeader*)dataP;
					// TODO original code calls CFSwapInt32LittleToHost. which seems unnecessary
					uint pvrTag = header->pvrTag;
					
					if ((uint)PVRTexIdentifier[0] != ((pvrTag >> 0) & 0xff) || (uint)PVRTexIdentifier[1] != ((pvrTag >> 8) & 0xff) || (uint)PVRTexIdentifier[2] != ((pvrTag >> 16) & 0xff) || (int)PVRTexIdentifier[3] != ((pvrTag >> 24) & 0xff)) {
						throw new InvalidOperationException("Provided data is not in PVR format (header lacks 'PVR!' tag)");
					}
					
					uint flags = header->flags;
					uint formatFlags = flags & PvrTextureFlagTypeMask;
					uint dataLength = 0;
					uint dataOffset = 0;
					uint dataSize = 0;
					uint blockSize = 0;
					uint heightBlocks = 0;
					uint widthBlocks = 0;
					uint bpp = 4;
					
					if (formatFlags == PVRTextureFlagTypePVRTC_4 || formatFlags == PVRTextureFlagTypePVRTC_2) {
						_imageData.Clear();
						
						if (formatFlags == PVRTextureFlagTypePVRTC_4) {
							InternalFormat = All.CompressedRgbaPvrtc4Bppv1Img;
						} else if (formatFlags == PVRTextureFlagTypePVRTC_2) {
							InternalFormat = All.CompressedRgbaPvrtc2Bppv1Img;
						}
						
						Width = header->width;
						Height = header->height;
						
						HasAlpha = header->bitmaskAlpha != 0;
						
						dataLength = header->dataLength;
						byte* imageDataP = dataP + sizeof(PVRTexHeader);
						
						
						uint currentWidth = Width;
						uint currentHeight = Height;
						
						while (dataOffset < dataLength) {
							if (formatFlags == PVRTextureFlagTypePVRTC_4) {
								blockSize = 4 * 4;
								widthBlocks = currentWidth / 4;
								heightBlocks = currentHeight / 4;
								bpp = 4;
							} else {
								blockSize = 8 * 4;
								widthBlocks = currentWidth / 8;
								heightBlocks = currentHeight / 4;
								bpp = 2;
							}
							
							if (widthBlocks < 2) {
								widthBlocks = 2;
							}
							if (heightBlocks < 2) {
								heightBlocks = 2;
							}
							
							dataSize = widthBlocks * heightBlocks * ((blockSize * bpp) / 8);
							
							byte[] b = new byte[dataSize];
							Marshal.Copy((IntPtr)((int)imageDataP + dataOffset), b, 0, (int)dataSize);
							
							_imageData.Add(b);
							dataOffset += dataSize;
							
							currentWidth = Math.Max(currentWidth >> 1, 1);
							currentHeight = Math.Max(currentHeight >> 1, 1);
						}
					}
				}
			}
		}

		public PVRTexture(string filename)
		{
			if (!File.Exists(filename)) {
				throw new FileNotFoundException(filename);
			}
			
			byte[] data = File.ReadAllBytes(filename);
			
			_imageData = new List<byte[]>(10);
			
			Name = 0;
			Width = Height = 0;
			InternalFormat = All.CompressedRgbaPvrtc4Bppv1Img;
			HasAlpha = false;
			RetainName = false;
			
			UnpackPVRData(data);
			CreateGLTexture();
		}
	}
}

