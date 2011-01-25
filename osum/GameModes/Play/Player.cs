//  Play.cs
//  Author: Dean Herbert <pe@ppy.sh>
//  Copyright (c) 2010 2010 Dean Herbert
using System;
using osum.GameplayElements;
using osum.GameplayElements.Beatmaps;
using osum.Helpers;
//using osu.Graphics.Renderers;
using osum.Graphics.Primitives;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using System.Drawing;
using osum.Audio;
using osum.Graphics.Renderers;
using osum.GameplayElements.Scoring;
using osum.GameModes.Play.Components;
using osum.Graphics.Sprites;
using osum.Graphics.Skins;

namespace osum.GameModes
{
    public class Player : GameMode
    {
        HitObjectManager hitObjectManager;

        HealthBar healthBar;
        ScoreDisplay scoreDisplay;
        ComboCounter comboCounter;

        Score currentScore;

        static Beatmap Beatmap;
        public static bool Autoplay;
        private pSprite playfield;

        public Player() : base()
        {
        }

        void InputManager_OnDown(InputSource source, TrackingPoint point)
        {
            //pass on the event to hitObjectManager for handling.
            hitObjectManager.HandlePressAt(point);
			
			if (InputManager.TrackingPoints.Count == 2)
			{
				Vector2 p1 = InputManager.TrackingPoints[0].WindowPosition;
				Vector2 p2 = InputManager.TrackingPoints[1].WindowPosition;
				
				if (Math.Max(p1.X,p2.X) > (GameBase.WindowBaseSize.Width - 40) &&
				    Math.Min(p1.X,p2.X) < 40 &&
				    p1.Y + p2.Y < 80)
				{
					Director.ChangeMode(OsuMode.SongSelect);
				}
				
			}
        }

        void InputManager_OnMove(InputSource source, TrackingPoint point)
        {
            // fast forward for iphone
            if (InputManager.TrackingPoints.Count >= 4)
            {
                AudioEngine.Music.SeekTo(Clock.AudioTime + 2000);
            }
        }

        internal override void Initialize()
        {
            InputManager.OnDown += new InputHandler(InputManager_OnDown);
            InputManager.OnMove += new InputHandler(InputManager_OnMove);

            hitObjectManager = new HitObjectManager(Beatmap);
            hitObjectManager.OnScoreChanged += new ScoreChangeDelegate(hitObjectManager_OnScoreChanged);

            hitObjectManager.LoadFile();

            healthBar = new HealthBar();

            scoreDisplay = new ScoreDisplay();

            comboCounter = new ComboCounter();

            currentScore = new Score();

            playfield =
                new pSprite(TextureManager.Load(@"playfield"), FieldTypes.StandardSnapCentre, OriginTypes.Centre,
                            ClockTypes.Mode, Vector2.Zero, 0, true, Color.White);
            spriteManager.Add(playfield);

            AudioEngine.Music.Load(Beatmap.GetFileBytes(Beatmap.AudioFilename));
            Director.OnTransitionEnded += new VoidDelegate(Director_OnTransitionEnded);
        }

        void Director_OnTransitionEnded()
        {
            AudioEngine.Music.Play();
        }

        void hitObjectManager_OnScoreChanged(ScoreChange change, HitObject hitObject)
        {
            //handle the score addition
            switch (change & ~ScoreChange.ComboAddition)
            {
                case ScoreChange.SpinnerBonus:
                    currentScore.totalScore += 1000;
                    break;
                case ScoreChange.SpinnerSpinPoints:
                    currentScore.totalScore += 500;
                    break;
                case ScoreChange.SpinnerSpin:
                    break;
                case ScoreChange.SliderRepeat:
                case ScoreChange.SliderEnd:
                    currentScore.totalScore += 30;
                    comboCounter.IncreaseCombo();
                    break;
                case ScoreChange.SliderTick:
                    currentScore.totalScore += 10;
                    comboCounter.IncreaseCombo();
                    break;
                case ScoreChange.Hit50:
                    currentScore.totalScore += 50;
                    currentScore.count50++;
                    comboCounter.IncreaseCombo();
                    break;
                case ScoreChange.Hit100:
                    currentScore.totalScore += 100;
                    currentScore.count100++;
                    comboCounter.IncreaseCombo();
                    break;
                case ScoreChange.Hit300:
                    currentScore.totalScore += 300;
                    currentScore.count300++;
                    comboCounter.IncreaseCombo();
                    break;
                case ScoreChange.Miss:
                    currentScore.countMiss++;
                    comboCounter.SetCombo(0);
                    break;
            }

            //then handle the hp addition
            switch (change)
            {
                case ScoreChange.Miss:
                    healthBar.ReduceCurrentHp(20);
                    break;
                default:
                    healthBar.IncreaseCurrentHp(5);
                    break;
            }

            scoreDisplay.SetScore(currentScore.totalScore);
            scoreDisplay.SetAccuracy(currentScore.accuracy * 100);
        }

        public override void Dispose()
        {
            InputManager.OnDown -= new InputHandler(InputManager_OnDown);

            AudioEngine.Music.Stop();

            hitObjectManager.Dispose();

            healthBar.Dispose();
            scoreDisplay.Dispose();

            base.Dispose();
        }

        public override bool Draw()
        {
            base.Draw();

            hitObjectManager.Draw();

            scoreDisplay.Draw();
            healthBar.Draw();
            comboCounter.Draw();

            return true;
        }

        public override void Update()
        {
            //check whether the map is finished
			if (hitObjectManager.AllNotesHit)
			{
				Ranking.RankableScore = currentScore;
				Director.ChangeMode(OsuMode.Ranking);
				
			}
			
			hitObjectManager.Update();

            healthBar.Update();
            scoreDisplay.Update();
            comboCounter.Update();

            base.Update();
        }

        internal static void SetBeatmap(Beatmap beatmap)
        {
            Beatmap = beatmap;
        }
    }
}

