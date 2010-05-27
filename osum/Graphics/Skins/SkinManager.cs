using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osum.Graphics.Skins
{
    public static class SkinManager
    {
        public static Dictionary<string, pTexture> textures = new Dictionary<string,pTexture>();

        public static pTexture LoadTexture(string path)
        {
            pTexture texture;

            if (!textures.ContainsKey(path))
                texture = pTexture.FromFile(path);
            else
                textures.TryGetValue(path, out texture);

            return texture;
        }
    }
}
