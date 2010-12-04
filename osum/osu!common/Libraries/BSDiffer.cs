using System;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.BZip2;
using Ionic.Zlib;

/* uncomment this to use unsafe version of memcmp */
//#define MEMCMP_UNSAFE

namespace osu_common.Libraries
{
    public class BSDiffer
    {
        /// <summary>
        /// Occurs when more than one whole percentage has been processed.
        /// This is not totally accurate because we do not push updates during large stream reads
        /// for performance reasons.
        /// </summary>
        public event ProgressUpdateHandler OnProgress;

        /// <summary>
        /// Internal check for change in progress of over 1%.
        /// </summary>
        private int progress;

        /// <summary>
        /// Read/write block size
        /// </summary>
        private const int SIZE = 64 * 1024;

        #region Algorithms

        private void split(int[] I, int[] V, int start, int len, int h)
        {
            int x, i, j, k;

            if (len < 16)
            {
                for (k = start; k < start + len; k += j)
                {
                    j = 1;
                    x = V[I[k] + h];
                    for (i = 1; k + i < start + len; i++)
                    {
                        if (V[I[k + i] + h] < x)
                        {
                            x = V[I[k + i] + h];
                            j = 0;
                        }

                        if (V[I[k + i] + h] == x)
                        {
                            int tmp = I[k + j];
                            I[k + j] = I[k + i];
                            I[k + i] = tmp;
                            j++;
                        }
                    }

                    for (i = 0; i < j; i++)
                        V[I[k + i]] = k + j - 1;

                    if (j == 1)
                        I[k] = -1;
                }

                return;
            }

            x = V[I[start + len / 2] + h];
            int jj = 0, kk = 0;
            for (i = start; i < start + len; i++)
            {
                if (V[I[i] + h] < x)
                    jj++;
                if (V[I[i] + h] == x)
                    kk++;
            }

            jj += start;
            kk += jj;

            i = start;
            j = 0;
            k = 0;

            while (i < jj)
            {
                if (V[I[i] + h] < x)
                {
                    i++;
                }
                else if (V[I[i] + h] == x)
                {
                    int tmp = I[i];
                    I[i] = I[jj + j];
                    I[jj + j] = tmp;
                    j++;
                }
                else
                {
                    int tmp = I[i];
                    I[i] = I[kk + k];
                    I[kk + k] = tmp;
                    k++;
                }
            }

            while (jj + j < kk)
            {
                if (V[I[jj + j] + h] == x)
                {
                    j++;
                }
                else
                {
                    int tmp = I[jj + j];
                    I[jj + j] = I[kk + k];
                    I[kk + k] = tmp;
                    k++;
                }
            }

            if (jj > start)
                split(I, V, start, jj - start, h);

            for (i = 0; i < kk - jj; i++)
                V[I[jj + i]] = kk - 1;
            if (jj == kk - 1)
                I[jj] = -1;

            if (start + len > kk)
                split(I, V, kk, start + len - kk, h);
        }

        private void qsufsort(int[] I, int[] V, byte[] oldfile, int oldsize)
        {
            int[] buckets = new int[256];

            for (int i = 0; i < oldsize; i++)
                buckets[oldfile[i]]++;
            for (int i = 1; i < 256; i++)
                buckets[i] += buckets[i - 1];
            for (int i = 255; i > 0; i--)
                buckets[i] = buckets[i - 1];
            buckets[0] = 0;

            for (int i = 0; i < oldsize; i++)
                I[++buckets[oldfile[i]]] = i;
            I[0] = oldsize;
            for (int i = 0; i < oldsize; i++)
                V[i] = buckets[oldfile[i]];
            V[oldsize] = 0;
            for (int i = 1; i < 256; i++)
            {
                if (buckets[i] == buckets[i - 1] + 1)
                    I[buckets[i]] = -1;
            }
            I[0] = -1;

            for (int h = 1; I[0] != -(oldsize + 1); h += h)
            {
                int len = 0;
                int i;
                for (i = 0; i < oldsize + 1; )
                {
                    if (I[i] < 0)
                    {
                        len -= I[i];
                        i -= I[i];
                    }
                    else
                    {
                        if (len != 0)
                            I[i - len] = -len;
                        len = V[I[i]] + 1 - i;
                        split(I, V, i, len, h);
                        i += len;
                        len = 0;
                    }
                }

                if (len != 0)
                    I[i - len] = -len;
            }

            for (int i = 0; i < oldsize + 1; i++)
                I[V[i]] = i;
        }

