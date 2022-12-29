using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFTransform.Models
{
    public class Heuristic
    {
        public static int career => 1;
        public static int course => 2;
        public static int period => 4;
        public static int group = 10;
        public static int teacher => 11;
        public static int dataStart => 13;
        

        public Heuristic()
        {
            
        }

        public static Acta GetData(string[] content, Acta acta)
        {
            int contentLen = content.Length;

            if(acta.career.Length == 0)
            {
                acta.career = content[career];
                acta.course = content[course].Replace("ASIGNATURA: ", "");
                acta.period = content[period].Replace("ACTA DE CALIFICACIONES FINALES DEL PERIODO ", "");
                acta.group = content[group];
                acta.teacher = content[teacher];
            }
            

            for(int index = dataStart; index < contentLen; index++)
            {
                string line = content[index];
                if (int.TryParse(line[0].ToString(), out _))
                {
                    string[] item = line.Split(" ");
                    int start = 1;
                    int end = item.Length - 1;

                    Student student = new Student();
                    student.studentId = item[start];

                    int limit = 1;
                    if (int.TryParse(item[end - 1].ToString(), out _))
                    {
                        student.grade = item[end - 1];
                    }
                    else
                    {
                        student.grade = item[end - 2];
                        limit = 2;
                    }
                        
                    string name = "";
                    for(int extract = 2; extract < end - limit; extract++)
                    {
                        name = name + item[extract] + " ";
                    }
                    name = name.TrimEnd(' ');

                    student.studentName = name;
                    acta.students.Add(student);
                }
                else
                {
                    break;
                }
            }

            acta.numStudents = acta.students.Count;
            acta.approved = acta.students.Where(s => s.grade != "NC").Count();

            if(acta.numStudents > 0)
            {
                acta.average = acta.students.Sum(s => s.grade != "NC" ? Convert.ToDecimal(s.grade) : 6) / acta.numStudents;
            }
            
            return acta;
        }

        public static bool CanAdd(List<Acta> actas, Acta acta)
        {
            bool canAdd = false;

            if (actas.Find(x => (x.group + x.course) == (acta.group + acta.course)) == null && acta.group.Length > 0)
            {
                canAdd = true;
            }
            
            return canAdd;
        }

        public static bool IsActa(string line)
        {
            bool isActa = false;

            if(line.ToLower().Contains("matricula nombre del alumno(a)"))
            {
                isActa = true;
            }

            return isActa;
        }
    }
}
