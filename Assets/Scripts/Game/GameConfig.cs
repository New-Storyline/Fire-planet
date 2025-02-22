using System.Collections.Generic;
using UnityEngine;

public class GameConfig
{
    // --------- Camera ---------
    public const float SCROLL_WHEEL_SPEED_MULTIPLIER = 20;
    public const float CAMERA_SPEED = 8f;


    // --------- Game ---------
    public const int MAX_PLAYERS = 4;
    public const int VILLAGE_MIN_DIST = 5;

    public static readonly Dictionary<WorldGen.WorldType,float> MOUNTIANS_SPAWN_RARES = new Dictionary<WorldGen.WorldType, float> {
        {WorldGen.WorldType.defaultWorld, 0.1f}
    };
    public const int GAME_START_GOLD = 5;
    public const float CAMERA_MAX_Y = 20;
}
