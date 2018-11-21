using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace ViewSat
{

    class GpsFile
    {
        string FileName { get; set; }
        FileStream GpsStream { get; set; }
//        MemoryStream GpsStream;
        
        LogFile Log;
        public GpsFile(string filename)
        {
            #region считывание файла
            Log = new LogFile("..\\..\\LogFile.txt");
            Log.Write("Открытие файла gps");
            try
            {
                GpsStream = new FileStream(filename, FileMode.Open);
                Console.WriteLine("Успешное открытие файла" + filename);
                Log.Write("Успешное открытие файла" + filename);
                Log.Write($"Размер файла {GpsStream.Length}");
                FileName = filename;
               

                Read();
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
        public void Read()
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
            int  a=1;
//            while (a!=-1)
            while (GpsStream.Read(buf, 0, 4096) != -1)
            {
//                a = GpsStream.ReadByte();
                Console.Clear();
                Console.WriteLine($"{GpsStream.Position} байт из {GpsStream.Length}");               
            }
        }
    }
}
