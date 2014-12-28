using UnityEngine;
using System.Collections;

namespace Tacticsoft
{
    public interface ITableViewDataSource
    {
        int GetNumberOfRowsForTableView(TableView tableView);
        float GetHeightForRowInTableView(TableView tableView, int row);
        TableViewCell GetCellForRowInTableView(TableView tableView, int row);
    }
}

