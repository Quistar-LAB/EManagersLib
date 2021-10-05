using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace EManagersLib {
    internal class EKeyBinding : UICustomControl {
        private const string thisCategory = "EManagersLib";
        private SavedInputKey m_EditingBinding;
        [RebindableKey("TreeAnarchy")]
        private static readonly string toggleStatsPanelVisibility = "toggleStatsPanelVisibility";
        private static readonly InputKey toggleStatsPanelVisiblityKey = SavedInputKey.Encode(KeyCode.L, true, false, false);
        internal static readonly SavedInputKey m_toggleStatsPanel = new SavedInputKey(toggleStatsPanelVisibility, EModule.m_settingsFile, toggleStatsPanelVisiblityKey, true);

        protected void Awake() {
            AddKeymapping("Toggle Stats Panel Visibility", m_toggleStatsPanel);
        }

        private int listCount = 0;
        private void AddKeymapping(string key, SavedInputKey savedInputKey) {
            UIPanel uIPanel = component.AttachUIComponent(UITemplateManager.GetAsGameObject("KeyBindingTemplate")) as UIPanel;
            if (listCount++ % 2 == 1) uIPanel.backgroundSprite = null;

            UILabel uILabel = uIPanel.Find<UILabel>("Name");
            UIButton uIButton = uIPanel.Find<UIButton>("Binding");

            uIButton.eventKeyDown += new KeyPressHandler(OnBindingKeyDown);
            uIButton.eventMouseDown += new MouseEventHandler(OnBindingMouseDown);
            uILabel.stringUserData = key;
            uILabel.text = key;
            uIButton.text = savedInputKey.ToLocalizedString("KEYNAME");
            uIButton.objectUserData = savedInputKey;
            uIButton.stringUserData = thisCategory; // used for localization TODO:
        }

        private void OnBindingKeyDown(UIComponent comp, UIKeyEventParameter p) {
            if (!(m_EditingBinding is null) && !IsModifierKey(p.keycode)) {
                p.Use();
                UIView.PopModal();
                KeyCode keycode = p.keycode;
                InputKey inputKey = (p.keycode == KeyCode.Escape) ? m_EditingBinding.value : SavedInputKey.Encode(keycode, p.control, p.shift, p.alt);
                if (p.keycode == KeyCode.Backspace) {
                    inputKey = SavedInputKey.Empty;
                }
                m_EditingBinding.value = inputKey;
                UITextComponent uITextComponent = p.source as UITextComponent;
                uITextComponent.text = m_EditingBinding.ToLocalizedString("KEYNAME");
                m_EditingBinding = null;
            }
        }

        private void OnBindingMouseDown(UIComponent comp, UIMouseEventParameter p) {
            if (m_EditingBinding is null) {
                p.Use();
                m_EditingBinding = (SavedInputKey)p.source.objectUserData;
                UIButton uIButton = p.source as UIButton;
                uIButton.buttonsMask = (UIMouseButton.Left | UIMouseButton.Right | UIMouseButton.Middle | UIMouseButton.Special0 | UIMouseButton.Special1 | UIMouseButton.Special2 | UIMouseButton.Special3);
                uIButton.text = "Press Any Key";
                p.source.Focus();
                UIView.PushModal(p.source);
            } else if (!IsUnbindableMouseButton(p.buttons)) {
                p.Use();
                UIView.PopModal();
                InputKey inputKey = SavedInputKey.Encode(ButtonToKeycode(p.buttons), IsControlDown(), IsShiftDown(), IsAltDown());
                m_EditingBinding.value = inputKey;
                UIButton uIButton2 = p.source as UIButton;
                uIButton2.text = m_EditingBinding.ToLocalizedString("KEYNAME");
                uIButton2.buttonsMask = UIMouseButton.Left;
                m_EditingBinding = null;
            }
        }

        private KeyCode ButtonToKeycode(UIMouseButton button) {
            switch (button) {
            case UIMouseButton.Left: return KeyCode.Mouse0;
            case UIMouseButton.Right: return KeyCode.Mouse1;
            case UIMouseButton.Middle: return KeyCode.Mouse2;
            case UIMouseButton.Special0: return KeyCode.Mouse3;
            case UIMouseButton.Special1: return KeyCode.Mouse4;
            case UIMouseButton.Special2: return KeyCode.Mouse5;
            case UIMouseButton.Special3: return KeyCode.Mouse6;
            default: return KeyCode.None;
            }
        }

        private bool IsUnbindableMouseButton(UIMouseButton code) => (code == UIMouseButton.Left || code == UIMouseButton.Right);
        private bool IsModifierKey(KeyCode code) => code == KeyCode.LeftControl || code == KeyCode.RightControl || code == KeyCode.LeftShift ||
                                                    code == KeyCode.RightShift || code == KeyCode.LeftAlt || code == KeyCode.RightAlt;
        private bool IsControlDown() => (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
        private bool IsShiftDown() => (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        private bool IsAltDown() => (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt));
    }
}
