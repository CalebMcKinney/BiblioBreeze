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
        Book selectedBook;
        int selectedBookIndex;

        public static Teacher activeTeacher;

        public TeacherView()
        {
            InitializeComponent();

            TeacherNameText.Text = String.Format("{0}. {1}", activeTeacher.title.ToString(), activeTeacher.lastName);
            
            #region Reupload Button
            var hyperlinkAction = new Hyperlink(new Run("Choose another file"));
            hyperlinkAction.Click += (s, e) => { ChooseAnotherFile(); };
            BookFileNameRetryText.Inlines.Add(hyperlinkAction);
            #endregion

            #region View Popular Searches -- No Results Found
            hyperlinkAction = new Hyperlink(new Run("view popular searches."));
            hyperlinkAction.Click += (s, e) => { ViewPopularSearches(); };
            NoResultsViewPopularTxt.Inlines.Add(hyperlinkAction);
            #endregion

            BookContainer.ItemsSource = activeTeacher.booksAssigned;

            if (activeTeacher.booksAssigned.Count == 0)
            {
                BookDisplayGrid.Visibility = Visibility.Collapsed;
                BooksNotFoundMsg.Visibility = Visibility.Visible;
            }
            else
            {
                BookDisplayGrid.Visibility = Visibility.Visible;
                BooksNotFoundMsg.Visibility = Visibility.Collapsed;
            }
        }

        private void ExpandBook(object sender, SelectionChangedEventArgs e)
        {
            if (BookContainer.SelectedIndex != -1)
            {
                selectedBook = BookContainer.SelectedValue as Book;

                CurrentExpandedBook.Text = selectedBook.bookName;
                selectedBookIndex = BookContainer.SelectedIndex;

                BookContainer.SelectedIndex = -1;
                ScreenDarken.Visibility = Visibility.Visible;
                BookExpandedMenu.Visibility = Visibility.Visible;

                StudentsInfoList.ItemsSource = selectedBook.studentsAssigned;

                if (selectedBook.studentsAssigned.Count == 0)
                {
                    StudentsInfoList.Visibility = Visibility.Collapsed;
                    StudentsNotFound.Visibility = Visibility.Visible;
                }
                else
                {
                    StudentsInfoList.Visibility = Visibility.Visible;
                    StudentsNotFound.Visibility = Visibility.Collapsed;
                }
                     
            }
        }

        private void CloseExpandedBook(object sender, RoutedEventArgs e)
        {
            ScreenDarken.Visibility = Visibility.Collapsed;
            BookExpandedMenu.Visibility = Visibility.Collapsed;
        }

        #region Student Menu Animation
        private void MouseOverStudent(object sender, RoutedEventArgs e)
        {
            Grid studentViews = sender as Grid;

            studentViews.Children[1].Visibility = Visibility.Collapsed;
            studentViews.Children[2].Visibility = Visibility.Visible;
        }

        private void MouseLeftStudent(object sender, RoutedEventArgs e)
        {
            Grid studentViews = sender as Grid;

            studentViews.Children[1].Visibility = Visibility.Visible;
            studentViews.Children[2].Visibility = Visibility.Collapsed;
        }
        #endregion
        
        #region Notification
        public void NotifySuccess(string msg)
        {
            SuccessText.Text = msg;

            ErrorAnimBegin.Storyboard.Seek(TimeSpan.Zero);
            ErrorAnimBegin.Storyboard.Pause();

            BeginStoryboard(SuccessAnimBegin.Storyboard);
        }

        public void NotifyError(string msg)
        {
            ErrorText.Text = msg;

            SuccessAnimBegin.Storyboard.Seek(TimeSpan.Zero);
            SuccessAnimBegin.Storyboard.Pause();

            BeginStoryboard(ErrorAnimBegin.Storyboard);
        }
        #endregion

        #region Account Menu
        private void OpenAccountSettingsPopup(object sender, RoutedEventArgs e)
        {
            AccountSettingsPopup.IsOpen = true;
        }

        private void Logout(object sender, RoutedEventArgs e)
        {
            (new MainWindow()).Show();
            this.Close();
        }
        #endregion
    }
}
