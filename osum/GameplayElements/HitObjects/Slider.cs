//  Slider.cs
//  Author: Dean Herbert <pe@ppy.sh>
//  Copyright (c) 2010 2010 Dean Herbert
using System;
namespace osum.GameplayElements.HitObjects
{

    internal enum CurveTypes
    {
        Catmull,
        Bezier,
        Linear
    } ;

    internal class Slider : HitObject
    {
        protected override IncreaseScoreType HitAction()
        {
            throw new System.NotImplementedException();
        }


        internal override HitObject Clone()
        {
            throw new System.NotImplementedException();
        }


        internal override int ComboNumber {
            get {
                throw new System.NotImplementedException();
            }
            set {
                throw new System.NotImplementedException();
            }
        }


        internal override bool IsVisible {
            get {
                throw new System.NotImplementedException();
            }
        }


        internal override IncreaseScoreType GetScorePoints(OpenTK.Vector2 currentMousePos)
        {
            throw new System.NotImplementedException();
        }


        internal override void SetEndTime(int time)
        {
            throw new System.NotImplementedException();
        }

        public Slider() : base()
        {
        }
    }
}

