using System;
using osum.Graphics.Sprites;
using osum.GameplayElements.Beatmaps;
using System.IO;
using OpenTK;
using OpenTK.Graphics;
using osum.Helpers;
namespace osum.GameModes.SongSelect
{
	internal class BeatmapPanel : pSpriteCollection
	{
		Beatmap beatmap;
		
		pSprite backingPlate;
		pSprite text;
		
		internal BeatmapPanel(Beatmap beatmap)
		{
			backingPlate = pSprite.FullscreenWhitePixel;
			backingPlate.Alpha = 1;
			backingPlate.AlwaysDraw = true;
			backingPlate.Colour = Color4.OrangeRed;
			backingPlate.Scale.Y = 80;
			backingPlate.Scale.X *= 0.8f;
			backingPlate.DrawDepth = 0.8f;
			SpriteCollection.Add(backingPlate);
			
			this.beatmap = beatmap;
			
            backingPlate.OnClick += delegate {
				
				backingPlate.MoveTo(backingPlate.Position - new Vector2(50,0),600);
				backingPlate.Transform(new Transformation(TransformationType.VectorScale,backingPlate.Scale, 
				                                          new Vector2(backingPlate.Scale.X * 1.2f, backingPlate.Scale.Y),
				                                          backingPlate.ClockingNow, backingPlate.ClockingNow + 600));
				
                backingPlate.UnbindAllEvents();

                Player.SetBeatmap(beatmap);
                Director.ChangeMode(OsuMode.Play);
            };
			
			backingPlate.HandleClickOnUp = true;

            backingPlate.OnHover += delegate { backingPlate.Colour = Color4.YellowGreen; };
            backingPlate.OnHoverLost += delegate { backingPlate.Colour = Color4.OrangeRed; };
			
			text = new pText(Path.GetFileNameWithoutExtension(beatmap.BeatmapFilename), 18, Vector2.Zero, Vector2.Zero, 1, true, Color4.White, false);
			SpriteCollection.Add(text);
		}
		
		internal void MoveTo(Vector2 location)
		{
			text.MoveTo(location + new Vector2(0,10),400);
			backingPlate.MoveTo(location,400);
		}
	}
}

