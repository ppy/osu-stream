using System;
using System.Runtime.InteropServices;
using osum.Helpers;
using ManagedBass;
using ManagedBass.Aac;
using osum.Audio.BassNetUtils;

namespace osum.Audio
{
    internal class BackgroundAudioPlayerAndroid : BackgroundAudioPlayer
    {
        private GCHandle audioHandle;
        private static int audioStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundAudioPlayerAndroid"/> class.
        /// </summary>
        public BackgroundAudioPlayerAndroid()
        {
            // BassNet.Registration("poo@poo.com", "2X25242411252422");

            Bass.Init(-1, 44100, DeviceInitFlags.Default, IntPtr.Zero);

            //Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, 100);
            //Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATEPERIOD, 10);
        }

        /// <summary>
        /// Plays the loaded audio.
        /// </summary>
        /// <returns></returns>
        public override bool Play()
        {
            Bass.ChannelPlay(audioStream);
            return true;
        }

        /// <summary>
        /// Stops the playing audio.
        /// </summary>
        /// <returns></returns>
        public override bool Stop(bool reset = true)
        {
            Bass.ChannelStop(audioStream);
            if (reset) SeekTo(0);
            return true;
        }

        /// <summary>
        /// Updates this instance. Called every frame when loaded as a component.
        /// </summary>
        public override void Update()
        {
        }

        internal void FreeMusic()
        {
            if (audioStream != 0)
            {
                if (audioHandle.IsAllocated)
                    audioHandle.Free();

                Bass.ChannelStop(audioStream);
                Bass.StreamFree(audioStream);
                audioStream = 0;
            }
        }

        public override bool Load(byte[] audio, bool looping, string identifier = "apain")
        {
            if (!base.Load(audio, looping, identifier))
                return false;

            FreeMusic();

            audioHandle = GCHandle.Alloc(audio, GCHandleType.Pinned);
            
            audioStream = Bass.CreateStream(audioHandle.AddrOfPinnedObject(), 0, audio.Length, BassFlags.Prescan | (looping ? BassFlags.Loop : 0));

            // If loading fails, attempts to load as an AAC based file
            if(Bass.LastError != Errors.OK)
                audioStream = BassAac.CreateMp4Stream(audioHandle.AddrOfPinnedObject(), 0, audio.Length, BassFlags.Prescan | (looping ? BassFlags.Loop : 0));

            updateVolume();

            return true;
        }

        public override bool Unload()
        {
            FreeMusic();
            return true;
        }

        public override double CurrentTime
        {
            get
            {
                if (audioStream == 0) return 0;

                long audioTimeRaw = Bass.ChannelGetPosition(audioStream);
                return Bass.ChannelBytes2Seconds(audioStream, audioTimeRaw);
            }
        }

        public override bool Pause()
        {
            Bass.ChannelPause(audioStream);
            return true;
        }

        public override bool SeekTo(int milliseconds)
        {
            if (audioStream == 0) return false;

            long byteTime = Bass.ChannelSeconds2Bytes(audioStream, (double)milliseconds / 1000d);
            Bass.ChannelSetPosition(audioStream, byteTime);
            return base.SeekTo(milliseconds);
        }

        #region IBackgroundAudioPlayer Members

        public override float CurrentPower
        {
            get
            {
                int word = Bass.ChannelGetLevel(audioStream);
                int left = Utils.LowWord32(word);
                int right = Utils.HighWord32(word);

                return (left + right) / 65536f * 2f;
            }
        }

        #endregion

        #region ITimeSource Members

        public override bool IsElapsing => Bass.ChannelIsActive(audioStream) == PlaybackState.Playing;

        #endregion

        protected override void updateVolume()
        {
            if (audioStream == 0) return;
            Bass.ChannelSetAttribute(audioStream, ChannelAttribute.Volume, pMathHelper.ClampToOne(DimmableVolume * MaxVolume));
        }
    }
}