using System.Collections.Generic;
using CMS21Together.ClientSide.Data.Garage.Tools;
using CMS21Together.ClientSide.Data.Player;
using CMS21Together.Shared;
using CMS21Together.Shared.Data;
using CMS21Together.Shared.Data.Vanilla;
using CMS21Together.Shared.Data.Vanilla.Cars;
using CMS21Together.Shared.Data.Vanilla.GarageTool;
using CMS21Together.Shared.Data.Vanilla.Jobs;
using MelonLoader;

namespace CMS21Together.ClientSide.Data.Handle;

public class ClientSend
{
	private static void SendData(Packet _packet, bool reliable = true)
	{
		_packet.WriteLength();
		Client.Instance.SendData(_packet, reliable);
	}

	public static void ConnectValidationPacket()
	{
		using (var packet = new Packet((int)PacketTypes.connect))
		{
			packet.Write(ClientData.UserData.playerID);
			packet.Write(ClientData.UserData.username);
			packet.Write(ContentManager.Instance.ownedContents);
			packet.Write(ContentManager.Instance.gameVersion);
			packet.Write(MainMod.ASSEMBLY_MOD_VERSION);
			packet.Write(ClientData.UserData.playerGUID);

			SendData(packet);
		}
	}

	public static void ReadyPacket(bool isReady, int playerID)
	{
		using (var packet = new Packet((int)PacketTypes.readyState))
		{
			packet.Write(playerID);
			packet.Write(isReady);

			SendData(packet);
		}
	}

	public static void PositionPacket(Vector3Serializable position)
	{
		using (var packet = new Packet((int)PacketTypes.position))
		{
			packet.Write(position);
			SendData(packet);
		}
	}

	public static void RotationPacket(QuaternionSerializable rotation)
	{
		using (var packet = new Packet((int)PacketTypes.rotation))
		{
			packet.Write(rotation);
			SendData(packet);
		}
	}

	public static void ItemPacket(ModItem item, InventoryAction action)
	{
		using (var packet = new Packet((int)PacketTypes.item))
		{
			packet.Write(action);
			packet.Write(item);

			SendData(packet);
		}
	}

	public static void GroupItemPacket(ModGroupItem groupItem, InventoryAction action)
	{
		using (var packet = new Packet((int)PacketTypes.groupItem))
		{
			packet.Write(action);
			packet.Write(groupItem);

			SendData(packet);
		}
	}

	public static void StatPacket(int diff, ModStats type, bool initial=false)
	{
		using (var packet = new Packet((int)PacketTypes.stat))
		{
			packet.Write(diff);
			packet.Write(type);
			packet.Write(initial);

			SendData(packet);
		}
	}
	
	public static void ExpPacket(int exp, int level)
	{
		using (var packet = new Packet((int)PacketTypes.exp))
		{
			packet.Write(exp);
			packet.Write(level);

			SendData(packet);
		}
	}
	
	public static void PointPacket(int availablePoints)
	{
		using (var packet = new Packet((int)PacketTypes.point))
		{
			packet.Write(availablePoints);

			SendData(packet);
		}
	}

	public static void LifterPacket(ModLifterState state, int carLoaderID)
	{
		using (var packet = new Packet((int)PacketTypes.lifter))
		{
			packet.Write(state);
			packet.Write(carLoaderID);

			SendData(packet);
		}
	}

	public static void LoadJobCarPacket(ModCar car)
	{
		using (var packet = new Packet((int)PacketTypes.loadJobCar))
		{
			packet.Write(car);

			SendData(packet);
		}
	}

	public static void LoadCarPacket(ModNewCarData carData, int carLoaderID)
	{
		using (var packet = new Packet((int)PacketTypes.loadCar))
		{
			packet.Write(carData);
			packet.Write(carLoaderID);

			SendData(packet);
		}
	}

	public static void BodyPartPacket(ModCarPart carPart, int carLoaderID)
	{
		using (var packet = new Packet((int)PacketTypes.bodyPart))
		{
			packet.Write(carPart);
			packet.Write(carLoaderID);

			SendData(packet);
		}
	}

	public static void PartScriptPacket(ModPartScript partScript, int carLoaderID)
	{
		using (var packet = new Packet((int)PacketTypes.partScript))
		{
			packet.Write(partScript);
			packet.Write(carLoaderID);

			SendData(packet);
		}
	}

