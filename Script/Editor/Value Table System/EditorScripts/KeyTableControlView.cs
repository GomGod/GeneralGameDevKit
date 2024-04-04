using System;
using UnityEngine.UIElements;

namespace GeneralGameDevKit.Script.Editor
{
    public class KeyTableControlView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<KeyTableControlView, UxmlTraits>{ }

        public Action OnButtonEditClicked;
        public Action OnButtonAddClicked;
        public Action OnButtonRemoveClicked;

        private readonly Button _buttonEdit;
        private readonly Button _buttonAdd;
        private readonly Button _buttonRemove;
        
        public KeyTableControlView()
        {
            _buttonEdit = new Button(() => OnButtonEditClicked?.Invoke())
            {
                text = "Edit"
            };
            _buttonAdd = new Button(() => OnButtonAddClicked?.Invoke())
            {
                text = "Add"
            };
            _buttonRemove = new Button(() => OnButtonRemoveClicked?.Invoke())
            {
                text = "Remove"
            };

            Add(_buttonRemove);
            Add(_buttonAdd);
            Add(_buttonEdit);
        }

        public void SetButtonEnable(bool edit, bool add, bool rem)
        {
            _buttonEdit.SetEnabled(edit);
            _buttonAdd.SetEnabled(add);
            _buttonRemove.SetEnabled(rem);
        }
    }
}