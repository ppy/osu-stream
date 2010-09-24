using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace osum.Graphics.Skins
{
    internal static partial class TextureManager
    {
		
		public static void UnloadAll()
		{
			foreach (pTexture p in SpriteCache.Values)
				p.Dispose();
			
			SpriteCache.Clear();
			AnimationCache.Clear();
		}

    	internal static Dictionary<string, pTexture> SpriteCache = new Dictionary<string, pTexture>();
        internal static Dictionary<string, pTexture[]> AnimationCache = new Dictionary<string, pTexture[]>();

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

        internal static pTexture[] LoadAll(string name)
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
}
