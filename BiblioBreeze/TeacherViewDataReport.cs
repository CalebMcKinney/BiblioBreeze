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
        #region Data Row Animation
        private void MouseOverDataRow(object sender, RoutedEventArgs e)
        {
            Grid dataRowGrid = sender as Grid;

            dataRowGrid.Children[1].Visibility = Visibility.Collapsed;
            dataRowGrid.Children[2].Visibility = Visibility.Collapsed;

            dataRowGrid.Children[3].Visibility = Visibility.Visible;
        }

        private void MouseLeftDataRow(object sender, RoutedEventArgs e)
        {
            Grid dataRowGrid = sender as Grid;

            dataRowGrid.Children[1].Visibility = Visibility.Visible;
            dataRowGrid.Children[2].Visibility = Visibility.Visible;

            dataRowGrid.Children[3].Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Chapter Data Expansion
        int activeStudentIndex = -1;

        ListBoxItem itemHovering;
        ListBox dataListBox;
        StudentCode studentHovering;

        TextBlock[] dataTextBlocks;

        private void MouseOverChapterData(object sender, RoutedEventArgs e)
        {
            Grid dataRowGrid = sender as Grid;

            dataTextBlocks = new TextBlock[]
            {
                dataRowGrid.Children[0] as TextBlock,
                dataRowGrid.Children[2] as TextBlock
            };

            dataListBox = MiscResources.Utility.FindParent<ListBox>(dataRowGrid);
            itemHovering = MiscResources.Utility.FindParent<ListBoxItem>(dataRowGrid);

            activeStudentIndex = dataListBox.Items.IndexOf(itemHovering.DataContext);

            var studentListBoxItem = MiscResources.Utility.FindParent<ListBoxItem>(dataListBox);
            studentHovering = studentListBoxItem.DataContext as StudentCode;

            dataRowGrid.Children[3].Visibility = Visibility.Visible;
        }

        private void MouseLeftChapterData(object sender, RoutedEventArgs e)
        {
            (sender as Grid).Children[3].Visibility = Visibility.Collapsed;
        }

        private void OpenStudentDataSettings(object sender, RoutedEventArgs e)
        {
            Grid parentGrid = MiscResources.Utility.FindParent<Grid>(sender as Button);
            var popup = parentGrid.Children[1] as System.Windows.Controls.Primitives.Popup;
            popup.IsOpen = true;
        }

        private void DataReportExcludeRow(object sender, RoutedEventArgs e)
        {
            bool prevExcluded = (itemHovering.DataContext as ReadingData).excludedFromTotal;

            (sender as Button).Content = prevExcluded ? "Exclude from Total" : "Include in Total";
            (itemHovering.DataContext as ReadingData).excludedFromTotal = !prevExcluded;

            dataTextBlocks[0].TextDecorations = !prevExcluded ? TextDecorations.Strikethrough : null;
            dataTextBlocks[1].TextDecorations = !prevExcluded ? TextDecorations.Strikethrough : null;

            studentHovering.NotifyPropertyChanged("totalTimeRead");
            studentHovering.NotifyPropertyChanged("totalPercentRead");
        }

        private void DataReportClearChData(object sender, RoutedEventArgs e)
        {
            if (itemHovering != null && activeStudentIndex != -1)
            {
                (itemHovering.DataContext as ReadingData).percentRead = 0;
                (itemHovering.DataContext as ReadingData).timeSpent = new TimeSpan();

                (itemHovering.DataContext as ReadingData).NotifyPropertyChanged("timeSpentFormatted");
                (itemHovering.DataContext as ReadingData).NotifyPropertyChanged("percentReadFormatted");

                studentHovering.NotifyPropertyChanged("totalTimeRead");
                studentHovering.NotifyPropertyChanged("totalPercentRead");

                NotifySuccess("Data successfully cleared");
            }
        }
        #endregion

        #region Reading Data Misc.
        private void GenerateDataReport(object sender, RoutedEventArgs e)
        {
            DataReportMenu.Visibility = Visibility.Visible;
            BookExpandedMenu.Visibility = Visibility.Collapsed;

            for (int studentIndex = 0; studentIndex < selectedBook.studentsAssigned.Count; studentIndex++)
            {
                string curStudentCode = selectedBook.studentsAssigned[studentIndex].bookCode;
                var readingDataRows = Database.db.FindRowsByColVal(1, curStudentCode, Database.Source.ReadingData);

                selectedBook.studentsAssigned[studentIndex].readingData = new ObservableCollection<ReadingData>();
                var allReadingData = readingDataRows.Select(x => Database.db.ReadRow(x, Database.Source.ReadingData));

                foreach(List<string> curData in allReadingData)
                {
                    selectedBook.studentsAssigned[studentIndex].readingData.Add(
                        new ReadingData(
                            float.Parse(curData[1]), 
                            TimeSpan.FromSeconds(Convert.ToDouble(curData[2])), 
                            curData[4]));
                }
            }
            
            DataReportSubheading.Text = "from '" + selectedBook.bookName + "'";
            DataReportStudentList.ItemsSource = selectedBook.studentsAssigned;
        }

        private void CloseDataReport(object sender, RoutedEventArgs e)
        {
            DataReportMenu.Visibility = Visibility.Collapsed;
            BookExpandedMenu.Visibility = Visibility.Visible;
        }
        #endregion
    }
}
