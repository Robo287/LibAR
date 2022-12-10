using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Paroxe.SuperCalendar.Internal
{
    public class CalendarCell : UIBehaviour
    {
        public Text m_DayNumber;

        public Sprite m_OtherSprite;
        public Sprite m_CurrentSprite;
        public Sprite m_SelectedSprite;

        public ICalendarCellPicker m_CalendarCellPicker;

        private State m_State;

        public enum State
        {
            Current,
            Other,
            Selected,
        }

        public void Select()
        {
            if (m_CalendarCellPicker == null)
                m_CalendarCellPicker = (ICalendarCellPicker) GetComponentInParent(typeof (ICalendarCellPicker));
            m_CalendarCellPicker.OnCellSelected(this);
        }

        public void SetState(State state)
        {
            m_State = state;

            switch (m_State)
            {
                case State.Current:
                    GetComponent<Image>().sprite = m_CurrentSprite;
                    m_DayNumber.color = Color.white;
                    break;
                case State.Other:
                    GetComponent<Image>().sprite = m_OtherSprite;
                    m_DayNumber.color = new Color(100.0f/255.0f, 100.0f/255.0f, 100.0f/255.0f, 1.0f);
                    break;
                case State.Selected:
                    GetComponent<Image>().sprite = m_SelectedSprite;
                    m_DayNumber.color = Color.white;
                    break;
            }
        }
    }
}