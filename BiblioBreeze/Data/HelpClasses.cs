using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiblioBreeze
{
    public class Question
    {
        public static List<Question> allQuestions =
            new List<Question>()
            {
                new Question(){ question = "How can I upload a new book to biblioBreeze?", answer = "To upload a book to biblioBreeze, the first step is ensuring that your file is in the correct format. If it is, then you are ready to upload the file to our software. Press the ‘Upload New Book’ button at the bottom of your home screen as a teacher. When the dialog prompts you to upload a book, select your book’s file location, then continue on to the file attribute assignment menu."},
                new Question(){ question = "How can I assign a book to students as I upload it?", answer = "To assign a book to students as you upload it, the first step is following the instructions in the book attribute assignment menu. Upload a .epub book, and give it a title and author. Once you’ve done that, scroll down to the list box below the heading ‘Students’. Press add student and fill out the information for the student you want to assign the book to."},
                new Question(){ question = "How can I assign a book to students after it has been uploaded?", answer = "To assign a book to students after you’ve already uploaded it, the first step is opening the book from the list of students on the home screen as a teacher by clicking on it. Once the book has been expanded, you can view the students you’ve already assigned the book to. You should press ‘Add Student’ at the bottom, and from there, you’ll be given a menu to add a new student to your book."},
                new Question(){ question = "How do my students read a book that I’ve assigned?", answer = "Whenever you assign students a book, there is a text box asking you what student code you would like to give to the student. There is a button beside it that allows you to randomize the contents of the textbox, but you may also enter whichever code you’d like to give your student. The student codes that are submitted can be used by students to read the book you’ve uploaded. For them to do this, they must first open the software to its home screen. There is a text box on the right side of the software offering students the ability to redeem their book code and read the book."},
                new Question(){ question = "How do I edit the user information of students?", answer = "To edit the user information of students, the first step is opening the book from the list of students on the home screen as a teacher by clicking on it. Once the book has been expanded, you can view the students already added to the book. Hover your cursor over whichever one you’d like to edit, and press the Edit Student button."},
                new Question(){ question = "How do my students save their location in the book they’re reading?", answer = "Book location is saved and loaded for students automatically in the students reading data. The teacher may view, from the data report menu, what page the students are at as well as the total amount of time they’ve spent reading the book."},
                new Question(){ question = "How do I view the reading data of my students?", answer = "To view the data report for your students, you should first open the expanded book menu. To do this, click on one of the books found in the list on your home screen as a teacher. Once the book has been expanded, click the button at the bottom of the screen, ‘View Data Report’. From the data report menu, you have the ability to view the total percentage read and time spent for each student. As you hover over an individual piece of data, you will be able to see the reading data for each student by chapter."},
                new Question(){ question = "How can I delete my student’s reading data for a chapter?", answer = "To delete a student’s reading data for a certain chapter, you should first open the data report menu. Go to the expanded book menu, then the data report menu. Hover over a student’s data, and click the button with the three dots on the right side. You will be given the option to delete or ignore the reading data for the highlighted chapter of the student’s reading data. Click the clear student data button, and the data for the highlighted chapter will be reset."},
                new Question(){ question = "How can I analyze my students’ reading data?", answer = "To analyze a student’s reading data, you should first open the data report menu. Go to the expanded book menu, then the data report menu. From this menu, you are given a variety of tools that allow you to customize and analyze data. You can view the total percentage read for a student for a book, or the percentage for each individual chapter. You may also view the total time read for a student for a book, and the time read for each individual chapter."},
                new Question(){ question = "How can I ignore the reading data of a chapter in the total reading data for a student?", answer = "To ignore a student’s reading data for a certain chapter, you should first open the data report menu. Go to the expanded book menu, then the data report menu. Hover over a student’s data, and click the button with the three dots on the right side. You will be given the option to delete or ignore the reading data for the highlighted chapter of the student’s reading data. Click the ignore student data button, and the data for the highlighted chapter will be ignored in the total of the student’s data for the book."},
            };

        public static List<Keyword> allKeywords =
            new List<Keyword>()
            {
                new Keyword("upload", allQuestions[0]),
                new Keyword("upload", allQuestions[1]), new Keyword("assign", allQuestions[1]),
                new Keyword("upload", allQuestions[2]), new Keyword("assign", allQuestions[2]),
                new Keyword("student", allQuestions[3]), new Keyword("read", allQuestions[3]),
                new Keyword("user", allQuestions[4]), new Keyword("student", allQuestions[4]), new Keyword("teacher", allQuestions[4]),
                new Keyword("student", allQuestions[5]), new Keyword("save", allQuestions[5]),
                new Keyword("data", allQuestions[6]), new Keyword("reading", allQuestions[6]), new Keyword("student", allQuestions[6]),
                new Keyword("data", allQuestions[7]), new Keyword("reading", allQuestions[7]), new Keyword("student", allQuestions[7]),
                new Keyword("analyze", allQuestions[8]), new Keyword("student", allQuestions[8]), new Keyword("reading", allQuestions[8]), new Keyword("data", allQuestions[8]),
                new Keyword("analyze", allQuestions[9]), new Keyword("student", allQuestions[9]), new Keyword("reading", allQuestions[9]), new Keyword("data", allQuestions[9])
            };

        public string question { get; set; }
        public string answer;

        public List<string> keywords
        {
            get
            {
                return allKeywords.Where(k => k.query == this).Select(q => q.word).ToList();
            }
        }
        public string keywordsFormatted
        {
            get
            {
                string allKeywords = "Keywords: ";

                foreach (string word in keywords)
                {
                    allKeywords += word + ", ";
                }

                return allKeywords.Substring(0, allKeywords.Length - 2);
            }
        }
    }

    public class Keyword
    {
        public string word;
        public Question query;

        public Keyword(string word, Question query)
        {
            this.word = word.ToLower();
            this.query = query;
        }
    }
}
