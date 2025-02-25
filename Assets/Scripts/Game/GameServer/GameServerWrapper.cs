using GameCore;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameClient;
using static GameServerWrapper;
using static Net.NetworkSourceServer;

public abstract class GameServerWrapper
{
    public delegate void OnReceived<T>(T response);

    protected List<Command> commands { get; private set; } = new List<Command>();
    /// <summary>
    /// Send data to another PC. If pc == null -> command is sent by the client, otherwise by the server
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="pc"></param>
    /// <param name="isOverTransmission"></param>
    protected abstract void SendData(byte[] bytes,ConnectedPC pc,bool isOverTransmission);
    public abstract IEnumerator VoidCall<T>(GameMethod gameMethod, OnReceived<T> onReceived, Type genericType = null);
    public abstract void Update();
    public bool IsServer()
    {
        return this is GameServer;
    }
    protected void SendCommand(Command cmd)
    {
        List<byte[]> bytes = cmd.Serialize();
        for (int i = 0; i < bytes.Count; i++)
        {
            bool isFinalPackage = false;
            if (i == bytes.Count - 1)
                isFinalPackage = true;

            SendData(bytes[i], cmd.pc, isFinalPackage);
        }
    }

    protected Command FindResponseCommand(Command.CommandType type,GameMethod method) {
        foreach (Command cmd in commands)
        {
            if (cmd.type == type && cmd.gameMethod == method && cmd.IsResponse)
            {
                commands.Remove(cmd);
                return cmd;
            }
        }
        return null;
    }
    public enum GameMethod
    {
        GetTerrain,
        GetBuildings,
        GetWorldMap
    }
}
