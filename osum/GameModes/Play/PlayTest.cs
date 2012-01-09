using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Audio;
using osum.GameplayElements;
using osu_common.Helpers;

namespace osum.GameModes.Play
{
    public class PlayTest : Player
    {
        public static int StartTime;
        public static bool AllowStreamSwitch = true;
        public static Difficulty InitialDifficulty;
        public static int InitialHp;

        public override void Dispose()
        {
            AudioEngine.Music.Stop();
            Beatmap.Dispose();
            base.Dispose();
        }

        public override void Initialize()
        {
            base.Initialize();
            AudioEngine.Music.SeekTo(StartTime);
            foreach (pList<HitObject> h in HitObjectManager.StreamHitObjects)
            {
                if (h == null) continue;
                h.RemoveAll(ho => ho.StartTime < StartTime);
            }
        }

        protected override void initializeUIElements()
        {
            base.initializeUIElements();
        }

        protected override void InitializeStream()
        {
            HitObjectManager.SetActiveStream(InitialDifficulty);
        }

        protected override void hitObjectManager_OnScoreChanged(ScoreChange change, HitObject hitObject)
        {
            base.hitObjectManager_OnScoreChanged(change, hitObject);

            if (InitialHp == 0)
                healthBar.ReduceCurrentHp(100);
            if (InitialHp == 200)
                healthBar.IncreaseCurrentHp(100);
        }

        protected override void UpdateStream()
        {
            if (!AllowStreamSwitch) return;
            base.UpdateStream();
        }
    }
}
