using System;
using OpenTK;
using OpenTK.Graphics.ES11;
using OpenTK.Platform.iPhoneOS;
using MonoTouch.CoreAnimation;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;

#if iOS
using TextureTarget = OpenTK.Graphics.ES11.All;
using TextureParameterName = OpenTK.Graphics.ES11.All;
using EnableCap = OpenTK.Graphics.ES11.All;
using BlendingFactorSrc = OpenTK.Graphics.ES11.All;
using BlendingFactorDest = OpenTK.Graphics.ES11.All;
using PixelStoreParameter = OpenTK.Graphics.ES11.All;
using VertexPointerType = OpenTK.Graphics.ES11.All;
using ColorPointerType = OpenTK.Graphics.ES11.All;
using ClearBufferMask = OpenTK.Graphics.ES11.All;
using TexCoordPointerType = OpenTK.Graphics.ES11.All;
using BeginMode = OpenTK.Graphics.ES11.All;
using MatrixMode = OpenTK.Graphics.ES11.All;
using PixelInternalFormat = OpenTK.Graphics.ES11.All;
using PixelFormat = OpenTK.Graphics.ES11.All;
using PixelType = OpenTK.Graphics.ES11.All;
using ShaderType = OpenTK.Graphics.ES11.All;
using VertexAttribPointerType = OpenTK.Graphics.ES11.All;
using ProgramParameter = OpenTK.Graphics.ES11.All;
using ShaderParameter = OpenTK.Graphics.ES11.All;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
#else
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using osum.Input;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
#endif

using System.Drawing;
using osum.Graphics.Skins;
using osum.Support.iPhone;
using osum.GameModes;
using osum.Audio;
using OpenTK.Graphics;

namespace osum
{
    [MonoTouch.Foundation.Register("EAGLView")]
    public partial class EAGLView : UIView
    {
        public static EAGLView Instance;
        All _depthFormat;
        bool _autoResize;
        iPhoneOSGraphicsContext _context;
        uint _framebuffer;
        uint _renderbuffer;
        uint _depthbuffer;
        SizeF _size;
        bool _hasBeenCurrent;
        private NSTimer _animationTimer;

        [Export("layerClass")]
        public static Class LayerClass()
        {
            return new Class (typeof(CAEAGLLayer));
        }

        public EAGLView(RectangleF frame) : this(frame, All.Rgba8Oes, 0, false)
        {
        }

        public EAGLView(RectangleF frame,All format) : this(frame, format, 0, false)
        {
        }

        public EAGLView(RectangleF frame,All format,All depth, bool retained) : base(frame)
        {
            CAEAGLLayer eaglLayer = (CAEAGLLayer)Layer;

            eaglLayer.DrawableProperties = NSDictionary.FromObjectsAndKeys(new NSObject[] {
                 NSNumber.FromBoolean(true),
                 EAGLColorFormat.RGBA8
             }, new NSObject[] {
                 EAGLDrawableProperty.RetainedBacking,
                 EAGLDrawableProperty.ColorFormat
             });

            _depthFormat = depth;
            _context = (iPhoneOSGraphicsContext)((IGraphicsContextInternal)GraphicsContext.CurrentContext).Implementation;
            CreateSurface();

            Instance = this;

            ExclusiveTouch = true;
            MultipleTouchEnabled = true;
            UserInteractionEnabled = true;
        }

        protected override void Dispose(bool disposing)
        {
            DestroySurface();
            _context.Dispose();
            _context = null;
        }

        void CreateSurface()
        {
            CAEAGLLayer eaglLayer = (CAEAGLLayer)Layer;
            if (!_context.IsCurrent)
                _context.MakeCurrent(null);

            var newSize = eaglLayer.Bounds.Size;
            newSize.Width = (float)Math.Round(newSize.Width);
            newSize.Height = (float)Math.Round(newSize.Height);

            int oldRenderbuffer = 0, oldFramebuffer = 0;
            GL.GetInteger(All.RenderbufferBindingOes, ref oldRenderbuffer);
            GL.GetInteger(All.FramebufferBindingOes, ref oldFramebuffer);

            GL.Oes.GenRenderbuffers(1, ref _renderbuffer);
            GL.Oes.BindRenderbuffer(All.RenderbufferOes, _renderbuffer);

            if (!_context.EAGLContext.RenderBufferStorage((uint)All.RenderbufferOes, eaglLayer))
            {
                GL.Oes.DeleteRenderbuffers(1, ref _renderbuffer);
                GL.Oes.BindRenderbuffer(All.RenderbufferBindingOes, (uint)oldRenderbuffer);
                throw new InvalidOperationException ("Error with RenderbufferStorage()!");
            }

            GL.Oes.GenFramebuffers(1, ref _framebuffer);
            GL.Oes.BindFramebuffer(All.FramebufferOes, _framebuffer);
            GL.Oes.FramebufferRenderbuffer(All.FramebufferOes, All.ColorAttachment0Oes, All.RenderbufferOes, _renderbuffer);
            if (_depthFormat != 0)
            {
                GL.Oes.GenRenderbuffers(1, ref _depthbuffer);
                GL.Oes.BindFramebuffer(All.RenderbufferOes, _depthbuffer);
                GL.Oes.RenderbufferStorage(All.RenderbufferOes, _depthFormat, (int)newSize.Width, (int)newSize.Height);
                GL.Oes.FramebufferRenderbuffer(All.FramebufferOes, All.DepthAttachmentOes, All.RenderbufferOes, _depthbuffer);
            }
            _size = newSize;
            if (!_hasBeenCurrent)
            {
                GL.Viewport(0, 0, (int)newSize.Width, (int)newSize.Height);
                GL.Scissor(0, 0, (int)newSize.Width, (int)newSize.Height);
                _hasBeenCurrent = true;
            } else
                GL.Oes.BindFramebuffer(All.FramebufferOes, (uint)oldFramebuffer);
            GL.Oes.BindRenderbuffer(All.RenderbufferOes, (uint)oldRenderbuffer);

            Action<EAGLView> a = OnResized;
            if (a != null)
                a(this);
        }

