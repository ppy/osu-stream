#region License
//
// The Open Toolkit Library License
//
// Copyright (c) 2006 - 2009 the Open Toolkit library.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights to 
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
#endregion


using System;

namespace OpenTK.Audio
{
	///<summary>Sound samples: Format specifier.</summary>
    public enum ALFormat : int
    {
        ///<summary>1 Channel, 8 bits per sample.</summary>
        Mono8 = 0x1100,

        ///<summary>1 Channel, 16 bits per sample.</summary>
        Mono16 = 0x1101,

        ///<summary>2 Channels, 8 bits per sample each.</summary>
        Stereo8 = 0x1102,

        ///<summary>2 Channels, 16 bits per sample each.</summary>
        Stereo16 = 0x1103,

        /// <summary>1 Channel, A-law encoded data. Requires Extension: AL_EXT_ALAW</summary>
        MonoALawExt = 0x10016,

        /// <summary>2 Channels, A-law encoded data. Requires Extension: AL_EXT_ALAW</summary>
        StereoALawExt = 0x10017,

        /// <summary>1 Channel, µ-law encoded data. Requires Extension: AL_EXT_MULAW</summary>
        MonoMuLawExt = 0x10014,

        /// <summary>2 Channels, µ-law encoded data. Requires Extension: AL_EXT_MULAW</summary>
        StereoMuLawExt = 0x10015,

        /// <summary>Ogg Vorbis encoded data. Requires Extension: AL_EXT_vorbis</summary>
        VorbisExt = 0x10003,

        /// <summary>MP3 encoded data. Requires Extension: AL_EXT_mp3</summary>
        Mp3Ext = 0x10020,

        /// <summary>1 Channel, IMA4 ADPCM encoded data. Requires Extension: AL_EXT_IMA4</summary>
        MonoIma4Ext = 0x1300,

        /// <summary>2 Channels, IMA4 ADPCM encoded data. Requires Extension: AL_EXT_IMA4</summary>
        StereoIma4Ext = 0x1301,

        /// <summary>1 Channel, single-precision floating-point data. Requires Extension: AL_EXT_float32</summary>
        MonoFloat32Ext = 0x10010,

        /// <summary>2 Channels, single-precision floating-point data. Requires Extension: AL_EXT_float32</summary>
        StereoFloat32Ext = 0x10011,

        /// <summary>1 Channel, double-precision floating-point data. Requires Extension: AL_EXT_double</summary>
        MonoDoubleExt = 0x10012,

        /// <summary>2 Channels, double-precision floating-point data. Requires Extension: AL_EXT_double</summary>
        StereoDoubleExt = 0x10013,

        /// <summary>Multichannel 5.1, 16-bit data. Requires Extension: AL_EXT_MCFORMATS</summary>
        Multi51Chn16Ext = 0x120B,

        /// <summary>Multichannel 5.1, 32-bit data. Requires Extension: AL_EXT_MCFORMATS</summary>
        Multi51Chn32Ext = 0x120C,

        /// <summary>Multichannel 5.1, 8-bit data. Requires Extension: AL_EXT_MCFORMATS</summary>
        Multi51Chn8Ext = 0x120A,

        /// <summary>Multichannel 6.1, 16-bit data. Requires Extension: AL_EXT_MCFORMATS</summary>
        Multi61Chn16Ext = 0x120E,

        /// <summary>Multichannel 6.1, 32-bit data. Requires Extension: AL_EXT_MCFORMATS</summary>
        Multi61Chn32Ext = 0x120F,

        /// <summary>Multichannel 6.1, 8-bit data. Requires Extension: AL_EXT_MCFORMATS</summary>
        Multi61Chn8Ext = 0x120D,

        /// <summary>Multichannel 7.1, 16-bit data. Requires Extension: AL_EXT_MCFORMATS</summary>
        Multi71Chn16Ext = 0x1211,

        /// <summary>Multichannel 7.1, 32-bit data. Requires Extension: AL_EXT_MCFORMATS</summary>
        Multi71Chn32Ext = 0x1212,

