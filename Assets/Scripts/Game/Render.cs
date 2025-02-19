using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TerrainUtils;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class Render : MonoBehaviour
{
    public delegate float GetSpriteYoffset(Vector2Int pos);

    [Header("Prefabs")]
    public GameObject groundPrefab;
    public GameObject mountainPrefab;
    public GameObject villagePrefab;
    public GameObject cityPrefab;
    public GameObject riflemanPrefab;
    [Header("Dynamic GUI Sprites")]
    public GameObject selectUnitSprite;
    public GameObject selectTileSprite;
    public GameObject unitMovePointSprite;

    /// <summary>
    /// Player index -> color material
    /// </summary>
    public Material[] playerColorMaterials;

    public GetSpriteYoffset getSpriteYoffset { get; private set; }

    [SerializeField]
    private Vector2 mapScale;

    private readonly Vector3 TILES_OFFSET = new Vector3(0.5f, 0, 0.5f);

    private Map map;

    private List<GameObject> unitMovePoints = new List<GameObject>();

    public void InitWorld(Map<Game.TerrainType> terrainMap, Map<Building>  buildings, Map<Unit> units)
    {
        BuildTerrain(terrainMap);
        BuildBuildings(buildings);
        BuildUnits(units);
    }

    public void SetDelegates(GetSpriteYoffset getSpriteYoffset) {
        this.getSpriteYoffset = getSpriteYoffset;
    }

    #region Build_World

    private void BuildTerrain(Map<Game.TerrainType> terrainMap)
    {
        map.terrainMap = new Map<GameObject>(new Vector2Int(terrainMap.GetSize(0), terrainMap.GetSize(1)));

        GameObject ground = Instantiate(groundPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        ground.transform.position = new Vector3(terrainMap.GetSize(0) * mapScale.x / 2, 0, terrainMap.GetSize(1) * mapScale.y / 2);
        ground.transform.localScale = new Vector3(terrainMap.GetSize(0) * mapScale.x, 1, terrainMap.GetSize(1) * mapScale.y);

        for (int x = 0; x < terrainMap.GetSize(0); x++)
        {
            for (int y = 0; y < terrainMap.GetSize(1); y++)
            {
                switch (terrainMap.Get(x,y))
                {
                    case Game.TerrainType.mountain:
                        map.terrainMap.Set(x, y, Instantiate(mountainPrefab, LocalPosToGlobal(x, y), Quaternion.identity));
                        break;
                    default:
                        break;
                }
            }
        }
    }

    private void BuildBuildings(Map<Building> buildingsMap)
    {
        map.buildingsMap = new Map<GameObject>(new Vector2Int(buildingsMap.GetSize(0), buildingsMap.GetSize(1)));

        for (int x = 0; x < buildingsMap.GetSize(0); x++)
        {
            for (int y = 0; y < buildingsMap.GetSize(1); y++)
            {
                Building building = buildingsMap.Get(x,y);

                if (building == null)
                    continue;

                CreateBuilding(building);
            }
        }
    }

    private void BuildUnits(Map<Unit> units)
    {
        map.unitsMap = new Map<GameObject>(new Vector2Int(units.GetSize(0), units.GetSize(1)));
        for (int x = 0; x < units.GetSize(0); x++)
        {
            for (int y = 0; y < units.GetSize(1); y++)
            {
                Unit unit = units.Get(x, y);

                if (unit == null)
                    continue;

                CreateUnit(unit);
            }
        }
    }

    #endregion

    public void CreateBuilding(Building building) {

        if (building is Village)
        {
            map.buildingsMap.Set(building.GetPosition(), Instantiate(villagePrefab, LocalPosToGlobal(building.GetPosition()), Quaternion.identity));
        }

        if (building is City)
        {
            City city = (City)building;
            GameObject cityObj = Instantiate(cityPrefab, LocalPosToGlobal(city.GetPosition()), Quaternion.identity);
            map.buildingsMap.Set(city.GetPosition(), cityObj);
            cityObj.GetComponent<CityObject>().SetMaterial(playerColorMaterials[(int)city.Owner.Team]);
        }

    }
    public void CreateUnit(Unit unit)
    {
        GameObject unitObj = null;

        if (unit is Rifleman)
        {
            Vector2Int pos = unit.GetPosition();
            unitObj = Instantiate(riflemanPrefab, LocalPosToGlobal(pos), Quaternion.identity);
            map.unitsMap.Set(pos, unitObj);
        }

        unitObj.GetComponent<UnitObject>().SetMaterial(playerColorMaterials[(int)unit.Owner.Team]);
    }

    public Vector3 LocalPosToGlobal(Vector2Int pos) {

        return new Vector3(pos.x * mapScale.x + TILES_OFFSET.x, 0, pos.y * mapScale.y + TILES_OFFSET.z);
    }

    public Vector3 LocalPosToGlobal(int x,int y)
    {
        return new Vector3(x * mapScale.x + TILES_OFFSET.x, 0, y * mapScale.y + TILES_OFFSET.z);
    }

    internal void SetWorldState(Building[,] buildingsMap, Unit[,] unitsMap)
    {
        throw new NotImplementedException();
    }

    public void RunAction(Action action)
    {
        action.Run(map);
    }

    public Vector2 GetMapScale()
    {
        return mapScale;
    }

    public void OnNextTurn()
    {
        for(int x = 0; x < map.unitsMap.GetSize(0); x++)
        {
            for (int y = 0; y < map.unitsMap.GetSize(1); y++)
            {
                if (map.unitsMap.Get(x, y) != null)
                {
                    GameObject unit = map.unitsMap.Get(x, y);
                    unit.transform.localScale = new Vector3(1, 1, 1);
                }
            }
        }
    }

    #region Selection_And_Movement

    public void ClearSelection()
    {
        ClearUnitMovePos();
        selectUnitSprite.SetActive(false);
        selectTileSprite.SetActive(false);
    }

    private void ClearUnitMovePos()
    {
        foreach (GameObject movePoint in unitMovePoints)
        {
            Destroy(movePoint);
        }
    }

    public void ShowUnitSelection(Vector2Int pos)
    {
        selectUnitSprite.SetActive(true);
        selectUnitSprite.transform.position = new Vector3(pos.x * mapScale.x + TILES_OFFSET.x, getSpriteYoffset(pos) + 0.505f, pos.y * mapScale.y + TILES_OFFSET.z);
    }

    public void ShowTileSelection(Vector2Int pos)
    {
        selectTileSprite.SetActive(true);
        selectTileSprite.transform.position = new Vector3(
            pos.x * mapScale.x + TILES_OFFSET.x,
            getSpriteYoffset(pos) + 0.505f, 
            pos.y * mapScale.y + TILES_OFFSET.z
        );
    }

    public void CreateUnitMovePoints(List<Vector2Int> movePoints)
    {
        ClearUnitMovePos();

        if (movePoints == null)
            return;

        foreach (Vector2Int movePoint in movePoints)
        {
            GameObject movePointObj = Instantiate(
                unitMovePointSprite, 
                new Vector3(
                    movePoint.x * mapScale.x + TILES_OFFSET.x,
                    getSpriteYoffset(movePoint) + 0.505f, 
                    movePoint.y * mapScale.y + TILES_OFFSET.z), 
                Quaternion.identity);
            unitMovePoints.Add(movePointObj);
        }
    }

    #endregion

    public struct Map {

        public Map<GameObject> terrainMap;
        public Map<GameObject> buildingsMap;
        public Map<GameObject> unitsMap;

    }
}
