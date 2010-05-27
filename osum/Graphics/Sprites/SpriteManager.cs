using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace osum.Graphics.Sprites
{
    public class SpriteManager
    {
        private List<ISpriteable> sprites;

        public SpriteManager()
        {
            this.sprites = new List<ISpriteable>();
        }

        public SpriteManager(IEnumerable<ISpriteable> sprites)
        {
            this.sprites = new List<ISpriteable>(sprites);
        }

        public void Add(ISpriteable sprite)
        {
            if (!sprites.Contains(sprite))
                sprites.Add(sprite);
        }

        public void Update()
        {
            for (int i = 0; i < sprites.Count; i++)
            {
                sprites[i].Update();
            }
        }

        public void Draw()
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
