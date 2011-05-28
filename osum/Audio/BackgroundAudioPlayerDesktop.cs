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
    class BackgroundAudioPlayerDesktop : IBackgroundAudioPlayer
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
        public bool Play()
        {
            Bass.BASS_ChannelPlay(audioStream, true);
            return true;
        }

        /// <summary>
        /// Stops the playing audio.
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            Bass.BASS_ChannelStop(audioStream);
            SeekTo(0);
            return true;
        }

        /// <summary>
        /// Updates this instance. Called every frame when loaded as a component.
        /// </summary>
        public void Update()
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

        public bool Load(byte[] audio, bool looping)
        {
            FreeMusic();

            audioHandle = GCHandle.Alloc(audio, GCHandleType.Pinned);

            audioStream = Bass.BASS_StreamCreateFile(audioHandle.AddrOfPinnedObject(), 0, audio.Length, BASSFlag.BASS_STREAM_PRESCAN | (looping ? BASSFlag.BASS_MUSIC_LOOP : 0));

            return true;
        }

        public bool Unload()
        {
            FreeMusic();
            return true;
        }

        public double CurrentTime
        {
            get {
                if (audioStream == 0) return 0;

                long audioTimeRaw = Bass.BASS_ChannelGetPosition(audioStream);
                return Bass.BASS_ChannelBytes2Seconds(audioStream, audioTimeRaw); 
            }
        }

        public bool Pause()
        {
            if (IsElapsing)
                Bass.BASS_ChannelPause(audioStream);
            else
                Bass.BASS_ChannelPlay(audioStream, false);
            
            return true;
        }

        public bool SeekTo(int milliseconds)
        {
            if (audioStream == 0) return false;

            Bass.BASS_ChannelSetPosition(audioStream, milliseconds / 1000d);
            return true;

        }

        #region IBackgroundAudioPlayer Members

        public float Volume
        {
            get
            {
                if (audioStream == 0) return 1;

                float o = 1;
                Bass.BASS_ChannelGetAttribute(audioStream, BASSAttribute.BASS_ATTRIB_VOL, ref o);
                return o;
            }
            set
            {
                if (audioStream == 0) return;
				
				Bass.BASS_ChannelSetAttribute(audioStream, BASSAttribute.BASS_ATTRIB_VOL, pMathHelper.ClampToOne(value));
            }
        }

        public float CurrentPower
        {
            get {

                int word = Bass.BASS_ChannelGetLevel(audioStream);
                int left = Utils.LowWord32(word);
                int right = Utils.HighWord32(word);

                return (left + right) / 65536f * 2f;
            }
        }

        #endregion

        #region ITimeSource Members


        public bool IsElapsing
        {
            get { return Bass.BASS_ChannelIsActive(audioStream) == BASSActive.BASS_ACTIVE_PLAYING; }
        }

        #endregion
    }
}
