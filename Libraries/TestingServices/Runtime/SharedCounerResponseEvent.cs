using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PSharp
{
    internal class SharedCounterResponseEvent : Event
    {
        public int value;

        public SharedCounterResponseEvent(int value)
        {
            this.value = value;
        }

    }
}
