//-----------------------------------------------------------------------
// <copyright file="SingletonSynchronousRuntime.cs">
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
using System.Reflection;

namespace Microsoft.PSharp
{
    /// <summary>
    /// Runtime for executing a single synchronous state-machine in production.
    /// </summary>
    internal sealed class SingletonSynchronousRuntime : PSharpRuntime
    {
        #region fields

        /// <summary>
        /// The machine executed by this runtime.
        /// </summary>
        private Machine Machine;

        #endregion

        #region initialization

        /// <summary>
        /// Constructor.
        /// </summary>
        internal SingletonSynchronousRuntime()
            : base() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="configuration">Configuration</param>
        internal SingletonSynchronousRuntime(Configuration configuration)
            : base(configuration) { }

        #endregion

        #region runtime interface

        /// <summary>
        /// Creates a new machine of the specified <see cref="Type"/> and with
        /// the specified optional <see cref="Event"/>. This event can only be
        /// used to access its payload, and cannot be handled.
        /// </summary>
        /// <param name="type">Type of the machine</param>
        /// <param name="e">Event</param>
        /// <returns>MachineId</returns>
        public override MachineId CreateMachine(Type type, Event e = null)
        {
            return this.TryCreateMachine(null, type, null, e);
        }

        /// <summary>
        /// Creates a new machine of the specified <see cref="Type"/> and name, and
        /// with the specified optional <see cref="Event"/>. This event can only be
        /// used to access its payload, and cannot be handled.
        /// </summary>
        /// <param name="type">Type of the machine</param>
        /// <param name="friendlyName">Friendly machine name used for logging</param>
        /// <param name="e">Event</param>
        /// <returns>MachineId</returns>
        public override MachineId CreateMachine(Type type, string friendlyName, Event e = null)
        {
            return this.TryCreateMachine(null, type, friendlyName, e);
        }

        /// <summary>
        /// Creates a new remote machine of the specified <see cref="Type"/> and with
        /// the specified optional <see cref="Event"/>. This event can only be used
        /// to access its payload, and cannot be handled.
        /// </summary>
        /// <param name="type">Type of the machine</param>
        /// <param name="endpoint">Endpoint</param>
        /// <param name="e">Event</param>
        /// <returns>MachineId</returns>
        public override MachineId RemoteCreateMachine(Type type, string endpoint, Event e = null)
        {
            return this.TryCreateRemoteMachine(null, type, null, endpoint, e);
        }

        /// <summary>
        /// Creates a new remote machine of the specified <see cref="Type"/> and name, and
        /// with the specified optional <see cref="Event"/>. This event can only be used
        /// to access its payload, and cannot be handled.
        /// </summary>
        /// <param name="type">Type of the machine</param>
        /// <param name="friendlyName">Friendly machine name used for logging</param>
        /// <param name="endpoint">Endpoint</param>
        /// <param name="e">Event</param>
        /// <returns>MachineId</returns>
        public override MachineId RemoteCreateMachine(Type type, string friendlyName,
            string endpoint, Event e = null)
        {
            return this.TryCreateRemoteMachine(null, type, friendlyName, endpoint, e);
        }

        /// <summary>
        /// Sends an asynchronous <see cref="Event"/> to a machine.
        /// </summary>
        /// <param name="target">Target machine id</param>
        /// <param name="e">Event</param>
        public override void SendEvent(MachineId target, Event e)
        {
            // If the target machine is null then report an error and exit.
            this.Assert(target != null, "Cannot send to a null machine.");
            // If the event is null then report an error and exit.
            this.Assert(e != null, "Cannot send a null event.");
            this.Send(null, target, e, false);
        }

        /// <summary>
        /// Sends an asynchronous <see cref="Event"/> to a remote machine.
        /// </summary>
        /// <param name="target">Target machine id</param>
        /// <param name="e">Event</param>
        public override void RemoteSendEvent(MachineId target, Event e)
        {
            // If the target machine is null then report an error and exit.
            this.Assert(target != null, "Cannot send to a null machine.");
            // If the event is null then report an error and exit.
            this.Assert(e != null, "Cannot send a null event.");
            this.SendRemotely(null, target, e, false);
        }

