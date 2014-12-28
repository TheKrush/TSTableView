using UnityEngine;
using System.Collections;

namespace Tacticsoft
{
    public interface ITableViewDelegate
    {
        void TableViewWillDisplayCell(TableView tableView, TableViewCell cell);
    }
}
