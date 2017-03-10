﻿//-----------------------------------------------------------------------
// <copyright file="BugFindingScheduler.cs">
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.PSharp.Utilities;

namespace Microsoft.PSharp.TestingServices.Scheduling
{
    /// <summary>
    /// Class implementing the basic P# bug-finding scheduler.
    /// </summary>
    internal sealed class BugFindingScheduler
    {
        #region fields

        /// <summary>
        /// The P# runtime.
        /// </summary>
        private readonly BugFindingRuntime Runtime;

        /// <summary>
        /// The scheduling strategy to be used for bug-finding.
        /// </summary>
        private ISchedulingStrategy Strategy;

        /// <summary>
        /// The scheduler completion source.
        /// </summary>
        private readonly TaskCompletionSource<bool> CompletionSource;

        /// <summary>
        /// Map from task ids to machine infos.
        /// </summary>
        private ConcurrentDictionary<int, MachineInfo> TaskMap;

        /// <summary>
        /// Checks if the scheduler is running.
        /// </summary>
        private bool IsSchedulerRunning;

        #endregion

        #region properties

        /// <summary>
        /// Info of the currently scheduled machine.
        /// </summary>
        internal MachineInfo ScheduledMachineInfo { get; private set; }

        /// <summary>
        /// Number of explored steps.
        /// </summary>
        internal int ExploredSteps
        {
            get { return this.Strategy.GetExploredSteps(); }
        }

        /// <summary>
        /// Checks if the schedule has been fully explored.
        /// </summary>
        internal bool HasFullyExploredSchedule { get; private set; }

        /// <summary>
        /// True if a bug was found.
        /// </summary>
        internal bool BugFound { get; private set; }

        /// <summary>
        /// Bug report.
        /// </summary>
        internal string BugReport { get; private set; }

        #endregion

        #region internal methods

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="runtime">BugFindingRuntime</param>
        /// <param name="strategy">SchedulingStrategy</param>
        internal BugFindingScheduler(BugFindingRuntime runtime, ISchedulingStrategy strategy)
        {
            this.Runtime = runtime;
            this.Strategy = strategy;
            this.CompletionSource = new TaskCompletionSource<bool>();
            this.TaskMap = new ConcurrentDictionary<int, MachineInfo>();
            this.IsSchedulerRunning = true;
            this.BugFound = false;
            this.HasFullyExploredSchedule = false;
        }

        /// <summary>
        /// Schedules the next machine to execute.
        /// </summary>
        internal void Schedule()
        {
            int? taskId = Task.CurrentId;

            // If the caller is the root task, then return.
            if (taskId != null && taskId == this.Runtime.RootTaskId)
            {
                return;
            }

            // Checks if synchronisation not controlled by P# was used.
            if (taskId == null)
            {
                string message = IO.Format("Detected synchronization context " +
                    "that is not controlled by the P# runtime.");
                this.NotifyAssertionFailure(message, true);
            }

            //if (taskId == null || taskId == this.Runtime.RootTaskId)
            //{
            //    return;
            //}

            if (!this.IsSchedulerRunning)
            {
                this.Stop();
            }

            // Checks if the scheduling steps bound has been reached.
            this.CheckIfSchedulingStepsBoundIsReached(false);

            MachineInfo machineInfo = null;
            if (!this.TaskMap.TryGetValue((int)taskId, out machineInfo))
            {
                IO.Debug($"<ScheduleDebug> Unable to schedule task '{taskId}'.");
                this.Stop();
            }

            MachineInfo next = null;
            var choices = this.TaskMap.Values.OrderBy(mi => mi.Machine.Id.Value);
            if (!this.Strategy.TryGetNext(out next, choices, machineInfo))
            {
                IO.Debug("<ScheduleDebug> Schedule explored.");
                this.HasFullyExploredSchedule = true;
                this.Stop();
            }

            this.ScheduledMachineInfo = next;

            this.Runtime.ScheduleTrace.AddSchedulingChoice(next.Machine.Id);
            next.Machine.ProgramCounter = 0;

            if (this.Runtime.Configuration.CacheProgramState &&
                this.Runtime.Configuration.SafetyPrefixBound <= this.ExploredSteps)
            {
                this.Runtime.StateCache.CaptureState(this.Runtime.ScheduleTrace.Peek());
            }

            // Checks the liveness monitors for potential liveness bugs.
            this.Runtime.LivenessChecker.CheckLivenessAtShedulingStep();

            IO.Debug($"<ScheduleDebug> Schedule task '{next.Id}' of machine " +
                $"'{next.Machine.Id}'.");

            if (next.IsWaitingToReceive)
            {
                string message = IO.Format("Detected livelock. Machine " +
                    $"'{next.Machine.Id}' is waiting for an event, " +
                    "but no other machine is enabled.");
                this.NotifyAssertionFailure(message, true);
            }

            if (machineInfo != next)
            {
                machineInfo.IsActive = false;
                lock (next)
                {
                    next.IsActive = true;
                    System.Threading.Monitor.PulseAll(next);
                }
                
                lock (machineInfo)
                {
                    if (machineInfo.IsCompleted)
                    {
                        return;
                    }

                    while (!machineInfo.IsActive)
                    {
                        IO.Debug($"<ScheduleDebug> Sleep task '{machineInfo.Id}' of machine " +
                            $"'{machineInfo.Machine.Id}'.");
                        System.Threading.Monitor.Wait(machineInfo);
                        IO.Debug($"<ScheduleDebug> Wake up task '{machineInfo.Id}' of machine " +
                            $"'{machineInfo.Machine.Id}'.");
                    }

                    if (!machineInfo.IsEnabled)
                    {
                        throw new MachineCanceledException();
                    }
                }
            }
        }

