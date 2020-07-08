using System;
using System.ComponentModel;

namespace BiblioBreeze
{
    public class ReadingData : INotifyPropertyChanged
    {
        public string chapterTitle { get; set; }

        public float percentRead { get; set; }
        public TimeSpan timeSpent { get; set; }

        public bool excludedFromTotal = false;

        public string timeSpentFormatted
        {
            get
            {
                if (timeSpent.Hours == 0)
                {
                    return timeSpent.Minutes + "m " + timeSpent.Seconds + "s";
                }
                else
                {
                    return timeSpent.Hours + "h " + timeSpent.Minutes + "m";
                }
            }
        }

        public string percentReadFormatted
        {
            get
            {
                return ((int)(percentRead * 100)).ToString() + "%";
            }
        }

        public ReadingData(float percentRead, TimeSpan timeSpent, string chapterTitle)
        {
            this.percentRead = percentRead;
            this.timeSpent = timeSpent;
            this.chapterTitle = chapterTitle;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
