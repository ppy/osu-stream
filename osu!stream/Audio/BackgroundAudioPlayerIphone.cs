using System;
using AVFoundation;
using MonoTouch;
using Foundation;
using AudioToolbox;
using System.IO;
using osum.Helpers;
using osum.Audio;

namespace osum
{
	public class BackgroundAudioPlayerIphone : BackgroundAudioPlayer
	{
		AVAudioPlayer player;

		public BackgroundAudioPlayerIphone()
		{
#if !SIMULATOR
			AudioSession.Initialize();
			AudioSession.Category = AudioSessionCategory.SoloAmbientSound;
			AudioSession.SetActive(true);
#endif
		}

		protected override void updateVolume()
        {
            if (player == null) return;
            player.Volume = pMathHelper.ClampToOne(DimmableVolume * MaxVolume);
		}

		public override float CurrentPower {
			get {
                if (player == null) return 0;

                player.UpdateMeters();
				return player.AveragePower(0);
			}
		}

		public override bool Prepare()
		{
			if (player == null) return false;

			player.PrepareToPlay();
			return true;
		}

		public override bool Play()
		{
			if (player == null)
				return false;

			player.Play();

			return true;
		}

		public override bool Pause()
		{
			if (player == null)
				return false;

            player.Pause();
			return true;
		}

		public override void Update()
		{

		}

        public override unsafe bool Load(byte[] audio, bool looping, string identifier = null)
        {
            if (!base.Load(audio, looping, identifier))
                return false;

            Unload();

            NSError error = null;

			using (NSData data = NSData.FromArray(audio))
			{
                player = AVAudioPlayer.FromData(data,out error);
				Prepare();
                //player.MeteringEnabled = true; -- enable for CurrentPower readings
                updateVolume();
                player.NumberOfLoops = looping ? -1 : 0;
			}

            return error == null;
        }

        public bool Load(string filename)
        {
            Unload();

            string path = filename;

			NSError error = null;

            using (NSUrl url = NSUrl.FromFilename(path))
			{
	            player = AVAudioPlayer.FromUrl(url,out error);
                player.PrepareToPlay();
                updateVolume();
	            //player.MeteringEnabled = true;
			}

            return error == null;
        }

        public override bool Unload()
        {
            if (player != null)
                player.Dispose();
			return true;
        }

        public override bool Stop(bool reset = true)
        {
        	if (player != null)
			{
				player.Stop();
                if (reset) SeekTo(0);
				return true;
			}

			return false;
        }

		public override double CurrentTime
        {
            get
            {
                return player == null ? 0 : player.CurrentTime;
            }
        }

        public override bool IsElapsing
        {
            get { return player != null && player.Playing; }
        }

		public override bool SeekTo(int milliseconds)
		{
			if (player == null)
				return false;

            if (IsElapsing)
            {
                player.Stop();
                player.CurrentTime = milliseconds/1000d;
                player.Play();
            }
            else
            {
                player.CurrentTime = milliseconds/1000d;
            }

			return base.SeekTo(milliseconds);
		}
	}
}