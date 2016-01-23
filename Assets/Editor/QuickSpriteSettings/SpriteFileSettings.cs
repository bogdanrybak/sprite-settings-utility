namespace Staple.EditorScripts
{
    public struct SpriteFileSettings {
        public bool OverridePackingTag;
        public string PackingTag;
        public SpriteSlicingOptions SlicingOptions;
        
        const char delimeterChar = ',';
    
        public override string ToString ()
        {
            string delimeterSpace = delimeterChar + " ";
            string serialized = string.Concat (OverridePackingTag, delimeterSpace, PackingTag, delimeterSpace,
                SlicingOptions);
            return serialized;
        }
        
        public static SpriteFileSettings FromString (string serialized)
        {
            var fileSettings = new SpriteFileSettings ();
            string[] entries = serialized.Split (new char[] {delimeterChar}, System.StringSplitOptions.None);
            // Support old format
            if (entries.Length == 3) 
            {
                fileSettings.SlicingOptions = SpriteSlicingOptions.FromString (serialized);
            } else 
            {
                fileSettings.OverridePackingTag = bool.Parse (entries[0]);
                fileSettings.PackingTag = entries[1];
                
                int startingIndex = entries[0].Length + entries[1].Length + 2;
                
                string serializedSlicingOptions = serialized.Substring (startingIndex, 
                    serialized.Length - startingIndex);
                fileSettings.SlicingOptions = SpriteSlicingOptions.FromString (serializedSlicingOptions);
            }
            
            return fileSettings;
        }
    }
}