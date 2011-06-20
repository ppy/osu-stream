using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osum.GameplayElements.Beatmaps
{
    public class BeatmapDifficultyInfo
    {
        public Difficulty Difficulty;
        public double ComboMultiplier;

        public BeatmapDifficultyInfo(Difficulty difficulty)
        {
            Difficulty = difficulty;
        }
    }
}
