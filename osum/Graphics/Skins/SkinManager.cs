using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace osum.Graphics.Skins
{
    internal static class SkinManager
    {
        internal static Dictionary<string, pTexture> textures = new Dictionary<string,pTexture>();

        internal static pTexture LoadFromFile(string path)
        {
            pTexture texture;
            string name = Path.GetFileNameWithoutExtension(path);

            if (!textures.ContainsKey(name))
            {
                texture = pTexture.FromFile(path);
                textures.Add(name, texture);
            }
            else
            {
                textures.TryGetValue(name, out texture);
            }

            return texture;
        }

        internal static pTexture Load(string name)
        {
            pTexture texture;
            textures.TryGetValue(name, out texture);
            return texture;
        }
    }
}
