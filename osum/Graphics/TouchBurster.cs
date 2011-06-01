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
                burstSprites.Add(burst);
                spriteManager.Add(burst);
            }

            InputManager.OnDown += InputManager_OnDown;

            base.Initialize();
        }

        void InputManager_OnDown(InputSource source, TrackingPoint trackingPoint)
        {
            int burstSpriteCount = 5;

            while (burstSpriteCount-- > 0)
            {
                int randTime = 500 + (int)(GameBase.Random.NextDouble() * 400);
                float randX = (float)(GameBase.Random.NextDouble() * 100);
                float randY = (float)(GameBase.Random.NextDouble() * 100);

                Vector2 start = trackingPoint.BasePosition;
                Vector2 end = trackingPoint.BasePosition + new Vector2(randX - 50, randY - 50);

                pSprite burst = burstSprites[nextBurstSprite];

                burst.Transformations.Clear();
                burst.Transform(new Transformation(start, end, Clock.Time, Clock.Time + randTime, EasingTypes.In));
                burst.Transform(new Transformation(TransformationType.Scale, 0.8f + 0.4f * (float)GameBase.Random.NextDouble(), 0.4f + 0.4f * (float)GameBase.Random.NextDouble(), Clock.Time, Clock.Time + randTime, EasingTypes.In));
                burst.Transform(new Transformation(TransformationType.Fade, (float)GameBase.Random.NextDouble(), 0, Clock.Time, Clock.Time + randTime));

                nextBurstSprite = (nextBurstSprite + 1) % burstSprites.Count;
            }
        }

        public override void Dispose()
        {
            InputManager.OnDown -= InputManager_OnDown;

            base.Dispose();
        }

        public override void Update()
        {
            base.Update();
        }
    }
}
