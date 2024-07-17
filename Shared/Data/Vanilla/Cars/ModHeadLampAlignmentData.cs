using System;
using Il2Cpp;

namespace CMS21Together.Shared.Data.Vanilla.Cars;

[Serializable]
public struct ModHeadLampAlignmentData
{
    public float Horizontal;
    public float Vertical;

    public ModHeadLampAlignmentData(HeadlampAlignmentData data)
    {
        Horizontal = data.Horizontal;
        Vertical = data.Vertical;
    }

    public HeadlampAlignmentData ToGame()
    {
        HeadlampAlignmentData data = new HeadlampAlignmentData();
        data.Horizontal = Horizontal;
        data.Vertical = Vertical;

        return data;
    }
}