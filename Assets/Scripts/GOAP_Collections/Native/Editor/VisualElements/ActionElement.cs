#if UNITY_EDITOR
using UnityEngine.UIElements;
using UnityEngine;

namespace GOAP_Native.Editor
{
    public class ActionElement : Foldout
    {
        private IAction _action;

        private TextElement _status = new TextElement();

        private TextElement _actionCost = new TextElement();

        private WorldStateElement _preconditions;
        private WorldStateElement _postconditions;

        public ActionElement(IAction action)
        {
            _action = action;

            text = _action.Name;
            value = false;

            Add(_status);

            Add(_actionCost);

            _preconditions = new WorldStateElement("Preconditions: ", false);
            _postconditions = new WorldStateElement("Effects: ", false);

            Add(_preconditions);
            Add(_postconditions);

            DrawAction(action);
        }

        public void DrawAction(IAction action)
        {
            _action = action;

            bool isvalid = _action.Validate();

            style.color = isvalid ? Color.green * 0.8f : Color.red * 0.8f;

            text = _action.Name;
            value = false;

            _status.text = isvalid.ToString();

            _actionCost.text = _action.CalculateCost().ToString();

            _preconditions.DrawStates(_action.Preconditions);
            _postconditions.DrawStates(_action.Effects);
        }
    }
}
#endif