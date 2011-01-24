using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osum.Graphics.Sprites
{
    internal class pSpriteCollection
    {
        internal List<pDrawable> SpriteCollection;

        internal pSpriteCollection()
        {
            this.SpriteCollection = new List<pDrawable>();
        }

        internal pSpriteCollection(IEnumerable<pDrawable> sprites)
        {
            this.SpriteCollection = new List<pDrawable>(sprites);
        }
    }
}
