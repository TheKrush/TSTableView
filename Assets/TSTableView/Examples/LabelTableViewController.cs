using UnityEngine;
using System.Collections;
using Tacticsoft;

namespace Tacticsoft.Examples
{
    public class LabelTableViewController : MonoBehaviour, ITableViewDataSource, ITableViewDelegate
    {
        public VisibleCounterLabel m_labelPrefab;
        public TableView m_tableView;

        public int m_numRows;

        // Use this for initialization
        void Start() {
            m_tableView.tableViewDelegate = this;
            m_tableView.tableViewDataSource = this;
        }

        #region ITableViewDataSource

        public int GetNumberOfRowsForTableView(TableView tableView) {
            return m_numRows;
        }

        public float GetHeightForRowInTableView(TableView tableView, int row) {
            return (m_labelPrefab.transform as RectTransform).rect.height;
        }

        public TableViewCell GetCellForRowInTableView(TableView tableView, int row) {
            VisibleCounterLabel cell = tableView.GetReusableCell(m_labelPrefab.reuseIdentifier) as VisibleCounterLabel;
            if (cell == null) {
                cell = (VisibleCounterLabel)GameObject.Instantiate(m_labelPrefab);
            }
            cell.SetRowNumber(row);
            cell.gameObject.name = "Row_" + row.ToString();
            return cell;
        }

        #endregion

        #region ITableViewDelegate

        public void TableViewWillDisplayCell(TableView tableView, TableViewCell cell) {
            VisibleCounterLabel counterLabel = (VisibleCounterLabel)cell;
            counterLabel.NotifyBecameVisible();
        }

        #endregion

    }
}
