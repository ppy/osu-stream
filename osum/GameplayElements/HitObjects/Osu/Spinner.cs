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
        private readonly Random randomizer = new Random();

        private readonly bool HighResApproachCircle;
        private readonly pSprite SpriteApproachCircle;

        private readonly pSprite spriteBackground;
        private readonly pSprite SpriteClear;
        private readonly pSprite spriteRpmBackground;
        private readonly pSpriteText spriteRpmText;
        private readonly pSprite spriteScoreMetre;
        private readonly pSprite SpriteSpin;
        private Transformation circleRotation;
        private int framecount;
        private double lastMouseAngle;
        private int lastRotationCount;
        private double maxAccel;
        private bool passed;
        internal int rotationCount;
        internal int rotationRequirement;
        private double rpm;
        private int scoringRotationCount;
        private bool spinstarted;
        protected pSprite spriteBonus;
        protected pSprite spriteCircle;
        internal double velocityCurrent;
        protected double velocityTheoretical;
        private int zeroCount;
        private const int SPINNER_CIRCLE_WIDTH = 666;
        private int SPINNER_TOP = 76;

        internal Spinner(HitObjectManager hit_object_manager, int startTime, int endTime, HitObjectSoundType soundType)
            : base(hit_object_manager, Vector2.Zero, startTime, soundType, true)
        {
            Position = new Vector2(GameBase.WindowBaseSize.Width / 2, GameBase.WindowBaseSize.Height / 2);
            StartTime = startTime;
            EndTime = endTime;
            Type = HitObjectType.Spinner;
            SoundType = soundType;
            Colour = Color4.Gray;

            // is this necessary?
            //if (GameBase.GamefieldCorrectionOffsetActive)
            //    SPINNER_TOP -= 16;

            /*
            Color4 fade = (GameBase.Mode == OsuModes.Play &&
                          (ModManager.CheckActive(Player.currentScore.enabledMods, Mods.SpunOut) || Player.Relaxing2)
                              ? Color4.Gray
                              : Color4.White);
            */
            Color4 fade = Color4.White;

            /*
            if (GameBase.Mode == OsuModes.Play && SkinManager.Current.SpinnerFadePlayfield)
            {
                pSprite black = new pSprite(GameBase.WhitePixel, FieldTypes.Standard, OriginTypes.TopLeft,
                ClockTypes.Audio, new Vector2(0, 0), SpriteManager.drawOrderFwdLowPrio(StartTime - 1), false, Color.Black);

                black.CurrentScale = 1.6f;

                black.UseVectorScale = true;
                black.VectorScale = new Vector2(640, SPINNER_TOP);

                SpriteCollection.Add(black);

                if (GameBase.GamefieldCorrectionOffsetActive)
                {
                    black = new pSprite(GameBase.WhitePixel, FieldTypes.Standard, OriginTypes.TopLeft,
    ClockTypes.Audio, new Vector2(0, 480 - 19), SpriteManager.drawOrderFwdLowPrio(StartTime - 1), false, Color.Black);

                    black.CurrentScale = 1.6f;

                    black.UseVectorScale = true;
                    black.VectorScale = new Vector2(640, 19);
                    //Use 19 here instead of 16 as the spinner-background graphic seems a little short...

                    SpriteCollection.Add(black);
                }

            }
            */

            //Check for a jpg background for beatmap-based skins (used to reduce filesize), then fallback to png.
            spriteBackground =
                new pSprite(SkinManager.Load("spinner-background.jpg") ?? SkinManager.Load("spinner-background"),
                            FieldTypes.Standard, OriginTypes.TopLeft, ClockTypes.Audio,
                            new Vector2(0, SPINNER_TOP), SpriteManager.drawOrderFwdLowPrio(StartTime - 1), false, fade);
            SpriteCollection.Add(spriteBackground);

            spriteCircle =
                new pSprite(SkinManager.Load("spinner-circle"),
                            FieldTypes.Standard, OriginTypes.Centre, ClockTypes.Audio,
                            new Vector2(GameBase.WindowBaseSize.Width/2, (SPINNER_TOP + GameBase.WindowBaseSize.Height)/2), SpriteManager.drawOrderFwdLowPrio(StartTime), false, fade);
            SpriteCollection.Add(spriteCircle);

            spriteScoreMetre =
                new pSprite(SkinManager.Load("spinner-metre"),
                            FieldTypes.Standard, OriginTypes.TopLeft, ClockTypes.Audio,
                            new Vector2(0, SPINNER_TOP), SpriteManager.drawOrderFwdLowPrio(StartTime + 1), false, fade);
            spriteScoreMetre.DrawHeight = 0;
            SpriteCollection.Add(spriteScoreMetre);

            // TODO: change these two sprites to calculated positions instead of constants

            spriteRpmBackground =
                new pSprite(SkinManager.Load("spinner-rpm"),
                            FieldTypes.Standard, OriginTypes.TopLeft, ClockTypes.Audio,
                            new Vector2(233, 445), SpriteManager.drawOrderFwdLowPrio(StartTime + 3), false, fade);
            SpriteCollection.Add(spriteRpmBackground);

            spriteRpmText = new pSpriteText("", "score", 3, // SkinManager.Current.FontScore, SkinManager.Current.FontScoreOverlap,
                                            FieldTypes.Standard, OriginTypes.TopRight, ClockTypes.Audio,
                                            new Vector2(400, 448), SpriteManager.drawOrderFwdLowPrio(StartTime + 4), false, fade);
            spriteRpmText.ScaleScalar = 0.9f;
            SpriteCollection.Add(spriteRpmText);

            /*
            if (GameBase.Mode != OsuModes.Edit && spinnerStuff)
            {
                SpriteCollection.Add(spriteRpmText);
                SpriteCollection.Add(spriteRpmBackground);
            }
            */

            pTexture highRes = SkinManager.Load("spinner-approachcircle");

            HighResApproachCircle = highRes != null; // || SkinManager.IsDefault;

            if (HighResApproachCircle)
            {
                SpriteApproachCircle =
                    new pSprite(SkinManager.Load("spinner-approachcircle"),
                                FieldTypes.Standard, OriginTypes.Centre, ClockTypes.Audio,
                                new Vector2(GameBase.WindowBaseSize.Width / 2, (SPINNER_TOP + GameBase.WindowBaseSize.Height) / 2), SpriteManager.drawOrderFwdLowPrio(StartTime + 2), false, fade);
            }
            else
            {
                SpriteApproachCircle =
                    new pSprite(SkinManager.Load("approachcircle"),
                                FieldTypes.Standard, OriginTypes.Centre, ClockTypes.Audio,
                                new Vector2(GameBase.WindowBaseSize.Width / 2, (SPINNER_TOP + GameBase.WindowBaseSize.Height) / 2), SpriteManager.drawOrderFwdLowPrio(StartTime + 2), false, fade); //SkinManager.LoadColour("SpinnerApproachCircle"));
            }

            //if (Player.currentScore == null || !ModManager.CheckActive(Player.currentScore.enabledMods, Mods.Hidden))
            SpriteCollection.Add(SpriteApproachCircle);

            spriteBonus = new pSpriteText("", "score", 3, // SkinManager.Current.FontScore, SkinManager.Current.FontScoreOverlap,
                                          FieldTypes.Standard, OriginTypes.Centre, ClockTypes.Audio,
                                          new Vector2(GameBase.WindowBaseSize.Width / 2, (GameBase.WindowBaseSize.Height - SPINNER_TOP) * 3 / 4), SpriteManager.drawOrderFwdLowPrio(StartTime + 3), false, fade);
            SpriteCollection.Add(spriteBonus);

            UpdateDraw();

                SpriteSpin = 
                    new pSprite(SkinManager.Load("spinner-spin"),
                                FieldTypes.Standard, OriginTypes.Centre, ClockTypes.Audio,
                                new Vector2(GameBase.WindowBaseSize.Width / 2, (GameBase.WindowBaseSize.Height + SPINNER_TOP) * 3 / 4), SpriteManager.drawOrderFwdLowPrio(StartTime + 2), false, fade);
                SpriteSpin.Transform(new Transformation(TransformationType.Fade, 0, 1, StartTime - DifficultyManager.FadeIn / 2, StartTime));
                SpriteSpin.Transform(new Transformation(TransformationType.Fade, 1, 0, EndTime - Math.Min(400, endTime - startTime), EndTime));
                SpriteCollection.Add(SpriteSpin);

                SpriteClear =
                    new pSprite(SkinManager.Load("spinner-clear"),
                                FieldTypes.Standard, OriginTypes.Centre, ClockTypes.Audio,
                                new Vector2(GameBase.WindowBaseSize.Width / 2, (GameBase.WindowBaseSize.Height + SPINNER_TOP * 3) / 4), SpriteManager.drawOrderFwdLowPrio(StartTime + 3), false, fade);
                SpriteClear.Transform(new Transformation(TransformationType.Fade, 0, 0, startTime, endTime));
                SpriteCollection.Add(SpriteClear);
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

        private void UpdateDraw()
        {
            foreach (pSprite p in SpriteCollection)
            {
                p.Transformations.Clear();

                p.Transform(new Transformation(TransformationType.Fade, 0, 1, StartTime - DifficultyManager.FadeIn, StartTime));
                p.Transform(new Transformation(TransformationType.Fade, 1, 0, EndTime, EndTime + DifficultyManager.FadeOut));
            }

            if (HighResApproachCircle)
            {
                SpriteApproachCircle.Transform(new Transformation(TransformationType.Scale, 1.86f, 0.1f, StartTime, EndTime));
            }
            else
            {
                SpriteApproachCircle.Transform(new Transformation(TransformationType.Scale, 6, 0.1f, StartTime, EndTime));
            }

            circleRotation = new Transformation(TransformationType.Rotation, 0, 0, StartTime, EndTime);
            spriteCircle.Transform(circleRotation);

            spriteRpmText.Transform(new Transformation(
                spriteRpmText.Position + new Vector2(0, 50), spriteRpmText.Position,
                StartTime - DifficultyManager.FadeIn, StartTime, EasingTypes.In));
            spriteRpmBackground.Transform(new Transformation(
                spriteRpmBackground.Position + new Vector2(0, 50), spriteRpmBackground.Position,
                StartTime - DifficultyManager.FadeIn, StartTime, EasingTypes.In));

            rotationCount = 0;
            rotationRequirement = (int)((float)(EndTime - StartTime) / 1000 * DifficultyManager.SpinnerRotationRatio);
            maxAccel = 0.00008 + Math.Max(0, (5000 - (double)(EndTime - StartTime)) / 1000 / 2000);
        }

        internal override void Shake()
        {
            return;
        }

#if ARCADE
        const int RELAX_BONUS_ACCEL = 12;
        const int RELAX_BONUS_VELOCITY = 2;
#else
        const int RELAX_BONUS_ACCEL = 4;
        const int RELAX_BONUS_VELOCITY = 1;
#endif

        public override void Update()
        {
            base.Update();

            if (IsHit || Clock.AudioTime < StartTime) // || (!InputManager.ScorableFrame))
                return;

            framecount++;

            //float rpm = (float)scoringRotationCount / (Clock.AudioTime - StartTime) * 60000;
            rpm = rpm * 0.9 + 0.1 * (Math.Abs(velocityCurrent) * Constants.SIXTY_FRAME_TIME * 60) / (Math.PI * 2) * 60;

            spriteRpmText.Text = string.Format("{0:#,0}", rpm);

            SetScoreMeter((int)((float)scoringRotationCount / rotationRequirement * 100));

            /*
            if (GameBase.Mode == OsuModes.Edit)
            {
                circleRotation.EndFloat = (float)(rotationRequirement * Math.PI);
                rotationCount =
                    (int)((float)(Clock.AudioTime - StartTime) / 1000 * HitObjectManager.SpinnerRotationRatio);
            }
            */
            //else 
            if (Clock.AudioTime < EndTime && Clock.AudioTime > StartTime) // && !Player.Recovering)
            {
                if (spriteCircle.Transformations.Contains(circleRotation))
                    spriteCircle.Transformations.Remove(circleRotation);

                double maxAccelPerSec = maxAccel * Constants.SIXTY_FRAME_TIME;
                /*
                if (GameBase.Mode == OsuModes.Play &&
                    ModManager.CheckActive(Player.currentScore.enabledMods, Mods.SpunOut) || Player.Relaxing2)
                    velocityCurrent = 0.03;
                */
                //else
                if (velocityTheoretical > velocityCurrent)
                {
                    velocityCurrent = velocityCurrent +
                        Math.Min(velocityTheoretical * RELAX_BONUS_VELOCITY - velocityCurrent,
                        velocityCurrent < 0
#if !ARCADE
                        //&& Player.Relaxing
#endif
                        ? maxAccelPerSec / RELAX_BONUS_ACCEL : maxAccelPerSec);
                }
                else
                {
                    velocityCurrent = velocityCurrent +
                        Math.Max(velocityTheoretical * RELAX_BONUS_VELOCITY - velocityCurrent,
                        velocityCurrent > 0
#if !ARCADE
                        //&& Player.Relaxing
#endif
                        ? -maxAccelPerSec / RELAX_BONUS_ACCEL : -maxAccelPerSec);
                }


                velocityCurrent = Math.Max(-0.05, Math.Min(velocityCurrent, 0.05));

                spriteCircle.Rotation = spriteCircle.Rotation + (float)(velocityCurrent * Constants.SIXTY_FRAME_TIME);


                if (velocityCurrent != 0)
                    StartSound();
                else
                    StopSound();

                /*
                if (GameBase.Mode == OsuModes.Play &&
                    ModManager.CheckActive(Player.currentScore.enabledMods, Mods.DoubleTime))
                    rotationCount = (int)((spriteCircle.CurrentRotation / Math.PI) * 1.5);
                else if (GameBase.Mode == OsuModes.Play &&
                         ModManager.CheckActive(Player.currentScore.enabledMods, Mods.HalfTime))
                    rotationCount = (int)((spriteCircle.CurrentRotation / Math.PI) * 0.75);
                else
                */
                rotationCount = (int)(spriteCircle.Rotation / Math.PI);
            }

            if (scoringRotationCount >= rotationRequirement && !passed)
            {
                passed = true;
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

            if (scoringRotationCount > 0 && !spinstarted)
            {
                if (SpriteSpin != null)
                {
                    if (Clock.AudioTime > StartTime + 500)
                    {
                        SpriteSpin.FadeOut(300);
                        spinstarted = true;
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

        protected override IncreaseScoreType HitAction()
        {
            StopSound();

            IncreaseScoreType val = IncreaseScoreType.Miss;
            if (scoringRotationCount > rotationRequirement + 1)
                val = IncreaseScoreType.Hit300;
            else if (scoringRotationCount > rotationRequirement)
                val = IncreaseScoreType.Hit100;
            else if (scoringRotationCount > rotationRequirement - 1)
                val = IncreaseScoreType.Hit50;
            if (val > 0)
                PlaySound();
            return val;
        }

        /*
        internal override void Select()
        {
            spriteCircle.OriginalColour = Color4.BlueViolet;
        }

        internal override void Deselect()
        {
            spriteCircle.OriginalColour = Color4.White;
        }

        internal override void ModifyTime(int newTime)
        {
            int diff = newTime - StartTime;
            StartTime += diff;
            SetEndTime(EndTime + diff);
        }

        internal override void ModifyPosition(Vector2 newPosition)
        {
            return;
        }
        */

        // scoring stuff
        //internal override IncreaseScoreType GetScorePoints(Vector2 currentMousePos)
            /*
            if (!InputManager.ScorableFrame)
                return 0;

            Vector2 calc = currentMousePos - spriteCircle.CurrentPositionScaled;
            double newMouseAngle = Math.Atan2(calc.Y, calc.X);

            double angleDiff = newMouseAngle - lastMouseAngle;

            if (newMouseAngle - lastMouseAngle < -Math.PI)
                angleDiff = (2 * Math.PI) + newMouseAngle - lastMouseAngle;
            else if (lastMouseAngle - newMouseAngle < -Math.PI)
                angleDiff = (-2 * Math.PI) - lastMouseAngle + newMouseAngle;

            if (angleDiff == 0)
            {
                if (zeroCount++ < 1)
                    velocityTheoretical = velocityTheoretical / 3;
                else
                    velocityTheoretical = 0;
            }
            else
            {
                zeroCount = 0;

                if (!Player.Relaxing &&
                    (
#if !ARCADE
(InputManager.leftButton == ButtonState.Released && InputManager.rightButton == ButtonState.Released) ||
#endif
 Clock.AudioTime < StartTime ||
                    Clock.AudioTime > EndTime))
                    angleDiff = 0;
                else
                {
                    double pyth = Vector2.Distance(currentMousePos, spriteCircle.CurrentPositionScaled);

                    if (pyth > GameBase.WindowRatioInverse * SPINNER_CIRCLE_WIDTH / 2 && !InputManager.ReplayMode &&
                        !GameBase.graphics.IsFullScreen)
                    {
                        Vector2 mousePos = spriteCircle.CurrentPositionScaled +
                                           calc * (float)((GameBase.WindowRatioInverse * SPINNER_CIRCLE_WIDTH / 2) / pyth);

                        MouseHandler.MousePosition = mousePos;
                        MouseHandler.MousePoint = new Point((int)mousePos.X, (int)mousePos.Y);
                        Mouse.SetPosition((int)mousePos.X, (int)mousePos.Y);
                    }
                }

                if (Math.Abs(angleDiff) < Math.PI && GameBase.SixtyFramesPerSecondLength > 0)
                    velocityTheoretical = angleDiff / GameBase.SIXTY_FRAME_TIME;
                else
                    velocityTheoretical = 0;
            }

            lastMouseAngle = newMouseAngle;

            return GetActualScore();
            */
        /*
        internal IncreaseScoreType GetActualScore()
        {
            IncreaseScoreType score = IncreaseScoreType.Ignore;

            if (rotationCount != lastRotationCount)
            {
                scoringRotationCount++;
                if (SkinManager.Current.SpinnerFrequencyModulate)
                    Bass.BASS_ChannelSetAttribute(AudioEngine.ch_spinnerSpin, BASSAttribute.BASS_ATTRIB_FREQ,
                                                  Math.Min(100000,
                                                           20000 +
                                                           (int)
                                                           (40000 * ((float)scoringRotationCount / rotationRequirement))));

                if (scoringRotationCount > rotationRequirement + 3 &&
                    (scoringRotationCount - (rotationRequirement + 3)) % 2 == 0)
                {
                    score = IncreaseScoreType.SpinnerBonus;
                    AudioEngine.PlaySample(AudioEngine.s_SpinnerBonus, AudioEngine.VolumeSample);
                    spriteBonus.Text = (1000 * (scoringRotationCount - (rotationRequirement + 3)) / 2).ToString();
                    spriteBonus.Transformations.Clear();
                    spriteBonus.Transform(
                        new Transformation(TransformationType.Fade, 1, 0, Clock.AudioTime, Clock.AudioTime + 800));
                    spriteBonus.Transform(
                        new Transformation(TransformationType.Scale, 1.28F, 2f, Clock.AudioTime, Clock.AudioTime + 800));
                    spriteBonus.Transformations[0].Easing = EasingTypes.In;
                    spriteBonus.Transformations[1].Easing = EasingTypes.In;
                    //Ensure we don't recycle this too early.
                    spriteBonus.Transform(
                        new Transformation(TransformationType.Fade, 0, 0, EndTime + 800, EndTime + 800));
                }
                else if (scoringRotationCount > 1 && scoringRotationCount % 2 == 0)
                    score = IncreaseScoreType.SpinnerSpinPoints;
                else if (scoringRotationCount > 1)
                    score = IncreaseScoreType.SpinnerSpin;
            }

            lastRotationCount = rotationCount;

            return score;
        }
        */
    }
}
