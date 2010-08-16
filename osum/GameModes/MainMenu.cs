using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using osum.GameModes;
using osum.Graphics.Sprites;
using osum.Graphics.Skins;
using osum.Helpers;
using OpenTK;
using OpenTK.Graphics;
using System.Drawing;

namespace osum.GameModes
{
    class MainMenu : GameMode
    {
		pSprite osuLogo;
		
		internal override void Initialize()
        {
            pSprite menuBackground =
                new pSprite(SkinManager.Load("menu-background"), FieldTypes.StandardSnapCentre, OriginTypes.Centre,
                            ClockTypes.Game, Vector2.Zero, 0, true, Color.White);
            spriteManager.Add(menuBackground);

            osuLogo = new pSprite(SkinManager.Load("menu-osu"), FieldTypes.StandardSnapCentre, OriginTypes.Centre, ClockTypes.Game, Vector2.Zero, 1, true, Color4.White);
            spriteManager.Add(osuLogo);
			
			osuLogo.Transform(new Transformation(TransformationType.Rotation,0,200,0,200000));
			
			//osuLogo.Transform(new Transformation(new Vector2(0,0),new Vector2(1024,768),0,5000));
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Draw()
        {
            base.Draw();
			
			osuLogo.ScaleScalar = 1+GameBase.Instance.backgroundAudioPlayer.CurrentVolume/100;
        }
    }
}