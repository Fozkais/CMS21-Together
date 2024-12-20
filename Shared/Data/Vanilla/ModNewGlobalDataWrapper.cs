using System;

namespace CMS21Together.Shared.Data.Vanilla;

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
		var newGlobalDataWrapper = new NewGlobalDataWrapper();

		newGlobalDataWrapper.PlayerMoney = PlayerMoney;
		newGlobalDataWrapper.PlayerLevel = PlayerLevel;
		newGlobalDataWrapper.PlayerExp = PlayerExp;
		newGlobalDataWrapper.PlayerScraps = PlayerScraps;
		newGlobalDataWrapper.BarnsAmount = BarnsAmount;
		newGlobalDataWrapper.MissionsFinished = MissionsFinished;
		newGlobalDataWrapper.CurrentMissionDone = CurrentMissionDone;
		newGlobalDataWrapper.UnlockedParkingLevels = UnlockedParkingLevels;
		newGlobalDataWrapper.StoryMissionInProgress = StoryMissionInProgress;

		return newGlobalDataWrapper;
	}
}