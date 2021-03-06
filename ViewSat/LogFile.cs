﻿using System;
using System.IO;

namespace ViewSat
{
    class LogFile
    {
        private readonly string FileName;

        public LogFile(string filename)
        {
            try
            {
                StreamReader logReader = new StreamReader(filename);
                FileName = filename;               
                logReader.Close();
            }
            catch (FileNotFoundException FNFE)        
            {
                Console.WriteLine("Ошибка: " + FNFE.Message);
            }           
        }

        public void Write(string message)
        {
            try
            {
//                StreamWriter logWriter = new StreamWriter(FileName);                
                File.AppendAllText(FileName, DateTime.Now + " ----- " + message + "\n");
                //logWriter.WriteLine(message);                
                //logWriter.Close();
            }
            catch (FileNotFoundException FNFE)
            {
                Console.WriteLine("Ошибка: " + FNFE.Message);
            }          
        }
    }
}
