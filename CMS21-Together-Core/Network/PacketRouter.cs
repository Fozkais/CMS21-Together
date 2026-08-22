using System;
using System.Collections.Generic;
using System.Reflection;
using CMS21_Together_Core.Logging;

namespace CMS21_Together_Core.Network;

public static class PacketRouter
{
	private static Dictionary<PacketTypes, MethodInfo> _handlers = new Dictionary<PacketTypes, MethodInfo>();
	private static Dictionary<Type, PacketTypes> _packetMap = new Dictionary<Type, PacketTypes>();

	/// <summary>
	/// Initializes the router by scanning the Core assembly (for Packets) 
	/// and the specified assembly (for Handlers).
	/// </summary>
	/// <param name="handlerAssembly">The assembly containing the Logic/Handlers (Client or Server)</param>
	public static void Initialize(Assembly handlerAssembly)
	{
		_handlers.Clear();
		_packetMap.Clear();
			
		ScanAssembly(Assembly.GetAssembly(typeof(PacketRouter)));

		// 2. Scan Logic Assembly (To find Handlers in Client or Server)
		ScanAssembly(handlerAssembly);
			
		Log.Info($"[PacketRouter] {_handlers.Count} handlers and {_packetMap.Count} packets registered.");
	}

	private static void ScanAssembly(Assembly assembly)
	{
		if (assembly == null) return;

		var types = assembly.GetTypes();

		foreach (var type in types)
		{
			// A. Register Packet Data Classes (Found mostly in Core)
			var packetAttr = type.GetCustomAttribute<NetworkPacket>();
			if (packetAttr != null)
			{
				if (!_packetMap.ContainsKey(type))
				{
					_packetMap[type] = packetAttr.Type;
				}
			}

			// B. Register Packet Handlers (Found mostly in Server/Client)
			// We scan static methods
			foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
			{
				var handlerAttr = method.GetCustomAttribute<PacketHandler>();
				if (handlerAttr != null)
				{
					if (_handlers.ContainsKey(handlerAttr.Type))
					{
						Log.Warn($"[PacketRouter] Multiple handlers found for packet {handlerAttr.Type}. Ignoring duplicate in {assembly.GetName().Name}.");
						continue;
					}
					_handlers.Add(handlerAttr.Type, method);
				}
			}
		}
	}
		
	public static PacketTypes GetPacketId<T>(T packetData) where T : INetworkData
	{
		if (_packetMap.TryGetValue(packetData.GetType(), out PacketTypes id))
			return id;
		throw new Exception($"packet {packetData.GetType().Name} don't have attribute [NetworkPacket] !");
	}
		
	public static void Dispatch(PacketTypes id, object deserializedData, long senderId)
	{
		if (_handlers.TryGetValue(id, out MethodInfo method))
		{
			try 
			{
				method.Invoke(null, new object[] { senderId, deserializedData });
			}
			catch (Exception ex)
			{
				Log.Error($"[PacketRouter] Error in handler {id}: {ex.InnerException?.Message}");
			}
		}
	}
}