using System;
using osum.Support;

namespace osum.Audio
{
    /// <summary>
    /// Play short-lived sound effects, and handle caching.
    /// </summary>
    public abstract class SoundEffectPlayer : IUpdateable
    {
        internal const int MAX_SOURCES = 32; //hardware limitation

        protected Source[] sourceInfo;

        private void CheckUnload()
        {
            foreach (Source s in sourceInfo)
                if (s.Disposable && !s.Playing)
                    s.DeleteBuffer();
            GameBase.Scheduler.Add(CheckUnload, 1000);
        }

        /// <summary>
        /// Loads the specified sound file.
        /// </summary>
        /// <param name="filename">Filename of a 44khz 16-bit wav sample.</param>
        /// <returns>-1 on error, bufferId on success.</returns>
        public abstract int Load(string filename);

        /// <summary>
        /// Unloads all samples and clears cache.
        /// </summary>
        public void UnloadAll()
        {
            foreach (Source s in sourceInfo)
                s.DeleteBuffer();
        }

        internal float Volume = 1;

        /// <summary>
        /// Plays the sample in provided buffer on a new source.
        /// </summary>
        public Source LoadBuffer(int buffer, float volume, bool loop = false, bool reserve = false)
        {
            int freeSource = -1;

            for (int i = 0; i < MAX_SOURCES; i++)
            {
                Source n = sourceInfo[i];

                if (n.Reserved || n.Playing)
                    continue;

                if (n.BufferId == buffer)
                {
                    //can reuse without a rebind.
                    freeSource = i;
                    break;
                }

                if (freeSource == -1)
                    freeSource = i;
            }

            if (freeSource == -1)
                return null; //no free sources

            Source info = sourceInfo[freeSource];

            info.Reserved = reserve;
            info.BufferId = buffer;
            info.Volume = volume * Volume;
            info.Looping = loop;
            info.Pitch = 1;

            return info;
        }

        /// <summary>
        /// Updates this instance. Called every frame when loaded as a component.
        /// </summary>
        public void Update()
        {
        }

        internal void StopAllLooping(bool unreserve = false)
        {
            foreach (Source s in sourceInfo)
            {
                if (s.Playing && s.Looping)
                {
                    s.Stop();
                    s.Reserved = false;
                }
            }
        }
    }

    public abstract class Source
    {
        public int TagNumeric;

        protected int sourceId;
        public int SourceId
        {
            get => sourceId;
            set
            {
                sourceId = value;
                Disposable = false;
                BufferId = 0;
                pitch = 1;
            }
        }

        private float pitch = 1;
        public virtual float Pitch
        {
            get => pitch;
            set
            {
                value = Math.Max(0.5f, Math.Min(2f, value));

                if (pitch == value)
                    return;

                pitch = value;
            }
        }

        public bool Reserved;

        protected int bufferId;
        public virtual int BufferId
        {
            get => bufferId;
            set
            {
                if (Disposable) DeleteBuffer();
                bufferId = value;
            }
        }

        public Source(int source)
        {
            sourceId = source;
        }

        private float volume = 1;
        public virtual float Volume
        {
            get => volume;
            set
            {
                if (value == volume) return;
                volume = value;
            }
        }

        private bool looping;
        public bool Disposable;
        public virtual bool Looping
        {
            get => looping;
            set
            {
                if (looping == value) return;
                looping = value;
            }
        }

        public abstract bool Playing { get; }

        internal abstract void Play();
        
        internal abstract void Stop();
        
        internal virtual void DeleteBuffer()
        {
            bufferId = 0;
            Disposable = false;
        }
    }
}

