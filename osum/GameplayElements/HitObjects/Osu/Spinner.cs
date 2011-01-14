using System;
using System.Collections.Generic;
using osum.Graphics;
using osum.Graphics.Skins;
using osum.Graphics.Sprites;
using osum.Helpers;
using OpenTK;
using OpenTK.Graphics;
using osum.GameplayElements.HitObjects;

namespace osum.GameplayElements
{
    internal class Spinner : HitObjectSpannable
    {
        /// <summary>
        /// Used for the flicker effects on the score metre.
        /// </summary>
        private readonly Random randomizer = new Random();

        #region Sprites
        private readonly bool HighResApproachCircle;
       
        private readonly pSprite SpriteApproachCircle;
        private readonly pSprite spriteBackground;
        private readonly pSprite SpriteClear;
        private readonly pSprite spriteRpmBackground;
        private readonly pSpriteText spriteRpmText;
        private readonly pSprite spriteScoreMetre;
        private readonly pSprite SpriteSpin;
        protected pSpriteText spriteBonus;
        protected pSprite spriteCircle;

        #endregion

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
        protected double velocityFromInput;

        /// <summary>
        /// Offset to align background with spinner circle.
        /// </summary>
        private int SPINNER_TOP = -GameBase.WindowBaseSize.Height / 40;

