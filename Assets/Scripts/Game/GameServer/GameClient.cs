using GameCore;
using Net;
using Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public class GameClient : GameServerWrapper
{
    private NetworkSource NS;

    public GameClient(string IP, string PORT)
    {
        NS = new NetworkSource(IP, PORT, OnConnected, OnDataReceived);
        NS.Connect();
    }

    public IEnumerator GetTerrain(OnReceived<GameCore.Map<Game.TerrainType>> onReceived)
    {
        Command.CommandType commandType = Command.CommandType.VoidCall;
        GameMethod gameMethod = GameMethod.GetWorldMap;

        SendCommand(Command.CreateCommandClient(commandType, gameMethod, null));

        Command response = null;

        yield return new WaitUntil(() =>
        {
            response = FindResponseCommand(commandType, gameMethod);
            return response != null;
        });

        Serialization.Map<Game.TerrainType> mapTerrainSerializer = (Serialization.Map<Game.TerrainType>)Serializable.Create(response.bytes);
        onReceived((GameCore.Map<Game.TerrainType>)mapTerrainSerializer.Deserialize());
    }

    /// <summary>
    /// Call method on server without args and with return value/values
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="gameMethod">Method to call</param>
    /// <param name="onReceived">Delegate to call when response is received</param>
    /// <param name="genericType">Generic type, if using generic class (for example, Map<Game.TerrainType>)</param>
    /// <returns></returns>
    public override IEnumerator VoidCall<T>(GameMethod gameMethod, OnReceived<T> onReceived,Type genericType=null)
    {
        Command.CommandType commandType = Command.CommandType.VoidCall;
        SendCommand(Command.CreateCommandClient(commandType, gameMethod, null));

        Command response = null;

        yield return new WaitUntil(() =>
        {
            response = FindResponseCommand(commandType, gameMethod);
            return response != null;
        });

        Serializable serializable = null;
        if (genericType != null)
            serializable = Serializable.CreateGeneric(genericType, response.bytes);
        else
            serializable = Serializable.Create(response.bytes);

        onReceived((T)serializable.Deserialize());
    }


    public override void Update()
    {
        NS.Update();
    }

    private void OnConnected()
    {
        Debug.Log("Connected Client!");
    }

    private void OnDataReceived(List<byte[]> packages)
    {
        Command cmd = Command.CreateAndDeserializeClient(new List<byte[]>(packages));

        if (cmd.IsResponse)
        {
            commands.Add(cmd);
        }
    }

    protected override void SendData(byte[] bytes, NetworkSourceServer.ConnectedPC pc, bool isOverTransmission)
    {
        NS.SendData(bytes, isOverTransmission);
    }

    
}
