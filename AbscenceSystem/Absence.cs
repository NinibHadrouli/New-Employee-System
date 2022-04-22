using System;
using System.Collections.Generic;
using System.Text;

namespace AbscenceSystem
{
    public class Absence
    {
        //public Employee Employee { get; set; }
        public AbsenceType AbsenceType { get; set; }
        public double Percentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }


        public bool Equals(Absence other)
        {
            //return Employee.Equals(other.Employee) && Date.Equals(other.Date);
            return true;
        }
    }

    public class AbsenceListForEmployee
    {
        private List<Absence> EmpAbcences { get; set; }
        private Employee Employee { get; set; }

        public AbsenceListForEmployee(Employee employee)
        {
            Employee = employee;
        }

        public void AddAbsence(Absence abs)
        {

        }
    }

    public enum AbsenceType
    {
        One = 1,
        Two = 2,
        Three = 3
    }
}