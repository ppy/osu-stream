using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osum.Audio
{
    class BackgroundAudioPlayerMono : IBackgroundAudioPlayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundAudioPlayerMono"/> class.
        /// </summary>
        public BackgroundAudioPlayerMono()
        {
        }

        /// <summary>
        /// Gets the current volume.
        /// </summary>
        /// <value>The current volume.</value>
        public float CurrentVolume
        {
            get { return 0; }
        }

        /// <summary>
        /// Plays the loaded audio.
        /// </summary>
        /// <returns></returns>
        public bool Play()
        {
            return false;
        }

        /// <summary>
        /// Updates this instance. Called every frame when loaded as a component.
        /// </summary>
        public void Update()
        {

        }
    }
}