        /// <summary>
        /// Waits to receive an <see cref="Event"/> of the specified types.
        /// </summary>
        /// <param name="eventTypes">Event types</param>
        /// <returns>Received event</returns>
        public override Event Receive(params Type[] eventTypes)
        {
            throw new NotSupportedException("This runtime does not support 'Receive'.");
        }

        /// <summary>
        /// Waits to receive an <see cref="Event"/> of the specified type
        /// that satisfies the specified predicate.
        /// </summary>
        /// <param name="eventType">Event type</param>
        /// <param name="predicate">Predicate</param>
        /// <returns>Received event</returns>
        public override Event Receive(Type eventType, Func<Event, bool> predicate)
        {
            throw new NotSupportedException("This runtime does not support 'Receive'.");
        }

        /// <summary>
        /// Waits to receive an <see cref="Event"/> of the specified types
        /// that satisfy the specified predicates.
        /// </summary>
        /// <param name="events">Event types and predicates</param>
        /// <returns>Received event</returns>
        public override Event Receive(params Tuple<Type, Func<Event, bool>>[] events)
        {
            throw new NotSupportedException("This runtime does not support 'Receive'.");
        }

        /// <summary>
        /// Registers a new specification monitor of the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="type">Type of the monitor</param>
        public override void RegisterMonitor(Type type)
        {
            throw new NotSupportedException("This runtime does not support monitors.");
        }

        /// <summary>
        /// Invokes the specified monitor with the specified <see cref="Event"/>.
        /// </summary>
        /// <typeparam name="T">Type of the monitor</typeparam>
        /// <param name="e">Event</param>
        public override void InvokeMonitor<T>(Event e)
        {
            throw new NotSupportedException("This runtime does not support monitors.");
        }

        /// <summary>
        /// Gets the id of the currently executing <see cref="Machine"/>.
        /// <returns>MachineId</returns>
        /// </summary>
        public override MachineId GetCurrentMachineId()
        {
            return Machine.Id;
        }

        /// <summary>
        /// Waits until all P# machines have finished execution.
        /// </summary>
        public override void Wait()
        {
            throw new NotSupportedException("This runtime does not support 'Wait'.");
        }

        #endregion

        #region state-machine execution

        /// <summary>
        /// Tries to create a new <see cref="Machine"/> of the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="creator">Creator machine</param>
        /// <param name="type">Type of the machine</param>
        /// <param name="friendlyName">Friendly machine name used for logging</param>
        /// <param name="e">Event</param>
        /// <returns>MachineId</returns>
        internal override MachineId TryCreateMachine(Machine creator, Type type, string friendlyName, Event e)
        {
            this.Assert(this.Machine == null, "This runtime already hosts a machine.");
            this.Assert(type.IsSubclassOf(typeof(Machine)), $"Type '{type.Name}' is not a machine.");

            MachineId mid = new MachineId(type, friendlyName, this);
            Machine machine = MachineFactory.Create(type);

            machine.SetMachineId(mid);
            machine.InitializeStateInformation();

            this.Machine = machine;

            this.Log($"<CreateLog> Machine '{mid}' is created.");

            this.Machine.GotoStartState(e);
            this.Machine.RunEventHandler();

            return mid;
        }

        /// <summary>
        /// Tries to create a new remote <see cref="Machine"/> of the specified <see cref="System.Type"/>.
        /// </summary>
        /// <param name="creator">Creator machine</param>
        /// <param name="type">Type of the machine</param>
        /// <param name="friendlyName">Friendly machine name used for logging</param>
        /// <param name="endpoint">Endpoint</param>
        /// <param name="e">Event</param>
        /// <returns>MachineId</returns>
        internal override MachineId TryCreateRemoteMachine(Machine creator, Type type,
            string friendlyName, string endpoint, Event e)
        {
            this.Assert(type.IsSubclassOf(typeof(Machine)),
                $"Type '{type.Name}' is not a machine.");
            return base.NetworkProvider.RemoteCreateMachine(type, friendlyName, endpoint, e);
        }