	public static void DeleteCarPacket(int carLoaderID)
	{
		using (var packet = new Packet((int)PacketTypes.deleteCar))
		{
			packet.Write(carLoaderID);

			SendData(packet);
		}
	}

	public static void CarPositionPacket(int carLoaderID, int no)
	{
		using (var packet = new Packet((int)PacketTypes.carPosition))
		{
			packet.Write(no);
			packet.Write(carLoaderID);

			SendData(packet);
		}
	}

	public static void GarageUpgradePacket(GarageUpgrade upgrade)
	{
		using (var packet = new Packet((int)PacketTypes.garageUpgrade))
		{
			packet.Write(upgrade);

			SendData(packet);
		}
	}

	public static void JobPacket(ModJob job)
	{
		using (var packet = new Packet((int)PacketTypes.newJob))
		{
			packet.Write(job);
			MelonLogger.Msg("newJob packet");
			SendData(packet);
		}
	}

	public static void JobActionPacket(ModJob job, bool takeJob)
	{
		using (var packet = new Packet((int)PacketTypes.jobAction))
		{
			packet.Write(job);
			packet.Write(takeJob);

			SendData(packet);
		}
	}

	public static void SelectedJobPacket(ModJob job, bool action)
	{
		using (var packet = new Packet((int)PacketTypes.selectedJob))
		{
			MelonLogger.Msg("SelectedJob packet");
			packet.Write(job);
			packet.Write(action);

			SendData(packet);
		}
	}

	public static void EndJobPacket(ModJob modJob)
	{
		using (var packet = new Packet((int)PacketTypes.endJob))
		{
			MelonLogger.Msg("EndJobPacket");
			packet.Write(modJob);

			SendData(packet);
		}
	}

	public static void SceneChangePacket(GameScene scene)
	{
		using (var packet = new Packet((int)PacketTypes.sceneChange))
		{
			packet.Write(scene);

			SendData(packet);
		}
	}

	public static void ToolPositionPacket(IOSpecialType tool, ModCarPlace place, bool playSound = false)
	{
		using (var packet = new Packet((int)PacketTypes.toolMove))
		{
			packet.Write((ModIOSpecialType)tool);
			packet.Write(place);
			packet.Write(playSound);

			SendData(packet);
		}
	}

	public static void SetSpringClampPacket(ModGroupItem item, bool instant, bool mount)
	{
		using (var packet = new Packet((int)PacketTypes.setSpringClamp))
		{
			packet.Write(item);
			packet.Write(instant);
			packet.Write(mount);

			SendData(packet);
		}
	}

	public static void ClearSpringClampPacket()
	{
		using (var packet = new Packet((int)PacketTypes.clearSpringClamp))
		{
			SendData(packet);
		}
	}

	public static void SetTireChangerPacket(ModGroupItem modGroupItem, bool instant, bool connect)
	{
		using (var packet = new Packet((int)PacketTypes.setTireChanger))
		{
			packet.Write(modGroupItem);
			packet.Write(instant);
			packet.Write(connect);

			SendData(packet);
		}
	}

	public static void ClearTireChangerPacket()
	{
		using (var packet = new Packet((int)PacketTypes.clearTireChanger))
		{
			SendData(packet);
		}
	}

	public static void SendOilBin(int carLoaderID)
	{
		using (var packet = new Packet((int)PacketTypes.oilBinUse))
		{
			packet.Write(carLoaderID);
			SendData(packet);
		}
	}

	public static void SendWheelBalancer(int action, GroupItem items=null)
	{
		using (var packet = new Packet((int)PacketTypes.wheelBalance))
		{
			packet.Write(action);
			if (items != null) packet.Write(new ModGroupItem(items));
			SendData(packet);
		}
	}

	public static void SkillChangePacket(string id, List<bool> skill)
	{
		using (var packet = new Packet((int)PacketTypes.skillChange))
		{
			packet.Write(ClientData.UserData.playerGUID);
			packet.Write(id);
			packet.Write(skill);
			
			SendData(packet);
		}
	}

	public static void EngineCraneHandlePacket(int action, int carLoaderID, ModGroupItem modGroupItem = null)
	{
		using (var packet = new Packet((int)PacketTypes.engineCrane))
		{
			packet.Write(action);
			packet.Write(carLoaderID);
			if(action == 1) packet.Write(modGroupItem);
			
			SendData(packet);
		}
	}

