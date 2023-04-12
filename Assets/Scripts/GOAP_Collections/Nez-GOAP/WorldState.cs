using System;
using System.Text;

namespace GOAP_Nez_Deprecated
{

    public class NezWorldState : IEquatable<NezWorldState>
    {
        //using bitmask to shift on the condition index to flip bits - long : 64 bits
        public long Values;

        //Second bitmask for false values. Explicitly states false - needed as seperate store because abscence of value (0) does not mean it is false
        public long DontCare;

        //required so that we can get the condition index from the string name
        internal ActionPlanner planner;

        public NezWorldState(ActionPlanner planner, long values, long dontcare)
        {
            this.planner = planner;
            Values = values;
            DontCare = dontcare;
        }

        public static NezWorldState CreateWorldState(ActionPlanner planner) { return new NezWorldState(planner, 0, -1); }

        /// <summary>
        /// Public accessible method to set condition values within the created bit mask
        /// </summary>
        /// <param name="conditionName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetConditionValues(string conditionName, bool value)
        {
            return SetConditionValues(planner.FindConditionNameIndex(conditionName), value);
        }

        /// <summary>
        /// reads back from conditional value bool into a bitmask using bit operations - setting both the dontcare mask and the values mask
        /// </summary>
        /// <param name="conditionID"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal bool SetConditionValues(int conditionID, bool value)
        {
            Values = value == true ?
                (Values | 1L << conditionID) :
                (Values & ~(1L << conditionID));

            DontCare ^= (1 << conditionID);
            return true;
        }

        /// <summary>
        /// Compares two world states by comparing the Values & ~DontCare to the other
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(NezWorldState other)
        {
            var care = ~DontCare;
            return (Values & care) == (other.Values & care);
        }

        /// <summary>
        /// Debugging method using a string builder to display the bools within 
        /// the action planner corresponding to bit values within the bitmasks created 
        /// </summary>
        /// <param name="planner"></param>
        /// <returns></returns>
        public string Describe(ActionPlanner planner)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < ActionPlanner.MAX_CONDITIONS; i++)
            {
                if ((DontCare & (1L << i)) == 0)
                {
                    var val = planner.ConditionNames[i];

                    if (val == null)
                        continue;

                    bool set = ((Values & (1L << i)) != 0L);

                    if (sb.Length > 0)
                        sb.Append(", ");
                    sb.Append(set == true ? val.ToUpper() : val);
                }
            }

            return sb.ToString();
        }
    }
}