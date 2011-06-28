using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.GameModes.SongSelect;
using osum.GameplayElements.Beatmaps;
using osum.GameplayElements;
using osum.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;
using osum.Helpers;
using osum.Graphics.Renderers;
using osum.Support;
using osum.GameplayElements.Scoring;
using osum.GameModes.Play.Components;
using osum.Audio;
using osum.GameplayElements.HitObjects.Osu;
using osum.Graphics.Skins;

namespace osum.GameModes.Play
{
    class Tutorial : Player
    {
        BackButton backButton;
        pText touchToContinueText;

        const int music_offset = 3050;
        const int music_beatlength = 375;

        public override void Initialize()
        {
            Difficulty = Difficulty.None;
            Beatmap = null;

            MainMenu.InitializeBgm();

            base.Initialize();

            touchToContinueText = new pText(osum.Resources.Tutorial.TapToContinue, 30, new Vector2(0, 20), 1, true, Color4.YellowGreen)
            {
                TextBounds = new Vector2(GameBase.BaseSizeFixedWidth.Width * 0.8f, 0),
                Field = FieldTypes.StandardSnapBottomCentre,
                TextShadow = true,
                Bold = true,
                Origin = OriginTypes.BottomCentre
            };

            topMostSpriteManager.Add(touchToContinueText);

            Beatmap = new Beatmap();
            Beatmap.ControlPoints.Add(new ControlPoint(music_offset, music_beatlength, TimeSignatures.SimpleQuadruple, SampleSet.Normal, CustomSampleSet.Default, 100, true, false));

            backButton = new BackButton(delegate { Director.ChangeMode(OsuMode.MainMenu); });
            backButton.Alpha = 0;
            topMostSpriteManager.Add(backButton);

            loadNextSegment();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        TutorialSegments currentSegment;
        TutorialSegments nextSegment = TutorialSegments.Introduction_1;

        VoidDelegate currentSegmentDelegate;

        bool touchToContinue = true;
        private void showTouchToContinue(bool showBackButton = true)
        {
            if (touchToContinue)
                return;

            touchToContinue = true;

            GameBase.Scheduler.Add(delegate
            {
                if (showBackButton)
                    backButton.FadeIn(1000, 0.3f);

                touchToContinueText.Transformations.Clear();
                touchToContinueText.Transform(new Transformation(TransformationType.Fade, 1, 0, Clock.ModeTime + 600, Clock.ModeTime + 1400, EasingTypes.In) { LoopDelay = 600, Looping = true });
            }, 400);
        }

        protected override void InputManager_OnDown(InputSource source, TrackingPoint point)
        {
            if (touchToContinue && !backButton.IsHovering)
            {
                if (touchToContinueText.Transformations.Count > 0)
                    loadNextSegment();
                return;
            }

            base.InputManager_OnDown(source, point);
        }

        private pText showText(string text, float verticalOffset = 0)
        {
            pText pt = new pText(text, 30, new Vector2(0, verticalOffset), new Vector2(GameBase.BaseSize.Width * 0.9f, 0), 1, true, Color4.White, true)
            {
                Field = FieldTypes.StandardSnapCentre,
                TextAlignment = TextAlignment.Centre,
                Origin = OriginTypes.Centre
            };

            pt.ScaleScalar = 1.4f;
            pt.FadeIn(300);
            pt.ScaleTo(1, 400, EasingTypes.In);
            tutorialSegmentManager.Add(pt);

            return pt;
        }

        public override bool Draw()
        {
            base.Draw();

            tutorialSegmentManager.Draw();

            return true;
        }

        int lastFrameBeat;
        int currentBeat;
        public override void Update()
        {
            if (!AudioEngine.Music.IsElapsing && !Failed)
                AudioEngine.Music.Play();

            lastFrameBeat = currentBeat;
            currentBeat = (Clock.AudioTime - music_offset) / music_beatlength;

            if (currentSegmentDelegate != null) currentSegmentDelegate();

            base.Update();

            tutorialSegmentManager.Update();
        }

        protected override void initializeUIElements()
        {
            //base.initializeUIElements();
        }

        enum TutorialSegments
        {
            None,
            Introduction_1,
            Introduction_2,
            Introduction_3,
            Introduction_4,
            HitCircle_1,
            HitCircle_2,
            HitCircle_3,
            HitCircle_4,
            HitCircle_5,
            HitCircle_6,
            HitCircle_Interact,
            HitCircle_Judge,
            Hold_1,
            Hold_2,
            Hold_Interact,
            Hold_Judge,
            Slider_1,
            Slider_2,
            Slider_3,
            Slider_4,
            Slider_Interact,
            Slider_Judge,
            Spinner_1,
            Spinner_2,
            Spinner_3,
            Spinner_4,
            Spinner_Interact,
            Spinner_Judge,
            Multitouch_1,
            Multitouch_2,
            Multitouch_3,
            Multitouch_Interact,
            Multitouch_Judge,
            Stacked_1,
            Stacked_2,
            Stacked_3,
            Stacked_Interact,
            Stacked_Judge,
            Stream_1,
            Stream_2,
            Stream_3,
            Stream_4,
            Stream_5,
            Healthbar_1,
            Healthbar_2,
            Healthbar_3,
            Healthbar_4,
            Healthbar_5,
            Healthbar_End,
            Score_1,
            Score_2,
            Score_3,
            Score_4,
            Outro,
            End,
        }

        SpriteManager tutorialSegmentManager = new SpriteManager();

        private HitObject sampleHitObject;

        private void loadNextSegment()
        {
            loadSegment(nextSegment);
        }

        private void loadSegment(TutorialSegments segment)
        {
            currentSegment = segment;

            nextSegment = (TutorialSegments)(currentSegment + 1);

            foreach (pDrawable p in tutorialSegmentManager.Sprites)
            {
                p.AlwaysDraw = false;
                p.FadeOut(100);
                p.ScaleTo(0.9f, 400, EasingTypes.In);
            }

            tutorialSegmentManager.Sprites.Clear();

            currentSegmentDelegate = null;

            touchToContinueText.Transformations.Clear();
            touchToContinueText.Alpha = 0;

            touchToContinue = false;

            backButton.FadeOut(200);

            switch (currentSegment)
            {
                case TutorialSegments.Introduction_1:
                    showText(osum.Resources.Tutorial.WelcomeToTheWorldOfOsu);
                    showTouchToContinue();
                    break;
                case TutorialSegments.Introduction_2:
                    showText(osum.Resources.Tutorial.Introduction2);
                    showTouchToContinue();
                    break;
                case TutorialSegments.Introduction_3:
                    showText(osum.Resources.Tutorial.Introduction3);
                    showTouchToContinue();
                    break;
                case TutorialSegments.Introduction_4:
                    showText(osum.Resources.Tutorial.Introduction4);
                    showTouchToContinue();
                    break;
                case TutorialSegments.HitCircle_1:
                    resetScore();
                    playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO, false);

                    showText(osum.Resources.Tutorial.HitCircle1);
                    showTouchToContinue();
                    break;
                case TutorialSegments.HitCircle_2:
                    {
                        showText(osum.Resources.Tutorial.HitCircle2, -50);

                        if (HitObjectManager != null) HitObjectManager.Dispose();
                        HitObjectManager = new HitObjectManager(null);

                        sampleHitObject = new HitCircle(HitObjectManager, new Vector2(256, 197), 0, true, 0, HitObjectSoundType.Normal);
                        sampleHitObject.Colour = Color4.OrangeRed;
                        sampleHitObject.ComboNumber = 1;

                        sampleHitObject.Sprites.ForEach(s =>
                        {
                            s.AlwaysDraw = true;
                            s.Transformations.Clear();
                            s.Clocking = ClockTypes.Mode;
                            s.FadeInFromZero(200);
                        });

                        HitCircle c = sampleHitObject as HitCircle;
                        c.SpriteApproachCircle.Alpha = 0;
                        c.SpriteApproachCircle.Transformations.Clear();

                        HitObjectManager.spriteManager.Add(sampleHitObject.Sprites);

                        showTouchToContinue();
                    }
                    break;
                case TutorialSegments.HitCircle_3:
                    {
                        showText(osum.Resources.Tutorial.HitCircle3, -80);

                        HitCircle c = sampleHitObject as HitCircle;
                        c.SpriteHitCircle1.FadeColour(ColourHelper.Darken(c.Colour, 0.3f), 200);
                        c.SpriteApproachCircle.ScaleScalar = 4;
                        c.SpriteApproachCircle.FadeIn(200);

                        showTouchToContinue();
                    }
                    break;
                case TutorialSegments.HitCircle_4:
                    {
                        showText(osum.Resources.Tutorial.HitCircle4, -80);

                        HitCircle c = sampleHitObject as HitCircle;

                        c.SpriteApproachCircle.Alpha = 1;
                        c.SpriteApproachCircle.ScaleTo(1, 4000);
                        c.SpriteHitCircle1.FadeColour(ColourHelper.Lighten(c.Colour, 0.7f), 4000);

                        currentSegmentDelegate = delegate
                        {
                            if (c.SpriteApproachCircle.Transformations.Count == 0)
                            {
                                if (!touchToContinue)
                                {
                                    AudioEngine.PlaySample(OsuSamples.HitNormal, SampleSet.Normal);
                                    showText(osum.Resources.Tutorial.HitCircle4_1, 80).Colour = Color4.Yellow;
                                    showTouchToContinue();
                                }

                                pDrawable lastFlash = null;
                                currentSegmentDelegate = delegate
                                {
                                    if (lastFlash == null || lastFlash.Alpha == 0)
                                        lastFlash = c.SpriteApproachCircle.AdditiveFlash(1000, 1).ScaleTo(1.4f, 1000);
                                };
                            }
                        };
                    }
                    break;
                case TutorialSegments.HitCircle_5:
                    {
                        showText(osum.Resources.Tutorial.HitCircle5, -90);

                        HitCircle c = sampleHitObject as HitCircle;

                        c.SpriteApproachCircle.FadeOut(200);

                        GameBase.Scheduler.Add(delegate
                        {
                            AudioEngine.PlaySample(OsuSamples.HitNormal, SampleSet.Normal);
                            showText(osum.Resources.Tutorial.Good, 80).FadeOut(1000);
                            c.HitAnimation(ScoreChange.Hit50);
                        }, 2000);

                        GameBase.Scheduler.Add(delegate
                        {
                            AudioEngine.PlaySample(OsuSamples.HitNormal, SampleSet.Normal);
                            AudioEngine.PlaySample(OsuSamples.HitWhistle, SampleSet.Normal);
                            showText(osum.Resources.Tutorial.Great, 90).FadeOut(1000);
                            c.HitAnimation(ScoreChange.Hit100);
                        }, 3000);

                        GameBase.Scheduler.Add(delegate
                        {
                            AudioEngine.PlaySample(OsuSamples.HitNormal, SampleSet.Normal);
                            AudioEngine.PlaySample(OsuSamples.HitFinish, SampleSet.Normal);
                            showText(osum.Resources.Tutorial.Perfect, 100).FadeOut(2000);
                            c.HitAnimation(ScoreChange.Hit300);
                        }, 4000);

                        GameBase.Scheduler.Add(delegate
                        {
                            loadNextSegment();
                        }, 6500);
                    }
                    break;
                case TutorialSegments.HitCircle_6:
                    showText(osum.Resources.Tutorial.HitCircle6);
                    showTouchToContinue();
                    break;
                case TutorialSegments.HitCircle_Interact:
                    {
                        prepareInteract();
                        HitObjectManager.OnScoreChanged += new ScoreChangeDelegate(hitObjectManager_OnScoreChanged);

                        const int x1 = 100;
                        const int x2 = 512 - 100;
                        const int y1 = 80;
                        const int y2 = 384 - 80;

                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y1), music_offset + 160 * music_beatlength, true, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x2, y1), music_offset + 164 * music_beatlength, false, 0, HitObjectSoundType.Finish), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y2), music_offset + 168 * music_beatlength, false, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x2, y2), music_offset + 172 * music_beatlength, false, 0, HitObjectSoundType.Finish), Difficulty);

                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x2, y1), music_offset + 176 * music_beatlength, true, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y2), music_offset + 180 * music_beatlength, false, 0, HitObjectSoundType.Finish), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x2, y2), music_offset + 184 * music_beatlength, false, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y1), music_offset + 188 * music_beatlength, false, 0, HitObjectSoundType.Finish), Difficulty);

                        HitObjectManager.PostProcessing();

                        HitObjectManager.SetActiveStream(Difficulty.Easy);

                        currentSegmentDelegate = delegate
                        {
                            if (!touchToContinue && HitObjectManager.AllNotesHit)
                                loadNextSegment();
                        };
                    }
                    break;
                case TutorialSegments.HitCircle_Judge:
                    playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO, false);

                    if (CurrentScore.countMiss > 2)
                    {
                        playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING, false);
                        showText(osum.Resources.Tutorial.HitCircleJudge1);
                        nextSegment = TutorialSegments.HitCircle_1;
                    }
                    else if (CurrentScore.count50 > 1 || CurrentScore.countMiss > 1)
                    {
                        showText(osum.Resources.Tutorial.HitCircleJudge2);
                        nextSegment = TutorialSegments.HitCircle_Interact;
                    }
                    else if (CurrentScore.count100 + CurrentScore.count50 + CurrentScore.countMiss > 0)
                    {
                        showText(osum.Resources.Tutorial.HitCircleJudge3);
                    }
                    else
                    {
                        showText(osum.Resources.Tutorial.HitCircleJudge4);
                    }

                    showTouchToContinue();
                    break;
                case TutorialSegments.Hold_1:
                    {
                        resetScore();
                        playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO, false);
                        Player.Autoplay = true;

                        Clock.ResetManual();
                        Clock.ManualTime = 0;

                        showText(osum.Resources.Tutorial.Hold1, -110);

                        if (HitObjectManager != null) HitObjectManager.Dispose();
                        HitObjectManager = new HitObjectManager(null);

                        sampleHitObject = new HoldCircle(HitObjectManager, new Vector2(256, 197), 1000, true, 0, HitObjectSoundType.Normal, 50, 20, null, 800, 10);
                        //arbitrary

                        sampleHitObject.Clocking = ClockTypes.Manual;

                        sampleHitObject.Colour = Color4.White;
                        sampleHitObject.ComboNumber = 1;

                        HitObjectManager.spriteManager.Add(sampleHitObject.Sprites);

                        bool hasShownText = false;

                        currentSegmentDelegate = delegate
                        {
                            Clock.IncrementManual(0.5f);

                            if (sampleHitObject.IsActive && !hasShownText)
                            {
                                showText(osum.Resources.Tutorial.AndHoldUntilTheCircleExplodes, 100);
                                hasShownText = true;
                            }

                            if (Clock.ManualTime > 3000 && !touchToContinue)
                                showTouchToContinue();

                            sampleHitObject.HitAnimation(sampleHitObject.CheckScoring());
                            sampleHitObject.Update();
                        };

                    }
                    break;
                case TutorialSegments.Hold_2:
                    showText(osum.Resources.Tutorial.Hold2);
                    showTouchToContinue();
                    break;
                case TutorialSegments.Hold_Interact:
                    {
                        prepareInteract();


                        const int x1 = 100;
                        const int x2 = 512 - 100;
                        const int y1 = 80;
                        const int y2 = 384 - 80;

                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x1, y1), music_offset + 160 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1), Difficulty);
                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x2, y2), music_offset + 168 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1), Difficulty);
                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x2, y1), music_offset + 176 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1), Difficulty);
                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x1, y2), music_offset + 184 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1), Difficulty);

                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x1, y1), music_offset + 192 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1), Difficulty);
                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x2, y2), music_offset + 196 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1), Difficulty);
                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x1, y2), music_offset + 200 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1), Difficulty);
                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x2, y1), music_offset + 204 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1), Difficulty);

                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x1, y2), music_offset + 208 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1), Difficulty);
                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x2, y2), music_offset + 212 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1), Difficulty);
                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x1, y1), music_offset + 216 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1), Difficulty);
                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x2, y1), music_offset + 220 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1), Difficulty);

                        HitObjectManager.PostProcessing();

                        HitObjectManager.SetActiveStream(Difficulty.Easy);

                        bool warned = false;

                        currentSegmentDelegate = delegate
                        {
                            if (!touchToContinue && HitObjectManager.AllNotesHit)
                                loadNextSegment();

                            if (Clock.AudioTime > music_offset + 188 * music_beatlength && !warned)
                            {
                                warned = true;
                                pText t = showText(osum.Resources.Tutorial.Hold3);
                                t.Transform(new Transformation(TransformationType.Fade, 1, 0, t.ClockingNow + music_beatlength * 4, t.ClockingNow + music_beatlength * 5));
                            }
                        };
                    }
                    break;
                case TutorialSegments.Hold_Judge:
                    playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO, false);

                    GameBase.Scheduler.Add(delegate
                    {

                        if (CurrentScore.countMiss > 3 || CurrentScore.count50 > 5)
                        {
                            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING, false);
                            showText(osum.Resources.Tutorial.HoldJudge1);
                            nextSegment = TutorialSegments.Hold_1;
                        }
                        else if (CurrentScore.count100 > 0)
                        {
                            showText(osum.Resources.Tutorial.HoldJudge2);
                        }
                        else
                        {
                            showText(osum.Resources.Tutorial.Perfect);
                        }

                        showTouchToContinue();
                    }, 500);
                    break;


                case TutorialSegments.Slider_1:
                    {
                        Clock.ResetManual();
                        Player.Autoplay = true;

                        showText(osum.Resources.Tutorial.Slider1, -80);

                        if (HitObjectManager != null) HitObjectManager.Dispose();
                        HitObjectManager = new HitObjectManager(null);

                        sampleHitObject = new Slider(HitObjectManager, new Vector2(100, 192), 2000, true, 0, HitObjectSoundType.Normal, CurveTypes.Bezier, 0, 300, new List<Vector2>() { new Vector2(100, 192), new Vector2(400, 192) }, null, 200, 40);


                        sampleHitObject.Colour = TextureManager.DefaultColours[0];
                        sampleHitObject.ComboNumber = 1;

                        sampleHitObject.Clocking = ClockTypes.Manual;

                        currentSegmentDelegate = delegate
                        {
                            if (Clock.ManualTime < 1500)
                                Clock.IncrementManual(0.5f);
                            else
                                showTouchToContinue();
                            sampleHitObject.CheckScoring();
                            sampleHitObject.Update();
                        };

                        HitObjectManager.spriteManager.Add(sampleHitObject.Sprites);

                        GameBase.Scheduler.Add(delegate
                        {
                            showText(osum.Resources.Tutorial.Slider1_1, 80);
                        }, 1000);

                    }
                    break;
                case TutorialSegments.Slider_2:
                    {
                        showText(osum.Resources.Tutorial.Slider2, -100);

                        GameBase.Scheduler.Add(delegate
                        {
                            showText(osum.Resources.Tutorial.Slider2_1, 120);
                        }, 1000);

                        currentSegmentDelegate = delegate
                        {
                            if (Clock.ManualTime < sampleHitObject.EndTime + 500)
                                Clock.IncrementManual(0.5f);
                            else
                                showTouchToContinue();

                            sampleHitObject.HitAnimation(sampleHitObject.CheckScoring());
                            sampleHitObject.Update();
                        };

                    }
                    break;
                case TutorialSegments.Slider_3:
                    showText(osum.Resources.Tutorial.Slider3, -80);

                    Clock.ResetManual();

                    if (HitObjectManager != null) HitObjectManager.Dispose();
                    HitObjectManager = new HitObjectManager(null);

                    sampleHitObject = new Slider(HitObjectManager, new Vector2(100, 192), 2000, true, 0, HitObjectSoundType.Normal, CurveTypes.Bezier, 2, 300, new List<Vector2>() { new Vector2(100, 192), new Vector2(400, 192) }, null, 200, 40);


                    sampleHitObject.Colour = TextureManager.DefaultColours[0];
                    sampleHitObject.ComboNumber = 1;

                    sampleHitObject.Clocking = ClockTypes.Manual;

                    pText arrowAtEnd = null;

                    currentSegmentDelegate = delegate
                    {
                        if (Clock.ManualTime < 5800)
                            Clock.IncrementManual(0.5f);
                        else if (!touchToContinue)
                        {
                            showTouchToContinue();
                            arrowAtEnd.FadeOut(50);
                            showText(osum.Resources.Tutorial.Slider3_1, 20).Colour = Color4.SkyBlue;
                        }

                        sampleHitObject.CheckScoring();
                        sampleHitObject.Update();
                    };

                    HitObjectManager.spriteManager.Add(sampleHitObject.Sprites);

                    GameBase.Scheduler.Add(delegate
                    {
                        arrowAtEnd = showText(osum.Resources.Tutorial.Slider3_2, 120);
                    }, 1000);

                    break;
                case TutorialSegments.Slider_4:
                    showText(osum.Resources.Tutorial.Slider4);
                    showTouchToContinue();
                    break;
                case TutorialSegments.Slider_Interact:
                    prepareInteract();

                    HitObjectManager.Add(new Slider(HitObjectManager, new Vector2(50, 92), music_offset + 160 * music_beatlength, true, 0, HitObjectSoundType.Normal, CurveTypes.Bezier, 0, 300, new List<Vector2>() { new Vector2(50, 92), new Vector2(200, 70), new Vector2(350, 92) }, null, 200, 300f / 8), Difficulty.Easy);
                    HitObjectManager.Add(new Slider(HitObjectManager, new Vector2(512 - 50, 384 - 92), music_offset + 168 * music_beatlength, true, 0, HitObjectSoundType.Normal, CurveTypes.Bezier, 0, 300, new List<Vector2>() { new Vector2(512 - 50, 384 - 92), new Vector2(512 - 200, 384 - 70), new Vector2(512 - 350, 384 - 92) }, null, 200, 300f / 8), Difficulty.Easy);
                    HitObjectManager.Add(new Slider(HitObjectManager, new Vector2(50, 50), music_offset + 176 * music_beatlength, true, 0, HitObjectSoundType.Normal, CurveTypes.Bezier, 0, 300, new List<Vector2>() { new Vector2(50, 50), new Vector2(50, 350) }, null, 200, 300f / 8), Difficulty.Easy);
                    HitObjectManager.Add(new Slider(HitObjectManager, new Vector2(512 - 50, 50), music_offset + 184 * music_beatlength, true, 0, HitObjectSoundType.Normal, CurveTypes.Bezier, 2, 300, new List<Vector2>() { new Vector2(512 - 50, 50), new Vector2(512 - 50, 350) }, null, 200, 300f / 8), Difficulty.Easy);

                    HitObjectManager.PostProcessing();
                    HitObjectManager.SetActiveStream(Difficulty.Easy);

                    currentSegmentDelegate = delegate
                    {
                        if (!touchToContinue && HitObjectManager.AllNotesHit)
                            loadNextSegment();
                    };

                    break;
                case TutorialSegments.Slider_Judge:
                    playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO, false);

                    GameBase.Scheduler.Add(delegate
                    {

                        if (CurrentScore.countMiss > 1 || CurrentScore.count50 > 3)
                        {
                            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING, false);
                            showText(osum.Resources.Tutorial.SliderJudge1);
                            nextSegment = TutorialSegments.Slider_1;
                        }
                        else if (CurrentScore.count50 > 2)
                        {
                            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING, false);
                            showText(osum.Resources.Tutorial.SliderJudge2);
                            nextSegment = TutorialSegments.Slider_Interact;
                        }
                        else if (CurrentScore.count100 > 0)
                        {
                            showText(osum.Resources.Tutorial.SliderJudge3);
                        }
                        else
                        {
                            showText(osum.Resources.Tutorial.Perfect);
                        }

                        showTouchToContinue();
                    }, 500);
                    break;



                case TutorialSegments.Spinner_1:
                    resetScore();

                    showText(osum.Resources.Tutorial.Spinner1);
                    showTouchToContinue();
                    break;

                case TutorialSegments.Spinner_2:
                    Player.Autoplay = true;
                    showText(osum.Resources.Tutorial.Spinner2, -140);
                    {
                        Spinner sp = null;

                        GameBase.Scheduler.Add(delegate
                        {

                            if (HitObjectManager != null) HitObjectManager.Dispose();
                            HitObjectManager = new HitObjectManager(null);

                            sampleHitObject = new Spinner(HitObjectManager, 0, 9999999, HitObjectSoundType.Normal);

                            sampleHitObject.Sprites.ForEach(s =>
                            {
                                s.AlwaysDraw = true;
                                s.Transformations.Clear();
                                s.Clocking = ClockTypes.Mode;
                                s.FadeInFromZero(500);
                            });

                            sp = sampleHitObject as Spinner;
                            sp.rotationRequirement = 5 * Spinner.sensitivity_modifier;
                            sp.SpriteClear.Transformations.Clear();
                            sp.SpriteSpin.Transformations.Clear();
                            sp.ApproachCircle.Transformations.Clear();

                            HitObjectManager.spriteManager.Add(sampleHitObject.Sprites);
                        }, 400);

                        GameBase.Scheduler.Add(delegate
                        {
                            showText(osum.Resources.Tutorial.Spinner2_1, 80);

                            GameBase.Scheduler.Add(delegate
                            {

                                currentSegmentDelegate = delegate
                                {
                                    sp.Update();

                                    if (!sp.Cleared)
                                    {
                                        sp.velocityFromInputPerMillisecond = 0.02f;
                                        sp.CheckScoring();
                                    }
                                    else
                                    {
                                        sp.StopSound();
                                        showTouchToContinue();
                                    }



                                };
                            }, 700);
                        }, 800);
                    }
                    break;
                case TutorialSegments.Spinner_3:
                    showText(osum.Resources.Tutorial.Spinner3, -140);
                    {
                        Spinner sp = sampleHitObject as Spinner;

                        currentSegmentDelegate = delegate
                        {
                            if (sp.BonusScore < 1000)
                            {
                                sp.velocityFromInputPerMillisecond = 0.04f;
                                sp.Update();
                                sp.CheckScoring();
                            }
                            else
                            {
                                if (!touchToContinue)
                                {
                                    showText(osum.Resources.Tutorial.Spinner3_1, 80);
                                    sp.StopSound();
                                    showTouchToContinue();
                                }
                            }
                        };
                    }
                    break;
                case TutorialSegments.Spinner_4:
                    sampleHitObject.Sprites.ForEach(s => { s.Transformations.Clear(); s.AlwaysDraw = false; });
                    showText(osum.Resources.Tutorial.Spinner4);
                    showTouchToContinue();
                    break;
                case TutorialSegments.Spinner_Interact:
                    prepareInteract();

                    HitObjectManager.Add(new Spinner(HitObjectManager, music_offset + 160 * music_beatlength, music_offset + 164 * music_beatlength, HitObjectSoundType.Normal), Difficulty);
                    HitObjectManager.Add(new Spinner(HitObjectManager, music_offset + 168 * music_beatlength, music_offset + 172 * music_beatlength, HitObjectSoundType.Normal), Difficulty);
                    HitObjectManager.Add(new Spinner(HitObjectManager, music_offset + 176 * music_beatlength, music_offset + 188 * music_beatlength, HitObjectSoundType.Normal), Difficulty);

                    HitObjectManager.PostProcessing();
                    HitObjectManager.SetActiveStream(Difficulty.Easy);

                    currentSegmentDelegate = delegate
                    {
                        if (!touchToContinue && HitObjectManager.AllNotesHit)
                            loadNextSegment();
                    };

                    break;
                case TutorialSegments.Spinner_Judge:
                    playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO, false);

                    GameBase.Scheduler.Add(delegate
                    {

                        if (CurrentScore.countMiss > 1)
                        {
                            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING, false);
                            showText(osum.Resources.Tutorial.SpinnerJudge1);
                            nextSegment = TutorialSegments.Spinner_Interact;
                        }
                        else if (CurrentScore.count50 > 0 || CurrentScore.count100 > 0 || CurrentScore.countMiss > 0)
                        {
                            showText(osum.Resources.Tutorial.SpinnerJudge2);
                            nextSegment = TutorialSegments.Spinner_Interact;
                        }
                        else
                        {
                            showText(osum.Resources.Tutorial.SpinnerJudge3);
                        }

                        showTouchToContinue();
                    }, 500);
                    break;
                case TutorialSegments.Multitouch_1:
                    {
                        Clock.ResetManual();
                        Player.Autoplay = true;

                        showText(osum.Resources.Tutorial.Multitouch1, -100);

                        if (HitObjectManager != null) HitObjectManager.Dispose();
                        HitObjectManager = new HitObjectManager(Beatmap);

                        sampleHitObject = new HitCircle(HitObjectManager, new Vector2(128, 180), 1500, true, 0, HitObjectSoundType.Normal);
                        sampleHitObject.ComboNumber = 1;
                        sampleHitObject.Clocking = ClockTypes.Manual;

                        HitObjectManager.Add(sampleHitObject, Difficulty.Easy);

                        sampleHitObject = new HitCircle(HitObjectManager, new Vector2(384, 180), 1500, true, 0, HitObjectSoundType.Normal);
                        sampleHitObject.ComboNumber = 1;
                        sampleHitObject.Clocking = ClockTypes.Manual;

                        HitObjectManager.Add(sampleHitObject, Difficulty.Easy);

                        HitObjectManager.PostProcessing();
                        HitObjectManager.SetActiveStream(Difficulty.Easy);

                        currentSegmentDelegate = delegate
                        {
                            if (Clock.ManualTime < 1100)
                                Clock.IncrementManual(0.5f);
                            else if (!touchToContinue)
                            {
                                showText(osum.Resources.Tutorial.Multitouch1_1, 120);
                                showTouchToContinue();
                            }
                        };
                    }
                    break;
                case TutorialSegments.Multitouch_2:
                    {

                        currentSegmentDelegate = delegate
                        {
                            if (Clock.ManualTime < 2000)
                            {
                                Clock.IncrementManual(0.5f);
                            }
                            else if (!touchToContinue)
                            {
                                showText(string.Format(osum.Resources.Tutorial.Multitouch2,GameBase.Instance.PlayersUseThumbs ? osum.Resources.Tutorial.Thumbs : osum.Resources.Tutorial.Fingers), 0);
                                showTouchToContinue();
                            }
                        };
                    }
                    break;
                case TutorialSegments.Multitouch_3:
                    showText(osum.Resources.Tutorial.Multitouch3);
                    showTouchToContinue();
                    break;
                case TutorialSegments.Multitouch_Interact:
                    {
                        prepareInteract();

                        const int x1 = 100;
                        const int x15 = 230;
                        const int x2 = 512 - 100;
                        const int x25 = 512 - 230;
                        const int y1 = 80;
                        const int y2 = 384 - 80;

                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y1), music_offset + 160 * music_beatlength, true, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x2, y1), music_offset + 160 * music_beatlength, false, 0, HitObjectSoundType.Finish), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y2), music_offset + 168 * music_beatlength, false, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x2, y2), music_offset + 168 * music_beatlength, false, 0, HitObjectSoundType.Finish), Difficulty);

                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x2, y1), music_offset + 176 * music_beatlength, true, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y2), music_offset + 176 * music_beatlength, false, 0, HitObjectSoundType.Finish), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x2, y2), music_offset + 184 * music_beatlength, false, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y1), music_offset + 184 * music_beatlength, false, 0, HitObjectSoundType.Finish), Difficulty);

                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y1), music_offset + 192 * music_beatlength, false, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x15, y1), music_offset + 192 * music_beatlength, false, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x2, y2), music_offset + 196 * music_beatlength, false, 0, HitObjectSoundType.Normal), Difficulty);

                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x2, y1), music_offset + 200 * music_beatlength, false, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x25, y1), music_offset + 200 * music_beatlength, false, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y2), music_offset + 204 * music_beatlength, false, 0, HitObjectSoundType.Normal), Difficulty);

                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x2, y2), music_offset + 208 * music_beatlength, false, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x25, y2), music_offset + 208 * music_beatlength, false, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y1), music_offset + 212 * music_beatlength, false, 0, HitObjectSoundType.Normal), Difficulty);

                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(256, 192), music_offset + 216 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1), Difficulty);

                        HitObjectManager.PostProcessing();
                        HitObjectManager.SetActiveStream(Difficulty.Easy);

                        currentSegmentDelegate = delegate
                        {
                            if (!touchToContinue && HitObjectManager.AllNotesHit)
                                loadNextSegment();
                        };
                    }
                    break;
                case TutorialSegments.Multitouch_Judge:
                    playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO, false);

                    GameBase.Scheduler.Add(delegate
                    {

                        if (CurrentScore.countMiss + CurrentScore.count50 > 5)
                        {
                            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING, false);
                            showText(osum.Resources.Tutorial.MultitouchJudge1);
                            nextSegment = TutorialSegments.Multitouch_Interact;
                        }
                        else if (CurrentScore.count100 > 2)
                        {
                            showText(osum.Resources.Tutorial.MultitouchJudge2);
                        }
                        else
                        {
                            showText(osum.Resources.Tutorial.MultitouchJudge3);
                        }

                        showTouchToContinue();
                    }, 800);
                    break;

                case TutorialSegments.Stacked_1:
                    showText(osum.Resources.Tutorial.Stacked1, -100);

                    Clock.ResetManual();
                    Player.Autoplay = true;

                    if (HitObjectManager != null) HitObjectManager.Dispose();
                    HitObjectManager = new HitObjectManager(Beatmap);

                    sampleHitObject = new HitCircle(HitObjectManager, new Vector2(256, 197), 1500, true, 0, HitObjectSoundType.Normal);
                    sampleHitObject.ComboNumber = 1;
                    sampleHitObject.Clocking = ClockTypes.Manual;

                    HitObjectManager.Add(sampleHitObject, Difficulty.Easy);

                    sampleHitObject = new HitCircle(HitObjectManager, new Vector2(256, 197), 2000, false, 0, HitObjectSoundType.Normal);
                    sampleHitObject.ComboNumber = 2;
                    sampleHitObject.Clocking = ClockTypes.Manual;

                    HitObjectManager.Add(sampleHitObject, Difficulty.Easy);

                    HitObjectManager.PostProcessing();
                    HitObjectManager.SetActiveStream(Difficulty.Easy);

                    currentSegmentDelegate = delegate
                    {
                        if (Clock.ManualTime < 1300)
                            Clock.IncrementManual(0.5f);
                        else if (!touchToContinue)
                        {
                            showText(osum.Resources.Tutorial.Stacked1_1, 120);
                            showTouchToContinue();
                        }
                    };
                    break;
                case TutorialSegments.Stacked_2:
                    currentSegmentDelegate = delegate
                    {
                        if (Clock.ManualTime < 2500)
                            Clock.IncrementManual(0.5f);
                        else if (!touchToContinue)
                        {
                            showText(osum.Resources.Tutorial.Stacked2);
                            showTouchToContinue();
                        }
                    };
                    break;
                case TutorialSegments.Stacked_3:
                    int i = 0;
                    showText(osum.Resources.Tutorial.Stacked3);
                    showTouchToContinue();
                    break;
                case TutorialSegments.Stacked_Interact:
                    {
                        prepareInteract();

                        const int x1 = 100;
                        const int x15 = 230;
                        const int x2 = 512 - 100;
                        const int x25 = 512 - 230;
                        const int y1 = 80;
                        const int y2 = 384 - 80;

                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y1), music_offset + 160 * music_beatlength, true, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y1), music_offset + 162 * music_beatlength, false, 0, HitObjectSoundType.Finish), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x2, y1), music_offset + 168 * music_beatlength, false, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x2, y1), music_offset + 170 * music_beatlength, false, 0, HitObjectSoundType.Finish), Difficulty);

                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y2), music_offset + 176 * music_beatlength, true, 0, HitObjectSoundType.Normal), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y2), music_offset + 178 * music_beatlength, false, 0, HitObjectSoundType.Finish), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y2), music_offset + 180 * music_beatlength, false, 0, HitObjectSoundType.Finish), Difficulty);
                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y2), music_offset + 182 * music_beatlength, false, 0, HitObjectSoundType.Finish), Difficulty);
                        HitObjectManager.Add(new Slider(HitObjectManager, new Vector2(x1, y2), music_offset + 184 * music_beatlength, true, 0, HitObjectSoundType.Normal, CurveTypes.Bezier,
                            2, 300, new List<Vector2>() { new Vector2(x1 + (x2 - x1) / 2, y2 - 20), new Vector2(x2, y2) }, null, 200, 300f / 8), Difficulty.Easy);

                        Beatmap.StackLeniency = 2;

                        HitObjectManager.PostProcessing();
                        HitObjectManager.SetActiveStream(Difficulty.Easy);

                        currentSegmentDelegate = delegate
                        {
                            if (!touchToContinue && HitObjectManager.AllNotesHit)
                                loadNextSegment();
                        };
                    }
                    break;
                case TutorialSegments.Stacked_Judge:
                    playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO, false);

                    GameBase.Scheduler.Add(delegate
                    {

                        if (CurrentScore.countMiss > 3 || CurrentScore.count50 > 4)
                        {
                            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING, false);
                            showText(osum.Resources.Tutorial.StackedJudge1);
                            nextSegment = TutorialSegments.Stacked_1;
                        }
                        else if (CurrentScore.count50 > 6)
                        {
                            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING, false);
                            showText(osum.Resources.Tutorial.StackedJudge2);
                            nextSegment = TutorialSegments.Slider_Interact;
                        }
                        else if (CurrentScore.count100 > 0)
                        {
                            showText(osum.Resources.Tutorial.StackedJudge3);
                        }
                        else
                        {
                            showText(osum.Resources.Tutorial.StackedJudge4);
                        }

                        showTouchToContinue();
                    }, 500);
                    break;

                case TutorialSegments.Stream_1:
                    showText(osum.Resources.Tutorial.Stream1);
                    showTouchToContinue();
                    break;
                case TutorialSegments.Stream_2:
                    showText(osum.Resources.Tutorial.Stream2);
                    showTouchToContinue();
                    break;
                case TutorialSegments.Stream_3:
                    showText(osum.Resources.Tutorial.Stream3);
                    showTouchToContinue();
                    break;
                case TutorialSegments.Stream_4:
                    Clock.ResetManual();
                    Player.Autoplay = true;

                    if (HitObjectManager != null) HitObjectManager.Dispose();
                    HitObjectManager = new HitObjectManager(Beatmap);

                    const int vpos = 240;

                    int hpos = 20;

                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos, vpos), 1350, true, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Easy);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos, vpos), 1350, true, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Normal);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos, vpos), 1350, true, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Hard);

                    hpos += 100;

                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos, vpos), 1500, false, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Hard);

                    HitObjectManager.Add(new Slider(HitObjectManager, new Vector2(hpos, vpos), 1550, false, 0, HitObjectSoundType.Normal, CurveTypes.Bezier,
                            2, 300, new List<Vector2>() { new Vector2(hpos + 100, vpos) }, null, 200, 300f / 8) { Clocking = ClockTypes.Manual }, Difficulty.Normal);
                    HitObjectManager.Add(new Slider(HitObjectManager, new Vector2(hpos, vpos), 1550, false, 0, HitObjectSoundType.Normal, CurveTypes.Bezier,
                            2, 300, new List<Vector2>() { new Vector2(hpos + 100, vpos) }, null, 200, 300f / 8) { Clocking = ClockTypes.Manual }, Difficulty.Hard);

                    hpos += 180;

                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos, vpos), 1650, false, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Hard);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos, vpos), 1650, false, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Normal);

                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos - 40, vpos - 40), 1650, false, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Easy);

                    hpos += 100;


                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos, vpos + 60), 1750, true, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Hard);

                    hpos += 100;

                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos, vpos), 1850, false, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Easy);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos, vpos), 1850, false, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Normal);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos, vpos), 1850, false, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Hard);



                    //HitObjectManager.Add(h, Difficulty.Hard);

                    HitObjectManager.PostProcessing();
                    HitObjectManager.SetActiveStream(Difficulty.Easy);
                    playfieldBackground.ChangeColour(Difficulty.Easy, false);

                    pText streamTitle = showText(getDifficultyName(Difficulty.Easy), -90);

                    Difficulty currentStream = Difficulty.Easy;
                    int lastSecond = 0;
                    bool forwards = true;
                    int startTime = 0;

                    const int delay_between_switches = 1200;

                    currentSegmentDelegate = delegate
                    {
                        if (Clock.ManualTime < 1200)
                        {
                            Clock.IncrementManual(1f);
                            startTime = Clock.Time;
                        }
                        else if ((Clock.Time - startTime) / delay_between_switches != lastSecond)
                        {
                            lastSecond = (Clock.Time - startTime) / delay_between_switches;

                            if (forwards)
                                currentStream = (Difficulty)(currentStream + 1);
                            else
                                currentStream = (Difficulty)(currentStream - 1);

                            if (currentStream == Difficulty.Hard || currentStream == GameplayElements.Difficulty.Easy)
                            {
                                forwards = !forwards;
                                showTouchToContinue();
                            }

                            streamTitle.FadeOut(50);
                            streamTitle = showText(currentStream.ToString() + "...", -90);

                            HitObjectManager.ActiveStream = currentStream;
                            playfieldBackground.ChangeColour(currentStream, true);
                        }

                        //else if (!touchToContinue)
                        //{
                        //    showText("Watch for multiple approach circles and tap in time with them.", 120);
                        //    showTouchToContinue();
                        //}
                    };

                    break;
                case TutorialSegments.Stream_5:
                    HitObjectManager.ActiveStream = Difficulty.Normal;
                    playfieldBackground.ChangeColour(Difficulty.Normal, true);

                    foreach (SpriteManager sm in HitObjectManager.streamSpriteManagers)
                        if (sm != null) sm.ScaleTo(0.5f, 500, EasingTypes.InOut).MoveTo(new Vector2(0,150),500,EasingTypes.In);

                    loadNextSegment();
                    break;
                case TutorialSegments.Healthbar_1:
                    showText(osum.Resources.Tutorial.Healthbar1, -120);
                    healthBar = new HealthBar();
                    {
                        pDrawable lastFlash = null;
                        currentSegmentDelegate = delegate
                        {
                            if (lastFlash == null || lastFlash.Alpha == 0)
                                lastFlash = healthBar.s_barBg.AdditiveFlash(1000, 1).ScaleTo(healthBar.s_barBg.ScaleScalar * 1.04f, 1000);
                        };
                    }

                    streamSwitchDisplay = new StreamSwitchDisplay();
                    showTouchToContinue();
                    break;
                case TutorialSegments.Healthbar_2:
                    showText(osum.Resources.Tutorial.Healthbar2, -120);
                    showTouchToContinue();
                    break;
                case TutorialSegments.Healthbar_3:
                    showText(osum.Resources.Tutorial.Healthbar3, -120);

                    playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_STANDARD, false);

                    healthBar.SetCurrentHp(100);
                    {
                        float increaseRate = 0;
                        currentSegmentDelegate = delegate
                        {
                            if (touchToContinue) return;

                            if (healthBar.CurrentHp == 200)
                            {
                                if (increaseRate > 20)
                                {
                                    streamSwitchDisplay.EndSwitch();
                                    HitObjectManager.ActiveStream = Difficulty.Hard;
                                    healthBar.SetCurrentHp(100);
                                    playfieldBackground.ChangeColour(Difficulty.Hard);
                                    showTouchToContinue();
                                }
                                else
                                {
                                    increaseRate += 0.2f;
                                    streamSwitchDisplay.BeginSwitch(true);
                                    playfieldBackground.Move(increaseRate);
                                }
                            }
                            else
                            {
                                healthBar.SetCurrentHp(healthBar.CurrentHp + 1);
                            }
                        };
                    }
                    break;
                case TutorialSegments.Healthbar_4:
                    showText(osum.Resources.Tutorial.Healthbar4, -120);
                    {
                        float increaseRate = 0;
                        currentSegmentDelegate = delegate
                        {
                            if (touchToContinue) return;

                            if (healthBar.CurrentHp == 0)
                            {
                                if (increaseRate > 20)
                                {
                                    streamSwitchDisplay.EndSwitch();
                                    HitObjectManager.ActiveStream = Difficulty.Normal;
                                    healthBar.SetCurrentHp(100);
                                    playfieldBackground.ChangeColour(Difficulty.Normal);

                                    showTouchToContinue();
                                }
                                else
                                {
                                    increaseRate += 0.2f;
                                    streamSwitchDisplay.BeginSwitch(false);
                                    playfieldBackground.Move(-increaseRate);
                                }
                            }
                            else
                            {
                                healthBar.SetCurrentHp(healthBar.CurrentHp - 1);
                            }
                        };
                    }
                    break;
                case TutorialSegments.Healthbar_5:
                    showText(osum.Resources.Tutorial.Healthbar5, -120);

                    HitObjectManager.ActiveStream = Difficulty.Easy;
                    playfieldBackground.ChangeColour(Difficulty.Easy, true);

                    currentSegmentDelegate = delegate
                    {
                        if (playfieldBackground.Velocity == 0)
                            healthBar.SetCurrentHp(healthBar.CurrentHp - 0.5f);
                        if (healthBar.CurrentHp == 0)
                        {

                            if (!touchToContinue)
                            {
                                showTouchToContinue();
                                playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO);
                                showFailSprite();
                                Failed = true;
                                AudioEngine.Music.Pause();
                            }
                        }
                        else if (healthBar.CurrentHp < HealthBar.HP_BAR_MAXIMUM / 3)
                            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING, false);
                    };
                    break;
                case TutorialSegments.Healthbar_End:
                    healthBar.SetCurrentHp(100);
                    hideFailSprite();
                    Failed = false;
                    AudioEngine.Music.Play();
                    healthBar.InitialIncrease = true;
                    currentSegmentDelegate = delegate { if (healthBar.DisplayHp > 20) loadNextSegment(); };
                    break;

                case TutorialSegments.Score_1:
                    scoreDisplay = new ScoreDisplay();
                    {
                        pDrawable lastFlash = null;
                        currentSegmentDelegate = delegate
                        {
                            if (lastFlash == null || lastFlash.Alpha == 0)
                                scoreDisplay.spriteManager.Sprites.ForEach(s => lastFlash = s.AdditiveFlash(1000, 1).ScaleTo(s.ScaleScalar * 1.1f, 1000));
                        };
                    }
                    showText(osum.Resources.Tutorial.Score1);
                    showTouchToContinue();
                    break;
                case TutorialSegments.Score_2:
                    showText(osum.Resources.Tutorial.Score2);
                    showTouchToContinue();
                    break;
                case TutorialSegments.Score_3:
                    showText(osum.Resources.Tutorial.Score3);
                    {
                        GameBase.Scheduler.Add(delegate
                        {
                            comboCounter = new ComboCounter();
                            comboCounter.SetCombo(35);

                            pDrawable lastFlash = null;
                            currentSegmentDelegate = delegate
                            {
                                if (comboCounter.displayCombo == 35)
                                {
                                    if (!touchToContinue)
                                        showTouchToContinue(false);

                                    if (lastFlash == null || lastFlash.Alpha == 0)
                                        comboCounter.spriteManager.Sprites.ForEach(s => lastFlash = s.AdditiveFlash(1000, 1).ScaleTo(s.ScaleScalar * 1.1f, 1000));
                                }
                            };
                        }, 500);
                    }
                    break;
                case TutorialSegments.Score_4:
                    comboCounter.SetCombo(0);
                    showText(osum.Resources.Tutorial.Score4);
                    GameBase.Scheduler.Add(delegate { showTouchToContinue(); }, 1500);
                    break;
            case TutorialSegments.Outro:
                    showText(osum.Resources.Tutorial.Completion);
                    showTouchToContinue(false);
                    break;
                case TutorialSegments.End:
                    backButton.HandleInput = false;
                    Director.ChangeMode(OsuMode.MainMenu, new FadeTransition(3000, FadeTransition.DEFAULT_FADE_IN));
                    break;

            }
        }

        private string getDifficultyName(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.Easy:
                    return osum.Resources.Tutorial.Easy;
                case Difficulty.Normal:
                    return osum.Resources.Tutorial.Normal;
                case Difficulty.Hard:
                    return osum.Resources.Tutorial.Hard;
            }

            return string.Empty;
        }

        private void prepareInteract()
        {
            resetScore();
            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_STANDARD, false);

            Player.Autoplay = false;

            Difficulty = Difficulty.Easy;

            if (countdown == null) countdown = new CountdownDisplay();

            firstCountdown = true;
            AudioEngine.Music.SeekTo(58000);
            CountdownResume(music_offset + 160 * music_beatlength, 8);

            loadBeatmap();
        }

        void hitObjectManager_OnScoreChanged(ScoreChange change, HitObject hitObject)
        {
            switch (change)
            {
                case ScoreChange.Hit300:
                case ScoreChange.Hit300g:
                case ScoreChange.Hit300k:
                case ScoreChange.Hit300m:
                    showText(osum.Resources.Tutorial.Perfect, 0).FadeOut(1000);
                    break;
                case ScoreChange.Hit100:
                case ScoreChange.Hit100m:
                case ScoreChange.Hit100k:
                    if (Clock.AudioTime < hitObject.StartTime)
                        showText("A bit early..", -60).FadeOut(1000);
                    else
                        showText("A bit late..", 60).FadeOut(1000);
                    break;
                case ScoreChange.Hit50:
                case ScoreChange.Hit50m:
                case ScoreChange.Miss:
                    if (Clock.AudioTime < hitObject.StartTime)
                    {
                        pText t = showText("Very early..", -60);
                        t.TextSize *= 1.4f;
                        t.Colour = Color4.OrangeRed;
                        t.FadeOut(1000);
                    }
                    else
                    {
                        pText t = showText("Very late..", 60);
                        t.TextSize *= 1.4f;
                        t.Colour = Color4.OrangeRed;
                        t.FadeOut(1000);
                    }
                    break;
            }
        }

        protected override void UpdateStream()
        {
        }

        protected override bool CheckForCompletion()
        {
            return false;
        }

    }
}