        /// <summary>
        /// Sends an asynchronous <see cref="Event"/> to a machine.
        /// </summary>
        /// <param name="sender">Sender machine</param>
        /// <param name="mid">MachineId</param>
        /// <param name="e">Event</param>
        /// <param name="isStarter">Is starting a new operation</param>
        internal override void Send(AbstractMachine sender, MachineId mid, Event e, bool isStarter)
        {
            this.Assert(this.Machine.Id.Equals(mid), $"This runtime does not host '{mid}'.");

            EventInfo eventInfo = new EventInfo(e, null);

            if (sender != null)
            {
                this.Log($"<SendLog> Machine '{sender.Id}' sent event " +
                    $"'{eventInfo.EventName}' to '{mid}'.");
            }
            else
            {
                this.Log($"<SendLog> Event '{eventInfo.EventName}' was sent to '{mid}'.");
            }

            bool runNewHandler = false;
            this.Machine.Enqueue(eventInfo, ref runNewHandler);
            if (runNewHandler)
            {
                this.Machine.RunEventHandler();
            }
        }

        /// <summary>
        /// Sends an asynchronous <see cref="Event"/> to a remote machine.
        /// </summary>
        /// <param name="sender">Sender machine</param>
        /// <param name="mid">MachineId</param>
        /// <param name="e">Event</param>
        /// <param name="isStarter">Is starting a new operation</param>
        internal override void SendRemotely(AbstractMachine sender, MachineId mid, Event e, bool isStarter)
        {
            base.NetworkProvider.RemoteSend(mid, e);
        }

        #endregion

        #region specifications and error checking

        /// <summary>
        /// Tries to create a new <see cref="PSharp.Monitor"/> of the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="type">Type of the monitor</param>
        internal override void TryCreateMonitor(Type type)
        {
            throw new NotSupportedException("This runtime does not support monitors.");
        }

        /// <summary>
        /// Invokes the specified <see cref="PSharp.Monitor"/> with the specified <see cref="Event"/>.
        /// </summary>
        /// <param name="sender">Sender machine</param>
        /// <typeparam name="T">Type of the monitor</typeparam>
        /// <param name="e">Event</param>
        internal override void Monitor<T>(AbstractMachine sender, Event e)
        {
            throw new NotSupportedException("This runtime does not support monitors.");
        }

        #endregion

        #region nondeterministic choices

        /// <summary>
        /// Returns a nondeterministic boolean choice, that can be
        /// controlled during analysis or testing.
        /// </summary>
        /// <param name="machine">Machine</param>
        /// <param name="maxValue">Max value</param>
        /// <returns>Boolean</returns>
        internal override bool GetNondeterministicBooleanChoice(AbstractMachine machine, int maxValue)
        {
            Random random = new Random(DateTime.Now.Millisecond);

            bool result = false;
            if (random.Next(maxValue) == 0)
            {
                result = true;
            }

            if (machine != null)
            {
                this.Log($"<RandomLog> Machine '{machine.Id}' " +
                    $"nondeterministically chose '{result}'.");
            }
            else
            {
                this.Log($"<RandomLog> Runtime nondeterministically chose '{result}'.");
            }

            return result;
        }

        /// <summary>
        /// Returns a fair nondeterministic boolean choice, that can be
        /// controlled during analysis or testing.
        /// </summary>
        /// <param name="machine">Machine</param>
        /// <param name="uniqueId">Unique id</param>
        /// <returns>Boolean</returns>
        internal override bool GetFairNondeterministicBooleanChoice(AbstractMachine machine, string uniqueId)
        {
            return this.GetNondeterministicBooleanChoice(machine, 2);
        }

        /// <summary>
        /// Returns a nondeterministic integer choice, that can be
        /// controlled during analysis or testing.
        /// </summary>
        /// <param name="machine">Machine</param>
        /// <param name="maxValue">Max value</param>
        /// <returns>Integer</returns>
        internal override int GetNondeterministicIntegerChoice(AbstractMachine machine, int maxValue)
        {
            Random random = new Random(DateTime.Now.Millisecond);
            var result = random.Next(maxValue);

            if (machine != null)
            {
                this.Log($"<RandomLog> Machine '{machine.Id}' " +
                    $"nondeterministically chose '{result}'.");
            }
            else
            {
                this.Log($"<RandomLog> Runtime nondeterministically chose '{result}'.");
            }

            return result;
        }

        #endregion

