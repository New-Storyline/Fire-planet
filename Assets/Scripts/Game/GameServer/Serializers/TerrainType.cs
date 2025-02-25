using GameCore;
using System.Collections.Generic;
using UnityEngine;

namespace Serialization
{
    public class TerrainType : Serializable
    {
        private Game.TerrainType terrainType;

        public TerrainType(Game.TerrainType terrainType)
        {
            this.terrainType = terrainType;
        }

        public TerrainType(List<byte[]> bytes)
        {
            this.bytes = bytes;
        }
        public override List<byte[]> Serialize()
        {
            List<byte[]> bytes = PrepareBytes();
            bytes.Add(new byte[] { (byte)terrainType });
            InsertPackagesCount(bytes);
            return bytes;
        }

        public override object Deserialize()
        {
            return (Game.TerrainType)bytes[0][0];
        }
    }
}
