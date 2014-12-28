using UnityEngine;
using System.Collections;

namespace Tacticsoft
{
    /// <summary>
    /// A delegate that receives events about a TableView's internals
    /// </summary>
    public interface ITableViewDelegate
    {
        /// <summary>
        /// Called after the TableView placed the cell, before it is shown for the first frame
        /// </summary>
        /// <param name="tableView">The TableView that will display the cell</param>
        /// <param name="cell">The cell about to be displayed</param>
        void TableViewWillDisplayCell(TableView tableView, TableViewCell cell);
    }
}
