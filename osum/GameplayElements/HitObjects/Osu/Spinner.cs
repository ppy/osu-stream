using System;
using System.Collections.Generic;
using osum.Graphics;
using osum.Graphics.Skins;
using osum.Graphics.Sprites;
using osum.Helpers;
using OpenTK;
using OpenTK.Graphics;
using osum.GameplayElements.HitObjects;
using osum.Graphics.Drawables;

namespace osum.GameplayElements
{
    internal class Spinner : HitObjectSpannable
    {
        /// <summary>
        /// Used for the flicker effects on the score metre.
        /// </summary>
        private static readonly Random randomizer = new Random();

        private readonly ApproachCircle ApproachCircle;
        internal readonly pSprite SpriteBackground;
        private readonly pSprite SpriteClear;
        private readonly pSprite spriteRpmBackground;
        private readonly pSpriteText spriteRpmText;
        private readonly pRectangle spriteScoreMetreBackground;
        private readonly pRectangle spriteScoreMetreForeground;
        private readonly pSprite SpriteSpin;
        protected pSpriteText spriteBonus;
        protected pSprite spriteCircle;

        /// <summary>
        /// The fastest acceleration that is allowed (depends on length of spinner).
        /// </summary>
        private double AccelerationCap;

        /// <summary>
        /// Have we cleared the spinner?
        /// </summary>
        private bool Cleared;

        /// <summary>
        /// Number of rotations currently spun.
        /// </summary>
        internal int currentRotationCount;


        /// <summary>
        /// Number of scored rotations (last scoring update).
        /// </summary>
        private int lastRotationCount;

        /// <summary>
        /// Number of scored rotations.
        /// </summary>
        private int scoringRotationCount;


        /// <summary>
        /// Number of rotations are required for a "clear".
        /// </summary>
        internal int rotationRequirement;

        /// <summary>
        /// Weighted RPM value (used for display).
        /// </summary>
        private double Rpm;

        /// <summary>
        /// Has the spinner started spinning? (used for hiding SPIN! graphic).
        /// </summary>
        private bool StartedSpinning;

        /// <summary>
        /// Velocity the spinner is visually spinning at.
        /// </summary>
        internal double velocityCurrent;

        /// <summary>
        /// Velocity the cursor is "spinning" at.
        /// </summary>
        protected double velocityFromInputPerMillisecond;

        Vector2 spinnerCentre = new Vector2(0, 210);

