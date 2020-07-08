using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace BiblioBreeze
{
    public class DateAssignedConv : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime assignedOn = (DateTime)value;
            string stringForm = "Assigned on ";

            if (assignedOn != null)
            {
                switch (assignedOn.Month)
                {
                    default: stringForm += "Jan. "; break;
                    case 2: stringForm += "Feb. "; break;
                    case 3: stringForm += "Mar. "; break;
                    case 4: stringForm += "Apr. "; break;
                    case 5: stringForm += "May "; break;
                    case 6: stringForm += "Jun. "; break;
                    case 7: stringForm += "Jul. "; break;
                    case 8: stringForm += "Aug. "; break;
                    case 9: stringForm += "Sep. "; break;
                    case 10: stringForm += "Oct. "; break;
                    case 11: stringForm += "Nov. "; break;
                    case 12: stringForm += "Dec. "; break;
                }

                stringForm += assignedOn.Day + ", ";
                stringForm += assignedOn.Year;
            }

            return stringForm;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "";
        }
    }
}
