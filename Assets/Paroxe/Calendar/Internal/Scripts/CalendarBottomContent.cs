using UnityEngine;
using UnityEngine.EventSystems;

namespace Paroxe.SuperCalendar.Internal
{
    public class CalendarBottomContent : UIBehaviour
    {
        private CanvasGroup m_CanvasGroup;
        private Vector3 m_BigStartScale = Vector3.one*2.0f;
        private Vector3 m_SmallStartScale = Vector3.one*0.75f;
        private float m_AnimationPosition;
        private float m_AnimationDuration = 0.50f;
        private bool m_Show;
        private bool m_Hide;
        private bool m_FromSmall;
        private bool m_ToSmall;
        private Calendar m_Calendar;

        public CalendarLayoutGroup m_CalendarLayoutGroup;
        public float m_TopPadding = 23.0f;

        protected override void OnEnable()
        {
            if (m_CanvasGroup == null)
                m_CanvasGroup = GetComponent<CanvasGroup>();
            if (m_Calendar == null)
                m_Calendar = GetComponentInParent<Calendar>();
        }

        public CalendarLayoutGroup GetContentLayoutGroup()
        {
            return m_CalendarLayoutGroup;
        }

        public void ShowImmediate()
        {
            if (m_CanvasGroup == null)
                m_CanvasGroup = GetComponent<CanvasGroup>();

            m_CanvasGroup.alpha = 1.0f;
            m_CanvasGroup.interactable = true;
            m_CanvasGroup.blocksRaycasts = true;
        }

        public void HideImmediate()
        {
            if (m_CanvasGroup == null)
                m_CanvasGroup = GetComponent<CanvasGroup>();

            m_CanvasGroup.alpha = 0.0f;
            m_CanvasGroup.interactable = false;
            m_CanvasGroup.blocksRaycasts = false;
        }

        public void Show(bool fromSmall, float delay)
        {
            if (!gameObject.activeInHierarchy)
                gameObject.SetActive(true);

            if (!fromSmall)
                gameObject.transform.SetAsLastSibling();

            m_AnimationPosition = -delay;
            m_Show = true;
            m_Hide = false;
            transform.localScale = fromSmall ? m_SmallStartScale : m_BigStartScale;
            m_CanvasGroup.alpha = 0.0f;
            m_CanvasGroup.interactable = true;
            m_CanvasGroup.blocksRaycasts = true;
            m_FromSmall = fromSmall;
        }

        public void Hide(bool toSmall, float delay)
        {
            if (!toSmall)
                gameObject.transform.SetAsLastSibling();

            m_AnimationPosition = -delay;
            m_Hide = true;
            m_Show = false;
            transform.localScale = Vector3.one;
            m_CanvasGroup.alpha = 1.0f;
            m_CanvasGroup.interactable = false;
            m_CanvasGroup.blocksRaycasts = false;
            m_ToSmall = toSmall;
        }

        float CubicEaseOut(float currentTime, float startingValue, float finalValue, float duration)
        {
            return finalValue * ((currentTime = currentTime / duration - 1) * currentTime * currentTime + 1) + startingValue;
        }

        void Update()
        {
            if (m_Show || m_Hide)
            {
                if (m_Calendar == null)
                    m_Calendar = GetComponentInParent<Calendar>();

                if (m_Calendar.AnimatePanels)
                {
                    m_AnimationPosition += Time.deltaTime/m_AnimationDuration;
                    if (m_AnimationPosition >= 1.0f)
                        m_AnimationPosition = 1.0f;
                }
                else
                    m_AnimationPosition = 1.0f;

                float alphaAnim = 0.0f;
                float scaleAnim = 0.0f;

                if (m_Show)
                {
                    alphaAnim = CubicEaseOut(Mathf.Clamp01(m_AnimationPosition), 0.0f, 1.0f, 1.0f);
                    scaleAnim = CubicEaseOut(Mathf.Clamp01(m_AnimationPosition), 0.0f, 1.0f, 1.0f);
                }
                else
                {
                    alphaAnim = CubicEaseOut(Mathf.Clamp01(m_AnimationPosition), 0.0f, 1.0f, 1.0f);
                    scaleAnim = CubicEaseOut(Mathf.Clamp01(m_AnimationPosition), 0.0f, 1.0f, 1.0f);
                }

                if (m_Show)
                    transform.localScale = Vector3.Lerp(m_FromSmall ? m_SmallStartScale : m_BigStartScale, Vector3.one,
                        scaleAnim);
                else
                    transform.localScale = Vector3.Lerp(Vector3.one, m_ToSmall ? m_SmallStartScale : m_BigStartScale,
                        scaleAnim);

                m_CanvasGroup.alpha = m_Show ? alphaAnim : 1.0f - alphaAnim;

                if (m_AnimationPosition >= 1.0f)
                {
                    m_Show = false;
                    m_Hide = false;
                }
            }
        }
    }
}