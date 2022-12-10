using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Paroxe.SuperCalendar.Internal
{
    [ExecuteInEditMode]
    public class CalendarClock : UIBehaviour
    {
        private Image m_Image;
        private RectTransform[] m_Children;
        private RectTransform m_RectTransform;

        protected override void OnEnable()
        {
            base.OnEnable();

            AdjustChildrenScale();
        }

        private void ResolveReferences()
        {
            if (m_Image == null)
                m_Image = GetComponent<Image>();

            if (m_Children == null)
                m_Children = GetComponentsInChildren<RectTransform>();

            if (m_RectTransform == null)
                m_RectTransform = transform as RectTransform;
        }

        private void AdjustChildrenScale()
        {
            ResolveReferences();

            for (int i = 0; i < m_RectTransform.childCount; ++i)
            {
                m_RectTransform.GetChild(i).localScale = Vector3.one * (m_RectTransform.rect.width / m_Image.sprite.textureRect.width);
            }
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();

            AdjustChildrenScale();
        }
    }
}
