using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.GameModes;
using osum.Graphics.Sprites;
using osum.Graphics.Skins;
using osum.Helpers;
using OpenTK.Graphics;
using OpenTK;

namespace osum.Graphics
{
    class TouchBurster : GameComponent
    {
        List<pSprite> burstSprites = new List<pSprite>();

       static float[] random = new float[] {
            0.54668277924985f,
            0.63373556682948f,
            0.52338286143199f,
            0.58051766265361f,
            0.36986539137682f,
            0.22496573513127f,
            0.56130552458487f,
            0.41475260860106f,
            0.51371260619368f,
            0.8843259537782f,
            0.69453112710452f,
            0.80740271434236f,
            0.092567767498132f,
            0.65969450403575f,
            0.20929404448393f,
            0.65064912941003f,
            0.84573072457247f,
            0.16164668727024f,
            0.43498435995396f,
            0.77836153829616f,
            0.15949302103904f,
            0.79974378282781f,
            0.35293735193193f
        };

        static int nextRandIndex;

        static float nextRand()
        {
#if iOS
            return random[nextRandIndex++ % random.Length];
#else
            return (float)GameBase.Random.NextDouble();
#endif
        }

        int nextBurstSprite;

        public TouchBurster(bool bindInput, Color4 tint)
        {
            BindInput = bindInput;
            Tint = tint;
            if (Tint != Color4.White)
                burstSprites.ForEach(p => p.Colour = tint);
        }

#if iOS
        const int MAX_BURST = 32;
#else
        const int MAX_BURST = 512;
#endif

        public override void Initialize()
        {
            for (int i = 0; i < MAX_BURST; i++)
            {
                pSprite burst = new pSprite(TextureManager.Load(OsuTexture.mouse_burst), FieldTypes.Standard, OriginTypes.Centre, ClockTypes.Game, Vector2.Zero, 1, true, Color4.White);
                burst.Additive = true;
                burst.Alpha = 0;
                burstSprites.Add(burst);
                spriteManager.Add(burst);

                burst.RemoveOldTransformations = false;
                burst.AlignToSprites = false;

                //make transformations beforehand to avoid creating many.
                burst.Transform(new TransformationV() { Type = TransformationType.Movement },
                    new TransformationF() { Type = TransformationType.Scale },
                    new TransformationF() { Type = TransformationType.Fade });
            }

            base.Initialize();
        }

        int spacing;

        private bool bindInput;
        private Color4 Tint;
        private bool BindInput
        {
            get { return bindInput; }
            set
            {
                if (value == BindInput)
                    return;
                bindInput = value;
                if (bindInput)
                {
                    InputManager.OnDown += InputManager_OnDown;
                    InputManager.OnMove += InputManager_OnMove;
                }
                else
                {
                    InputManager.OnDown -= InputManager_OnDown;
                    InputManager.OnMove -= InputManager_OnMove;
                }
            }
        }

        int lastBurstMs = 0;
        
        internal void InputManager_OnMove(InputSource source, TrackingPoint trackingPoint)
        {
#if iOS
            if (InputManager.IsPressed && spacing++ % 1 == 0)
                Burst(trackingPoint.BasePosition, 20, 0.5f, 1);
#else
            if (InputManager.IsPressed || source == null)
                Burst(trackingPoint.BasePosition, 20, 0.5f, 4);
#endif
        }

        internal void InputManager_OnDown(InputSource source, TrackingPoint trackingPoint)
        {
#if iOS
            Burst(trackingPoint.BasePosition, 100, 1, 5);
#else
            Burst(trackingPoint.BasePosition, 100, 1, 30);
#endif
        }

        internal void Burst(Vector2 pos, float spread = 100, float scale = 1, int count = 5)
        {
            int now = Clock.Time;
            if (now - lastBurstMs < 10)
                return;
            lastBurstMs = now;

            while (count-- > 0)
            {
                int randTime = 300 + (int)(nextRand() * 400);

                Vector2 end = pos;

                if (spread > 0)
                {
                    float randX = (float)(nextRand() * spread);
                    float randY = (float)(nextRand() * spread);

                    end += new Vector2(randX - spread / 2, randY - spread / 2);
                }

                pSprite burst = burstSprites[nextBurstSprite];

                TransformationV tPos = (TransformationV)burst.Transformations[0];
                TransformationF tScale = (TransformationF)burst.Transformations[1];
                TransformationF tFade = (TransformationF)burst.Transformations[2];

                tPos.StartTime = Clock.Time;
                tPos.EndTime = Clock.Time + randTime;
                tPos.StartVector = pos;
                tPos.EndVector = end;

                tScale.StartTime = Clock.Time;
                tScale.EndTime = Clock.Time + randTime;
                tScale.StartFloat = scale * 0.8f + 0.4f * (float)nextRand();
                tScale.EndFloat = scale * 0.4f + 0.4f * (float)nextRand();

                tFade.StartTime = Clock.Time;
                tFade.EndTime = Clock.Time + randTime;
                tFade.StartFloat = (float)nextRand() * 0.6f;
                tFade.EndFloat = 0;

                nextBurstSprite = (nextBurstSprite + 1) % burstSprites.Count;
            }
        }

        public override void Dispose()
        {
            BindInput = false;
            base.Dispose();
        }

        public override void Update()
        {
            base.Update();
        }
    }
}
