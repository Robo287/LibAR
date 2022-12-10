using UnityEngine;
using UnityEngine.EventSystems;

namespace Paroxe.SuperCalendar.Internal
{
    [ExecuteInEditMode]
    public class CalendarBottomTransform : UIBehaviour
    {
        Calendar m_Calendar;

        protected override void OnEnable()
        {
            m_Calendar = GetComponentInParent<Calendar>();

            if (m_Calendar.m_Panels[1].GetContentLayoutGroup() != null)
                m_Calendar.m_Panels[1].GetContentLayoutGroup().OnRectTransformChanged += OnCalendarLayoutRectTransformChanged;
        }

        protected override void OnDisable()
        {
            if (m_Calendar.m_Panels[1].GetContentLayoutGroup() != null)
                m_Calendar.m_Panels[1].GetContentLayoutGroup().OnRectTransformChanged -= OnCalendarLayoutRectTransformChanged;
        }

        private void OnCalendarLayoutRectTransformChanged(object sender)
        {
            if (m_Calendar.m_Panels[1].GetContentLayoutGroup() == null)
                return;

            RectTransform rt = transform as RectTransform;
            rt.sizeDelta = new Vector2(rt.sizeDelta.x,
                m_Calendar.m_Panels[1].GetContentLayoutGroup().ContentDimension.y
                + m_Calendar.m_Panels[1].m_TopPadding - 3.0f);
        }
    }
}