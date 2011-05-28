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
        float Volume
        {
            get;
			set;
        }
		
		/// <summary>
        /// Gets the current power of the music.
        /// </summary>
        /// <value>The current power.</value>
        float CurrentPower
        {
            get;
        }

        /// <summary>
        /// Loads an audio track.
        /// </summary>
        bool Load(byte[] bytes, bool looping);

        /// <summary>
        /// Unloads the current audio track.
        /// </summary>
        bool Unload();
		
        /// <summary>
        /// Plays the loaded audio.
        /// </summary>
        /// <returns></returns>
        bool Play();

        /// <summary>
        /// Stops the playing audio.
        /// </summary>
        bool Stop(bool reset = true);

        /// <summary>
        /// Pause the playing audio.
        /// </summary>
        bool Pause();

        /// <summary>
        /// Seek to specified location.
        /// </summary>
        bool SeekTo(int milliseconds);
	}
}

