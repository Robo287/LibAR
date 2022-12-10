using UnityEngine;
using UnityEngine.UI;

namespace Paroxe.SuperCalendar.Internal
{
    public class CalendarLayoutGroup : LayoutGroup
    {
        public enum Corner
        {
            UpperLeft = 0,
            UpperRight = 1,
            LowerLeft = 2,
            LowerRight = 3
        }

        public enum Axis
        {
            Horizontal = 0,
            Vertical = 1
        }

        public delegate void RectTransformChangedEventHandler(object sender);

        public event RectTransformChangedEventHandler OnRectTransformChanged;

        [SerializeField] protected Corner m_StartCorner = Corner.UpperLeft;

        public Corner startCorner
        {
            get { return m_StartCorner; }
            set { SetProperty(ref m_StartCorner, value); }
        }

        protected Axis m_StartAxis = Axis.Horizontal;

        protected Axis startAxis
        {
            get { return m_StartAxis; }
            set { SetProperty(ref m_StartAxis, value); }
        }

        public bool m_SquareCell = true;

        public bool m_GetOtherRectTranformHeight = false;
        public RectTransform m_RectTransformHeightReference;

        protected Vector2 m_CellSize = new Vector2(100, 100);

        public Vector2 cellSize
        {
            get
            {
                RectTransform rt = transform as RectTransform;

                float width = 0.0f;
                float height = 0.0f;

                if (m_SquareCell)
                {
                    width = (rt.rect.width - 1.0f)/m_ConstraintColCount + 1.0f;
                    height = (rt.rect.width - 1.0f)/m_ConstraintColCount + 1.0f;
                }
                else
                {
                    width = (rt.rect.width - 1.0f)/m_ConstraintColCount + 1.0f;

                    if (!m_GetOtherRectTranformHeight)
                        height = (rt.rect.height - 1.0f)/m_ConstraintRowCount + 1.0f;
                    else
                        height = (m_RectTransformHeightReference.rect.height - 1.0f)/m_ConstraintRowCount + 1.0f;
                }

                return new Vector2(width, height);
            }
        }

        public Vector2 ContentDimension
        {
            get { return new Vector2(cellSize.x*m_ConstraintColCount, cellSize.y*constraintRowCount); }
        }

        protected Vector2 m_Spacing = -1.0f*Vector2.one;

        protected Vector2 spacing
        {
            get { return m_Spacing; }
            set { SetProperty(ref m_Spacing, value); }
        }

        [SerializeField]
        protected int m_ConstraintColCount = 7;

        public int constraintColCount
        {
            get { return m_ConstraintColCount; }
            set { SetProperty(ref m_ConstraintColCount, Mathf.Max(1, value)); }
        }

        [SerializeField] protected int m_ConstraintRowCount = 5;

        public int constraintRowCount
        {
            get { return m_ConstraintRowCount; }
            set { SetProperty(ref m_ConstraintRowCount, Mathf.Max(1, value)); }
        }

        protected CalendarLayoutGroup()
        {
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();

            if (OnRectTransformChanged != null)
                OnRectTransformChanged(this);
        }

#if UNITY_EDITOR && !UNITY_4_6 && !UNITY_4_7
    protected override void OnValidate()
    {
        base.OnValidate();
        constraintColCount = constraintColCount;
    }

#endif

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            int minColumns = 0;
            int preferredColumns = 0;

            minColumns = preferredColumns = m_ConstraintColCount;

            SetLayoutInputForAxis(
                padding.horizontal + (cellSize.x + spacing.x)*minColumns - spacing.x,
                padding.horizontal + (cellSize.x + spacing.x)*preferredColumns - spacing.x,
                -1, 0);
        }

        public override void CalculateLayoutInputVertical()
        {
            int minRows = Mathf.CeilToInt(rectChildren.Count/(float) m_ConstraintColCount - 0.001f);

            float minSpace = padding.vertical + (cellSize.y + spacing.y)*minRows - spacing.y;
            SetLayoutInputForAxis(minSpace, minSpace, -1, 1);
        }

        public override void SetLayoutHorizontal()
        {
            SetCellsAlongAxis(0);
        }

        public override void SetLayoutVertical()
        {
            SetCellsAlongAxis(1);
        }

        private void SetCellsAlongAxis(int axis)
        {
            if (axis == 0)
            {
                for (int i = 0; i < rectChildren.Count; i++)
                {
                    RectTransform rect = rectChildren[i];

                    m_Tracker.Add(this, rect,
                        DrivenTransformProperties.Anchors |
                        DrivenTransformProperties.AnchoredPosition |
                        DrivenTransformProperties.SizeDelta);

                    rect.anchorMin = Vector2.up;
                    rect.anchorMax = Vector2.up;
                    rect.sizeDelta = cellSize;
                }
                return;
            }

            int cellCountX = m_ConstraintColCount;
            int cellCountY = Mathf.CeilToInt(rectChildren.Count / (float)cellCountX - 0.001f);
            int cornerX = (int) startCorner%2;
            int cornerY = (int) startCorner/2;

            int cellsPerMainAxis, actualCellCountX, actualCellCountY;
            if (startAxis == Axis.Horizontal)
            {
                cellsPerMainAxis = cellCountX;
                actualCellCountX = Mathf.Clamp(cellCountX, 1, rectChildren.Count);
                actualCellCountY = Mathf.Clamp(cellCountY, 1,
                    Mathf.CeilToInt(rectChildren.Count/(float) cellsPerMainAxis));
            }
            else
            {
                cellsPerMainAxis = cellCountY;
                actualCellCountY = Mathf.Clamp(cellCountY, 1, rectChildren.Count);
                actualCellCountX = Mathf.Clamp(cellCountX, 1,
                    Mathf.CeilToInt(rectChildren.Count/(float) cellsPerMainAxis));
            }

            Vector2 requiredSpace = new Vector2(
                actualCellCountX*cellSize.x + (actualCellCountX - 1)*spacing.x,
                actualCellCountY*cellSize.y + (actualCellCountY - 1)*spacing.y
                );
            Vector2 startOffset = new Vector2(
                GetStartOffset(0, requiredSpace.x),
                GetStartOffset(1, requiredSpace.y)
                );

            for (int i = 0; i < rectChildren.Count; i++)
            {
                int positionX;
                int positionY;
                if (startAxis == Axis.Horizontal)
                {
                    positionX = i%cellsPerMainAxis;
                    positionY = i/cellsPerMainAxis;
                }
                else
                {
                    positionX = i/cellsPerMainAxis;
                    positionY = i%cellsPerMainAxis;
                }

                if (cornerX == 1)
                    positionX = actualCellCountX - 1 - positionX;
                if (cornerY == 1)
                    positionY = actualCellCountY - 1 - positionY;

                SetChildAlongAxis(rectChildren[i], 0, startOffset.x + (cellSize[0] + spacing[0])*positionX, cellSize[0]);
                SetChildAlongAxis(rectChildren[i], 1, startOffset.y + (cellSize[1] + spacing[1])*positionY, cellSize[1]);
            }
        }
    }
}