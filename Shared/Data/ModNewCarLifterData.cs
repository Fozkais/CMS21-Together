using System;
using Il2Cpp;

namespace CMS21Together.Shared.Data
{
    [Serializable]
    public class ModNewCarLifterData
    {
        public int lifterData;

        public ModNewCarLifterData(NewCarLifterData data)
        {
            lifterData = data.lifterData;
        }

        public NewCarLifterData ToGame()
        {
            NewCarLifterData a = new NewCarLifterData();
            a.lifterData = this.lifterData;
            return a;
        }
    }
}