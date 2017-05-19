using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PSharp.TestingServices
{
    internal sealed class SharedCounterMachine : Machine
    {
        int counter = 0;

        [OnEventDoAction(typeof(SharedCounterEvent), nameof(ProcessEvent))]
        class Init : MachineState { }

        void ProcessEvent()
        {
            var e = this.ReceivedEvent as SharedCounterEvent;
            switch (e.op)
            {
                case SharedCounterEvent.SharedCounterOp.SET:
                    counter = e.value;
                    break;
                case SharedCounterEvent.SharedCounterOp.GET:
                    this.Send(e.sender, new SharedCounterResponseEvent(counter));
                    break;
                case SharedCounterEvent.SharedCounterOp.INC:
                    counter++;
                    break;
                case SharedCounterEvent.SharedCounterOp.DEC:
                    counter--;
                    break;
            }

        }
    }
}
