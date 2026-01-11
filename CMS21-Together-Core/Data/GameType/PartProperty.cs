using System;
using CMS21_Together_Core.Data.Enum;

namespace CMS21_Together_Core.Data.GameType;

[Serializable]
public class PartProperty
{
	public string ID;
	public int Price;
	public int PartGroup;
	public string ShopGroup;
	public int ExamineGroup;
	public int RepairGroup;
	public SpecialGroup SpecialGroup;
	public int[] DLC;
	public bool HaveDLC;
	public float TuningValue;
	public float BrakesValue;
	public bool IsBody;
	public bool CanPaint;
	public string LocalizedName;
	public string Brand;
	public string ShopName;
	public string CarID;
	public bool IsMod;
}