using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using osum.Graphics.Skins;
using osum.Graphics.Sprites;
using osum.Helpers;

namespace osum.GameplayElements
{
    internal class HitCircle : HitObject
    {
        #region General & Timing

        private int comboNumber;
        private const float TEXT_SIZE = 0.8f;

        internal virtual string SpriteNameHitCircle { get { return "hitcircle"; } }

        internal HitCircle(Vector2 startPosition, int startTime, bool newCombo)
            : this(startPosition, startTime, newCombo, false, false, false)
        {
        }

        internal HitCircle(Vector2 startPosition, int startTime, bool newCombo, HitObjectSoundType soundType)
            : this(startPosition, startTime, newCombo, (soundType & HitObjectSoundType.Whistle) > 0, (soundType & HitObjectSoundType.Finish) > 0, (soundType & HitObjectSoundType.Clap) > 0)
        {
        }

        internal HitCircle(Vector2 pos, int startTime, bool newCombo, bool addWhistle, bool addFinish, bool addClap)
        {
            Position = pos;
            StartTime = startTime;
            EndTime = startTime;

            Type = HitObjectType.Normal;
            SoundType = HitObjectSoundType.Normal;

            if (newCombo)
                Type |= HitObjectType.NewCombo;
            if (addWhistle)
                SoundType |= HitObjectSoundType.Whistle;
            if (addFinish)
                SoundType |= HitObjectSoundType.Finish;
            if (addClap)
                SoundType |= HitObjectSoundType.Clap;

            Color4 white = Color4.White;

            SpriteApproachCircle = new pSprite(SkinManager.Load("approachcircle"), FieldType.Gamefield512x384, OriginType.Centre, ClockType.Audio, Position, SpriteManager.drawOrderFwdPrio(StartTime - HitObjectManager.PreEmpt), false, white);
            //if (ShowApproachCircle && (Player.currentScore == null || !ModManager.CheckActive(Player.currentScore.enabledMods, Mods.Hidden)))
            SpriteCollection.Add(SpriteApproachCircle);

            SpriteHitCircle1 =
                new pSprite(SkinManager.Load(SpriteNameHitCircle), FieldType.Gamefield512x384, OriginType.Centre, ClockType.Audio, Position, SpriteManager.drawOrderBwd(StartTime), false, white);
            SpriteCollection.Add(SpriteHitCircle1);
            SpriteHitCircle1.TagNumeric = 1;
            DimCollection.Add(SpriteHitCircle1);


            SpriteHitCircle2 =
                new pAnimation(SkinManager.LoadAll(SpriteNameHitCircle + "overlay"), FieldType.Gamefield512x384,
                            OriginType.Centre, ClockType.Audio, Position,
                            SpriteManager.drawOrderBwd(StartTime - (HitObjectManager.ShowOverlayAboveNumber ? 2 : 1)), false, Color4.White);
            SpriteHitCircle2.frameSkip = 30;
            SpriteCollection.Add(SpriteHitCircle2);
            DimCollection.Add(SpriteHitCircle2);
            SpriteHitCircleText = new pSpriteText(null, SkinManager.Current.FontHitCircle, SkinManager.Current.FontHitCircleOverlap, FieldType.Gamefield512x384, OriginType.Centre,
                                                  ClockType.Audio, Position, SpriteManager.drawOrderBwd(StartTime - (HitObjectManager.ShowOverlayAboveNumber ? 1 : 2)),
                                                  false,
                                                  Color4.White);

            SpriteHitCircleText.CurrentScale = TEXT_SIZE;
            if (ShowCircleText)
            {
                SpriteCollection.Add(SpriteHitCircleText);
                DimCollection.Add(SpriteHitCircleText);
            }

            SpriteApproachCircle.Transform(new Transform(TransformType.Fade, 0, 0.9F, 
                startTime - HitObjectManager.PreEmpt, Math.Min(startTime, startTime - HitObjectManager.PreEmpt + HitObjectManager.FadeIn * 2)));

            SpriteApproachCircle.Transform(new Transform(TransformType.Scale, 4, 1, 
                startTime - HitObjectManager.PreEmpt, startTime));
            
            if (Player.currentScore != null && ModManager.CheckActive(Player.currentScore.enabledMods, Mods.Hidden))
            {
                SpriteHitCircle1.Transform(new Transform(TransformType.Fade, 0, 1, 
                    startTime - HitObjectManager.PreEmpt, startTime - (int)(HitObjectManager.PreEmpt * 0.6)));

                SpriteHitCircle2.Transform(new Transform(TransformType.Fade, 0, 1, 
                    startTime - HitObjectManager.PreEmpt, startTime - (int)(HitObjectManager.PreEmpt * 0.6)));

                SpriteHitCircleText.Transform(new Transform(TransformType.Fade, 0, 1,
                    startTime - HitObjectManager.PreEmpt, startTime - (int)(HitObjectManager.PreEmpt * 0.6)));

                SpriteHitCircle1.Transform(new Transform(TransformType.Fade, 1, 0,
                    startTime - (int)(HitObjectManager.PreEmpt * 0.6), startTime - (int)(HitObjectManager.PreEmpt * 0.3)));

                SpriteHitCircle2.Transform(new Transform(TransformType.Fade, 1, 0,
                    startTime - (int)(HitObjectManager.PreEmpt * 0.6), startTime - (int)(HitObjectManager.PreEmpt * 0.3)));

                SpriteHitCircleText.Transform(new Transform(TransformType.Fade, 1, 0,
                    startTime - (int)(HitObjectManager.PreEmpt * 0.6), startTime - (int)(HitObjectManager.PreEmpt * 0.3)));
            }
            else
            {
                SpriteHitCircle1.Transform(new Transform(TransformType.Fade, 0, 1,
                    startTime - HitObjectManager.PreEmpt, startTime - HitObjectManager.PreEmpt + HitObjectManager.FadeIn));

                SpriteHitCircle2.Transform(new Transform(TransformType.Fade, 0, 1, 
                    startTime - HitObjectManager.PreEmpt, startTime - HitObjectManager.PreEmpt + HitObjectManager.FadeIn));

                SpriteHitCircleText.Transform(new Transform(TransformType.Fade, 0, 1, 
                    startTime - HitObjectManager.PreEmpt, startTime - HitObjectManager.PreEmpt + HitObjectManager.FadeIn));

                SpriteHitCircle1.Transform(new Transform(TransformType.Fade, 1, 0,
                    startTime, startTime + HitObjectManager.hitWindow50));

                SpriteHitCircle2.Transform(new Transform(TransformType.Fade, 1, 0,
                    startTime, startTime + HitObjectManager.hitWindow50));

                SpriteHitCircleText.Transform(new Transform(TransformType.Fade, 1, 0,
                    startTime, startTime + HitObjectManager.hitWindow50));

            }
        }

