using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.Graphics.Sprites;
using osum.Graphics.Skins;
using OpenTK.Graphics;
using OpenTK;

namespace osum.GameModes
{
    class PositioningTest : GameMode
    {
        public override void Initialize()
        {
            InputManager.OnDown += new Helpers.InputHandler(InputManager_OnDown);

            spriteManager = new SpriteManagerDraggable();

            pointAt(new Vector2(GameBase.BaseSizeFixedWidth.Width - 1, GameBase.BaseSizeFixedWidth.Height - 1));
            pointAt(new Vector2(GameBase.BaseSize.Width - 1, GameBase.BaseSize.Height - 1));
        }

        private void pointAt(Vector2 vector2)
        {
            pSprite sp = new pSprite(TextureManager.Load(OsuTexture.finger_inner), vector2)
            {
                Origin = OriginTypes.Centre,
                Colour = new Color4(50,50,50,255),
                ScaleScalar = 0.5f                
            };

            pText text = new pText(vector2.ToString(), 12, vector2, 1, true, Color4.White)
            {
                Origin = OriginTypes.Centre
            };

            spriteManager.Add(text);
            spriteManager.Add(sp);
        }

        void InputManager_OnDown(InputSource source, TrackingPoint trackingPoint)
        {
            pointAt(trackingPoint.BasePosition / GameBase.InputToFixedWidthAlign);
        }

    }
}
