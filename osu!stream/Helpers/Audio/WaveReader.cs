#region --- License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2006-2008 the OpenTK Team.
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing details.
 */
#endregion

using System;
using System.IO;

namespace osum.Helpers.Audio
{
    internal sealed class WaveReader : AudioReader
    {
        private SoundData decoded_data;

        //RIFF header
        private string signature;
        private int riff_chunck_size;
        private string format;
            
        //WAVE header
        private string format_signature;
        private int format_chunk_size;
        private short audio_format;
        private short channels;
        private int sample_rate;
        private int byte_rate;
        private short block_align;
        private short bits_per_sample;
            
        //DATA header
        private string data_signature;
        private int data_chunk_size;

        private readonly BinaryReader reader;

        internal WaveReader() { }

        public override void Dispose()
        {
            reader.Close();
            base.Dispose();
        }

        internal WaveReader(Stream s)
        {
            if (s == null) throw new ArgumentNullException();
            if (!s.CanRead) throw new ArgumentException("Cannot read from specified Stream.");

            reader = new BinaryReader(s);
            Stream = s;
        }

#if false
        /// <summary>
        /// Writes the WaveSound's data to the specified OpenAL buffer in the correct format.
        /// </summary>
        /// <param name="bid">
        /// A <see cref="System.UInt32"/>
        /// </param>
        public void WriteToBuffer(uint bid)
        {
            unsafe
            {
                //fix the array as a byte
                fixed (byte* p_Data = _Data)
                {
                    if (Channels == 1)
                    {
                        if (BitsPerSample == 16)
                        {
                            Console.Write("Uploading 16-bit mono data to OpenAL...");
                            AL.BufferData(bid, ALFormat.Mono16, (IntPtr)p_Data, _Data.Length, SampleRate);
                        }
                        else
                        {
                            if (BitsPerSample == 8)
                            {
                                Console.Write("Uploading 8-bit mono data to OpenAL...");
                                AL.BufferData(bid, ALFormat.Mono8, (IntPtr)p_Data, _Data.Length, SampleRate);
                            }
                        }

                    }
                    else
                    {
                        if (Channels == 2)
                        {
                            if (BitsPerSample == 16)
                            {
                                Console.Write("Uploading 16-bit stereo data to OpenAL...");
                                AL.BufferData(bid, ALFormat.Stereo16, (IntPtr)p_Data, _Data.Length, SampleRate);
                            }
                            else
                            {
                                if (BitsPerSample == 8)
                                {
                                    Console.Write("Uploading 8-bit stereo data to OpenAL...");
                                    AL.BufferData(bid, ALFormat.Stereo8, (IntPtr)p_Data, _Data.Length, SampleRate);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Writes all relevent information about the WaveSound to the console window.
        /// </summary>
        public void DumpParamsToConsole()
        {			
            Console.WriteLine("AudioFormat:" + AudioFormat);
            Console.WriteLine("Channels:" + Channels);
            Console.WriteLine("SampleRate:" + SampleRate);
            Console.WriteLine("ByteRate:" + ByteRate);
            Console.WriteLine("BlockAlign:" + BlockAlign);
            Console.WriteLine("BitsPerSample:" + BitsPerSample);
        }
        
        /// <value>
        /// Returns the WaveSound's raw sound data as an array of bytes.
        /// </value>
        public byte[] Data
        {
            get
            {
                return _Data;
            }
        }
#endif

        #region --- Public Members ---

        #region public override bool Supports(Stream s)

        /// <summary>
        /// Checks whether the specified stream contains valid WAVE/RIFF buffer.
        /// </summary>
        /// <param name="s">The System.IO.Stream to check.</param>
        /// <returns>True if the stream is a valid WAVE/RIFF file; false otherwise.</returns>
        public override bool Supports(Stream s)
        {
            BinaryReader reader = new BinaryReader(s);
            return ReadHeaders(reader);
        }

        #endregion

        #region public override SoundData ReadSamples(int samples)

        /// <summary>
        /// Reads and decodes the specified number of samples from the sound stream.
        /// </summary>
        /// <param name="samples">The number of samples to read and decode.</param>
        /// <returns>An OpenTK.Audio.SoundData object that contains the decoded buffer.</returns>
        public override SoundData ReadSamples(long samples)
        {
            if (samples > reader.BaseStream.Length - reader.BaseStream.Position)
                samples = reader.BaseStream.Length - reader.BaseStream.Position;

            //while (samples > decoded_data.Data.Length * (bits_per_sample / 8))
            //    Array.Resize<byte>(ref decoded_data.Data, decoded_data.Data.Length * 2);

            decoded_data = new SoundData(new SoundFormat(channels, bits_per_sample, sample_rate),
                                                         reader.ReadBytes((int)samples));

            return decoded_data;
        }

        #endregion

        #region public override SoundData ReadToEnd()

        /// <summary>
        /// Reads and decodes the sound stream.
        /// </summary>
        /// <returns>An OpenTK.Audio.SoundData object that contains the decoded buffer.</returns>
        public override SoundData ReadToEnd()
        {
            try
            {
                //read the buffer into a byte array, even if the format is 16 bit
                //decoded_data = new byte[data_chunk_size];

                decoded_data = new SoundData(new SoundFormat(channels, bits_per_sample, sample_rate),
                                                             reader.ReadBytes((int)reader.BaseStream.Length));
                
                //return new SoundData(decoded_data, new SoundFormat(channels, bits_per_sample, sample_rate));
                return decoded_data;
            }
            catch (AudioReaderException)
            {
                reader.Close();
                throw;
            }
        }

        #endregion

        #endregion

        #region --- Protected Members ---

        protected override Stream Stream
        {
            get { return base.Stream; }
            set
            {
                base.Stream = value;
                if (!ReadHeaders(reader))
                    throw new AudioReaderException("Invalid WAVE/RIFF file: invalid or corrupt signature.");

#if DEBUF
                Console.WriteLine(String.Format("Opened WAVE/RIFF file: ({0}, {1}, {2}, {3}) ", sample_rate.ToString(), bits_per_sample.ToString(),
                                          channels.ToString(), audio_format.ToString()));
#endif
            }
        }

        #endregion

        #region --- Private Members ---

        // Tries to read the WAVE/RIFF headers, and returns true if they are valid.
        private bool ReadHeaders(BinaryReader reader)
        {
            // Don't explicitly call reader.Close()/.Dispose(), as that will close the inner stream.
            // There's no such danger if the BinaryReader isn't explicitly destroyed.

            // RIFF header
            signature = new string(reader.ReadChars(4));
            if (signature != "RIFF")
                return false;

            riff_chunck_size = reader.ReadInt32();

            format = new string(reader.ReadChars(4));
            if (format != "WAVE")
                return false;

            // WAVE header
            format_signature = new string(reader.ReadChars(4));
            if (format_signature != "fmt ")
                return false;

            format_chunk_size = reader.ReadInt32();
            audio_format = reader.ReadInt16();
            channels = reader.ReadInt16();
            sample_rate = reader.ReadInt32();
            byte_rate = reader.ReadInt32();
            block_align = reader.ReadInt16();
            bits_per_sample = reader.ReadInt16();

            data_signature = new string(reader.ReadChars(4));
            if (data_signature != "data")
                return false;

            data_chunk_size = reader.ReadInt32();

            return true;
        }

        #endregion
    }
}
