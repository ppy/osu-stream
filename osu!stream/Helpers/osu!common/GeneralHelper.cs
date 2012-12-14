using System;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Text;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace osu_common.Helpers
{
    public static class GeneralHelper
    {
        public static string FormatEnum(string s)
        {
            string o = "";
            for (int i = 0; i < s.Length; i++)
            {
                if (i > 0 && Char.IsUpper(s[i]))
                    o += " ";
                o += s[i];
            }

            return o;
        }

        public static string WindowsFilenameStrip(string entry)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                entry = entry.Replace(c.ToString(), "");
            return entry;
        }

        public static string WindowsPathStrip(string entry)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                entry = entry.Replace(c.ToString(), "");
            entry = entry.Replace(".", "");
            return entry;
        }

#if !iOS
        public static void RegistryAdd(string extension, string progId, string description, string executable)
        {
            try
            {
                if (extension.Length != 0)
                {
                    if (extension[0] != '.')
                    {
                        extension = "." + extension;
                    }

                    // register the extension, if necessary
                    using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(extension, true))
                    {
                        if (key == null)
                        {
                            using (RegistryKey extKey = Registry.ClassesRoot.CreateSubKey(extension))
                            {
                                extKey.SetValue(String.Empty, progId);
                            }
                        }
                        else
                        {
                            key.SetValue(String.Empty, progId);
                        }
                    }


                    // register the progId, if necessary
                    using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(progId))
                    {
                        if (key == null || key.OpenSubKey("shell\\open\\command").GetValue(String.Empty).ToString() != String.Format("\"{0}\" \"%1\"", executable))
                        {
                            using (RegistryKey progIdKey = Registry.ClassesRoot.CreateSubKey(progId))
                            {
                                progIdKey.SetValue(String.Empty, description);

                                using (RegistryKey command = progIdKey.CreateSubKey("shell\\open\\command"))
                                {
                                    command.SetValue(String.Empty, String.Format("\"{0}\" \"%1\"", executable));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public static void RegistryDelete(string progId)
        {
            try
            {
                if (null != Registry.ClassesRoot.OpenSubKey(progId))
                    Registry.ClassesRoot.DeleteSubKeyTree(progId);
            }
            catch (Exception)
            {
            }

        }

        // Adds an ACL entry on the specified directory for the specified account. 
        public static void AddDirectorySecurity(string FileName, string Account, FileSystemRights Rights,
                                                InheritanceFlags Inheritance, PropagationFlags Propogation,
                                                AccessControlType ControlType)
        {
            // Create a new DirectoryInfo object. 
            DirectoryInfo dInfo = new DirectoryInfo(FileName);
            // Get a DirectorySecurity object that represents the  
            // current security settings. 
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            // Add the FileSystemAccessRule to the security settings.  
            dSecurity.AddAccessRule(new FileSystemAccessRule(Account,
                                                             Rights,
                                                             Inheritance,
                                                             Propogation,
                                                             ControlType));
            // Set the new access settings. 
            dInfo.SetAccessControl(dSecurity);
        }
#endif

        public static string UrlEncode(string str)
        {
            if (str == null)
            {
                return null;
            }
            return UrlEncode(str, Encoding.UTF8, false);
        }

        public static string UrlEncodeParam(string str)
        {
            if (str == null)
            {
                return null;
            }
            return UrlEncode(str, Encoding.UTF8, true);
        }

        public static string UrlEncode(string str, Encoding e, bool paramEncode)
        {
            if (str == null)
            {
                return null;
            }
            return Encoding.ASCII.GetString(UrlEncodeToBytes(str, e, paramEncode));
        }

        public static byte[] UrlEncodeToBytes(string str, Encoding e, bool paramEncode)
        {
            if (str == null)
            {
                return null;
            }
            byte[] bytes = e.GetBytes(str);
            return UrlEncodeBytesToBytespublic(bytes, 0, bytes.Length, false, paramEncode);
        }

        private static byte[] UrlEncodeBytesToBytespublic(byte[] bytes, int offset, int count, bool alwaysCreateReturnValue, bool paramEncode)
        {
            int num = 0;
            int num2 = 0;
            for (int i = 0; i < count; i++)
            {
                char ch = (char)bytes[offset + i];
                if (paramEncode && ch == ' ')
                {
                    num++;
                }
                else if (!IsSafe(ch))
                {
                    num2++;
                }
            }
            if ((!alwaysCreateReturnValue && (num == 0)) && (num2 == 0))
            {
                return bytes;
            }
            byte[] buffer = new byte[count + (num2 * 2)];
            int num4 = 0;
            for (int j = 0; j < count; j++)
            {
                byte num6 = bytes[offset + j];
                char ch2 = (char)num6;
                if (IsSafe(ch2))
                {
                    buffer[num4++] = num6;
                }
                else if (paramEncode && ch2 == ' ')
                {
                    buffer[num4++] = 0x2b;
                }
                else
                {
                    buffer[num4++] = 0x25;
                    buffer[num4++] = (byte)IntToHex((num6 >> 4) & 15);
                    buffer[num4++] = (byte)IntToHex(num6 & 15);
                }
            }
            return buffer;
        }

        public static bool IsSafe(char ch)
        {
            if ((((ch >= 'a') && (ch <= 'z')) || ((ch >= 'A') && (ch <= 'Z'))) || ((ch >= '0') && (ch <= '9')))
            {
                return true;
            }
            switch (ch)
            {
                case '\'':
                case '(':
                case ')':
                case '*':
                case '-':
                case '.':
                case '_':
                case '!':
                    return true;
            }
            return false;
        }

        public static char IntToHex(int n)
        {
            if (n <= 9)
            {
                return (char)(n + 0x30);
            }
            return (char)((n - 10) + 0x61);
        }

        public static void RemoveReadOnlyRecursive(string s)
        {
            foreach (string f in Directory.GetFiles(s))
            {
                FileInfo myFile = new FileInfo(f);
                if ((myFile.Attributes & FileAttributes.ReadOnly) > 0)
                    myFile.Attributes &= ~FileAttributes.ReadOnly;
            }

            foreach (string d in Directory.GetDirectories(s))
                RemoveReadOnlyRecursive(d);
        }

        public static bool FileMove(string src, string dest)
        {
            try
            {
                File.Move(src, dest);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static string AsciiOnly(string input)
        {
            if (input == null) return null;

            int last = input.LastIndexOf('\\');

            string asc = last >= 0 ? input.Substring(0, last + 1) : "";

            foreach (char c in last >= 0 ? input.Remove(0, last + 1) : input)
                if (c < 256)
                    asc += c;
            return asc;
        }

        public static void RecursiveMove(string oldFolder, string newFolder)
        {
            if (oldFolder == "Songs\\")
                return;

            foreach (string dir in Directory.GetDirectories(oldFolder))
            {
                string newDir = newFolder + "\\" + dir.Remove(0, 1 + dir.LastIndexOf('\\'));
                try
                {
                    Directory.CreateDirectory(newDir);
                }
                catch
                {
                }
                RecursiveMove(dir, newDir);
            }

            bool didExist = Directory.Exists(newFolder);
            if (!didExist)
                Directory.CreateDirectory(newFolder);

            foreach (string file in Directory.GetFiles(oldFolder))
            {
                string newFile = newFolder + "\\" + Path.GetFileName(file);

                bool didMove = true;

                try
                {
                    if (didExist) File.Delete(newFile);

                    try
                    {
                        File.Move(file, newFile);
                    }
                    catch (Exception)
                    {
                        File.Copy(file, newFile);
                        didMove = false;
                    }
                }
                catch (Exception)
                {
                    File.Delete(file);
                }

                if (!didMove)
                    File.Delete(file);
            }

            Directory.Delete(oldFolder, true);
        }

        public static string CleanStoryboardFilename(string filename)
        {
            return filename.Trim(new[] { '"' }).Replace("/", "\\");
        }

        public static DateTime PerthToLocalTime(DateTime dateTime)
        {
            return DateTime.SpecifyKind(dateTime.AddHours(-8), DateTimeKind.Utc).ToLocalTime();
        }

        //This is better than encoding as it doesn't check for origin specific data or remove invalid chars.
        public unsafe static string rawBytesToString(byte[] encoded)
        {
            if (encoded.Length == 0)
                return String.Empty;

            char[] converted = new char[(encoded.Length + 1) / 2];
            fixed (byte* bytePtr = encoded)
            fixed (char* stringPtr = converted)
            {
                byte* stringBytes = (byte*)stringPtr;
                byte* stringEnd = (byte*)(stringPtr) + converted.Length * 2;
                byte* bytePtr2 = bytePtr;
                do
                {
                    *stringBytes = *(bytePtr2++);
                    stringBytes++;
                } while (stringBytes != stringEnd);
            }
            return new string(converted);
        }


        //This is better than encoding as it doesn't check for origin specific data or remove invalid chars.
        public unsafe static byte[] rawStringToBytes(string decoded)
        {
            if (decoded == string.Empty)
                return new byte[] { };

            char[] decodedC = decoded.ToCharArray();
            byte[] converted = new byte[decodedC.Length * 2];
            fixed (char* stringPtr = decodedC)
            fixed (byte* convertedB = converted)
            {
                char* stringPtr2 = stringPtr;
                char* convertedC = (char*)convertedB;
                char* convertedEnd = convertedC + converted.Length;
                do
                {
                    *convertedC = *(stringPtr2++);
                    convertedC++;
                } while (convertedC != convertedEnd);
            }
            return converted;
        }

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }


        public static unsafe bool CompareByteSequence(byte[] a1, byte[] a2)
        {
            if (a1 == null || a2 == null || a1.Length != a2.Length)
                return false;
            fixed (byte* p1 = a1, p2 = a2)
            {
                byte* x1 = p1, x2 = p2;
                int l = a1.Length;
                for (int i = 0; i < l / 8; i++, x1 += 8, x2 += 8)
                    if (*((long*)x1) != *((long*)x2)) return false;
                if ((l & 4) != 0) { if (*((int*)x1) != *((int*)x2)) return false; x1 += 4; x2 += 4; }
                if ((l & 2) != 0) { if (*((short*)x1) != *((short*)x2)) return false; x1 += 2; x2 += 2; }
                if ((l & 1) != 0) if (*((byte*)x1) != *((byte*)x2)) return false;
                return true;
            }
        }

         public static TDest[] ConvertArray<TSource, TDest>(TSource[] source)
        where TSource : struct
        where TDest : struct {
    
        if (source == null)
            throw new ArgumentNullException("source");
    
            var sourceType = typeof(TSource);
            var destType = typeof(TDest);
    
            if (sourceType == typeof(char) || destType == typeof(char))
                throw new NotSupportedException(
                    "Can not convert from/to a char array. Char is special " +
                    "in a somewhat unknown way (like enums can't be based on " +
                    "char either), and Marshal.SizeOf returns 1 even when the " +
                    "values held by a char can be above 255."
                );
    
            var sourceByteSize = Buffer.ByteLength(source);
            var destTypeSize = Marshal.SizeOf(destType);
            if (sourceByteSize % destTypeSize != 0)
                throw new Exception(
                    "The source array is " + sourceByteSize + " bytes, which can " +
                    "not be transfered to chunks of " + destTypeSize + ", the size " +
                    "of type " + typeof(TDest).Name + ". Change destination type or " +
                    "pad the source array with additional values."
                );
    
            var destCount = sourceByteSize / destTypeSize;
            var destArray = new TDest[destCount];
    
            Buffer.BlockCopy(source, 0, destArray, 0, sourceByteSize);
    
            return destArray;
        }
    }
}