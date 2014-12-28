using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tacticsoft
{
	struct Pair<T>
	{
        public T first;
        public T second;
        
        public Pair(T first, T second)
        {
            this.first = first;
            this.second = second;
        }
	}
}
