using System;
using CMS21_Together_Core;
using CMS21_Together_Core.Network;
using CMS21_Together_Core.Network.Packets;
using CMS21_Together_Server.Data;
using CMS21_Together_Server.Log;

namespace CMS21_Together_Server.Network.Handlers;

public static class StatsHandlers
{
    [PacketHandler(PacketTypes.StatsAction)]
    public static void HandleStatsAction(long clientId, StatsActionPacket packet)
    {
        var state = GameDataManager.CurrentState.WorldState;
        bool changed = false;

        // Scraps
        if (packet.ScrapsDelta != 0)
        {
            state.Scraps += packet.ScrapsDelta;
            if (state.Scraps < 0) state.Scraps = 0;
            changed = true;
        }

        // Exp & Level
        if (packet.ExpDelta > 0 && packet.ExpDelta < 10000) // basic validation
        {
            int exp = packet.ExpDelta;
            while (exp > 0)
            {
                int diffToNextLvl = GetDiffToNextLvl(state.Level - 1); // RealPlayerLevel is PlayerLevel + 1. So if Level is 1, PlayerLevel is 0.
                int num = Math.Max(0, Math.Min(exp, diffToNextLvl - state.Exp));
                exp -= num;
                state.Exp += num;

                if (state.Exp >= diffToNextLvl)
                {
                    state.Level++;
                    state.Exp = 0;
                    
                    // Update available points for garage
                    GameDataManager.CurrentState.GarageState.AvailablePoints = GarageUpgradeHandler.ComputeAvailablePoints(state, GameDataManager.CurrentState.GarageState);
                    Server.SendToClients(GameDataManager.CurrentState.GarageState);
                    
                    Logger.Info($"[Stats] Level Up! New Level: {state.Level}");
                }
            }
            changed = true;
        }

        if (changed)
        {
            Server.SendToClients(state);
        }
    }

    public static int GetCapToNextLvl(int lvl)
    {
        return (int)(Math.Pow((float)lvl, 1.62221f) * 2f) * 75;
    }

    public static int GetDiffToNextLvl(int lvl)
    {
        return GetCapToNextLvl(lvl + 1) - GetCapToNextLvl(lvl);
    }
}