        internal Spinner(HitObjectManager hitObjectManager, int startTime, int endTime, HitObjectSoundType soundType)
            : base(hitObjectManager, Vector2.Zero, startTime, soundType, true)
        {
            Position = new Vector2(GameBase.GamefieldBaseSize.Width / 2, GameBase.GamefieldBaseSize.Height / 2);
            EndTime = endTime;
            Type = HitObjectType.Spinner;
            Colour = Color4.Gray;

            Color4 fade = Color4.White;

            //Check for a jpg background for beatmap-based skins (used to reduce filesize), then fallback to png.
            spriteBackground =
                new pSprite(TextureManager.Load("spinner-background.jpg") ?? TextureManager.Load("spinner-background"),
                            FieldTypes.StandardSnapCentre, OriginTypes.Centre , ClockTypes.Audio,
                            new Vector2(0, SPINNER_TOP), SpriteManager.drawOrderFwdLowPrio(StartTime - 1), false, fade);
            SpriteCollection.Add(spriteBackground);

            spriteCircle =
                new pSprite(TextureManager.Load("spinner-circle"),
                            FieldTypes.Standard, OriginTypes.Centre, ClockTypes.Audio,
                            new Vector2(GameBase.WindowBaseSize.Width / 2, (SPINNER_TOP + GameBase.WindowBaseSize.Height) / 2), SpriteManager.drawOrderFwdLowPrio(StartTime), false, fade);
            SpriteCollection.Add(spriteCircle);

            spriteScoreMetre =
                new pSprite(TextureManager.Load("spinner-metre"),
                            FieldTypes.Standard, OriginTypes.TopLeft, ClockTypes.Audio,
                            new Vector2(0, SPINNER_TOP), SpriteManager.drawOrderFwdLowPrio(StartTime + 1), false, fade);
            spriteScoreMetre.DrawHeight = 0;
            SpriteCollection.Add(spriteScoreMetre);

            spriteRpmBackground =
                new pSprite(TextureManager.Load("spinner-rpm"),
                            FieldTypes.StandardSnapBottomCentre, OriginTypes.BottomCentre, ClockTypes.Audio,
                            Vector2.Zero, SpriteManager.drawOrderFwdLowPrio(StartTime + 3), false, fade);
            SpriteCollection.Add(spriteRpmBackground);

            spriteRpmText = new pSpriteText("100", "score", 3,
                                            FieldTypes.StandardSnapBottomCentre, OriginTypes.BottomCentre, ClockTypes.Audio,
                                            Vector2.Zero, SpriteManager.drawOrderFwdLowPrio(StartTime + 4), false, fade);
            spriteRpmText.ScaleScalar = 0.9f;
            SpriteCollection.Add(spriteRpmText);

            pTexture highRes = TextureManager.Load("spinner-approachcircle");

            HighResApproachCircle = highRes != null; // || SkinManager.IsDefault;

            if (HighResApproachCircle)
            {
                SpriteApproachCircle =
                    new pSprite(TextureManager.Load("spinner-approachcircle"),
                                FieldTypes.Standard, OriginTypes.Centre, ClockTypes.Audio,
                                new Vector2(GameBase.WindowBaseSize.Width / 2, (SPINNER_TOP + GameBase.WindowBaseSize.Height) / 2), SpriteManager.drawOrderFwdLowPrio(StartTime + 2), false, fade);
            }
            else
            {
                SpriteApproachCircle =
                    new pSprite(TextureManager.Load("approachcircle"),
                                FieldTypes.Standard, OriginTypes.Centre, ClockTypes.Audio,
                                new Vector2(GameBase.WindowBaseSize.Width / 2, (SPINNER_TOP + GameBase.WindowBaseSize.Height) / 2), SpriteManager.drawOrderFwdLowPrio(StartTime + 2), false, fade);
            }

            SpriteCollection.Add(SpriteApproachCircle);

            spriteBonus = new pSpriteText("", "score", 3, // SkinManager.Current.FontScore, SkinManager.Current.FontScoreOverlap,
                                          FieldTypes.Standard, OriginTypes.Centre, ClockTypes.Audio,
                                          new Vector2(GameBase.WindowBaseSize.Width / 2, (GameBase.WindowBaseSize.Height - SPINNER_TOP) * 3 / 4), SpriteManager.drawOrderFwdLowPrio(StartTime + 3), false, fade);
            SpriteCollection.Add(spriteBonus);

            SpriteSpin =
                new pSprite(TextureManager.Load("spinner-spin"),
                            FieldTypes.Standard, OriginTypes.Centre, ClockTypes.Audio,
                            new Vector2(GameBase.WindowBaseSize.Width / 2, (GameBase.WindowBaseSize.Height + SPINNER_TOP) * 3 / 4), SpriteManager.drawOrderFwdLowPrio(StartTime + 2), false, fade);
            SpriteSpin.Transform(new Transformation(TransformationType.Fade, 0, 1, StartTime - DifficultyManager.FadeIn / 2, StartTime));
            SpriteSpin.Transform(new Transformation(TransformationType.Fade, 1, 0, EndTime - Math.Min(400, endTime - startTime), EndTime));
            SpriteCollection.Add(SpriteSpin);

            foreach (pSprite p in SpriteCollection)
            {
                p.Transformations.Clear();

                p.Transform(new Transformation(TransformationType.Fade, 0, 1, StartTime - DifficultyManager.FadeIn, StartTime));
                p.Transform(new Transformation(TransformationType.Fade, 1, 0, EndTime, EndTime + DifficultyManager.FadeOut));
            }

            SpriteClear =
                new pSprite(TextureManager.Load("spinner-clear"),
                            FieldTypes.Standard, OriginTypes.Centre, ClockTypes.Audio,
                            new Vector2(GameBase.WindowBaseSize.Width / 2, (GameBase.WindowBaseSize.Height + SPINNER_TOP * 3) / 4), SpriteManager.drawOrderFwdLowPrio(StartTime + 3), false, fade);
            SpriteClear.Transform(new Transformation(TransformationType.Fade, 0, 0, startTime, endTime));
            SpriteCollection.Add(SpriteClear);

            if (HighResApproachCircle)
            {
                SpriteApproachCircle.Transform(new Transformation(TransformationType.Scale, 1.86f, 0.1f, StartTime, EndTime));
            }
            else
            {
                SpriteApproachCircle.Transform(new Transformation(TransformationType.Scale, 6, 0.1f, StartTime, EndTime));
            }

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
		
		internal override int HittableEndTime {
			get {
				return EndTime;
			}
		}

        TrackingPoint cursorTrackingPoint;
        Vector2 cursorTrackingPosition;
        internal override ScoreChange CheckScoring()
        {
            //Update the angles
            velocityFromInput = 0;
            
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
			
            Vector2 oldPos = cursorTrackingPosition - spriteCircle.Position;

            //Update to the new mouse position.
            cursorTrackingPosition = cursorTrackingPoint.WindowPosition;

            Vector2 newPos = cursorTrackingPosition - spriteCircle.Position;

            double oldAngle = Math.Atan2(oldPos.Y, oldPos.X);
            double newAngle = Math.Atan2(newPos.Y, newPos.X);

            double angleDiff = newAngle - oldAngle;

            if (angleDiff < -Math.PI)
                angleDiff = (2 * Math.PI) + angleDiff;
            else if (oldAngle - newAngle < -Math.PI)
                angleDiff = (-2 * Math.PI) - angleDiff;

            velocityFromInput = angleDiff / Constants.SIXTY_FRAME_TIME;

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
                        new Transformation(TransformationType.Scale, 1.28F, 2f, Clock.AudioTime, Clock.AudioTime + 800,EasingTypes.In));
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

            Rpm = Rpm * 0.9 + 0.1 * (Math.Abs(velocityCurrent) * Constants.SIXTY_FRAME_TIME * 60) / (Math.PI * 2) * 60;

            spriteRpmText.Text = string.Format("{0:#,0}", Rpm);

            SetScoreMeter((int)((float)scoringRotationCount / rotationRequirement * 100));

            if (IsActive)
            {
                double maxAccelPerSec = AccelerationCap * Constants.SIXTY_FRAME_TIME;
                
                if (velocityFromInput > velocityCurrent)
                {
                    velocityCurrent = velocityCurrent +
                        Math.Min(velocityFromInput - velocityCurrent/4, maxAccelPerSec);
                }
                else
                {
                    velocityCurrent = velocityCurrent +
                        Math.Max(velocityFromInput - velocityCurrent/4, -maxAccelPerSec);
                }

                //hard rate limit
                velocityCurrent = Math.Max(-0.05, Math.Min(velocityCurrent, 0.05));

                spriteCircle.Rotation = spriteCircle.Rotation + (float)(velocityCurrent * Constants.SIXTY_FRAME_TIME);


                if (velocityCurrent != 0)
                    StartSound();
                else
                    StopSound();

                currentRotationCount = (int)(spriteCircle.Rotation / Math.PI);
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

            spriteScoreMetre.DrawTop = (int)(69.2 * (10 - barCount));
            spriteScoreMetre.DrawHeight = (int)(69.2 * (barCount));
            spriteScoreMetre.Position.Y = (float)(SPINNER_TOP + 43.25 * (10 - barCount));
            //spriteScoreMetre.Height = (int)(43.25 * (10 - barCount));
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
