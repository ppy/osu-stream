using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Un4seen.Bass;
using System.IO;
using System.Runtime.InteropServices;
using osum.Helpers;

namespace osum.Audio
{
    class BackgroundAudioPlayerDesktop : BackgroundAudioPlayer
    {
        private GCHandle audioHandle;
        private static int audioStream;
        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundAudioPlayerDesktop"/> class.
        /// </summary>
        public BackgroundAudioPlayerDesktop()
        {
            BassNet.Registration("poo@poo.com", "2X25242411252422");

            Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, (IntPtr)0, null);

            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_BUFFER, 100);
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATEPERIOD, 10);
        }

        /// <summary>
        /// Plays the loaded audio.
        /// </summary>
        /// <returns></returns>
        public override bool Play()
        {
            Bass.BASS_ChannelPlay(audioStream, false);
            return true;
        }

        /// <summary>
        /// Stops the playing audio.
        /// </summary>
        /// <returns></returns>
        public override bool Stop(bool reset = true)
        {
            Bass.BASS_ChannelStop(audioStream);
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

                Bass.BASS_ChannelStop(audioStream);
                Bass.BASS_StreamFree(audioStream);
                audioStream = 0;
            }
        }

        public override bool Load(byte[] audio, bool looping, string identifier = null)
        {
            if (!base.Load(audio, looping, identifier))
                return false;

            FreeMusic();

            audioHandle = GCHandle.Alloc(audio, GCHandleType.Pinned);

            audioStream = Bass.BASS_StreamCreateFile(audioHandle.AddrOfPinnedObject(), 0, audio.Length, BASSFlag.BASS_STREAM_PRESCAN | (looping ? BASSFlag.BASS_MUSIC_LOOP : 0));

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

                long audioTimeRaw = Bass.BASS_ChannelGetPosition(audioStream);
                return Bass.BASS_ChannelBytes2Seconds(audioStream, audioTimeRaw);
            }
        }

        public override bool Pause()
        {
            Bass.BASS_ChannelPause(audioStream);
            return true;
        }

        public override bool SeekTo(int milliseconds)
        {
            if (audioStream == 0) return false;

            Bass.BASS_ChannelSetPosition(audioStream, milliseconds / 1000d);
            return base.SeekTo(milliseconds);

        }

        #region IBackgroundAudioPlayer Members

        public override float CurrentPower
        {
            get
            {

                int word = Bass.BASS_ChannelGetLevel(audioStream);
                int left = Utils.LowWord32(word);
                int right = Utils.HighWord32(word);

                return (left + right) / 65536f * 2f;
            }
        }

        #endregion

        #region ITimeSource Members


        public override bool IsElapsing
        {
            get { return Bass.BASS_ChannelIsActive(audioStream) == BASSActive.BASS_ACTIVE_PLAYING; }
        }

        #endregion

        protected override void updateVolume()
        {
            if (audioStream == 0) return;
            Bass.BASS_ChannelSetAttribute(audioStream, BASSAttribute.BASS_ATTRIB_VOL, pMathHelper.ClampToOne(DimmableVolume * MaxVolume));
        }
    }
}
