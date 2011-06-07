using System.Collections.Generic;
using osum.GameplayElements.HitObjects.Osu;
using OpenTK;
using osum.GameplayElements;
using osum.GameplayElements.HitObjects;

namespace osum.GameplayElements.HitObjects
{
    internal abstract class HitFactory
    {
        protected readonly HitObjectManager hitObjectManager;

        internal HitFactory(HitObjectManager hitObjectMananager)
        {
            hitObjectManager = hitObjectMananager;
        }

        internal abstract HitCircle CreateHitCircle(Vector2 startPosition, int startTime, bool newCombo,
                                                    HitObjectSoundType soundType, int comboOffset);

        internal abstract Slider CreateSlider(Vector2 startPosition, int startTime, bool newCombo,
                                              HitObjectSoundType soundType, CurveTypes curveType, int repeatCount, double sliderLength, List<Vector2> sliderPoints, List<HitObjectSoundType> soundTypes, int comboOffset, double velocity, double tickDistance);

        internal abstract Spinner CreateSpinner(int startTime, int endTime, HitObjectSoundType soundType);

        internal abstract HoldCircle CreateHoldCircle(Vector2 pos, int time, bool newCombo, HitObjectSoundType soundType, int repeatCount, double length, List<HitObjectSoundType> sounds, int comboOffset, double velocity);
    }
}