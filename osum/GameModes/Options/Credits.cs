using System;
using osum.GameplayElements;

namespace osum.GameModes.Options
{
    public class Credits : Player
    {
        public override void Initialize()
        {
            Difficulty = Difficulty.None;
            Beatmap = null;

            base.Initialize();
        }
    }
}

