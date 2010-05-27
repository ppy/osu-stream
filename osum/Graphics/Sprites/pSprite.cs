using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using OpenTK;
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
        private Color colour;
        private float rotation;
        private SpriteEffect effect;
        private BlendingFactorDest blending;

        public pSprite(string path, Vector2 position, Vector2 origin, Color colour, Vector2 scale, float rotation, float alpha)
            : this(SkinManager.LoadTexture(path), position, origin, Color.FromArgb((int)(alpha*255), colour.R, colour.G, colour.B), scale, rotation)
        {
        }

        public pSprite(string path, Vector2 position, Vector2 origin, Color colour, Vector2 scale, float rotation)
            : this(SkinManager.LoadTexture(path), position, origin, colour, scale, rotation)
        {
        }

        public pSprite(pTexture texture, Vector2 position, Vector2 origin, Color colour, Vector2 scale, float rotation, float alpha)
            : this(texture, position, origin, Color.FromArgb((int)(alpha * 255), colour.R, colour.G, colour.B), scale, rotation)
        {
        }

        public pSprite(pTexture texture, Vector2 position, Vector2 origin, Color colour, Vector2 scale, float rotation)
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
            // furui henka he "sayounara" tte
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
                            colour = t.CurrentColour;
                            break;

                        case TransformType.Fade:
                            colour = Color.FromArgb((int)(t.CurrentFloat * 255), colour.R, colour.G, colour.B);
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
