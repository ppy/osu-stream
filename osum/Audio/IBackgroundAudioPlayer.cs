using System;
using osum.Support;
namespace osum
{
    /// <summary>
    /// Interface for a class which plays music. Provides access to specific information during playback such as time, levels etc.
    /// </summary>
    public interface IBackgroundAudioPlayer : IUpdateable, ITimeSource
    {
        /// <summary>
        /// Gets the current volume.
        /// </summary>
        /// <value>The current volume.</value>
        float CurrentVolume
        {
            get;
        }

        /// <summary>
        /// Loads an audio track.
        /// </summary>
        bool Load(byte[] bytes);

        /// <summary>
        /// Plays the loaded audio.
        /// </summary>
        /// <returns></returns>
        bool Play();

        /// <summary>
        /// Stops the playing audio.
        /// </summary>
        bool Stop();

        /// <summary>
        /// Pause the playing audio.
        /// </summary>
        bool Pause();
	}
}

