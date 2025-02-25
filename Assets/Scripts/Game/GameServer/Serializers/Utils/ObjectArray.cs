using System;
using System.Collections.Generic;
using UnityEngine;
using static GameCore.Game;

namespace Serialization
{
    public class ObjectArray : Serializable
    {
        private static System.Type[] genericTypes = new System.Type[] { 
            typeof(GameCore.Game.TerrainType),
            typeof(GameCore.Building),
            typeof(GameCore.Unit),
        };
        private Serializable[] objects;
        public ObjectArray(Serializable[] objects)
        {
            this.objects = objects;
        }

        public ObjectArray(List<byte[]> bytes)
        {
            this.bytes = bytes;
        } 

        public override List<byte[]> Serialize()
        {
            if(objects.Length > 255)
                throw new System.Exception("ObjectArray can't have more than 255 objects");

            List<byte[]> bytes = PrepareBytes();
            bytes.Add(new byte[] { (byte)objects.Length});

            foreach (Serializable obj in objects)
            {
                if (obj.GetType().IsGenericType)
                {
                    bytes.Add(new byte[] { (byte)Array.IndexOf(genericTypes, obj.GetType().GetGenericArguments()[0]) });
                }
                else
                {
                    bytes.Add(new byte[] { 255 });
                }
            }

            foreach (Serializable obj in objects)
            {
                bytes.AddRange(obj.Serialize());
            }
            InsertPackagesCount(bytes);
            return bytes;
        }
        public override object Deserialize()
        {
            byte objectsCount = bytes[0][0];
            bytes.RemoveRange(0, 1);

            Type[] genericTypes = new Type[objectsCount];
            for (int i = 0; i < objectsCount; i++)
            {
                byte type = bytes[0][0];
                bytes.RemoveAt(0);
                if (type == 255)
                {
                    genericTypes[i] = null;
                }
                else
                {
                    genericTypes[i] = ObjectArray.genericTypes[type];
                }
            }

            object[] objects = new object[objectsCount];
            for (int i = 0; i < objectsCount; i++)
            {
                Serializable obj = null;

                if (genericTypes[i] == null)
                    obj = Create(bytes);
                else
                    obj = CreateGeneric(genericTypes[i], bytes);

                objects[i] = obj.Deserialize();
            }
            return objects;
        }
    }
}