        internal Spinner(HitObjectManager hitObjectManager, int startTime, int endTime, HitObjectSoundType soundType)
            : base(hitObjectManager, Vector2.Zero, startTime, soundType, true, 0)
        {
            Position = (new Vector2(GameBase.BaseSize.Width / 2, GameBase.BaseSize.Height) - spinnerCentre) - GameBase.GamefieldOffsetVector1;
            EndTime = endTime;
            Type = HitObjectType.Spinner;
            Colour = Color4.Gray;

            Color4 white = Color4.White;

            //Check for a jpg background for beatmap-based skins (used to reduce filesize), then fallback to png.
            SpriteBackground =
                new pSprite(TextureManager.Load(OsuTexture.spinner_background),
                            FieldTypes.StandardSnapBottomCentre, OriginTypes.BottomCentre, ClockTypes.Audio,
                            new Vector2(0, 0), SpriteManager.drawOrderFwdLowPrio(StartTime - 1), false, white);
            Sprites.Add(SpriteBackground);

            spriteCircle =
                new pSprite(TextureManager.Load(OsuTexture.spinner_circle),
                            FieldTypes.StandardSnapBottomCentre, OriginTypes.Centre, ClockTypes.Audio,
                            spinnerCentre, SpriteManager.drawOrderFwdLowPrio(StartTime), false, white);
            Sprites.Add(spriteCircle);

            spriteScoreMetreBackground =
                new pRectangle(Vector2.Zero, new Vector2(GameBase.BaseSizeFixedWidth.Width, GameBase.BaseSize.Height), false, SpriteManager.drawOrderFwdLowPrio(StartTime - 3), new Color4(20, 20, 20, 255))
                {
                    Clocking = ClockTypes.Audio,
                    Field = FieldTypes.StandardSnapBottomCentre,
                    Origin = OriginTypes.BottomCentre
                };

            Sprites.Add(spriteScoreMetreBackground);

            spriteScoreMetreForeground =
                new pRectangle(Vector2.Zero, new Vector2(GameBase.BaseSizeFixedWidth.Width, 0), false, SpriteManager.drawOrderFwdLowPrio(StartTime - 2), Color4.OrangeRed)
                {
                    Clocking = ClockTypes.Audio,
                    Field = FieldTypes.StandardSnapBottomCentre,
                    Origin = OriginTypes.BottomCentre
                };

            Sprites.Add(spriteScoreMetreForeground);

            spriteRpmBackground =
                new pSprite(TextureManager.Load(OsuTexture.spinner_spm),
                            FieldTypes.StandardSnapBottomCentre, OriginTypes.BottomCentre, ClockTypes.Audio,
                            Vector2.Zero, SpriteManager.drawOrderFwdLowPrio(StartTime + 3), false, white);
            Sprites.Add(spriteRpmBackground);

            spriteRpmText = new pSpriteText("100", "score", 3,
                                            FieldTypes.StandardSnapBottomCentre, OriginTypes.BottomCentre, ClockTypes.Audio,
                                            new Vector2(10, 0), SpriteManager.drawOrderFwdLowPrio(StartTime + 4), false, white);
            spriteRpmText.ScaleScalar = 0.9f;
            Sprites.Add(spriteRpmText);

            ApproachCircle = new ApproachCircle(spinnerCentre, 1, false, SpriteManager.drawOrderFwdLowPrio(StartTime + 2), new Color4(77 / 255f, 139 / 255f, 217 / 255f, 1));
            ApproachCircle.Width = 8;
            ApproachCircle.Clocking = ClockTypes.Audio;
            ApproachCircle.Field = FieldTypes.StandardSnapBottomCentre;
            Sprites.Add(ApproachCircle);

            spriteBonus = new pSpriteText("", "score", 3, // SkinManager.Current.FontScore, SkinManager.Current.FontScoreOverlap,
                                          FieldTypes.StandardSnapBottomCentre, OriginTypes.Centre, ClockTypes.Audio,
                                          spinnerCentre - new Vector2(0, 80), SpriteManager.drawOrderFwdLowPrio(StartTime + 3), false, white);
            Sprites.Add(spriteBonus);

            foreach (pDrawable p in Sprites)
            {
                p.Transformations.Clear();
                p.Transform(new Transformation(TransformationType.Fade, 0, 1, StartTime - DifficultyManager.FadeIn, StartTime));
                p.Transform(new Transformation(TransformationType.Fade, 1, 0, EndTime, EndTime + DifficultyManager.FadeOut));
                p.AlignToSprites = true;
            }

            SpriteSpin =
                new pSprite(TextureManager.Load(OsuTexture.spinner_spin),
                            FieldTypes.StandardSnapBottomCentre, OriginTypes.Centre, ClockTypes.Audio,
                            spinnerCentre, SpriteManager.drawOrderFwdLowPrio(StartTime + 2), false, white);
            SpriteSpin.Transform(new Transformation(TransformationType.Fade, 0, 1, StartTime - DifficultyManager.FadeIn / 2, StartTime));
            SpriteSpin.Transform(new Transformation(TransformationType.Fade, 1, 0, EndTime - Math.Min(400, endTime - startTime), EndTime));
            SpriteSpin.AlignToSprites = true;
            Sprites.Add(SpriteSpin);

            ApproachCircle.Transform(new Transformation(TransformationType.Scale, GameBase.BaseSizeFixedWidth.Height * 0.47f, 0.1f, StartTime, EndTime));

            SpriteClear =
                new pSprite(TextureManager.Load(OsuTexture.spinner_clear),
                            FieldTypes.StandardSnapBottomCentre, OriginTypes.Centre, ClockTypes.Audio,
                            spinnerCentre + new Vector2(0, 80), SpriteManager.drawOrderFwdLowPrio(StartTime + 3), false, white);
            SpriteClear.AlignToSprites = true;
            SpriteClear.Transform(new Transformation(TransformationType.Fade, 0, 0, startTime, endTime));
            Sprites.Add(SpriteClear);

            spriteRpmText.Transform(new Transformation(
                spriteRpmText.Position + new Vector2(0, 50), spriteRpmText.Position,
                StartTime - DifficultyManager.FadeIn, StartTime, EasingTypes.In));
            spriteRpmBackground.Transform(new Transformation(
                spriteRpmBackground.Position + new Vector2(0, 50), spriteRpmBackground.Position,
                StartTime - DifficultyManager.FadeIn, StartTime, EasingTypes.In));

            currentRotationCount = 0;
            rotationRequirement = (int)((float)(EndTime - StartTime) / 1000 * DifficultyManager.SpinnerRotationRatio);
            AccelerationCap = 0.00008 + Math.Max(0, (5000 - (double)(EndTime - StartTime)) / 1000 / 2000);
        }

