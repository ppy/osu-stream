using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osum.Graphics.Sprites
{
    internal class SpriteManager : IDisposable
    {
        private List<pSprite> sprites;

        internal SpriteManager()
        {
            this.sprites = new List<pSprite>();
        }

        internal SpriteManager(IEnumerable<pSprite> sprites)
        {
            this.sprites = new List<pSprite>(sprites);
        }

        pSpriteDepthComparer depth = new pSpriteDepthComparer();
        public static float UniversalDim;

        internal void Add(pSprite sprite)
        {
            //todo: make this more efficient. .Contains() is slow with a lot of items in the list.
            //if (!sprites.Contains(sprite))

            int pos = sprites.BinarySearch(sprite, depth);

            if (pos < 0) pos = ~pos;

            sprites.Insert(pos, sprite);
        }

        internal void Add(pSpriteCollection collection)
        {
            foreach (pSprite p in collection.SpriteCollection)
                Add(p); //todo: can optimise this when they are already sorted in depth order.
        }

        /// <summary>
        ///   Update all sprites managed by this sprite manager.
        /// </summary>
        internal void Update()
        {
            for (int i = 0; i < sprites.Count; i++)
                sprites[i].Update();
        }

        /// <summary>
        ///   Draw all sprites managed by this sprite manager.
        /// </summary>
        internal void Draw()
        {
            TextureGl.EnableTexture();
            
            foreach(pSprite p in sprites)
                //todo: consider case updates need to happen even when not visible (ie. animations)
                if (p.Alpha > 0) p.Draw();
            
            TextureGl.DisableTexture();
        }

        /// <summary>
        ///   Used by spinners.  Has a range of 0-0.2
        /// </summary>
        /// <param name = "number"></param>
        /// <returns></returns>
        static internal float drawOrderFwdLowPrio(float number)
        {
            return (number % 200000) / 1000000;
        }

        /// <summary>
        ///   Used by hit values.  Has a range of 0.8-1 and loops every 10000 seconds (over 1 hour).
        /// </summary>
        /// <param name = "number"></param>
        /// <returns></returns>
        static internal float drawOrderFwdPrio(float number)
        {
            return 0.8f + (number % 6000000) / 30000000;
        }

        /// <summary>
        ///   Used by hitcircles.  Has a range of 0.8-0.2 and loops every 6000 seconds (1 hour).
        /// </summary>
        /// <param name = "number"></param>
        /// <returns></returns>
        static internal float drawOrderBwd(float number)
        {
            return 0.8f - (number % 6000000) / 10000000;
        }

        public void Dispose()
        {
            //todo: do we want to dispose of sprites being managed by this manager? possibly.
            sprites = null;
        }
    }
}
