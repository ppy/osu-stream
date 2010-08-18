using System;
using osum.Support;
namespace osum
{
	public interface IBackgroundAudioPlayer : IUpdateable
	{
		float CurrentVolume
		{
			get;
		}
		
		bool Play();
	}
}

