using ColossalFramework;
using ColossalFramework.UI;
using System.IO;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace UI {
    public class UIIndicator : UIPanel {
        private const float indicatorWidth = 26f;
        private const float indicatorHeight = 26f;
        private const int spriteMaxSize = 32;
        private const string IndicatorPanelName = @"IndicatorPanel";
        private const string SnappingIconName = @"Snap";
        private const string AnarchyIconName = @"Anarchy";
        private const string LockForestryIconName = @"LockForestry";
        public static UIIcon SnapIndicator { get; private set; } = null;
        public static UIIcon AnarchyIndicator { get; private set; } = null;
        public static UIIcon LockForestryIndicator { get; private set; } = null;

        public class UIIcon : UIPanel {
            private bool m_state;
            public string m_disabledTooltip;
            public string m_enabledTooltip;

            public bool State {
                get => m_state;
                set {
                    m_state = value;
                    backgroundSprite = value ? name + @"Enabled" : name + @"Disabled";
                    OnLocalizeTooltip();
                    RefreshTooltip();
                }
            }
            public override void Awake() {
                base.Awake();
                isLocalized = true;
            }
            protected override void OnLocalizeTooltip() {
                if (!string.IsNullOrEmpty(name)) {
                    tooltip = m_state ? m_enabledTooltip : m_disabledTooltip;
                }
            }
            public void SetState(bool state) {
                m_state = state;
                backgroundSprite = state ? name + @"Enabled" : name + @"Disabled";
                OnLocalizeTooltip();
                RefreshTooltip();
            }
            public virtual void UpdateState() {
                tooltip = m_state ? m_enabledTooltip : m_disabledTooltip;
                backgroundSprite = m_state ? name + @"Enabled" : name + @"Disabled";
            }
            public void InvalidateTooltip() {
                OnLocaleChanged();
                RefreshTooltip();
            }
        }

        protected override void OnResolutionChanged(Vector2 previousResolution, Vector2 currentResolution) {
            base.OnResolutionChanged(previousResolution, currentResolution);
            relativePosition = new Vector3(0f - width - 5f, 0f);
        }

        /* Hate this hack, but the only way I can think of
         * If anyone has a better solution, please don't hesitate to send a pull request
         * or email me. Thanks in advance */
        private void OnInitialFrame(object _) {
            while (UIView.GetAView().framesRendered < 5) { Thread.Sleep(100); }
            relativePosition = new Vector3(0f - width - 5f, 0f);
        }

        public override void OnDisable() {
            base.OnDisable();
            if (SnapIndicator) {
                RemoveUIComponent(SnapIndicator);
                Destroy(SnapIndicator.gameObject);
                SnapIndicator = null;
            }
            if (AnarchyIndicator) {
                RemoveUIComponent(AnarchyIndicator);
                Destroy(AnarchyIndicator.gameObject);
                AnarchyIndicator = null;
            }
            if (LockForestryIndicator) {
                RemoveUIComponent(LockForestryIndicator);
                Destroy(LockForestryIndicator.gameObject);
                LockForestryIndicator = null;
            }
        }

        public static UIIndicator Setup() {
            UIPanel infoPanel = UIView.GetAView().FindUIComponent<UIPanel>(@"InfoPanel");
            UIIndicator indicatorPanel = infoPanel.GetComponentInChildren<UIIndicator>();
            if (indicatorPanel is null) {
                Debug.Log($"IndicatorPanel: Is NULL");
            } else {
                Debug.Log($"IndicatorPanel: {indicatorPanel.name}");
            }
            if (indicatorPanel is null && (Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.MapEditor) == ItemClass.Availability.None) {
                Debug.Log("Creating new Indicator panel");
                indicatorPanel = UIView.GetAView().FindUIComponent<UIPanel>(@"InfoPanel").AddUIComponent<UIIndicator>();
                indicatorPanel.name = IndicatorPanelName;
                indicatorPanel.UpdatePosition();
                return indicatorPanel;
            }
            return indicatorPanel;
        }

        public void UpdatePosition() {
            UIButton uIButton = parent.Find<UIButton>(@"Heat'o'meter");
            if (uIButton is null) {
                uIButton = parent.Find<UIButton>(@"PopulationPanel");
            }
            if (!(uIButton is null)) {
                uIButton.relativePosition += new Vector3(10f, 0f);
                cachedTransform.parent = uIButton.cachedTransform;
                transform.parent = uIButton.transform;
                ThreadPool.QueueUserWorkItem(OnInitialFrame);
            }
        }

        public UIIcon AddSnappingIcon(string enableTooltip, string disableTooltip, bool defState, MouseEventHandler callback, out bool finalState) {
            UIIcon icon = SnapIndicator;
            if (icon is null) {
                icon = AddUIComponent<UIIcon>();
                icon.name = SnappingIconName;
                icon.m_enabledTooltip = enableTooltip;
                icon.m_disabledTooltip = disableTooltip;
                icon.atlas = CreateTextureAtlas($"UI" + SnappingIconName + "Atlas", new string[] {
                    SnappingIconName + "Enabled",
                    SnappingIconName + "Disabled"
                });
                icon.size = new Vector2(indicatorWidth, indicatorHeight);
                icon.eventClicked += callback;
                icon.playAudioEvents = true;
                icon.relativePosition = new Vector3(0f, 0f);
                size = icon.size;
                icon.State = defState;
                SnapIndicator = icon;
            } else {
                icon.m_enabledTooltip += "\n" + enableTooltip;
                icon.m_disabledTooltip += "\n" + disableTooltip;
                icon.InvalidateTooltip();
                if (defState != icon.State) {
                    finalState = icon.State;
                    icon.State = finalState;
                }
                icon.eventClicked += callback;
            }
            finalState = icon.State;
            icon.UpdateState();
            return icon;
        }

        public UIIcon AddAnarchyIcon(string enableTooltip, string disableTooltip, bool defState, MouseEventHandler callback, out bool finalState) {
            UIIcon icon = AnarchyIndicator;
            if (icon is null) {
                icon = AddUIComponent<UIIcon>();
                icon.name = AnarchyIconName;
                icon.m_enabledTooltip = enableTooltip;
                icon.m_disabledTooltip = disableTooltip;
                icon.atlas = CreateTextureAtlas($"UI" + SnappingIconName + "Atlas", new string[] {
                    AnarchyIconName + "Enabled",
                    AnarchyIconName + "Disabled"
                });
                icon.size = new Vector2(indicatorWidth, indicatorHeight);
                icon.eventClicked += callback;
                icon.playAudioEvents = true;
                UIIcon anchor = SnapIndicator;
                if (anchor is null) {
                    icon.relativePosition = new Vector3(0f, 0f);
                } else {
                    icon.relativePosition = new Vector3(anchor.relativePosition.x + anchor.width, 0f);
                }
                size = new Vector2(size.x + icon.width, size.y);
                icon.State = defState;
                AnarchyIndicator = icon;
            } else {
                icon.m_enabledTooltip += "\n" + enableTooltip;
                icon.m_disabledTooltip += "\n" + disableTooltip;
                icon.InvalidateTooltip();
                if (defState != icon.State) {
                    finalState = icon.State;
                    icon.State = finalState;
                }
                icon.eventClicked += callback;
            }
            finalState = icon.State;
            icon.UpdateState();
            return icon;
        }

        public UIIcon AddLockForestryIcon(string enableTooltip, string disableTooltip, bool defState, MouseEventHandler callback) {
            UIIcon icon = LockForestryIndicator;
            if (icon is null) {
                icon = AddUIComponent<UIIcon>();
                icon.name = LockForestryIconName;
                icon.m_enabledTooltip = enableTooltip;
                icon.m_disabledTooltip = disableTooltip;
                icon.atlas = CreateTextureAtlas($"UI" + LockForestryIconName + "Atlas", new string[] {
                    LockForestryIconName + "Enabled",
                    LockForestryIconName + "Disabled"
                });
                icon.size = new Vector2(indicatorWidth, indicatorHeight);
                icon.eventClicked += callback;
                icon.playAudioEvents = true;
                UIIcon anchor = AnarchyIndicator;
                if (anchor is null) {
                    icon.relativePosition = new Vector3(0f, 0f);
                } else {
                    icon.relativePosition = new Vector3(anchor.relativePosition.x + anchor.width, 0f);
                }
                size = new Vector2(size.x + icon.width, size.y);
                icon.State = defState;
                LockForestryIndicator = icon;
            }
            return icon;
        }

        private static UITextureAtlas CreateTextureAtlas(string atlasName, string[] spriteNames) {
            Texture2D texture2D = new Texture2D(spriteMaxSize, spriteMaxSize, TextureFormat.ARGB32, false);
            Texture2D[] array = new Texture2D[spriteNames.Length];
            for (int i = 0; i < spriteNames.Length; i++) {
                array[i] = LoadTextureFromAssembly(spriteNames[i] + ".png");
            }
            Rect[] array2 = texture2D.PackTextures(array, 2, spriteMaxSize);
            UITextureAtlas uITextureAtlas = ScriptableObject.CreateInstance<UITextureAtlas>();
            Material material = Instantiate<Material>(UIView.GetAView().defaultAtlas.material);
            material.mainTexture = texture2D;
            uITextureAtlas.material = material;
            uITextureAtlas.name = atlasName;
            for (int j = 0; j < spriteNames.Length; j++) {
                UITextureAtlas.SpriteInfo item = new UITextureAtlas.SpriteInfo() {
                    name = spriteNames[j],
                    texture = array[j],
                    region = array2[j]
                };
                uITextureAtlas.AddSprite(item);
            }
            return uITextureAtlas;
        }

        private static Texture2D LoadTextureFromAssembly(string filename) {
            UnmanagedMemoryStream s = (UnmanagedMemoryStream)Assembly.GetExecutingAssembly().GetManifestResourceStream("UIIndicator.Icons." + filename);
            byte[] array = new byte[s.Length];
            s.Read(array, 0, array.Length);
            Texture2D texture = new Texture2D(spriteMaxSize, spriteMaxSize, TextureFormat.ARGB32, false);
            texture.LoadImage(array);
            texture.Compress(false);
            return texture;
        }
    }
}
