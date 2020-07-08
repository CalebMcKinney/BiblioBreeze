using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace BiblioBreeze
{
    //This class pertains to student modification, addition, and deletion
    public partial class TeacherView : Window
    {
        private void InvokeStudentModify(object sender, RoutedEventArgs e)
        {
            BookExpandedMenu.Visibility = Visibility.Collapsed;
            ScreenDarken.Visibility = Visibility.Visible;
            StudentMenu.Visibility = Visibility.Visible;

            if ((sender as Button).Tag.ToString() == "Add")
            {
                StudentMenuHeading.Text = "Add a New Student to:";
                DataChangeButtonLabel.Text = "Add Student";

                StudentMenuSubheading.Text = selectedBook.bookName;

                StudentNameBox.Clear();
                GradYearBox.Clear();
                StudentCodeBox.Clear();
            }
            else
            {
                StudentMenuHeading.Text = "Editing Info of:";

                DataChangeButtonLabel.Text = "Apply Changes";

                ListBoxItem parentListItem = MiscResources.Utility.FindParent<ListBoxItem>(sender as Button);
                StudentCode highlightedStudent = parentListItem.DataContext as StudentCode;

                editingIndex = StudentsInfoList.Items.IndexOf(highlightedStudent);

                StudentNameBox.Text = highlightedStudent.studentName;
                StudentMenuSubheading.Text = highlightedStudent.studentName;

                GradYearBox.Text = highlightedStudent.graduationYear;
                StudentCodeBox.Text = highlightedStudent.bookCode;
            }
        }

        private void CancelStudentModify(object sender, RoutedEventArgs e)
        {
            BookExpandedMenu.Visibility = Visibility.Visible;
            StudentMenu.Visibility = Visibility.Collapsed;
        }

        private void RemoveStudent(object sender, RoutedEventArgs e)
        {
            ListBoxItem parentListItem = MiscResources.Utility.FindParent<ListBoxItem>(sender as Button);
            int buttonIndex = StudentsInfoList.Items.IndexOf(parentListItem.DataContext);

            string bookCode = (parentListItem.DataContext as StudentCode).bookCode;
            int studentRow = Database.db.FindRowsByColVal(1, bookCode, Database.Source.Students)[0];
            //Database.db.DeleteRow(studentRow, bookCode, Database.Source.Students);

            (StudentsInfoList.ItemsSource as ObservableCollection<StudentCode>).RemoveAt(buttonIndex);

            if (selectedBook.studentsAssigned.Count == 0)
            {
                StudentsInfoList.Visibility = Visibility.Collapsed;
                StudentsNotFound.Visibility = Visibility.Visible;
            }

            NotifySuccess("Student successfully removed from book");
        }

        private void RandomizeCode(object sender, RoutedEventArgs e)
        {
            CodeGenerator codeGen = new CodeGenerator();
            StudentCodeBox.Text = codeGen.GenerateCode();
        }

        //Equal to -1 if nothing is being edited
        int editingIndex = -1;

        private bool VerifyStudentConfirmation()
        {
            if (String.IsNullOrWhiteSpace(StudentCodeBox.Text))
            {
                NotifyError("Please enter a student code!");
                return true;
            }
            if (String.IsNullOrWhiteSpace(StudentNameBox.Text))
            {
                NotifyError("Please enter a student name!");
                return true;
            }
            if (String.IsNullOrWhiteSpace(GradYearBox.Text))
            {
                NotifyError("Please enter a student graduation year!");
                return true;
            }
            int gradYearInt;
            if (!int.TryParse(GradYearBox.Text, out gradYearInt))
            {
                NotifyError("Please enter a valid student graduation year!");
                return true;
            }
            //If the student code is being edited and already exists, allow it.
            if (editingIndex == -1 && Database.db.FindRowsByColVal(1, StudentCodeBox.Text, Database.Source.Students).Count != 0)
            {
                NotifyError("The entered book code already exists");
                return true;
            }

            return false;
        }

        private void ConfirmStudentAddition(object sender, RoutedEventArgs e)
        {
            if (VerifyStudentConfirmation())
                return;

            //If book is not being edited, run through default routine.
            if (editingIndex == -1)
            {
                StudentCode newStudent = new StudentCode();
                List<object> studentAttributes = new List<object>();

                #region Assign attributes
                newStudent.bookCode = StudentCodeBox.Text;
                studentAttributes.Add(StudentCodeBox.Text);

                newStudent.studentName = StudentNameBox.Text;
                studentAttributes.Add(StudentNameBox.Text);

                newStudent.assigned = DateTime.Now;
                studentAttributes.Add(DateTime.Now.ToString("yyyy-MM-dd"));

                newStudent.graduationYear = GradYearBox.Text;
                studentAttributes.Add(GradYearBox.Text);

                newStudent.bookSource = selectedBook;
                studentAttributes.Add(selectedBook.bookID);
                #endregion

                (BookContainer.ItemsSource as ObservableCollection<Book>)[selectedBookIndex].studentsAssigned.Add(newStudent);
                Database.db.AppendRow(studentAttributes, Database.Source.Students);

                StudentNameBox.Clear();
                GradYearBox.Clear();
                StudentCodeBox.Clear();

                BookExpandedMenu.Visibility = Visibility.Visible;
                StudentMenu.Visibility = Visibility.Collapsed;

                StudentsInfoList.Visibility = Visibility.Visible;
                StudentsNotFound.Visibility = Visibility.Collapsed;

                NotifySuccess("Student successfully added.");
            }
            else
            {
                string oldStudentCode = (StudentsInfoList.ItemsSource as ObservableCollection<StudentCode>)[editingIndex].bookCode;
                int studentRow = Database.db.FindRowsByColVal(1, oldStudentCode, Database.Source.Students)[0];

                StudentCode mutableStudent = (StudentsInfoList.ItemsSource as ObservableCollection<StudentCode>)[editingIndex];

                if (!String.Equals(StudentNameBox.Text, mutableStudent.studentName))
                {
                    mutableStudent.studentName = StudentNameBox.Text;
                    Database.db.WriteToCell(2, studentRow, StudentNameBox.Text, Database.Source.Students);
                }

                if (!String.Equals(GradYearBox.Text, mutableStudent.graduationYear))
                {
                    mutableStudent.graduationYear = GradYearBox.Text;
                    Database.db.WriteToCell(4, studentRow, GradYearBox.Text, Database.Source.Students);
                }

                if (!String.Equals(StudentCodeBox.Text, mutableStudent.bookCode))
                {
                    if (Database.db.FindRowsByColVal(1, StudentCodeBox.Text, Database.Source.Students).Count != 0)
                    {
                        NotifyError("The entered book code already exists");
                        return;
                    }

                    mutableStudent.bookCode = StudentCodeBox.Text;
                    Database.db.WriteToCell(1, studentRow, StudentCodeBox.Text, Database.Source.Students);

                    //Update Reading Data to reflect accurately the new student code
                    foreach (int readingDataRow in Database.db.FindRowsByColVal(1, oldStudentCode, Database.Source.ReadingData))
                    {
                        Database.db.WriteToCell(1, readingDataRow, StudentCodeBox.Text, Database.Source.ReadingData);
                    }
                }

                (StudentsInfoList.ItemsSource as ObservableCollection<StudentCode>)[editingIndex] = mutableStudent;
                NotifySuccess("Student successfully modified");

                StudentsInfoList.Items.Refresh();

                StudentNameBox.Clear();
                GradYearBox.Clear();
                StudentCodeBox.Clear();

                BookExpandedMenu.Visibility = Visibility.Visible;
                StudentMenu.Visibility = Visibility.Collapsed;

                editingIndex = -1;
            }
        }

    }
}