        void DestroySurface()
        {
            EAGLContext oldContext = EAGLContext.CurrentContext;

            if (!_context.IsCurrent)
                _context.MakeCurrent(null);

            if (_depthFormat != 0)
            {
                GL.Oes.DeleteRenderbuffers(1, ref _depthbuffer);
                _depthbuffer = 0;
            }

            GL.Oes.DeleteRenderbuffers(1, ref _renderbuffer);
            _renderbuffer = 0;

            GL.Oes.DeleteFramebuffers(1, ref _framebuffer);
            _framebuffer = 0;

            EAGLContext.SetCurrentContext(oldContext);
        }

        public override void LayoutSubviews()
        {
            var bounds = Bounds;
            if (_autoResize && ((float)Math.Round(bounds.Width) != _size.Width) || ((float)Math.Round(bounds.Height) != _size.Height))
            {
                DestroySurface();
                CreateSurface();
            }
        }

        public void SetAutoResizesEaglSurface(bool resize)
        {
            _autoResize = resize;
            if (_autoResize)
                LayoutSubviews();
        }

        public void SetCurrentContext()
        {
            SetCurrentContext(_context);
        }

        public void SetCurrentContext(IGraphicsContext context)
        {
            context.MakeCurrent(null);
        }

        public bool IsCurrentContext {
            get { return _context.IsCurrent; }
        }

        public void ClearCurrentContext()
        {
            if (!EAGLContext.SetCurrentContext(null))
                Console.WriteLine("Failed to clear current context!");
        }

        public void SwapBuffers()
        {
            EAGLContext oldContext = EAGLContext.CurrentContext;

            if (!_context.IsCurrent) _context.MakeCurrent(null);

            int oldRenderbuffer = 0;
            GL.GetInteger(All.RenderbufferBindingOes, ref oldRenderbuffer);
            GL.Oes.BindRenderbuffer(All.RenderbufferOes, _renderbuffer);

            if (!_context.EAGLContext.PresentRenderBuffer((uint)All.RenderbufferOes))
                Console.WriteLine("Failed to swap renderbuffer!");

            EAGLContext.SetCurrentContext(oldContext);
        }

        private void StartAnimation()
        {
            // creating a TimeSpan with ticks. 10 million ticks per second.
            _animationTimer = NSTimer.CreateRepeatingTimer(0.0001, DrawFrame);

            NSRunLoop.Main.AddTimer(_animationTimer, "NSDefaultRunLoopMode");
        }

        private void StopAnimation()
        {
            _animationTimer.Dispose();
            _animationTimer = null;
        }

        public PointF ConvertPointFromViewToSurface(PointF point)
        {
            var bounds = Bounds;
            return new PointF ((point.X - bounds.X) / bounds.Width * _size.Width, (point.Y - bounds.Y) / bounds.Height * _size.Height);
        }

        public RectangleF ConvertRectFromViewToSurface(RectangleF rect)
        {
            var bounds = Bounds;
            return new RectangleF ((rect.X - bounds.X) / bounds.Width * _size.Width, (rect.Y - bounds.Y) / bounds.Height * _size.Height, rect.Width / bounds.Width * _size.Width, rect.Height / bounds.Height * _size.Height);
        }

        GameBase game;

        private void DrawFrame()
        {
            SetCurrentContext();
            game.Update();
            game.Draw();
            SwapBuffers();
        }

        public void Run(GameBase game)
        {
            this.game = game;
            StartAnimation();
        }

        public event Action<EAGLView> OnResized;
    }
}

