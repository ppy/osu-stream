using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using osum.Graphics;
using osum.Graphics.Skins;

namespace osum.Graphics.Sprites
{
    public class pSprite : ISpriteable
    {
        private List<Transform> transformations = new List<Transform>();

        private pTexture texture;
        private Vector2 position, origin, scale;
        private Color4 colour;
        private float rotation;
        private SpriteEffect effect;
        private BlendingFactorDest blending;

        public pSprite(string path, Vector2 position, Vector2 origin, Color4 colour, Vector2 scale, float rotation, float alpha)
            : this(SkinManager.LoadTexture(path), position, origin, new Color4(colour.R, colour.G, colour.B, alpha), scale, rotation)
        {
        }

        public pSprite(string path, Vector2 position, Vector2 origin, Color4 colour, Vector2 scale, float rotation)
            : this(SkinManager.LoadTexture(path), position, origin, colour, scale, rotation)
        {
        }

        public pSprite(pTexture texture, Vector2 position, Vector2 origin, Color4 colour, Vector2 scale, float rotation, float alpha)
            : this(texture, position, origin, new Color4(colour.R, colour.G, colour.B, alpha), scale, rotation)
        {
        }

        public pSprite(pTexture texture, Vector2 position, Vector2 origin, Color4 colour, Vector2 scale, float rotation)
        {
            this.texture = texture;
            this.position = position;
            this.origin = origin;
            this.colour = colour;
            this.scale = scale;
            this.rotation = rotation;
            this.effect = SpriteEffect.None;
            this.blending = BlendingFactorDest.OneMinusSrcAlpha;
        }

        public void Add(Transform transform)
        {
            if (!transformations.Contains(transform))
                transformations.Add(transform);
        }

        public void Update()
        {
            // remove old transformations
            for (int i = 0; i < transformations.Count; i++)
            {
                if (transformations[i].Terminated)
                    transformations.RemoveAt(i);
            }

            // modify variables based on transformations
            for (int i = 0; i < transformations.Count; i++)
            {
                Transform t = transformations[i];

                // reset some values
                effect = SpriteEffect.None;
                blending = BlendingFactorDest.OneMinusSrcAlpha;

                if (t.Initiated)
                {
                    switch (t.Type)
                    {
                        case TransformType.Colour:
                            Color4 c = t.CurrentColour;
                            colour = new Color4(c.R, c.G, c.B, colour.A);
                            break;

                        case TransformType.Fade:
                            colour = new Color4(colour.R, colour.G, colour.B, t.CurrentFloat);
                            break;

                        case TransformType.Movement:
                            position = t.CurrentVector;
                            break;

                        case TransformType.MovementX:
                            position.X = t.CurrentFloat;
                            break;

                        case TransformType.MovementY:
                            position.Y = t.CurrentFloat;
                            break;

                        case TransformType.ParameterAdditive:
                            blending = BlendingFactorDest.One;
                            break;

                        case TransformType.ParameterFlipHorizontal:
                            effect |= SpriteEffect.FlipHorizontally;
                            break;

                        case TransformType.ParameterFlipVertical:
                            effect |= SpriteEffect.FlipVertically;
                            break;

                        case TransformType.Rotation:
                            rotation = t.CurrentFloat;
                            break;

                        case TransformType.Scale:
                            scale = new Vector2(t.CurrentFloat, t.CurrentFloat);
                            break;

                        case TransformType.VectorScale:
                            scale = t.CurrentVector;
                            break;
                    }
                }
            }
        }

        public void Draw()
        {
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, blending);
            texture.TextureGl.Draw(position, origin, colour, scale, rotation, null, effect);
        }
    }
}
