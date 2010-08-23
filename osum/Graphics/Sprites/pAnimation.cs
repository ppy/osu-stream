using osum.Graphics.Skins;
using osum.Helpers;
using OpenTK;
using OpenTK.Graphics;

namespace osum.Graphics.Sprites
{
    internal enum LoopTypes
    {
        LoopForever,
        LoopOnce
    }

    internal class pAnimation : pSprite
    {
        private int currentFrame;
        private double currentFrameSkip;
        public int[] CustomSequence;
        public bool DrawDimensionsManualOverride;
        internal double frameSkip;
        public LoopTypes LoopType;
        internal bool RunAnimation = true;

        private pTexture[] textureArray;

        internal int TextureCount;

        internal pAnimation(pTexture[] textures, FieldTypes fieldType, OriginTypes originType, ClockTypes clockType,
                            Vector2 startPosition, float drawDepth, bool alwaysDraw, Color4 colour)
            : base(textures == null || textures.Length == 0 ? null : textures[0], fieldType, originType, clockType, startPosition, drawDepth, alwaysDraw, colour)
        {
            if (textures != null)
                TextureArray = textures;
        }

        internal int CurrentFrame
        {
            get { return currentFrame; }
            set
            {
                currentFrame = value;
                UpdateFrame();
            }
        }

        internal pTexture[] TextureArray
        {
            get { return textureArray; }
            set
            {
                textureArray = value;


                if (textureArray != null)
                {
                    TextureCount = textureArray.Length;
                    currentFrame = 0;
                    if (TextureCount > 0)
                        base.texture = textureArray[0];
                    else
                        base.texture = null;
                }
            }
        }

        internal override pTexture Texture
        {
            get
            {
                if (TextureArray == null || TextureCount == 0)
                    return null;
                return texture;
            }
        }

        internal event VoidDelegate AnimationFinished;

        private void InvokeAnimationFinished()
        {
            VoidDelegate a = AnimationFinished;
            if (a != null) a();
        }

        int lastFrame;
        double lastFrameTime;
        bool firstFrame = true;

        private void resetAnimation()
        {
            firstFrame = true;
            currentFrame = 0;
        }

        internal void UpdateFrame()
        {
            if ((!RunAnimation && lastFrame == currentFrame))
            // || !GameBase.SixtyFramesPerSecondFrame || (Clocking == ClockTypes.Audio && AudioEngine.AudioState == AudioStates.Stopped))
                return;

            if (TextureCount < 2)
                return;

            double spriteTime = Clock.GetTime(Clocking);

            if (Transformations.Count > 0 && Transformations[0].StartTime > spriteTime)
            {
                resetAnimation();

                return;
            }

            if (firstFrame)
            {
                firstFrame = false;
                if (Transformations.Count > 0)
                    lastFrameTime = Transformations[0].StartTime;
            }

            double elapsed = spriteTime - lastFrameTime;

            currentFrameSkip = currentFrameSkip + elapsed / Constants.SIXTY_FRAME_TIME;

            lastFrameTime = spriteTime;

            if (frameSkip > 0)
            {
                if (elapsed < 0)
                {
                    //reverse seek occurred..
                    while (currentFrameSkip < 0)
                    {
                        currentFrameSkip += frameSkip;

                        //rewind~
                        increaseCurrentFrame(true);

                    }
                }
                else if (elapsed > 200)
                {
                    //forwards seek occurred...

                    while (currentFrameSkip > 20)
                    {
                        currentFrameSkip -= frameSkip;

                        increaseCurrentFrame(false);
                    }
                }
            }


            if (currentFrameSkip > frameSkip)
            {
                currentFrameSkip -= frameSkip;

                increaseCurrentFrame(false);
            }


            if (lastFrame != currentFrame)
            {
                texture = hasCustomSequence ? TextureArray[CustomSequence[currentFrame]] : TextureArray[currentFrame];

                if (Texture != null)
                {
                    //if (!DrawDimensionsManualOverride)
                    //    UpdateTextureSize();
                    if (Origin != OriginTypes.TopLeft)
                        UpdateTextureAlignment();
                }
            }

            lastFrame = currentFrame;
        }

        private void increaseCurrentFrame(bool reverse)
        {
            if (LoopType == LoopTypes.LoopOnce && currentFrame == maxFrame)
            {
                CustomSequence = null;
                InvokeAnimationFinished();
            }
            else
            {
                if (Reverse) reverse = !reverse;
                if (reverse)
                    currentFrame = (TextureCount + currentFrame - 1) % (maxFrame + 1);
                else
                    currentFrame = (currentFrame + 1) % (maxFrame + 1);
            }
        }

        internal void SetFramerateFromSkin()
        {
            return;
            
            /*
            if (SkinManager.Current == null || textureArray == null) return;

            if (SkinManager.Current.AnimationFramerate > 0)
                frameSkip = (1000f / SkinManager.Current.AnimationFramerate) / Constant.SIXTY_FRAME_TIME;
            else
                frameSkip = (1000f / TextureArray.Length) / Constant.SIXTY_FRAME_TIME;
            */
        }

        public override pSprite Clone()
        {
            pAnimation clone = new pAnimation(TextureArray, Field, Origin, Clocking, StartPosition, DrawDepth, AlwaysDraw, Colour);
            clone.frameSkip = frameSkip;
            foreach (Transformation t in Transformations)
                clone.Transformations.Add(t.Clone());
            return clone;
        }

        internal void SetFrameDelay(float p)
        {
            frameSkip = p / (100 / 6f);
        }

        public bool hasCustomSequence { get { return CustomSequence != null; } }

        public int maxFrame { get { return !hasCustomSequence ? TextureCount - 1 : CustomSequence.Length - 1; } }
    }
}