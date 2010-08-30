using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using osum.Helpers;

namespace osum.GameplayElements.HitObjects
{
    class HitObjectSpannable : HitObject
    {
        public HitObjectSpannable(HitObjectManager hit_object_manager, Vector2 position, int startTime, HitObjectSoundType soundType, bool newCombo)
            : base(hit_object_manager, position, startTime, soundType, newCombo)
        {
        }

        internal virtual int EndPosition
        {
            get;
            set;
        }

        internal override int EndTime
        {
            get; set;
        }

        protected override IncreaseScoreType HitAction()
        {
            throw new NotImplementedException();
        }

        internal override bool IsVisible
        {
            get { return Clock.AudioTime > StartTime && Clock.AudioTime < EndTime; }
        }
    }
}
