using System;
using Il2Cpp;

namespace CMS21Together.Shared.Data
{
    [Serializable]
    public class ModProfileData
    {
        public string Name;
       // public NewMachines machines; TODO: Implement this
        public ModDifficultyLevel difficulty;
        public ModNewInventoryData inventoryData;
        //public ModJobsData jobsData; TODO: Implement this
        //public ModJukeboxData jukeboxData; TODO: Implement this
        public byte saveVersion;
        //public ModUnlockedPosition unlockedPosition;
        //public ModWarehouseData warehouseData;
        public string buildVersion;
        //TODO: IMPLEMENT REST OF VARIABLE (~15 missing)

    }
}