        internal override int ComboNumber
        {
            get { return 1; }
            set { }
        }

        internal override bool IsVisible
        {
            get
            {
                return Clock.AudioTime >= StartTime - DifficultyManager.FadeIn && Clock.AudioTime <= EndTime;
            }
        }

        internal override void Shake()
        {
            return;
        }

        internal override int HittableEndTime
        {
            get
            {
                return EndTime;
            }
        }

        TrackingPoint cursorTrackingPoint;
        Vector2 cursorTrackingPosition;
        internal override ScoreChange CheckScoring()
        {
            //Update the angles
            velocityFromInputPerMillisecond = 0;

            ScoreChange change = base.CheckScoring();
            if (change != ScoreChange.Ignore)
                return change;

            if (InputManager.PrimaryTrackingPoint != cursorTrackingPoint)
            {
                cursorTrackingPoint = InputManager.PrimaryTrackingPoint;
                return ScoreChange.Ignore;
            }

            if (cursorTrackingPoint == null || !InputManager.IsPressed)
                return ScoreChange.Ignore;

            if (InputManager.PrimaryTrackingPoint == null)
                return ScoreChange.Ignore;

            Vector2 centre = spriteCircle.FieldPosition / GameBase.BaseToNativeRatio;

            Vector2 oldPos = cursorTrackingPosition - centre;

            //Update to the new mouse position.
            cursorTrackingPosition = cursorTrackingPoint.BasePosition;

            Vector2 newPos = cursorTrackingPosition - centre;

            double oldAngle = Math.Atan2(oldPos.Y, oldPos.X);
            double newAngle = Math.Atan2(newPos.Y, newPos.X);

            double angleDiff = newAngle - oldAngle;

            if (angleDiff < -Math.PI)
                angleDiff = (2 * Math.PI) + angleDiff;
            else if (oldAngle - newAngle < -Math.PI)
                angleDiff = (-2 * Math.PI) - angleDiff;

            velocityFromInputPerMillisecond = angleDiff / GameBase.ElapsedMilliseconds;

            ScoreChange score = ScoreChange.Ignore;

            //Update the rotation count
            if (currentRotationCount != lastRotationCount)
            {
                scoringRotationCount++;

                if (scoringRotationCount > rotationRequirement + 3 &&
                    (scoringRotationCount - (rotationRequirement + 3)) % 2 == 0)
                {
                    score = ScoreChange.SpinnerBonus;
                    //AudioEngine.PlaySample(AudioEngine.s_SpinnerBonus, AudioEngine.VolumeSample);
                    spriteBonus.Text = (1000 * (scoringRotationCount - (rotationRequirement + 3)) / 2).ToString();
                    spriteBonus.Transformations.Clear();
                    spriteBonus.Transform(
                        new Transformation(TransformationType.Fade, 1, 0, Clock.AudioTime, Clock.AudioTime + 800, EasingTypes.In));
                    spriteBonus.Transform(
                        new Transformation(TransformationType.Scale, 1.28F, 2f, Clock.AudioTime, Clock.AudioTime + 800, EasingTypes.In));
                    //Ensure we don't recycle this too early.
                    spriteBonus.Transform(
                        new Transformation(TransformationType.Fade, 0, 0, EndTime + 800, EndTime + 800));
                }
                else if (scoringRotationCount > 1 && scoringRotationCount % 2 == 0)
                    score = ScoreChange.SpinnerSpinPoints;
                else if (scoringRotationCount > 1)
                    score = ScoreChange.SpinnerSpin;
            }

            lastRotationCount = currentRotationCount;

            return score;
        }

