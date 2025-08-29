using UnityEngine;
using UnityEngine.UIElements;

namespace UI_Docs
{
    public abstract class UI_Handler : MonoBehaviour
    {
        protected VisualElement root;

        [SerializeField] VisualTreeAsset _visualTree;
        [SerializeField] StyleSheet _styleSheet;

        protected virtual VisualTreeAsset VisualTree => _visualTree;
        protected virtual StyleSheet StyleSheet => _styleSheet;


        protected virtual void OnEnable()
        {
            var uiRoot = GetComponent<UIDocument>().rootVisualElement;
            if (uiRoot == null)
            {
                Debug.LogWarning($"{GetType().Name}: No root visual element found.");
                return;
            }

            if (VisualTree != null)
            {
                root = VisualTree.CloneTree();
                uiRoot.Clear();
                uiRoot.Add(root);
            }
            else
            {
                root = uiRoot;
            }

            if (StyleSheet != null)
            {
                root.styleSheets.Add(StyleSheet);
            }

            InitUI();
        }

        // Do stuff here
        protected abstract void InitUI();
    }

}

