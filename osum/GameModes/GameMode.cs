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
		SongSelect,
        Play,
		Ranking
	}

    /// <summary>
    /// A specific scene/screen that is to be displayed in the game.
    /// </summary>
	public abstract class GameMode : IDrawable, IDisposable
    {
        /// <summary>
        /// Do all initialization here.
        /// </summary>
        internal abstract void Initialize();
		
        /// <summary>
        /// A spriteManager provided free of charge.
        /// </summary>
        internal SpriteManager spriteManager;

        internal GameMode()
        {
            spriteManager = new SpriteManager();
        }

        /// <summary>
        /// Clean-up this instance.
        /// </summary>
        public virtual void Dispose()
        {
            spriteManager.Dispose();
            //GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Updates this instance. Called every frame when loaded as a component.
        /// </summary>
        public virtual void Update()
        {
            spriteManager.Update();
        }

        /// <summary>
        /// Draws this object to screen.
        /// </summary>
        public virtual bool Draw()
        {
            spriteManager.Draw();
            return true;
        }

        public virtual void OnFirstUpdate()
        {
            
        }
    }
}