        /// <summary>
        /// Returns the next nondeterministic boolean choice.
        /// </summary>
        /// <param name="maxValue">Max value</param>
        /// <param name="uniqueId">Unique id</param>
        /// <returns>Boolean</returns>
        internal bool GetNextNondeterministicBooleanChoice(
            int maxValue, string uniqueId = null)
        {
            // Checks if the scheduling steps bound has been reached.
            this.CheckIfSchedulingStepsBoundIsReached(false);

            var choice = false;
            if (!this.Strategy.GetNextBooleanChoice(maxValue, out choice))
            {
                IO.Debug("<ScheduleDebug> Schedule explored.");
                this.Stop();
            }

            if (uniqueId == null)
            {
                this.Runtime.ScheduleTrace.AddNondeterministicBooleanChoice(choice);
            }
            else
            {
                this.Runtime.ScheduleTrace.AddFairNondeterministicBooleanChoice(uniqueId, choice);
            }

            foreach(var m in TaskMap.Values)
            {
                if (m.IsActive)
                {
                    m.Machine.ProgramCounter++;
                    break;
                }
            }
            
            if (this.Runtime.Configuration.CacheProgramState &&
                this.Runtime.Configuration.SafetyPrefixBound <= this.ExploredSteps)
            {
                this.Runtime.StateCache.CaptureState(this.Runtime.ScheduleTrace.Peek());
            }

            // Checks the liveness monitors for potential liveness bugs.
            this.Runtime.LivenessChecker.CheckLivenessAtShedulingStep();
            
            return choice;
        }

        /// <summary>
        /// Returns the next nondeterministic integer choice.
        /// </summary>
        /// <param name="maxValue">Max value</param>
        /// <returns>Integer</returns>
        internal int GetNextNondeterministicIntegerChoice(int maxValue)
        {
            // Checks if the scheduling steps bound has been reached.
            this.CheckIfSchedulingStepsBoundIsReached(false);

            var choice = 0;
            if (!this.Strategy.GetNextIntegerChoice(maxValue, out choice))
            {
                IO.Debug("<ScheduleDebug> Schedule explored.");
                this.Stop();
            }

            this.Runtime.ScheduleTrace.AddNondeterministicIntegerChoice(choice);
            
            if (this.Runtime.Configuration.CacheProgramState &&
                this.Runtime.Configuration.SafetyPrefixBound <= this.ExploredSteps)
            {
                this.Runtime.StateCache.CaptureState(this.Runtime.ScheduleTrace.Peek());
            }

            // Checks the liveness monitors for potential liveness bugs.
            this.Runtime.LivenessChecker.CheckLivenessAtShedulingStep();

            return choice;
        }

