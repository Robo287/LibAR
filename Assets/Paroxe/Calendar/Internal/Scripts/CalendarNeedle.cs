using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Paroxe.SuperCalendar.Internal
{
    public class CalendarNeedle : UIBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public enum NeedleType
        {
            Second,
            Minute,
            Hour
        }

        public NeedleType m_NeedleType;
        private Vector2 m_BeginPosition;
        private Calendar m_Calendar;
        private bool m_Registered;
        private Quaternion m_RotationToGo = Quaternion.identity;
        private bool m_SuspendUpdate;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (m_Calendar == null)
                m_Calendar = GetComponentInParent<Calendar>();

            if (!m_Registered)
            {
                m_Calendar.DateTimeChanged += OnDateTimeChanged;

                m_Registered = true;

                OnDateTimeChanged(m_Calendar.DateTime);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (m_Registered)
            {
                m_Calendar.DateTimeChanged -= OnDateTimeChanged;

                m_Registered = false;
            }
        }

        private void OnDateTimeChanged(DateTime datetime)
        {
            if (m_NeedleType == NeedleType.Minute)
            {
                m_RotationToGo = Quaternion.Euler(0.0f, 0.0f, -datetime.Minute * 6.0f - (datetime.Second / 60.0f) * 6.0f);
            }
            else if (m_NeedleType == NeedleType.Second)
            {
                m_RotationToGo = Quaternion.Euler(0.0f, 0.0f, -datetime.Second * 6.0f);
            }
            else if (m_NeedleType == NeedleType.Hour)
            {
                m_RotationToGo = Quaternion.Euler(0.0f, 0.0f, -datetime.Hour * 6.0f * 5.0f - (datetime.Minute / 60.0f) * 30.0f);
            }
        }

        void Update()
        {
            if (m_SuspendUpdate)
                return;

            transform.localRotation = Quaternion.Lerp(transform.localRotation, m_RotationToGo, Time.deltaTime * 5.0f);
        }

        public void OnDrag(PointerEventData eventData)
        {
            m_BeginPosition += eventData.delta;

            Vector3 localPointerPosition;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(transform as RectTransform, m_BeginPosition,
                eventData.pressEventCamera, out localPointerPosition))
            {
                Vector3 pos = (transform.position - localPointerPosition).normalized;

                float angle = Mathf.Atan2(pos.y, pos.x)*Mathf.Rad2Deg;
                if (angle < 0)
                    angle = 360 + angle;
                angle = 360 - angle;

                Quaternion prevRotation = transform.localRotation;
                transform.localRotation = Quaternion.Euler(0.0f, 0.0f, -angle + 90.0f);

                if (m_NeedleType == NeedleType.Minute)
                {
                    int prevMinute = (60 - ((int)(prevRotation.eulerAngles.z)) / 6) % 60;
                    int minute = (60 - ((int) (transform.localRotation.eulerAngles.z))/6)%60;
                    int relHour = 0;

                    if (prevMinute < 60 && prevMinute > 45 && minute < prevMinute && minute < 15)
                        relHour = 1;
                    else if (minute < 60 && minute > 45 && prevMinute < minute && prevMinute < 15)
                        relHour = -1;

                    m_Calendar.DateTime = new DateTime(
                        m_Calendar.DateTime.AddHours(relHour).Year,
                        m_Calendar.DateTime.AddHours(relHour).Month,
                        m_Calendar.DateTime.AddHours(relHour).Day,
                        m_Calendar.DateTime.AddHours(relHour).Hour,
                        minute,
                        m_Calendar.DateTime.AddHours(relHour).Second,
                        m_Calendar.DateTime.AddHours(relHour).Millisecond,
                        m_Calendar.DateTime.AddHours(relHour).Kind);
                }
                else if (m_NeedleType == NeedleType.Second)
                {
                    int prevSecond = (60 - ((int)(prevRotation.eulerAngles.z)) / 6) % 60;
                    int second = (60 - ((int)(transform.localRotation.eulerAngles.z)) / 6) % 60;
                    int relMinute = 0;

                    if (prevSecond < 60 && prevSecond > 45 && second < prevSecond && second < 15)
                        relMinute = 1;
                    else if (second < 60 && second > 45 && prevSecond < second && prevSecond < 15)
                        relMinute = -1;

                    m_Calendar.DateTime = new DateTime(
                        m_Calendar.DateTime.AddMinutes(relMinute).Year,
                        m_Calendar.DateTime.AddMinutes(relMinute).Month,
                        m_Calendar.DateTime.AddMinutes(relMinute).Day,
                        m_Calendar.DateTime.AddMinutes(relMinute).Hour,
                        m_Calendar.DateTime.AddMinutes(relMinute).Minute,
                        second,
                        m_Calendar.DateTime.AddMinutes(relMinute).Millisecond,
                        m_Calendar.DateTime.AddMinutes(relMinute).Kind);
                }
                else if (m_NeedleType == NeedleType.Hour)
                {
                    int prevDateTimeHour = m_Calendar.DateTime.Hour;
                    int prevHour = (12 - ((int)(prevRotation.eulerAngles.z)) / 30) % 12;
                    int hour = (12 - ((int)(transform.localRotation.eulerAngles.z)) / 30) % 12;

                    int rel = 0;

                    if (prevHour < 12 && prevHour > 9 && hour < prevHour && hour < 3)
                        hour += 12;
                    else if (hour < 12 && hour > 9 && prevHour < hour && prevHour < 3)
                        hour -= 12;

                    if (prevDateTimeHour != prevHour)
                    {
                        prevHour += 12;
                        hour += 12;
                    }

                    rel = hour - prevHour;


                    m_Calendar.DateTime = new DateTime(
                        m_Calendar.DateTime.AddHours(rel).Year,
                        m_Calendar.DateTime.AddHours(rel).Month,
                        m_Calendar.DateTime.AddHours(rel).Day,
                        m_Calendar.DateTime.AddHours(rel).Hour,
                        m_Calendar.DateTime.AddHours(rel).Minute,
                        m_Calendar.DateTime.AddHours(rel).Second,
                        m_Calendar.DateTime.AddHours(rel).Millisecond,
                        m_Calendar.DateTime.AddHours(rel).Kind);
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            m_BeginPosition = eventData.pressPosition;

            m_SuspendUpdate = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            m_SuspendUpdate = false;
        }
    }
}