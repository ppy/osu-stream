using System;
using OpenTK.Graphics.ES11;
using OpenTK.Platform.iPhoneOS;
using MonoTouch.CoreAnimation;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;
using MonoTouch.UIKit;
using System.Drawing;
using OpenTK.Graphics;

namespace osum
{
    [MonoTouch.Foundation.Register("EAGLView")]
    public partial class EAGLView : UIView
    {
        public static EAGLView Instance;
        iPhoneOSGraphicsContext context;
        GameBase game;
        uint frameBuffer;
        uint renderbuffer;
        private NSTimer timer;

        [Export("layerClass")]
        public static Class LayerClass()
        {
            return new Class (typeof(CAEAGLLayer));
        }

        public EAGLView(RectangleF frame) : base(frame)
        {
            CAEAGLLayer eagl = (CAEAGLLayer)Layer;

            eagl.DrawableProperties = NSDictionary.FromObjectsAndKeys(new NSObject[] {
                 NSNumber.FromBoolean(true),
                 EAGLColorFormat.RGBA8
             }, new NSObject[] {
                 EAGLDrawableProperty.RetainedBacking,
                 EAGLDrawableProperty.ColorFormat
             });

            eagl.ContentsScale = UIScreen.MainScreen.Scale;

            context = (iPhoneOSGraphicsContext)((IGraphicsContextInternal)GraphicsContext.CurrentContext).Implementation;

            CAEAGLLayer eaglLayer = (CAEAGLLayer)Layer;
            context.MakeCurrent(null);

            GL.Oes.GenRenderbuffers(1, ref renderbuffer);
            GL.Oes.BindRenderbuffer(All.RenderbufferOes, renderbuffer);

            if (!context.EAGLContext.RenderBufferStorage((uint)All.RenderbufferOes, eaglLayer))
                throw new InvalidOperationException ("Error with RenderbufferStorage()!");

            GL.Oes.GenFramebuffers(1, ref frameBuffer);
            GL.Oes.BindFramebuffer(All.FramebufferOes, frameBuffer);
            GL.Oes.FramebufferRenderbuffer(All.FramebufferOes, All.ColorAttachment0Oes, All.RenderbufferOes, renderbuffer);

            Instance = this;

            Opaque = true;
            ExclusiveTouch = true;
            MultipleTouchEnabled = true;
            UserInteractionEnabled = true;
        }

        //CADisplayLink dl;

        public void StartAnimation()
        {
            if (timer != null) return;

            timer = NSTimer.CreateRepeatingTimer(0.0001, DrawFrame);
            NSRunLoop.Main.AddTimer(timer, "NSDefaultRunLoopMode");

            //Thread t = new Thread(DrawFrame);
            //t.Start();

            //dl = UIScreen.MainScreen.CreateDisplayLink(this, new Selector("DrawFrame"));
            //dl.AddToRunLoop(NSRunLoop.Current, "NSDefaultRunLoopMode");
        }

        public void StopAnimation()
        {
            timer.Dispose();
            timer = null;
        }

        private void DrawFrame()
        {
            //using (NSAutoreleasePool pool = new NSAutoreleasePool())
            //while (true)
            {
                //while (CFRunLoop.Current.RunInMode("NSDefaultRunLoopMode",0.003, false) == CFRunLoopExitReason.HandledSource)
                //{}

                game.Update();
                game.Draw();

                context.EAGLContext.PresentRenderBuffer((int)All.RenderbufferOes);
            }
        }

        public void Run(GameBase game)
        {
            this.game = game;
        }
    }
}

