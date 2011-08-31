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
using osu_common.Libraries.NetLib;

namespace osum.GameModes
{
    public partial class SongSelectMode : GameMode
    {
        SpriteManagerDraggable rankingSpriteManager;

        List<Score> rankingScores;
        StringNetRequest rankingNetRequest;

        void Ranking_Show()
        {
            State = SelectState.RankingDisplay;

            spriteManagerDifficultySelect.FadeOut(200);

            if (rankingSpriteManager != null)
            {
                rankingSpriteManager.Clear();
                rankingSpriteManager.FadeIn(0);
            }
            else
            {
                rankingSpriteManager = new SpriteManagerDraggable();
            }

            footerHide();

            GameBase.ShowLoadingOverlay = true;

            s_SongInfo.FadeOut(100);

            int period = 0;

            rankingNetRequest = new StringNetRequest(@"http://osustream.com/score/retrieve.php", "POST", 
                "udid=" + GameBase.Instance.DeviceIdentifier + 
                "&filename=" + NetRequest.UrlEncode(Path.GetFileName(Player.Beatmap.ContainerFilename)) +
                "&period=" + period +
                "&difficulty=" + (int)Player.Difficulty);

            rankingNetRequest.onFinish += rankingReceived;

            NetManager.AddRequest(rankingNetRequest);
        }

        void rankingReceived(string _result, Exception e)
        {
            rankingNetRequest = null;

            if (e != null || _result == null)
            {
                //error
                Ranking_Hide();
                return;
            }

            rankingScores = new List<Score>();

            foreach (string s in _result.Split('\n'))
            {
                if (s.Length == 0) continue;

                string[] split = s.Split('|');

                int i = 0;

                Score score = new Score()
                {
                    Id = Int32.Parse(split[i++]),
                    Username = split[i++],
                    hitScore = Int32.Parse(split[i++]),
                    comboBonusScore = Int32.Parse(split[i++]),
                    spinnerBonusScore = Int32.Parse(split[i++]),
                    count300 = UInt16.Parse(split[i++]),
                    count100 = UInt16.Parse(split[i++]),
                    count50 = UInt16.Parse(split[i++]),
                    countMiss = UInt16.Parse(split[i++]),
                    maxCombo = UInt16.Parse(split[i++]),
                    date = UnixTimestamp.Parse(Int32.Parse(split[i++]))
                };

                rankingScores.Add(score);
            }

            int index = 0;
            foreach (Score score in rankingScores)
            {
                ScorePanel sp = new ScorePanel(score, onScoreClicked, index + 1);
                sp.Sprites.ForEach(s => s.Position = new Vector2(0, BeatmapPanel.PANEL_HEIGHT + 5 + (ScorePanel.PANEL_HEIGHT + 3) * index));
                
                rankingSpriteManager.Add(sp);

                index++;
            }

            GameBase.ShowLoadingOverlay = false;
        }

        void onScoreClicked(object sender, EventArgs args)
        {
            ScorePanel panel = ((pDrawable)sender).Tag as ScorePanel;
            if (panel == null) return;

            Results.RankableScore = panel.Score;
            Director.ChangeMode(OsuMode.Results);

            AudioEngine.PlaySample(OsuSamples.MenuHit);
        }

        void Ranking_Hide()
        {
            if (rankingNetRequest != null)
            {
                rankingNetRequest.Abort();
                rankingNetRequest = null;
            }

            spriteManagerDifficultySelect.FadeIn(200);
            rankingSpriteManager.FadeOut(200);

            GameBase.ShowLoadingOverlay = false;

            showDifficultySelection2();

            rankingScores = null;
        }
    }
}
