using System.IO;
using osum.Helpers;
using osum.Support;

namespace osum.Audio
{
    /// <summary>
    /// Interface for a class which plays music. Provides access to specific information during playback such as time, levels etc.
    /// </summary>
    public abstract class BackgroundAudioPlayer : IUpdateable, ITimeSource
    {
        /// <summary>
        /// Gets the current volume.
        /// </summary>
        /// <value>The current volume.</value>
        private float dimmableVolume = 1;

        public float DimmableVolume
        {
            get => dimmableVolume;
            set
            {
                float clamped = pMathHelper.ClampToOne(value);
                if (dimmableVolume == clamped) return;
                dimmableVolume = clamped;
                updateVolume();
            }
        }

        private float maxVolume = -1;

        public float MaxVolume
        {
            get => maxVolume;
            set
            {
                float clamped = pMathHelper.ClampToOne(value);
                if (maxVolume == clamped) return;
                maxVolume = clamped;
                updateVolume();
            }
        }

        protected abstract void updateVolume();

        /// <summary>
        /// Gets the current power of the music.
        /// </summary>
        /// <value>The current power.</value>
        public abstract float CurrentPower { get; }

        /// <summary>
        /// Loads an audio track.
        /// </summary>
        public virtual bool Load(byte[] audio, bool looping, string identifier = null)
        {
            if (identifier != null && lastLoaded == identifier) return false;

            lastLoaded = identifier;
            return true;
        }

        public string lastLoaded;

        /// <summary>
        /// Loads an audio track directly from a file.
        /// </summary>
        public bool Load(string filename, bool looping)
        {
            return Load(File.ReadAllBytes("/sdcard/" + filename), looping, filename);
        }

        /// <summary>
        /// Unloads the current audio track.
        /// </summary>
        public abstract bool Unload();

        /// <summary>
        /// Buffers the audio ready for low-latency play() operations.
        /// </summary>
        /// <returns></returns>
        public virtual bool Prepare()
        {
            return true;
        }

        /// <summary>
        /// Plays the loaded audio.
        /// </summary>
        /// <returns></returns>
        public abstract bool Play();

        /// <summary>
        /// Stops the playing audio.
        /// </summary>
        public abstract bool Stop(bool reset = true);

        /// <summary>
        /// Pause the playing audio.
        /// </summary>
        public abstract bool Pause();

        /// <summary>
        /// Seek to specified location.
        /// </summary>
        public virtual bool SeekTo(int milliseconds)
        {
            Clock.SkipOccurred(milliseconds);
            return true;
        }

        #region IUpdateable Members

        public abstract void Update();

        #endregion

        #region ITimeSource Members

        public abstract double CurrentTime { get; }

        public abstract bool IsElapsing { get; }

        #endregion
    }
}