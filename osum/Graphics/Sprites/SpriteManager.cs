using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Helpers;
using osum.Support;
using OpenTK;
using System.Drawing;
using osu_common.Helpers;

#if iOS
using OpenTK.Graphics.ES11;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;

using TextureTarget = OpenTK.Graphics.ES11.All;
using TextureParameterName = OpenTK.Graphics.ES11.All;
using EnableCap = OpenTK.Graphics.ES11.All;
using ArrayCap = OpenTK.Graphics.ES11.All;
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
using ErrorCode = OpenTK.Graphics.ES11.All;
using TextureEnvParameter = OpenTK.Graphics.ES11.All;
using TextureEnvTarget =  OpenTK.Graphics.ES11.All;
#else
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using osum.Input;
#endif

namespace osum.Graphics.Sprites
{
    public class SpriteManager : pDrawable, IDisposable
    {
        internal List<pDrawable> Sprites;

        int creationTime = Clock.Time;

        internal SpriteManager(IEnumerable<pDrawable> sprites)
        {
            Sprites = new List<pDrawable>(sprites);
            foreach (pSprite s in Sprites)
                s.SpriteManager = this;

            InputManager.OnMove += HandleInputManagerOnMove;
            InputManager.OnDown += HandleInputManagerOnDown;
            InputManager.OnUp += HandleInputManagerOnUp;

            AlwaysDraw = true;
            Alpha = 1;
        }

        internal SpriteManager()
            : this(new List<pDrawable>())
        {

        }

        internal virtual void HandleInputManagerOnUp(InputSource source, TrackingPoint trackingPoint)
        {
            if (lastUpdate != Clock.Time) return;

            if (Sprites == null) return;

            mapToCoordinates(ref trackingPoint);

            for (int i = Sprites.Count - 1; i >= 0; i--)
                Sprites[i].HandleOnUp(source, trackingPoint);
        }

        void mapToCoordinates(ref TrackingPoint t)
        {
            //todo: optimise?
            if (!matrixOperations)
                return;

            t = (TrackingPoint)t.Clone();

            t.UpdatePositions();

            Vector2 pos = t.BasePosition;
            Vector2 origPos = pos;

            pos.X -= GameBase.BaseSizeFixedWidth.Width / 2;
            pos.Y -= GameBase.BaseSizeFixedWidth.Height / 2;

            float cos = (float)Math.Cos(-Rotation);
            float sin = (float)Math.Sin(-Rotation);

            float newX = cos * pos.X - sin * pos.Y;
            float newY = sin * pos.X + cos * pos.Y;

            pos.X = newX;
            pos.Y = newY;

            pos.X /= Scale.X;
            pos.Y /= Scale.Y;

            pos.X += GameBase.BaseSizeFixedWidth.Width / 2;
            pos.Y += GameBase.BaseSizeFixedWidth.Height / 2;

            pos -= (Position + Offset);

            t.BasePosition = pos;
            t.WindowDelta = pos - origPos;

            return;
        }

        internal override bool IsOnScreen
        {
            get
            {
                return true;
            }
        }

        internal virtual void HandleInputManagerOnDown(InputSource source, TrackingPoint trackingPoint)
        {
            if (lastUpdate != Clock.Time || Director.IsTransitioning) return;

            mapToCoordinates(ref trackingPoint);

            //todo: find out why these are needed (see tutorial hitcircles part when failing)
            if (Sprites == null) return;

            for (int i = Sprites.Count - 1; i >= 0; i--)
                Sprites[i].HandleOnDown(source, trackingPoint);
        }

        internal virtual void HandleInputManagerOnMove(InputSource source, TrackingPoint trackingPoint)
        {
            if (lastUpdate != Clock.Time || Director.IsTransitioning) return;

            if (Sprites == null) return;

            mapToCoordinates(ref trackingPoint);

            for (int i = Sprites.Count - 1; i >= 0; i--)
                Sprites[i].HandleOnMove(source, trackingPoint);
        }

        pDrawableDepthComparer depth = new pDrawableDepthComparer();
        public static float UniversalDim;

        private bool forwardPlayOptimisedAdd;
        internal bool ForwardPlayOptimisedAdd
        {
            get { return forwardPlayOptimisedAdd; }
            set
            {
                if (forwardPlayOptimisedAdd && !value)
                {
                    if (ForwardPlayList.Count > 0)
                    {
                        if (SpriteQueue == null)
                            SpriteQueue = new Queue<pDrawable>(ForwardPlayList);
                        else
                            foreach (pDrawable p in ForwardPlayList)
                                SpriteQueue.Enqueue(p);
                        ForwardPlayList.Clear();
                    }
                }
                forwardPlayOptimisedAdd = value;
            }
        }

