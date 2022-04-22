using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Collections.Specialized;
using System.Reflection;
using CsvHelper;

namespace AbscenceSystem
{
    public class FileHandler : IFileHandler
    {
        //public Dictionary<int, List<Absence>> EmpDictionaryCsv = new Dictionary<int, List<Absence>>();
        private Dictionary<int, List<Absence>> empDictionaryCsv;
        public Dictionary<int, List<Absence>> EmpDictionaryCsv
        {
            get
            {
                if (empDictionaryCsv == null)
                {
                    empDictionaryCsv= new Dictionary<int, List<Absence>>();
                }
                    
                return empDictionaryCsv;
            }
            set
            {
                empDictionaryCsv = value;
            }
        }

        SortedDictionary<DateTime, List<Employee>> EmpDictionaryXml = new SortedDictionary<DateTime, List<Employee>>();

        private List<Employee> result = new List<Employee>();

        public FileHandler()
        {

        }

        #region csv


        public void ReadCsvFile(string csvfile)
        {
            if (!File.Exists(csvfile)) throw new Exception($"File {csvfile} doesn't exist!");
            
            var currentAbsence = ReadCsv(csvfile);
            HandleAbsenceDetails(currentAbsence);
        }

        private void HandleAbsenceDetails(List<KeyValuePair<int, Absence>> absences)
        {

            var absenceDetails = absences
                .Select(x => x.Key)
                .Distinct()
                .Select(item => new
                {
                    id = item,
                    absence = absences.Where(i => i.Key == item).Select(i => i.Value).ToList()
                });
            foreach (var embAbDetail in absenceDetails)
            {
                foreach (var absenceRecord in embAbDetail.absence)
                {
                    AddNewAbsenceRec(absenceRecord, embAbDetail.id);

                }

            }
        }
        
        public void AddNewAbsenceRec(Absence absRec, int id)
        {
            var l = new List<Absence>();
            if (EmpDictionaryCsv.ContainsKey(id))
            {
                l = EmpDictionaryCsv[id];
                l.Add(absRec);
                return;
            }

            l.Add(absRec);
            EmpDictionaryCsv.Add(id, l);
        }

      
        private double GetDiffInDays(DateTime current, DateTime last)
        {
            return (current - last).TotalDays;
        }

        private List<KeyValuePair<int, Absence>> ReadCsv(string path)
        {
            var lines = File.ReadAllLines(path);

            List<KeyValuePair<int, Absence>> listAbsence = new List<KeyValuePair<int, Absence>>();

            foreach (var line in lines)
            {
                var split = line.Split(';');
                listAbsence.Add(new KeyValuePair<int, Absence>(int.Parse(split[0]), new Absence
                {
                    StartDate = DateTime.Parse(split[1]),
                    AbsenceType = (AbsenceType) int.Parse(split[2]),
                    Percentage = double.Parse(split[3])

                }));
            }

            return listAbsence;
        }

        #endregion

        #region Xml

        private void UpdateWithXmlFile(FileInfo f)
        {
            try
            {
                using (TextReader reader = new StringReader(File.ReadAllText(f.FullName)))
                {

                    findNode(f.FullName);

                }
            }
            catch (Exception ex)
            {

            }
        }

        private void findNode(string xmlFilePath)
        {
            XmlDocument doc = new XmlDocument();
            DateTime date = DateTime.MaxValue;
            doc.Load(xmlFilePath);
            XmlNode root = doc.DocumentElement;
            List<Employee> Employees = new List<Employee>();

            var serializer = new XmlSerializer(typeof(Employee), new XmlRootAttribute("Employee"));
            foreach (XmlNode n in root.ChildNodes)
            {
                if (n.Name == "Employees")
                {
                    foreach (XmlNode nChildNode in n.ChildNodes)
                    {
                        using (TextReader reader = new StringReader(nChildNode.OuterXml))
                        {
                            var em = (Employee) serializer.Deserialize(reader);
                            //em.Percentage = double.Parse(nChildNode.ChildNodes[3].InnerText);
                            em.EmployeeId = int.Parse(nChildNode.Attributes["EmployeeId"].Value);
                            Employees.Add(em);
                        }


                    }
                }

                if (n.Name == "FileDate")
                {
                    date = DateTime.Parse(n.InnerText);
                }
            }

            EmpDictionaryXml.Add(date, Employees);
        }

        public void ReadXmlFiles(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                DirectoryInfo dirSource = new DirectoryInfo(folderPath);
                var allXMLFiles = dirSource.GetFiles("*.xml", SearchOption.AllDirectories).ToList();

                foreach (var f in allXMLFiles)
                {
                    UpdateWithXmlFile(f);
                }
            }
        }


        #endregion


        public void ScanAndUpdate()
        {
            foreach (var empRec in EmpDictionaryXml)
            {

                FindAndUpdateRecord(empRec);
            }
            //Result is ready from Xml files, start update the csv file ...
            foreach (var emp in result)
            {
                FindAndUpdateRecordsInCsv(emp);
            }

        }

        private void FindAndUpdateRecord(KeyValuePair<DateTime, List<Employee>> empRec)
        {
            if (result.Any())
            {
                //Add new records to result list with elements
                foreach (var rec in empRec.Value)
                {
                    CheckRecordExistAndUpdate(rec);
                }
            }
            else
            {
                result.AddRange(empRec.Value);
            }
        }

