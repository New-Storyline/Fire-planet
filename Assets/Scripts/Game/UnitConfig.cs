using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitConfig
{
    private static Dictionary<Type, Dictionary<UnitParam, float>> unitParams = new Dictionary<Type, Dictionary<UnitParam, float>>() {
        { 
            typeof(Rifleman) ,new Dictionary<UnitParam, float>() {
                { UnitParam.GoldCost, 10 },
                { UnitParam.PopulationCost, 5 }
            }
        }
    };

    public static float GetUnitParam(Type unitType, UnitParam param)
    {
        if (!unitParams.ContainsKey(unitType))
            throw new Exception($"Unit {unitType} not registered");
        if (!unitParams[unitType].ContainsKey(param))
            throw new Exception($"Unit {unitType} does not have parameter {param}");
        return unitParams[unitType][param];
    }

    public enum UnitParam
    {
        GoldCost,
        PopulationCost
    }
}
