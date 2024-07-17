using System;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace CMS21Together.Shared.Data.Vanilla.GarageTool
{
    [Serializable]
    public class ModNewUnlockedPosition
    {
        public bool[] position;

        public ModNewUnlockedPosition(NewUnlockedPosition data)
        {
            position = new bool[data.position.Length];
        for (int i = 0; i < data.position.Length; i++)
            {
                position[i] = data.position[i];
            }
        }

        public NewUnlockedPosition ToGame()
        {
            NewUnlockedPosition a = new NewUnlockedPosition();
            a.position = new Il2CppStructArray<bool>(this.position.Length);
            for (int i = 0; i < this.position.Length; i++)
            {
                a.position[i] = this.position[i];
            }

            return a;
        }
    }
}