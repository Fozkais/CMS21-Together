using System;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace CMS21Together.Shared.Data.Vanilla
{
    
    [Serializable]
    public class ModNewCarLoaderData
    {
        public int[] position;

        public int[] specialState;

        public ModNewCarLoaderData(NewCarLoaderData data)
        {
            position =  new int[data.position.Length];
            for (int i = 0; i < position.Length; i++)
            {
                position[i] = data.position[i];
            }
            specialState = new int[data.specialState.Length];
            for (int i = 0; i < specialState.Length; i++)
            {
                specialState[i] = data.specialState[i];
            }
        }

        public NewCarLoaderData ToGame()
        {
            NewCarLoaderData a = new NewCarLoaderData();
            a.position = new Il2CppStructArray<int>(this.position.Length);
            for (int i = 0; i < position.Length; i++)
            {
                a.position[i] = this.position[i];
            }
            a.specialState = new Il2CppStructArray<int>(this.specialState.Length);
            for (int i = 0; i < specialState.Length; i++)
            {
                a.specialState[i] = this.specialState[i];
            }

            return a;
        }
    }
}