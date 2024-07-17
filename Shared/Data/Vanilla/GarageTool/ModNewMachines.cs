using System;
using Il2Cpp;

namespace CMS21Together.Shared.Data.Vanilla.GarageTool
{
    [Serializable]
    public class ModNewMachines
    {
        public ModGroupItem GroupOnWheelBalancer;

        public bool WheelWasBalanced;

        public ModGroupItem GroupOnTireChanger;

        public bool GroupOnTireChangerIsMounting;

        public ModGroupItem GroupOnSpringClamp;

        public bool GroupOnSpringClampIsMounting;

        public ModGroupItem GroupOnEngineStand;

        public float EngineStandAngle;

        public ModItem ItemOnBatteryCharger;

        public ModItem ItemOnBrakeLathe;

        public ModNewMachines(NewMachines data)
        {
            GroupOnWheelBalancer =  new ModGroupItem(data.GroupOnWheelBalancer);
            WheelWasBalanced = data.WheelWasBalanced;
            GroupOnTireChanger = new ModGroupItem(data.GroupOnTireChanger);
            GroupOnTireChangerIsMounting = data.GroupOnTireChangerIsMounting;
            GroupOnSpringClamp =  new ModGroupItem(data.GroupOnSpringClamp);
            GroupOnSpringClampIsMounting = data.GroupOnSpringClampIsMounting;
            GroupOnEngineStand =  new ModGroupItem(data.GroupOnEngineStand);
            EngineStandAngle = data.EngineStandAngle;
            ItemOnBatteryCharger =  new ModItem(data.ItemOnBatteryCharger);
        }

        public NewMachines ToGame()
        {
            NewMachines a = new NewMachines();
            a.GroupOnWheelBalancer = this.GroupOnWheelBalancer.ToGame();
            a.WheelWasBalanced = this.WheelWasBalanced;
            a.GroupOnTireChanger = this.GroupOnTireChanger.ToGame();
            a.GroupOnTireChangerIsMounting = this.GroupOnTireChangerIsMounting;
            a.GroupOnSpringClamp = this.GroupOnSpringClamp.ToGame();
            a.GroupOnSpringClampIsMounting = this.GroupOnSpringClampIsMounting;
            a.GroupOnEngineStand = this.GroupOnEngineStand.ToGame();
            a.EngineStandAngle = this.EngineStandAngle;
            a.ItemOnBatteryCharger = this.ItemOnBatteryCharger.ToGame();
            return a;
        }
    }
}