using GameCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameGUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject tilePanel;
    public GameObject trainPanel;
    public GameObject trainPanelListContainer;
    public GameObject trainPanelElemPrefab;
    [Header("Buttons")]
    public GameObject trainBtn;
    public GameObject buildFarmBtn;
    public GameObject buildFactoryBtn;

    public UnityEngine.UI.Button addRiflemanBtn;
    public UnityEngine.UI.Button closeTrainPanel;

    private Render render;
    private GameController GC;
    private Mode mode;
    private City selectedCity;
    
    private Dictionary<Type,string> unitNames = new Dictionary<Type, string> {
        {typeof(Rifleman),"Rifleman"},
    };
    public void Init(Render render, GameController GC)
    {
        this.render = render;
        this.GC = GC;

        addRiflemanBtn.onClick.AddListener(() => OnAddUnitBtnClick(typeof(Rifleman)));
        closeTrainPanel.onClick.AddListener(OnTrainPanelCloseClick);

        trainBtn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnTrainBtnClick);
        buildFactoryBtn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnBuildFactoryBtnClick);
        buildFarmBtn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(OnBuildFarmBtnClick);

        ClosePanel();
    }

    /// <summary>
    /// Open panel with actions for the desired mode
    /// </summary>
    /// <param name="mode"></param>
    public void OpenPanel(Mode mode) {

        this.mode = mode;
        tilePanel.SetActive(true);
        trainBtn.SetActive(false);
        buildFarmBtn.SetActive(false);
        buildFactoryBtn.SetActive(false);

        switch (mode)
        {
            case Mode.City:
                trainBtn.SetActive(true);
                break;
            case Mode.Tile:
                buildFarmBtn.SetActive(true);
                buildFactoryBtn.SetActive(true);
                break;
        }
    }

    public void ClosePanel()
    {
        tilePanel.SetActive(false);
    }

    public void OnBuildFarmBtnClick()
    {

    }

    public void OnBuildFactoryBtnClick()
    {

    }

    #region City_interaction

    public void OnTrainBtnClick()
    {
        ClearAllChildren(trainPanelListContainer.transform);

        List<Type> unitSpawnQuene = selectedCity.untisSpawnQuene;

        foreach (Type unitType in unitSpawnQuene)
        {
            AddUnitToList(unitType);
        }
        trainPanel.SetActive(true);

        GC.CC.isCanSelectTiles = false;
        GC.CC.isCameraMoveEnabled = false;
    }

    public void OnAddUnitBtnClick(Type unitType)
    {
        if (!GC.TryBuyUnit(unitType,selectedCity))
            return;

        GC.UpdatePlayerIndicators();
        AddUnitToList(unitType);
    }

    public void OnRemoveUnitBtnClick(Type unitType,GameObject listElem)
    {
        Destroy(listElem);
        GC.RemoveUnitFromSpawnQuene(unitType,selectedCity);
        GC.UpdatePlayerIndicators();
    }

    public void OnTrainPanelCloseClick() { 
        
        trainPanel.SetActive(false);
        GC.CC.isCanSelectTiles = true;
        GC.CC.isCameraMoveEnabled = true;
    }

    internal void SetSelectedCity(City building)
    {
        selectedCity = building;
    }

    private void AddUnitToList(Type unitType) {

        GameObject unitListElem = Instantiate(trainPanelElemPrefab, trainPanelListContainer.transform);
        unitListElem.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = unitNames[unitType];
        unitListElem.GetComponentInChildren<UnityEngine.UI.Button>().onClick.AddListener(() => OnRemoveUnitBtnClick(unitType, unitListElem));
    }

    #endregion

    private void ClearAllChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }

    private void SetActiveRecursively(GameObject obj, bool isActive)
    {
        obj.SetActive(isActive);
        foreach (Transform child in obj.transform)
        {
            SetActiveRecursively(child.gameObject, isActive);
        }
    }

    public enum Mode { 
        City,
        Tile,
    }
}
