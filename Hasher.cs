using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace StreamFormatDecryptor
{

    public class Hasher{
      public static byte[] CreateMD5(byte[] input)
	    {
	    // Use input string to calculate MD5 hash
	        using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
	        { 
	            byte[] hashBytes = md5.ComputeHash(input);
	            return hashBytes;
	        }
	    }

	    public byte[] AESDecryptKey(string ArtistName, string BeatmapSetID, string Mapper, string SongTitle, bool is_osz2){

		    string KeyAlg = "";

	   		switch (is_osz2)
	    	{
		    	case true:
			    	KeyAlg = (char)0x08 + Mapper + "yhxyfjo5" + BeatmapSetID;
				    break;

			    case false:
				    KeyAlg = (char)0x08 + SongTitle + "4390gn8931i" + ArtistName;
    			    break;
    		}

			byte[] Key = CreateMD5(Encoding.ASCII.GetBytes(KeyAlg));

			return Key;
		}
    }
}