using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osum.Graphics.Sprites
{
    public class pSpriteCollection : ISpriteable
    {
        private List<pSprite> sprites;

        public pSpriteCollection()
        {
            this.sprites = new List<pSprite>();
        }

        public pSpriteCollection(IEnumerable<pSprite> sprites)
        {
            this.sprites = new List<pSprite>(sprites);
        }

        public void Add(pSprite sprite)
        {
            if (!sprites.Contains(sprite))
                sprites.Add(sprite);
        }

        public void Add(Transform transform)
        {
            for (int i = 0; i < sprites.Count; i++)
            {
                sprites[i].Add(transform);
            }
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
            for (int i = 0; i < sprites.Count; i++)
            {
                sprites[i].Draw();
            }
        }
    }
}
