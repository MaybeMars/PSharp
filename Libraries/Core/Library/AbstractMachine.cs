//-----------------------------------------------------------------------
// <copyright file="AbstractMachine.cs">
//      Copyright (c) Microsoft Corporation. All rights reserved.
// 
//      THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//      EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//      MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//      IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//      CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//      TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//      SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.PSharp
{
    /// <summary>
    /// Abstract class representing a P# machine.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class AbstractMachine
    {
        #region fields

        /// <summary>
        /// The P# runtime that executes this machine.
        /// </summary>
        internal PSharpRuntime Runtime { get; private set; }

        /// <summary>
        /// The unique machine id.
        /// </summary>
        protected internal MachineId Id { get; private set; }

        /// <summary>
        /// The operation id.
        /// </summary>
        internal int OperationId { get; private set; }

        /// <summary>
        /// Checks if the machine is executing an OnExit method.
        /// </summary>
        internal bool IsInsideOnExit;

        /// <summary>
        /// Checks if the currently executing machine action invoked
        /// a transition (i.e. raise, goto or pop).
        /// </summary>
        internal bool IsPendingTransition;

        /// <summary>
        /// Program counter used for state-caching. Distinguishes
        /// scheduling from non-deterministic choices.
        /// </summary>
        internal int ProgramCounter;

        #endregion

        #region generic public and override methods

        /// <summary>
        /// Constructor.
        /// </summary>
        public AbstractMachine()
        {
            this.OperationId = 0;
            this.IsInsideOnExit = false;
            this.IsPendingTransition = false;
        }

        /// <summary>
        /// Determines whether the specified System.Object is equal
        /// to the current System.Object.
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            AbstractMachine m = obj as AbstractMachine;
            if (m == null ||
                this.GetType() != m.GetType())
            {
                return false;
            }

            return this.Id.Value == m.Id.Value;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>int</returns>
        public override int GetHashCode()
        {
            return this.Id.Value.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the current machine.
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return this.Id.Name;
        }

        #endregion

        #region internal methods
        
        /// <summary>
        /// Sets the id of this machine.
        /// </summary>
        /// <param name="mid">MachineId</param>
        internal void SetMachineId(MachineId mid)
        {
            this.Id = mid;
            this.Runtime = mid.Runtime;
        }

        /// <summary>
        /// Sets the operation id of this machine.
        /// </summary>
        /// <param name="opid">OperationId</param>
        internal void SetOperationId(int opid)
        {
            this.OperationId = opid;
        }

        /// <summary>
        /// Returns true if the given operation id is pending
        /// execution by the machine.
        /// </summary>
        /// <param name="opid">OperationId</param>
        /// <returns>Boolean</returns>
        internal virtual bool IsOperationPending(int opid)
        {
            return false;
        }

        /// <summary>
        /// Asserts that a raise, goto or pop method has not already been called.
        /// Also records that a raise, goto or pop method has been called.
        /// </summary>
        internal void AssertCorrectTransitionInvocation()
        {
            this.Runtime.Assert(!this.IsInsideOnExit, $"Machine '{this.Id}' has called " +
                "Raise, Goto or Pop inside an OnExit action.");
            this.Runtime.Assert(!this.IsPendingTransition, $"Machine '{this.Id}' has " +
                "called multiple Raise, Goto or Pop in the same action.");
            this.IsPendingTransition = true;
        }

        /// <summary>
        /// Asserts that a transition method has not already been invoked.
        /// Transition methods include raise, goto and pop.
        /// </summary>
        /// <param name="callee">Callee</param>
        internal void AssertNoPendingTransitionInvocation(string callee)
        {
            this.Runtime.Assert(!this.IsPendingTransition, $"Machine '{this.Id}' cannot " +
                $"call '{callee}' after calling Raise, Goto or Pop in the same action.");
        }

        #endregion

        #region Code Coverage Methods

        /// <summary>
        /// Returns the set of all states in the machine
        /// (for code coverage).
        /// </summary>
        /// <returns>Set of all states in the machine</returns>
        internal virtual HashSet<string> GetAllStates()
        {
            return new HashSet<string>();
        }

        /// <summary>
        /// Returns the set of all (states, registered event) pairs in the machine
        /// (for code coverage).
        /// </summary>
        /// <returns>Set of all (states, registered event) pairs in the machine</returns>
        internal virtual HashSet<Tuple<string, string>> GetAllStateEventPairs()
        {
            return new HashSet<Tuple<string, string>>();
        }

        #endregion
    }
}
