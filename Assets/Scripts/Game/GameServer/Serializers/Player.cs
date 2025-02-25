using GameCore;
using Net;
using System.Collections.Generic;
using UnityEngine;
using static GameCore.Player;

namespace Serialization
{
    public class Player : Serializable
    {
        private GameCore.Player player;
        public Player(GameCore.Player player)
        {
            this.player = player;
        }
        public Player(List<byte[]> bytes)
        {
            this.bytes = bytes;
        }
        public override List<byte[]> Serialize()
        {
            List<byte[]> bytes = PrepareBytes();
            bytes.Add(Utils.IntToBytes(player.gold));
            bytes.Add(Utils.FloatToByte(player.population));
            bytes.Add(new byte[] { (byte)player.Team });
            InsertPackagesCount(bytes);
            return bytes;
        }
        public override object Deserialize()
        {
            int gold = Utils.BytesToInt(bytes[0]);
            float population = Utils.BytesToFloat(bytes[1]);
            TeamColor team = (TeamColor)bytes[2][0];
            bytes.RemoveRange(0, 3);
            GameCore.Player player = new GameCore.Player(team);
            player.LoadGold(gold);
            player.LoadPopulation(population);
            return player;
        }
    }
}
