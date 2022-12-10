using System;
using Paroxe.SuperCalendar;
using UnityEngine;

public class CalendarEventExample : MonoBehaviour
{
#pragma warning disable 649
	[SerializeField]
	private Calendar m_Calendar;
#pragma warning restore 649

	public void OnCalendarDateTimeChanged()
    {
        Debug.Log(m_Calendar.DateTime);

        m_Calendar.DateTime = new DateTime(2020, 5, 2);
    }
}
