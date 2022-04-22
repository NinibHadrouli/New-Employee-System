using System;
using AbscenceSystem;

namespace EmployeesAbsence
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            IFileHandler fHandler = new FileHandler();
            fHandler.ReadCsvFile(fHandler.GetRootPath()+ "StartData.csv");
            fHandler.ReadXmlFiles(fHandler.GetRootPath());
            fHandler.ScanAndUpdate();

            fHandler.GetEmpAbsMarch();
            fHandler.GetEmpCountAbsMarchTA();
            fHandler.GetEmpContextAbsApril();
            fHandler.WriteNewCsvToFile();
        }
    }
}
