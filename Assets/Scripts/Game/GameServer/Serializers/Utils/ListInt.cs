using Net;
using System.Collections.Generic;
using UnityEngine;

namespace Serialization
{
    public class ListInt : Serializable
    {
        private List<int> list;
        public ListInt(List<int> list)
        {
            this.list = list;
        }
        public ListInt(List<byte[]> bytes)
        {
            this.bytes = bytes;
        }
        public override List<byte[]> Serialize()
        {
            List<byte[]> bytes = PrepareBytes();
            foreach (int i in list)
            {
                bytes.Add(Utils.IntToBytes(i));
            }
            InsertPackagesCount(bytes);
            return bytes;
        }
        public override object Deserialize()
        {
            list = new List<int>();
            foreach (byte[] b in bytes)
            {
                list.Add(Utils.BytesToInt(b));
            }
            return list;
        }
    }
}
