using CMS21_Together_Core.Network.Packets;
using CMS21Together.Network;
using HarmonyLib;

namespace CMS21Together.Logic.Hook
{
	[HarmonyPatch]
	public static class DisconnectHooks
	{
		[HarmonyPatch(typeof(NotificationCenter), nameof(NotificationCenter.SelectSceneToLoad),
			typeof(string), typeof(SceneType), typeof(bool), typeof(bool))]
		[HarmonyPrefix]
		public static void SelectSceneToLoadPrefix(string newSceneName, SceneType sceneType, bool useFader, bool saveGame)
		{
			if (newSceneName == "Menu")
			{
				if (Client.Instance != null && Client.Instance.IsConnected)
				{
					Client.Instance.Send(new DisconnectPacket()
					{
						playerID = Client.Instance.ID,
						message = "Player returned to menu."
					});
					Client.Instance.Disconnect();
				}
			}
		}
	}
}
