using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Transactions;

namespace StreamFormatDecryptor
{
	public class Program
	{
		public static bool isRunning = true;

		public static string? filePath = String.Empty;

		public static void ContinueOnPress()
		{
			Console.ReadLine();
		}


		private static (string? theIssue, bool isInvalid) CheckPathValidity(string? filePath)
		{
			bool isInvalid = false;
			string checkerResult = "Looks fine here but somehow we still got triggered?";

			if (string.IsNullOrWhiteSpace(filePath))
			{
				checkerResult = "Are you even putting anything there? It's as simple as dragging it to the console.";
				ContinueOnPress();
				isInvalid = true;
			}

			if (!File.Exists(filePath)){
				checkerResult = "The file does not exist. Check your file path and try again...unless it's not even a file path to begin with.";
				ContinueOnPress();
				isInvalid = true;
			}

			if (isInvalid)
				return (checkerResult, isInvalid);
				Console.Clear(); //return to the input

			return (null, false);

		}

		public static string? RequestPath (){
			Console.Write("Insert osu!stream beatmap file (osz2/osf2) to decrypt: ");

			string? filePath = Convert.ToString(Console.ReadLine()?.Replace("\"", string.Empty));

			(string? result, bool isWrong) = CheckPathValidity(filePath);
			if (isWrong == true) {Console.WriteLine(result);} // We already did a null check in another function so it can be safely ignored
			
			return filePath;
		}

		public static bool[] CheckFileFormat(string? filePath)
		{
			bool isInvalidFormat = false;
			bool isOsz2 = Path.GetExtension(filePath) switch
			{
				".osz2" => true,
				".osf2" => false,
				_ => isInvalidFormat = true
			};
			return [isInvalidFormat, isOsz2];
		}

		public static void Main(string[] args)
		{
			if (Environment.IsPrivilegedProcess)
			{
				Console.WriteLine("This program is being run as administrator. This is not recommended under normal circumstances.");
				Console.WriteLine("If it needs to, under specific reasons; note that file dragging won't work. Alternatively, right click on the file while holding SHIFT and click \"Copy as Path\".");
			}

			// filePath = RequestPath();

			filePath = "C:\\Users\\Windows\\Documents\\! Codes\\fStreamDecryptor\\Cranky - Dee Dee Cee (Deed).osz2"; // This is for debugging convenience purposes ONLY; Revert to L81 when done.

			bool isOsz2 = CheckFileFormat(filePath)[1];

			if (CheckFileFormat(filePath)[0] == true)
			{
				Console.WriteLine("Invalid file format. Please try again.");
				ContinueOnPress();
				filePath = RequestPath();
			}

			Console.WriteLine($"File name: {Path.GetFileName(filePath)}");
			Console.WriteLine($"File format: {Path.GetExtension(filePath)}");

			if (filePath != null)
			{
				using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
				var fileData = new byte[fileStream.Length];
				var read = fileStream.Read(fileData, 0, fileData.Length);

				var fileMeta = new fMetadata().Fetcher(fileStream);

				if (fileMeta == null)
				{
					Console.WriteLine("One or more metadatas are missing!");
					ContinueOnPress();
					return;
				}

				Console.WriteLine($"File metadata: \n Title: {fileMeta[0]} \n Artist: {fileMeta[1]} \n Mapper: {fileMeta[2]} \n Beatmap ID: {fileMeta[3]} \n");
				
				byte[] keyRaw = new Hasher().AESDecryptKey(fileMeta[1], fileMeta[3], fileMeta[2], fileMeta[0], isOsz2);
				var keyOut = keyRaw.ToString();
				Console.WriteLine($"Decryption key ({Path.GetExtension(filePath)}): {keyOut}");
				uint[] key = new uint[4];
				
				// Convert to little endian
				if (BitConverter.IsLittleEndian)
				{
					for (int i = 0; i < 4; i++)
					{
						key[i] = BitConverter.ToUInt32(keyRaw, i * 4);
					}
				}
				else
				{
					// If on a big-endian system, manually convert to little endian
					for (int i = 0; i < 4; i++)
					{
						key[i] = (uint)(keyRaw[i * 4] | (keyRaw[i * 4 + 1] << 8) | (keyRaw[i * 4 + 2] << 16) | (keyRaw[i * 4 + 3] << 24));
					}
				}
				
					keyOut.ToLower().Replace("-", string.Empty);

				var fileHeaders = new fMetadata().ReadHeader(fileStream);
				DecryptFile(fileHeaders[0], fileHeaders[3], key);
				
				
				//TODO: Decryption shit here
				//

				fileData = null;
				fileStream.Dispose();
			}

			GC.Collect();
			Console.WriteLine("File unloaded."); ;
			// ContinueOnPress(); filePath = RequestPath(); //repeat process
		}



		private static void DecryptFile(byte[] iv, byte[] bodyHash, uint[] key)
		{
			if (key == null || key.Length != 4)
				throw new ArgumentException("Key must be a 4-word array");
			if (iv == null || iv.Length != 16)
				throw new ArgumentException("IV must be a 16-byte array");
			if (bodyHash == null || bodyHash.Length != 16)
				throw new ArgumentException("Body hash must be a 16-byte array");

			var encryptor = new SafeEncryptionProvider(key);
			iv = iv;
			bodyHash = bodyHash;
		}

		public byte[] DecryptData(byte[] encryptedData)
		{
		    if (encryptedData == null || encryptedData.Length == 0)
			    throw new ArgumentException("Input data is null or empty");

		    // Create a copy of the data to work with
		    byte[] result = new byte[encryptedData.Length];
		    Array.Copy(encryptedData, result, encryptedData.Length);

		    // First decode the IV by XORing with body hash
		    byte[] decodedIv = new byte[16];
		    for (int i = 0; i < 16; i++)
		    {
			    decodedIv[i] = (byte)(DecryptFile.iv[i] ^ bodyHash[i]);
		    }

		    // Then XOR first block with decoded IV
		    for (int i = 0; i < Math.Min(16, result.Length); i++)
		    {
			    result[i] ^= decodedIv[i];
		    }

		    // Decrypt the data
		    encryptor.Decrypt(result, 0, result.Length);

		    // Validate first 64 bytes against known plain text
		    if (result.Length >= 64)
		    {
			    bool matches = true;
			    for (int i = 0; i < 64; i++)
			    {
				    if (result[i] != _knownPlain[i])
				    {
					    matches = false;
					    break;
				    }
			    }
			    Console.WriteLine($"Known plain text validation: {(matches ? "PASSED" : "FAILED")}");

			    if (matches)
			    {
				    // Skip known plain text and return the rest
				    byte[] actualData = new byte[result.Length - 64];
				    Array.Copy(result, 64, actualData, 0, actualData.Length);
				    return actualData;
			    }
		    }

		    return result;
		}
	}
}