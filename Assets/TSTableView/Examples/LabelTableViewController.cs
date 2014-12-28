using UnityEngine;
using System.Collections;
using Tacticsoft;

public class LabelTableViewController : MonoBehaviour, ITableViewDataSource, ITableViewDelegate {

    public VisibleCounterLabel m_labelPrefab;
    public TableView m_tableView;

    public int m_numRows;

	// Use this for initialization
	void Start () {
        m_tableView.tableViewDelegate = this;
        m_tableView.tableViewDataSource = this;
	}

    #region TSTableViewDataSource

    public int GetNumberOfRowsForTableView(TableView tableView)
    {
        return m_numRows;
    }

    public float GetHeightForRowInTableView(TableView tableView, int row)
    {
        return (m_labelPrefab.transform as RectTransform).rect.height;
    }

    public TableViewCell GetCellForRowInTableView(TableView tableView, int row)
    {
        VisibleCounterLabel counterLabel = (VisibleCounterLabel)GameObject.Instantiate(m_labelPrefab);
        counterLabel.SetRowNumber(row);
        counterLabel.gameObject.name = "Row_" + row.ToString();
        return counterLabel;
    }

    #endregion

    #region TSTableViewDelegate

    public void TableViewWillDisplayCell(TableView tableView, TableViewCell cell)
    {
        VisibleCounterLabel counterLabel = (VisibleCounterLabel)cell;
        counterLabel.NotifyBecameVisible();
    }

    #endregion

}
