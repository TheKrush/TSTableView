using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Tacticsoft
{
    /// <summary>
    /// A reusable table for for (vertical) tables. API inspired by Cocoa's UITableView
    /// Hierarchy structure should be :
    /// GameObject + TableView (this) + Mask + Scroll Rect (point to child)
    /// - Child GameObject + Vertical Layout Group
    /// </summary>
    public class TableView : MonoBehaviour
    {
        /// <summary>
        /// The view that will contain the cell views. Usually a child of the view this behavior is attached to
        /// </summary>
        public RectTransform m_contentParentView;

        /// <summary>
        /// The data source that will feed this table view with information. Required.
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

        /// <summary>
        /// Get a cell that is no longer in use for reusing
        /// </summary>
        /// <param name="reuseIdentifier">The identifier for the cell type</param>
        /// <returns>A prepared cell if available, null if none</returns>
        public TableViewCell GetReusableCell(string reuseIdentifier) {
            LinkedList<TableViewCell> cells;
            if (!m_reusableCells.TryGetValue(reuseIdentifier, out cells)) {
                return null;
            }
            if (cells.Count == 0) {
                return null;
            }
            TableViewCell cell = cells.First.Value;
            cells.RemoveFirst();
            return cell;
        }

        /// <summary>
        /// Reload the table view. Manually call this if the data source changed in a way that alters the basic layout
        /// (number of rows changed, etc)
        /// </summary>
        public void ReloadData() {
            m_rowHeights = new float[m_tableViewDataSource.GetNumberOfRowsForTableView(this)];
            m_cumulativeRowHeights = new float[m_rowHeights.Length];

            for (int i = 0; i < m_rowHeights.Length; i++) {
                m_rowHeights[i] = m_tableViewDataSource.GetHeightForRowInTableView(this, i);
                if (i > 0) {
                    m_cumulativeRowHeights[i] = m_rowHeights[i] + m_cumulativeRowHeights[i - 1];
                } else {
                    m_cumulativeRowHeights[i] = m_rowHeights[i];
                }
            }
            m_contentParentView.sizeDelta = new Vector2(m_contentParentView.sizeDelta[0], m_cumulativeRowHeights[m_rowHeights.Length-1]);

            foreach (TableViewCell cell in m_visibleCells) {
                Destroy(cell);
            }
            m_visibleCells.Clear();
            m_visibleRowRange = new Pair<int>(-1, -1);
            SetInitialVisibleRows();
            m_requiresReload = false;
        }

        /// <summary>
        /// Event listener for the scroll rect scrolling.
        /// Make sure this method is added as a callback to its "changed scroll" event
        /// </summary>
        /// <param name="newScrollValue"></param>
        public void ScrollViewValueChanged(Vector2 newScrollValue) {
            float relativeScroll = 1 - newScrollValue.y;
            float scrollableHeight = m_contentParentView.rect.height - (this.transform as RectTransform).rect.height;
            m_scrollY = relativeScroll * scrollableHeight;
            //Debug.Log(m_scrollY.ToString(("0.00")));
            RefreshVisibleRows();
        }

        private bool m_requiresReload;

        private LayoutElement m_topPadding;
        private LayoutElement m_bottomPadding;
        
        private float[] m_rowHeights;
        private float[] m_cumulativeRowHeights;

        private LinkedList<TableViewCell> m_visibleCells;
        private Pair<int> m_visibleRowRange;
        
        private RectTransform m_reusableCellContainer;
        private Dictionary<string, LinkedList<TableViewCell>> m_reusableCells;

        private float m_scrollY;

        void Awake()
        {
            m_topPadding = CreateEmptyPaddingElement("TopPadding");
            m_topPadding.transform.SetParent(m_contentParentView, false);
            m_bottomPadding = CreateEmptyPaddingElement("Bottom");
            m_bottomPadding.transform.SetParent(m_contentParentView, false);
            m_visibleCells = new LinkedList<TableViewCell>();

            m_reusableCellContainer = new GameObject("ReusableCells", typeof(RectTransform)).GetComponent<RectTransform>();
            m_reusableCellContainer.SetParent(this.transform, false);
            m_reusableCellContainer.gameObject.SetActive(false);
            m_reusableCells = new Dictionary<string, LinkedList<TableViewCell>>();
        }
        
        void Update()
        {
            if (m_requiresReload)
            {
                ReloadData();
            }
        }

        
        private Pair<int> CalculateCurrentVisibleRowRange()
        {
            float startY = m_scrollY;
            float endY = m_scrollY + (this.transform as RectTransform).rect.height;
            float cumulativeHeight = 0;
            int curRow = 0;
            int firstVisibleRow = -1;
            while (cumulativeHeight < endY && curRow < m_rowHeights.Length)
            {
                cumulativeHeight += m_rowHeights[curRow];
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

            LayoutElement layoutElement = newCell.GetComponent<LayoutElement>();
            if (layoutElement == null) {
                layoutElement = newCell.gameObject.AddComponent<LayoutElement>();
            }
            layoutElement.preferredHeight = m_rowHeights[row];

            if (atEnd) {
                m_visibleCells.AddLast(newCell);
                newCell.transform.SetSiblingIndex(m_contentParentView.childCount - 2); //One before bottom padding
            } else {
                m_visibleCells.AddFirst(newCell);
                newCell.transform.SetSiblingIndex(1); //One after the top padding
            }

            if (this.tableViewDelegate != null) {
                this.tableViewDelegate.TableViewWillDisplayCell(this, newCell);
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
                hiddenElementsHeightSum += m_rowHeights[i];
            }
            m_topPadding.preferredHeight = hiddenElementsHeightSum;
            for (int i = m_visibleRowRange.first; i <= m_visibleRowRange.second; i++) {
                hiddenElementsHeightSum += m_rowHeights[i];
            }
            float bottomPaddingHeight = m_contentParentView.rect.height - hiddenElementsHeightSum;
            m_bottomPadding.preferredHeight = bottomPaddingHeight;
        }

        private void HideRow(bool last)
        {
            //Debug.Log("Hiding row at scroll y " + m_scrollY.ToString("0.00"));
            if (last) {
                StoreCellForReuse(m_visibleCells.Last.Value);
                m_visibleCells.RemoveLast();
            } else {
                StoreCellForReuse(m_visibleCells.First.Value);
                m_visibleCells.RemoveFirst();
            }
        }

        private LayoutElement CreateEmptyPaddingElement(string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(LayoutElement));
            LayoutElement le = go.GetComponent<LayoutElement>();
            return le;
        }

        

        private void StoreCellForReuse(TableViewCell cell) {
            string reuseIdentifier = cell.reuseIdentifier;
            
            if (string.IsNullOrEmpty(reuseIdentifier)) {
                GameObject.Destroy(cell.gameObject);
                return;
            }

            if (!m_reusableCells.ContainsKey(reuseIdentifier)) {
                m_reusableCells.Add(reuseIdentifier, new LinkedList<TableViewCell>());
            }
            m_reusableCells[reuseIdentifier].AddLast(cell);
            cell.transform.SetParent(m_reusableCellContainer, false);
        }
    }
}