        private static int matchlen(byte[] oldfile, int oldoffset, int oldsize, byte[] newfile, int newoffset, int newsize)
        {
            int i;

            for (i = 0; (i < oldsize - oldoffset) && (i < newsize - newoffset); i++)
            {
                if (oldfile[i + oldoffset] != newfile[i + newoffset])
                    break;
            }

            return i;
        }

        private int search(int[] I, byte[] oldfile, int oldsize, byte[] newfile, int newoffset, int newsize, int st, int en, out int pos)
        {
            int x, y;

            if (en - st < 2)
            {
                x = matchlen(oldfile, I[st], oldsize, newfile, newoffset, newsize);
                y = matchlen(oldfile, I[en], oldsize, newfile, newoffset, newsize);

                if (x > y)
                {
                    pos = I[st];
                    return x;
                }
                else
                {
                    pos = I[en];
                    return y;
                }
            }

            x = st + (en - st) / 2;

            int memcmpres;

#if MEMCMP_UNSAFE
            unsafe
            {
                fixed(byte* a = oldfile, b = newfile)
                {
                    memcmpres = memcmp(a + I[x], b + newoffset, Math.Min(oldsize - I[x], newsize - newoffset));
                }
            }
#else
            memcmpres = memcmp(oldfile, I[x], newfile, newoffset, Math.Min(oldsize - I[x], newsize - newoffset));
#endif
            if (memcmpres < 0)
            {
                return search(I, oldfile, oldsize, newfile, newoffset, newsize, x, en, out pos);
            }
            else
            {
                return search(I, oldfile, oldsize, newfile, newoffset, newsize, st, x, out pos);
            }
        }

        private void offtout(int x, byte[] buf)
        {
            int y;
            if (x < 0)
                y = -x;
            else
                y = x;

            for (int i = 0; i < 7; i++)
            {
                buf[i] = (byte)(y % 256);
                y -= buf[i];
                y /= 256;
            }
            buf[7] = (byte)(y % 256);

            if (x < 0)
                buf[7] |= 0x80;
        }

        private unsafe static int memcmp(byte* a, byte* b, int count)
        {
            int v = 0;
            for (int i = 0; i < count && v == 0; i++)
                v = a[i] - b[i];

            return v;
        }

        private static int memcmp(byte[] a, int aoffset, byte[] b, int boffset, int count)
        {
            int v = 0;
            for (int i = 0; i < count && v == 0; i++)
                v = a[i + aoffset] - b[i + boffset];

            return v;
        }

#endregion

        public void Diff(string oldpath, string newpath, string patchpath)
        {
            using (FileStream fileStream = File.Open(patchpath, FileMode.Create))
                Diff(oldpath, newpath, fileStream);
        }