        /// <summary>
        /// Returns the enabled machines.
        /// </summary>
        /// <returns>Enabled machines</returns>
        internal HashSet<MachineId> GetEnabledMachines()
        {
            var enabledMachines = new HashSet<MachineId>();
            foreach (MachineInfo machineInfo in this.TaskMap.Values)
            {
                if (machineInfo.IsEnabled && !machineInfo.IsWaitingToReceive)
                {
                    enabledMachines.Add(machineInfo.Machine.Id);
                }
            }

            return enabledMachines;
        }

        /// <summary>
        /// Notify that a new task has been created for the given machine.
        /// </summary>
        /// <param name="taskId">Task id</param>
        /// <param name="machine">Machine</param>
        internal void NotifyNewTaskCreated(int taskId, AbstractMachine machine)
        {
            MachineInfo machineInfo = new MachineInfo(taskId, machine);

            IO.Debug($"<ScheduleDebug> Created task '{machineInfo.Id}' for machine " +
                $"'{machineInfo.Machine.Id}'.");

            if (this.TaskMap.Count == 0)
            {
                machineInfo.IsActive = true;
            }

            this.TaskMap.TryAdd(taskId, machineInfo);
        }

        /// <summary>
        /// Notify that the task has started.
        /// </summary>
        /// <param name="taskId">Task id</param>
        internal void NotifyTaskStarted(int taskId)
        {
            MachineInfo machineInfo = this.TaskMap[taskId];

            IO.Debug($"<ScheduleDebug> Started task '{machineInfo.Id}' of machine " +
                $"'{machineInfo.Machine.Id}'.");

            lock (machineInfo)
            {
                machineInfo.HasStarted = true;
                System.Threading.Monitor.PulseAll(machineInfo);
                while (!machineInfo.IsActive)
                {
                    IO.Debug($"<ScheduleDebug> Sleep task '{machineInfo.Id}' of machine " +
                        $"'{machineInfo.Machine.Id}'.");
                    System.Threading.Monitor.Wait(machineInfo);
                    IO.Debug($"<ScheduleDebug> Wake up task '{machineInfo.Id}' of machine " +
                        $"'{machineInfo.Machine.Id}'.");
                }

                if (!machineInfo.IsEnabled)
                {
                    throw new MachineCanceledException();
                }
            }
        }

        /// <summary>
        /// Notify that the task is waiting to receive an event.
        /// </summary>
        /// <param name="taskId">Task id</param>
        internal void NotifyTaskBlockedOnEvent(int taskId)
        {
            MachineInfo machineInfo = this.TaskMap[taskId];
            machineInfo.IsWaitingToReceive = true;

            IO.Debug($"<ScheduleDebug> Task '{machineInfo.Id}' of machine " +
                $"'{machineInfo.Machine.Id}' is waiting to receive an event.");
        }

        /// <summary>
        /// Notify that the machine received an event that it was waiting for.
        /// </summary>
        /// <param name="machine">Machine</param>
        internal void NotifyTaskReceivedEvent(AbstractMachine machine)
        {
            MachineInfo machineInfo = this.TaskMap.Values.First(mi => mi.Machine.Equals(machine) && !mi.IsCompleted);
            machineInfo.IsWaitingToReceive = false;

            IO.Debug($"<ScheduleDebug> Task '{machineInfo.Id}' of machine " +
                $"'{machineInfo.Machine.Id}' received an event and unblocked.");
        }