        protected virtual bool ShowCircleText
        {
            get { return true; }
        }

        protected virtual bool ShowApproachCircle
        {
            get { return true; }
        }

        internal override void SetEndTime(int time)
        {
            throw new Exception();
        }

        internal override HitObject Clone()
        {
            HitCircle h = new HitCircle(Position, StartTime, (Type & HitObjectType.NewCombo) > 0,
                                              (SoundType & HitObjectSoundType.Whistle) > 0,
                                              (SoundType & HitObjectSoundType.Finish) > 0,
                                              (SoundType & HitObjectSoundType.Clap) > 0
                );
            h.SetColour(Colour);
            h.ComboNumber = ComboNumber;
            h.Selected = Selected;

            return h;
        }

        internal override void ModifyPosition(Vector2 newPosition)
        {
            Position = newPosition;

            for (int i = 0; i<SpriteCollection.Count; i++)
                SpriteCollection[i].Position = newPosition;
        }

        internal override void ModifyTime(int newTime)
        {
            int difference = newTime - StartTime;
            StartTime = newTime;
            EndTime = StartTime;
            SpriteApproachCircle.TimeWarp(difference);
            SpriteHitCircle1.TimeWarp(difference);
            SpriteHitCircle2.TimeWarp(difference);
            SpriteHitCircleText.TimeWarp(difference);
            SpriteSelectionCircle.TimeWarp(difference);
        }

        /*
        internal override void Select()
        {
            SpriteSelectionCircle.FadeIn(100);
        }

        internal override void Deselect()
        {
            SpriteSelectionCircle.FadeOut(100);
        }
        */

