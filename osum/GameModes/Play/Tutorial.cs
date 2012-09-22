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
using osum.UI;
using osum.Resources;

namespace osum.GameModes.Play
{
    class Tutorial : Player
    {
        BackButton backButton;
        pText touchToContinueText;

        const int music_offset = 0;
        const int music_beatlength = 375;

        public override void Initialize()
        {
            Difficulty = Difficulty.None;
            Beatmap = null;

            MainMenu.InitializeBgm();

            base.Initialize();

            touchToContinueText = new pText(LocalisationManager.GetString(OsuString.TapToContinue), 30, new Vector2(0, 20), 1, true, Color4.Yellow)
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

            s_Demo = new pSprite(TextureManager.Load(OsuTexture.demo), new Vector2(0, 50)) { Alpha = 0, Field = FieldTypes.StandardSnapTopCentre, Origin = OriginTypes.Centre };
            spriteManager.Add(s_Demo);

            loadNextSegment();
        }

        public override void Dispose()
        {
            InputManager.OnDown -= InputManager_OnDown;
            Beatmap = null;

            base.Dispose();
        }

        private void showDemo()
        {
            s_Demo.Transform(new TransformationF(TransformationType.Fade, 1, 1, Clock.ModeTime, Clock.ModeTime + 500) { Looping = true, LoopDelay = 500 });
            s_Demo.Transform(new TransformationF(TransformationType.Fade, 0.5f, 0.5f, Clock.ModeTime + 500, Clock.ModeTime + 1000) { Looping = true, LoopDelay = 500 });
        }

