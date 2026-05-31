using System;

namespace CMS21_Together_Core.Data.GameType;

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

}

