using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.PSharp.TestingServices
{
    /// <summary>
    /// Implements a shared counter
    /// </summary>
    internal sealed class MockSharedCounter : ISharedCounter
    {
        /// <summary>
        /// The counter
        /// </summary>
        MachineId counterMachine;

        BugFindingRuntime Runtime;

        /// <summary>
        /// Initializes the counter
        /// </summary>
        /// <param name="value">Initial value</param>
        /// <param name="Runtime">Runtime</param>
        public MockSharedCounter(int value, BugFindingRuntime Runtime)
        {
            this.Runtime = Runtime;
            var counterMachine = Runtime.CreateMachine(typeof(SharedCounterMachine));
            Runtime.SendEvent(counterMachine, SharedCounterEvent.SetEvent(value));
        }

        /// <summary>
        /// Increments the counter
        /// </summary>
        public void Increment()
        {
            Runtime.SendEvent(counterMachine, SharedCounterEvent.IncrementEvent());
        }

        /// <summary>
        /// Decrements the counter
        /// </summary>
        public void Decrement()
        {
            Runtime.SendEvent(counterMachine, SharedCounterEvent.DecrementEvent());
        }

        /// <summary>
        /// Gets current value of the counter
        /// </summary>
        public int GetValue()
        {
            var currentMachine = Runtime.GetCurrentMachine();
            Runtime.SendEvent(counterMachine, SharedCounterEvent.GetEvent(currentMachine.Id));
            var response = currentMachine.Receive(typeof(SharedCounterResponseEvent)).Result;
            return (response as SharedCounterResponseEvent).value;
        }
    }
}
