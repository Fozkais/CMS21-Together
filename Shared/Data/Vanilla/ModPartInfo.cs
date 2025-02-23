using System;
using CMS.UI.Logic.RepairPart;

namespace CMS21Together.Shared.Data.Vanilla;

[Serializable]
public class ModPartInfo
{
	public ModItem Item;
	public int RepairCost;
	public float SuccessCondition;
	public float FailCondition;
	public float CurrentCondition;
	public float DentSuccessCondition;
	public float DentFailCondition;
	public float DentCurrentCondition;
	public bool InstantRepair;
	
	public ModPartInfo(PartInfo partInfo)
	{
		Item = new ModItem(partInfo.Item);
		RepairCost = partInfo.RepairCost;
		SuccessCondition = partInfo.SuccessCondition;
		FailCondition = partInfo.FailCondition;
		CurrentCondition = partInfo.CurrentCondition;
		DentSuccessCondition = partInfo.DentSuccessCondition;
		DentCurrentCondition = partInfo.DentCurrentCondition;
		DentFailCondition = partInfo.DentFailCondition;
	}

	public PartInfo ToGame()
	{
		PartInfo info = new PartInfo();
		info.Item = Item.ToGame();
		info.RepairCost = RepairCost;
		info.SuccessCondition = SuccessCondition;
		info.FailCondition = FailCondition;
		info.CurrentCondition = CurrentCondition;
		info.DentSuccessCondition = DentSuccessCondition;
		info.DentCurrentCondition = DentCurrentCondition;
		info.DentFailCondition = DentFailCondition;
		return (info);
	}
}