using System;
using osum.GameplayElements.Scoring;
using osum.Graphics.Sprites;
using osum.Graphics.Skins;
using osum.Helpers;
using OpenTK;
using OpenTK.Graphics;
namespace osum.GameModes
{
	public class Ranking : GameMode
	{
		internal static Score RankableScore;
		
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
		}
		
		public Ranking()
		{
			
		}
	}
}

