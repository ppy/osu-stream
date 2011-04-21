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

        bool visible = true;
        internal bool Visible
        {
            get { return visible; }
            set
            {
                if (value == visible) return;
                visible = value;
                SpriteCollection.ForEach(s => s.Alpha = visible ? 1 : 0);
            }
        }
    }
}
