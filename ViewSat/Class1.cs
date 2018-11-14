using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
//using static System.GC;
using System.Runtime.InteropServices;
using System.Globalization;

namespace ViewSat
{
    class Class1
    {
        public interface IPayload
        {

        }

        [StructLayout(LayoutKind.Explicit, Size = 30, Pack = 1)]
        struct Pg01E : IPayload
        {
            [MarshalAs(UnmanagedType.R8)]
            [FieldOffset(0)]
            public double Fi;

            [MarshalAs(UnmanagedType.R8)]
            [FieldOffset(8)]
            public double Lam;

            [MarshalAs(UnmanagedType.R8)]
            [FieldOffset(16)]
            public double H;


            [MarshalAs(UnmanagedType.R4)]
            [FieldOffset(24)]
            public float Ps; //Position SEP


            [MarshalAs(UnmanagedType.I1)]
            [FieldOffset(28)]
            public byte Type;

            [MarshalAs(UnmanagedType.I1)]
            [FieldOffset(29)]
            public byte Chksum;
        }

        [StructLayout(LayoutKind.Explicit, Size = 18, Pack = 1)]
        struct Dp012 : IPayload
        {
            //...
        }

        public class DataFactory
        {
            private static class Headers
            {
                public static readonly short PG = GetHeader("PG");
                public static readonly short DP = GetHeader("DP");

                private static short GetHeader(string value) => Convert.ToInt16(Encoding.ASCII.GetBytes(value));
            }


            public IEnumerable<IPayload> ReadPayloads(string path)
            {
                using (var stream = new FileStream(path, FileMode.Open))
                using (var reader = new BinaryReader(stream))
                {
                    while (stream.CanRead)
                    {
                        yield return ReadPayload(reader);
                    }
                }
            }

            private IPayload ReadPayload(BinaryReader reader)
            {
                short header = reader.ReadInt16();
                byte[] size = reader.ReadBytes(3);

                int payloadSize = GetSize(size);
                byte[] data = reader.ReadBytes(payloadSize);


                if (header == Headers.PG)
                {
                    return ByteArrayToStructure<Pg01E>(data);
                }

                if (header == Headers.DP)
                {
                    return ByteArrayToStructure<Dp012>(data);
                }

                throw new NotImplementedException();
            }

            //Вот это надо как-нибудь по-хитрому, я делаю в лоб и медленно
            private int GetSize(byte[] sizeBytes) => int.Parse(Encoding.ASCII.GetString(sizeBytes), NumberStyles.HexNumber);

            T ByteArrayToStructure<T>(byte[] bytes) where T : struct
            {
                T stuff;
                GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                try
                {
                    stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
                }
                finally
                {
                    handle.Free();
                }
                return stuff;
            }
        }
    }
}
