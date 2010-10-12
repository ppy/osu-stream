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
		
		public float CurrentVolume {
			get {
                if (player == null) return 0;

                player.UpdateMeters();
				return player.AveragePower(0);
			}
		}
		

		public bool Play ()
		{
			if (player == null)
				return false;
			
			player.Play();
			
			return true;
		}
		
		public void Update()
		{
			
		}

        public unsafe bool Load(byte[] bytes)
        {
            if (player != null)
                player.Dispose();

            NSError error = null;


            fixed (byte* ptr = bytes)
            {
                NSData data = NSData.FromBytes((IntPtr)ptr,(uint)bytes.Length);

                player = AVAudioPlayer.FromData(data,out error);
                player.MeteringEnabled = true;
            }

            return error == null;
        }

        public bool Load(string filename)
        {
            if (player != null)
                player.Dispose();

            string path = filename;//NSBundle.MainBundle.BundlePath + "/test.mp3";
            NSError error;

            NSUrl url = NSUrl.FromFilename(path);

            player = AVAudioPlayer.FromUrl(url,out error);
            player.MeteringEnabled = true;

            return error == null;
        }

        public double CurrentTime
        {
            get
            {
                return player == null ? 0 : player.CurrentTime;
            }
        }
    }
}

