using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace BiblioBreeze
{
    public class StudentCode : INotifyPropertyChanged
    {
        public string studentName { get; set; }
        public string bookCode { get; set; }
        public DateTime assigned { get; set; }
        public string graduationYear { get; set; }

        public string totalPercentRead
        {
            get
            {
                var allPercentRead = readingData.Where(x => !x.excludedFromTotal).Select(x => x.percentRead);

                if (allPercentRead.Count() != 0)
                {
                    float percent = allPercentRead.Average();
                    return ((int)(percent * 100)).ToString() + "%";
                }
                else
                {
                    return "0%";
                }
            }
        }

        public string totalTimeRead
        {
            get
            {
                double totalSecs = readingData.Where(x => !x.excludedFromTotal).Select(x => x.timeSpent.TotalSeconds).Sum();
                var totalTimeSpan = TimeSpan.FromSeconds(totalSecs);

                if (totalTimeSpan.Hours == 0)
                {
                    return totalTimeSpan.Minutes + "m " + totalTimeSpan.Seconds + "s";
                }
                else
                {
                    return totalTimeSpan.Hours + "h " + totalTimeSpan.Minutes + "m";
                }
            }
        }

        //Stores a tuple in the format <Percent scrolled at end, Time began, Time spent>
        public ObservableCollection<ReadingData> readingData { get; set; }
        public Book bookSource;

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
