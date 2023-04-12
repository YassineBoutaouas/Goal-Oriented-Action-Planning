using System;
using System.Collections.Generic;

namespace GOAP_Nez_Deprecated
{

    public class GOAPAction
    {
        //Name for debugging reasons - could just be class type
        public string Name;

        /// <summary>
        /// The cost for performing the action. Cost is considered by the planner when actions are chained
        /// </summary>
        public int Cost = 1;

        /// <summary>
        /// A list of unique value pairs determining the pre conditions for each action
        /// </summary>
        internal HashSet<Tuple<string, bool>> _preConditions = new HashSet<Tuple<string, bool>>();

        /// <summary>
        /// A list of unique value pairs determining the post conditions/effects for each action
        /// </summary>
        internal HashSet<Tuple<string, bool>> _postConditions = new HashSet<Tuple<string, bool>>();

        public GOAPAction() { }
        public GOAPAction(string name) { Name = name; }
        public GOAPAction(string name, int cost) : this(name) { Cost = cost; }

        /// <summary>
        /// Add to the list of pre conditions that are considered/needed before the action is started
        /// </summary>
        /// <param name="conditionName"></param>
        /// <param name="value"></param>
        public void AddPrecondition(string conditionName, bool value)
        {
            _preConditions.Add(new Tuple<string, bool>(conditionName, value));

            //Maybe add getter functions for getting preconditions and remove functions to remove conditions
        }

        /// <summary>
        /// Add to the list of post conditions that are considered/needed by the next action
        /// </summary>
        /// <param name="conditionName"></param>
        /// <param name="value"></param>
        public void AddPostCondition(string conditionName, bool value)
        {
            _postConditions.Add(new Tuple<string, bool>(conditionName, value));
        }

        /// <summary>
        /// Called before plan is proposed - gives the action an opportunity to set its score or to opt out if it can't be used
        /// </summary>
        /// <returns></returns>
        public virtual bool Validate() { return true; }

        public override string ToString() { return string.Format("[GOAPAction] {0} - Cost: {1}", Name, Cost); }
    }
}