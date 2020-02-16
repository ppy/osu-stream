using System;
using OpenTK;
using OpenTK.Graphics;
using osum.GameModes.Play;
using osum.Graphics;
using osum.Graphics.Drawables;
using osum.Graphics.Sprites;
using osum.Helpers;

namespace osum.GameplayElements.HitObjects.Osu
{
    internal class HitCircle : HitObject
    {
        #region General & Timing

        internal HitCircle(HitObjectManager hit_object_manager, Vector2 pos, int startTime, bool newCombo, int comboOffset, HitObjectSoundType soundType)
            : base(hit_object_manager, pos, startTime, soundType, newCombo, comboOffset)
        {
            Type = HitObjectType.Circle;

            Color4 white = Color4.White;

            SpriteHitCircle1 =
                new pSprite(TextureManager.Load(OsuTexture.hitcircle0), FieldTypes.GamefieldSprites, OriginTypes.Centre, ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(StartTime), false, white);
            Sprites.Add(SpriteHitCircle1);
            //SpriteHitCircle1.TagNumeric = 1;
            SpriteHitCircle1.TagNumeric = DIMMABLE_TAG;


            SpriteHitCircleText = new pSpriteText(null, "default", 3, //SkinManager.Current.FontHitCircle, SkinManager.Current.FontHitCircleOverlap,
                                                    FieldTypes.GamefieldSprites, OriginTypes.Centre,
                                                    ClockTypes.Audio, Position, SpriteManager.drawOrderBwd(StartTime - 1),
                                                    false, white);
            SpriteHitCircleText.TextConstantSpacing = false;

            SpriteHitCircleText.TagNumeric = DIMMABLE_TAG;

            SpriteApproachCircle = new ApproachCircle(Position, 1, false, 1, white);
            SpriteApproachCircle.Clocking = ClockTypes.Audio;
            Sprites.Add(SpriteApproachCircle);

            if (ShowCircleText)
            {
                Sprites.Add(SpriteHitCircleText);
            }

            SpriteApproachCircle.Transform(new TransformationF(TransformationType.Fade, 0, 0.9F,
                startTime - DifficultyManager.PreEmpt, Math.Min(startTime, startTime - DifficultyManager.PreEmpt + DifficultyManager.FadeIn * 2)));

            SpriteApproachCircle.Transform(new TransformationF(TransformationType.Scale, 4, 1,
                startTime - DifficultyManager.PreEmpt, startTime));

            SpriteApproachCircle.Transform(new TransformationF(TransformationType.Fade, 0.9f, 0,
                startTime, startTime + (int)(DifficultyManager.PreEmpt * 0.1f)));

            Transformation fadeIn = new TransformationF(TransformationType.Fade, 0, 1,
                startTime - DifficultyManager.PreEmpt, startTime - DifficultyManager.PreEmpt + DifficultyManager.FadeIn);

            SpriteHitCircle1.Transform(fadeIn);
            SpriteHitCircleText.Transform(fadeIn);

            Transformation fadeOut = new TransformationF(TransformationType.Fade, 1, 0,
                startTime, startTime + DifficultyManager.HitWindow50);

            SpriteHitCircle1.Transform(fadeOut);
            SpriteHitCircleText.Transform(fadeOut);
        }

        protected virtual bool ShowCircleText => true;

        protected virtual bool ShowApproachCircle => true;

        protected override ScoreChange HitActionInitial()
        {
            int hitTime = Clock.AudioTimeInputAdjust;
            int accuracy = Math.Abs(hitTime - StartTime);

            if (accuracy < DifficultyManager.HitWindow300 || Player.Autoplay)
                hitValue = ScoreChange.Hit300;
            else if (accuracy < DifficultyManager.HitWindow100)
                hitValue = ScoreChange.Hit100;
            else if (accuracy < DifficultyManager.HitWindow50)
                hitValue = ScoreChange.Hit50;
            else
                hitValue = ScoreChange.Miss;

            if (hitValue != ScoreChange.Miss)
                PlaySound();

            return hitValue;
        }

        internal override void HitAnimation(ScoreChange action, bool animateNumber = false)
        {
            SpriteHitCircle1.Transformations.Clear();
            SpriteHitCircleText.Transformations.Clear();
            SpriteApproachCircle.Transformations.Clear();

            if (connectedObject != null)
                connectionSprite.FadeOut(100);

            int now = SpriteHitCircle1.ClockingNow;

            if (action > ScoreChange.Miss)
            {
                //Fade out the actual hit circle
                Transformation circleScaleOut = new TransformationF(TransformationType.Scale, 1.1F, 1.4F,
                    now, now + DifficultyManager.FadeOut, EasingTypes.InHalf);

                Transformation textScaleOut = new TransformationF(TransformationType.Scale, 1.1F, 1.4F,
                    now, now + DifficultyManager.FadeOut, EasingTypes.InHalf);

                Transformation circleFadeOut = new TransformationF(TransformationType.Fade, 1, 0,
                    now, now + DifficultyManager.FadeOut);

                SpriteHitCircle1.Transformations.Clear();
                SpriteHitCircle1.Transform(circleScaleOut);
                SpriteHitCircle1.Transform(circleFadeOut);

                SpriteHitCircleText.Transformations.Clear();
                if (animateNumber)
                {
                    SpriteHitCircleText.Transform(textScaleOut);
                    SpriteHitCircleText.Transform(circleFadeOut);
                }
            }

            base.HitAnimation(action);
        }

        #endregion

        internal pDrawable SpriteApproachCircle;
        internal pSprite SpriteHitCircle1;
        internal pSpriteText SpriteHitCircleText;

        private int comboNumber;
        internal override int ComboNumber
        {
            get => comboNumber;
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

        public override bool IsVisible => SpriteHitCircle1.Alpha > 0;

        internal override int ColourIndex {
            get => base.ColourIndex;
            set {
                SpriteHitCircle1.Texture = TextureManager.Load(OsuTexture.hitcircle0 + value);
                base.ColourIndex = value;
            }
        }

        internal override Color4 Colour
        {
            get => base.Colour;
            set
            {
                SpriteApproachCircle.Colour = value;

                base.Colour = value;
            }
        }

    }

}
