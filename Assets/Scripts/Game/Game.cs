using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Game
{
    private Map<TerrainType> terrainMap;
    private Map<Building> buildingsMap;
    private Map<Unit> unitsMap;

    private PlayerController PC;

    public Game(Vector2Int mapSize, int playersCount, int seed)
    {

        UnityEngine.Random.InitState(seed);

        terrainMap = new Map<TerrainType>(mapSize);
        buildingsMap = new Map<Building>(mapSize);
        unitsMap = new Map<Unit>(mapSize);

        PC = new PlayerController(playersCount, CaputeSettlement, SpawnUnit);

        WorldGen.GenerateWorld(terrainMap, buildingsMap, WorldGen.WorldType.defaultWorld);
        SetStartSettlementsForPlayers();
        PC.DistributeStartUnits();
    }

    /// <summary>
    /// Finds added villages and distributes them among players, one by one
    /// </summary>
    private void SetStartSettlementsForPlayers()
    {
        List<Village> findVillages()
        {
            List<Village> villages = new List<Village>();

            for (int x = 0; x < buildingsMap.GetSize().x; x++)
            {
                for (int y = 0; y < buildingsMap.GetSize().y; y++)
                {
                    if (buildingsMap.Get(new Vector2Int(x, y)) is Village)
                        villages.Add((Village)buildingsMap.Get(new Vector2Int(x, y)));
                }
            }
            return villages;
        }

        List<Village> villages = findVillages();

        PC.DistributeVillages(villages);
    }

    public void CaputeSettlement(Player player, Vector2Int pos)
    {
        if (buildingsMap.Get(pos) is Village)
        {
            City city = new City(pos);
            city.SetOwner(player);
            buildingsMap.Set(pos, city);
        }
        else
        {
            City city = (City)buildingsMap.Get(pos);
            city.SetOwner(player);
        }
    }

    public void SpawnUnit(Unit unit)
    {
        if (!isValidUnitSpawn(unit))
            return;

        unit.Owner.AddUnit(unit);
        unitsMap.Set(unit.GetPosition(), unit);
    }

    public void MoveUnit(Vector2Int unitPos, Vector2Int newPos)
    {
        Unit unit = unitsMap.Get(unitPos);

        if (unit == null)
            throw new Exception($"Cannot move a unit from an empty tile! From: {unitPos.ToString()}. To: {newPos.ToString()}");

        if(!unit.isCanMoveInTurn)
            throw new Exception($"Cannot move a unit in the same turn! From: {unitPos.ToString()}. To: {newPos.ToString()}");

        if (!MathUtils.IsListContainsVec2Int(GetUnitPossibleMovePoints(unitPos), newPos))
            throw new Exception($"Cannot move a unit to a non-adjacent tile! From: {unitPos.ToString()}. To: {newPos.ToString()}");

        unitsMap.Move(unitPos, newPos);
        unit.OnMove(GetUnitPossibleAttackPoints(newPos).Count != 0);
    }

    public void NextTurn()
    {
        PC.NextTurn();
    }

    #region Getters

    internal Map<Building> GetBuildings()
    {
        return buildingsMap;
    }

    internal Map<TerrainType> GetTerrain()
    {
        return terrainMap;
    }

    internal Map<Unit> GetUnits()
    {
        return unitsMap;
    }
    internal List<Vector2Int> GetUnitPossibleMovePoints(Vector2Int unitPos)
    {
        if (
            !IsUnitCanMove(unitPos) || 
            PC.GetCurrentPlayer() != unitsMap.Get(unitPos).Owner
        )
            return null;

        int moveRange = unitsMap.Get(unitPos).GetMoveRange();

        List <Vector2Int> possibleMoves = new List<Vector2Int>();
        for (int x = -moveRange; x <= moveRange;x++)
            for(int y = -moveRange; y <= moveRange; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                Vector2Int newPos = unitPos + new Vector2Int(x, y);

                if (!isPosValid(newPos))
                    continue;

                if (unitsMap.Get(newPos) != null)
                    continue;

                possibleMoves.Add(newPos);
            }
       
        return possibleMoves;
    }

    public List<Vector2Int> GetUnitPossibleAttackPoints(Vector2Int unitPos)
    {
        Unit unit = unitsMap.Get(unitPos);

        if (
            !IsUnitCanAttack(unitPos) ||
            PC.GetCurrentPlayer() != unit.Owner
        )
            return null;

        int attackRange = unit.GetAttackRange();

        List<Vector2Int> possibleAttackMoves = new List<Vector2Int>();
        for (int x = -attackRange; x <= attackRange; x++)
            for (int y = -attackRange; y <= attackRange; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                Vector2Int newPos = unitPos + new Vector2Int(x, y);

                if (!isPosValid(newPos))
                    continue;

                Unit currUnit = unitsMap.Get(newPos);

                if (currUnit == null || currUnit.Owner.Team == unit.Owner.Team)
                    continue;

                possibleAttackMoves.Add(newPos);
            }

        return possibleAttackMoves;
    }

    internal Unit GetUnit(Vector2Int pos)
    {
        return unitsMap.Get(pos);
    }

    internal TerrainType GetTerrainElem(Vector2Int pos)
    {
        return terrainMap.Get(pos);
    }

    #endregion

    private bool isValidUnitSpawn(Unit unit)
    {
        if (!isPosValid(unit.GetPosition()))
            return false;

        int cityCount = unit.Owner.GetCitiesCount();
        for (int i = 0; i < cityCount; i++)
        {
            Vector2Int cityPos = unit.Owner.GetCity(i).GetPosition();
            if (MathUtils.IsPosInRange(unit.GetPosition(), cityPos - new Vector2Int(1, 1), cityPos + new Vector2Int(1, 1)))
                return true;
        }
        return false;
    }

    private bool isPosValid(Vector2Int pos) {

        return MathUtils.IsPosInRange(pos,Vector2Int.zero, terrainMap.GetSize() - new Vector2Int(1,1));
    }

    public bool IsUnitCanAttack(Vector2Int pos)
    {
        return unitsMap.Get(pos).isCanAttackInTurn;
    }

    public bool IsUnitCanMove(Vector2Int pos)
    {
        return unitsMap.Get(pos).isCanMoveInTurn;
    }

    public enum TerrainType
    {
        ground,
        mountain
    }
}
