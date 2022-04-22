using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AbscenceSystem;

namespace TestAbsence
{
    [TestClass]
    public class TestFileHandler
    {
        IFileHandler fHandler;
        //Dictionary<int, List<Absence>> EmpDictionaryCsv;
        private void Setup()
        {
             fHandler = new FileHandler();
             //EmpDictionaryCsv = new Dictionary<int, List<Absence>>();
        }
        [TestMethod]
        public void TestAddNewAbsenceRec()
        {
            Setup();
            Absence absRec = new Absence();
            int id = 1;
            fHandler.AddNewAbsenceRec(absRec,id);
            
            //Dictionary should have items...
            Assert.IsTrue(fHandler.EmpDictionaryCsv.Any());

            //New absence record should exist in dictionary 
            Assert.IsTrue(fHandler.EmpDictionaryCsv.ContainsKey(id));

        }
    }
}
