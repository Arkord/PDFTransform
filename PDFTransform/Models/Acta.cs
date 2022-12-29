using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFTransform.Models
{
    public class Acta
    {
        public string career { get; set; }
        public string teacher { get; set; }
        public string course { get; set; }
        public string group { get; set; }
        public string period { get; set; }
        public List<Student> students { get; set; }
        public int numStudents { get; set; }    
        public int approved { get; set; }
        public int failed => numStudents - approved;
        public decimal average { get; set; }

    public Acta()
        {
            career = "";
            teacher = "";
            course = "";
            group = "";
            period = "";
            students = new List<Student>();
        }

        private int GetAproved()
        {
            int approved = 0;

            foreach(Student student in students)
            {
                if(student.grade != "NC")
                {
                    approved++;
                }
                
            }

            return approved;
        }

        private int GetFailed()
        {
            int failed = 0;

            foreach (Student student in students)
            {
                if (student.grade == "NC")
                {
                    failed++;
                }

            }

            return failed;
        }

        private decimal GetAverage()
        {
            decimal average = 0;

            if (students.Count > 0)
            {
                foreach (Student student in students)
                {
                    if (student.grade != "NC")
                    {
                        average = average + Convert.ToInt32(student.grade);
                    }
                    else
                    {
                        average = average + 6;
                    }
                }
                average = average / students.Count;
            }
            return average;
        }
    }

    public class Student
    {
        public string studentId { get; set; }
        public string studentName { get; set; }
        public string grade { get; set; }

        public Student()
        {
            studentId = "";
            studentName = "";
            grade = "";
        }
    }
}