        private void hideDemo()
        {
            s_Demo.Transformations.Clear();
            s_Demo.FadeOut(300);
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
                touchToContinueText.Transformations.Clear();
                touchToContinueText.Transform(new TransformationF(TransformationType.Fade, 1, 1, Clock.ModeTime, Clock.ModeTime + 600, EasingTypes.In) { LoopDelay = 800, Looping = true });
                touchToContinueText.Transform(new TransformationF(TransformationType.Fade, 1, 0, Clock.ModeTime + 600, Clock.ModeTime + 1400, EasingTypes.In) { LoopDelay = 600, Looping = true });
            }, 400);
        }

        protected override void InputManager_OnDown(InputSource source, TrackingPoint point)
        {
            if (point.HoveringObject is BackButton)
                return;

            if (touchToContinue && (backButton == null || !backButton.IsHovering))
            {
                if (touchToContinueText.Transformations.Count > 0)
                    loadNextSegment();
                return;
            }

            base.InputManager_OnDown(source, point);
        }

        private pText showText(string text, float verticalOffset = 0)
        {
            pText pt = new pText(text, 30, new Vector2(0, verticalOffset), new Vector2(GameBase.BaseSizeFixedWidth.Width * 0.9f, 0), 1, true, Color4.White, true)
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

        SpriteManager tutorialSegmentManager = new SpriteManager();

        private HitObject sampleHitObject;
        private pSprite s_Demo;
        private SampleSetInfo drumSampleSetInfo = new SampleSetInfo() { SampleSet = SampleSet.Drum, Volume = 0.8f };

        protected override void resetScore()
        {
            //#if DEBUG
            //            if (GuideFingers == null)
            //            {
            //                //this is just for debugging so guidefingers will be loaded if we start from not the introduction.
            //                GuideFingers = new GuideFinger() { TouchBurster = touchBurster, MovementSpeed = 0.5f };
            //                ShowGuideFingers = true;
            //            }
            //#endif

            base.resetScore();
        }

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

            switch (currentSegment)
            {
                case TutorialSegments.Introduction_1:
                    showText(LocalisationManager.GetString(OsuString.WelcomeToTheWorldOfOsu));

                    GameBase.Scheduler.Add(delegate
                    {
                        backButton = new BackButton(delegate
                        {
                            GameBase.Notify(new Notification(LocalisationManager.GetString(OsuString.Notice), LocalisationManager.GetString(OsuString.ExitTutorial), NotificationStyle.YesNo, delegate(bool yes)
                                {
                                    if (yes)
                                        Director.ChangeMode(OsuMode.MainMenu);
                                }));
                        }, true);
                        backButton.FadeInFromZero(500);
                        topMostSpriteManager.Add(backButton);
                    }, 500);

                    showTouchToContinue();
                    break;
                case TutorialSegments.Introduction_2:
                    showText(LocalisationManager.GetString(OsuString.Introduction2));
                    showTouchToContinue();
                    break;
                case TutorialSegments.Introduction_3:
#if ARCADE
                    loadNextSegment();
                    break;
#endif

                    showText(LocalisationManager.GetString(OsuString.Introduction3));
                    showTouchToContinue();
                    break;
                case TutorialSegments.GuideFingers_1:
                    if (GuideFingers == null)
                    {
                        //this is just for debugging so guidefingers will be loaded if we start from not the introduction.
                        GuideFingers = new GuideFinger() { TouchBurster = touchBurster, MovementSpeed = 0.5f };
                        ShowGuideFingers = true;
                    }

                    showText(LocalisationManager.GetString(OsuString.MeetTheTwoFingerGuides), -80);

                    GameBase.Scheduler.Add(delegate
                    {
                        showTouchToContinue();
                    }, 1000);

                    int elapsed = Clock.Time;
                    currentSegmentDelegate = delegate
                    {
                        if (Clock.Time - elapsed > 1000)
                        {
                            GuideFingers.leftFinger.AdditiveFlash(500, 1f).ScaleTo(2f, 500);
                            GuideFingers.rightFinger.AdditiveFlash(500, 1f).ScaleTo(2f, 500);

                            GuideFingers.leftFinger2.AdditiveFlash(500, 1f).ScaleTo(2f, 500);
                            GuideFingers.rightFinger2.AdditiveFlash(500, 1f).ScaleTo(2f, 500);
                            elapsed = Clock.Time;
                        }
                    };

                    break;
                case TutorialSegments.Introduction_4:
                    showText(LocalisationManager.GetString(OsuString.Introduction4));
                    showTouchToContinue();
                    break;
                case TutorialSegments.HitCircle_1:
                    resetScore();
                    Clock.ResetManual();
                    Autoplay = true;

                    playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO, false);
                    showText(LocalisationManager.GetString(OsuString.HitCircle1));
                    showTouchToContinue();
                    break;
                case TutorialSegments.HitCircle_2:
                    {
                        showText(LocalisationManager.GetString(OsuString.HitCircle2), -50);

                        showDemo();
                        if (HitObjectManager != null) HitObjectManager.Dispose();
                        HitObjectManager = new HitObjectManager(Beatmap);

                        sampleHitObject = new HitCircle(HitObjectManager, new Vector2(256, 197), 700, true, 0, HitObjectSoundType.Normal);
                        sampleHitObject.Clocking = ClockTypes.Manual;

                        sampleHitObject.ComboOffset = 2;

                        HitCircle c = sampleHitObject as HitCircle;
                        c.SpriteApproachCircle.Bypass = true;

                        HitObjectManager.Add(c, Difficulty.Easy);
                        HitObjectManager.PostProcessing();
                        HitObjectManager.SetActiveStream(Difficulty.Easy);

                        showTouchToContinue();
                    }
                    break;
                case TutorialSegments.HitCircle_3:
                    {
                        showText(LocalisationManager.GetString(OsuString.HitCircle3), -80);

                        HitCircle c = sampleHitObject as HitCircle;
                        c.SpriteApproachCircle.Bypass = false;

                        showTouchToContinue();
                    }
                    break;
                case TutorialSegments.HitCircle_4:
                    {
                        showText(LocalisationManager.GetString(OsuString.HitCircle4), -70);

                        bool textShown = false;

                        currentSegmentDelegate = delegate
                        {
                            Clock.IncrementManual(0.3f);

                            if (Clock.ManualTime > 700 && !textShown)
                            {
                                textShown = true;
                                showText(LocalisationManager.GetString(OsuString.HitCircle4_1), 20).Colour = Color4.GreenYellow;
                            }

                            if (Clock.ManualTime > 1300)
                            {
                                if (!touchToContinue)
                                    showTouchToContinue();
                            }
                        };
                    }
                    break;
                case TutorialSegments.HitCircle_5:
                    {
                        showText(LocalisationManager.GetString(OsuString.HitCircle5), -90);

                        HitCircle c = sampleHitObject as HitCircle;

                        c.SpriteApproachCircle.FadeOut(200);

                        GameBase.Scheduler.Add(delegate
                        {
                            AudioEngine.PlaySample(OsuSamples.HitNormal, SampleSet.Normal);
                            showText(LocalisationManager.GetString(OsuString.Good), 80).FadeOut(1000);
                            c.HitAnimation(ScoreChange.Hit50);
                        }, 2000);

                        GameBase.Scheduler.Add(delegate
                        {
                            AudioEngine.PlaySample(OsuSamples.HitNormal, SampleSet.Normal);
                            AudioEngine.PlaySample(OsuSamples.HitWhistle, SampleSet.Normal);
                            showText(LocalisationManager.GetString(OsuString.Great), 90).FadeOut(1000);
                            c.HitAnimation(ScoreChange.Hit100);
                        }, 3000);

                        GameBase.Scheduler.Add(delegate
                        {
                            AudioEngine.PlaySample(OsuSamples.HitNormal, SampleSet.Normal);
                            AudioEngine.PlaySample(OsuSamples.HitFinish, SampleSet.Normal);
                            showText(LocalisationManager.GetString(OsuString.Perfect), 100).FadeOut(2000);
                            c.HitAnimation(ScoreChange.Hit300);
                        }, 4000);

                        GameBase.Scheduler.Add(delegate
                        {
                            loadNextSegment();
                        }, 6500);
                    }
                    break;
                case TutorialSegments.HitCircle_6:
                    hideDemo();
                    showText(LocalisationManager.GetString(OsuString.HitCircle6));
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
                    judge();

                    if (CurrentScore.countMiss > 2)
                    {
                        playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING, false);
                        showText(LocalisationManager.GetString(OsuString.HitCircleJudge1));
                        nextSegment = TutorialSegments.HitCircle_1;
                    }
                    else if (CurrentScore.count50 > 1 || CurrentScore.countMiss > 1)
                    {
                        showText(LocalisationManager.GetString(OsuString.HitCircleJudge2));
                        nextSegment = TutorialSegments.HitCircle_Interact;
                    }
                    else if (CurrentScore.count100 + CurrentScore.count50 + CurrentScore.countMiss > 0)
                    {
                        showText(LocalisationManager.GetString(OsuString.HitCircleJudge3));
                    }
                    else
                    {
                        showText(LocalisationManager.GetString(OsuString.HitCircleJudge4));
                    }

                    showTouchToContinue();
                    break;
                case TutorialSegments.Hold_1:
                    {
                        resetScore();
                        playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO, false);
                        Player.Autoplay = true;
                        Clock.ResetManual();

                        showText(LocalisationManager.GetString(OsuString.Hold1), -90);

                        showDemo();
                        if (HitObjectManager != null) HitObjectManager.Dispose();
                        HitObjectManager = new HitObjectManager(Beatmap);

                        sampleHitObject = new HoldCircle(HitObjectManager, new Vector2(256, 197), 1000, true, 0, HitObjectSoundType.Normal, 50, 20, null, 800, 10);
                        sampleHitObject.Clocking = ClockTypes.Manual;
                        sampleHitObject.SampleSet = new SampleSetInfo() { SampleSet = SampleSet.Drum, Volume = 0.8f };

                        HitObjectManager.Add(sampleHitObject, Difficulty.Easy);
                        HitObjectManager.PostProcessing();
                        HitObjectManager.SetActiveStream(Difficulty.Easy);

                        bool hasShownText = false;

                        currentSegmentDelegate = delegate
                        {
                            Clock.IncrementManual(0.5f);

                            if (sampleHitObject.IsActive && !hasShownText)
                            {
                                showText(LocalisationManager.GetString(OsuString.AndHoldUntilTheCircleExplodes), 100);
                                hasShownText = true;
                            }

                            if (Clock.ManualTime > 2700 && !touchToContinue)
                                showTouchToContinue();

                            sampleHitObject.HitAnimation(sampleHitObject.CheckScoring());
                            sampleHitObject.Update();
                        };

                    }
                    break;
                case TutorialSegments.Hold_2:
                    hideDemo();
                    showText(LocalisationManager.GetString(OsuString.Hold2));
                    showTouchToContinue();
                    break;
                case TutorialSegments.Hold_Interact:
                    {
                        prepareInteract();


                        const int x1 = 100;
                        const int x2 = 512 - 100;
                        const int y1 = 80;
                        const int y2 = 384 - 80;

                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x1, y1), music_offset + 160 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1) { SampleSet = new SampleSetInfo() { SampleSet = SampleSet.Drum, Volume = 0.8f } }, Difficulty);
                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x2, y2), music_offset + 168 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1) { SampleSet = new SampleSetInfo() { SampleSet = SampleSet.Drum, Volume = 0.8f } }, Difficulty);
                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x2, y1), music_offset + 176 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1) { SampleSet = new SampleSetInfo() { SampleSet = SampleSet.Drum, Volume = 0.8f } }, Difficulty);
                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x1, y2), music_offset + 184 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1) { SampleSet = new SampleSetInfo() { SampleSet = SampleSet.Drum, Volume = 0.8f } }, Difficulty);

                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x1, y1), music_offset + 192 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1) { SampleSet = new SampleSetInfo() { SampleSet = SampleSet.Drum, Volume = 0.8f } }, Difficulty);
                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x2, y2), music_offset + 196 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1) { SampleSet = new SampleSetInfo() { SampleSet = SampleSet.Drum, Volume = 0.8f } }, Difficulty);
                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x1, y2), music_offset + 200 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1) { SampleSet = new SampleSetInfo() { SampleSet = SampleSet.Drum, Volume = 0.8f } }, Difficulty);
                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x2, y1), music_offset + 204 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1) { SampleSet = new SampleSetInfo() { SampleSet = SampleSet.Drum, Volume = 0.8f } }, Difficulty);

                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x1, y1), music_offset + 208 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1) { SampleSet = new SampleSetInfo() { SampleSet = SampleSet.Drum, Volume = 0.8f } }, Difficulty);
                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x2, y1), music_offset + 212 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1) { SampleSet = new SampleSetInfo() { SampleSet = SampleSet.Drum, Volume = 0.8f } }, Difficulty);
                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x1, y2), music_offset + 216 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1) { SampleSet = new SampleSetInfo() { SampleSet = SampleSet.Drum, Volume = 0.8f } }, Difficulty);
                        HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(x2, y2), music_offset + 220 * music_beatlength, true, 0, HitObjectSoundType.Normal, (4 * music_beatlength) / 8f / 1000f, 8, null, 1, 1) { SampleSet = new SampleSetInfo() { SampleSet = SampleSet.Drum, Volume = 0.8f } }, Difficulty);

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
                                pText t = showText(LocalisationManager.GetString(OsuString.Hold3), 30);
                                t.Transform(new TransformationF(TransformationType.Fade, 1, 0, t.ClockingNow + music_beatlength * 4, t.ClockingNow + music_beatlength * 5));
                            }
                        };
                    }
                    break;
                case TutorialSegments.Hold_Judge:
                    judge();

                    GameBase.Scheduler.Add(delegate
                    {

                        if (CurrentScore.countMiss > 3 || CurrentScore.count50 > 5)
                        {
                            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING, false);
                            showText(LocalisationManager.GetString(OsuString.HoldJudge1));
                            nextSegment = TutorialSegments.Hold_1;
                        }
                        else if (CurrentScore.count100 + CurrentScore.count50 + CurrentScore.countMiss > 1)
                        {
                            showText(LocalisationManager.GetString(OsuString.HoldJudge2));
                        }
                        else
                        {
                            showText(LocalisationManager.GetString(OsuString.Perfect));
                        }

                        showTouchToContinue();
                    }, 500);
                    break;
                case TutorialSegments.Slider_1:
                    {
                        resetScore();
                        playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO, false);
                        Clock.ResetManual();
                        Player.Autoplay = true;

                        showText(LocalisationManager.GetString(OsuString.Slider1), -80);

                        showDemo();
                        if (HitObjectManager != null) HitObjectManager.Dispose();
                        HitObjectManager = new HitObjectManager(Beatmap);

                        sampleHitObject = new Slider(HitObjectManager, new Vector2(100, 192), 2000, true, 0, HitObjectSoundType.Normal, CurveTypes.Bezier, 0, 300, new List<Vector2>() { new Vector2(100, 192), new Vector2(400, 192) }, null, 200, 40);
                        sampleHitObject.Clocking = ClockTypes.Manual;

                        currentSegmentDelegate = delegate
                        {
                            if (Clock.ManualTime < 1550)
                                Clock.IncrementManual(0.5f);
                            else
                                showTouchToContinue();
                        };

                        HitObjectManager.Add(sampleHitObject, Difficulty.Easy);

                        HitObjectManager.PostProcessing();
                        HitObjectManager.SetActiveStream(Difficulty.Easy);

                        GameBase.Scheduler.Add(delegate
                        {
                            showText(LocalisationManager.GetString(OsuString.Slider1_1), 80);
                        }, 1000);

                    }
                    break;
                case TutorialSegments.Slider_2:
                    {
                        showText(LocalisationManager.GetString(OsuString.Slider2), -100);

                        GameBase.Scheduler.Add(delegate
                        {
                            showText(LocalisationManager.GetString(OsuString.Slider2_1), 120);
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
                    showText(LocalisationManager.GetString(OsuString.Slider3), -100);

                    Clock.ResetManual();
                    Player.Autoplay = true;

                    if (HitObjectManager != null) HitObjectManager.Dispose();
                    HitObjectManager = new HitObjectManager(Beatmap);

                    sampleHitObject = new Slider(HitObjectManager, new Vector2(100, 192), 2000, true, 0, HitObjectSoundType.Normal, CurveTypes.Bezier, 2, 300, new List<Vector2>() { new Vector2(100, 192), new Vector2(400, 192) }, null, 200, 40);
                    sampleHitObject.Clocking = ClockTypes.Manual;

                    HitObjectManager.Add(sampleHitObject, Difficulty.Easy);

                    HitObjectManager.PostProcessing();
                    HitObjectManager.SetActiveStream(Difficulty.Easy);


                    pText arrowAtEnd = null;

                    currentSegmentDelegate = delegate
                    {
                        if (Clock.ManualTime < 5800)
                            Clock.IncrementManual(0.5f);
                        else if (!touchToContinue)
                        {
                            showTouchToContinue();
                            arrowAtEnd.FadeOut(50);
                            showText(LocalisationManager.GetString(OsuString.Slider3_1)).Colour = Color4.SkyBlue;
                        }
                    };

                    GameBase.Scheduler.Add(delegate
                    {
                        arrowAtEnd = showText(LocalisationManager.GetString(OsuString.Slider3_2), 120);
                    }, 1000);

                    break;
                case TutorialSegments.Slider_4:
                    hideDemo();
                    showText(LocalisationManager.GetString(OsuString.Slider4));
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
                    judge();

                    GameBase.Scheduler.Add(delegate
                    {

                        if (CurrentScore.countMiss > 1 || CurrentScore.count50 > 3)
                        {
                            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING, false);
                            showText(LocalisationManager.GetString(OsuString.SliderJudge1));
                            nextSegment = TutorialSegments.Slider_1;
                        }
                        else if (CurrentScore.count50 > 2)
                        {
                            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING, false);
                            showText(LocalisationManager.GetString(OsuString.SliderJudge2));
                            nextSegment = TutorialSegments.Slider_Interact;
                        }
                        else if (CurrentScore.count100 + CurrentScore.count50 + CurrentScore.countMiss > 2)
                        {
                            showText(LocalisationManager.GetString(OsuString.SliderJudge3));
                        }
                        else
                        {
                            showText(LocalisationManager.GetString(OsuString.Perfect));
                        }

                        showTouchToContinue();
                    }, 500);
                    break;

                case TutorialSegments.Spinner_1:
                    playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO, false);
                    resetScore();

                    showText(LocalisationManager.GetString(OsuString.Spinner1));
                    showTouchToContinue();
                    break;

                case TutorialSegments.Spinner_2:
                    {
                        showText(LocalisationManager.GetString(OsuString.Spinner2), -140);

                        Clock.ResetManual();
                        Player.Autoplay = true;

                        showDemo();
                        if (HitObjectManager != null) HitObjectManager.Dispose();
                        HitObjectManager = new HitObjectManager(Beatmap);

                        sampleHitObject = new Spinner(HitObjectManager, 800, 4000, HitObjectSoundType.Normal);
                        sampleHitObject.Clocking = ClockTypes.Manual;

                        HitObjectManager.Add(sampleHitObject, Difficulty.Easy);
                        HitObjectManager.PostProcessing();
                        HitObjectManager.SetActiveStream(Difficulty.Easy);

                        currentSegmentDelegate = delegate
                        {
                            if (Clock.ManualTime < 2500)
                                Clock.IncrementManual(0.5f);
                            else
                            {
                                showTouchToContinue();
                            }
                        };

                        GameBase.Scheduler.Add(delegate
                        {
                            showText(LocalisationManager.GetString(OsuString.Spinner2_1), 80);
                        }, 800);
                    }
                    break;
                case TutorialSegments.Spinner_3:
                    showText(LocalisationManager.GetString(OsuString.Spinner3), -140);
                    {
                        currentSegmentDelegate = delegate
                        {
                            if (Clock.ManualTime < 4600)
                                Clock.IncrementManual(0.5f);
                            else
                            {
                                loadNextSegment();
                            }
                        };
                    }
                    break;
                case TutorialSegments.Spinner_4:
                    hideDemo();
                    showText(LocalisationManager.GetString(OsuString.Spinner4));
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
                    judge();

                    GameBase.Scheduler.Add(delegate
                    {

                        if (CurrentScore.countMiss > 1)
                        {
                            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING, false);
                            showText(LocalisationManager.GetString(OsuString.SpinnerJudge1));
                            nextSegment = TutorialSegments.Spinner_Interact;
                        }
                        else if (CurrentScore.count50 > 1 || CurrentScore.count100 > 1)
                        {
                            showText(LocalisationManager.GetString(OsuString.SpinnerJudge2));
                            nextSegment = TutorialSegments.Spinner_Interact;
                        }
                        else
                        {
                            showText(LocalisationManager.GetString(OsuString.SpinnerJudge3));
                        }

                        showTouchToContinue();
                    }, 500);
                    break;
                case TutorialSegments.Multitouch_1:
                    {
                        playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO, false);
                        Clock.ResetManual();
                        Player.Autoplay = true;

                        showText(LocalisationManager.GetString(OsuString.Multitouch1), -100);

                        showDemo();
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
                            if (Clock.ManualTime < 1000)
                                Clock.IncrementManual(0.5f);
                            else if (!touchToContinue)
                            {
                                showText(LocalisationManager.GetString(OsuString.Multitouch1_1), 120);
                                showTouchToContinue();
                            }
                        };
                    }
                    break;
                case TutorialSegments.Multitouch_2:
