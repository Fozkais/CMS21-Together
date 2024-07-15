using System;
using Il2Cpp;

namespace CMS21Together.Shared.Data.Vanilla;

[Serializable]
public class ModMountObjectData
{
    public string ParentPath;

    public float[] Condition;

    public bool[] IsStuck;
    public ModMountObjectData(MountObjectData data)
    {
        ParentPath = data.ParentPath;
        Condition = data.Condition;
        IsStuck = data.IsStuck;
    }

    public MountObjectData ToGame()
    {
        MountObjectData data = new MountObjectData();
        data.Condition = Condition;
        data.IsStuck = IsStuck;
        data.ParentPath = ParentPath;

        return data;
    }
}