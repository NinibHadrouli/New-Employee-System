using System;
using System.Collections.Generic;
using System.Text;

namespace AbscenceSystem
{
    [Serializable]
    public class Employee
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TypeId { get; set; }
        public double Percentage { get; set; }
        public int EmployeeId { get; set; }
    }
}