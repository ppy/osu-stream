using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osum.Graphics
{
    [Flags]
    public enum SpriteEffects
    {
        FlipHorizontally = 1,
        FlipVertically = 0x100,
        None = 0
    }
}
