using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Audio;

namespace osum.GameModes.Play
{
    public class PlayTest : Player
    {
        public static int StartTime;
        public static bool AllowStreamSwitch = true;

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
        }

        protected override void UpdateStream()
        {
            if (!AllowStreamSwitch) return;
            base.UpdateStream();
        }
    }
}