        private void CheckRecordExistAndUpdate(Employee rec)
        {
            var foundRecs = result.FirstOrDefault(re => re.EmployeeId == rec.EmployeeId && re.StartDate == rec.StartDate);
            if (foundRecs != null)
            {
                foundRecs.EndDate = rec.EndDate;
                foundRecs.Percentage = rec.Percentage;
                foundRecs.TypeId = rec.TypeId;
            }
            else
            {
                result.Add(rec);
            }
        }

        //public void WriteNewCsvToFile()
        //{
        //    using (var writer = new StreamWriter(GetRootPath()+"\\NewData.csv"))
        //    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        //    {
        //        foreach (var item in EmpDictionaryCsv)
        //        {
                    
        //            csv.WriteRecords(item.Value.ToString());

        //        }
        //    }
        //}

        private void FindAndUpdateRecordsInCsv(Employee emp)
        {
            if (!EmpDictionaryCsv.ContainsKey(emp.EmployeeId))
            {
                EmpDictionaryCsv.Add(emp.EmployeeId, new List<Absence>());
            }
            var thisEmployeeAbs = EmpDictionaryCsv.FirstOrDefault(i => i.Key == emp.EmployeeId);


            foreach (var d in GetDates(emp.StartDate, emp.EndDate))
            {
                CheckThisDateExistsInCsv(thisEmployeeAbs.Value, d, emp);
            }
        }

        private void CheckThisDateExistsInCsv(List<Absence> thisEmployeeAbs, DateTime d, Employee emp)
        {
            var abs= thisEmployeeAbs.FirstOrDefault(ab=>ab.StartDate==d);
            if (abs == null)
                AddDateToEmployeeAbsences(d, emp);
            else
            {
                abs.Percentage = emp.Percentage;
                abs.AbsenceType = (AbsenceType)emp.TypeId;
            }
        }

        private void AddDateToEmployeeAbsences(DateTime dateTime, Employee emp)
        {
            EmpDictionaryCsv[emp.EmployeeId].Add(new Absence
            {
                StartDate = dateTime,
                AbsenceType = (AbsenceType) emp.TypeId,
                Percentage = emp.Percentage
            });
        }

        private IEnumerable<DateTime> GetDates(DateTime start, DateTime end)
        {
            for (var day = start.Date; day <= end; day = day.AddDays(1))
                yield return day;
        }

        public void GetEmpAbsMarch()
        {
            Console.WriteLine("Anställnings id som är frånvarande någon under Mars månad med minst 85% frånvaro..");
            List<int> ids = new List<int>();
            //var embAbs= EmpDictionaryCsv.Where(d=>d.Value.st.Month==3 ).Select(id=>id.EmployeeId);
            foreach (var abs in EmpDictionaryCsv)
            {
                foreach (var ab in abs.Value)
                {
                    if (ab.StartDate.Month == 3 && ab.Percentage >= 0.85)
                    {
                        ids.Add(abs.Key);
                    }
                }
            }
            var newListIds = ids.Distinct().ToList();
            foreach (var item in newListIds)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine("**************************");
        }
        public void GetEmpCountAbsMarchTA()
        {
            Console.WriteLine("Anställnings antal som är frånvarande någon under March med frånvarotyp A.");
            List<int> employees = new List<int>();

            foreach (var abs in EmpDictionaryCsv)
            {
                foreach (var ab in abs.Value)
                {

                    if (ab.StartDate.Month == 3 && ab.AbsenceType == (AbsenceType)1)
                    {
                        employees.Add(abs.Key);
                    }
                }
            }
            var employeeCount = employees.Distinct().ToList();
            Console.WriteLine(employeeCount.Count + "\n**************************");
        }
        public void GetEmpContextAbsApril()
        {
            Console.WriteLine("Anställnings antal som har sammanhängande frånvaro om minst 5 dagar under April oavsett typ och procent.");
            List<int> employees = new List<int>();

            foreach (var abs in EmpDictionaryCsv)
            {
                foreach (var ab in abs.Value)
                {
                    var diffDay = GetDiffInDays(ab.StartDate, ab.EndDate);
                    if (ab.StartDate.Month == 4 && diffDay >= 5)
                    {
                        employees.Add(abs.Key);
                    }
                }
            }
            var employeeCount = employees.Distinct().ToList();
            Console.WriteLine(employeeCount.Count + "\n**************************");
        }

        public string GetRootPath()
        {
            var path=  Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Data\");
            DirectoryInfo directoryInfo = new DirectoryInfo(path).Parent.Parent.Parent.Parent;
            var allPath = directoryInfo + "\\Data\\";

            return allPath;
        }
    }

    public interface IFileHandler
    {
        public Dictionary<int, List<Absence>> EmpDictionaryCsv { get; set; }
        public void ReadCsvFile(string file);
        public void ReadXmlFiles(string folderPath);
        public string GetRootPath();
        void ScanAndUpdate();
        void WriteNewCsvToFile();
        void AddNewAbsenceRec(Absence absRec, int id);

        void GetEmpAbsMarch();
        void GetEmpCountAbsMarchTA();
        void GetEmpContextAbsApril();
    }
}
