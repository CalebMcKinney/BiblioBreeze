using Google.Apis.Download;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace BiblioBreeze
{
    public partial class StudentView : Window
    {
        public static StudentCode activeStudent;

        private string _tempPath;
        private string baseBookDir;
        private List<string> bookLocations;
        private int _currentPage;

        private string tocPath;
        List<string> chTitles;

        List<XElement> allChapterNavPoints;

        private string _downloadLocation;

        public StudentView()
        {
            InitializeComponent();

            AccountName.Text = activeStudent.studentName;
            BookTitleText.Text = activeStudent.bookSource.bookName.ToUpper();

            Teacher teacher = activeStudent.bookSource.assignedBy;
            TeacherNameText.Text = String.Format("Assigned by {0}. {1}", teacher.title, teacher.lastName);

            if (!Directory.Exists("Library"))
            {
                Directory.CreateDirectory("Library");
            }

            _downloadLocation = Path.Combine("Library", activeStudent.bookSource.fileLocation + ".epub");

            if (!File.Exists(_downloadLocation))
            {
                Action download = async () =>
                {
                    await Database.db.DownloadFromDriveAsync(activeStudent.bookSource.fileLocation,
                    (IDownloadProgress progress) =>
                    {
                        Action action = () =>
                        {
                            if (progress.Status == DownloadStatus.Completed)
                            {
                                InitializeBook();
                            }
                        };

                        Dispatcher.BeginInvoke(action);
                    });
                };

                Dispatcher.BeginInvoke(download);
            }
            else
            {
                InitializeBook();
            }
        }

        public void InitializeBook()
        {
            DownloadingBook.Visibility = Visibility.Collapsed;
            EpubDisplayContainer.Visibility = Visibility.Visible;

            OpenBook(_downloadLocation);

            #region Initialize Active Student's Reading Data
            List<int> readingDataRows = Database.db.FindRowsByColVal(1, activeStudent.bookSource.bookID, Database.Source.ReadingData);

            activeStudent.readingData = new ObservableCollection<ReadingData>();

            foreach (int i in readingDataRows)
            {
                List<string> curRow = Database.db.ReadRow(i, Database.Source.ReadingData);

                activeStudent.readingData.Add(new ReadingData(
                    float.Parse(curRow[1]),
                    TimeSpan.FromSeconds(Convert.ToDouble(curRow[2])),
                    chTitles[Convert.ToUInt16(curRow[3])]));
            }

            //fill in the rest of the reading data
            for (int i = readingDataRows.Count; i < bookLocations.Count; i++)
            {
                activeStudent.readingData.Add(new ReadingData(
                    0f,
                    TimeSpan.Zero,
                    chTitles[_currentPage]));
            }

            activeStudent.bookSource.chapterTitles = chTitles;
            #endregion
        }

        List<string> GetAllBookPages(XDocument tableOfContents)
        {
            var chapterSources = allChapterNavPoints.Select(x => x.Descendants(tableOfContents.Root.GetDefaultNamespace() + "content").First()).Select(x => x.Attribute("src").Value);
            return chapterSources.ToList();
        }

        List<string> GetChapterTitles(XDocument tableOfContents)
        {
            var chapterTitles = allChapterNavPoints.Select(x => x.Descendants(tableOfContents.Root.GetDefaultNamespace() + "navLabel").First().Value);

            return chapterTitles.ToList();
        }

        #region EPUB Read
        private void OpenBook(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            if (!Directory.Exists("Library"))
            {
                Directory.CreateDirectory("Library");
            }

            //In the future, add smarter loading to avoid unneccessary overwrites
            File.Copy(filePath, Path.Combine("Library", fileName + ".zip"), true);
            _tempPath = Path.Combine("Library", fileName);

            if (Directory.Exists(_tempPath))
            {
                FileUtility.DeleteDirectory(_tempPath);
            }

            FileUtility.UnZIPFiles(Path.Combine("Library", fileName + ".zip"), Path.Combine("Library", fileName));

            //XDocument opens the container.xml file 
            var containerReader = XDocument.Load(ConvertToMemoryStream(Path.Combine("Library", fileName, "META-INF", "container.xml")));

            //Find the file containing the metadata of the ebook and store the directory its located in for use in finding the next page of the book
            var baseMenuXmlPath = containerReader.Root.Descendants(containerReader.Root.GetDefaultNamespace() + "rootfile").First().Attribute("full-path").Value;
            baseBookDir = Path.GetDirectoryName(baseMenuXmlPath);

            //Open the OPF file with the metadata via the XML with LINQ class
            XDocument menuReader = XDocument.Load(Path.Combine(_tempPath, baseMenuXmlPath));

            #region Find Table of Contents Path
            string tocID = menuReader.Root.Element(menuReader.Root.GetDefaultNamespace() + "spine").Attribute("toc").Value;
            string tocHref = menuReader.Root.Element(menuReader.Root.GetDefaultNamespace() + "manifest").Descendants().Where(x => x.Attribute("id").Value == tocID).First().Attribute("href").Value;

            tocPath = Path.GetFullPath(Path.Combine(_tempPath, baseBookDir, tocHref));
            #endregion

            XDocument tableOfContents = XDocument.Load(tocPath);
            allChapterNavPoints = tableOfContents.Root.Descendants(tableOfContents.Root.GetDefaultNamespace() + "navPoint").ToList();

            //Make sure there exists only one element for each page to avoid redundant pages
            allChapterNavPoints = 
                allChapterNavPoints.GroupBy(p => 
                {
                    string x = p.Descendants(tableOfContents.Root.GetDefaultNamespace() + "content").First().Attribute("src").Value;

                    if (x.Contains('#'))
                        return x.Substring(0, x.LastIndexOf("#"));
                    else
                        return x;
                })
                .Select(g => g.First()).ToList();

            bookLocations = GetAllBookPages(tableOfContents);
            chTitles = GetChapterTitles(tableOfContents);

            _currentPage = 0;
            string uri = GetPath(_currentPage);

            epubDisplay.Navigate(uri);
            NextButton.IsEnabled = bookLocations.Count > 1;
        }
        
        public string GetPath(int index)
        {
            return String.Format("file:///{0}", Path.GetFullPath(Path.Combine(_tempPath, baseBookDir, bookLocations[index])));
        }

        public MemoryStream ConvertToMemoryStream(string filePath)
        {
            var xml = File.ReadAllText(filePath);
            byte[] encodedString = Encoding.UTF8.GetBytes(xml);

            // Put the byte array into a stream and rewind it to the beginning
            MemoryStream ms = new MemoryStream(encodedString);
            ms.Flush();
            ms.Position = 0;

            return ms;
        }
        #endregion

        #region EPUB Control
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            LeavePage(true);
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            LeavePage(false);
        }
        #endregion

        #region Student Data  
        DateTime pageOpened;

        private void LeavePage(bool forwards)
        {
            SavePageLocation();

            //Handle navigation
            if (forwards)
            {
                _currentPage++;

                if (_currentPage == bookLocations.Count - 1)
                {
                    NextButton.IsEnabled = false;
                }

                PreviousButton.IsEnabled = bookLocations.Count > 1;

                string uri = GetPath(_currentPage);
                epubDisplay.Navigate(uri);
            }
            else
            {
                _currentPage--;

                if (_currentPage < 1)
                {
                    PreviousButton.IsEnabled = false;
                }

                NextButton.IsEnabled = bookLocations.Count > 1;

                string uri = GetPath(_currentPage);
                epubDisplay.Navigate(uri);
            }
        }

        private void SavePageLocation()
        {
            float distScrolled = (float)(epubDisplay.Document.Body.ScrollTop);
            float totalDist = (float)(epubDisplay.Document.Body.ScrollRectangle.Height);

            float currentScrollPercent = Math.Min(1, (distScrolled + epubDisplay.Height) / totalDist);

            TimeSpan prevTotalTimeSpent = new TimeSpan();

            prevTotalTimeSpent = activeStudent.readingData[_currentPage].timeSpent;
            
            activeStudent.readingData[_currentPage].percentRead = currentScrollPercent;
            activeStudent.readingData[_currentPage].timeSpent = (DateTime.Now - pageOpened) + prevTotalTimeSpent;

            List<object> newReadingDataValues = new List<object>()
            {
                activeStudent.bookCode,
                currentScrollPercent.ToString(),
                Convert.ToInt64(((DateTime.Now - pageOpened) + prevTotalTimeSpent).TotalSeconds).ToString(),
                _currentPage.ToString(),
                chTitles[_currentPage]
            };
            
            var pageData = Database.db.FindRowsByColVal(4, _currentPage.ToString(), Database.Source.ReadingData);

            foreach (int dataRows in Database.db.FindRowsByColVal(1, activeStudent.bookCode, Database.Source.ReadingData))
            {
                if (pageData.Contains(dataRows)) // If the current student has reading data for their current page
                {
                    Database.db.WriteRow(dataRows, newReadingDataValues, Database.Source.ReadingData);
                    return;
                }
            }

            //If no reading data with the page was found, then create a new row
            Database.db.AppendRow(newReadingDataValues, Database.Source.ReadingData);
        }

        private void LoadPageLocation(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            float previousPercent = 0;

            //If there is enough pages for it to not cause an error, set previous percent
            if (activeStudent.readingData.Count() >= _currentPage) 
                previousPercent = activeStudent.readingData[_currentPage].percentRead;
            
            int scrollLocation = Math.Max(0, (int)(previousPercent * epubDisplay.Document.Body.ScrollRectangle.Height) - epubDisplay.Height);
            epubDisplay.Document.Window.ScrollTo(new System.Drawing.Point(0, scrollLocation));

            pageOpened = DateTime.Now;
        }

        #endregion

        #region Student Account
        private void Logout(object sender, RoutedEventArgs e)
        {
            (new MainWindow()).Show();
            this.Close();
        }
        #endregion
    }
}
