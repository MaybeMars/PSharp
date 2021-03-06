﻿//-----------------------------------------------------------------------
// <copyright file="ExceptionPropagationTest.cs">
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
using System.Threading.Tasks;

using Xunit;

namespace Microsoft.PSharp.Core.Tests.Unit
{
    public class ExceptionPropagationTest
    {
        internal class Configure : Event
        {
            public TaskCompletionSource<bool> TCS;

            public Configure(TaskCompletionSource<bool> tcs)
            {
                this.TCS = tcs;
            }
        }

        class M : Machine
        {
            [Start]
            [OnEntry(nameof(InitOnEntry))]
            class Init : MachineState { }

            void InitOnEntry()
            {
                var tcs = (this.ReceivedEvent as Configure).TCS;
                try
                {
                    this.Assert(false);
                }
                finally
                {
                    tcs.SetResult(true);
                }
            }
        }

        class N: Machine
        {
            [Start]
            [OnEntry(nameof(InitOnEntry))]
            class Init : MachineState { }

            void InitOnEntry()
            {
                var tcs = (this.ReceivedEvent as Configure).TCS;
                try
                {
                    throw new InvalidOperationException();
                }
                finally
                {
                    tcs.SetResult(true);
                }
            }
        }

        [Fact]
        public void TestAssertFailureNoEventHandler()
        {
            PSharpRuntime runtime = PSharpRuntime.Create();
            var tcs = new TaskCompletionSource<bool>();
            runtime.CreateMachine(typeof(M), new Configure(tcs));
            tcs.Task.Wait();
        }

        [Fact]
        public void TestAssertFailureEventHandler()
        {
            var tcsFail = new TaskCompletionSource<bool>();

            PSharpRuntime runtime = PSharpRuntime.Create();
            runtime.OnFailure += delegate (Exception exception)
            {
                tcsFail.SetException(exception);
            };

            var tcs = new TaskCompletionSource<bool>();
            runtime.CreateMachine(typeof(M), new Configure(tcs));
            tcs.Task.Wait();

            AggregateException ex = Assert.Throws<AggregateException>(() => tcsFail.Task.Wait());
            Assert.IsType<AssertionFailureException>(ex.InnerException);
        }

        [Fact]
        public void TestUnhandledExceptionEventHandler()
        {
            var tcsFail = new TaskCompletionSource<bool>();

            PSharpRuntime runtime = PSharpRuntime.Create();
            runtime.OnFailure += delegate (Exception exception)
            {
                tcsFail.SetException(exception.InnerException);
            };

            var tcs = new TaskCompletionSource<bool>();
            runtime.CreateMachine(typeof(N), new Configure(tcs));
            tcs.Task.Wait();

            AggregateException ex = Assert.Throws<AggregateException>(() => tcsFail.Task.Wait());
            Assert.IsType<InvalidOperationException>(ex.InnerException);
        }
    }
}
