using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osum.Graphics.Sprites
{
    internal class SpriteManager : IDisposable
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
            //todo: make this more efficient. .Contains() is slow with a lot of items in the list.
            if (!sprites.Contains(sprite))
                sprites.Add(sprite);
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
            
            for (int i = 0; i < sprites.Count; i++)
                sprites[i].Draw();
            
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
