using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace ViewSat
{
    class Program
    {

        static void Main(string[] args)
        {
            LogFile Log = new LogFile("LogFile.txt");
            Console.WriteLine("Введите имя файла");
            string filename = Console.ReadLine();
            filename = "..\\..\\10_1_1_C1.gps";
            GpsFile Stream = new GpsFile(filename);            
            Console.Read();
        }

    }
}
