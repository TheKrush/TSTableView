using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Tacticsoft
{
    public class TableView : MonoBehaviour
    {
        /// <summary>
        /// The view that will contain the cell views. Usually a child of the view this behavior is attached to
        /// </summary>
        public RectTransform m_contentParentView;

        /// <summary>
        /// The data source that will feed this table view with information
        /// </summary>
        public ITableViewDataSource tableViewDataSource
        {
            get { return m_tableViewDataSource; }
            set { m_tableViewDataSource = value; m_requiresReload = true; }
        }
        private ITableViewDataSource m_tableViewDataSource;

        /// <summary>
        /// Optional delegate that will receieve various events about the internals
        /// </summary>
        public ITableViewDelegate tableViewDelegate { get; set; }
        private ITableViewDelegate m_tableViewDelegate;
        

        private bool m_requiresReload;

        private LayoutElement m_topPadding;
        private LayoutElement m_bottomPadding;
        
        private float?[] m_rowHeights;
        private LinkedList<TableViewCell> m_visibleCells;
        private Pair<int> m_visibleRowRange;
        
        private float m_scrollY;

        void Start()
        {
            m_topPadding = CreateEmptyPaddingElement("TopPadding");
            m_topPadding.transform.SetParent(m_contentParentView, false);
            m_bottomPadding = CreateEmptyPaddingElement("Bottom");
            m_bottomPadding.transform.SetParent(m_contentParentView, false);
            m_visibleCells = new LinkedList<TableViewCell>();
        }
        
        void Update()
        {
            if (m_requiresReload)
            {
                ReloadData();
                m_requiresReload = false;
            }
        }

        public void ReloadData()
        {
            m_rowHeights = new float?[m_tableViewDataSource.GetNumberOfRowsForTableView(this)];
            foreach (TableViewCell cell in m_visibleCells)
            {
                Destroy(cell);
            }
            m_visibleCells.Clear();
            m_visibleRowRange = new Pair<int>(-1, -1);
            SetInitialVisibleRows();
        }

        private Pair<int> CalculateCurrentVisibleRowRange()
        {
            float startY = m_scrollY;
            float endY = m_scrollY + (this.transform as RectTransform).rect.height;
            float cumulativeHeight = 0;
            int curRow = 0;
            int firstVisibleRow = -1;
            while (cumulativeHeight < endY)
            {
                cumulativeHeight += GetHeightForRow(curRow);
                if (cumulativeHeight >= startY && firstVisibleRow == -1)
                {
                    firstVisibleRow = curRow;
                }
                curRow++;
            }
            int lastVisibleRow = curRow;
            return new Pair<int>(firstVisibleRow, lastVisibleRow-1);
        }

        private void SetInitialVisibleRows()
        {
            Pair<int> visibleRows = CalculateCurrentVisibleRowRange();
            for (int i = visibleRows.first; i <= visibleRows.second; i++)
            {
                AddRow(i, true);
            }
            m_visibleRowRange = visibleRows;
            UpdatePaddingElements();
        }

        private void AddRow(int row, bool atEnd)
        {
            TableViewCell newCell = m_tableViewDataSource.GetCellForRowInTableView(this, row);
            newCell.transform.SetParent(m_contentParentView, false);
            LayoutElement layoutElement = newCell.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = m_rowHeights[row].Value;
            if (atEnd) {
                m_visibleCells.AddLast(newCell);
                newCell.transform.SetSiblingIndex(m_contentParentView.childCount - 2); //One before bottom padding
            } else {
                m_visibleCells.AddFirst(newCell);
                newCell.transform.SetSiblingIndex(1); //One after the top padding
            }
        }

        private void RefreshVisibleRows()
        {
            Pair<int> newVisibleRows = CalculateCurrentVisibleRowRange();
            //Remove rows that disappeared to the top
            for (int i = m_visibleRowRange.first; i < newVisibleRows.first; i++)
            {
                HideRow(false);
            }
            //Remove rows that disappeared to the bottom
            for (int i = newVisibleRows.second; i < m_visibleRowRange.second; i++)
            {
                HideRow(true);
            }
            //Add rows that appeared on top
            for (int i = newVisibleRows.first; i < m_visibleRowRange.first; i++) {
                AddRow(i, false);
            }
            //Add rows that appeared on bottom
            for (int i = m_visibleRowRange.second + 1; i <= newVisibleRows.second; i++) {
                AddRow(i, true);
            }
            m_visibleRowRange = newVisibleRows;
            UpdatePaddingElements();
        }

        private void UpdatePaddingElements() {
            float hiddenElementsHeightSum = 0;
            for (int i = 0; i < m_visibleRowRange.first; i++) {
                hiddenElementsHeightSum += GetHeightForRow(i);
            }
            m_topPadding.preferredHeight = hiddenElementsHeightSum;
            for (int i = m_visibleRowRange.first; i <= m_visibleRowRange.second; i++) {
                hiddenElementsHeightSum += GetHeightForRow(i);
            }
            float bottomPaddingHeight = m_contentParentView.rect.height - hiddenElementsHeightSum;
            m_bottomPadding.preferredHeight = bottomPaddingHeight;
        }

        private void HideRow(bool last)
        {
            Debug.Log("Hiding row at scroll y " + m_scrollY.ToString("0.00"));
            if (last) {
                GameObject.Destroy(m_visibleCells.Last.Value.gameObject);
                m_visibleCells.RemoveLast();
            } else {
                GameObject.Destroy(m_visibleCells.First.Value.gameObject);
                m_visibleCells.RemoveFirst();
            }
        }

        private float GetHeightForRow(int row)
        {
            if (!m_rowHeights[row].HasValue)
            {
                m_rowHeights[row] = m_tableViewDataSource.GetHeightForRowInTableView(this, row);
            }
            return m_rowHeights[row].Value;
        }

        public void ScrollViewValueChanged(Vector2 newScrollValue)
        {
            float relativeScroll = 1 - newScrollValue.y;
            float scrollableHeight = m_contentParentView.rect.height - (this.transform as RectTransform).rect.height;
            m_scrollY = relativeScroll * scrollableHeight;
            Debug.Log(m_scrollY.ToString(("0.00")));
            RefreshVisibleRows();
        }

        private LayoutElement CreateEmptyPaddingElement(string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(LayoutElement));
            LayoutElement le = go.GetComponent<LayoutElement>();
            return le;
        }
    }
}
