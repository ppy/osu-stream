using System;
using osum.GameplayElements.Scoring;
using osum.Graphics.Sprites;
using osum.Graphics.Skins;
using osum.Helpers;
using OpenTK;
using OpenTK.Graphics;
using System.Collections;
using System.Collections.Generic;
namespace osum.GameModes
{
	public class Ranking : GameMode
	{
		internal static Score RankableScore;
		
		pSprite fill1;
		pSprite fill2;
		pSprite fill3;
		
		List<pSprite> fillSprites = new List<pSprite>();
		
		float actualSpriteScaleX;
		
		internal override void Initialize()
		{
			//add a temporary button to allow returning to song select
			pSprite backButton = new pSprite(TextureManager.Load("menu-back"),FieldTypes.StandardSnapBottomLeft, OriginTypes.BottomLeft,
			                         ClockTypes.Game, Vector2.Zero, 1, true, new Color4(1,1,1,0.4f));
			
			backButton.OnClick += delegate {
				backButton.UnbindAllEvents();
				Director.ChangeMode(OsuMode.SongSelect);
			};
			
			spriteManager.Add(backButton);
			
			float ratio300 = (float)RankableScore.count300 / RankableScore.totalHits;
			float ratio100 = (float)RankableScore.count100 / RankableScore.totalHits;
			float ratio50 = (float)RankableScore.count50 / RankableScore.totalHits;
			
			fill1 = pSprite.FullscreenWhitePixel;
			fill2 = pSprite.FullscreenWhitePixel;
			fill3 = pSprite.FullscreenWhitePixel;
			
			fill1.Colour = new Color4(1,0.63f,0.01f,1);
			fill2.Colour = new Color4(0.55f,0.84f,0,1);
			fill3.Colour = new Color4(0.50f,0.29f,0.635f,1);
			
			fillSprites.Add(fill1);
			fillSprites.Add(fill2);
			fillSprites.Add(fill3);
			
			const int start_colour_change = 3000;
			const int colour_change_length = 700;
			
			const int end_bouncing = 4000;
			
			const int time_between_fills = 500;
			
			int i = 0;
			foreach (pSprite p in fillSprites)
			{
				p.Alpha = 1;
				p.AlwaysDraw = true;
				
				int offset = i++ * time_between_fills;
				
				p.Transform(new Transformation(Color4.LightGray, Color4.LightGray, 0, start_colour_change + offset));
				p.Transform(new Transformation(Color4.White, p.Colour, start_colour_change + offset, start_colour_change + colour_change_length + offset));
				//force the initial colour to be an ambiguous gray.
				
			}
				  
			actualSpriteScaleX = fill1.Scale.X;
			
			fill1.Transform(new TransformationBounce(0, end_bouncing, fill1.Scale.Y * ratio300, fill1.Scale.Y * ratio300, 1));
			fill2.Transform(new TransformationBounce(time_between_fills, end_bouncing, fill2.Scale.Y * ratio100, fill2.Scale.Y * ratio100, 1));
			fill3.Transform(new TransformationBounce(time_between_fills * 2, end_bouncing, fill3.Scale.Y * ratio50, fill3.Scale.Y * ratio50, 1));
			
			spriteManager.Add(fillSprites);
		}
		
		public Ranking()
		{
		}
		
		public override void Draw()
		{
			base.Draw();
		}
		
		public override void Update()
		{
			base.Update();
			
			//set the x scale back to the default value (override the bounce transformation).
			foreach (pSprite p in fillSprites)
				p.Scale.X = actualSpriteScaleX / 2;
			
			fill2.Position.Y = 1 + fill1.Position.Y + fill1.Scale.Y * GameBase.SpriteRatioToWindowBase;
			fill3.Position.Y = 1 + fill2.Position.Y + fill2.Scale.Y * GameBase.SpriteRatioToWindowBase;
		}
		
	}
}