        /// <summary>
        /// Notify that the task of the scheduled machine changed.
        /// This can occur if a machine is using async/await.
        /// </summary>
        /// <param name="taskId">Task id</param>
        internal void NotifyScheduledMachineTaskChanged(int taskId)
        {
            MachineInfo parentInfo = this.ScheduledMachineInfo;

            Console.WriteLine(">>> NotifyScheduledMachineTaskChanged from " + parentInfo.Id + " to " + taskId);

            parentInfo.IsEnabled = false;
            parentInfo.IsCompleted = true;

            this.TaskMap.TryRemove(parentInfo.Id, out parentInfo);

            this.ScheduledMachineInfo = this.TaskMap[taskId];
            this.ScheduledMachineInfo.HasStarted = true;

            //MachineInfo machineInfo = this.TaskMap[(int)id];

            //IO.Debug($"<ScheduleDebug> Completed task '{machineInfo.Id}' of machine " +
            //    $"'{machineInfo.Machine.Id}'.");

            //machineInfo.IsEnabled = false;
            //machineInfo.IsCompleted = true;

            //this.Schedule();

            //IO.Debug($"<ScheduleDebug> Exit task '{machineInfo.Id}' of machine " +
            //    $"'{machineInfo.Machine.Id}'.");

            //this.TaskMap.TryRemove((int)id, out machineInfo);
        }

        /// <summary>
        /// Notify that the task has completed.
        /// </summary>
        /// <param name="taskId">Task id</param>
        internal void NotifyTaskCompleted(int taskId)
        {
            MachineInfo machineInfo = this.TaskMap[taskId];

            IO.Debug($"<ScheduleDebug> Completed task '{machineInfo.Id}' of machine " +
                $"'{machineInfo.Machine.Id}'.");

            machineInfo.IsEnabled = false;
            machineInfo.IsCompleted = true;

            this.Schedule();

            IO.Debug($"<ScheduleDebug> Exit task '{machineInfo.Id}' of machine " +
                $"'{machineInfo.Machine.Id}'.");

            this.TaskMap.TryRemove(taskId, out machineInfo);
        }

        /// <summary>
        /// Wait for the task to start.
        /// </summary>
        /// <param name="taskId">Task id</param>
        internal void WaitForTaskToStart(int taskId)
        {
            Console.WriteLine("WaitForTaskToStart: " + taskId);
            MachineInfo machineInfo = this.TaskMap[taskId];
            lock (machineInfo)
            {
                while (!machineInfo.HasStarted)
                {
                    System.Threading.Monitor.Wait(machineInfo);
                }
            }
        }

        /// <summary>
        /// Checks if there is already an enabled task for the given machine.
        /// </summary>
        /// <param name="machine">Machine</param>
        /// <returns>Boolean</returns>
        internal bool HasEnabledTaskForMachine(AbstractMachine machine)
        {
            var enabledTasks = this.TaskMap.Values.Where(machineInfo => machineInfo.IsEnabled).ToList();
            return enabledTasks.Any(machineInfo => machineInfo.Machine.Equals(machine));
        }

        /// <summary>
        /// Notify that an assertion has failed.
        /// </summary>
        /// <param name="text">Bug report</param>
        /// <param name="killTasks">Kill tasks</param>
        internal void NotifyAssertionFailure(string text, bool killTasks = true)
        {
            if (!this.BugFound)
            {
                this.BugReport = text;

                ErrorReporter.Report(text);

                IO.Log("<StrategyLog> Found bug using " +
                    $"'{this.Runtime.Configuration.SchedulingStrategy}' strategy.");

                if (this.Strategy.GetDescription().Length > 0)
                {
                    IO.Log($"<StrategyLog> {this.Strategy.GetDescription()}");
                }

                this.BugFound = true;

                if (this.Runtime.Configuration.AttachDebugger)
                {
                    System.Diagnostics.Debugger.Break();
                }
            }

            if (killTasks)
            {
                this.Stop();
            }
        }

        /// <summary>
        /// Switches the scheduler to the specified scheduling strategy,
        /// and returns the previously installed strategy.
        /// </summary>
        /// <param name="strategy">ISchedulingStrategy</param>
        /// <returns>ISchedulingStrategy</returns>
        internal ISchedulingStrategy SwitchSchedulingStrategy(ISchedulingStrategy strategy)
        {
            ISchedulingStrategy previous = this.Strategy;
            this.Strategy = strategy;
            return previous;
        }

