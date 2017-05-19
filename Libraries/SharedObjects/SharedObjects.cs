using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PSharp;

namespace Microsoft.PSharp
{
    /// <summary>
    /// Creates objects that can be shared by multiple P# machines
    /// </summary>
    public static class SharedObjects
    {
        /// <summary>
        /// Creates a shared counter
        /// </summary>
        /// <param name="runtime">PSharp runtime</param>
        /// <param name="value">Initial value</param>
        public static ISharedCounter CreateSharedCounter(PSharpRuntime runtime, int value = 0)
        {
            if (runtime is StateMachineRuntime)
            {
                return new ProductionSharedCounter(value);
            }
            else if (runtime is TestingServices.BugFindingRuntime)
            {
                return new TestingServices.MockSharedCounter(value, runtime as TestingServices.BugFindingRuntime);
            }
            else
            {
                throw new PSharp.RuntimeException("Unknown runtime object type: " + runtime.GetType().Name);
            }
        }

    }
}