#if ARCADE
                    loadNextSegment();
                    break;
#endif

                    {

                        currentSegmentDelegate = delegate
                        {
                            if (Clock.ManualTime < 2000)
                            {
                                Clock.IncrementManual(0.5f);
                            }
                            else if (!touchToContinue)
                            {
                                showText(string.Format(LocalisationManager.GetString(OsuString.Multitouch2), GameBase.IsHandheld ? LocalisationManager.GetString(OsuString.Thumbs) : LocalisationManager.GetString(OsuString.Fingers)), 0);
                                showTouchToContinue();
                            }
                        };
                    }
                    break;
                case TutorialSegments.Multitouch_3:
                    hideDemo();
                    showText(LocalisationManager.GetString(OsuString.Multitouch3));
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

                        HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(x1, y1), music_offset + 192 * music_beatlength, true, 0, HitObjectSoundType.Normal), Difficulty);
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
                    judge();

                    GameBase.Scheduler.Add(delegate
                    {

                        if (CurrentScore.countMiss + CurrentScore.count50 > 5)
                        {
                            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING, false);
                            showText(LocalisationManager.GetString(OsuString.MultitouchJudge1));
                            nextSegment = TutorialSegments.Multitouch_Interact;
                        }
                        else if (CurrentScore.count100 + CurrentScore.count50 + CurrentScore.countMiss > 2)
                        {
                            showText(LocalisationManager.GetString(OsuString.MultitouchJudge2));
                        }
                        else
                        {
                            showText(LocalisationManager.GetString(OsuString.MultitouchJudge3));
                        }

                        showTouchToContinue();
                    }, 800);
                    break;

                case TutorialSegments.Stacked_1:
                    playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO, false);
                    showText(LocalisationManager.GetString(OsuString.Stacked1), -100);

                    Clock.ResetManual();
                    Player.Autoplay = true;

                    showDemo();
                    if (HitObjectManager != null) HitObjectManager.Dispose();
                    HitObjectManager = new HitObjectManager(Beatmap);

                    sampleHitObject = new HitCircle(HitObjectManager, new Vector2(256, 197), 1500, true, 0, HitObjectSoundType.Normal);
                    sampleHitObject.ComboNumber = 1;
                    sampleHitObject.Clocking = ClockTypes.Manual;

                    HitObjectManager.Add(sampleHitObject, Difficulty.Easy);

                    sampleHitObject = new HitCircle(HitObjectManager, new Vector2(256, 197), 1800, false, 0, HitObjectSoundType.Normal);
                    sampleHitObject.ComboNumber = 2;
                    sampleHitObject.Clocking = ClockTypes.Manual;

                    HitObjectManager.Add(sampleHitObject, Difficulty.Easy);

                    HitObjectManager.PostProcessing();
                    HitObjectManager.SetActiveStream(Difficulty.Easy);

                    currentSegmentDelegate = delegate
                    {
                        if (Clock.ManualTime < 1000)
                            Clock.IncrementManual(0.5f);
                        else if (!touchToContinue)
                        {
                            showText(LocalisationManager.GetString(OsuString.Stacked1_1), 120);
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
                            showText(LocalisationManager.GetString(OsuString.Stacked2));
                            showTouchToContinue();
                        }
                    };
                    break;
                case TutorialSegments.Stacked_3:
                    hideDemo();
                    showText(LocalisationManager.GetString(OsuString.Stacked3));
                    showTouchToContinue();
                    break;
                case TutorialSegments.Stacked_Interact:
                    {
                        prepareInteract();

                        const int x1 = 100;
                        const int x2 = 512 - 100;
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
                    judge();

                    GameBase.Scheduler.Add(delegate
                    {

                        if (CurrentScore.countMiss > 3 || CurrentScore.count50 > 4)
                        {
                            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING, false);
                            showText(LocalisationManager.GetString(OsuString.StackedJudge1));
                            nextSegment = TutorialSegments.Stacked_1;
                        }
                        else if (CurrentScore.count50 > 6)
                        {
                            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING, false);
                            showText(LocalisationManager.GetString(OsuString.StackedJudge2));
                            nextSegment = TutorialSegments.Slider_Interact;
                        }
                        else if (CurrentScore.count100 + CurrentScore.count50 + CurrentScore.countMiss > 0)
                        {
                            showText(LocalisationManager.GetString(OsuString.StackedJudge3));
                        }
                        else
                        {
                            showText(LocalisationManager.GetString(OsuString.StackedJudge4));
                        }

                        showTouchToContinue();
                    }, 500);
                    break;

                case TutorialSegments.Stream_1:
                    ShowGuideFingers = false;
                    showText(LocalisationManager.GetString(OsuString.Stream1));
                    showTouchToContinue();
                    break;
                case TutorialSegments.Stream_2:
                    showText(LocalisationManager.GetString(OsuString.Stream2));
                    showTouchToContinue();
                    break;
                case TutorialSegments.Stream_3:
                    showText(LocalisationManager.GetString(OsuString.Stream3));
                    showTouchToContinue();
                    break;
                case TutorialSegments.Stream_4:
                    Clock.ManualTime = 1200;
                    Player.Autoplay = true;

                    showDemo();
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
                            2, 100, new List<Vector2>() { new Vector2(hpos + 100, vpos) }, null, 200, 300f / 8) { Clocking = ClockTypes.Manual }, Difficulty.Normal);
                    HitObjectManager.Add(new Slider(HitObjectManager, new Vector2(hpos, vpos), 1550, false, 0, HitObjectSoundType.Normal, CurveTypes.Bezier,
                            2, 100, new List<Vector2>() { new Vector2(hpos + 100, vpos) }, null, 200, 300f / 8) { Clocking = ClockTypes.Manual }, Difficulty.Hard);

                    hpos += 180;

                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos, vpos), 1650, false, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Hard);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos, vpos), 1650, false, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Normal);

                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos - 40, vpos - 40), 1650, false, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Easy);

                    hpos += 100;


                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos, vpos + 60), 1750, false, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Hard);

                    hpos += 100;

                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos, vpos), 1850, false, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Easy);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos, vpos), 1850, false, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Normal);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(hpos, vpos), 1850, false, 0, HitObjectSoundType.Normal) { Clocking = ClockTypes.Manual }, Difficulty.Hard);

                    HitObjectManager.PostProcessing();
                    HitObjectManager.SetActiveStream(Difficulty.Easy);
                    playfieldBackground.ChangeColour(Difficulty.Easy, false);

                    pText streamTitle = showText(getDifficultyName(Difficulty.Easy), -90);

                    Difficulty currentStream = Difficulty.Easy;
                    int lastSecond = 0;
                    bool forwards = true;
                    int startTime = Clock.Time;

                    const int delay_between_switches = 1600;

                    currentSegmentDelegate = delegate
                    {
                        if ((Clock.Time - startTime) / delay_between_switches != lastSecond)
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
                            streamTitle = showText(getDifficultyName(currentStream), -90);

                            HitObjectManager.SetActiveStream(currentStream, true);
                            playfieldBackground.ChangeColour(currentStream, true);
                        }
                    };

                    break;
                case TutorialSegments.Stream_5:
                    HitObjectManager.SetActiveStream(Difficulty.Normal, true);
                    playfieldBackground.ChangeColour(Difficulty.Normal, true);

                    foreach (SpriteManager sm in HitObjectManager.streamSpriteManagers)
                        if (sm != null) sm.ScaleTo(0.5f, 500, EasingTypes.InOut).MoveTo(new Vector2(0, 150), 500, EasingTypes.In);

                    loadNextSegment();
                    break;
                case TutorialSegments.Healthbar_1:
                    showText(LocalisationManager.GetString(OsuString.Healthbar1), -90);
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
                    showText(LocalisationManager.GetString(OsuString.Healthbar2), -90);
                    showTouchToContinue();
                    break;
                case TutorialSegments.Healthbar_3:
                    showText(LocalisationManager.GetString(OsuString.Healthbar3), -80);

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
                                    HitObjectManager.SetActiveStream(Difficulty.Hard, true);
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
                                healthBar.SetCurrentHp(healthBar.CurrentHp + 1 * Clock.ElapsedRatioToSixty);
                            }
                        };
                    }
                    break;
                case TutorialSegments.Healthbar_4:
                    showText(LocalisationManager.GetString(OsuString.Healthbar4), -90);
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
                                    HitObjectManager.SetActiveStream(Difficulty.Normal, true);
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
                                healthBar.SetCurrentHp(healthBar.CurrentHp - 1 * Clock.ElapsedRatioToSixty);
                            }
                        };
                    }
                    break;
                case TutorialSegments.Healthbar_5:
                    showText(LocalisationManager.GetString(OsuString.Healthbar5), -80);

                    HitObjectManager.SetActiveStream(Difficulty.Easy, true);
                    playfieldBackground.ChangeColour(Difficulty.Easy, true);

                    currentSegmentDelegate = delegate
                    {
                        if (playfieldBackground.Velocity == 0)
                            healthBar.SetCurrentHp(healthBar.CurrentHp - 0.5f * Clock.ElapsedRatioToSixty);
                        if (healthBar.CurrentHp == 0)
                        {

                            if (!touchToContinue)
                            {
                                showTouchToContinue();
                                showFailScreen();
                            }
                        }
                        else if (healthBar.CurrentHp < HealthBar.HP_BAR_MAXIMUM / 3)
                            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING, false);
                    };
                    break;
                case TutorialSegments.Healthbar_End:
                    healthBar.SetCurrentHp(100);
                    hideFailScreen();
                    AudioEngine.Music.Play();
                    AudioEngine.Music.DimmableVolume = 1; //may have been dimmed during fail.

                    healthBar.InitialIncrease = true;
                    currentSegmentDelegate = delegate { if (healthBar.DisplayHp > 20) loadNextSegment(); };
                    break;

                case TutorialSegments.Score_1:
                    scoreDisplay = new ScoreDisplay(false);
                    {
                        pDrawable lastFlash = null;
                        currentSegmentDelegate = delegate
                        {
                            if (lastFlash == null || lastFlash.Alpha == 0)
                            {
                                lastFlash = scoreDisplay.s_Accuracy.AdditiveFlash(1000, 1).ScaleTo(scoreDisplay.s_Accuracy.ScaleScalar * 1.1f, 1000);
                                lastFlash = scoreDisplay.s_Score.AdditiveFlash(1000, 1).ScaleTo(scoreDisplay.s_Score.ScaleScalar * 1.1f, 1000);
                            }
                        };
                    }
                    showText(LocalisationManager.GetString(OsuString.Score1));
                    showTouchToContinue();
                    break;
                case TutorialSegments.Score_2:
                    showText(LocalisationManager.GetString(OsuString.Score2));
                    showTouchToContinue();
                    break;
                case TutorialSegments.Score_3:
                    showText(LocalisationManager.GetString(OsuString.Score3));
                    {
                        backButton.FadeOut(500);
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
                                        showTouchToContinue();

                                    if (lastFlash == null || lastFlash.Alpha == 0)
                                        lastFlash = comboCounter.s_hitCombo.AdditiveFlash(1000, 1).ScaleTo(comboCounter.s_hitCombo.ScaleScalar * 1.1f, 1000);
                                }
                            };
                        }, 500);
                    }
                    break;
                case TutorialSegments.Score_4:
                    comboCounter.SetCombo(0);
                    showText(LocalisationManager.GetString(OsuString.Score4));
                    GameBase.Scheduler.Add(delegate
                    {
                        backButton.FadeOut(500);
                        showTouchToContinue();
                    }, 1500);
                    break;
                case TutorialSegments.TutorialMap_Introduction:
                    hideDemo();
                    showText(LocalisationManager.GetString(OsuString.PutTogether));
                    showTouchToContinue();
                    break;
                case TutorialSegments.TutorialMap_Interact:
                    if (HitObjectManager != null) HitObjectManager.Dispose();
                    HitObjectManager = new HitObjectManager(Beatmap);

                    prepareInteract();

                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(56, 72), music_offset + 60000, true, 0, HitObjectSoundType.Normal), Difficulty);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(456, 72), music_offset + 61500, false, 0, HitObjectSoundType.Normal), Difficulty);
                    HitObjectManager.Add(new Slider(HitObjectManager, new Vector2(456, 192), music_offset + 63000, false, 0, HitObjectSoundType.Normal, CurveTypes.Bezier, 1, 400, new List<Vector2>() { new Vector2(456, 192), new Vector2(48, 192) }, null, 266.666666666, 50), Difficulty);

                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(456, 312), music_offset + 66000, true, 0, HitObjectSoundType.Normal), Difficulty);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(56, 312), music_offset + 67500, false, 0, HitObjectSoundType.Normal), Difficulty);
                    HitObjectManager.Add(new Slider(HitObjectManager, new Vector2(56, 192), music_offset + 69000, false, 0, HitObjectSoundType.Normal, CurveTypes.Bezier, 2, 400, new List<Vector2>() { new Vector2(56, 192), new Vector2(456, 192) }, null, 266.666666666, 50), Difficulty);

                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(112, 64), music_offset + 72750, true, 0, HitObjectSoundType.Normal), Difficulty);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(400, 320), music_offset + 73500, false, 0, HitObjectSoundType.Normal), Difficulty);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(400, 320), music_offset + 74250, false, 0, HitObjectSoundType.Normal), Difficulty);

                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(256, 264), music_offset + 75000, true, 0, HitObjectSoundType.Normal), Difficulty);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(112, 320), music_offset + 75750, false, 0, HitObjectSoundType.Normal), Difficulty);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(400, 64), music_offset + 76500, false, 0, HitObjectSoundType.Normal), Difficulty);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(400, 64), music_offset + 77250, false, 0, HitObjectSoundType.Normal), Difficulty);

                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(56, 192), music_offset + 78000, true, 0, HitObjectSoundType.Normal), Difficulty);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(56, 192), music_offset + 78750, false, 0, HitObjectSoundType.Normal), Difficulty);
                    HitObjectManager.Add(new Slider(HitObjectManager, new Vector2(56, 192), music_offset + 79500, false, 0, HitObjectSoundType.Normal, CurveTypes.Bezier, 1, 400, new List<Vector2>() { new Vector2(56, 192), new Vector2(256, 136), new Vector2(472, 196) }, null, 266.666666666, 50), Difficulty);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(400, 320), music_offset + 81750, false, 0, HitObjectSoundType.Normal), Difficulty);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(256, 320), music_offset + 82500, false, 0, HitObjectSoundType.Normal), Difficulty);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(112, 320), music_offset + 83250, false, 0, HitObjectSoundType.Normal), Difficulty);

                    HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(400, 104), music_offset + 84000, true, 0, HitObjectSoundType.Normal, 50, 8, null, 266.666666666, 50) { SampleSet = new SampleSetInfo() { SampleSet = SampleSet.Drum, Volume = 0.8f } }, Difficulty);
                    HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(112, 104), music_offset + 85500, true, 0, HitObjectSoundType.Normal, 50, 8, null, 266.666666666, 50) { SampleSet = new SampleSetInfo() { SampleSet = SampleSet.Drum, Volume = 0.8f } }, Difficulty);
                    HitObjectManager.Add(new HoldCircle(HitObjectManager, new Vector2(400, 288), music_offset + 87000, true, 0, HitObjectSoundType.Normal, 50, 8, null, 266.666666666, 50) { SampleSet = drumSampleSetInfo }, Difficulty);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(120, 264), music_offset + 88500, true, 0, HitObjectSoundType.Normal), Difficulty);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(256, 152), music_offset + 89250, false, 0, HitObjectSoundType.Normal), Difficulty);

                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(80, 96), music_offset + 90000, true, 0, HitObjectSoundType.Normal), Difficulty);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(432, 96), music_offset + 90000, false, 0, HitObjectSoundType.Normal), Difficulty);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(24, 208), music_offset + 90750, false, 0, HitObjectSoundType.Normal), Difficulty);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(488, 208), music_offset + 90750, false, 0, HitObjectSoundType.Normal), Difficulty);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(384, 296), music_offset + 91500, false, 0, HitObjectSoundType.Normal), Difficulty);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(128, 296), music_offset + 91500, false, 0, HitObjectSoundType.Normal), Difficulty);
                    HitObjectManager.Add(new HitCircle(HitObjectManager, new Vector2(256, 184), music_offset + 92250, false, 0, HitObjectSoundType.Normal), Difficulty);

                    HitObjectManager.Add(new Spinner(HitObjectManager, music_offset + 93000, music_offset + 96000, HitObjectSoundType.Normal), Difficulty);

                    HitObjectManager.PostProcessing();
                    HitObjectManager.SetActiveStream(Difficulty.Easy);

                    bool done = false;

                    currentSegmentDelegate = delegate
                    {
                        if (HitObjectManager.AllNotesHit && !done)
                        {
                            loadNextSegment();
                        }
                    };
                    break;
                case TutorialSegments.TutorialMap_Judge:
                    if (HitObjectManager != null) HitObjectManager.Dispose();
                    HitObjectManager = null;

                    judge();

                    GameBase.Scheduler.Add(delegate
                    {
                        if (CurrentScore.countMiss > 10 || CurrentScore.count50 > 20)
                        {
                            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING, false);
                            showText(LocalisationManager.GetString(OsuString.MorePractice));
                            nextSegment = TutorialSegments.TutorialMap_Interact;
                        }
                        else if (CurrentScore.count100 + CurrentScore.count50 + CurrentScore.countMiss > 5)
                        {
                            showText(LocalisationManager.GetString(OsuString.StackedJudge3));
                        }
                        else
                        {
                            showText(LocalisationManager.GetString(OsuString.StackedJudge4));
                        }

                        showTouchToContinue();
                    }, 500);

                    break;
                case TutorialSegments.Outro:
                    showText(LocalisationManager.GetString(OsuString.Completion));
                    showTouchToContinue();
                    backButton.FadeOut(100);
                    break;
                case TutorialSegments.End:
