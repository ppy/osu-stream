using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osum.Graphics.Sprites
{
    internal class pSpriteCollection : ISpriteable
    {
        internal List<pSprite> SpriteCollection;

        internal pSpriteCollection()
        {
            this.SpriteCollection = new List<pSprite>();
        }

        internal pSpriteCollection(IEnumerable<pSprite> sprites)
        {
            this.SpriteCollection = new List<pSprite>(sprites);
        }

        internal void Update()
        {
            for (int i = 0; i < SpriteCollection.Count; i++)
            {
                SpriteCollection[i].Update();
            }
        }

        internal void Draw()
        {
            for (int i = 0; i < SpriteCollection.Count; i++)
            {
                SpriteCollection[i].Draw();
            }
        }
    }
}
