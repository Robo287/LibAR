using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Paroxe.SuperCalendar.Internal
{
    public class CalendarMonthPicker : UIBehaviour, ICalendarCellPicker
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
                m_Calendar.DateTime.Year,
                cell.transform.GetSiblingIndex() + 1,
                Mathf.Clamp(m_Calendar.DateTime.Day, 1, DateTime.DaysInMonth(m_Calendar.DateTime.Year, cell.transform.GetSiblingIndex() + 1)),
                m_Calendar.DateTime.Hour,
                m_Calendar.DateTime.Minute,
                m_Calendar.DateTime.Second,
                m_Calendar.DateTime.Millisecond,
                m_Calendar.DateTime.Kind);

            m_Calendar.OnMonthSelected();
        }

        public void SetupCellsLayout(DateTime dateTime)
        {
            GetCells();

            for (int i = 0; i < m_CalendarCells.Length; ++i)
            {
                if ((m_Calendar.SelectionState > Calendar.SelectionStateType.Year || m_Calendar.KeepSelected)
                    && m_CalendarCells[i].transform.GetSiblingIndex() + 1 == dateTime.Month)
                    m_CalendarCells[i].SetState(CalendarCell.State.Selected);
                else
                    m_CalendarCells[i].SetState(CalendarCell.State.Current);
            }
        }

        public void SetupCellsCulture(CultureInfo culture)
        {
            GetCells();

            for (int i = 0; i < m_CalendarCells.Length; ++i)
            {
                m_CalendarCells[i].m_DayNumber.text = culture.DateTimeFormat.GetAbbreviatedMonthName(i + 1);
            }
        }
    }
}