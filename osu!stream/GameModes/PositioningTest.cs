using OpenTK;
using OpenTK.Graphics;
using osum.Graphics;
using osum.Graphics.Sprites;
using osum.Input;
using osum.Input.Sources;

namespace osum.GameModes
{
    internal class PositioningTest : GameMode
    {
        public override void Initialize()
        {
            InputManager.OnDown += InputManager_OnDown;

            spriteManager = new SpriteManagerDraggable();

            pointAt(new Vector2(GameBase.BaseSizeFixedWidth.X - 1, GameBase.BaseSizeFixedWidth.Y - 1));
            pointAt(new Vector2(GameBase.BaseSize.X - 1, GameBase.BaseSize.Y - 1));
        }

        private void pointAt(Vector2 vector2)
        {
            pSprite sp = new pSprite(TextureManager.Load(OsuTexture.finger_inner), vector2)
            {
                Origin = OriginTypes.Centre,
                Colour = new Color4(50, 50, 50, 255),
                ScaleScalar = 0.5f
            };

            sp.OnHover += delegate { sp.FadeColour(Color4.White, 100); };
            sp.OnHoverLost += delegate { sp.FadeColour(new Color4(50, 50, 50, 255), 100); };

            pText text = new pText(vector2.ToString(), 12, vector2, 1, true, Color4.White)
            {
                Origin = OriginTypes.Centre
            };

            spriteManager.Add(text);
            spriteManager.Add(sp);
        }

        private void InputManager_OnDown(InputSource source, TrackingPoint trackingPoint)
        {
            pRectangle rect = new pRectangle(Vector2.Zero, new Vector2(GameBase.NativeSize.Width, GameBase.NativeSize.Height), false, 0.2f, Color4.White);
            rect.FadeOutFromOne(500);
            spriteManager.Add(rect);

            pointAt(trackingPoint.BasePosition / GameBase.InputToFixedWidthAlign);
        }
    }
}