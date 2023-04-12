using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEngine.UI;
using Unity.Collections;

namespace GOAP_DOTS.Editor
{
    public class WorldStateElement : VisualElement
    {
        private WorldState _worldState;

        private TextElement _worldStateName = new TextElement();
        private TextElement _worldStatePriority = new TextElement();
        private TextElement _worldStatus = new TextElement();

        private TextElement _states = new TextElement();

        public WorldStateElement(bool displayName = true)
        {
            if (displayName)
                Add(_worldStateName);

            Add(_worldStatePriority);
            Add(_worldStatus);
            Add(_states);
        }

        public void DrawStates(WorldState worldState)
        {
            _worldState = worldState;

            _worldStateName.text = _worldState.Name.ToString();

            _worldStatePriority.text = _worldState.Priority.ToString();
            _worldStatus.text = _worldState.IsValid.ToString();

            _states.Clear();
            NativeArray<FixedString32Bytes> keys = _worldState.States.GetKeyArray(Allocator.Temp);

            foreach (FixedString32Bytes key in keys)
            {
                string state = string.Format("[{0} : {1}]", key.ToString(), _worldState.States[key]);
                _states.Add(new TextElement { text = state });
            }

            keys.Dispose();
        }
    }
}