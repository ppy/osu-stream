using System;
using OpenTK.Graphics.ES11;
using OpenTK.Platform.iPhoneOS;
using CoreAnimation;
using Foundation;
using ObjCRuntime;
using OpenGLES;
using UIKit;
using System.Drawing;
using OpenTK.Graphics;
using System.Threading;
using osum.Helpers;
using CoreFoundation;

namespace osum
{
    [Foundation.Register("EAGLView")]
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

            GL.Oes.GenRenderbuffers(1, out renderbuffer);
            GL.Oes.BindRenderbuffer(All.RenderbufferOes, renderbuffer);

            if (!context.EAGLContext.RenderBufferStorage((uint)All.RenderbufferOes, eaglLayer))
                throw new InvalidOperationException ("Error with RenderbufferStorage()!");

            GL.Oes.GenFramebuffers(1, out frameBuffer);
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
        }

        bool throttling = false;

        public void StopAnimation()
        {
            if (dl != null)
                dl.RemoveFromRunLoop(NSRunLoop.Current, NSRunLoop.NSDefaultRunLoopMode);
        }

        [Export("DrawFrame")]
        private void DrawFrame()
        {
            bool shouldThrottle = GameBase.GloballyDisableInput || GameBase.ThrottleExecution;
            if (shouldThrottle != throttling)
                dl.FrameInterval = throttling ? 2 : 1;

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