        public void Diff(string oldpath, string newpath, Stream outStream)
        {
            int oldsize, newsize;
            int progressMax;
            int progressCur = 0;

            byte[] oldfile;
            using (FileStream stream = File.OpenRead(oldpath))
            {
                oldsize = (int)stream.Length;
                oldfile = new byte[oldsize];

                progressMax = oldsize * 9;

                for (int i = 0; i < oldsize; i += SIZE)
                {
                    stream.Read(oldfile, i, Math.Min(SIZE, oldsize - i));
                    updateProgress(progressCur + i, progressMax);
                }

                progressCur = oldsize;
            }

            int[] I = new int[oldsize + 1];
            int[] V = new int[oldsize + 1];

            // runs 
            qsufsort(I, V, oldfile, oldsize);
            updateProgress(progressCur = oldsize*2, progressMax);

            // free V
            V = null;

            byte[] newfile;
            using (FileStream stream = File.OpenRead(newpath))
            {
                newsize = (int)stream.Length;
                newfile = new byte[newsize];

                progressMax += newsize*7 - oldsize*7;

                for (int i = 0; i < newsize; i += SIZE)
                {
                    stream.Read(newfile, i, Math.Min(SIZE, newsize - i));
                    updateProgress(progressCur + i, progressMax);
                }

                progressCur += newsize;
            }

            byte[] db = new byte[newsize + 1];
            byte[] eb = new byte[newsize + 1];

            int dblen = 0;
            int eblen = 0;

            using (BinaryWriter bw = new BinaryWriter(outStream))
            {
                /* Header is
		            0	8	 "BSDIFF40"
		            8	8	length of bzip2ed ctrl block
		            16	8	length of bzip2ed diff block
		            24	8	length of new file */
                /* File is
                    0	32	Header
                    32	??	Bzip2ed ctrl block
                    ??	??	Bzip2ed diff block
                    ??	??	Bzip2ed extra block */

                bw.Write(Encoding.ASCII.GetBytes("BSDIFF40"));
                bw.Write((long)0);
                bw.Write((long)0);
                bw.Write((long)newsize);

                using (MemoryStream mem = new MemoryStream())
                {
                    using (
                        GZipStream gzstream = new GZipStream(mem, CompressionMode.Compress,
                                                             CompressionLevel.BestCompression, true))
                    {
                        int pos = 0;
                        int scan = 0, len = 0;
                        int lastscan = 0, lastpos = 0, lastoffset = 0;

                        while (scan < newsize)
                        {
                            int oldscore = 0;

                            for (int scsc = (scan += len); scan < newsize; scan++)
                            {
                                len = search(I, oldfile, oldsize, newfile, scan, newsize, 0, oldsize, out pos);

                                for (; scsc < scan + len; scsc++)
                                {
                                    if ((scsc + lastoffset < oldsize) && (oldfile[scsc + lastoffset] == newfile[scsc]))
                                        oldscore++;
                                }

                                if (((len == oldscore) && (len != 0)) || (len > oldscore + 8))
                                    break;

                                if ((scan + lastoffset < oldsize) &&
                                    (oldfile[scan + lastoffset] == newfile[scan]))
                                    oldscore--;
                            }

                            if ((len != oldscore) || (scan == newsize))
                            {
                                int s = 0, Sf = 0, lenf = 0;
                                int Sb = 0, lenb = 0;
                                int Ss = 0, lens = 0;

                                for (int i = 0; (lastscan + i < scan) && (lastpos + i < oldsize);)
                                {
                                    if (oldfile[lastpos + i] == newfile[lastscan + i])
                                        s++;

                                    i++;

                                    if (s*2 - i > Sf*2 - lenf)
                                    {
                                        Sf = s;
                                        lenf = i;
                                    }
                                }

                                if (scan < newsize)
                                {
                                    s = 0;

                                    for (int i = 1; (scan >= lastscan + i) && (pos >= i); i++)
                                    {
                                        if (oldfile[pos - i] == newfile[scan - i])
                                            s++;

                                        if (s*2 - i > Sb*2 - lenb)
                                        {
                                            Sb = s;
                                            lenb = i;
                                        }
                                    }
                                }

                                if (lastscan + lenf > scan - lenb)
                                {
                                    int overlap = (lastscan + lenf) - (scan - lenb);
                                    s = 0;

                                    for (int i = 0; i < overlap; i++)
                                    {
                                        if (newfile[lastscan + lenf - overlap + i] ==
                                            oldfile[lastpos + lenf - overlap + i])
                                            s++;

                                        if (newfile[scan - lenb + i] == oldfile[pos - lenb + i])
                                            s--;

                                        if (s > Ss)
                                        {
                                            Ss = s;
                                            lens = i + 1;
                                        }
                                    }

                                    lenf += lens - overlap;
                                    lenb -= lens;
                                }

                                for (int i = 0; i < lenf; i++)
                                    db[dblen + i] = (byte) (newfile[lastscan + i] - oldfile[lastpos + i]);

                                for (int i = 0; i < (scan - lenb) - (lastscan + lenf); i++)
                                    eb[eblen + i] = newfile[lastscan + lenf + i];

                                dblen += lenf;
                                eblen += (scan - lenb) - (lastscan + lenf);

                                byte[] buffer = new byte[8];

                                offtout(lenf, buffer);
                                gzstream.Write(buffer, 0, 8);

                                offtout((scan - lenb) - (lastscan + lenf), buffer);
                                gzstream.Write(buffer, 0, 8);

                                offtout((pos - lenb) - (lastpos + lenf), buffer);
                                gzstream.Write(buffer, 0, 8);

                                lastscan = scan - lenb;
                                lastpos = pos - lenb;
                                lastoffset = pos - scan;
                            }

                            updateProgress(progressCur + scan, progressMax);
                        }

                        progressCur += newsize;
                    }

                    byte[] data = mem.ToArray();
                    progressMax += data.Length - newsize;
                    for (int i = 0; i < data.Length; i += SIZE)
                    {
                        bw.Write(data, i, Math.Min(SIZE, data.Length - i));
                        updateProgress(progressCur + i, progressMax);
                    }

                    progressCur += data.Length;
                }

                bw.Flush();
                long lenctrl = bw.BaseStream.Position - 32;

                // write compressed diff data
                using (MemoryStream mem = new MemoryStream())
                {
                    using (
                        GZipStream gzstream = new GZipStream(mem, CompressionMode.Compress,
                                                             CompressionLevel.BestCompression, true))
                    {
                        progressMax += dblen - newsize;

                        for (int i = 0; i < dblen; i += SIZE)
                        {
                            gzstream.Write(db, i, Math.Min(SIZE, dblen - i));
                            updateProgress(progressCur + i, progressMax);
                        }

                        progressCur += dblen;
                    }

                    byte[] data = mem.ToArray();
                    progressMax += data.Length - newsize;
                    for (int i = 0; i < data.Length; i += SIZE)
                    {
                        bw.Write(data, i, Math.Min(SIZE, data.Length - i));
                        updateProgress(progressCur += i, progressMax);
                    }
                }

                bw.Flush();
                long lendiff = bw.BaseStream.Position - lenctrl - 32;

                // write compressed extra data
                using (MemoryStream mem = new MemoryStream())
                {
                    using (
                        GZipStream gzstream = new GZipStream(mem, CompressionMode.Compress,
                                                             CompressionLevel.BestCompression, true))
                    {
                        progressMax += eblen - newsize;

                        for (int i = 0; i < eblen; i += SIZE)
                        {
                            gzstream.Write(eb, i, Math.Min(SIZE, eblen - i));
                            updateProgress(progressCur + i, progressMax);
                        }

                        progressCur += eblen;
                    }

                    byte[] data = mem.ToArray();
                    progressMax += data.Length - newsize;
                    for (int i = 0; i < data.Length; i += SIZE)
                    {
                        bw.Write(data, i, Math.Min(SIZE, data.Length - i));
                        updateProgress(progressCur + i, progressMax);
                    }
                }

                // fill in gaps in header
                bw.Flush();
                bw.Seek(8, SeekOrigin.Begin);
                bw.Write(lenctrl);
                bw.Write(lendiff);

                if (OnProgress != null)
                    OnProgress(this, 1, 1);
            }
        }

        private void updateProgress(long pos, long size)
        {
            int lastPercent = progress;
            progress = (int)((float)pos / size * 100);
            if (lastPercent != progress && OnProgress != null)
                OnProgress(this, pos, size);
        }
    }
}