        internal pList<pDrawable> ForwardPlayList = new pList<pDrawable>() { UseBackwardsSearch = true };

        internal void Add(pDrawable sprite)
        {
            if (ForwardPlayOptimisedAdd && sprite.Transformations.Count > 0)
            {
                ForwardPlayList.AddInPlace(sprite);
                return;
            }

            sprite.SpriteManager = this;

            int pos = Sprites.BinarySearch(sprite, depth);

            if (pos < 0) pos = ~pos;

            Sprites.Insert(pos, sprite);
        }

        internal void Add(List<pDrawable> sprites)
        {
            foreach (pDrawable p in sprites)
                Add(p); //todo: can optimise this when they are already sorted in depth order.
        }

        internal void Add(pSpriteCollection collection)
        {
            foreach (pDrawable p in collection.Sprites)
                Add(p); //todo: can optimise this when they are already sorted in depth order.
        }

        internal Queue<pDrawable> SpriteQueue;
        internal void OptimizeTimeline(ClockTypes clock)
        {
            List<pDrawable> optimizableSprites = Sprites.FindAll(s => s.Transformations.Count > 0 && !s.AlwaysDraw && s.Clocking == clock);

            //sort all sprites in order of first transformation.
            optimizableSprites.Sort((a, b) => { return a.Transformations[0].StartTime.CompareTo(b.Transformations[0].StartTime); });

            if (SpriteQueue == null)
            {
                SpriteQueue = new Queue<pDrawable>(optimizableSprites);
                optimizableSprites.ForEach(s => Sprites.Remove(s));
            }
            else
            {
                foreach (pSprite p in optimizableSprites)
                {
                    SpriteQueue.Enqueue(p);
                    Sprites.Remove(p);
                }
            }
        }

        bool firstRender = true;
        List<int> removableSprites = new List<int>();


        int lastUpdate;

        /// <summary>
        ///   Update all sprites managed by this sprite manager.
        /// </summary>
        public override void Update()
        {
            base.Update();

            lastUpdate = Clock.Time;

            if (SpriteQueue != null)
            {
                do
                {
                    if (SpriteQueue.Count == 0)
                    {
                        //we ran out of sprites in the queue. throw away queue and leave.
                        SpriteQueue = null;
                        break;
                    }

                    pDrawable topSprite = SpriteQueue.Peek();


                    if (topSprite.Transformations.Count == 0)
                        SpriteQueue.Dequeue(); //throw away; transformations got removed before we even got around to displaying.
                    else if (topSprite.Transformations[0].StartTime <= Clock.GetTime(topSprite.Clocking))
                        Add(SpriteQueue.Dequeue());
                    else
                        break;
                }
                while (true);
            }

            int i = 0;
            foreach (pDrawable p in Sprites)
            {
                p.Update();
                if (p.IsRemovable)
                {
                    removableSprites.Add(i);
                    p.Dispose();
                }
                i++;
            }

#if FULLER_DEBUG
            if (Sprites.Count > 5)
                DebugOverlay.AddLine("SpriteManager: tracking " + Sprites.Count + " sprites (" + Sprites.FindAll(s => s.IsOnScreen).Count + " on-screen)");
#endif

            for (i = removableSprites.Count - 1; i >= 0; i--)
                Sprites.RemoveAt(removableSprites[i]);
            removableSprites.Clear();
        }

        static BlendingFactorDest lastBlendDest = BlendingFactorDest.One;
        static BlendingFactorSrc lastBlendSrc = BlendingFactorSrc.OneMinusSrcAlpha;

        internal static void SetBlending(BlendingFactorSrc src, BlendingFactorDest dst)
        {
            if (lastBlendDest == dst && lastBlendSrc == src)
                return;

            lastBlendSrc = src;
            lastBlendDest = dst;

            GL.BlendFunc(lastBlendSrc, lastBlendDest);
        }

        void addToBatch(pDrawable p)
        {
            //todo: implement batching.
        }

        void flushBatch()
        {
            //todo: implement batching.
        }

