using Il2Cpp;

namespace CMS21MP.DataHandle.CL_Handle
{
    public class GarageInteractionHandle
    {
        public static void UpdateLifter( CarLifterState _state, int _carLoaderID)
        {
            var carLifterState = MainMod.carLoaders[_carLoaderID].lifter.currentState;
            var carLifter = MainMod.carLoaders[_carLoaderID].lifter;
            if (carLifterState == CarLifterState.Up && _state == CarLifterState.OnFloor)
            {
                carLifter.Action(1);
                carLifter.Action(1);
            }
            else if (carLifterState == CarLifterState.Up && _state == CarLifterState.Middle)
            {
                carLifter.Action(1);
            }
            else if (carLifterState == CarLifterState.Middle && _state == CarLifterState.Up)
            {
                carLifter.Action(0);
            }
            else if (carLifterState == CarLifterState.Middle && _state == CarLifterState.OnFloor)
            {
                carLifter.Action(1);
            }
            else if (carLifterState == CarLifterState.OnFloor && _state == CarLifterState.Up)
            {
                carLifter.Action(0);
                carLifter.Action(0);
            }
            else if (carLifterState == CarLifterState.OnFloor && _state == CarLifterState.Middle)
            {
                carLifter.Action(0);
            }
        }
    }
}