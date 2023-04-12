#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;

namespace GOAP_Native.Editor
{
    public class WorldStateElement : Foldout
    {
        private WorldState _worldState;

        private TextElement _worldStatePriority = new TextElement();
        private TextElement _worldStatus = new TextElement();

        private TextElement _states = new TextElement();

        public WorldStateElement(string label, bool displaypriority = true)
        {
            text = label;

            if(displaypriority)
                Add(_worldStatePriority);
    
            Add(_worldStatus);
            Add(_states);
        }

        public void DrawStates(WorldState worldState, bool modifyStyle = false)
        {
            _worldState = worldState;

            if (_worldState.Priority != null)
                _worldStatePriority.text = string.Format("Priority: {0}", _worldState.Priority().ToString());

            if (_worldState.Validation != null)
            {
                bool valid = _worldState.Validation();
                _worldStatus.text = string.Format("Status: {0}", valid.ToString());

                if(modifyStyle == true)
                    style.color = valid ? new Color(0f, 1f, 0f, 0.4f) : new Color(1f, 0f, 0f, 0.4f);
            }

            _states.Clear();
            foreach (string key in _worldState.States.Keys)
            {
                string state = string.Format("[{0} : {1}]", key.ToString(), _worldState.States[key].ToString());
                _states.Add(new TextElement { text = state });
            }
        }
    }
}
#endif