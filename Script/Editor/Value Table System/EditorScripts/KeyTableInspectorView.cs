using GeneralGameDevKit.ValueTableSystem.Internal;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace GeneralGameDevKit.Script.Editor
{
    public class KeyTableInspectorView : VisualElement
    {
        private readonly TextField _txtFieldGuid;
        private readonly TextField _txtFieldCurrentPath;
        private readonly TextField _txtPathToEdit;
        private readonly Button _btnCopyAndPasteCurrentToEdit;
        
        public new class UxmlFactory : UxmlFactory<KeyTableInspectorView, UxmlTraits>{ }
        public KeyTableInspectorView()
        {
            _txtFieldGuid = new TextField("GUID")
            {
                bindingPath = nameof(KeyEntity.guid)
            };
            _txtFieldGuid.SetEnabled(false);
            
            _txtFieldCurrentPath = new TextField("Current Path")
            {
                bindingPath = nameof(KeyEntity.pathOfKey)
            };
            _txtFieldCurrentPath.SetEnabled(false);
            
            _txtPathToEdit = new TextField("Edit Path");
            
            _btnCopyAndPasteCurrentToEdit = new Button(() => { _txtPathToEdit.value = _txtFieldCurrentPath.value; })
            {
                text = "C&P Current Path To Edit Field"
            };

            Add(_txtFieldGuid);
            Add(_txtFieldCurrentPath);
            Add(_txtPathToEdit);
            Add(_btnCopyAndPasteCurrentToEdit);
        }

        public void SetInspectorActive(bool isActive)
        {
            _btnCopyAndPasteCurrentToEdit.SetEnabled(isActive);
            
            if (isActive)
            {
                //...
            }
            else
            {
                _txtPathToEdit.value = string.Empty;
            }
        }
        
        public void ChangeCurrentTargetKeyEntity(KeyEntity keyToChange)
        {
            if (!keyToChange)
            {
                SetInspectorActive(false);
                return;
            }

            var objToBind = new SerializedObject(keyToChange);
            _txtFieldGuid.Bind(objToBind);
            _txtFieldCurrentPath.Bind(objToBind);
            SetInspectorActive(true);
        }

        public string GetPathToEdit() => _txtPathToEdit.value;
    }
}