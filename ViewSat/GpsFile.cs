﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ViewSat
{
    class Package
    {
        string Name { get; set; }
        int Size { get; set; }
        public byte Chksum { get; set; }

        public Package(string name, int size)
        {
            Name = name;
            Size = size;
        }

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
        FileStream TempStream { get; set; }
        MemoryStream GpsStream;

        LogFile Log;
        public GpsFile(string filename)
        {
            #region считывание файла
            Log = new LogFile("..\\..\\LogFile.txt");
            Log.Write("Открытие файла gps");
            try
            {
                TempStream = new FileStream(filename, FileMode.Open);
                Console.WriteLine("Успешное открытие файла" + filename);
                Log.Write("Успешное открытие файла" + filename);
                Log.Write($"Размер файла {TempStream.Length} байт");
                FileName = filename;
                GpsStream = new MemoryStream();
                TempStream.CopyTo(GpsStream);
                GpsStream.Position = 0;


                TempStream.Close();

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

        private int RecognizePackage(List<string> Packages, string buf)
        {
            for (int i = 0; i < Packages.Count; i++)
            {
                if (buf.Contains(Packages[i].Remove(1, 1)))
                    return i;
            }
            return -1;
        }

//        List<Package> Packages;
        public void Read()
        {
            Log.Write("====================================================================================================");
            Log.Write("Чтение файла");
            byte[] buf = new byte[5];
            List<Package> _Packages = new List<Package>();

            //Packages.Add(new Package("~~0", 5));
            //Packages.Add(new Package("RD0", 5));
            //Packages.Add(new Package("PG0", 5));
            //Packages.Add(new Package("VG0", 5));
            //Packages.Add(new Package("SG0", 5));
            //Packages.Add(new Package("DP0", 5));
            //Packages.Add(new Package("PS0", 5));
            //Packages.Add(new Package("TO0", 5));
            //Packages.Add(new Package("SI0", 5));
            //Packages.Add(new Package("SS0", 5));
            //Packages.Add(new Package("EL0", 5));
            //Packages.Add(new Package("AZ0", 5));
            //Packages.Add(new Package("EC0", 5));
            //Packages.Add(new Package("FC0", 5));
            //Packages.Add(new Package("||0", 1));


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

            int[] SizeOfPackage = new int[15]
            {
                5,
                6,
                20,
                36,
                18,
                18,
                9,
                17,
                0,
                0,
                0,
                0,
                0,
                0,
                1
            };

            List<PG01E> PG = new List<PG01E>();
            DateTime Start = new DateTime(); // Время запуска           
            DateTime Stoped = new DateTime(); //Время окончания
            TimeSpan Elapsed = new TimeSpan();
            Start = DateTime.Now;
            bool coincidence = true;
            string CurBuf;
            //размер пакета в 16ричной системе
            string xPackSize = "";
            int PackSize = 0;
            string LastPackage = "";
            int LastPackSize = 0;
            Start = DateTime.Now; // Старт (Записываем время)
            while (GpsStream.Read(buf, 0, 5) != 0)
            {

//                Stoped = DateTime.Now; // Стоп (Записываем время)
//                Elapsed = Stoped.Subtract(Start);
                Console.Clear();
                Console.WriteLine(Elapsed.Ticks.ToString());
                Console.WriteLine($"{GpsStream.Position} байт из {GpsStream.Length}");
                CurBuf = Encoding.Default.GetString(buf);
                try
                {
                    xPackSize = Encoding.Default.GetString(buf, 3, 2);
                }
                catch (Exception e)
                {
                    Log.Write($"Ошибка xPackSize: Чтения {GpsStream.Position} байта. {e.Message}");
                    Log.Write($"Ошибка xPackSize: Потеря байтов у маски пакета");
                }

                try
                {
                    //на один больше чтоб попасть на позицию следующей маски пакета
                    PackSize = Convert.ToInt32(xPackSize, 16) + 1;
                }
                catch (FormatException e)
                {
                    int ChkBuf = RecognizePackage(Packages, CurBuf);
                    Log.Write($"/Ошибка PackSize: Чтения {GpsStream.Position} байта. {e.Message}");
                    if (ChkBuf > -1)
                    {
                        GpsStream.Position--;
                        GpsStream.Position += SizeOfPackage[ChkBuf]; //
                        GpsStream.Position++;
                        Log.Write($"\\Ошибка: Чтения маски {Packages[ChkBuf]}. Потерян первый байт маски");
                        coincidence = true;
                    }
                    else
                    {
                        //Log.Write($"Проблема чтения {LastPackage} пакета");
                        bool next = true;  //признак выхода из двух циклов
                        GpsStream.Position -= LastPackSize + 5;  //выставить позицию чтения на размер считанного пакета на предыдущем такте и на размер маски считанной в текщем такте
                        if (coincidence == false)
                        {
                            GpsStream.Position -= 3;
                            GpsStream.Read(buf, 0, 5);
                            CurBuf = Encoding.Default.GetString(buf);
                            int ChkBuf1 = RecognizePackage(Packages, CurBuf);
                            Log.Write($"/Ошибка PackSize: Чтения {GpsStream.Position} байта. {e.Message}");
                            if (ChkBuf1 > -1)
                            {
                                GpsStream.Position--;
                                GpsStream.Position += SizeOfPackage[ChkBuf1]; //
                                GpsStream.Position++;
                                Log.Write($"\\Ошибка: Чтения маски {Packages[ChkBuf1]}. Потерян первый байт маски");
                                coincidence = true;
                                next = false;
                            }
                        }
                        for (int i = 0; i < LastPackSize && next; i++)
                        {
                            byte[] ebuf = new byte[3];  //считываем по 3 байта и ищем знакомую маску
                            GpsStream.Read(ebuf, 0, 3); // позиция +3
                            GpsStream.Position -= 2;  // выставляем позицию чтения на один дальше от текущей позиции (позиция-2)

                            CurBuf = Encoding.Default.GetString(ebuf); // получаем маску
                            for (int j = 0; j < Packages.Count && next; j++)
                            {
                                if (CurBuf == Packages[j])  // если буфер совпал с известной маской 
                                {
                                    Log.Write($"\\Ошибка: Пакет {LastPackage} по адреcу {GpsStream.Position - LastPackSize} равен {i} байт. Требуемый размер {LastPackSize} байт");
                                    //Log.Write($"Ошибка чтения {GpsStream.Position} байта");
                                    GpsStream.Position -= 1;
                                    next = false;  // выходим из циклов
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    string a = e.Message;
                }

                //GpsStream.Position -= 4;
                //Ищем совпадения среди известных пакетов
                for (int i = 0; i < Packages.Count; i++)
                {
                    //как только нашли совпадение
                    if (CurBuf == Packages[i] + xPackSize)
                    {
                        coincidence = true;
                        //смещаем позици чтения на размер пакета в байтах
                        GpsStream.Position += PackSize;
                        //зафиксируем последнюю удачно считанную маску пакета
                        LastPackage = Packages[i];
                        //и его размер
                        LastPackSize = PackSize;
                        //пропускаем остальные сравнения 
                        break;
                    }
                    else
                        coincidence = false;
                }

                if (coincidence == false)
                    //coincidence = false;
                Log.Write("Не найдено совпадение среди известных пакетов");

                //if (coincidence == false)
                //{
                //    int ChkBuf = RecognizePackage(Packages, CurBuf);
                //    Log.Write($"Ошибка PackSize: Чтения {GpsStream.Position} байта");
                //    if (ChkBuf > -1)
                //    {
                //        GpsStream.Position--;
                //        GpsStream.Position += SizeOfPackage[ChkBuf];
                //        GpsStream.Position++;
                //        Log.Write($"Ошибка: Чтения маски {Packages[ChkBuf]}. Потерян первый байт маски");
                //    }
                //    coincidence = true;
                //}
                //                Start = DateTime.Now; // Старт (Записываем время)
            }
            Stoped = DateTime.Now; // Стоп (Записываем время)
            Elapsed = Stoped.Subtract(Start);
            Log.Write("Файл прочитан");
            Log.Write($"Время затраченное на чтение = {Elapsed.Minutes.ToString()}:{Elapsed.Seconds.ToString()}.{Elapsed.Milliseconds.ToString()} м");
            Log.Write("====================================================================================================");

        }
    }
}
