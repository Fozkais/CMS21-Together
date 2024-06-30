using System;
using System.Collections.Generic;
using System.Linq;
using Il2Cpp;

namespace CMS21Together.Shared.Data
{
    [Serializable]
    public struct ModNewUpgradeSystemData
    {
        public string[] id;

        public List<ModBoolArrayWrapper> unlocked;

        public int points;

        public ModNewUpgradeSystemData(NewUpgradeSystemData data)
        {
            id = data.id;
            unlocked = new List<ModBoolArrayWrapper>();
            for (int i = 0; i < data.unlocked.Count; i++)
            {
                unlocked.Add(new ModBoolArrayWrapper(data.unlocked._items[i]));
            }

            points = data.points;
        }

        public NewUpgradeSystemData ToGame()
        {
            NewUpgradeSystemData a = new NewUpgradeSystemData();

            a.id = this.id;
            a.unlocked = new Il2CppSystem.Collections.Generic.List<BoolArrayWrapper>();
            for (int i = 0; i < this.unlocked.Count; i++)
            {
                a.unlocked.Add(this.unlocked[i].ToGame());
            }

            a.points = this.points;

            return a;
        }
    }
    
    
    [Serializable]
    public struct ModBoolArrayWrapper
    {
        public bool[] element;

        public ModBoolArrayWrapper(BoolArrayWrapper item)
        {
            element = new bool[item.element.Length];
            for (int i = 0; i < item.element.Length; i++)
            {
                element[i] = item.element[i];
            }
        }

        public BoolArrayWrapper ToGame()
        {
            BoolArrayWrapper a = new BoolArrayWrapper();
            a.element = element;
            return a;
        }
        
    }
}