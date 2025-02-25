using GameCore;
using System.Collections.Generic;
using UnityEngine;

namespace Serialization
{
    public class Village : Serializable
    {
        private GameCore.Village village;
        public Village(GameCore.Village village)
        {
            this.village = village;
        }
        public Village(List<byte[]> bytes)
        {
            this.bytes = bytes;
        }
        public override List<byte[]> Serialize()
        {
            List<byte[]> bytes = PrepareBytes();
            Vec2Int position = new Vec2Int(village.GetPosition());
            bytes.AddRange(position.Serialize());
            InsertPackagesCount(bytes);
            return bytes;
        }

        public override object Deserialize()
        {
            Vec2Int position = (Vec2Int)Create(bytes);
            return new GameCore.Village((Vector2Int)position.Deserialize());
        }
    }
}
