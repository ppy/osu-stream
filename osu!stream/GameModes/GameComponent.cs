using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Text;

namespace osum.GameModes
{
    /// <summary>
    /// An automatically initializing game mode.
    /// </summary>
    public class GameComponent : GameMode
    {
        public GameComponent()
            : base()
        {
            Initialize();
        }

        public override void Initialize()
        {

        }
    }
}
