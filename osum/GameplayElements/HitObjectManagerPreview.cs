using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.GameplayElements.Beatmaps;
using osum.GameModes;

namespace osum.GameplayElements
{
    class HitObjectManagerPreview : HitObjectManager
    {
        public HitObjectManagerPreview(Beatmap beatmap)
            : base(beatmap)
        {
        }

        internal override void PostProcessing()
        {
            //remove every second bookmark
            List<int> newStreamSwitchPoints = new List<int>();

            for (int i = 0; i < beatmap.StreamSwitchPoints.Count; i++)
                if (i % 2 == 1)
                    newStreamSwitchPoints.Add(beatmap.StreamSwitchPoints[i]);
            beatmap.StreamSwitchPoints = newStreamSwitchPoints;

            base.PostProcessing();
        }

        protected override bool shouldLoadDifficulty(Difficulty difficulty)
        {
            //adjust the player difficulty here t
            Player.Difficulty = difficulty == Difficulty.Expert ? Difficulty.Expert : Difficulty.Normal;

            return true;
        }

        public override bool IsLowestStream { get { return ActiveStream == Difficulty.Easy; } }
        public override bool IsHighestStream { get { return ActiveStream == Difficulty.Expert; } }


    }
}
