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

            AudioEngine.Music.Load(Beatmap.GetFileBytes(Beatmap.AudioFilename));
            AudioEngine.Music.Play();
        }

        void hitObjectManager_OnScoreChanged(ScoreChange change, HitObject hitObject)
        {
            switch (change & ~ScoreChange.ComboAddition)
            {
                case ScoreChange.SpinnerBonus:
                    scoreDisplay.Increase(1000);
                    break;
                case ScoreChange.SpinnerSpinPoints:
                    scoreDisplay.Increase(500);
                    break;
                case ScoreChange.SpinnerSpin:
                    break;
                case ScoreChange.SliderRepeat:
                    scoreDisplay.Increase(30);
                    break;
                case ScoreChange.SliderTick:
                    scoreDisplay.Increase(10);
                    break;
                case ScoreChange.Hit50:
                    scoreDisplay.Increase(50);
                    break;
                case ScoreChange.Hit100:
                    scoreDisplay.Increase(100);
                    break;
                case ScoreChange.Hit300:
                    scoreDisplay.Increase(300);
                    break;
                case ScoreChange.Miss:
                    healthBar.ReduceCurrentHp(20);
                    break;
                default:
                    healthBar.IncreaseCurrentHp(10);
                    break;
            }
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

