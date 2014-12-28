using UnityEngine;
using System.Collections;
using Tacticsoft;

namespace Tacticsoft.Examples
{
    public class SimpleTableViewController : MonoBehaviour, ITableViewDataSource, ITableViewDelegate
    {
        public VisibleCounterCell m_labelPrefab;
        public TableView m_tableView;

        public int m_numRows;

        //Register as the TableView's delegate (required) and data source (optional)
        //to receive the calls
        void Start() {
            m_tableView.tableViewDelegate = this;
            m_tableView.tableViewDataSource = this;
        }

        #region ITableViewDataSource

        //Will be called by the TableView to know how many rows are in this table
        public int GetNumberOfRowsForTableView(TableView tableView) {
            return m_numRows;
        }

        //Will be called by the TableView to know what is the height of each row
        public float GetHeightForRowInTableView(TableView tableView, int row) {
            return (m_labelPrefab.transform as RectTransform).rect.height;
        }

        //Will be called by the TableView when a cell needs to be created for display
        public TableViewCell GetCellForRowInTableView(TableView tableView, int row) {
            VisibleCounterCell cell = tableView.GetReusableCell(m_labelPrefab.reuseIdentifier) as VisibleCounterCell;
            if (cell == null) {
                cell = (VisibleCounterCell)GameObject.Instantiate(m_labelPrefab);
            }
            cell.SetRowNumber(row);
            cell.gameObject.name = "Row_" + row.ToString();
            return cell;
        }

        #endregion

        #region ITableViewDelegate

        //Will be called by the TableView when a cell is about to be displayed (after it has
        //been positioned in the hierarchy)
        public void TableViewWillDisplayCell(TableView tableView, TableViewCell cell) {
            VisibleCounterCell counterLabel = (VisibleCounterCell)cell;
            counterLabel.NotifyBecameVisible();
        }

        #endregion

    }
}
