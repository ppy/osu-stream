using System;
using MonoTouch.AVFoundation;
using MonoTouch;
using MonoTouch.Foundation;
using MonoTouch.AudioToolbox;
using System.IO;

namespace osum
{
	public class BackgroundAudioPlayerIphone : IBackgroundAudioPlayer
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
		
		public float Volume {
			get {
				if (player == null) return 0;
				
				return player.Volume;
			}
			set {
				if (player == null) return;
				
				player.Volume = value;
			}
		}
		
		public float CurrentPower {
			get {
                if (player == null) return 0;

                player.UpdateMeters();
				return player.AveragePower(0);
			}
		}
		

		public bool Play()
		{
			if (player == null)
				return false;
			
			player.Play();
			
			return true;
		}
		
		public bool Pause()
		{
			if (player == null)
				return false;
			
			player.Pause();
			
			return true;
		}
		
		public void Update()
		{
			
		}

        public unsafe bool Load(byte[] audio, bool looping)
        {
            Unload();

            NSError error = null;

			using (NSData data = NSData.FromArray(audio))
			{
                player = AVAudioPlayer.FromData(data,out error);
                //player.MeteringEnabled = true; -- enable for CurrentPower readings
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
	            player.MeteringEnabled = true;
			}

            return error == null;
        }

        public bool Unload()
        {
            if (player != null)
                player.Dispose();
			return true;
        }

        public bool Stop ()
        {
        	if (player != null)
			{
				player.Stop();
				player.Dispose();
				player = null;
				
				return true;
			}
			
			return false;
        }
        
		public double CurrentTime
        {
            get
            {
                return player == null ? 0 : player.CurrentTime;
            }
        }

        public bool IsElapsing
        {
            get { return player != null && player.Playing; }
        }

		public bool SeekTo(int milliseconds)
		{
			if (player == null)
				return false;
			player.CurrentTime = milliseconds/1000d;
			return true;
		}
	}
}