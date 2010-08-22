namespace Un4seen.Bass
{
    using System;

    [Flags]
    public enum BASSSync
    {
        BASS_SYNC_CD_ERROR = 0x3e8,
        BASS_SYNC_CD_SPEED = 0x3ea,
        BASS_SYNC_DOWNLOAD = 7,
        BASS_SYNC_END = 2,
        BASS_SYNC_FREE = 8,
        BASS_SYNC_META = 4,
        BASS_SYNC_MIDI_CUE = 0x10001,
        BASS_SYNC_MIDI_EVENT = 0x10004,
        BASS_SYNC_MIDI_LYRIC = 0x10002,
        BASS_SYNC_MIDI_MARKER = 0x10000,
        BASS_SYNC_MIDI_TEXT = 0x10003,
        BASS_SYNC_MIDI_TICK = 0x10005,
        BASS_SYNC_MIXER_ENVELOPE = 0x10200,
        BASS_SYNC_MIXTIME = 0x40000000,
        BASS_SYNC_MUSICFX = 3,
        BASS_SYNC_MUSICINST = 1,
        BASS_SYNC_MUSICPOS = 10,
        BASS_SYNC_OGG_CHANGE = 12,
        BASS_SYNC_ONETIME = -2147483648,
        BASS_SYNC_POS = 0,
        BASS_SYNC_SETPOS = 11,
        BASS_SYNC_SLIDE = 5,
        BASS_SYNC_STALL = 6,
        BASS_SYNC_WMA_CHANGE = 0x10100,
        BASS_SYNC_WMA_META = 0x10101,
        BASS_WINAMP_SYNC_BITRATE = 100
    }
}

