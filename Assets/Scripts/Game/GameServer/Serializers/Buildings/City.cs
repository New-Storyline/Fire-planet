using GameCore;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Serialization
{
    public class City : Serializable
    {
        private GameCore.City city;
        public City(GameCore.City city)
        {
            this.city = city;
        }
        public City(List<byte[]> bytes)
        {
            this.bytes = bytes;
        }
        public override List<byte[]> Serialize()
        {
            List<byte[]> bytes = PrepareBytes();
            Player player = new Player(city.Owner);
            
            ListInt unitsTypesList = new ListInt(GetUnitsTypesList(city.untisSpawnQuene));
            Vec2Int position = new Vec2Int(city.GetPosition());

            bytes.AddRange(player.Serialize());
            bytes.AddRange(unitsTypesList.Serialize());
            bytes.AddRange(position.Serialize());
            InsertPackagesCount(bytes);
            return bytes;
        }
        public override object Deserialize()
        {
            Player player = (Player)Create(bytes);
            ListInt unitsTypesList = (ListInt)Create(bytes);
            Vec2Int position = (Vec2Int)Create(bytes);

            city = new GameCore.City((Vector2Int)position.Deserialize());
            city.LoadUntisSpawnQuene(GetUnitsTypesList((List<int>)unitsTypesList.Deserialize()));
            city.SetOwner((GameCore.Player)player.Deserialize());
            return city;
        }

        private List<int> GetUnitsTypesList(List<Type> types)
        {
            List<int> result = new List<int>();
            foreach (Type type in types)
            {
                result.Add(Array.IndexOf(Unit.unitTypes, type));
            }
            return result;
        }
        private List<Type> GetUnitsTypesList(List<int> types)
        {
            List<Type> result = new List<Type>();
            foreach (int type in types)
            {
                result.Add(Unit.unitTypes[type]);
            }
            return result;
        }
    }
}