        /// <summary>Multichannel 7.1, 8-bit data. Requires Extension: AL_EXT_MCFORMATS</summary>
        Multi71Chn8Ext = 0x1210,

        /// <summary>Multichannel 4.0, 16-bit data. Requires Extension: AL_EXT_MCFORMATS</summary>
        MultiQuad16Ext = 0x1205,

        /// <summary>Multichannel 4.0, 32-bit data. Requires Extension: AL_EXT_MCFORMATS</summary>
        MultiQuad32Ext = 0x1206,

        /// <summary>Multichannel 4.0, 8-bit data. Requires Extension: AL_EXT_MCFORMATS</summary>
        MultiQuad8Ext = 0x1204,

        /// <summary>1 Channel rear speaker, 16-bit data. See Quadrophonic setups. Requires Extension: AL_EXT_MCFORMATS</summary>
        MultiRear16Ext = 0x1208,

        /// <summary>1 Channel rear speaker, 32-bit data. See Quadrophonic setups. Requires Extension: AL_EXT_MCFORMATS</summary>
        MultiRear32Ext = 0x1209,

        /// <summary>1 Channel rear speaker, 8-bit data. See Quadrophonic setups. Requires Extension: AL_EXT_MCFORMATS</summary>
        MultiRear8Ext = 0x1207,
    }
	
	
    /// <summary>Describes the format of the SoundData.</summary>
    public struct SoundFormat
    {
        #region --- Constructors ---

        /// <summary>Constructs a new SoundFormat.</summary>
        public SoundFormat(int channels, int bitsPerSample, int sampleRate)
        {
            if (sampleRate <= 0) throw new ArgumentOutOfRangeException("sampleRate", "Must be higher than 0.");

            SampleFormat = 0;

            switch (channels)
            {
                case 1:
                    if (bitsPerSample == 8) SampleFormat = SampleFormat.Mono8;
                    else if (bitsPerSample == 16) SampleFormat = SampleFormat.Mono16;
                    break;

                case 2:
                    if (bitsPerSample == 8) SampleFormat = SampleFormat.Stereo8;
                    else if (bitsPerSample == 16) SampleFormat = SampleFormat.Stereo16;
                    break;
            }
            SampleRate = sampleRate;
        }

        #endregion

        #region --- Public Members ---

        /// <summary>Describes the SampleFormat of the SoundData.</summary>
        public readonly SampleFormat SampleFormat;

        /// <summary>Describes the sample rate (frequency) of the SoundData.</summary>
        public readonly int SampleRate;

        /// <summary>Gets the SampleFormat of the buffer as an OpenTK.Audio.ALFormat enumeration.</summary>
        public OpenTK.Audio.ALFormat SampleFormatAsOpenALFormat
        {
            get
            {
                switch (SampleFormat)
                {
                    case SampleFormat.Mono8: return OpenTK.Audio.ALFormat.Mono8;
                    case SampleFormat.Mono16: return OpenTK.Audio.ALFormat.Mono16;
                    case SampleFormat.Stereo8: return OpenTK.Audio.ALFormat.Stereo8;
                    case SampleFormat.Stereo16: return OpenTK.Audio.ALFormat.Stereo16;
                    default: throw new NotSupportedException("Unknown PCM SampleFormat.");
                }
            }
        }

        #endregion
    }

    #region public enum SampleFormat

    /// <summary>Defines the available formats for SoundData.</summary>
    public enum SampleFormat
    {
        /// <summary>8 bits per sample, 1 channel.</summary>
        Mono8 = OpenTK.Audio.ALFormat.Mono8,
        /// <summary>16 bits per sample, 1 channel.</summary>
        Mono16 = OpenTK.Audio.ALFormat.Mono16,
        /// <summary>8 bits per sample, 2 channels.</summary>
        Stereo8 = OpenTK.Audio.ALFormat.Stereo8,
        /// <summary>16 bits per sample, 2 channels.</summary>
        Stereo16 = OpenTK.Audio.ALFormat.Stereo16
    }

    #endregion
}