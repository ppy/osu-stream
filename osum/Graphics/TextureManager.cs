using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using osum.Support;
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


namespace osum.Graphics.Skins
{
    internal class SpriteSheetTexture
    {
        internal string SheetName;
        internal int X;
        internal int Y;
        internal int Width;
        internal int Height;

        public SpriteSheetTexture(string name, int x, int y, int width, int height)
        {
            this.SheetName = name;
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }
    }

    /// <summary>
    /// Handle the loading of textures from various sources.
    /// Caching, reuse, unloading and everything else.
    /// </summary>
    internal static partial class TextureManager
    {
        static Dictionary<OsuTexture, SpriteSheetTexture> textureLocations = new Dictionary<OsuTexture, SpriteSheetTexture>();

        public static void Initialize()
        {

            textureLocations.Add(OsuTexture.hit0, new SpriteSheetTexture("hit", 638, 279, 140, 134));

            textureLocations.Add(OsuTexture.hit50, new SpriteSheetTexture("hit", 369, 0, 133, 132));

            textureLocations.Add(OsuTexture.hit100, new SpriteSheetTexture("hit", 189, 0, 180, 180));
            textureLocations.Add(OsuTexture.hit100k, new SpriteSheetTexture("hit", 190, 180, 180, 180));

            textureLocations.Add(OsuTexture.hit300, new SpriteSheetTexture("hit", 0, 0, 189, 190));
            textureLocations.Add(OsuTexture.hit300k, new SpriteSheetTexture("hit", 0, 190, 190, 190));
            textureLocations.Add(OsuTexture.hit300g, new SpriteSheetTexture("hit", 626, 0, 208, 207));

            textureLocations.Add(OsuTexture.sliderfollowcircle, new SpriteSheetTexture("hit", 370, 132, 256, 257));
            textureLocations.Add(OsuTexture.sliderscorepoint, new SpriteSheetTexture("hit", 190, 366, 20, 18));

            textureLocations.Add(OsuTexture.hitcircle, new SpriteSheetTexture("hit", 834, 0, 108, 108));
            textureLocations.Add(OsuTexture.hitcircleoverlay, new SpriteSheetTexture("hit", 834, 109, 128, 128));

            textureLocations.Add(OsuTexture.approachcircle, new SpriteSheetTexture("hit", 0, 380, 126, 128));

            textureLocations.Add(OsuTexture.sliderarrow, new SpriteSheetTexture("hit", 626, 206, 77, 58));

            textureLocations.Add(OsuTexture.holdcircle, new SpriteSheetTexture("hit", 834, 238, 157, 158));
            textureLocations.Add(OsuTexture.followpoint, new SpriteSheetTexture("hit", 195, 387, 11, 11));

            textureLocations.Add(OsuTexture.default_0, new SpriteSheetTexture("hit", 131, 456, 65, 47));
            textureLocations.Add(OsuTexture.default_1, new SpriteSheetTexture("hit", 201, 456, 13, 47));
            textureLocations.Add(OsuTexture.default_2, new SpriteSheetTexture("hit", 219, 456, 65, 47));
            textureLocations.Add(OsuTexture.default_3, new SpriteSheetTexture("hit", 289, 456, 65, 47));
            textureLocations.Add(OsuTexture.default_4, new SpriteSheetTexture("hit", 359, 456, 65, 47));
            textureLocations.Add(OsuTexture.default_5, new SpriteSheetTexture("hit", 429, 456, 65, 47));
            textureLocations.Add(OsuTexture.default_6, new SpriteSheetTexture("hit", 499, 456, 65, 47));
            textureLocations.Add(OsuTexture.default_7, new SpriteSheetTexture("hit", 569, 456, 65, 47));
            textureLocations.Add(OsuTexture.default_8, new SpriteSheetTexture("hit", 639, 456, 65, 47));
            textureLocations.Add(OsuTexture.default_9, new SpriteSheetTexture("hit", 709, 456, 65, 47));
        }

        public static void Update()
        {
#if DEBUG
            DebugOverlay.AddLine("TextureManager: " + SpriteCache.Count + " cached " + DisposableTextures.Count + " dynamic");
#endif
        }


		public static void DisposeAll()
		{
			UnloadAll();
			
			SpriteCache.Clear();
            
			AnimationCache.Clear();
		}
		
