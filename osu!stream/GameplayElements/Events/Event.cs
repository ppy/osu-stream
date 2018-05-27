using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Text;

namespace osum.GameplayElements.Events
{
    class Event
    {
        
    }

    internal enum EventType
    {
        Background = 0,
        Video = 1,
        Break = 2,
        Colour = 3,
        Sprite = 4,
        Sample = 5,
        Animation = 6
    }

    internal enum EventLoopTrigger
    {
        HitSoundClap,
        HitSoundFinish,
        HitSoundWhistle,
        Passing,
        Failing
    }

    internal enum StoryLayer
    {
        Background = 0,
        Fail = 1,
        Pass = 2,
        Foreground = 3
    }
}