        #region notifications

        /// <summary>
        /// Notifies that a machine entered a state.
        /// </summary>
        /// <param name="machine">AbstractMachine</param>
        internal override void NotifyEnteredState(AbstractMachine machine)
        {
            if (base.Configuration.Verbose <= 1)
            {
                return;
            }

            string machineState = (machine as Machine).CurrentStateName;

            this.Log($"<StateLog> Machine '{machine.Id}' enters " +
                $"state '{machineState}'.");
        }

        /// <summary>
        /// Notifies that a machine exited a state.
        /// </summary>
        /// <param name="machine">AbstractMachine</param>
        internal override void NotifyExitedState(AbstractMachine machine)
        {
            if (base.Configuration.Verbose <= 1)
            {
                return;
            }

            string machineState = (machine as Machine).CurrentStateName;

            this.Log($"<StateLog> Machine '{machine.Id}' exits " +
                $"state '{machineState}'.");
        }

        /// <summary>
        /// Notifies that a machine invoked an action.
        /// </summary>
        /// <param name="machine">AbstractMachine</param>
        /// <param name="action">Action</param>
        /// <param name="receivedEvent">Event</param>
        internal override void NotifyInvokedAction(AbstractMachine machine, MethodInfo action, Event receivedEvent)
        {
            if (base.Configuration.Verbose <= 1)
            {
                return;
            }

            string machineState = (machine as Machine).CurrentStateName;

            this.Log($"<ActionLog> Machine '{machine.Id}' invoked action " +
                $"'{action.Name}' in state '{machineState}'.");
        }

        /// <summary>
        /// Notifies that a machine dequeued an <see cref="Event"/>.
        /// </summary>
        /// <param name="machine">Machine</param>
        /// <param name="eventInfo">EventInfo</param>
        internal override void NotifyDequeuedEvent(Machine machine, EventInfo eventInfo)
        {
            this.Log($"<DequeueLog> Machine '{machine.Id}' dequeued " +
                $"event '{eventInfo.EventName}'.");
        }

        /// <summary>
        /// Notifies that a machine raised an <see cref="Event"/>.
        /// </summary>
        /// <param name="machine">AbstractMachine</param>
        /// <param name="eventInfo">EventInfo</param>
        /// <param name="isStarter">Is starting a new operation</param>
        internal override void NotifyRaisedEvent(AbstractMachine machine, EventInfo eventInfo, bool isStarter)
        {
            if (base.Configuration.Verbose <= 1)
            {
                return;
            }

            string machineState = (machine as Machine).CurrentStateName;
            this.Log($"<RaiseLog> Machine '{machine.Id}' raised " +
                $"event '{eventInfo.EventName}'.");
        }

        /// <summary>
        /// Notifies that a machine is waiting to receive one or more events.
        /// </summary>
        /// <param name="machine">Machine</param>
        /// <param name="events">Events</param>
        internal override void NotifyWaitEvents(Machine machine, string events)
        {
            this.Log($"<ReceiveLog> Machine '{machine.Id}' " +
                $"is waiting on events:{events}.");

            lock (machine)
            {
                while (machine.IsWaitingToReceive)
                {
                    System.Threading.Monitor.Wait(machine);
                }
            }
        }

        /// <summary>
        /// Notifies that a machine received an <see cref="Event"/> that it was waiting for.
        /// </summary>
        /// <param name="machine">Machine</param>
        /// <param name="eventInfo">EventInfo</param>
        internal override void NotifyReceivedEvent(Machine machine, EventInfo eventInfo)
        {
            this.Log($"<ReceiveLog> Machine '{machine.Id}' received " +
                $"event '{eventInfo.EventName}' and unblocked.");

            lock (machine)
            {
                System.Threading.Monitor.Pulse(machine);
                machine.IsWaitingToReceive = false;
            }
        }

        /// <summary>
        /// Notifies that a machine has halted.
        /// </summary>
        /// <param name="machine">Machine</param>
        internal override void NotifyHalted(Machine machine)
        {
            this.Log($"<HaltLog> Machine '{machine.Id}' halted.");
        }

        #endregion

        #region cleanup

        /// <summary>
        /// Disposes runtime resources.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
        }

        #endregion
    }
}
