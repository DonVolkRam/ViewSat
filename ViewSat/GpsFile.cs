using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;


namespace ViewSat
{
    public interface IPayload
    {

    }

    public class Arr<T> : IPayload
    {
        T obj;
        List<T> List;
        public Arr()
        {
            List = new List<T>();
        }

        public void Add(T item)
        {
            List.Add(item);
        }
    }

    class GpsFile
    {

        struct PV02E            //Cartesian Position and Velocity
        {
            double x;
            double y;
            double z;
            float pSigma;
            float vx;
            float vy;
            float vz;
            float vSigma;
            byte type;
            byte chksum;
        };


        //[StructLayout(LayoutKind.Explicit)]
        //public struct PG01E            //Geodesic Position
        //{
        //    [FieldOffset(0)]
        //    public string FileDate;
        //    [FieldOffset(5)]
        //    public double fi;
        //    [FieldOffset(13)]
        //    public double lam;
        //    [FieldOffset(21)]
        //    public double h;
        //    [FieldOffset(29)]
        //    public float ps;            //Position SEP
        //    [FieldOffset(33)]
        //    public char type;
        //    [FieldOffset(34)]
        //    public byte chksum;
        //};




        [StructLayout(LayoutKind.Explicit, Size = 30, Pack = 1)]
        struct PG01E : IPayload
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

                    Console.Clear();
                    Console.WriteLine($"{stream.Position} байт из {stream.Length}");
                    yield return ReadPayload(reader);
                }
            }
        }

        private IPayload ReadPayload(BinaryReader reader)
        {
            byte temp;
            if (reader.BaseStream.Position>0)
                temp = reader.ReadByte();
            short header = reader.ReadInt16();
            byte[] size = reader.ReadBytes(3);

            int payloadSize = GetSize(size);
            byte[] data = reader.ReadBytes(payloadSize);


            if (header == 18256)
            {
                return ByteArrayToStructure<PG01E>(data);
            }
            
            //if (header == Headers.DP)
            //{
            //    return ByteArrayToStructure<Dp012>(data);
            //}

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



        //public static T ByteToType<T>(BinaryReader reader)
        //{
        //    byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));

        //    GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        //    T theStructure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        //    handle.Free();

        //    return theStructure;
        //}


        string FileName { get; set; }
        FileStream GpsStream { get; set; }
        BinaryReader br { get; set; }
        //        MemoryStream GpsStream;
        List<PG01E> pg = new List<PG01E>();
//        Arr<PG01E> pg = new Arr<PG01E>();
        LogFile Log;
        public GpsFile(string filename)
        {
            #region считывание файла
            Log = new LogFile("..\\..\\LogFile.txt");
            Log.Write("Открытие файла gps");
            try
            {
                //                GpsStream = new FileStream(filename, FileMode.Open);
                Console.WriteLine("Успешное открытие файла" + filename);
                Log.Write("Успешное открытие файла" + filename);
                //                Log.Write($"Размер файла {GpsStream.Length}");
                FileName = filename;
                //                br = new BinaryReader(GpsStream);

                //               ReadPayloads(filename);
                foreach (PG01E a in ReadPayloads(filename))
                {
                    pg.Add(a);
                }
                GpsStream.Close();
                Console.WriteLine("Чтение завершено");
                Log.Write("Чтение завершено");
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("Ошибка: " + e.Message);
                Log.Write("Ошибка: " + e.Message);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("Ошибка: " + e.Message);
                Log.Write("Ошибка: " + e.Message);
            }

            #endregion
        }
        public IEnumerable<IPayload> Read(string filename)
        {
            Log.Write("Чтение файла");
            byte[] buf = new byte[4096];


            List<string> Packages = new List<string>

            {
                "~~0",
                "RD0",
                "PG0",
                "VG0",
                "SG0",
                "DP0",
                "PS0",
                "TO0",
                "SI0",
                "SS0",
                "EL0",
                "AZ0",
                "EC0",
                "FC0",
                "||0"
            };
            int a = 1;
            //            while (a!=-1)

            using (var stream = new FileStream(filename, FileMode.Open))
            using (var reader = new BinaryReader(stream))
            {
                while (stream.CanRead)
                {
                    Console.Clear();
                    Console.WriteLine($"{GpsStream.Position} байт из {GpsStream.Length}");
                    yield return ReadPayload(reader);
                }
            }

            //            while (true)
            ////                while (GpsStream.Read(buf, 0, 4096) != -1)
            //            {
            //                //                a = GpsStream.ReadByte();
            //                pg.Add(ByteToType<PG01E>(br));
            //                Console.Clear();
            //                Console.WriteLine($"{GpsStream.Position} байт из {GpsStream.Length}");               
            //            }
        }
    }
}
