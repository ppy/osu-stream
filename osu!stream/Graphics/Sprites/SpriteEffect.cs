using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Text;

namespace osum.Graphics.Sprites
{
    [Flags]
    public enum SpriteEffect
    {
        FlipHorizontally = 1,
        FlipVertically = 0x100,
        None = 0
    }
}
