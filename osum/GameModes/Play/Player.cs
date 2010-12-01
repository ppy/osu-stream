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

namespace osum.GameModes
{
    public class Player : GameMode
    {
        HitObjectManager hitObjectManager;

        HealthBar healthBar;
        ScoreDisplay scoreDisplay;

        Score currentScore;

        static Beatmap Beatmap;

        public Player() : base()
        {
        }

        void InputManager_OnDown(InputSource source, TrackingPoint point)
        {
            //pass on the event to hitObjectManager for handling.
            hitObjectManager.HandlePressAt(point);
        }

        internal override void Initialize()
        {
            InputManager.OnDown += new InputHandler(InputManager_OnDown);

            hitObjectManager = new HitObjectManager(Beatmap);
            hitObjectManager.OnScoreChanged += new ScoreChangeDelegate(hitObjectManager_OnScoreChanged);

            hitObjectManager.LoadFile();

            healthBar = new HealthBar();

            scoreDisplay = new ScoreDisplay();

            currentScore = new Score();

            AudioEngine.Music.Load(Beatmap.GetFileBytes(Beatmap.AudioFilename));
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
                    break;
                case ScoreChange.SliderTick:
                    currentScore.totalScore += 10;
                    break;
                case ScoreChange.Hit50:
                    currentScore.totalScore += 50;
                    currentScore.count50++;
                    break;
                case ScoreChange.Hit100:
                    currentScore.totalScore += 100;
                    currentScore.count100++;
                    break;
                case ScoreChange.Hit300:
                    currentScore.totalScore += 300;
                    currentScore.count300++;
                    break;
                case ScoreChange.Miss:
                    currentScore.countMiss++;
                    break;
                default:
                    throw new Exception("unhandled score change");
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

            hitObjectManager.Dispose();

            healthBar.Dispose();
            scoreDisplay.Dispose();

            base.Dispose();
        }

        public override void Draw()
        {
            hitObjectManager.Draw();

            scoreDisplay.Draw();
            healthBar.Draw();

            base.Draw();
        }

        public override void Update()
        {
            hitObjectManager.Update();

            healthBar.Update();
            scoreDisplay.Update();

            base.Update();
        }



        internal static void SetBeatmap(Beatmap beatmap)
        {
            Beatmap = beatmap;
        }
    }
}

