using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PSharp
{
    internal class SharedCounterEvent : Event
    {
        internal enum SharedCounterOp { GET, SET, INC, DEC };

        public SharedCounterOp op { get; private set; }

        public int value { get; private set; }

        public MachineId sender { get; private set; }

        SharedCounterEvent(SharedCounterOp op, int value, MachineId sender)
        {
            this.op = op;
            this.value = value;
            this.sender = sender;
        }

        public static SharedCounterEvent IncrementEvent()
        {
            return new SharedCounterEvent(SharedCounterOp.INC, 0, null);
        }

        public static SharedCounterEvent DecrementEvent()
        {
            return new SharedCounterEvent(SharedCounterOp.DEC, 0, null);
        }

        public static SharedCounterEvent SetEvent(int value)
        {
            return new SharedCounterEvent(SharedCounterOp.SET, value, null);
        }

        public static SharedCounterEvent GetEvent(MachineId sender)
        {
            return new SharedCounterEvent(SharedCounterOp.GET, 0, sender);
        }

    }
}
