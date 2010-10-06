using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
            textureLocations.Add(OsuTexture.hit300, new SpriteSheetTexture("hit", 0, 0, 256, 256));
            textureLocations.Add(OsuTexture.hit100, new SpriteSheetTexture("hit", 256, 0, 256, 256));
            textureLocations.Add(OsuTexture.hit50, new SpriteSheetTexture("hit", 512, 0, 256, 256));
            textureLocations.Add(OsuTexture.sliderfollowcircle, new SpriteSheetTexture("hit", 0, 256, 256, 256));
        }

		public static void UnloadAll()
		{
			foreach (pTexture p in SpriteCache.Values)
				p.Dispose();
			
			SpriteCache.Clear();
			AnimationCache.Clear();
		}

    	internal static Dictionary<string, pTexture> SpriteCache = new Dictionary<string, pTexture>();
        internal static Dictionary<string, pTexture[]> AnimationCache = new Dictionary<string, pTexture[]>();

        internal static pTexture Load(OsuTexture texture)
        {
            SpriteSheetTexture info;

            if (textureLocations.TryGetValue(texture, out info))
            {
                pTexture tex = Load(info.SheetName);
                tex.X = info.X;
                tex.Y = info.Y;
                tex.Width = info.Width;
                tex.Height = info.Height;

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
        hit0
    }
}
