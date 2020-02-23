using System.Collections.Generic;
using osum.GameplayElements.Beatmaps;

namespace BeatmapCombinator
{
    public class BeatmapDifficulty : Beatmap
    {
        internal string VersionName;
        internal List<HitObjectLine> HitObjectLines = new List<HitObjectLine>();
        internal List<string> HeaderLines = new List<string>();

        internal double VelocityAt(int time)
        {
            return (100000.0f * DifficultySliderMultiplier / beatLengthAt(time, true));
        }

        internal double ScoringDistanceAt(int time)
        {
            return ((100 * DifficultySliderMultiplier / bpmMultiplierAt(time)) / DifficultySliderTickRate);
        }
    }
}