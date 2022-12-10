using System;
using System.Globalization;

namespace Paroxe.SuperCalendar.Internal
{
    public interface ICalendarCellPicker
    {
        void OnCellSelected(CalendarCell cell);
        void SetupCellsLayout(DateTime dateTime);
        void SetupCellsCulture(CultureInfo culture);
    }
}