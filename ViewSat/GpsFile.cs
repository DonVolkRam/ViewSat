using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


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
        public void Read(/*ref FileStream gpsStream*/)
        {

            LogFile Log = new LogFile("LogFile.txt");
            Log.Write("Открытие файла gps");
            //            char[] buf = new char[5];
            byte[] buf = new byte[5];
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

            List<PG01E> PG = new List<PG01E>();
            DateTime Start = new DateTime(); // Время запуска
            Start = DateTime.Now;
            DateTime Stoped = new DateTime(); //Время окончания
            TimeSpan Elapsed = new TimeSpan();
            string CurBuf;
            string xPackSize;
            int PackSize=0;
            string LatPackage = "";
            while ((GpsStream.Read(buf, 0, 5)) != -1)
            {

                Stoped = DateTime.Now; // Стоп (Записываем время)
                Elapsed = Stoped.Subtract(Start);
                Console.Clear();
                Console.WriteLine(Elapsed.Ticks.ToString());
                Console.WriteLine($"{GpsStream.Position} байт из {GpsStream.Length}");

                CurBuf = Encoding.Default.GetString(buf);
                xPackSize = Encoding.Default.GetString(buf, 3, 2);
                try
                {
                    PackSize = Convert.ToInt32(xPackSize, 16) + 1;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine($"Проблема чтения {LatPackage} пакета");
                    
                    break;
                }
                //GpsStream.Position -= 4;

                for (int i = 0; i < Packages.Count; i++)
                {
                    if (CurBuf == Packages[i] + xPackSize)
                    {
                        GpsStream.Position += PackSize;
                        LatPackage = Packages[i];
                        continue;
                    }

                }

                Start = DateTime.Now; // Старт (Записываем время)

            }
        }
    }
}
