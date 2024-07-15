using System;
using Il2Cpp;

namespace CMS21Together.Shared.Data.Vanilla
{
    [Serializable]
    public class ModNewGlobalDataWrapper
    {
        public int PlayerMoney;
        public int PlayerLevel;
        public int PlayerExp;
        public int PlayerScraps;
        public int BarnsAmount;
        public int MissionsFinished;
        public bool CurrentMissionDone = true;
        public int UnlockedParkingLevels = 1;
        public bool StoryMissionInProgress;

        public ModNewGlobalDataWrapper(NewGlobalDataWrapper data)
        {
            PlayerMoney = data.PlayerMoney;
            PlayerLevel = data.PlayerLevel;
            PlayerExp = data.PlayerExp;
            PlayerScraps = data.PlayerScraps;
            BarnsAmount = data.BarnsAmount;
            MissionsFinished = data.MissionsFinished;
            CurrentMissionDone = data.CurrentMissionDone;
            UnlockedParkingLevels = data.UnlockedParkingLevels;
            StoryMissionInProgress = data.StoryMissionInProgress;
        }

        public NewGlobalDataWrapper ToGame()
        {
            NewGlobalDataWrapper newGlobalDataWrapper = new NewGlobalDataWrapper();

            newGlobalDataWrapper.PlayerMoney = this.PlayerMoney;
            newGlobalDataWrapper.PlayerLevel = this.PlayerLevel;
            newGlobalDataWrapper.PlayerExp = this.PlayerExp;
            newGlobalDataWrapper.PlayerScraps = this.PlayerScraps;
            newGlobalDataWrapper.BarnsAmount = this.BarnsAmount;
            newGlobalDataWrapper.MissionsFinished = this.MissionsFinished;
            newGlobalDataWrapper.CurrentMissionDone = this.CurrentMissionDone;
            newGlobalDataWrapper.UnlockedParkingLevels = this.UnlockedParkingLevels;
            newGlobalDataWrapper.StoryMissionInProgress = this.StoryMissionInProgress;

            return newGlobalDataWrapper;
        }
    }
}