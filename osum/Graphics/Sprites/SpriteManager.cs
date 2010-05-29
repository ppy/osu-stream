using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace osum.Graphics.Sprites
{
    internal class SpriteManager
    {
        private List<ISpriteable> sprites;

        internal SpriteManager()
        {
            this.sprites = new List<ISpriteable>();
        }

        internal SpriteManager(IEnumerable<ISpriteable> sprites)
        {
            this.sprites = new List<ISpriteable>(sprites);
        }

        internal void Add(ISpriteable sprite)
        {
            if (!sprites.Contains(sprite))
                sprites.Add(sprite);
        }

        internal void Update()
        {
            for (int i = 0; i < sprites.Count; i++)
            {
                sprites[i].Update();
            }
        }

        internal void Draw()
        {
            TextureGl.EnableTexture();

            for (int i = 0; i < sprites.Count; i++)
            {
                sprites[i].Draw();
            }

            TextureGl.DisableTexture();
        }
    }
}
