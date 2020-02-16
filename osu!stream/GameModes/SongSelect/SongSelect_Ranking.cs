using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using osum.Audio;
using osum.GameModes.Play;
using osum.GameplayElements.Scoring;
using osum.Graphics.Sprites;
using osum.Helpers;
using osum.Libraries.NetLib;
using osum.Localisation;

namespace osum.GameModes.SongSelect
{
    public partial class SongSelectMode : GameMode
    {
        private SpriteManagerDraggable rankingSpriteManager;

        private List<Score> rankingScores;
        private StringNetRequest rankingNetRequest;

        private void Ranking_Show()
        {
            State = SelectState.RankingDisplay;

            if (spriteManagerDifficultySelect != null)
                spriteManagerDifficultySelect.FadeOut(200);

            if (rankingSpriteManager != null)
            {
                rankingSpriteManager.Clear();
                rankingSpriteManager.FadeIn(0);
                rankingSpriteManager.ScrollTo(0);
            }
            else
            {
                rankingSpriteManager = new SpriteManagerDraggable { StartBufferZone = BeatmapPanel.PANEL_HEIGHT + 5 };
            }

            footerHide();

            GameBase.ShowLoadingOverlay = true;

            if (s_SongInfo != null) s_SongInfo.FadeOut(100);

            int period = 0;

            rankingNetRequest = new StringNetRequest(@"https://osustream.com/score/retrieve.php", "POST",
                "udid=" + GameBase.Instance.DeviceIdentifier +
                "&filename=" + NetRequest.UrlEncode(Path.GetFileName(Player.Beatmap.ContainerFilename)) +
                "&period=" + period +
                "&difficulty=" + (int)Player.Difficulty);

            rankingNetRequest.onFinish += rankingReceived;

            NetManager.AddRequest(rankingNetRequest);
        }

        private void rankingReceived(string _result, Exception e)
        {
            rankingNetRequest = null;

            if (e != null || _result == null)
            {
                //error occurred
                GameBase.Notify(LocalisationManager.GetString(OsuString.InternetFailed), delegate { Ranking_Hide(); });
                return;
            }

            try
            {
                rankingScores = new List<Score>();

                foreach (string s in _result.Split('\n'))
                {
                    if (s.Length == 0) continue;

                    string[] split = s.Split('|');

                    int i = 0;

                    Score score = new Score
                    {
                        Id = int.Parse(split[i++], GameBase.nfi),
                        OnlineRank = int.Parse(split[i++], GameBase.nfi),
                        Username = split[i++],
                        hitScore = int.Parse(split[i++], GameBase.nfi),
                        comboBonusScore = int.Parse(split[i++], GameBase.nfi),
                        spinnerBonusScore = int.Parse(split[i++], GameBase.nfi),
                        count300 = ushort.Parse(split[i++], GameBase.nfi),
                        count100 = ushort.Parse(split[i++], GameBase.nfi),
                        count50 = ushort.Parse(split[i++], GameBase.nfi),
                        countMiss = ushort.Parse(split[i++], GameBase.nfi),
                        maxCombo = ushort.Parse(split[i++], GameBase.nfi),
                        date = UnixTimestamp.Parse(int.Parse(split[i++], GameBase.nfi)),
                        guest = split[i++] == "1"
                    };

                    rankingScores.Add(score);
                }

                int index = 0;
                foreach (Score score in rankingScores)
                {
                    ScorePanel sp = new ScorePanel(score, onScoreClicked);
                    sp.Sprites.ForEach(s => s.Position = new Vector2(0, (ScorePanel.PANEL_HEIGHT + 3) * index));

                    rankingSpriteManager.Add(sp);

                    index++;
                }

                GameBase.ShowLoadingOverlay = false;

                rankingSpriteManager.FadeInFromZero(300);
            }
            catch
            {
                GameBase.Notify(LocalisationManager.GetString(OsuString.InternetFailed), delegate { Ranking_Hide(); });
            }
        }

        private void onScoreClicked(object sender, EventArgs args)
        {
            if (!(((pDrawable)sender).Tag is ScorePanel panel)) return;

            Results.Results.RankableScore = panel.Score;
            Director.ChangeMode(OsuMode.Results, true);

            AudioEngine.PlaySample(OsuSamples.MenuHit);
        }

        private void Ranking_Hide()
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
