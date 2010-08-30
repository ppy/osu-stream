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

        /// <summary>
        /// Internal judging of a Hit() call. Is only called after preliminary checks have been completed.
        /// </summary>
        /// <returns>A <see cref="IncreaseScoreType"/></returns>
        protected override IncreaseScoreType HitAction()
        {
            return IncreaseScoreType.Ignore;
        }

        /// <summary>
        /// Is this object currently within an active range?
        /// </summary>
        internal virtual bool IsActive
        {
            get
            {
                return StartTime < Clock.AudioTime && EndTime > Clock.AudioTime;
            }
        }

        /// <summary>
        /// This is called every frame that this object is visible to pick up any intermediary scoring that is not associated with the initial hit.
        /// </summary>
        /// <returns></returns>
        internal virtual IncreaseScoreType CheckScoring()
        {
            return IncreaseScoreType.Ignore;
        }

        internal override bool IsVisible
        {
            get { return Clock.AudioTime > StartTime && Clock.AudioTime < EndTime; }
        }
    }
}