	public static void EngineStandAnglePacket(float val, bool useAlt)
	{
		using (var packet = new Packet((int)PacketTypes.engineStandAngle))
		{
			packet.Write(val);
			packet.Write(useAlt);
			
			SendData(packet);
		}
	}

	public static void EngineStandSetGroupPacket(ModGroupItem engineGroupItem, Vector3Serializable position, bool useAlt)
	{
		using (var packet = new Packet((int)PacketTypes.engineStandSetGroup))
		{
			packet.Write(engineGroupItem);
			packet.Write(position);
			packet.Write(useAlt);
			
			SendData(packet);
		}
	}

	public static void TakeOffEnginePacket(bool useAlt)
	{
		using (var packet = new Packet((int)PacketTypes.engineStandTakeOff))
		{
			packet.Write(useAlt);
			
			SendData(packet);
		}
	}

	public static void CarFluid(int carLoaderID, ModFluidData carFluid)
	{
		using (var packet = new Packet((int)PacketTypes.carFluid))
		{
			packet.Write(carLoaderID);
			packet.Write(carFluid);
			SendData(packet);
		}
	}

	public static void DisconnectPacket()
	{
		using (var packet = new Packet((int)PacketTypes.disconnect))
		{
			SendData(packet);
		}
	}

	public static void ResyncCar(int carLoaderID)
	{
		using (var packet = new Packet((int)PacketTypes.resync))
		{
			packet.Write(PacketTypes.loadCar);
			packet.Write(carLoaderID);
			SendData(packet);
		}
	}
	
	public static void ResyncTools()
	{
		using (var packet = new Packet((int)PacketTypes.resync))
		{
			packet.Write(PacketTypes.toolMove);
			SendData(packet);
		}
	}
	
	public static void ResyncUpgrade()
	{
		using (var packet = new Packet((int)PacketTypes.resync))
		{
			packet.Write(PacketTypes.garageUpgrade);
			SendData(packet);
		}
	}

	public static void CarWashPacket(int carLoaderID, bool interior=false)
	{
		using (var packet = new Packet((int)PacketTypes.carWash))
		{
			packet.Write(carLoaderID);
			packet.Write(interior);
			SendData(packet);
		}
	}

	public static void CarPaint(ModColor modColor)
	{
		using (var packet = new Packet((int)PacketTypes.carPaint))
		{
			packet.Write(modColor);
			SendData(packet);
		}
	}

	public static void WelderPacket(int carLoaderID)
	{
		using (var packet = new Packet((int)PacketTypes.useWelder))
		{
			packet.Write(carLoaderID);
			SendData(packet);
		}
	}

	public static void RepairPart(ModPartInfo modPartInfo,  bool isBody, bool success)
	{
		using (var packet = new Packet((int)PacketTypes.repairPart))
		{
			packet.Write(modPartInfo);
			packet.Write(isBody);
			packet.Write(success);
			SendData(packet);
		}
	}

	public static void ResyncEngineStandPacket(bool alt)
	{
		using (var packet = new Packet((int)PacketTypes.resync))
		{
			MelonLogger.Msg("Ask resync for addition engine stand");
			packet.Write(PacketTypes.engineStandSetGroup);
			packet.Write(alt);
			SendData(packet);
		}
	}
	public static void ResyncPark()
	{
		using (var packet = new Packet((int)PacketTypes.resync))
		{
			MelonLogger.Msg("Ask resync for park");
			packet.Write(PacketTypes.parkAdd);
			SendData(packet);
		}
	}

	public static void AddCarToParkPacket(ModNewCarData modNewCarData, int index)
	{
		using (var packet = new Packet((int)PacketTypes.parkAdd))
		{
			MelonLogger.Msg("Add a car to parking");
			packet.Write(modNewCarData);
			packet.Write(index);
			SendData(packet);
		}
	}

	public static void RemoveCarFromParkPacket(int index)
	{
		using (var packet = new Packet((int)PacketTypes.parkRemove))
		{
			MelonLogger.Msg("Remove a car to parking");
			packet.Write(index);
			SendData(packet);
		}
	}

	public static void AskFullSync()
	{
		using (var packet = new Packet((int)PacketTypes.resync))
		{
			packet.Write(PacketTypes.resync);
			SendData(packet);
		}
	}
}