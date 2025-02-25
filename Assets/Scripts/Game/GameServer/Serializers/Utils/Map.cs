using UnityEngine;
using GameCore;
using NUnit.Framework;
using System.Collections.Generic;
using Net;
using System;
using System.Reflection;

namespace Serialization { 
    public class Map<T>: Serializable
    {
        private GameCore.Map<T> map;
        private Type genericSerType;
        public Map(GameCore.Map<T> map, Type genericSerializableType)
        {
            this.map = map;
            genericSerType = genericSerializableType;
        }
        public Map(List<byte[]> bytes)
        {
            this.bytes = bytes;
        }
        public override List<byte[]> Serialize()
        {
            List<byte[]> bytes = PrepareBytes();
            //byte genericClassCode = (byte)Array.IndexOf(serializableObjectsHeadCodes, GetType());
            int width = map.GetSize(0);
            int height = map.GetSize(1);

            //bytes.Add(new byte[] { genericClassCode });
            bytes.Add(Utils.IntToBytes(width));
            bytes.Add(Utils.IntToBytes(height));

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    ConstructorInfo constructor = genericSerType.GetConstructor(new Type[] { typeof(T) });
                    Serializable ser = (Serializable)constructor.Invoke(new object[] { map.Get(x, y) });
                    bytes.AddRange(ser.Serialize());
                }
            }
            InsertPackagesCount(bytes);
            return bytes;
        }

        public override object Deserialize()
        {
            //byte genericClassCode = bytes[0][0];
            int width = Utils.BytesToInt(bytes[0]);
            int height = Utils.BytesToInt(bytes[1]);
            bytes.RemoveRange(0, 2);

            GameCore.Map<T> map = new GameCore.Map<T>(new Vector2Int(width, height));
            for(int x = 0,num = 0; x < width ; x++)
                for(int y = 0; y < height ; y++,num++)
                {
                    Serializable terrainType = Create(bytes);
                    map.Set(x, y, (T)terrainType.Deserialize());
                }
            return map;
        }

    }
}