using System;
namespace osum
{
	public interface IBackgroundAudioPlayer
	{
		float CurrentVolume
		{
			get;
		}
		
		bool Play();
	}
}

