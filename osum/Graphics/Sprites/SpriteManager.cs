using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Helpers;
using osum.Support;
#if IPHONE
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
using System.Drawing;
using osum.Input;
#endif

namespace osum.Graphics.Sprites
{
    internal class SpriteManager : IDisposable
    {
        internal List<pDrawable> Sprites;

        int creationTime = Clock.Time;

        internal SpriteManager()
        {
            this.Sprites = new List<pDrawable>();
        }

        internal SpriteManager(IEnumerable<pDrawable> sprites)
        {
            this.Sprites = new List<pDrawable>(sprites);
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
                    int c = forwardPlayList.Count;
                    if (c > 0)
                    {
                        if (SpriteQueue == null)
                            SpriteQueue = new Queue<pDrawable>(forwardPlayList);
                        forwardPlayList.Clear();
                    }
                }
                forwardPlayOptimisedAdd = value;
            }
        }

        private List<pDrawable> forwardPlayList = new List<pDrawable>();

        internal void Add(pDrawable sprite)
        {
            if (ForwardPlayOptimisedAdd && sprite.Transformations.Count > 0)
            {
                int index = forwardPlayList.BinarySearch(sprite);

                if (index < 0)
                    forwardPlayList.Insert(~index, sprite);
                else
                    forwardPlayList.Insert(index, sprite);

                return;
            }

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
            foreach (pDrawable p in collection.SpriteCollection)
                Add(p); //todo: can optimise this when they are already sorted in depth order.
        }

        internal Queue<pDrawable> SpriteQueue;
        internal void OptimizeTimeline(ClockTypes clock)
        {
            //SpriteQueue = new Queue<pDrawable>(Sprites);
            //Sprites.Clear();

            //return;

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

        /// <summary>
        ///   Update all sprites managed by this sprite manager.
        /// </summary>
        internal void Update()
        {
            texturesEnabled = false; //reset on new frame.
			
			if (firstRender)
            {
                int loadTime = Clock.Time - creationTime;

                foreach (pDrawable p in Sprites)
                    if (p.Clocking == ClockTypes.Game)
                        p.Transformations.ForEach(t => t.Offset(loadTime));

                firstRender = false;
            }

            if (SpriteQueue != null)
            {
                pDrawable topSprite = SpriteQueue.Peek();
                while (topSprite.Transformations[0].StartTime <= Clock.GetTime(topSprite.Clocking))
                {
                    Add(SpriteQueue.Dequeue());

                    if (SpriteQueue.Count == 0)
                    {
                        //we ran out of sprites in the queue. throw away queue and leave.
                        SpriteQueue = null;
                        break;
                    }

                    topSprite = SpriteQueue.Peek();
                }
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

#if DEBUG
            if (Sprites.Count > 5)
                DebugOverlay.AddLine("SpriteManager: tracking " + Sprites.Count + " sprites (" + Sprites.FindAll(s => s.IsOnScreen).Count + " on-screen)");
#endif
			
			for (i = removableSprites.Count - 1; i >= 0; i--)
				Sprites.RemoveAt(removableSprites[i]);

            removableSprites.Clear();
        }
		
		static BlendingFactorDest lastBlend = BlendingFactorDest.OneMinusDstAlpha;
		
        /// <summary>
        ///   Draw all sprites managed by this sprite manager.
        /// </summary>
        internal bool Draw()
        {
			foreach (pDrawable p in Sprites)
			{
                if (p.Alpha > 0)
				{
		            if (lastBlend != p.BlendingMode)
					{
						GL.BlendFunc(BlendingFactorSrc.SrcAlpha, p.BlendingMode);
						lastBlend = p.BlendingMode;
					}

					TexturesEnabled = p.UsesTextures;
					p.Draw();
				}
			}
            return true;
        }
		
		static bool texturesEnabled = false;
		internal static bool TexturesEnabled
		{
			get { return texturesEnabled; }	
			
			set {
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

        internal static void Reset()
        {
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

        public void Dispose()
        {
            foreach (pDrawable p in Sprites)
                p.Dispose();

            Sprites = null;
        }
    }
}
