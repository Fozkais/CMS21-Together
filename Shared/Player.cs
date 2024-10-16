using System;
using System.Collections.Generic;
using CMS21Together.ClientSide.Data.PlayerData;
using CMS21Together.ClientSide.Data;
using CMS21Together.Shared.Data;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CMS21Together.Shared
{
    [Serializable]
    public class Player
    {
        public int id;
        public string username;
        public bool isReady;

        public Vector3Serializable position;
        public Vector3Serializable desiredPosition = new Vector3Serializable(0,0,0);
        public QuaternionSerializable rotation;

        public GameScene scene;
        public List<(int, string)> carToResync = new List<(int, string)>();

        public Player(int _id, string _username, Vector3 _spawnPosition)
        {
            id = _id;
            username = _username;
            position = new Vector3Serializable(_spawnPosition);
            rotation = new QuaternionSerializable(Quaternion.identity);
            scene = ModSceneManager.currentScene();
        }

        public void Disconnect()
        {
            if(ClientData.Instance.PlayersGameObjects != null)
                if(ClientData.Instance.PlayersGameObjects.TryGetValue(id, out var o))
                    Object.Destroy(o);
            if(ClientData.Instance.players != null)
                if(ClientData.Instance.players.ContainsKey(id))
                    ClientData.Instance.players.Remove(id);
        }
    }

    [Serializable]
    public class Vector3Serializable
    {
        public float x;
        public float y;
        public float z;

        public static Vector3Serializable Subtract(Vector3Serializable a, Vector3Serializable b)
        {
            return new Vector3Serializable(
                a.x - b.x, 
                a.y - b.y,
                a.z - b.z
            );
        }
        
        public Vector3Serializable(float _x, float _y, float _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }
        
        public Vector3Serializable(Vector3 _vector3)
        {
            x = _vector3.x;
            y = _vector3.y;
            z = _vector3.z;
        }
        
        public Vector3 toVector3()
        {
            return new Vector3(x, y, z);
        }

        public static Vector3Serializable Add(Vector3Serializable a, Vector3Serializable b)
        {
            return new Vector3Serializable(
                a.x + b.x, 
                a.y + b.y,
                a.z + b.z
            );
        }
    }
    [Serializable]
    public class QuaternionSerializable
    {
        public float x;
        public float y;
        public float z;
        public float w;
        
        public QuaternionSerializable(float _x, float _y, float _z, float _w)
        {
            x = _x;
            y = _y;
            z = _z;
            w = _w;
        }

        
        public QuaternionSerializable(Quaternion _quaternion)
        {
            x = _quaternion.x;
            y = _quaternion.y;
            z = _quaternion.z;
            w = _quaternion.w;
        }
        
        public Quaternion toQuaternion()
        {
            return new Quaternion(x, y, z, w);
        }
        
    }
}