		public static void UnloadAll()
		{
			foreach (pTexture p in SpriteCache.Values)
				p.UnloadTexture();
			foreach (pTexture p in DisposableTextures)
				p.Dispose();

			DisposableTextures.Clear();

            availableSurfaces = null;
		}
		
		public static void ReloadAll()
		{
			foreach (pTexture p in SpriteCache.Values)
				p.ReloadIfPossible();
		}
		
		public static void RegisterDisposable(pTexture t)
		{
            DisposableTextures.Add(t);
		}
		
    	internal static Dictionary<string, pTexture> SpriteCache = new Dictionary<string, pTexture>();
        internal static Dictionary<string, pTexture[]> AnimationCache = new Dictionary<string, pTexture[]>();
		internal static List<pTexture> DisposableTextures = new List<pTexture>();

        internal static pTexture Load(OsuTexture texture)
        {
            SpriteSheetTexture info;

            if (textureLocations.TryGetValue(texture, out info))
            {
                pTexture tex = Load(info.SheetName);
                tex = tex.Clone(); //make a new instance because we may be using different coords.
                tex.X = info.X;
                tex.Y = info.Y;
                tex.Width = info.Width;
                tex.Height = info.Height;
                tex.OsuTextureInfo = texture;

                return tex;
            }
            else
            {
                //fallback to separate files
                return Load(texture.ToString());
            }
        }
        
        internal static pTexture Load(string name)
        {
            pTexture texture;

            if (SpriteCache.TryGetValue(name, out texture))
                return texture;

            string path = name.IndexOf('.') < 0 ? string.Format(@"Skins/Default/{0}.png", name) : @"Skins/Default/" + name;
	
			if (File.Exists(path))
            {
				texture = pTexture.FromFile(path);
                SpriteCache.Add(name, texture);
                return texture;
            }
			
            return null;
        }

        internal static pTexture[] LoadAnimation(string name)
        {
            pTexture[] textures;
            pTexture texture;

            if (AnimationCache.TryGetValue(name, out textures))
                return textures;

            texture = Load(name + "-0");

            // if the texture is found, load all subsequent textures
            if (texture != null)
            {
                List<pTexture> list = new List<pTexture>();
                list.Add(texture);

                for (int i = 1; true; i++)
                {
                    texture = Load(name + "-" + i);

                    if (texture == null)
                    {
                        textures = list.ToArray();
                        AnimationCache.Add(name, textures);
                        return textures;
                    }

                    list.Add(texture);
                }
            }
            
            // if the texture can't be found, try without number
            texture = Load(name);

            if (texture != null)
            {
                textures = new[] { texture };
                AnimationCache.Add(name, textures);
                return textures;
            }

            return null;
        }

        static Queue<pTexture> availableSurfaces;
		
		internal static void PopulateSurfaces()
		{
			if (availableSurfaces == null)
            {
                availableSurfaces = new Queue<pTexture>();
				
				int size = GameBase.NativeSize.Width;

                for (int i = 0; i < 4; i++)
                {
                    TextureGl gl = new TextureGl(size, size);
                    gl.SetData(IntPtr.Zero, 0, PixelFormat.Rgba);
                    pTexture t = new pTexture(gl, size, size);
					t.BindFramebuffer();
					
                    RegisterDisposable(t);
                    availableSurfaces.Enqueue(t);
                }
            }	
		}
	
        
        internal static pTexture RequireTexture(int width, int height)
        {
            PopulateSurfaces();

            pTexture tex = availableSurfaces.Dequeue();
            tex.Width = width;
            tex.Height = height;

            return tex;
        }

        internal static void ReturnTexture(pTexture texture)
        {
            availableSurfaces.Enqueue(texture);
        }
    }

    internal enum TextureSource
    {
        Game,
        Skin,
        Beatmap
    }

    internal enum OsuTexture
    {
        None = 0,
        hit50,
        hit100,
        hit300,
        sliderfollowcircle,
        hit100k,
        hit300g,
        hit300k,
        hit0,
        sliderscorepoint,
        hitcircle,
        hitcircleoverlay,
        approachcircle,
        sliderarrow,
        holdcircle,
        followpoint,
        connectionline,
        default_0,
        default_1,
        default_2,
        default_3,
        default_4,
        default_5,
        default_6,
        default_7,
        default_8,
        default_9

    }
}
