using OpenTK;
using OpenTK.Graphics;
using osum.Audio;
using osum.GameplayElements;
using osum.GameplayElements.HitObjects;
using osum.Graphics.Sprites;
using osum.Helpers;

namespace osum.GameModes.Play
{
    public class PlayTest : Player
    {
        public static int StartTime;
        public static bool AllowStreamSwitch = true;
        public static Difficulty InitialDifficulty;
        public static int InitialHp;
        private pText currentTime;

        public override void Dispose()
        {
            if (AudioEngine.Music != null)
            {
                AudioEngine.Music.Stop();
                Beatmap.Dispose();
            }
            base.Dispose();
        }

        public override void Initialize()
        {
            Director.ActiveTransition = null; //can get in a weird state if we don't reset this.
            
            Autoplay = true;

            base.Initialize();
            if (AudioEngine.Music != null && StartTime > 0)
                AudioEngine.Music.SeekTo(StartTime);
            foreach (pList<HitObject> h in HitObjectManager.StreamHitObjects)
            {
                if (h == null) continue;
                h.RemoveAll(ho => ho.StartTime < StartTime);
            }

            currentTime = new pText("", 20, new Vector2(5, 0), 1, true, Color4.White)
            {
                Field = FieldTypes.StandardSnapCentre,
                Origin = OriginTypes.Centre
            };
            topMostSpriteManager.Add(currentTime);
        }

        public override void Update()
        {
            currentTime.Text = Clock.AudioTime + "ms";
            base.Update();
        }

        protected override void initializeUIElements()
        {
            base.initializeUIElements();
        }

        protected override void InitializeStream()
        {
            HitObjectManager.SetActiveStream(InitialDifficulty);
        }

        protected override void hitObjectManager_OnScoreChanged(ScoreChange change, HitObject hitObject)
        {
            base.hitObjectManager_OnScoreChanged(change, hitObject);

            if (InitialHp == 0)
                healthBar.ReduceCurrentHp(100);
            if (InitialHp == 200)
                healthBar.IncreaseCurrentHp(100);
        }

        protected override void UpdateStream()
        {
            if (!AllowStreamSwitch) return;
            base.UpdateStream();
        }
    }
}
