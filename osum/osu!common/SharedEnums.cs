using System;
using System.Globalization;

namespace osu_common
{
    class osu_common
    {
        internal static NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
   }

    [Flags]
    public enum Mods
    {
        None = 0,
        NoFail = 1,
        Easy = 2,
        NoVideo = 4,
        Hidden = 8,
        HardRock = 16,
        SuddenDeath = 32,
        DoubleTime = 64,
        Relax = 128,
        HalfTime = 256,
        //Taiko = 512,
        Flashlight = 1024,
        Autoplay = 2048,
        SpunOut = 4096,
        Relax2 = 8192,
        LastMod = 16384
    }

    public enum SlotTeams
    {
        Neutral,
        Blue,
        Red
    }

    public enum PlayModes
    {
        Osu = 0,
        Taiko = 1,
        CatchTheBeat = 2
    }

    public enum LinkId
    {
        Set,
        Beatmap,
        Topic,
        Post,
        Checksum
    }

    [Flags]
    public enum Permissions
    {
        None = 0,
        Normal = 1,
        BAT = 2,
        Subscriber = 4
    }

    public enum MatchTypes
    {
        Standard = 0,
        Powerplay = 1
    }

    public enum MatchScoringTypes
    {
        Score = 0,
        Accuracy = 1
    }

    public enum MatchTeamTypes
    {
        HeadToHead = 0,
        //TagCoop = 1
        TagCoop = 1,
        TeamVs = 2,
        TagTeamVs = 3
    }

    public enum SubmissionStatus
    {
        Unknown,
        NotSubmitted,
        Pending,
        EditableCutoff,
        Ranked,
        Approved
    }

    public enum Rankings
    {
        XH,
        SH,
        X,
        S,
        A,
        B,
        C,
        D,
        F,
        N
    } ;

    public enum bStatus
    {
        Idle,
        Afk,
        Playing,
        Editing,
        Modding,
        Multiplayer,
        Watching,
        Unknown,
        Testing,
        Submitting,
        Paused,
        Lobby,
        Multiplaying,
        OsuDirect
    }

    [Flags]
    public enum SkinSource
    {
        None = 0,
        Osu = 1,
        Skin = 2,
        Beatmap = 4,
        Permanent = 8,
        Temporal = 16,
        All = Osu | Skin | Beatmap
    }

    public enum BeatmapDifficulty
    {
        Easy,
        Normal,
        Hard,
        Insane,
        Expert
    }

    public enum AIModType
    {
        All = 0,
        Spacing,
        Snapping,
        Errors,
        Difficulty,
        Style
    }


}