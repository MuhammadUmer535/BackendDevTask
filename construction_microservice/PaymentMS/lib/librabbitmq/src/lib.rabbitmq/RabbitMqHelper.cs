using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Lib.RabbitMq
{
    //TODO: Should be in Database
    public struct MessageQueues
    {
        public const string AUTHTask = "AUTH.Task";
        public const string PAYMENTRPCQueue = "PAYMENT.RPC";
    }

    public static class RabbitMqHelper
    {
        public static byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;

            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static byte[] XDocumentToByte(Object obj)
        {
            if (obj == null)
                return null;

            string s = obj.ToString();
            byte[] b = Encoding.ASCII.GetBytes(s);

            return b;
        }

        public static Object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);

            return obj;
        }
    }
}