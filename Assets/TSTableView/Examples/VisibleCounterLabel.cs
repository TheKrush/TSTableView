using UnityEngine;
using System.Collections;
using Tacticsoft;
using UnityEngine.UI;

public class VisibleCounterLabel : TableViewCell {

    public Text m_rowNumberText;
    public Text m_visibleCountText;

    public void SetRowNumber(int rowNumber)
    {
        m_rowNumberText.text = rowNumber.ToString();
    }

    private int m_numTimesVisible;
    public void NotifyBecameVisible()
    {
        m_numTimesVisible++;
        m_visibleCountText.text = m_numTimesVisible.ToString();
    }

}
