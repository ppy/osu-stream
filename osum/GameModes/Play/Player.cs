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

namespace osum.GameModes
{
    public class Player : GameMode
    {
        HitObjectManager hitObjectManager;

        HealthBar healthBar;

        public Player()
            : base()
        {
        }

        void InputManager_OnDown(InputSource source, TrackingPoint point)
        {
            //pass on the event to hitObjectManager for handling.
            hitObjectManager.HandlePressAt(point);
        }

        internal override void Initialize()
        {
            Beatmap beatmap = new Beatmap("Beatmaps/bcl/");

            InputManager.OnDown += new InputHandler(InputManager_OnDown);

            hitObjectManager = new HitObjectManager(beatmap);
            hitObjectManager.OnScoreChanged += new ScoreChangeDelegate(hitObjectManager_OnScoreChanged);

            hitObjectManager.LoadFile();

            healthBar = new HealthBar();

            AudioEngine.Music.Load("Beatmaps/bcl/babycruisingedit.mp3");
            AudioEngine.Music.Play();
        }

        void hitObjectManager_OnScoreChanged(ScoreChange change, HitObject hitObject)
        {
            switch (change)
            {
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

            base.Dispose();
        }

        public override void Draw()
        {
            hitObjectManager.Draw();

            healthBar.Draw();

            base.Draw();
        }

        public override void Update()
        {
            hitObjectManager.Update();

            healthBar.Update();

            base.Update();
        }


    }
}

