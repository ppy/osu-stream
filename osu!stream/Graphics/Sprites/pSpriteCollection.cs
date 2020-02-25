using System.Collections.Generic;
using OpenTK;

namespace osum.Graphics.Sprites
{
    public class pSpriteCollection
    {
        internal List<pDrawable> Sprites;

        internal pSpriteCollection()
        {
            Sprites = new List<pDrawable>();
        }

        internal pSpriteCollection(IEnumerable<pDrawable> sprites)
        {
            Sprites = new List<pDrawable>(sprites);
        }

        private bool visible = true;

        internal bool Visible
        {
            get => visible;
            set
            {
                if (value == visible) return;
                visible = value;
                Sprites.ForEach(s => s.Alpha = visible ? 1 : 0);
            }
        }

        internal void Add(pDrawable p)
        {
            Sprites.Add(p);
        }

        internal void Add(List<pDrawable> p)
        {
            Sprites.AddRange(p);
        }

        internal void Add(pSpriteCollection sc)
        {
            Sprites.AddRange(sc.Sprites);
        }

        internal void MoveTo(Vector2 location, int duration)
        {
            //todo: this is horribly inefficient.
            Sprites.ForEach(s => s.MoveTo(location, duration, EasingTypes.In));
        }
    }
}