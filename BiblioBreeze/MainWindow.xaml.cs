using Google.Apis.Download;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BiblioBreeze
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += SetDefaultAnimGoal;

            //Intialize the database service
            new Database();
            //Database.db.DeleteRow(5, Database.Source.Students);
        }

        #region Focus Panel Swap
        Storyboard creditsTransitionAnim;

        bool creditsOnLeft = false;

        public Thickness marginTo;
        public Thickness marginFrom;

        void SetDefaultAnimGoal(object sender, RoutedEventArgs e)
        {
            UpdateSwapAnimationGoals();
        }

        private void UpdateSwapAnimationGoals()
        {
            double columnWidth = UIcontainer.ColumnDefinitions[0].ActualWidth;

            if (creditsOnLeft)
            {
                marginTo = new Thickness(columnWidth, 0, 0, 0);
                marginFrom = new Thickness(0, 0, columnWidth, 0);
            }
            else
            {
                marginTo = new Thickness(0, 0, columnWidth, 0);
                marginFrom = new Thickness(columnWidth, 0, 0, 0);
            }
        }

        private void SwapViewButtonClick(object sender, RoutedEventArgs e)
        {
            //creditsOnLeft = !creditsOnLeft;
            if(creditsOnLeft != ((sender as Button).Name == "SwapToLeftBtn"))
            {
                creditsOnLeft = !creditsOnLeft;

                UpdateSwapAnimationGoals();

                creditsTransitionAnim = CreditsRegion.FindResource("CreditsTransitionAnim") as Storyboard;

                (creditsTransitionAnim.Children[0] as ThicknessAnimation).To = marginTo;
                (creditsTransitionAnim.Children[0] as ThicknessAnimation).From = CreditsRegion.Margin;

                creditsTransitionAnim.Stop();
                BeginStoryboard(creditsTransitionAnim);
            }
        }
        #endregion

        #region Login/Create Acc
        private void SwapToCreate(object sender, RoutedEventArgs e)
        {
            BeginStoryboard(SwapToCreateAnimBegin.Storyboard);
        }

        private void SwapToLogin(object sender, RoutedEventArgs e)
        {
            BeginStoryboard(SwapToLoginAnimBegin.Storyboard);
        }
        #endregion

        private bool VerifyAccountCreation()
        {
            if(String.IsNullOrWhiteSpace(AccountCreationUsername.Text))
            {
                TeacherError("Please enter a username!");
                return false;
            }
            else if(!(AccountCreationUsername.Text.All(char.IsLetterOrDigit)))
            {
                TeacherError("Username is in an invalid format!");
                return false;
            }
            else if (String.IsNullOrWhiteSpace(AccountCreationPassword.Password))
            {
                TeacherError("Please enter a password!");
                return false;
            }
            else if (String.IsNullOrWhiteSpace(AccountCreationLastName.Text))
            {
                TeacherError("Please enter a last name!");
                return false;
            }
            else if (AccountCreationTitle.SelectedIndex == 0)
            {
                TeacherError("Please enter a title!");
                return false;
            }
            else if(Database.db.FindRowsByColVal(1, AccountCreationUsername.Text, Database.Source.Teachers).Count != 0)
            {
                TeacherError("Username already taken!");
                return false;
            }

            return true;
        }

        private void CreateAccount(object sender, RoutedEventArgs e)
        {
            if (!VerifyAccountCreation()) return;

            List<object> teacherAttributes = new List<object>()
            {
                AccountCreationUsername.Text,
                AccountCreationPassword.Password,
                (AccountCreationTitle.SelectedIndex - 1).ToString(),
                AccountCreationLastName.Text,
                Teacher.IncrementTeacherID(Database.db.FindLastColVal(5, Database.Source.Teachers))
            };

            Database.db.AppendRow(teacherAttributes, Database.Source.Teachers);

            var usernameRows = Database.db.FindRowsByColVal(1, teacherAttributes[0] as string, Database.Source.Teachers);
            int teacherRow = usernameRows[0];

            TeacherView.activeTeacher = InitializeTeacher(teacherRow);

            TeacherView teacherWindow = new TeacherView();
            teacherWindow.Show();
            this.Close();
        }

        void SignInAsTeacher(object sender, RoutedEventArgs e)
        {
            var usernameRows = Database.db.FindRowsByColVal(1, LoginUsernameBox.Text, Database.Source.Teachers);

            if(usernameRows.Count > 0)
            {
                string correctPassword = Database.db.ReadCell(2, usernameRows[0], Database.Source.Teachers);
                if (LoginPasswordBox.Password == correctPassword)
                {
                    int teacherRow = usernameRows[0];
                    TeacherView.activeTeacher = InitializeTeacher(teacherRow);

                    TeacherView teacherWindow = new TeacherView();
                    teacherWindow.Show();
                    this.Close();
                }
                else
                {
                    TeacherError("Incorrect password entered!");
                }
            }
            else
            {
                TeacherError("Username couldn't be found!");
            }
        }

        Teacher InitializeTeacher(int teacherRow)
        {
            Teacher activeTeacher = new Teacher();
            List<string> teacherAttributes = Database.db.ReadRow(teacherRow, Database.Source.Teachers);

            activeTeacher.title = (Teacher.Title)Convert.ToUInt16(teacherAttributes[2]);
            activeTeacher.lastName = teacherAttributes[3];

            activeTeacher.teacherID = teacherAttributes[4];
            string teacherID = teacherAttributes[4];

            foreach(int bookRow in Database.db.FindRowsByColVal(5, teacherID, Database.Source.Books))
            {
                Book curBook = new Book();
                List<string> bookAttributes = Database.db.ReadRow(bookRow, Database.Source.Books);

                string curBookID = bookAttributes[0];
                curBook.bookID = bookAttributes[0];

                curBook.bookName = bookAttributes[2];
                curBook.author = bookAttributes[3];
                
                curBook.studentsAssigned = new ObservableCollection<StudentCode>();

                foreach(int studentRow in Database.db.FindRowsByColVal(5, curBookID, Database.Source.Students))
                {
                    StudentCode curStudent = new StudentCode();
                    List<string> studentAttributes = Database.db.ReadRow(studentRow, Database.Source.Students);

                    curStudent.bookCode = studentAttributes[0];
                    curStudent.studentName = studentAttributes[1];
                    curStudent.assigned = DateTime.ParseExact(studentAttributes[2], "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                    curStudent.graduationYear = studentAttributes[3];

                    curBook.studentsAssigned.Add(curStudent);
                }

                activeTeacher.booksAssigned.Add(curBook);
            }

            return activeTeacher;
        }

        //Verify the existance of the entered student code
        void SignInAsStudent(object sender, RoutedEventArgs e)
        {
            var studentsWithCode = Database.db.FindRowsByColVal(1, StudentCodeEntry.Text, Database.Source.Students);

            if(studentsWithCode.Count() == 1)
            {
                int studentRow = studentsWithCode[0];
                StudentView.activeStudent = InitializeStudent(studentRow);

                StudentView studentWindow = new StudentView();
                studentWindow.Show();
                this.Close();
            }
            else
            {
                CodeError("Student code couldn't be found!");
            }
        }

        StudentCode InitializeStudent(int studentRow)
        {
            StudentCode activeStudent = new StudentCode();

            #region Scrape Book Data
            Book activeBook = new Book();

            string bookID = Database.db.ReadCell(5, studentRow, Database.Source.Students);
            int bookIndex = Database.db.FindRowsByColVal(1, bookID, Database.Source.Books)[0];

            List<string> bookAttributes = Database.db.ReadRow(bookIndex, Database.Source.Books);

            activeBook.bookID = bookID;
            activeBook.fileLocation = bookAttributes[1];
            activeBook.bookName = bookAttributes[2];
            activeBook.author = bookAttributes[3];
            #endregion

            #region Scrape Teacher Data
            Teacher assignedBy = new Teacher();
            string teacherID = Database.db.ReadCell(5, bookIndex, Database.Source.Books);

            int teacherIndex = Database.db.FindRowsByColVal(5, teacherID, Database.Source.Teachers)[0];

            Trace.WriteLine(teacherIndex);

            List<string> teacherAttributes = Database.db.ReadRow(teacherIndex, Database.Source.Teachers);

            assignedBy.teacherID = teacherID;
            assignedBy.lastName = teacherAttributes[3];
            int titleIndex = Convert.ToInt16(teacherAttributes[2]);
            assignedBy.title = (Teacher.Title)titleIndex;

            #endregion

            activeBook.assignedBy = assignedBy;
            activeStudent.bookSource = activeBook;
            activeStudent.bookCode = StudentCodeEntry.Text;
            activeStudent.studentName = Database.db.ReadCell(2, studentRow, Database.Source.Students);

            return activeStudent;
        }

        #region Show/Hide Creation/Login Labels
        private void LoginPasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordBoxLabel.Visibility = Visibility.Collapsed;
        }

        private void LoginUsernameBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UsernameBoxLabel.Visibility = Visibility.Collapsed;
        }

        private void LoginUsernameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((sender as TextBox).Text == String.Empty)
            {
                UsernameBoxLabel.Visibility = Visibility.Visible;
            }
            else
            {
                UsernameBoxLabel.Visibility = Visibility.Collapsed;
            }
        }

        private void LoginPasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((sender as PasswordBox).Password == String.Empty)
            {
                PasswordBoxLabel.Visibility = Visibility.Visible;
            }
            else
            {
                PasswordBoxLabel.Visibility = Visibility.Collapsed;
            }
        }

        private void AccountCreationLastName_GotFocus(object sender, RoutedEventArgs e)
        {
            AccountCreationLastNameLabel.Visibility = Visibility.Collapsed;
        }

        private void AccountCreationLastName_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((sender as TextBox).Text == String.Empty)
            {
                AccountCreationLastNameLabel.Visibility = Visibility.Visible;
            }
            else
            {
                AccountCreationLastNameLabel.Visibility = Visibility.Collapsed;
            }
        }

        private void AccountCreationUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            AccountCreationUsernameLabel.Visibility = Visibility.Collapsed;
        }

        private void AccountCreationUsername_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((sender as TextBox).Text == String.Empty)
            {
                AccountCreationUsernameLabel.Visibility = Visibility.Visible;
            }
            else
            {
                AccountCreationUsernameLabel.Visibility = Visibility.Collapsed;
            }
        }

        private void AccountCreationPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            AccountCreationPasswordLabel.Visibility = Visibility.Collapsed;
        }

        private void AccountCreationPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((sender as PasswordBox).Password == String.Empty)
            {
                AccountCreationPasswordLabel.Visibility = Visibility.Visible;
            }
            else
            {
                AccountCreationPasswordLabel.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        public void TeacherError(string msg)
        {
            TeacherErrorText.Text = msg;

            TeacherErrorAnimBegin.Storyboard.Seek(TimeSpan.Zero);
            TeacherErrorAnimBegin.Storyboard.Pause();

            BeginStoryboard(TeacherErrorAnimBegin.Storyboard);
        }

        public void CodeError(string msg)
        {
            CodeErrorText.Text = msg;

            CodeErrorAnimBegin.Storyboard.Seek(TimeSpan.Zero);
            CodeErrorAnimBegin.Storyboard.Pause();

            BeginStoryboard(CodeErrorAnimBegin.Storyboard);
        }
    }
}
