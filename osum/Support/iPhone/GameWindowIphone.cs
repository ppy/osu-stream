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
using System.Threading;
using osum.Helpers;
using MonoTouch.CoreFoundation;

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

        CADisplayLink dl;

        public void StartAnimation()
        {
            if (dl == null) dl = CADisplayLink.Create(DrawFrame);
            dl.AddToRunLoop(NSRunLoop.Current, NSRunLoop.NSDefaultRunLoopMode);
            Console.WriteLine("display link with frame interval " + dl.FrameInterval);
        }

        bool throttling = false;

        public void StopAnimation()
        {
            dl.RemoveFromRunLoop(NSRunLoop.Current, NSRunLoop.NSDefaultRunLoopMode);
        }

        int updateCount;

        [Export("DrawFrame")]
        private void DrawFrame()
        {
            bool shouldThrottle = GameBase.GloballyDisableInput || GameBase.ThrottleExecution;
            if (GameBase.GloballyDisableInput || shouldThrottle != throttling)
            {
                throttling = shouldThrottle;
                StopAnimation();
                StartAnimation();
            }

            game.Update();
            game.Draw();
            context.EAGLContext.PresentRenderBuffer((int)All.RenderbufferOes);
        }

        public void Run(GameBase game)
        {
            this.game = game;
        }
    }
}

