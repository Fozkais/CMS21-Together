using System;

namespace CMS21Together.Shared.Data.Vanilla.Cars;

[Serializable]
public struct ModCarInfoData
{
	public int BuyPrice;
	public int Mileage;
	public ModCarFrom CarFrom;

	public ModCarInfoData(CarInfoData data)
	{
		BuyPrice = data.BuyPrice;
		Mileage = data.Mileage;
		CarFrom = (ModCarFrom)data.CarFrom;
	}

	public CarInfoData ToGame(ModCarInfoData _data)
	{
		var data = new CarInfoData();
		data.BuyPrice = _data.BuyPrice;
		data.Mileage = _data.Mileage;
		data.CarFrom = (CarFrom)_data.CarFrom;
		return data;
	}
}

[Serializable]
public enum ModCarFrom
{
	Auction,
	Barn,
	Junkyard,
	Mission,
	None,
	Order,
	Salon
}