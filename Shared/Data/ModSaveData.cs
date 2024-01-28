using System;

namespace CMS21Together.Shared.Data
{
    [Serializable]
    public class ModSaveData
    {
        public string Name;
        public int saveIndex;
        public bool alreadyLoaded;

        public ModSaveData(string saveName, int index, bool loaded)
        {
            Name = saveName;
            saveIndex = index;
            alreadyLoaded = loaded;
        }

        public ModSaveData() { }
    }
}