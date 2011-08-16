using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using osum.Audio;
using osum.GameModes;
using osum.GameplayElements.Beatmaps;
using osum.Graphics.Sprites;
using osum.Graphics.Skins;
using osum.Helpers;
using osum.GameModes.SongSelect;
using OpenTK.Graphics;
using osum.GameModes.Play.Components;
using osum.Graphics.Drawables;
using osum.GameplayElements;
using System.Threading;
using osum.Graphics.Renderers;
using osu_common.Libraries.Osz2;
using osum.Resources;
using osum.GameplayElements.Scoring;
using osum.Graphics;

namespace osum.GameModes
{
    public partial class SongSelectMode : GameMode
    {
        SpriteManagerDraggable rankingSpriteManager;

        void Ranking_Show()
        {
            State = SelectState.RankingDisplay;

            spriteManagerDifficultySelect.FadeOut(200);

            if (rankingSpriteManager != null)
                rankingSpriteManager.Clear();
            else
                rankingSpriteManager = new SpriteManagerDraggable();

            footerHide();

            GameBase.ShowLoadingOverlay = true;

            s_SongInfo.FadeOut(100);
        }

        void Ranking_Hide()
        {
            spriteManagerDifficultySelect.FadeIn(200);
            rankingSpriteManager.FadeOut(200);

            GameBase.ShowLoadingOverlay = false;

            showDifficultySelection2();
        }
    }
}
