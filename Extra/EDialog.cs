using UnityEngine;
using System;
using ColossalFramework.UI;
// example:
// HFTDialog.MessageBox("error", "Sorry but you're S.O.L", () => { Application.Quit() });

namespace EManagersLib.Extra {
    public class EDialog : MonoBehaviour {
        private const int id = 0xab44932;
        private Rect m_windowRect;
        //private Action m_action;
        private string m_title;
        private string m_msg;
        private GUISkin skin;

        public static void MessageBox(string title, string msg) {
            GameObject go = new GameObject("EDialog");
            DontDestroyOnLoad(go);
            EDialog dlg = go.AddComponent<EDialog>();
            dlg.Init(title, msg);
        }

        private void Init(string title, string msg) {
            m_title = title;
            m_msg = msg;
            //m_action = action;
            GUI.BringWindowToFront(id);
        }

        public void Start() {
            useGUILayout = true;
        }

        protected void OnGUI() {
            const int maxWidth = 640;
            const int maxHeight = 480;

            if (skin is null) {
                Texture2D bgTexture = new Texture2D(1, 1);
                bgTexture.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.2f, 1f));
                bgTexture.Apply();

                skin = ScriptableObject.CreateInstance<GUISkin>();
                skin.box = new GUIStyle(GUI.skin.box);
                skin.button = new GUIStyle(GUI.skin.button);
                skin.horizontalScrollbar = new GUIStyle(GUI.skin.horizontalScrollbar);
                skin.horizontalScrollbarLeftButton = new GUIStyle(GUI.skin.horizontalScrollbarLeftButton);
                skin.horizontalScrollbarRightButton = new GUIStyle(GUI.skin.horizontalScrollbarRightButton);
                skin.horizontalScrollbarThumb = new GUIStyle(GUI.skin.horizontalScrollbarThumb);
                skin.horizontalSlider = new GUIStyle(GUI.skin.horizontalSlider);
                skin.horizontalSliderThumb = new GUIStyle(GUI.skin.horizontalSliderThumb);
                skin.label = new GUIStyle(GUI.skin.label);
                skin.scrollView = new GUIStyle(GUI.skin.scrollView);
                skin.textArea = new GUIStyle(GUI.skin.textArea);
                skin.textField = new GUIStyle(GUI.skin.textField);
                skin.toggle = new GUIStyle(GUI.skin.toggle);
                skin.verticalScrollbar = new GUIStyle(GUI.skin.verticalScrollbar);
                skin.verticalScrollbarDownButton = new GUIStyle(GUI.skin.verticalScrollbarDownButton);
                skin.verticalScrollbarThumb = new GUIStyle(GUI.skin.verticalScrollbarThumb);
                skin.verticalScrollbarUpButton = new GUIStyle(GUI.skin.verticalScrollbarUpButton);
                skin.verticalSlider = new GUIStyle(GUI.skin.verticalSlider);
                skin.verticalSliderThumb = new GUIStyle(GUI.skin.verticalSliderThumb);
                skin.window = new GUIStyle(GUI.skin.window);
                skin.window.normal.background = bgTexture;
                skin.window.onNormal.background = bgTexture;

                skin.settings.cursorColor = GUI.skin.settings.cursorColor;
                skin.settings.cursorFlashSpeed = GUI.skin.settings.cursorFlashSpeed;
                skin.settings.doubleClickSelectsWord = GUI.skin.settings.doubleClickSelectsWord;
                skin.settings.selectionColor = GUI.skin.settings.selectionColor;
                skin.settings.tripleClickSelectsLine = GUI.skin.settings.tripleClickSelectsLine;
            } else {
                GUI.skin = skin;
            }
            int width = EMath.Min(maxWidth, Screen.width - 20);
            int height = EMath.Min(maxHeight, Screen.height - 20);
            m_windowRect = new Rect(
                (Screen.width - width) / 2,
                (Screen.height - height) / 2,
                width,
                height);
            m_windowRect = GUI.Window(id, m_windowRect, WindowFunc, m_title);
            Cursor.lockState = CursorLockMode.Confined;
        }

        private void WindowFunc(int windowID) {
            const int border = 10;
            const int width = 50;
            const int height = 25;
            const int spacing = 10;

            GUI.Label(new Rect(
                border,
                border + spacing,
                m_windowRect.width - border * 2,
                m_windowRect.height - border * 2 - height - spacing), m_msg);

            Rect b = new Rect(
                m_windowRect.width - width - border,
                m_windowRect.height - height - border,
                width,
                height);

            if (GUI.Button(b, "ok")) {
                Destroy(gameObject);
                //m_action();
            }
        }
    }
}
