using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Sprites;

namespace osum.GameModes
{
    internal abstract class GameMode : ISpriteable, IDisposable
    {
        internal abstract void Initialize();

        internal SpriteManager spriteManager;

        internal GameMode()
        {
            spriteManager = new SpriteManager();
        }

        ~GameMode()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            spriteManager.Dispose();
        }


        public virtual void Update()
        {
            spriteManager.Update();
        }

        public virtual void Draw()
        {
            spriteManager.Draw();
        }
    }
}