#if !ARCADE
                    backButton.HandleInput = false;
                    Options.Options.DisplayFingerGuideDialog();
                    Options.Options.DisplayEasyModeDialog();
#endif

                    currentSegmentDelegate = delegate
                    {
                        if (!Director.IsTransitioning && GameBase.NotificationQueue.Count == 0 && GameBase.ActiveNotification == null)
                            Director.ChangeMode(OsuMode.SongSelect, new FadeTransition(3000, FadeTransition.DEFAULT_FADE_IN));
                    };
                    break;

            }
        }

        private void judge()
        {
            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO, false);
            backButton.FadeIn(500);
        }

        private string getDifficultyName(Difficulty difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.Easy:
                    return LocalisationManager.GetString(OsuString.Easy);
                case Difficulty.Normal:
                    return LocalisationManager.GetString(OsuString.Normal);
                case Difficulty.Hard:
                    return LocalisationManager.GetString(OsuString.Hard);
            }

            return string.Empty;
        }

        private void prepareInteract()
        {
            backButton.FadeOut(500);

            resetScore();
            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_STANDARD, false);

            Player.Autoplay = false;

            Difficulty = Difficulty.Easy;

            if (countdown == null) countdown = new CountdownDisplay();

            AudioEngine.Music.SeekTo(55000);
            Resume(music_offset + 160 * music_beatlength, 8);
            loadBeatmap();
        }

        protected override void hitObjectManager_OnScoreChanged(ScoreChange change, HitObject hitObject)
        {
            switch (change)
            {
                case ScoreChange.Hit300:
                case ScoreChange.Hit300g:
                case ScoreChange.Hit300k:
                case ScoreChange.Hit300m:
                    showText(LocalisationManager.GetString(OsuString.Perfect), 0).FadeOut(1000);
                    break;
                case ScoreChange.Hit100:
                case ScoreChange.Hit100m:
                case ScoreChange.Hit100k:
                    if (Clock.AudioTime < hitObject.StartTime)
                        showText(LocalisationManager.GetString(OsuString.TimingEarly), -60).FadeOut(1000);
                    else
                        showText(LocalisationManager.GetString(OsuString.TimingLate), 60).FadeOut(1000);
                    break;
                case ScoreChange.Hit50:
                case ScoreChange.Hit50m:
                case ScoreChange.Miss:
                    if (Clock.AudioTime < hitObject.StartTime)
                    {
                        pText t = showText(LocalisationManager.GetString(OsuString.TimingVeryEarly), -60);
                        t.TextSize *= 1.4f;
                        t.Colour = Color4.OrangeRed;
                        t.FadeOut(1000);
                    }
                    else
                    {
                        pText t = showText(LocalisationManager.GetString(OsuString.TimingVeryLate), 60);
                        t.TextSize *= 1.4f;
                        t.Colour = Color4.OrangeRed;
                        t.FadeOut(1000);
                    }
                    break;
            }

            base.hitObjectManager_OnScoreChanged(change, hitObject);
        }

        protected override void UpdateStream()
        {
        }

        protected override bool CheckForCompletion()
        {
            return false;
        }

        enum TutorialSegments
        {
            None,
            Introduction_1,
            Introduction_2,
            Introduction_3,
            GuideFingers_1,
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
            TutorialMap_Introduction,
            TutorialMap_Interact,
            TutorialMap_Judge,
            Outro,
            End,
        }
    }
}
