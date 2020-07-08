using Google.Apis.Upload;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BiblioBreeze
{
    public partial class TeacherView : Window
    {
        string bookPath;
        string bookExtractedPath;

        Book mutableBook;

        private void UploadNewBook(object sender, RoutedEventArgs e)
        {
            ScreenDarken.Visibility = Visibility.Visible;
            UploadBookMenu.Visibility = Visibility.Visible;
        }

        private void CancelBookUpload(object sender, RoutedEventArgs e)
        {
            ScreenDarken.Visibility = Visibility.Collapsed;
            UploadBookMenu.Visibility = Visibility.Collapsed;

            BookUploadPrompt.Visibility = Visibility.Visible;
            BookUploadInfo.Visibility = Visibility.Collapsed;
            BookUploadAddStudentMenu.Visibility = Visibility.Collapsed;

            BookDisplayGrid.Visibility = Visibility.Visible;
            BooksNotFoundMsg.Visibility = Visibility.Collapsed;
            
            AssignBookTitle.Clear();
            AssignBookAuthor.Clear();

            BookUploadInfo.ScrollToHome();
        }

        private void ChooseFileButton(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "EPUB files (*.epub) | *.epub";
            
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                BookUploadPrompt.Visibility = Visibility.Collapsed;
                BookUploadInfo.Visibility = Visibility.Visible;

                bookPath = openFileDialog.FileName;
                BookFileNameText.Text = System.IO.Path.GetFileName(bookPath);

                ExtractEPUB();

                mutableBook = new Book();
                mutableBook.studentsAssigned = new ObservableCollection<StudentCode>();
                mutableBook.fileLocation = bookExtractedPath;
                mutableBook.assignedBy = activeTeacher;

                BookUploadStudentsInfo.ItemsSource = mutableBook.studentsAssigned;
            }
        }

        private void ExtractEPUB()
        {
            string fileName = System.IO.Path.GetFileNameWithoutExtension(bookPath);

            File.Copy(bookPath, System.IO.Path.Combine("Library", fileName + ".zip"), true);
            string tempPath = System.IO.Path.Combine("Library", fileName);

            if (Directory.Exists(tempPath))
            {
                FileUtility.DeleteDirectory(tempPath);
            }

            FileUtility.UnZIPFiles(System.IO.Path.Combine("Library", fileName + ".zip"), tempPath);

            bookExtractedPath = tempPath;
        }

        private void ChooseAnotherFile()
        {
            BookUploadPrompt.Visibility = Visibility.Visible;
            BookUploadInfo.Visibility = Visibility.Collapsed;

            FileUtility.DeleteDirectory(bookExtractedPath);

            bookExtractedPath = string.Empty;
            bookPath = string.Empty;
        }

        private void GenerateBookInformation(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(bookExtractedPath))
            {
                return;
            }

            var allBookInfo = Directory.GetFiles(bookExtractedPath, "*.opf", SearchOption.AllDirectories);

            if (allBookInfo.Length > 0)
            {
                StreamReader bookInfoFile = new StreamReader(allBookInfo[0]);
                string bookInfoText = bookInfoFile.ReadToEnd();

                AssignBookTitle.Text = FindTextBetweenTag("<dc:title", "<", bookInfoText);
                AssignBookAuthor.Text = FindTextBetweenTag("opf:role=\"aut\"", "<", bookInfoText);

                //Need to figure out way of finding the cover picture
                string coverFileName = FindTextBetweenTag("", "<", bookInfoText);
            }
            else
            {
                NotifyError("Book file has no book data");
            }
        }

        private string FindTextBetweenTag(string opening, string closing, string source)
        {
            int openingIndex = source.IndexOf('>', source.IndexOf(opening) + opening.Length) + 1;

            if (openingIndex != -1)
            {
                int closingIndex = source.IndexOf(closing, openingIndex);
                return source.Substring(openingIndex, closingIndex - openingIndex);
            }
            else
            {
                return string.Empty;
            }
        }

        private void BookUploadAddStudent(object sender, RoutedEventArgs e)
        {
            BookUploadInfo.Visibility = Visibility.Collapsed;
            BookUploadAddStudentMenu.Visibility = Visibility.Visible;
        }

        private void RandomizeBookUploadStudentCode(object sender, RoutedEventArgs e)
        {
            CodeGenerator codeGen = new CodeGenerator();
            BookUploadStudentCode.Text = codeGen.GenerateCode();
        }

        private void BookUploadConfirmStudent(object sender, RoutedEventArgs e)
        {
            BookUploadInfo.Visibility = Visibility.Visible;
            BookUploadAddStudentMenu.Visibility = Visibility.Collapsed;

            bool codeMatch = false;

            foreach (StudentCode curStudent in mutableBook.studentsAssigned)
            {
                if (StudentCodeBox.Text == curStudent.bookCode)
                {
                    NotifyError("Book code already exists");
                    codeMatch = true;
                    break;
                }
            }

            if (!codeMatch)
            {
                StudentCode newStudent = new StudentCode()
                {
                    studentName = BookUploadStudentName.Text,
                    graduationYear = BookUploadGradYear.Text,
                    bookCode = BookUploadStudentCode.Text,
                    bookSource = mutableBook
                };

                BookUploadStudentName.Clear();
                BookUploadGradYear.Clear();
                BookUploadStudentCode.Clear();

                mutableBook.studentsAssigned.Add(newStudent);
            }

            double studentItemHeight = 75;

            if (BookUploadStudentsInfo.Items.Count * studentItemHeight < ScrollGuardian.Height)
                ScrollGuardian.Visibility = Visibility.Visible;
            else
                ScrollGuardian.Visibility = Visibility.Collapsed;
        }

        private void CancelBookUploadStudentAddition(object sender, RoutedEventArgs e)
        {
            BookUploadInfo.Visibility = Visibility.Visible;
            BookUploadAddStudentMenu.Visibility = Visibility.Collapsed;

            BookUploadStudentName.Clear();
            BookUploadGradYear.Clear();
            BookUploadStudentCode.Clear();
        }        

        private void BookUploadDeleteStudent(object sender, RoutedEventArgs e)
        {
            ListBoxItem parentListItem = MiscResources.Utility.FindParent<ListBoxItem>(sender as Button);
            int buttonIndex = BookUploadStudentsInfo.Items.IndexOf(parentListItem.DataContext);

            mutableBook.studentsAssigned.RemoveAt(buttonIndex);
        }

        private void ConfirmBookUpload(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(bookPath))
            {
                NotifyError("Failed to find book on computer!");
                return;
            }

            if (String.IsNullOrWhiteSpace(AssignBookTitle.Text))
            {
                NotifyError("Please enter a book title!");
                return;
            }

            if (String.IsNullOrWhiteSpace(AssignBookAuthor.Text))
            {
                NotifyError("Please enter a book author!");
                return;
            }

            BookUploadInProgressMenu.Visibility = Visibility.Visible;
            BookUploadClose.Visibility = Visibility.Collapsed;

            Action upload = async () =>
            {
                string driveLocation = await Database.db.UploadToDriveAsync(bookPath, AssignBookTitle.Text,
                   (IUploadProgress progress) =>
                   {
                       Action action = () =>
                       {
                           //if (progress.Status == UploadStatus.Completed)
                           //{
                           //}
                           //
                           //if(progress.Status == UploadStatus.Uploading)
                           //{
                           //}
                       };
                   
                       Dispatcher.BeginInvoke(action);
                   });

                #region Upload Book INFORMATION
                string bookID = Book.IncrementBookID(Database.db.FindLastColVal(1, Database.Source.Books));

                mutableBook.bookName = AssignBookTitle.Text;
                mutableBook.author = AssignBookAuthor.Text;

                for (int i = 0; i < mutableBook.studentsAssigned.Count; i++)
                {
                    mutableBook.studentsAssigned[i].assigned = DateTime.Now;
                }

                activeTeacher.booksAssigned.Add(mutableBook);
                List<object> bookAttributes = new List<object>
                {
                    bookID,
                    driveLocation,
                    mutableBook.bookName,
                    mutableBook.author,
                    activeTeacher.teacherID
                };

                Database.db.AppendRow(bookAttributes, Database.Source.Books);

                foreach (StudentCode student in mutableBook.studentsAssigned)
                {
                    List<object> studentAttributes = new List<object>
                    {
                        student.bookCode,
                        student.studentName,
                        DateTime.Now.ToString("yyyy-MM-dd"),
                        student.graduationYear,
                        bookID
                    };

                    Database.db.AppendRow(studentAttributes, Database.Source.Students);
                }

                CloseBookUploadMenu();
                NotifySuccess("Book successfully uploaded");
                #endregion
            };

            Dispatcher.BeginInvoke(upload);
        }

        public void CloseBookUploadMenu()
        {
            AssignBookTitle.Clear();
            AssignBookAuthor.Clear();

            BookUploadPrompt.Visibility = Visibility.Visible;
            BookUploadInfo.Visibility = Visibility.Collapsed;
            BookUploadAddStudentMenu.Visibility = Visibility.Collapsed;
            BookUploadInProgressMenu.Visibility = Visibility.Collapsed;

            BookUploadClose.Visibility = Visibility.Visible;

            BookDisplayGrid.Visibility = Visibility.Visible;
            BooksNotFoundMsg.Visibility = Visibility.Collapsed;

            ScreenDarken.Visibility = Visibility.Collapsed;
            UploadBookMenu.Visibility = Visibility.Collapsed;

            BookUploadInfo.ScrollToHome();
        }
    }
}
