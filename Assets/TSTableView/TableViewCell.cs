using UnityEngine;
using System.Collections;

namespace Tacticsoft
{
    public class TableViewCell : MonoBehaviour
    {
        public virtual string reuseIdentifier { 
            get { 
                return this.GetType().Name; 
            } 
        }
    }
}
