using System;
using System.Collections.Generic;
using UnityEngine;

namespace Serialization
{
    public class Building : Serializable
    {
        private GameCore.Building building;

        private Type[] buildingTypes = new Type[] {
            typeof(GameCore.City),
            typeof(GameCore.Village)
        };

        public Building(GameCore.Building building)
        {
            this.building = building;
        }
        public Building(List<byte[]> bytes)
        {
            this.bytes = bytes;
        }
        public override List<byte[]> Serialize()
        {
            List<byte[]> bytes = PrepareBytes();

            if(building == null)
            {
                InsertPackagesCount(bytes);
                return bytes;
            }

            if (building.GetType() == typeof(GameCore.City))
            {
                City city = new City((GameCore.City)building);
                bytes.AddRange(city.Serialize());
            }
            else if (building.GetType() == typeof(GameCore.Village))
            {
                Village village = new Village((GameCore.Village)building);
                bytes.AddRange(village.Serialize());
            }
            else
                throw new InvalidOperationException($"Building type not found in buildingTypes. Type: {building.GetType().FullName}");
           
            InsertPackagesCount(bytes);
            return bytes;
        }

        public override object Deserialize()
        {
            if(bytes.Count == 0)
            {
                return null;
            }

            return Create(bytes).Deserialize();
        }
    }
}
