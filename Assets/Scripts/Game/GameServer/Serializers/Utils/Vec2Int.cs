using Net;
using System.Collections.Generic;
using UnityEngine;

namespace Serialization
{
    public class Vec2Int : Serializable
    {
        private UnityEngine.Vector2Int vec;
        public Vec2Int(Vector2Int vec)
        {
            this.vec = vec;
        }
        public Vec2Int(List<byte[]> bytes)
        {
            this.bytes = bytes;
        }
        public override List<byte[]> Serialize()
        {
            List<byte[]> bytes = PrepareBytes();
            bytes.Add(Utils.IntToBytes(vec.x));
            bytes.Add(Utils.IntToBytes(vec.y));
            InsertPackagesCount(bytes);
            return bytes;
        }
        public override object Deserialize()
        {
            return new Vector2Int(Utils.BytesToInt(bytes[0]), Utils.BytesToInt(bytes[1]));
        }
    }
}
