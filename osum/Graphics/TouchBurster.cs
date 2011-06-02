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

        int nextBurstSprite;

        internal override void Initialize()
        {
            for (int i = 0; i < 16; i++)
            {
                pSprite burst = new pSprite(TextureManager.Load(OsuTexture.mouse_burst), FieldTypes.Standard, OriginTypes.Centre, ClockTypes.Game, Vector2.Zero, 1, true, Color4.White);
                burst.Additive = true;
                burst.Alpha = 0;
                burst.AlignToSprites = false;
                burstSprites.Add(burst);
                spriteManager.Add(burst);

                burst.RemoveOldTransformations = false;

                //make transformations beforehand to avoid creating many.
                burst.Transform(new Transformation() { Type = TransformationType.Movement },
                    new Transformation() { Type = TransformationType.Scale },
                    new Transformation() { Type = TransformationType.Fade });
            }

            InputManager.OnDown += InputManager_OnDown;
            InputManager.OnMove += InputManager_OnMove;

            base.Initialize();
        }

        int spacing;
        void InputManager_OnMove(InputSource source, TrackingPoint trackingPoint)
        {
            if (InputManager.IsPressed && spacing++ % 1 == 0)
                burst(trackingPoint.BasePosition, 20, 0.5f,  1);
        }

        void InputManager_OnDown(InputSource source, TrackingPoint trackingPoint)
        {
            burst(trackingPoint.BasePosition, 100, 1, 5);
        }

        private void burst(Vector2 pos, float spread, float scale, int count)
        {
            while (count-- > 0)
            {
                int randTime = 300 + (int)(GameBase.Random.NextDouble() * 400);

                Vector2 end = pos;

                if (spread > 0)
                {
                    float randX = (float)(GameBase.Random.NextDouble() * spread);
                    float randY = (float)(GameBase.Random.NextDouble() * spread);

                    end += new Vector2(randX - spread / 2, randY - spread / 2);
                }

                pSprite burst = burstSprites[nextBurstSprite];

                for (int i = 0; i < 3; i++)
                {
                    Transformation t = burst.Transformations[i];

                    t.StartTime = Clock.Time;
                    t.EndTime = Clock.Time + randTime;

                    switch (i)
                    {
                        case 0:
                            t.StartVector = pos;
                            t.EndVector = end;
                            break;
                        case 1:
                            t.StartFloat = scale * 0.8f + 0.4f * (float)GameBase.Random.NextDouble();
                            t.EndFloat = scale * 0.4f + 0.4f * (float)GameBase.Random.NextDouble();
                            break;
                        case 2:
                            t.StartFloat = (float)GameBase.Random.NextDouble() * 0.6f;
                            t.EndFloat = 0;
                            break;
                    }
                }

                nextBurstSprite = (nextBurstSprite + 1) % burstSprites.Count;
            }
        }

        public override void Dispose()
        {
            InputManager.OnDown -= InputManager_OnDown;
            InputManager.OnMove -= InputManager_OnMove;

            base.Dispose();
        }

        public override void Update()
        {
            base.Update();
        }
    }
}
