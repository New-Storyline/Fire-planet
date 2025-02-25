using GameCore;
using Net;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Serialization {
    public abstract class Serializable
    {
        /// <summary>
        /// Array of serializable objects and their head codes to identify each serialized object. Array order index represents head code.
        /// </summary>
        protected static Type[] serializableObjectsHeadCodes = new Type[] {
            typeof(Map<>),
            typeof(TerrainType),
            typeof(Village),
            typeof(Building),
            typeof(City),
            typeof(Player),
            typeof(Unit),
            typeof(ListInt),
            typeof(Vec2Int),
            typeof(ObjectArray)
        };

        protected List<byte[]> bytes;

        /// <summary>
        /// Creates a serializable object and recognizes the head code of the object and packages count to deserialize it.
        /// After that, removes packages (head code, packages count, package data...) nessessary to create the object.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Serializable Create(List<byte[]> bytes)
        {
            byte headCode = bytes[0][0];
            int packagesCount = Utils.BytesToInt(bytes[1]);
            bytes.RemoveRange(0, 2);

            Type type = serializableObjectsHeadCodes[headCode];
            Serializable serializable = (Serializable)Activator.CreateInstance(type, new object[] { bytes.GetRange(0, packagesCount) });
            bytes.RemoveRange(0, packagesCount);
            return serializable;
        }

        public static Serializable CreateGeneric(Type genericType, List<byte[]> bytes)
        {
            byte headCode = bytes[0][0];
            int packagesCount = Utils.BytesToInt(bytes[1]);
            bytes.RemoveRange(0, 2);

            Type type = serializableObjectsHeadCodes[headCode].MakeGenericType(genericType);
            Serializable serializable = (Serializable)Activator.CreateInstance(type, new object[] { bytes.GetRange(0, packagesCount) });
            bytes.RemoveRange(0, packagesCount);
            return serializable;
        }

        /// <summary>
        /// Creates a list of bytes to be sent over the network and adds the head code of the object to the list.
        /// </summary>
        /// <returns></returns>
        public List<byte[]> PrepareBytes()
        {
            Type targetType = GetType().IsGenericType ? GetType().GetGenericTypeDefinition() : GetType();
            int index = Array.FindIndex(serializableObjectsHeadCodes, t => t == targetType);
            if (index == -1)
            {
                throw new InvalidOperationException("Type not found in serializableObjectsHeadCodes.");
            }
            byte headTypeCode = (byte)index;
            return new List<byte[]>() { new byte[] { headTypeCode } };
        }

        protected void InsertPackagesCount(List<byte[]> bytes)
        {
            bytes.Insert(1, Utils.IntToBytes(bytes.Count - 1));
        }
        public abstract List<byte[]> Serialize();
        public abstract object Deserialize();

    }
}