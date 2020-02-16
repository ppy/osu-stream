using System;
using OpenTK;
using OpenTK.Graphics;
using osum.GameModes.Play.Components;
using osum.GameModes.SongSelect;
using osum.GameplayElements;
using osum.Graphics.Sprites;
using osum.Helpers;
using osum.Support;

namespace osum.GameModes.Play
{
    internal class PreviewPlayer : Player
    {
        private pText t_currentStream;

        public override void Initialize()
        {
            base.Initialize();

            if (HitObjectManager != null)
            {
                t_currentStream = new pText(HitObjectManager.ActiveStream.ToString(), 64, new Vector2(20, 20), 1, true, Color4.White);
                t_currentStream.Field = FieldTypes.StandardSnapBottomRight;
                t_currentStream.Origin = OriginTypes.BottomRight;
                t_currentStream.TextShadow = true;
                spriteManager.Add(t_currentStream);

                topMostSpriteManager.Add(new BackButton(delegate { Director.ChangeMode(OsuMode.Store); }, false));
            }
        }

        protected override bool CheckForCompletion()
        {
            if (HitObjectManager.AllNotesHit && !Director.IsTransitioning && !Completed)
            {
                Completed = true;
                Director.ChangeMode(OsuMode.Store, new FadeTransition(3000, FadeTransition.DEFAULT_FADE_IN));
                return true;
            }

            return false;
        }

        public override void Dispose()
        {
            Beatmap = null;
            base.Dispose();
        }

        protected override void initializeUIElements()
        {
            streamSwitchDisplay = new StreamSwitchDisplay();
            countdown = new CountdownDisplay();

            progressDisplay = new ProgressDisplay();
        }

        protected override void UpdateStream()
        {
            if (HitObjectManager != null && !HitObjectManager.StreamChanging)
                switchStream(true);
            else
            {
#if DEBUG
                DebugOverlay.AddLine("Stream changing at " + HitObjectManager.nextStreamChange + " to " + HitObjectManager.ActiveStream);
#endif
                playfieldBackground.Move((isIncreasingStream ? 1 : -1) * Math.Max(0, (2000f - (queuedStreamSwitchTime - Clock.AudioTime)) / 200));
            }
        }

        protected override void loadBeatmap()
        {
            HitObjectManager = new HitObjectManagerPreview(Beatmap);

            HitObjectManager.OnScoreChanged += hitObjectManager_OnScoreChanged;
            HitObjectManager.OnStreamChanged += hitObjectManager_OnStreamChanged;

            try
            {
                if (Beatmap.Package != null)
                    HitObjectManager.LoadFile();
            }
            catch
            {
                if (HitObjectManager != null) HitObjectManager.Dispose();
                HitObjectManager = null;
                //if this fails, it will be handled later on in Initialize()
            }

            Difficulty = Difficulty.Easy;
            //force back to stream difficulty, as it may be modified during load to get correct AR etc. variables.
        }

        protected override void hitObjectManager_OnStreamChanged(Difficulty newStream)
        {
            base.hitObjectManager_OnStreamChanged(newStream);
            t_currentStream.Text = HitObjectManager.ActiveStream.ToString();
        }
    }
}
