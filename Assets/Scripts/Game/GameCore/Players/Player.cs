using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class Player
    {
        public int gold { get; private set; } = GameConfig.GAME_START_GOLD;
        public float population { get; private set; }

        private List<City> cities = new List<City>();
        private List<Unit> units = new List<Unit>();

        public TeamColor Team { get; private set; }

        public Player(TeamColor team)
        {
            Team = team;
        }

        public City GetCity(int num) { return cities[num]; }
        public Unit GetUnit(int num) { return units[num]; }
        public float GetPopulationGrowth() { return 1f; }
        public int GetGoldGrowth() { return 1; }
        public void LoadGold(int gold) { this.gold = gold; }
        public void LoadPopulation(float population) { this.population = population; }
        internal void AddCity(City city) { cities.Add(city); }
        internal void AddUnit(Unit unit) { units.Add(unit); }
        internal void AddGold(int gold)
        {
            this.gold += gold;
        }
        internal void AddPopulation(float population)
        {
            this.population += population;
        }
        internal void RemoveUnit(Unit unit) { units.Remove(unit); }

        public int GetCitiesCount()
        {
            return cities.Count;
        }

        private void ActivateUnits()
        {
            foreach (var unit in units)
            {
                unit.isCanMoveInTurn = true;
                unit.isCanAttackInTurn = true;
            }
        }

        internal void NextTurn()
        {
            gold += GetGoldGrowth();
            population += GetPopulationGrowth();

            ActivateUnits();
        }

        public enum TeamColor
        {
            Red,
            Blue,
            Green,
            Yellow
        }
    }
}
