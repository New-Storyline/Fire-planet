using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    private List<City> cities = new List<City>();
    private List<Unit> units = new List<Unit>();

    public TeamColor Team { get; private set; }

    public Player(TeamColor team)
    {
        Team = team;
    }

    public City GetCity(int num) { return cities[num]; }
    public Unit GetUnit(int num) { return units[num]; }
    public void AddCity(City city) { cities.Add(city); }
    public void AddUnit(Unit unit) { units.Add(unit); }
    internal void RemoveUnit(Unit unit) { units.Remove(unit); }

    public int GetCitiesCount()
    {
        return cities.Count;
    }

    public void ActivateUnits()
    {
        foreach (var unit in units)
        {
            unit.isCanMoveInTurn = true;
            unit.isCanAttackInTurn = true;
        }
    }

    public enum TeamColor
    {
        Red,
        Blue,
        Green,
        Yellow
    }
}