        public override void Update()
        {
            base.Update();

            if (IsHit || Clock.AudioTime < StartTime) // || (!InputManager.ScorableFrame))
                return;

            Rpm = Rpm * 0.9 + 0.1 * (Math.Abs(velocityCurrent) * 60000) / (Math.PI * 2);

            spriteRpmText.Text = string.Format("{0:#,0}", Rpm);

            SetScoreMeter((int)((float)scoringRotationCount / rotationRequirement * 100));

            if (IsActive)
            {
                double maxAccelPerSec = AccelerationCap * GameBase.ElapsedMilliseconds;

                if (velocityFromInputPerMillisecond > velocityCurrent)
                {
                    velocityCurrent = velocityCurrent +
                        Math.Min(velocityFromInputPerMillisecond - velocityCurrent / 4, maxAccelPerSec);
                }
                else
                {
                    velocityCurrent = velocityCurrent +
                        Math.Max(velocityFromInputPerMillisecond - velocityCurrent / 4, -maxAccelPerSec);
                }

                //hard rate limit
                velocityCurrent = Math.Max(-0.05, Math.Min(velocityCurrent, 0.05));

                spriteCircle.Rotation = spriteCircle.Rotation + (float)(velocityCurrent * GameBase.ElapsedMilliseconds);


                if (velocityCurrent != 0)
                    StartSound();
                else
                    StopSound();

                currentRotationCount = (int)(spriteCircle.Rotation / (Math.PI * 2));
            }

            if (scoringRotationCount >= rotationRequirement && !Cleared)
            {
                Cleared = true;
                if (SpriteSpin != null)
                {
                    SpriteSpin.FadeOut(100);

                    SpriteClear.Transformations.Clear();
                    SpriteClear.Transform(new Transformation(TransformationType.Fade, 0, 1, Clock.AudioTime, Math.Min(EndTime, Clock.AudioTime + 400), EasingTypes.In));
                    SpriteClear.Transform(new Transformation(TransformationType.Scale, 2, 0.8f, Clock.AudioTime, Math.Min(EndTime, Clock.AudioTime + 240), EasingTypes.In));
                    SpriteClear.Transform(new Transformation(TransformationType.Scale, 0.8f, 1, Math.Min(EndTime, Clock.AudioTime + 240), Math.Min(EndTime, Clock.AudioTime + 400), EasingTypes.None));
                    SpriteClear.Transform(new Transformation(TransformationType.Fade, 1, 0, EndTime - 50, EndTime));
                }
            }

            //Hide the "SPIN!" sprite once we have started spinning.
            if (scoringRotationCount > 0 && !StartedSpinning)
            {
                if (SpriteSpin != null)
                {
                    if (Clock.AudioTime > StartTime + 500)
                    {
                        SpriteSpin.FadeOut(300);
                        StartedSpinning = true;
                    }
                }
            }
        }

        private void StartSound()
        {
            //if (SkinManager.Current.SpinnerFrequencyModulate)
            //    Bass.BASS_ChannelSetAttribute(AudioEngine.ch_spinnerSpin, BASSAttribute.BASS_ATTRIB_FREQ,
            //                                  Math.Min(100000,
            //                                           20000 +
            //                                           (int)(40000 * ((float)scoringRotationCount / rotationRequirement))));
            //if (!AudioEngine.IsPlaying(AudioEngine.ch_spinnerSpin))
            //    Bass.BASS_ChannelPlay(AudioEngine.ch_spinnerSpin, false);
            //Bass.BASS_ChannelSetAttribute(AudioEngine.ch_spinnerSpin, BASSAttribute.BASS_ATTRIB_VOL, (float)AudioEngine.VolumeSample / 100);
        }

        internal override void StopSound()
        {
            //Bass.BASS_ChannelPause(AudioEngine.ch_spinnerSpin);
        }

        private void SetScoreMeter(int percent)
        {
            percent = Math.Min(99, percent);

            int randomAmount = percent % 10;
            int barCount = percent / 10;


            if (randomizer.NextDouble() < (float)randomAmount / 10) // || SkinManager.Current.SpinnerNoBlink)
                barCount++;

            spriteScoreMetreForeground.Scale.Y = 42.6f * barCount;
        }

        internal override bool HitTestInitial(TrackingPoint tracking)
        {
            return false;
        }

        protected override ScoreChange HitActionInitial()
        {
            if (Clock.AudioTime < EndTime)
                return ScoreChange.Ignore;

            StopSound();

            ScoreChange val = ScoreChange.Miss;
            if (scoringRotationCount > rotationRequirement + 1)
                val = ScoreChange.Hit300;
            else if (scoringRotationCount > rotationRequirement)
                val = ScoreChange.Hit100;
            else if (scoringRotationCount > rotationRequirement - 1)
                val = ScoreChange.Hit50;
            if (val > 0)
                PlaySound();
            return val;
        }
    }
}
