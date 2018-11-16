using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ViewSat
{





    class GpsFile
    {
        string FileName { get; set; }
        FileStream FileGpsStream { get; set; }
        MemoryStream MemoryGpsStream;
        static List<Package> Packages = new List<Package>();
        LogFile Log;

        static GpsFile()
        {
            Packages = new List<Package>
            {
                new Package("~~005"),
                new Package("RD006"),
                new Package("PG01E"),
                new Package("VG012"),
                new Package("SG012"),
                new Package("DP012"),
                new Package("PS009"),
                new Package("TO011"),

                new Package("SI000"),
                new Package("SS000"),
                new Package("EL000"),
                new Package("AZ000"),
                new Package("EC000"),
                new Package("FC000"),

                new Package("||001")
            };
        }
        /// <summary>
        /// Открытие файла *gps
        /// </summary>
        /// <param name="filename">путь к файлу</param>
        public GpsFile(string filename)
        {
            #region считывание файла
            Log = new LogFile("..\\..\\LogFile.txt");
            Log.Write("Открытие файла gps");
            try
            {
                FileGpsStream = new FileStream(filename, FileMode.Open);
                Console.WriteLine("Успешное открытие файла" + filename);
                Log.Write("Успешное открытие файла" + filename);
                Log.Write($"Размер файла {FileGpsStream.Length}");
                FileName = filename;
                MemoryGpsStream = new MemoryStream();
                FileGpsStream.CopyTo(MemoryGpsStream);
                MemoryGpsStream.Position = 0;


                FileGpsStream.Close();

                Read();
                MemoryGpsStream.Close();
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


        /// <summary>
        /// Распознование пакета по неполной маске
        /// </summary>
        /// <param name="buf">буфер с неполной моской</param>
        /// <returns>номер пакета в списке с которым найдено совпадение</returns>
        private int RecognizePackage(string buf)
        {
            for (int i = 0; i < Packages.Count; i++)
            {
                if (buf.Contains(Packages[i].ChkName.Remove(1, 1)))
                    return i;
            }
            return -1;
        }

        public void Read()
        {
 //           Initianalize();
            Log.Write("Чтение файла");
            byte[] buf = new byte[5];

            List<PG01E> PG = new List<PG01E>();
            DateTime Start = new DateTime(); // Время запуска           
            DateTime Stoped = new DateTime(); //Время окончания
            TimeSpan Elapsed = new TimeSpan();
            Start = DateTime.Now;
            string CurBuf;
            //размер пакета в 16ричной системе
            string xPackSize = "";
            int PackSize = 0;
            string LastPackage = "";
            int LastPackSize = 0;
            while (MemoryGpsStream.Read(buf, 0, 5) != -1)
            {

                Stoped = DateTime.Now; // Стоп (Записываем время)
                Elapsed = Stoped.Subtract(Start);
                Console.Clear();
                Console.WriteLine(Elapsed.Ticks.ToString());
                Console.WriteLine($"{MemoryGpsStream.Position} байт из {MemoryGpsStream.Length}");
                CurBuf = Encoding.Default.GetString(buf);
                try
                {
                    xPackSize = Encoding.Default.GetString(buf, 3, 2);
                }
                catch (Exception e)
                {
                    Log.Write($"Ошибка xPackSize: Чтения {MemoryGpsStream.Position} байта. {e.Message}");
                    Log.Write($"Ошибка xPackSize: Потеря байтов у маски пакета");
                }

                try
                {
                    //на один больше чтоб попасть на позицию следующей маски пакета
                    PackSize = Convert.ToInt32(xPackSize, 16) + 1;
                }
                catch (FormatException e)
                {
                    int a = RecognizePackage(CurBuf);
                    int c;
                    if (a > -1)
                        c = 7;
                    Log.Write($"Ошибка PackSize: Чтения {MemoryGpsStream.Position} байта. {e.Message}");
                    //Log.Write($"Проблема чтения {LastPackage} пакета");
                    bool next = true;
                    MemoryGpsStream.Position -= LastPackSize + 5;
                    for (int i = 0; i < LastPackSize && next; i++)
                    {
                        byte[] ebuf = new byte[3];
                        MemoryGpsStream.Read(ebuf, 0, 3);
                        MemoryGpsStream.Position -= 2;

                        CurBuf = Encoding.Default.GetString(ebuf);
                        for (int j = 0; j < Packages.Count && next; j++)
                        {
                            if (CurBuf == Packages[j].FullName)
                            {
                                Log.Write($"Ошибка: Пакет {LastPackage} по адреcу {MemoryGpsStream.Position - LastPackSize} равен {i} байт. Требуемый размер {LastPackSize} байт");
                                //Log.Write($"Ошибка чтения {GpsStream.Position} байта");
                                MemoryGpsStream.Position -= 1;
                                next = false;
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
                        //смещаем позици чтения на размер пакета в байтах
                        MemoryGpsStream.Position += PackSize;
                        //зафиксируем последнюю удачно считанную маску пакета
                        LastPackage = Packages[i].ChkName;
                        //и его размер
                        LastPackSize = PackSize;
                        //пропускаем остальные сравнения 
                        continue;
                    }
                }

                Start = DateTime.Now; // Старт (Записываем время)
            }
        }
    }
}