        /// <summary>
        ///   Draw all sprites managed by this sprite manager.
        /// </summary>
        public override bool Draw()
        {
            if (!base.Draw()) return false;

            pTexture currentBatchTexture = null;
            TexturesEnabled = false;

            matrixOperations = Rotation != 0 || ScaleScalar != 1 || FieldPosition != Vector2.Zero;

            if (matrixOperations)
            {
                GL.Translate(GameBase.NativeSize.Width / 2f, GameBase.NativeSize.Height / 2f, 0);
                if (Rotation != 0)
                    GL.Rotate(Rotation / (float)Math.PI * 180, 0, 0, 1);
                if (ScaleScalar != 1)
                    GL.Scale(Scale.X, Scale.Y, 0);
                GL.Translate(-GameBase.NativeSize.Width / 2f, -GameBase.NativeSize.Height / 2f, 0);

                if (FieldPosition != Vector2.Zero)
                    GL.Translate(FieldPosition.X, FieldPosition.Y, 0);
            }

            float tempAlpha = 0;

            foreach (pDrawable p in Sprites)
            {
                if (p.Alpha > 0)
                {
                    SetBlending(p.Premultiplied ? BlendingFactorSrc.One : BlendingFactorSrc.SrcAlpha, p.BlendingMode);

                    if (Alpha < 1)
                    {
                        tempAlpha = p.Alpha;
                        p.Alpha *= Alpha;
                    }

                    TexturesEnabled = p.UsesTextures;
                    AlphaBlend = p.AlphaBlend || p.Alpha != 1;

                    if (p.Draw())
                    {
                        //todo: implement batching!

                        //pSprite ps = p as pSprite;
                        //if (ps != null)
                        //{
                        //    if (ps.Texture != currentBatchTexture)
                        //    {
                        //        //this texture is different from the current batch; we will need to flush and render fresh.
                        //        flushBatch();
                        //        currentBatchTexture = ps.Texture;
                        //    }

                        //    addToBatch(ps);
                        //}
                    }

                    if (Alpha < 1)
                        p.Alpha = tempAlpha;
                }
            }

            if (matrixOperations)
                GameBase.Instance.SetViewport();

            flushBatch();

            return true;
        }

        private bool matrixOperations;

        static bool texturesEnabled = false;
        internal static bool TexturesEnabled
        {
            get { return texturesEnabled; }

            set
            {
                if (texturesEnabled == value)
                    return;

                texturesEnabled = value;

                if (texturesEnabled)
                {
                    GL.Enable(EnableCap.Texture2D);
                    GL.EnableClientState(ArrayCap.TextureCoordArray);
                }
                else
                {
                    GL.Disable(EnableCap.Texture2D);
                    GL.DisableClientState(ArrayCap.TextureCoordArray);
                }
            }
        }

        static bool alphaBlend = false;
        internal static bool AlphaBlend
        {
            get { return alphaBlend; }

            set
            {
                if (alphaBlend == value)
                    return;

                alphaBlend = value;

                if (alphaBlend)
                {
                    GL.Enable(EnableCap.Blend);
                }
                else
                {
                    GL.Disable(EnableCap.Blend);
                }
            }
        }


        internal static void Reset()
        {
            texturesEnabled = true;
            TexturesEnabled = false; //force a reset
            SetBlending(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
            TextureGl.Reset();
        }

        /// <summary>
        ///   Used by spinners.  Has a range of 0-0.2
        /// </summary>
        /// <param name = "number"></param>
        /// <returns></returns>
        static internal float drawOrderFwdLowPrio(float number)
        {
            return (number % 200000) / 1000000;
        }

        /// <summary>
        ///   Used by hit values.  Has a range of 0.8-1 and loops every 10000 seconds (over 1 hour).
        /// </summary>
        /// <param name = "number"></param>
        /// <returns></returns>
        static internal float drawOrderFwdPrio(float number)
        {
            return 0.8f + (number % 6000000) / 30000000;
        }

        /// <summary>
        ///   Used by hitcircles.  Has a range of 0.8-0.2 and loops every 6000 seconds (1 hour).
        /// </summary>
        /// <param name = "number"></param>
        /// <returns></returns>
        static internal float drawOrderBwd(float number)
        {
            return 0.8f - (number % 6000000) / 10000000;
        }


        public override void Dispose()
        {
            base.Dispose();

            if (Sprites != null)
            {
                foreach (pDrawable p in Sprites)
                    p.Dispose();
            }

            InputManager.OnMove -= HandleInputManagerOnMove;
            InputManager.OnDown -= HandleInputManagerOnDown;
            InputManager.OnUp -= HandleInputManagerOnUp;

            Sprites = null;
        }
    }
}
