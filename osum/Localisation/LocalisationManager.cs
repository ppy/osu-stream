using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace osum.Resources
{
    public static class LocalisationManager
    {
        private static Dictionary<OsuString, string> strings = new Dictionary<OsuString, string>();
        private static bool initialised;

        public static string GetString(OsuString stringType)
        {
            if (!initialised)
                init();

            return strings[stringType];
        }

        private static void init()
        {
            initialised = true;

            readResources("en");
            string regionalSetting = System.Threading.Thread.CurrentThread.CurrentUICulture.ToString();

            if (regionalSetting == "en") return;

            if (!regionalSetting.StartsWith("zh")) //chinese has sub-localisations for traditional/simplified.
                regionalSetting = regionalSetting.Substring(0, 2);

            readResources(regionalSetting);
        }

        private static void readResources(string p)
        {
            string resFile = "Localisation/" + p + ".txt";
            if (!NativeAssetManager.Instance.FileExists(resFile))
                return;
            using (Stream str = NativeAssetManager.Instance.GetFileStream(resFile))
            using (StreamReader sw = new StreamReader(str))
            {
                while (!sw.EndOfStream)
                {
                    string line = sw.ReadLine();
                    int index = line.IndexOf('=');
                    if (index <= 0) continue;
                    strings[(OsuString)Enum.Parse(typeof(OsuString), line.Remove(index), false)] = line.Substring(index + 1).Replace("\\n", "\n");
                }
            }
        }

    }

    public enum OsuString
    {
        AndHoldUntilTheCircleExplodes,
        WelcomeToTheWorldOfOsu,
        TapToContinue,
        Introduction2,
        Introduction3,
        Introduction4,
        HitCircle1,
        HitCircle2,
        HitCircle3,
        HitCircle4,
        HitCircle4_1,
        HitCircle5,
        Good,
        Great,
        Perfect,
        HitCircle6,
        HitCircleJudge1,
        HitCircleJudge2,
        HitCircleJudge3,
        HitCircleJudge4,
        Hold1,
        Hold2,
        Hold3,
        HoldJudge1,
        HoldJudge2,
        Slider1,
        Slider1_1,
        Slider2,
        Slider2_1,
        Slider3,
        Slider3_1,
        Slider3_2,
        Slider4,
        SliderJudge1,
        SliderJudge2,
        SliderJudge3,
        Spinner1,
        Spinner2,
        Spinner2_1,
        Spinner3,
        Spinner3_1,
        Spinner4,
        SpinnerJudge1,
        SpinnerJudge2,
        SpinnerJudge3,
        Multitouch1,
        Multitouch1_1,
        Multitouch2,
        Thumbs,
        Fingers,
        Multitouch3,
        MultitouchJudge1,
        MultitouchJudge2,
        MultitouchJudge3,
        Stacked1,
        Stacked1_1,
        Stacked2,
        Stacked3,
        StackedJudge1,
        StackedJudge2,
        StackedJudge3,
        StackedJudge4,
        Stream1,
        Stream2,
        Stream3,
        Easy,
        Normal,
        Hard,
        Healthbar1,
        Healthbar2,
        Healthbar3,
        Healthbar4,
        Healthbar5,
        Score1,
        Score2,
        Score3,
        Score4,
        Completion,
        TimingEarly,
        TimingLate,
        TimingVeryEarly,
        TimingVeryLate,
        UseFingerGuides,
        UseGuideFingers_Explanation,
        DefaultToEasyMode,
        DefaultToEasyMode_Explanation,
        MeetTheTwoFingerGuides,
        MaxCombo,
        AvgTiming,
        Score,
        Accuracy,
        Spin,
        Combo,
        Hit,
        DownloadMoreSongs,
        ErrorWhileDownloadingSongListing,
        HaveAllAvailableSongPacks,
        Notice,
        Yes,
        No,
        Okay,
        Alert,
        PlayCount,
        HighScore,
        YouCantFail,
        DynamicStreamSwitching,
        NotForTheFaintHearted,
        ExpertUnlock,
        GameCentreInactive,
        GameCentreInactiveExplanation,
        Loading,
        About,
        Credits,
        OnlineHelp,
        DifficultySettings,
        EffectVolume,
        MusicVolume,
        OnlineOptions,
        GameCentreLoggedIn,
        GameCentreTapToLogin,
        Free,
        Congratulations,
        UnlockedExpert,
        Audio,
        NoticeOnlineRanking,
        PauseInfo,
        FirstRunWelcome,
        FirstRunTutorial,
        MorePractice,
        PutTogether,
        Update,
        ExitTutorial,
        Twitter,
        Cancel,
        TwitterLink,
        TwitterUnlink,
        GuestUsername,
        ChooseUsername,
        InternetFailed,
        Crashed,
        AppleReallyScrewedThisUp1,
        RestorePurchases,
        TwitterLinkError,
        AccessNotGranted,
        AccessNotGrantedDetails,
        TwitterLinkQuestion,
        TwitterLinkQuestionDetails,
        TwitterSuccess,
        TwitterSuccessDetails
    }
}
