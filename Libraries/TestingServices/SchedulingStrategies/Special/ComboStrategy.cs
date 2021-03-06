﻿//-----------------------------------------------------------------------
// <copyright file="ComboStrategy.cs">
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

using System.Collections.Generic;

namespace Microsoft.PSharp.TestingServices.Scheduling
{
    /// <summary>
    /// Class representing a combination of two strategies, used
    /// one after the other.
    /// </summary>
    public class ComboStrategy : ISchedulingStrategy
    {
        #region fields

        /// <summary>
        /// The configuration.
        /// </summary>
        protected Configuration Configuration;

        /// <summary>
        /// The safety prefix depth.
        /// </summary>
        private int SafetyPrefixDepth;

        /// <summary>
        /// The prefix strategy.
        /// </summary>
        private ISchedulingStrategy PrefixStrategy;

        /// <summary>
        /// The suffix strategy.
        /// </summary>
        private ISchedulingStrategy SuffixStrategy;

        #endregion

        #region public API

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="configuration">Configuration</param>
        /// <param name="prefixStrategy">Prefix strategy </param>
        /// <param name="suffixStrategy">Suffix strategy</param>
        public ComboStrategy(Configuration configuration, ISchedulingStrategy prefixStrategy, ISchedulingStrategy suffixStrategy)
        {
            this.Configuration = configuration;
            this.SafetyPrefixDepth = this.Configuration.SafetyPrefixBound == 0 ? this.Configuration.MaxUnfairSchedulingSteps
                : this.Configuration.SafetyPrefixBound;
            this.PrefixStrategy = prefixStrategy;
            this.SuffixStrategy = suffixStrategy;
        }

        /// <summary>
        /// Returns the next machine to schedule.
        /// </summary>
        /// <param name="next">Next</param>
        /// <param name="choices">Choices</param>
        /// <param name="current">Curent</param>
        /// <returns>Boolean</returns>
        public bool TryGetNext(out MachineInfo next, IEnumerable<MachineInfo> choices, MachineInfo current)
        {
            if (this.PrefixStrategy.GetExploredSteps() > this.SafetyPrefixDepth)
            {
                return this.SuffixStrategy.TryGetNext(out next, choices, current);
            }
            else
            {
                return this.PrefixStrategy.TryGetNext(out next, choices, current);
            }
        }

        /// <summary>
        /// Returns the next boolean choice.
        /// </summary>
        /// <param name="maxValue">Max value</param>
        /// <param name="next">Next</param>
        /// <returns>Boolean</returns>
        public bool GetNextBooleanChoice(int maxValue, out bool next)
        {
            if (this.PrefixStrategy.GetExploredSteps() > this.SafetyPrefixDepth)
            {
                return this.SuffixStrategy.GetNextBooleanChoice(maxValue, out next);
            }
            else
            {
                return this.PrefixStrategy.GetNextBooleanChoice(maxValue, out next);
            }
        }

        /// <summary>
        /// Returns the next integer choice.
        /// </summary>
        /// <param name="maxValue">Max value</param>
        /// <param name="next">Next</param>
        /// <returns>Boolean</returns>
        public bool GetNextIntegerChoice(int maxValue, out int next)
        {
            if (this.PrefixStrategy.GetExploredSteps() > this.SafetyPrefixDepth)
            {
                return this.SuffixStrategy.GetNextIntegerChoice(maxValue, out next);
            }
            else
            {
                return this.PrefixStrategy.GetNextIntegerChoice(maxValue, out next);
            }
        }

        /// <summary>
        /// Returns the explored steps.
        /// </summary>
        /// <returns>Explored steps</returns>
        public int GetExploredSteps()
        {
            if (this.PrefixStrategy.GetExploredSteps() > this.SafetyPrefixDepth)
            {
                return this.SuffixStrategy.GetExploredSteps() + this.SafetyPrefixDepth;
            }
            else
            {
                return this.PrefixStrategy.GetExploredSteps();
            }
        }

        /// <summary>
        /// True if the scheduling strategy has reached the max
        /// scheduling steps for the given scheduling iteration.
        /// </summary>
        /// <returns>Boolean</returns>
        public bool HasReachedMaxSchedulingSteps()
        {
            return this.SuffixStrategy.HasReachedMaxSchedulingSteps();
        }

        /// <summary>
        /// Returns true if the scheduling has finished.
        /// </summary>
        /// <returns>Boolean</returns>
        public bool HasFinished()
        {
            return this.SuffixStrategy.HasFinished() && this.PrefixStrategy.HasFinished();
        }

        /// <summary>
        /// Checks if this a fair scheduling strategy.
        /// </summary>
        /// <returns>Boolean</returns>
        public bool IsFair()
        {
            return this.SuffixStrategy.IsFair();
        }

        /// <summary>
        /// Configures the next scheduling iteration.
        /// </summary>
        public void ConfigureNextIteration()
        {
            this.PrefixStrategy.ConfigureNextIteration();
            this.SuffixStrategy.ConfigureNextIteration();
        }

        /// <summary>
        /// Resets the scheduling strategy.
        /// </summary>
        public void Reset()
        {
            this.PrefixStrategy.Reset();
            this.SuffixStrategy.Reset();
        }

        /// <summary>
        /// Returns a textual description of the scheduling strategy.
        /// </summary>
        /// <returns>String</returns>
        public string GetDescription()
        {
            return string.Format("Combo[{0},{1}]", PrefixStrategy.GetDescription(), SuffixStrategy.GetDescription());
        }

        #endregion
    }
}
