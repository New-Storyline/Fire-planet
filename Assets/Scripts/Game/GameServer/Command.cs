using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using static GameServerWrapper;
using static Net.NetworkSourceServer;

public class Command
{
    public CommandType type { get; private set; }
    public GameMethod gameMethod { get; private set; }
    public bool IsResponse { get; private set; }
    public List<byte[]> bytes { get; private set; }
    public ConnectedPC pc { get; private set; }

    /// <summary>
    /// Constructor for Command.
    /// Package structure: [type (1 byte), gameMethod (1 byte),IsResponse (1 byte), bytes]
    /// </summary>
    /// <param name="bytes">List of bytes arrays. First list elem represents 3 bytes - type, gameMethod and IsResponse, other list elems - other bytes</param>
    public Command(ConnectedPC pc)
    {
        this.pc = pc;
    }

    public Command(){}

    private Command(ConnectedPC pc,CommandType type, GameMethod gameMethod,bool IsResponse, List<byte[]> bytes)
    {
        this.pc = pc;
        this.type = type;
        this.gameMethod = gameMethod;
        this.IsResponse = IsResponse;
        this.bytes = bytes;
    }

    public static Command CreateCommand(ConnectedPC pc, CommandType type, GameMethod gameMethod, List<byte[]> bytes)
    {
        return new Command(pc, type, gameMethod, false, bytes);
    }
    public static Command CreateCommandClient(CommandType type, GameMethod gameMethod, List<byte[]> bytes)
    {
        return new Command(null, type, gameMethod, false, bytes);
    }
    public static Command CreateResponse(Command cmd, List<byte[]> bytes)
    {
        return new Command(cmd.pc, cmd.type, cmd.gameMethod, true, bytes);
    }

    public static Command CreateAndDeserialize(ConnectedPC pc,List<byte[]> bytes)
    {
        Command cmd = new Command(pc, (CommandType)bytes[0][0], (GameMethod)bytes[0][1], bytes[0][2] == 1, bytes);
        bytes.RemoveAt(0);
        return cmd;
    }

    public static Command CreateAndDeserializeClient(List<byte[]> bytes)
    {
        Command cmd = new Command(null, (CommandType)bytes[0][0], (GameMethod)bytes[0][1], bytes[0][2] == 1, bytes);
        bytes.RemoveAt(0);
        return cmd;
    }

    public List<byte[]> Serialize() {

        List<byte[]> result = new List<byte[]>();
        byte[] commandHead = new byte[3] { 
            (byte)this.type,
            (byte)gameMethod, 
            IsResponse? (byte)1 : (byte)0 
        };
        result.Add(commandHead);
        if(bytes != null)
            result.AddRange(bytes);
        return result;
    }

    public override string ToString()
    {
        return $"Command: {type}, {gameMethod}, {IsResponse}, bytes list length: {bytes.Count}";
    }

    /// <summary>
    /// Command types:
    /// VoidEvent - call method command without args and return value
    /// Evemt - call method command with args and without return
    /// VoidGetter - call method command without args, but with return value
    /// Getter - call method command with args and return value
    /// MultiGetter - call method command with args and return value, but with multiple return values (object array)
    /// </summary>
    public enum CommandType
    {
        VoidEvent,
        Event,
        VoidCall
    }

}