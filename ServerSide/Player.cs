using System;
using System.Collections.Generic;
using System.IO;
using CMS21MP.ClientSide;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using UnityEngine;

namespace CMS21MP.ServerSide
{
    [Serializable]
    public class Player
    {
        public int id;
        public string username;
        public bool isReady;

        public Vector3Serializable position;
        public QuaternionSerializable rotation;
        
        public Player(int _id, string _username, Vector3 _spawnPosition)
        {
            id = _id;
            username = _username;
            position = new Vector3Serializable(_spawnPosition);
            rotation = new QuaternionSerializable(Quaternion.identity);
        }
    }

    [Serializable]
    public class Vector3Serializable
    {
        public float x;
        public float y;
        public float z;
        
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