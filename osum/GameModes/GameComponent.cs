using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osum.GameModes
{
    /// <summary>
    /// An automatically initializing game mode.
    /// </summary>
    class GameComponent : GameMode
    {
        public GameComponent()
            : base()
        {
            Initialize();
        }

        internal override void Initialize()
        {

        }
    }
}
