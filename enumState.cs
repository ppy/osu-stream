namespace StreamFormatDecryptor{
    public class fEnum{

		public enum fDecryptMode{
			DIST,
			OSUM,
			Unknown
		}
		
		public enum MapMetaType
		{
			Title = 0,
			Artist = 1,
			Creator = 2,
			Version = 3,
			BeatmapSetID = 10001 // this is the actual value for some reason (not 4)
		}
		
		public enum EncryptionMethod
		{
			None,
			One,
			Two,
			Three,
			Four
			// Encryption Three and Four never existed
			// It was all a lie...
		}
		
		
		 // enum is more efficient than string
    }
}