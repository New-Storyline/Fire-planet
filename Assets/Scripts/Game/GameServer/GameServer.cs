using GameCore;
using Net;
using NUnit.Framework;
using Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameCore.Game;
using static Net.NetworkSourceServer;

public class GameServer : GameServerWrapper
{
    private NetworkSourceServer NSS;
    private Game game;

    public GameServer(string PORT)
    {
        game = new Game(new Vector2Int(16,16),3, 1);
        NSS = new NetworkSourceServer( PORT, OnDataReceived, OnNewConnection, OnConnectionLost);
    }

    public override void Update()
    {
        NSS.Update();
    }

    public void CreateGame(Vector2Int mapSize,int players,int seed)
    {
        game = new Game(mapSize, players, seed);
    }

    public void OnNewConnection(ConnectedPC pc)
    {
        Debug.Log("New Connection!");
    }

    public void OnDataReceived(ConnectedPC pc)
    {
        Debug.Log("Data Received! " + pc.IP);
        Command cmd = Command.CreateAndDeserialize(pc, new List<byte[]>(pc.packages));
        HandleCommand(cmd);
    }

    private void HandleCommand(Command command)
    {
        switch (command.type)
        {
            case Command.CommandType.VoidEvent:
                break;
            case Command.CommandType.Event:
                break;
            case Command.CommandType.VoidCall:
                HandleVoidCall(command);
                break;
        }
    }

    private void HandleVoidCall(Command command)
    {
        switch (command.gameMethod)
        {
            case GameMethod.GetTerrain:
                {
                    GameCore.Map<Game.TerrainType> terrains = game.GetTerrain();
                    Serialization.Map<Game.TerrainType> mapTerrain = new Serialization.Map<Game.TerrainType>(terrains, typeof(Serialization.TerrainType));
                    Command response = Command.CreateResponse(command, mapTerrain.Serialize());
                    SendCommand(response);
                }
                break;
            case GameMethod.GetBuildings:
                {
                    GameCore.Map<GameCore.Building> buildings = game.GetBuildings();
                    Serialization.Map<GameCore.Building> mapBuildings = new Serialization.Map<GameCore.Building>(buildings, typeof(Serialization.Building));
                    Command responseBuildings = Command.CreateResponse(command, mapBuildings.Serialize());
                    SendCommand(responseBuildings);
                }
                break;
            case GameMethod.GetWorldMap:
                {
                    GameCore.Map<Game.TerrainType> terrains = game.GetTerrain();
                    GameCore.Map<GameCore.Building> buildings = game.GetBuildings();
                    GameCore.Map<Unit> units = game.GetUnits();

                    Serializable[] serializableData = new Serializable[] {
                    new Serialization.Map<Game.TerrainType>(terrains,typeof(Serialization.TerrainType)) ,
                    new Serialization.Map<GameCore.Building>(buildings,typeof(Serialization.Building)) 
                    //,
                    //new Serialization.Map<Unit>(units,typeof(Serialization.TerrainType))
                    };

                    ObjectArray objectArray = new ObjectArray(serializableData);

                    Command response = Command.CreateResponse(command, objectArray.Serialize());
                    SendCommand(response);

                    break;
                }
            default:
                throw new Exception($"Can`t find getter method! {command.ToString()}");
        }
    }

    public void OnConnectionLost(ConnectedPC pc)
    {

    }

    protected override void SendData(byte[] bytes, ConnectedPC pc, bool isOverTransmission)
    {
        NSS.SendData(bytes, pc, isOverTransmission);
    }

    public override IEnumerator VoidCall<T>(GameMethod gameMethod, GameClient.OnReceived<T> onReceived, Type genericType = null)
    {
        throw new NotImplementedException();
    }
}
