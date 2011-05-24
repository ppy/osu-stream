using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using osum.Graphics.Skins;
using osum.Graphics.Sprites;
using osum.Helpers;
using osum.Graphics.Drawables;

namespace osum.GameplayElements
{
    internal class HitCircle : HitObject
    {
        #region General & Timing

        private const float TEXT_SIZE = 0.8f;

        internal HitCircle(HitObjectManager hit_object_manager, Vector2 pos, int startTime, bool newCombo, int comboOffset, HitObjectSoundType soundType)
            : base(hit_object_manager, pos, startTime, soundType, newCombo, comboOffset)
        {
            Type = HitObjectType.Circle;

            Color4 white = Color4.White;

            SpriteHitCircle1 =
                new pSprite(TextureManager.Load(OsuTexture.hitcircle), FieldTypes.GamefieldSprites, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(StartTime), false, white);
            Sprites.Add(SpriteHitCircle1);
            //SpriteHitCircle1.TagNumeric = 1;
            SpriteCollectionDim.Add(SpriteHitCircle1);


            SpriteHitCircle2 =
                new pSprite(TextureManager.Load(OsuTexture.hitcircleoverlay), FieldTypes.GamefieldSprites,
                            OriginTypes.Centre, ClockTypes.Audio, Position,
                            SpriteManager.drawOrderBwd(StartTime - (BeatmapManager.ShowOverlayAboveNumber ? 2 : 1)), false, Color4.White);
            Sprites.Add(SpriteHitCircle2);
            SpriteCollectionDim.Add(SpriteHitCircle2);
            SpriteHitCircleText = new pSpriteText(null, "default", 3, //SkinManager.Current.FontHitCircle, SkinManager.Current.FontHitCircleOverlap, 
                                                    FieldTypes.GamefieldSprites, OriginTypes.Centre,
                                                    ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(StartTime - (BeatmapManager.ShowOverlayAboveNumber ? 1 : 2)),
                                                    false, white);
            SpriteHitCircleText.TextConstantSpacing = false;

            SpriteApproachCircle = new ApproachCircle(Position, 1, false, 1, white);
            SpriteApproachCircle.Clocking = ClockTypes.Audio;
            Sprites.Add(SpriteApproachCircle);

            SpriteHitCircleText.ScaleScalar = TEXT_SIZE;

            if (ShowCircleText)
            {
                Sprites.Add(SpriteHitCircleText);
                SpriteCollectionDim.Add(SpriteHitCircleText);
            }

            SpriteApproachCircle.Transform(new Transformation(TransformationType.Fade, 0, 0.9F,
                startTime - DifficultyManager.PreEmpt, Math.Min(startTime, startTime - DifficultyManager.PreEmpt + DifficultyManager.FadeIn * 2)));

            SpriteApproachCircle.Transform(new Transformation(TransformationType.Scale, 4, 1,
                startTime - DifficultyManager.PreEmpt, startTime));

            SpriteApproachCircle.Transform(new Transformation(TransformationType.Fade, 0.9f, 0,
                startTime, startTime + (int)(DifficultyManager.PreEmpt * 0.1f)));

            Transformation fadeIn = new Transformation(TransformationType.Fade, 0, 1,
                startTime - DifficultyManager.PreEmpt, startTime - DifficultyManager.PreEmpt + DifficultyManager.FadeIn);

            SpriteHitCircle1.Transform(fadeIn);
            SpriteHitCircle2.Transform(fadeIn);
            SpriteHitCircleText.Transform(fadeIn);

            Transformation fadeOut = new Transformation(TransformationType.Fade, 1, 0,
                startTime, startTime + DifficultyManager.HitWindow50);

            SpriteHitCircle1.Transform(fadeOut);
            SpriteHitCircle2.Transform(fadeOut);
            SpriteHitCircleText.Transform(fadeOut);
        }

        protected virtual bool ShowCircleText
        {
            get { return true; }
        }

        protected virtual bool ShowApproachCircle
        {
            get { return true; }
        }

        protected override ScoreChange HitActionInitial()
        {
            int hitTime = Clock.AudioTime;
            int accuracy = Math.Abs(hitTime - StartTime);

            if (accuracy < DifficultyManager.HitWindow300)
                hitValue = ScoreChange.Hit300;
            else if (accuracy < DifficultyManager.HitWindow100)
                hitValue = ScoreChange.Hit100;
            else if (accuracy < DifficultyManager.HitWindow50)
                hitValue = ScoreChange.Hit50;
            else
                hitValue = ScoreChange.Miss;

            if (hitValue > 0)
                PlaySound();

            return hitValue;
        }

        internal override void HitAnimation(ScoreChange action)
        {
            if (action > 0)
            {

                //Fade out the actual hit circle
                Transformation circleScaleOut = new Transformation(TransformationType.Scale, 1.1F, 1.4F,
                    Clock.AudioTime, Clock.AudioTime + DifficultyManager.FadeOut, EasingTypes.InHalf);

                Transformation textScaleOut = new Transformation(TransformationType.Scale, 1.1F * TEXT_SIZE, 1.4F * TEXT_SIZE,
                    Clock.AudioTime, Clock.AudioTime + DifficultyManager.FadeOut, EasingTypes.InHalf);

                Transformation circleFadeOut = new Transformation(TransformationType.Fade, 1, 0,
                    Clock.AudioTime, Clock.AudioTime + DifficultyManager.FadeOut);

                SpriteHitCircle1.Transformations.Clear();
                SpriteHitCircle1.Transform(circleScaleOut);
                SpriteHitCircle1.Transform(circleFadeOut);

                SpriteHitCircle2.Transformations.Clear();
                SpriteHitCircle2.Transform(circleScaleOut);
                SpriteHitCircle2.Transform(circleFadeOut);

                SpriteHitCircleText.Transformations.Clear();
                SpriteHitCircleText.Transform(textScaleOut);
                SpriteHitCircleText.Transform(circleFadeOut);


                if (connectedObject != null)
                    connectionSprite.FadeOut(100);

                SpriteApproachCircle.Transformations.Clear();
            }
            else
            {
                foreach (pDrawable p in Sprites)
                    p.Transformations.Clear();
            }

            base.HitAnimation(action);
        }

        #endregion

        internal pDrawable SpriteApproachCircle;
        internal pSprite SpriteHitCircle1;
        internal pSprite SpriteHitCircle2;
        internal pSpriteText SpriteHitCircleText;

        private int comboNumber;
        internal override int ComboNumber
        {
            get { return comboNumber; }
            set
            {
                if (value == comboNumber) return;

                if (value > 0)
                    SpriteHitCircleText.Text = value.ToString();
                else
                    SpriteHitCircleText.Text = string.Empty;

                comboNumber = value;
            }
        }

        internal override bool IsVisible
        {
            get
            {
                return Clock.AudioTime >= StartTime - DifficultyManager.PreEmpt &&
                     Clock.AudioTime <= EndTime + DifficultyManager.FadeOut;
            }
        }

        internal override Color4 Colour
        {
            get
            {
                return base.Colour;
            }
            set
            {
                SpriteHitCircle1.Colour = value;
                SpriteApproachCircle.Colour = value;

                base.Colour = value;
            }
        }

    }

}
