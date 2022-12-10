using System;
using System.Globalization;
using UnityEngine.EventSystems;

namespace Paroxe.SuperCalendar.Internal
{
    public class CalendarDayPicker : UIBehaviour, ICalendarCellPicker
    {
        public Calendar m_Calendar;
        private CalendarCell[] m_CalendarCells;

        protected override void OnEnable()
        {
            base.OnEnable();

            GetCells();
        }

        private void GetCells()
        {
            if (m_CalendarCells == null || m_CalendarCells.Length == 0)
                m_CalendarCells = GetComponentsInChildren<CalendarCell>();
        }

        public void OnCellSelected(CalendarCell cell)
        {
            int index = cell.transform.GetSiblingIndex();

            int firstDay = (int) new DateTime(m_Calendar.DateTime.Year, m_Calendar.DateTime.Month, 1).DayOfWeek;
            int numberOfDays = DateTime.DaysInMonth(m_Calendar.DateTime.Year, m_Calendar.DateTime.Month);
            int numberOfDaysLastMonth = 0;

            if (m_Calendar.DateTime.Month == 1)
                numberOfDaysLastMonth = DateTime.DaysInMonth(m_Calendar.DateTime.Year - 1, 12);
            else
                numberOfDaysLastMonth = DateTime.DaysInMonth(m_Calendar.DateTime.Year, m_Calendar.DateTime.Month - 1);

            if (index < firstDay)
            {
                m_Calendar.DateTime = new DateTime(
                    m_Calendar.DateTime.AddMonths(-1).Year,
                    m_Calendar.DateTime.AddMonths(-1).Month,
                    numberOfDaysLastMonth - firstDay + index + 1,
                    m_Calendar.DateTime.AddMonths(-1).Hour,
                    m_Calendar.DateTime.AddMonths(-1).Minute,
                    m_Calendar.DateTime.AddMonths(-1).Second,
                    m_Calendar.DateTime.AddMonths(-1).Millisecond,
                    m_Calendar.DateTime.AddMonths(-1).Kind);
            }
            else if (index > firstDay + numberOfDays - 1)
            {
                m_Calendar.DateTime = new DateTime(
                    m_Calendar.DateTime.AddMonths(1).Year,
                    m_Calendar.DateTime.AddMonths(1).Month,
                    index - numberOfDays - firstDay + 1,
                    m_Calendar.DateTime.AddMonths(1).Hour,
                    m_Calendar.DateTime.AddMonths(1).Minute,
                    m_Calendar.DateTime.AddMonths(1).Second,
                    m_Calendar.DateTime.AddMonths(1).Millisecond,
                    m_Calendar.DateTime.AddMonths(1).Kind);
            }
            else
            {
                m_Calendar.DateTime = new DateTime(
                    m_Calendar.DateTime.Year,
                    m_Calendar.DateTime.Month,
                    index - firstDay + 1,
                    m_Calendar.DateTime.Hour,
                    m_Calendar.DateTime.Minute,
                    m_Calendar.DateTime.Second,
                    m_Calendar.DateTime.Millisecond,
                    m_Calendar.DateTime.Kind);
            }

            m_Calendar.OnDaySelected();

            
        }

        public void SetupCellsLayout(DateTime dateTime)
        {
            if (m_CalendarCells == null)
                m_CalendarCells = GetComponentsInChildren<CalendarCell>();

            int firstDay = (int) new DateTime(dateTime.Year, dateTime.Month, 1).DayOfWeek;
            int numberOfDays = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);

            GetCells();

            if (firstDay != 0)
            {
                int numberOfDaysLastMonth = 0;

                if (dateTime.Month == 1)
                    numberOfDaysLastMonth = DateTime.DaysInMonth(dateTime.Year - 1, 12);
                else
                    numberOfDaysLastMonth = DateTime.DaysInMonth(dateTime.Year, dateTime.Month - 1);

                for (int i = 0; i < firstDay; ++i)
                {
                    m_CalendarCells[i].m_DayNumber.text = (numberOfDaysLastMonth - firstDay + i + 1).ToString();

                    m_CalendarCells[i].SetState(CalendarCell.State.Other);
                }
            }

            for (int i = 0; i < numberOfDays; ++i)
            {
                m_CalendarCells[i + firstDay].m_DayNumber.text = (i + 1).ToString();

                m_CalendarCells[i + firstDay].SetState(CalendarCell.State.Current);

                if (i + 1 == dateTime.Day)
                {
                    if (m_Calendar.SelectionState > Calendar.SelectionStateType.Month || m_Calendar.KeepSelected)
                        m_CalendarCells[i + firstDay].SetState(CalendarCell.State.Selected);
                }
                
            }

            for (int i = numberOfDays + firstDay; i < m_CalendarCells.Length; ++i)
            {
                m_CalendarCells[i].m_DayNumber.text = (i - numberOfDays - firstDay + 1).ToString();

                m_CalendarCells[i].SetState(CalendarCell.State.Other);
            }
        }

        public void SetupCellsCulture(CultureInfo culture)
        {

        }
    }
}