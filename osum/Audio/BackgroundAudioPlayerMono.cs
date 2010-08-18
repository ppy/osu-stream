using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osum.Audio
{
    //todo: implement
    class BackgroundAudioPlayerMono : IBackgroundAudioPlayer
    {
        public BackgroundAudioPlayerMono()
        {
        }

        public float CurrentVolume
        {
            get { return 0; }
        }

        public bool Play()
        {
            return false;
        }

        public void Update()
        {
            
        }
    }
}
