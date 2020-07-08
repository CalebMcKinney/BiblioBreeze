using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BiblioBreeze
{
    public class Book
    {
        public string bookID;
        public string fileLocation;

        public string bookName { get; set; }
        public string author { get; set; }

        public bool defaultFont = false;

        public Teacher assignedBy;

        public ObservableCollection<StudentCode> studentsAssigned = new ObservableCollection<StudentCode>();

        public List<string> chapterTitles = new List<string>();

        public static string IncrementBookID(string previous)
        {
            int newNum = Convert.ToInt16(previous.Substring(1)) + 1;
            return String.Format("B{0}", newNum.ToString());
        }
    }
}
