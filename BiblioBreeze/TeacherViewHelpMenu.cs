using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BiblioBreeze
{
    public partial class TeacherView : Window
    {
        List<Question> queryResults;
        Question expandedQuestion;

        private void ExpandHelp(object sender, RoutedEventArgs e)
        {
            AccountSettingsPopup.IsOpen = false;

            ScreenDarken.Visibility = Visibility.Visible;
            HelpMenu.Visibility = Visibility.Visible;
        }

        private void SearchHelp(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Tag != null)
            {
                //Later, sort query results according to number of keywords
                #region Potential Solution
                //List<List<string>> questionKeywords = 
                //    questionParts
                //        .Select(k => Question.allKeywords
                //            .Where(q => questionParts.Contains(q))
                //        .ToList())
                //        .Where(x => x.Count > 0)
                //    .ToList();
                #endregion

                List<string> questionParts = MainSearchBar.Text.ToLower().Split(' ').ToList();
                queryResults = Question.allKeywords.Where(k => questionParts.Contains(k.word)).Select(q => q.query).ToList();

                SearchResults.ItemsSource = queryResults;

                PrevSearchBar.Text = MainSearchBar.Text;

                SearchHomeMenu.Visibility = Visibility.Collapsed;
                SearchResultsMenu.Visibility = Visibility.Visible;
            }
            else
            {
                List<string> questionParts = PrevSearchBar.Text.ToLower().Split(' ').ToList();
                queryResults = Question.allKeywords.Where(k => questionParts.Contains(k.word)).Select(q => q.query).ToList();

                SearchResults.ItemsSource = queryResults;
            }

            if (queryResults.Count() == 0)
            {
                SearchResults.Visibility = Visibility.Collapsed;
                SearchResultsNotFound.Visibility = Visibility.Visible;
            }
            else
            {
                SearchResults.Visibility = Visibility.Visible;
                SearchResultsNotFound.Visibility = Visibility.Collapsed;
            }

            SearchTitleBar.Visibility = Visibility.Visible;
            PopularSearchesTitle.Visibility = Visibility.Collapsed;
        }

        private void ExpandSearchResult(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResults.SelectedIndex == -1)
                return;

            SearchResultExpanded.Visibility = Visibility.Visible;

            expandedQuestion = SearchResults.Items[SearchResults.SelectedIndex] as Question;
            SearchResults.SelectedIndex = -1;

            ExpandedResultTitle.Text = expandedQuestion.question;
            ExpandedResultContents.Text = expandedQuestion.answer;
        }

        private void PreviousSearch(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Tag != null)
            {
                if ((sender as Button).Tag.ToString() == "1")
                {
                    SearchResultsMenu.Visibility = Visibility.Collapsed;
                    SearchHomeMenu.Visibility = Visibility.Visible;

                    PrevSearchBar.Clear();
                    MainSearchBar.Clear();
                }

                if ((sender as Button).Tag.ToString() == "2")
                {
                    SearchResultExpanded.Visibility = Visibility.Collapsed;
                    SearchResultsMenu.Visibility = Visibility.Visible;
                }
            }
        }

        private void ViewPopularSearches(object sender, RoutedEventArgs e)
        {
            ViewPopularSearches();
        }

        private void ViewPopularSearches()
        {
            SearchHomeMenu.Visibility = Visibility.Collapsed;
            SearchResultsMenu.Visibility = Visibility.Visible;

            SearchResults.Visibility = Visibility.Visible;
            SearchResultsNotFound.Visibility = Visibility.Collapsed;

            PopularSearchesTitle.Visibility = Visibility.Visible;
            SearchTitleBar.Visibility = Visibility.Collapsed;

            SearchResults.ItemsSource = Question.allQuestions;
        }

        private void CloseHelpMenu(object sender, RoutedEventArgs e)
        {
            MainSearchBar.Clear();

            //Close whole menu
            HelpMenu.Visibility = Visibility.Collapsed;
            ScreenDarken.Visibility = Visibility.Collapsed;

            //Close individual tabs
            SearchResultsMenu.Visibility = Visibility.Collapsed;
            SearchResultExpanded.Visibility = Visibility.Collapsed;

            //Open homepage of help search 
            SearchHomeMenu.Visibility = Visibility.Visible;
        }
    }
}
