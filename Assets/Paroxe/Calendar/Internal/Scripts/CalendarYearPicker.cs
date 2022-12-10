using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Paroxe.SuperCalendar.Internal
{
    public class CalendarYearPicker : UIBehaviour, ICalendarCellPicker
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
            m_Calendar.DateTime = new DateTime(
                int.Parse(cell.m_DayNumber.text),
                m_Calendar.DateTime.Month,
                Mathf.Clamp(m_Calendar.DateTime.Day, 1, DateTime.DaysInMonth(int.Parse(cell.m_DayNumber.text), m_Calendar.DateTime.Month)),
                m_Calendar.DateTime.Hour,
                m_Calendar.DateTime.Minute,
                m_Calendar.DateTime.Second,
                m_Calendar.DateTime.Millisecond,
                m_Calendar.DateTime.Kind);

            m_Calendar.OnYearSelected();
        }

        public void SetupCellsLayout(DateTime dateTime)
        {
            GetCells();

            int firstYear = m_Calendar.YearsRangeBegin;

            for (int i = 0; i < m_CalendarCells.Length; ++i)
            {
                m_CalendarCells[i].m_DayNumber.text = (firstYear + i).ToString();
            }

            for (int i = 0; i < m_CalendarCells.Length; ++i)
            {
                if ((m_Calendar.SelectionState > Calendar.SelectionStateType.None || m_Calendar.KeepSelected) &&
                    m_CalendarCells[i].m_DayNumber.text == dateTime.Year.ToString())
                    m_CalendarCells[i].SetState(CalendarCell.State.Selected);
                else
                    m_CalendarCells[i].SetState(CalendarCell.State.Current);
            }
        }

        public void SetupCellsCulture(CultureInfo culture)
        {

        }
    }
}