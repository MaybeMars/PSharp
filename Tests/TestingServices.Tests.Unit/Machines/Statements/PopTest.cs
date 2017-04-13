//-----------------------------------------------------------------------
// <copyright file="PopTest.cs">
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

using Xunit;

namespace Microsoft.PSharp.TestingServices.Tests.Unit
{
    public class PopTest : BaseTest
    {
        class E : Event { }

        class M : Machine
        {

            [Start]
            [OnEntry(nameof(Init))]
            public class S1 : MachineState { }

            void Init()
            {
                // unbalanced pop
                this.Pop();
            }
        }

        [Fact]
        public void TestPopState()
        {
            var test = new Action<PSharpRuntime>((r) => {
                r.CreateMachine(typeof(M), "M");
            });

            var bugReport = "Machine 'M()' popped with no matching push.";
            base.AssertFailed(test, bugReport);
        }
    }
}
