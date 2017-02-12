﻿using System;
using System.Threading.Tasks;

using Microsoft.PSharp;

namespace Raft
{
    internal class ElectionTimer : Machine
    {
        internal class ConfigureEvent : Event
        {
            public MachineId Target;

            public ConfigureEvent(MachineId id)
                : base()
            {
                this.Target = id;
            }
        }

        internal class StartTimer : Event { }
        internal class CancelTimer : Event { }
        internal class Timeout : Event { }

        private class TickEvent : Event { }

        MachineId Target;

        [Start]
        [OnEventDoAction(typeof(ConfigureEvent), nameof(Configure))]
        [OnEventGotoState(typeof(StartTimer), typeof(Active))]
        class Init : MachineState { }

        async Task Configure()
        {
            this.Target = (this.ReceivedEvent as ConfigureEvent).Target;
			//await this.Raise(new StartTimer());
			await this.DoneTask;
        }

        [OnEntry(nameof(ActiveOnEntry))]
        [OnEventDoAction(typeof(TickEvent), nameof(Tick))]
        [OnEventGotoState(typeof(CancelTimer), typeof(Inactive))]
        [IgnoreEvents(typeof(StartTimer))]
        class Active : MachineState { }

        async Task ActiveOnEntry()
        {
            await this.Send(this.Id, new TickEvent());
        }

        async Task Tick()
        {
            if (this.Random())
            {
                Console.WriteLine("\n [ElectionTimer] " + this.Target + " | timed out\n");
                await this.Send(this.Target, new Timeout());
            }

            //await this.Send(this.Id, new TickEvent());
            await this.Raise(new CancelTimer());
        }

        [OnEventGotoState(typeof(StartTimer), typeof(Active))]
        [IgnoreEvents(typeof(CancelTimer), typeof(TickEvent))]
        class Inactive : MachineState { }
    }
}
