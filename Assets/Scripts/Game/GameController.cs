using GameCore;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TerrainUtils;
using static GameCore.Game;

public class GameController : MonoBehaviour
{
    /*
      * Баги/недоработки: 
      * - При движении юнита на гору, он остается на том же уровне, что и на земле 
    */

    public CameraController CC;

    public Vector2Int mapSize;

    private Render render;
    private Game game;
    private SelectionTool SelTool;

    private List<Action> actions = new List<Action>();
    public void Start()
    {
        game = new Game(mapSize,3,1);
        render = GetComponent<Render>();
        SelTool = new SelectionTool(render, this);

        render.InitWorld(this,game.GetTerrain(), game.GetBuildings(), game.GetUnits());
        UpdatePlayerIndicators();

        Vector2 mapScale = render.GetMapScale();
        CC.SetMoveLimits(new Vector3(-5, 0, -5), new Vector3(mapSize.x * mapScale.x, GameConfig.CAMERA_MAX_Y, mapSize.y * mapScale.y));
        CC.TileClicked += OnTileClicked;
        //render.SetWorldState(game.GetBuildings(), game.GetUnits());
    }

    public void OnTileClicked(Vector2Int pos)
    {
        if (pos != SelTool.selectedTilePosition)
        {
            if (SelTool.selectionType == SelectionTool.SelectionType.Unit && (game.IsUnitCanMove(SelTool.selectedTilePosition) || game.IsUnitCanAttack(SelTool.selectedTilePosition))) {

                List<Vector2Int> movePoints = game.GetUnitPossibleMovePoints(SelTool.selectedTilePosition);
                List<Vector2Int> attackPoints = game.GetUnitPossibleAttackPoints(SelTool.selectedTilePosition);

                if(movePoints != null || attackPoints != null)
                {
                    if (movePoints != null && MathUtils.IsListContainsVec2Int(movePoints, pos))
                    {
                        // Unit move
                        game.MoveUnit(SelTool.selectedTilePosition, pos);
                        bool isCanAttack = game.IsUnitCanAttack(pos);
                        AddAndRunAction(new MoveUnit(SelTool.selectedTilePosition, pos, !isCanAttack, render));

                        SelTool.ClearSelection();

                        if (isCanAttack)
                            SelTool.Select(pos);
                    }

                    if (attackPoints != null && MathUtils.IsListContainsVec2Int(attackPoints, pos))
                    {
                        // Unit attack

                        Unit attacker = game.GetUnit(SelTool.selectedTilePosition);
                        Unit defender = game.GetUnit(pos);

                        game.AttackUnit(SelTool.selectedTilePosition, pos);
                        bool isCanAttack = game.IsUnitCanAttack(pos);
                        AddAndRunAction(new AttackUnit(SelTool.selectedTilePosition,pos,attacker, defender, render));

                        SelTool.ClearSelection();

                        if (isCanAttack)
                            SelTool.Select(pos);
                    }

                    return;
                }
            }

            SelTool.Select(pos);
        }
        else
        {
            SelTool.IncreaseSelectLevel();
        }
    }

    public void NextTurn() {

        render.OnNextTurn();
        game.NextTurn();
        UpdatePlayerIndicators();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
            NextTurn();
    }

    private void AddAndRunAction(Action action) {

        actions.Add(action);
        render.RunAction(action);
    }

    public void UpdatePlayerIndicators() {
        Indicator<int> gold = game.GetMainPlayerGoldInd();
        Indicator<float> population = game.GetMainPlayerPopulationInd();
        render.UpdatePlayerIndicatiors(gold.value,gold.growth,population.value,population.growth);
    }

    public bool IsUnitExsist(Vector2Int pos,out List<Vector2Int> movePoints,out List<Vector2Int> attackPoints) {

        movePoints = null;
        attackPoints = null;

        if (game.GetUnit(pos) != null)
        {
            movePoints = game.GetUnitPossibleMovePoints(pos);
            attackPoints = game.GetUnitPossibleAttackPoints(pos);

            return true;
        }
        return false;
    }

    public float GetSelectSpriteYoffset(Vector2Int pos)
    {
        if (game.GetTerrainElem(pos) == Game.TerrainType.mountain)
            return 0.12f;
        return 0;
    }

    /// <summary>
    /// Checks if building with the specified type exists in the specified position.
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    internal bool IsBuildingWithTypeExsist(Vector2Int pos, System.Type type,out Building buildingOut)
    {
        Building building = game.GetBuilding(pos);

        if (building != null && building.GetType() == type){
            buildingOut = building;
            return true;
        }

        buildingOut = null;
        return false;
    }

    internal bool TryBuyUnit(System.Type unitType, City selectedCity)
    {
        return game.TryBuyUnit(unitType, selectedCity);
    }

    internal void RemoveUnitFromSpawnQuene(System.Type unitType, City selectedCity)
    {
        game.RemoveUnitFromSpawnQuene(unitType, selectedCity);
    }
}
