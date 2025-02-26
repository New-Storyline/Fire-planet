﻿
using GameCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class is responsible for handling and rendering the selection of objects/unit moving points in the game. 
/// </summary>
public class SelectionTool
{
    public SelectionType selectionType { get; private set; } = SelectionType.None;
    public Vector2Int selectedTilePosition { get; private set; }
    /// <summary>
    /// 0 - unit selection, 1 - tile selection, 2 - no selection
    /// </summary>
    public int selectLevel { get; private set; } = 2;
    private Render render;
    private GameController GC;
    public SelectionTool(Render render,GameController GC)
    {
        this.GC = GC;
        this.render = render;
    }

    public void IncreaseSelectLevel()
    {
        selectLevel++;

        switch (selectLevel)
        {
            case 0:
                TrySelectUnit();
                break;
            case 1:
                TrySelectTile();
                break;
            case 2:
                ClearSelection();
                break;
            default:
                TrySelectUnit();
                break;
        }
    }

    /// <summary>
    /// Selects proper object based on the current selection level.
    /// </summary>
    /// <param name="pos"></param>
    public void Select(Vector2Int pos)
    {
        selectedTilePosition = pos;

        TrySelectUnit();
    }

    public void ClearSelection()
    {
        selectLevel = 2;

        render.ClearSelection();
        selectionType = SelectionType.None;
        selectedTilePosition = new Vector2Int(-1, -1);

        render.CloseTilePanel();
    }

    private void TrySelectTile()
    {
        selectLevel = 1;

        SelectTile();
    }

    private void TrySelectUnit()
    {
        selectLevel = 0;
        List<Vector2Int> movePoints = new List<Vector2Int>();
        List<Vector2Int> attackPoints = new List<Vector2Int>();

        if (!GC.IsUnitExsist(selectedTilePosition,out movePoints,out attackPoints))
        {
            IncreaseSelectLevel();
            return;
        }

        SelectUnit(movePoints, attackPoints);
    }

    /// <summary>
    /// Selects a tile with unit and renders the selection.
    /// </summary>
    /// <param name="unit"></param>
    private void SelectUnit(List<Vector2Int> movePoints,List<Vector2Int> attackPoints) {

        render.ClearSelection();
        render.ClearUnitSpecPoints();

        render.ShowUnitSelection(selectedTilePosition);
        render.CreateUnitAttackPoints(attackPoints);
        render.CreateUnitMovePoints(movePoints);
        selectionType = SelectionType.Unit;
    }

    /// <summary>
    /// Selects a terrain tile and renders the selection.
    /// </summary>
    /// <param name="unit"></param>
    private void SelectTile()
    {
        render.ClearSelection();
        render.ShowTileSelection(selectedTilePosition);
        selectionType = SelectionType.Tile;

        Building building = null;

        if (GC.IsBuildingWithTypeExsist(selectedTilePosition,typeof(City),out building))
        {
            render.gameGUI.OpenPanel(GameGUI.Mode.City);
            render.gameGUI.SetSelectedCity((City)building);
        }
        else
            render.gameGUI.OpenPanel(GameGUI.Mode.Tile);
    }

    public enum SelectionType
    {
        None,
        Tile,
        Unit
    }
}