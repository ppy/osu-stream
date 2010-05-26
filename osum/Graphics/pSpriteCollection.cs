using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osum.Graphics
{
    public class pSpriteCollection : IDrawable
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

        public void AddSprite(pSprite sprite)
        {
            if (!sprites.Contains(sprite))
                sprites.Add(sprite);
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