        /// <summary>
        /// Stops the scheduler.
        /// </summary>
        internal void Stop()
        {
            this.IsSchedulerRunning = false;
            this.ScheduledMachineInfo = null;
            this.KillRemainingMachines();

            // Check if the completion source is completed. If not synchronize on
            // it (as it can only be set once) and set its result.
            if (!this.CompletionSource.Task.IsCompleted)
            {
                lock (this.CompletionSource)
                {
                    if (!this.CompletionSource.Task.IsCompleted)
                    {
                        this.CompletionSource.SetResult(true);
                    }
                }
            }

            throw new MachineCanceledException();
        }

        /// <summary>
        /// Waits the scheduler to terminate.
        /// </summary>
        internal void Wait()
        {
            this.CompletionSource.Task.Wait();
        }

        /// <summary>
        /// Returns a test report with the scheduling statistics.
        /// </summary>
        /// <returns>TestReport</returns>
        internal TestReport GetReport()
        {
            TestReport report = new TestReport(this.Runtime.Configuration);

            if (this.BugFound)
            {
                report.NumOfFoundBugs++;
                report.BugReports.Add(this.BugReport);
            }

            if (this.Strategy.IsFair())
            {
                report.NumOfExploredFairSchedules++;

                if (this.Strategy.HasReachedMaxSchedulingSteps())
                {
                    report.MaxFairStepsHitInFairTests++;
                }

                if (this.ExploredSteps >= report.Configuration.MaxUnfairSchedulingSteps)
                {
                    report.MaxUnfairStepsHitInFairTests++;
                }

                if (!this.Strategy.HasReachedMaxSchedulingSteps())
                {
                    report.TotalExploredFairSteps += this.ExploredSteps;

                    if (report.MinExploredFairSteps < 0 ||
                        report.MinExploredFairSteps > this.ExploredSteps)
                    {
                        report.MinExploredFairSteps = this.ExploredSteps;
                    }

                    if (report.MaxExploredFairSteps < this.ExploredSteps)
                    {
                        report.MaxExploredFairSteps = this.ExploredSteps;
                    }
                }
            }
            else
            {
                report.NumOfExploredUnfairSchedules++;

                if (this.Strategy.HasReachedMaxSchedulingSteps())
                {
                    report.MaxUnfairStepsHitInUnfairTests++;
                }
            }

            return report;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Returns the number of available machines to schedule.
        /// </summary>
        /// <returns>Int</returns>
        private int NumberOfAvailableMachinesToSchedule()
        {
            var availableMachines = this.TaskMap.Values.Where(
                m => m.IsEnabled && !m.IsBlocked && !m.IsWaitingToReceive).ToList();
            return availableMachines.Count;
        }

        /// <summary>
        /// Checks if the scheduling steps bound has been reached.
        /// If yes, it stops the scheduler and kills all enabled
        /// machines.
        /// </summary>
        /// <param name="isSchedulingDecision">Is a machine scheduling decision</param>
        private void CheckIfSchedulingStepsBoundIsReached(bool isSchedulingDecision)
        {
            if (this.Strategy.HasReachedMaxSchedulingSteps())
            {
                var msg = IO.Format("Scheduling steps bound of {0} reached.",
                    this.Strategy.IsFair() ? this.Runtime.Configuration.MaxFairSchedulingSteps :
                    this.Runtime.Configuration.MaxUnfairSchedulingSteps);

                if (isSchedulingDecision &&
                    this.NumberOfAvailableMachinesToSchedule() == 0)
                {
                    this.HasFullyExploredSchedule = true;
                }

                if (this.Runtime.Configuration.ConsiderDepthBoundHitAsBug)
                {
                    this.NotifyAssertionFailure(msg, true);
                }
                else
                {
                    IO.Debug($"<ScheduleDebug> {msg}");
                    this.Stop();
                }
            }
        }

        /// <summary>
        /// Kills any remaining machines at the end of the schedule.
        /// </summary>
        private void KillRemainingMachines()
        {
            foreach (MachineInfo machineInfo in this.TaskMap.Values)
            {
                machineInfo.IsActive = true;
                machineInfo.IsEnabled = false;

                if (!machineInfo.IsCompleted)
                {
                    lock (machineInfo)
                    {
                        System.Threading.Monitor.PulseAll(machineInfo);
                    }
                }
            }
        }

        #endregion
    }
}
