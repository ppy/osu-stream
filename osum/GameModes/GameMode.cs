using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Sprites;

namespace osum.GameModes
{
    public enum OsuMode
	{
		Unknown = 0,
		MainMenu,
		SongSelect
	}
	
	public abstract class GameMode : ISpriteable, IDisposable
    {
        internal abstract void Initialize();

        internal SpriteManager spriteManager;

        internal GameMode()
        {
            spriteManager = new SpriteManager();
        }

        public virtual void Dispose()
        {
            //GC.SuppressFinalize(this);
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
