using GameCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TerrainUtils;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class Render : MonoBehaviour
{
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
    [Header("Canvas")]
    public TextMeshProUGUI playerIndicatorsText;
    public GameObject unitSpawnListElem;
    public Transform unitTrainListContainer;

    /// <summary>
    /// Player index -> color material
    /// </summary>
    public Material[] playerColorMaterials;

    [SerializeField]
    private Vector2 mapScale;

    private readonly Vector3 TILES_OFFSET = new Vector3(0.5f, 0, 0.5f);

    private Map map;
    public GameGUI gameGUI;
    public GameController GC { get; private set; }

    /// <summary>
    /// List of all unit move and attack points
    /// </summary>
    private List<GameObject> unitSpecPoints = new List<GameObject>();

    public void InitWorld(GameController GC, Map<Game.TerrainType> terrainMap, Map<Building>  buildings, Map<Unit> units)
    {
        this.GC = GC;
        gameGUI = GetComponent<GameGUI>();
        gameGUI.Init(this,GC);
        BuildTerrain(terrainMap);
        BuildBuildings(buildings);
        BuildUnits(units);
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
    public void CreateBuilding(Building building)
    {

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

        UnitObject unitObject = unitObj.GetComponent<UnitObject>();
        unitObject.SetMaterial(playerColorMaterials[(int)unit.Owner.Team]);
        unitObject.SetHP(unit.HP, unit.GetMaxHP());
    }
    #endregion

    public void RunAction(Action action)
    {
        action.Run(map);
    }
    public void UpdatePlayerIndicatiors(int gold,int goldGrowth,float population,float populationGrowth)
    {
        population = (float)Math.Round(population, 1);
        populationGrowth = (float)Math.Round(populationGrowth, 1);
        playerIndicatorsText.text = $"Gold: {gold} (+{goldGrowth})\nPopulation: {population} (+{populationGrowth})\n";
    }
    public void SetWorldState(Building[,] buildingsMap, Unit[,] unitsMap)
    {
        throw new NotImplementedException();
    }
    public void OnNextTurnButtonClick() { GC.NextTurn();}
    public void OnNextTurn()
    {
        for (int x = 0; x < map.unitsMap.GetSize(0); x++)
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
    public Vector3 LocalPosToGlobal(Vector2Int pos) {

        return new Vector3(pos.x * mapScale.x + TILES_OFFSET.x, 0, pos.y * mapScale.y + TILES_OFFSET.z);
    }
    public Vector3 LocalPosToGlobal(int x,int y)
    {
        return new Vector3(x * mapScale.x + TILES_OFFSET.x, 0, y * mapScale.y + TILES_OFFSET.z);
    }
    public Vector2 GetMapScale()
    {
        return mapScale;
    }

    #region Selection_And_Movement

    public void ClearSelection()
    {
        ClearUnitSpecPoints();
        selectUnitSprite.SetActive(false);
        selectTileSprite.SetActive(false);
    }

    public void ClearUnitSpecPoints()
    {
        foreach (GameObject point in unitSpecPoints)
        {
            Destroy(point);
        }
    }

    public void ShowUnitSelection(Vector2Int pos)
    {
        selectUnitSprite.SetActive(true);
        selectUnitSprite.transform.position = new Vector3(pos.x * mapScale.x + TILES_OFFSET.x, GC.GetSelectSpriteYoffset(pos) + 0.505f, pos.y * mapScale.y + TILES_OFFSET.z);
    }

    public void ShowTileSelection(Vector2Int pos)
    {
        selectTileSprite.SetActive(true);
        selectTileSprite.transform.position = new Vector3(
            pos.x * mapScale.x + TILES_OFFSET.x,
            GC.GetSelectSpriteYoffset(pos) + 0.505f, 
            pos.y * mapScale.y + TILES_OFFSET.z
        );
    }

    public void CreateUnitMovePoints(List<Vector2Int> movePoints)
    {

        if (movePoints == null)
            return;

        foreach (Vector2Int movePoint in movePoints)
        {
            GameObject movePointObj = Instantiate(
                unitMovePointSprite, 
                new Vector3(
                    movePoint.x * mapScale.x + TILES_OFFSET.x,
                    GC.GetSelectSpriteYoffset(movePoint) + 0.505f, 
                    movePoint.y * mapScale.y + TILES_OFFSET.z), 
                Quaternion.identity);
            unitSpecPoints.Add(movePointObj);
        }
    }

    internal void CreateUnitAttackPoints(List<Vector2Int> attackPoints)
    {
        if (attackPoints == null)
            return;

        foreach (Vector2Int attackPoint in attackPoints)
        {
            GameObject attackPointObj = Instantiate(
                selectUnitSprite,
                new Vector3(
                    attackPoint.x * mapScale.x + TILES_OFFSET.x,
                    GC.GetSelectSpriteYoffset(attackPoint) + 0.505f,
                    attackPoint.y * mapScale.y + TILES_OFFSET.z),
                Quaternion.identity);
            attackPointObj.GetComponentInChildren<SpriteRenderer>().color = Color.red;
            unitSpecPoints.Add(attackPointObj);
        }
    }

    internal void CloseTilePanel()
    {
        gameGUI.ClosePanel();
    }

    #endregion

    public struct Map {

        public Map<GameObject> terrainMap;
        public Map<GameObject> buildingsMap;
        public Map<GameObject> unitsMap;

    }
}
