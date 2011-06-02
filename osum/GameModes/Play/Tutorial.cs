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

namespace osum.GameModes.Play
{
    class Tutorial : Player
    {
        BackButton backButton;
        pText touchToContinueText;

        internal override void Initialize()
        {
            Difficulty = Difficulty.None;

            MainMenu.InitializeBgm();

            touchToContinueText = new pText("Tap to continue!", 30, new Vector2(0, 20), 1, true, Color4.YellowGreen)
            {
                TextBounds = new Vector2(GameBase.BaseSize.Width * 0.8f, 0),
                Field = FieldTypes.StandardSnapBottomCentre,
                TextAlignment = TextAlignment.Centre,
                TextShadow = true,
                Bold = true,
                Origin = OriginTypes.BottomCentre
            };
            spriteManager.Add(touchToContinueText);

            backButton = new BackButton(delegate { Director.ChangeMode(OsuMode.MainMenu); });
            spriteManager.Add(backButton);

            loadNextSegment();

            base.Initialize();
        }

        enum TutorialSegments
        {
            None,
            Introduction_1,
            Introduction_2,
            Healthbar_1,
            Healthbar_2,
            Healthbar_3,
            Healthbar_4,
            Healthbar_5,
            Healthbar_6,
            End,
        }

        TutorialSegments currentSegment;
        VoidDelegate currentSegmentDelegate;

        bool touchToContinue = true;
        List<pDrawable> currentSegmentSprites = new List<pDrawable>();
        private void loadNextSegment()
        {
            currentSegment = (TutorialSegments)(currentSegment + 1);

            foreach (pDrawable p in currentSegmentSprites)
            {
                p.AlwaysDraw = false;
                p.FadeOut(100);
                p.ScaleTo(0.9f, 400, EasingTypes.In);
            }

            currentSegmentSprites.Clear();
            currentSegmentDelegate = null;

            switch (currentSegment)
            {
                case TutorialSegments.Introduction_1:
                    showText("Welcome to the world of osu!.\nThis tutorial will teach you everything you need to know in order to become a rhythm master.");
                    break;
                case TutorialSegments.Introduction_2:
                    showText("osu!stream is a rhythm game which requires both rhythmical and positional accuracy.");
                    playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_STANDARD);
                    break;
                case TutorialSegments.Healthbar_1:
                    showText("The health bar is located at the top-left of your display.");
                    healthBar = new HealthBar();
                    healthBar.spriteManager.Sprites.ForEach(s => s.AdditiveFlash(1000, 1));
                    streamSwitchDisplay = new StreamSwitchDisplay();
                    break;
                case TutorialSegments.Healthbar_2:
                    showText("It will go up or down depending on your performance.");
                    break;
                case TutorialSegments.Healthbar_3:
                    showText("In stream mode gameplay, if the health bar hits zero, you will drop down a stream.", -120);
                    currentSegmentDelegate = delegate { playfieldBackground.Move(-4); };
                    streamSwitchDisplay.BeginSwitch(false);
                    healthBar.SetCurrentHp(0);
                    break;
                case TutorialSegments.Healthbar_4:
                    showText("In a similar matter, if it fills up, you will rise up a stream.", -120);
                    currentSegmentDelegate = delegate { playfieldBackground.Move(4); };
                    streamSwitchDisplay.BeginSwitch(true);
                    healthBar.SetCurrentHp(200);
                    break;
                case TutorialSegments.Healthbar_5:
                    showText("If it hits zero on the lowest stream you will fail instantly, so watch out!", 0);
                    streamSwitchDisplay.EndSwitch();

                    currentSegmentDelegate = delegate
                    {
                        healthBar.SetCurrentHp(healthBar.CurrentHp - 1);
                        if (healthBar.CurrentHp == 0)
                            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_INTRO);
                        else if (healthBar.CurrentHp < HealthBar.HP_BAR_MAXIMUM / 3)
                            playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_WARNING);
                    };
                    
                    break;
                case TutorialSegments.Healthbar_6:
                    healthBar.SetCurrentHp(100);
                    playfieldBackground.ChangeColour(PlayfieldBackground.COLOUR_STANDARD);
                    healthBar.InitialIncrease = true;
                    break;
                case TutorialSegments.End:
                    backButton.HandleInput = false;
                    Director.ChangeMode(OsuMode.MainMenu, new FadeTransition(3000, FadeTransition.DEFAULT_FADE_IN));
                    touchToContinue = false;
                    break;
            }


            touchToContinueText.Transformations.Clear(); 
            if (touchToContinue)
            {
                touchToContinueText.Transform(new TransformationBounce(Clock.Time, Clock.Time + 300, 1, -0.2f, 1));
                touchToContinueText.Transform(new Transformation(TransformationType.Fade, 1, 0, Clock.Time, Clock.Time + 500) { LoopDelay = 1000, Looping = true });
            }
            else
                touchToContinueText.FadeOut(100);

            spriteManager.Add(currentSegmentSprites);
        }

        protected override void InputManager_OnDown(InputSource source, TrackingPoint point)
        {
            if (touchToContinue)
            {
                loadNextSegment();
                return;
            }

            base.InputManager_OnDown(source, point);
        }

        private void showText(string text, float verticalOffset = 0)
        {
            pText pt = new pText(text, 35, new Vector2(0,verticalOffset), 1, true, Color4.White)
            {
                TextBounds = new Vector2(GameBase.BaseSize.Width * 0.95f, 0),
                Field = FieldTypes.StandardSnapCentre,
                TextAlignment = TextAlignment.Centre,
                TextShadow = true,
                Origin = OriginTypes.Centre
            };

            pt.ScaleScalar = 1.4f;
            pt.FadeIn(300);
            pt.ScaleTo(1, 400, EasingTypes.In);
            currentSegmentSprites.Add(pt);
        }

        public override void Update()
        {
            if (currentSegmentDelegate != null) currentSegmentDelegate();
            
            base.Update();
        }

        protected override void initializeUIElements()
        {
            //base.initializeUIElements();
        }
    }
}
