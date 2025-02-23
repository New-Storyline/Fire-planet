using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

namespace GameCore { 
    public class City : Building
    {
        public Player Owner { get; private set; }
        public List<Type> untisSpawnQuene { get; private set; } = new List<Type>();
        public City(Vector2Int newPos) : base(newPos)
        {
        }

        internal void SetOwner(Player player)
        {
            Owner = player;
            player.AddCity(this);
        }

        internal void RemoveUnitFromSpawnQuene(Type unitType)
        {
            if(!untisSpawnQuene.Contains(unitType))
                throw new Exception($"Unit not in spawn quene. untisSpawnQuene length: {untisSpawnQuene.Count}");

            untisSpawnQuene.Remove(unitType);
            Owner.AddGold((int)UnitConfig.GetUnitParam(unitType,UnitConfig.UnitParam.GoldCost));
            Owner.AddPopulation(UnitConfig.GetUnitParam(unitType, UnitConfig.UnitParam.PopulationCost));
        }

        internal bool AddUnitToTrainQuene(Type unitType)
        {
            int goldCost = (int)UnitConfig.GetUnitParam(unitType, UnitConfig.UnitParam.GoldCost);
            float populationCost = UnitConfig.GetUnitParam(unitType, UnitConfig.UnitParam.PopulationCost);

            if (Owner.gold - goldCost <= 0 || Owner.population - populationCost <= 0)
                return false;

            Owner.AddGold(-goldCost);
            Owner.AddPopulation(-populationCost);

            untisSpawnQuene.Add(unitType);
            return true;
        }
    }
}