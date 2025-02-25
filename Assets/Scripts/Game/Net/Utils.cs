using System;
using UnityEngine;

namespace Net
{
    public class Utils
    {
        public static byte[] FloatToByte(float v)
        {
            return BitConverter.GetBytes(v);
        }
        public static byte[] FloatArrToByte(float[] arr)
        {

            var byteArray = new byte[arr.Length * 4];
            System.Buffer.BlockCopy(arr, 0, byteArray, 0, byteArray.Length);
            return byteArray;
        }
        public static float[] ByteArrToFloat(byte[] arr)
        {

            float[] floatArray2 = new float[arr.Length / 4];
            System.Buffer.BlockCopy(arr, 0, floatArray2, 0, arr.Length);
            return floatArray2;
        }

        public static byte[] IntToBytes(int v)
        {
            return System.BitConverter.GetBytes(v);
        }
        public static int BytesToInt(byte[] arr)
        {
            return System.BitConverter.ToInt32(arr, 0);
        }

        public static float BytesToFloat(byte[] arr)
        {
            return System.BitConverter.ToSingle(arr, 0);
        }
    }
}
