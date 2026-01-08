using System;
using System.Collections.Generic;
using System.Reflection;

namespace CMS21_Together_Core.Network;

public static class PacketRouter
{
	private static Dictionary<PacketTypes, MethodInfo> _handlers = new Dictionary<PacketTypes, MethodInfo>();
    private static Dictionary<Type, PacketTypes> _packetMap = new Dictionary<Type, PacketTypes>();

    public static void Initialize(Assembly assemblyToScan)
    {
        _handlers.Clear();
        _packetMap.Clear();

        var types = assemblyToScan.GetTypes();

        foreach (var type in types)
        {
            var packetAttr = type.GetCustomAttribute<NetworkPacket>();
            if (packetAttr != null)
            {
                _packetMap[type] = packetAttr.Type;
            }
            
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
            {
                var handlerAttr = method.GetCustomAttribute<PacketHandler>();
                if (handlerAttr != null)
                {
                    if (_handlers.ContainsKey(handlerAttr.Type))
                    { 
                        Console.WriteLine($"[PacketRouter] found multiple handlers for type {handlerAttr.Type}.");
                        continue;
                    }
                    _handlers.Add(handlerAttr.Type, method);
                }
            }
        }
        Console.WriteLine($"[PacketRouter] {_handlers.Count} handlers and {_packetMap.Count} packet registered.");
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
                method.Invoke(null, [senderId, deserializedData]);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in handler {id}: {ex.InnerException?.Message}");
            }
        }
    }
}