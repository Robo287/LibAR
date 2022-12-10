using System;
using System.Globalization;
using UnityEngine;

namespace Paroxe.SuperCalendar
{
    [Serializable]
    public class SerializableDateTime : ISerializationCallbackReceiver
    {
        [NonSerialized]
        private DateTime m_DateTime = DateTime.MinValue;
        [SerializeField]
        private string m_ProxyDate;

        public DateTime DateTime
        {
            get { return m_DateTime; }
            set { m_DateTime = value; }
        }

        public SerializableDateTime(DateTime dateTime)
        {
            DateTime = dateTime;
        }

        public void OnBeforeSerialize()
        {
            m_ProxyDate = m_DateTime.ToString(new CultureInfo("en-US"));
        }

        public void OnAfterDeserialize()
        {
            m_DateTime = DateTime.Parse(m_ProxyDate, new CultureInfo("en-US"));
        }
    }
}
