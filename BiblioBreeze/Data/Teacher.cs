using System;
using System.Collections.ObjectModel;

namespace BiblioBreeze
{
    public class Teacher
    {
        public string teacherID;

        public enum Title { Mr, Ms, Mrs, Dr, Prof }
        public Title title;

        public string lastName;

        public ObservableCollection<Book> booksAssigned = new ObservableCollection<Book>();

        public static string IncrementTeacherID(string previous)
        {
            int newNum = Convert.ToInt16(previous.Substring(1)) + 1;
            return String.Format("T{0}", newNum.ToString());
        }
    }
}
