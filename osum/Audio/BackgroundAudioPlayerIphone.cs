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

			string path = NSBundle.MainBundle.BundlePath + "/test.mp3";
			
			Console.WriteLine("ca find file ("+path+"):" + File.Exists(path));
			
			NSUrl url = NSUrl.FromFilename(path);
			
			NSError error;
			player = AVAudioPlayer.FromUrl(url,out error);
			
			player.MeteringEnabled = true;
		}
		
		public float CurrentVolume {
			get {
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

	}
}

