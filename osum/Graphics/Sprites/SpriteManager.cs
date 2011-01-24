using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Helpers;
using osum.Support;

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

        internal void Add(pDrawable sprite)
        {
            //todo: make this more efficient. .Contains() is slow with a lot of items in the list.
            //if (!sprites.Contains(sprite))

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

        /// <summary>
        ///   Update all sprites managed by this sprite manager.
        /// </summary>
        internal void Update()
        {
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
			
			List<int> removable = new List<int>();
			
			int i = 0;
            foreach (pDrawable p in Sprites)
			{
                p.Update();
				if (p.IsRemovable)
					removable.Add(i);
				i++;
			}

#if DEBUG
            if (Sprites.Count > 5)
                DebugOverlay.AddLine("SpriteManager: tracking " + Sprites.Count + " sprites");
#endif
			
			for (i = removable.Count - 1; i >= 0; i--)
				Sprites.RemoveAt(removable[i]);
        }

        /// <summary>
        ///   Draw all sprites managed by this sprite manager.
        /// </summary>
        internal bool Draw()
        {
            foreach (pDrawable p in Sprites)
                if (p.Alpha > 0) p.Draw();
            return true;
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
