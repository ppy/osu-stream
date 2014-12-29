using System;
using osum.Input.Sources;
using osum.Audio;
using OpenTK.Graphics.OpenGL;
using osum.GameModes;
using osum.GameplayElements.Beatmaps;
using osum.GameModes.Play;
using OpenTK.Platform;
using System.Reflection;
using System.Drawing;


namespace osum
{
    public class GameBaseDesktop : GameBase
    {
        public GameWindowDesktop Window;

        public GameBaseDesktop(OsuMode mode = OsuMode.Unknown) : base(mode)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
        }
        
        override public void Run()
        {
            Window = new GameWindowDesktop();
            Window.Run();
            Director.CurrentMode.Dispose();
        }

        protected override BackgroundAudioPlayer InitializeBackgroundAudio()
        {
            return new BackgroundAudioPlayerDesktop();
        }

        protected override SoundEffectPlayer InitializeSoundEffects()
        {
            return new SoundEffectPlayerBass();
        }

        protected override void InitializeInput()
        {
            //InputSourceMouse source = new InputSourceMouse(Window.Mouse);

            InputSourceTouch source = new InputSourceTouch(Window);
            InputManager.AddSource(source);
        }

        public override void UpdateSpriteResolution()
        {
            SpriteResolution = (int)(Math.Max(BASE_SPRITE_RES, NativeSize.Width / Math.Min((float)NativeSize.Width / NativeSize.Height, GamefieldBaseSize.X / GamefieldBaseSize.Y)));
        }

        public override void SetupScreen()
        {
            NativeSize = Window.ClientSize;

            base.SetupScreen();
        }

        public void Exit()
        {
            Window.Exit();
        }
    }
}

