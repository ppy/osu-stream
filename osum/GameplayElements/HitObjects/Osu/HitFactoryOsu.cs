//  HitFactoryOsu.cs
//  Author: Dean Herbert <pe@ppy.sh>
//  Copyright (c) 2010 2010 Dean Herbert
using System.Collections.Generic;
using osum.GameplayElements;
using OpenTK;
using osum.GameplayElements.HitObjects;

namespace osu.GameplayElements.HitObjects.Osu
{
    internal class HitFactoryOsu : HitFactory
    {
        public HitFactoryOsu(HitObjectManager hitObjectMananager) : base(hitObjectMananager)
        {
        }

        internal override HitCircle CreateHitCircle(Vector2 startPosition, int startTime, bool newCombo,
                                                    HitObjectSoundType soundType, int comboOffset)
        {
            return new HitCircle(startPosition, startTime, newCombo, soundType);
        }

        internal override Slider CreateSlider(Vector2 startPosition, int startTime, bool newCombo,
                                              HitObjectSoundType soundType, CurveTypes curveType, int repeatCount, double sliderLength, List<Vector2> sliderPoints, List<HitObjectSoundType> soundTypes, int comboOffset)
        {
            return new Slider(startPosition, startTime, newCombo, soundType, curveType, repeatCount, sliderLength, sliderPoints, soundTypes);
        }

        internal override Spinner CreateSpinner(int startTime, int endTime, HitObjectSoundType soundType)
        {
            return new Spinner(startTime, endTime, soundType);
        }
    }
}
