using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using OpenTK.Input;
using osum.Graphics;

namespace osum
{
    class Game : GameWindow
    {
        private pSpriteCollection sprites;

        /// <summary>Creates a 1024x768 window with the specified title.</summary>
        public Game()
            : base(1024, 768, GraphicsMode.Default, "osu!m")
        {
            VSync = VSyncMode.On;
        }

        /// <summary>Load resources here.</summary>
        /// <param name="e">Not used.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //GL.Enable(EnableCap.DepthTest);

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            // enabling and disabling the following block changes nothing
            GL.Disable(EnableCap.Lighting);
            GL.Enable(EnableCap.Blend);
            //GL.Enable(EnableCap.ColorMaterial);
            //GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.Emission);

            pTexture texture = pTexture.FromFile(@"puush.png");
            // see note in pSprite.ctor

            sprites = new pSpriteCollection();
            sprites.AddSprite(new pSprite(texture, new Vector2(110, 110), Vector2.Zero, Color.FromArgb(50, 255, 255, 255), Vector2.One, 0));
            sprites.AddSprite(new pSprite(texture, new Vector2(80, 80), Vector2.Zero, Color.FromArgb(128, 255, 255, 255), Vector2.One, 0));
            sprites.AddSprite(new pSprite(texture, new Vector2(50, 50), Vector2.Zero, Color.FromArgb(255, 255, 255, 255), Vector2.One, 0));
        }

        /// <summary>
        /// Called when your window is resized. Set your viewport here. It is also
        /// a good place to set up your projection matrix (which probably changes
        /// along when the aspect ratio of your window).
        /// </summary>
        /// <param name="e">Not used.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, 1024, 768);

            Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0, 1024, 768, 0, 0, 1);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
        }

        /// <summary>
        /// Called when it is time to setup the next frame. Add you game logic here.
        /// </summary>
        /// <param name="e">Contains timing information for framerate independent logic.</param>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (Keyboard[Key.Escape])
                Exit();
        }

        /// <summary>
        /// Called when it is time to render the next frame. Add your rendering code here.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            MakeCurrent();
            //ensures the gl context is in the current thread.

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(Color.MidnightBlue);

            //GL.Viewport(0, 0, Size.Width, Size.Height);
            //Matrix4 modelview = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY);
            //GL.LoadIdentity();
            //unnecessary?

            GL.MatrixMode(MatrixMode.Modelview);
            

            TextureGl.EnableTexture();
            //best to enable once here, rather than constantly switching in and out.  should be once per spritemanager draw call, really.

            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            //have to set the blend method (and enable blend in the init)
            //this gets set in spritemanager eventually.

            //draw code goes here
            IDrawable d = (IDrawable)sprites;
            d.Draw();
            // this will be handled by a sprite manager

            TextureGl.DisableTexture();
            //as above (enable call).

            SwapBuffers();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (Game game = new Game())
            {
                game.Run(60);
            }
        }
    }
}