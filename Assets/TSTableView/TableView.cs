using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Tacticsoft
{
    public class TableView : MonoBehaviour
    {
        public RectTransform m_contentParentView;

        private ITableViewDataSource m_tableViewDataSource;
        public ITableViewDataSource tableViewDataSource
        {
            get { return m_tableViewDataSource; }
            set { m_tableViewDataSource = value; m_requiresReload = true; }
        }

        private ITableViewDelegate m_tableViewDelegate;
        public ITableViewDelegate tableViewDelegate { get; set; }

        private bool m_requiresReload;

        private LayoutElement m_topPadding;
        private LayoutElement m_bottomPadding;

        private float?[] m_rowHeights;
        private List<TableViewCell> m_visibleCells;
        private Pair<int> m_visibleRowRange;
        
        private float m_scrollY;

        void Start()
        {
            m_topPadding = CreateEmptyPaddingElement("TopPadding");
            m_bottomPadding = CreateEmptyPaddingElement("BottomPadding");
            m_topPadding.transform.SetParent(m_contentParentView, false);
            m_bottomPadding.transform.SetParent(m_contentParentView, false);
            m_visibleCells = new List<TableViewCell>();
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
            for (int i = 0; i < m_visibleCells.Count; i++)
            {
                Destroy(m_visibleCells[i]);
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
        }

        private void AddRow(int row, bool atEnd)
        {
            TableViewCell newCell = m_tableViewDataSource.GetCellForRowInTableView(this, row);
            newCell.transform.SetParent(m_contentParentView, false);
            LayoutElement layoutElement = newCell.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = m_rowHeights[row].Value;
        }

        private void RefreshVisibleRows()
        {
            Pair<int> newVisibleRows = CalculateCurrentVisibleRowRange();
            //Remove rows that disappeared to the top
            for (int i = m_visibleRowRange.first; i < newVisibleRows.first; i++)
            {
                HideRow(i);
            }
            //Remove rows that disappeared to the bottom
            for (int i = newVisibleRows.second; i < m_visibleRowRange.second; i++)
            {
                HideRow(i);
            }
        }

        private void HideRow(int i)
        {
            throw new System.NotImplementedException();
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

        }

        private LayoutElement CreateEmptyPaddingElement(string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(LayoutElement));
            LayoutElement le = go.GetComponent<LayoutElement>();
            return le;
        }
    }
}
