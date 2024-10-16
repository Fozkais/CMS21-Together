using CMS21Together.ClientSide.Data;
using CMS21Together.Shared.Data;

namespace CMS21Together.Shared;

public class SceneManager
{
	public static GameScene UpdateScene(string scene)
	{
		if (scene == "Barn")
			return GameScene.barn;
		if (scene == "garage")
			return GameScene.garage;
		if (scene == "Junkyard")
			return GameScene.junkyard;
		if (scene == "Auto_salon")
			return GameScene.auto_salon;
		if (scene == "Menu")
			return GameScene.menu;

		return GameScene.unknow;
	}

	public static GameScene CurrentScene(UserData user = null)
	{
		if (IsInBarn(user))
			return GameScene.barn;
		if (IsInGarage(user))
			return GameScene.garage;
		if (IsInJunkyard(user))
			return GameScene.junkyard;
		if (IsInDealer(user))
			return GameScene.auto_salon;
		if (IsInMenu(user))
			return GameScene.menu;

		return GameScene.unknow;
	}

	public static bool IsInMenu(UserData user = null)
	{
		if (user != null)
			return user.scene == GameScene.menu;

		return ClientData.UserData.scene == GameScene.menu;
	}

	public static bool IsInGarage(UserData user = null)
	{
		if (user != null)
			return user.scene == GameScene.garage;

		return ClientData.UserData.scene == GameScene.garage;
	}

	public static bool IsInJunkyard(UserData user = null)
	{
		if (user != null)
			return user.scene == GameScene.junkyard;

		return ClientData.UserData.scene == GameScene.junkyard;
	}

	public static bool IsInDealer(UserData user = null)
	{
		if (user != null)
			return user.scene == GameScene.auto_salon;

		return ClientData.UserData.scene == GameScene.auto_salon;
	}

	public static bool IsInBarn(UserData user = null)
	{
		if (user != null)
			return user.scene == GameScene.barn;

		return ClientData.UserData.scene == GameScene.barn;
	}
}