        internal override void HitAnimation(bool isHit)
        {
            if (isHit)
            {
                //Fade out the actual hit circle
                Transform circleScaleOut = new Transform(TransformType.Scale, 1.1F, 1.9F, 
                    Clock.Time, (int)(Clock.Time + (HitObjectManager.FadeOut * 0.7)), EasingType.In);

                Transform circleScaleOut2 = new Transform(TransformType.Scale, 1.9F, 2F,
                    (int)(Clock.Time + (HitObjectManager.FadeOut * 0.7)), (Clock.Time + HitObjectManager.FadeOut));

                Transform textScaleOut = new Transform(TransformType.Scale, TEXT_SIZE * 1.1F, TEXT_SIZE * 1.9F,
                    Clock.Time, (int)(Clock.Time + (HitObjectManager.FadeOut * 0.7)), EasingType.In);

                Transform textScaleOut2 = new Transform(TransformType.Scale, TEXT_SIZE * 1.9F, TEXT_SIZE * 2F,
                    (int)(Clock.Time + (HitObjectManager.FadeOut * 0.7)), (Clock.Time + HitObjectManager.FadeOut));

                Transform circleFadeOut = new Transform(TransformType.Fade, 1, 0, 
                    Clock.Time, Clock.Time + HitObjectManager.FadeOut);

                //SpriteHitCircle1.Depth = SpriteManager.drawOrderFwd(StartTime + 1);
                SpriteHitCircle1.transformations.Clear();
                SpriteHitCircle1.Clock = ClockType.Game;
                SpriteHitCircle1.Transform(circleScaleOut);
                SpriteHitCircle1.Transform(circleScaleOut2);
                SpriteHitCircle1.Transform(circleFadeOut);

                //SpriteHitCircle2.Depth = SpriteManager.drawOrderFwd(StartTime + 2);
                SpriteHitCircle2.Transformations.Clear();
                SpriteHitCircle2.Clock = ClockType.Game;
                SpriteHitCircle2.Transform(circleScaleOut);
                SpriteHitCircle2.Transform(circleScaleOut2);
                SpriteHitCircle2.Transform(circleFadeOut);

                //SpriteHitCircleText.Depth = SpriteManager.drawOrderFwd(StartTime + 2);
                SpriteHitCircleText.transformations.Clear();
                SpriteHitCircleText.Clock = ClockType.Game;
                SpriteHitCircleText.Transform(textScaleOut);
                SpriteHitCircleText.Transform(textScaleOut2);
                SpriteHitCircleText.Transform(circleFadeOut);

                SpriteApproachCircle.transformations.Clear();
            }
            else
            {
                foreach (pSprite p in SpriteCollection)
                    p.transformations.Clear();
            }
        }

        #endregion

        #region Drawing

        internal pSprite SpriteApproachCircle;
        internal pSprite SpriteHitCircle1;
        internal pAnimation SpriteHitCircle2;
        internal pSprite SpriteHitCircleText;
        internal pSprite SpriteSelectionCircle;

        internal override int ComboNumber
        {
            get { return comboNumber; }
            set
            {
                if (value > 0)
                    SpriteHitCircleText.Text = value.ToString();
                else
                    SpriteHitCircleText.Text = string.Empty;
                //SpriteHitCircleText.OriginPosition = GameBase.HitCircleFont.MeasureString(value.ToString())*0.5F -
                //                                     new Vector2(0, 4);
                comboNumber = value;
            }
        }

        internal override Vector2 EndPosition
        {
            get { return Position; }
            set { }
        }

        internal override bool IsVisible
        {
            get
            {
                return Clock.AudioTime >= StartTime - HitObjectManager.PreEmpt &&
                     Clock.AudioTime <= EndTime + HitObjectManager.FadeOut + HitObjectManager.ForceFadeOut;
            }
        }

        internal override void SetColour(Color4 colour)
        {
            if (colour != Colour)
            {
                SpriteHitCircle1.StartColour = colour;

                SpriteApproachCircle.StartColour = colour;

                /*
                if (GameBase.Mode == OsuModes.Edit)
                {
                    SpriteHitCircle1.Transformations.RemoveAll(
                        delegate(Transformation t) { return (t.Type & TransformationType.Colour) > 0; });
                    SpriteHitCircle1.Transform(
                        new Transformation(colour, Color4.White, StartTime - 5, EndTime - 5));
                }
                */

                Colour = colour;
                ColourDim =
                    new Color4((byte)Math.Max(0, colour.R * 0.75F), (byte)Math.Max(0, colour.G * 0.75F),
                              (byte)Math.Max(0, colour.B * 0.75F), 255);
            }
        }

        #endregion

        internal override IncreaseScoreType GetScorePoints(Vector2 currentMousePos)
        {
            throw new NotImplementedException();
        }
    }

}
