using System;
using System.Collections.Generic;
using System.IO;

namespace ViewSat
{
    class Package
    {

    }

    struct RD006           //Receiver Date
    {
        //Receiver reference time [enumerated] 0 - GPS,
        //  1 - UTC_USNO, 2 - GLONASS, 3 - UTC_SU, 4-254 - Reserved

        public ushort Year { get; set; }
        public byte Month { get; set; }
        public byte Day { get; set; }
        public byte BaseTime { get; set; }
        public byte Chksum { get; set; }

    };

    struct PG01E            //Geodesic Position
    {        
        public double Fi { get; set; }
        public double Lam { get; set; }
        public double H { get; set; }
        public float Ps { get; set; } //Position SEP
        public byte Type { get; set; }
        public byte Chksum { get; set; }
    };

    class GpsFile
    {
        string FileName { get; set; }
        FileStream GpsStream { get; set; }
        public GpsFile(string filename)
        {
            #region считывание файла
            LogFile Log = new LogFile("LogFile.txt");
            Log.Write("Открытие файла gps");
            try
            {
                //                StreamReader gpsReader = new StreamReader(filename);
                GpsStream = new FileStream(filename, FileMode.Open);
                Console.WriteLine("Успешное открытие файла" + filename);
                Log.Write("Успешное открытие файла" + filename);

                FileName = filename;                

                Read(/*ref gpsReader*/);
                GpsStream.Close();
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
        public void Read(/*ref FileStream gpsStream*/)
        {

            LogFile Log = new LogFile("LogFile.txt");
            Log.Write("Открытие файла gps");
            //            char[] buf = new char[5];
            byte[] buf = new byte[5];
            List<PG01E> PG = new List<PG01E>();
            int i;
            while ((i = GpsStream.ReadByte()) != -1)
            {
                GpsStream.Position--;
                //memcpy(com_temp, com_temp + 1, 5);
                GpsStream.Read(buf, 0, 5);
                GpsStream.Position--
                //gpsStream.ReadBlock(buf, 0, 5);
            }
        }
    }
}
