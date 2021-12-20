using ColossalFramework;
using ColossalFramework.UI;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace UI {
    public class UIIndicator : UIPanel {
        private const float indicatorWidth = 36f;
        private const float indicatorHeight = 36f;
        private const int spriteMaxSize = 128;
        private const string IndicatorPanelName = @"IndicatorPanel";
        private const string SnappingIconName = @"Snap";
        private const string AnarchyIconName = @"Anarchy";
        private const string LockForestryIconName = @"LockForestry";
        private static UIComponent m_anchor;
        private static UIComponent m_temperaturePanel;
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
            UIComponent anchor = m_anchor;
            UIComponent tempPanel = m_temperaturePanel;
            Vector3 demandAbsPos = anchor.relativePosition + anchor.parent.relativePosition;
            relativePosition = new Vector3(demandAbsPos.x + anchor.size.x + 8f, demandAbsPos.y + (anchor.size.y - size.y) / 2f);
            if (tempPanel) {
                float xOffset = relativePosition.x + size.x + 5f;
                if (xOffset > tempPanel.relativePosition.x) {
                    tempPanel.relativePosition = new Vector3(xOffset, tempPanel.relativePosition.y);
                }
            }
        }

        /* Hate this hack, but the only way I can think of
         * If anyone has a better solution, please don't hesitate to send a pull request
         * or email me. Thanks in advance */
        private IEnumerator InitialFrameHandler(UIComponent anchor, UIButton tempPanel) {
            while (UIView.GetAView().framesRendered < 5) { yield return new WaitForSeconds(0.1f); }
            Vector3 demandAbsPos = anchor.relativePosition + anchor.parent.relativePosition;
            relativePosition = new Vector3(demandAbsPos.x + anchor.size.x + 8f, demandAbsPos.y + (anchor.size.y - size.y) / 2f);
            if (tempPanel) {
                float offsetX = relativePosition.x + size.x + 5f;
                if (offsetX > tempPanel.relativePosition.x) {
                    tempPanel.relativePosition = new Vector3(offsetX, tempPanel.relativePosition.y);
                }
            }
        }

        public override void OnDestroy() {
            base.OnDestroy();
            m_anchor = null;
            m_temperaturePanel = null;
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
                Debug.Log($"UIIndicator: IndicatorPanel is NULL");
            } else {
                Debug.Log($"UIIndicator: IndicatorPanel {indicatorPanel.name}");
            }
            if (indicatorPanel is null && (Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.MapEditor) == ItemClass.Availability.None) {
                UIPanel demand = infoPanel.Find<UIPanel>(@"Demand");
                UIButton tempPanel = infoPanel.Find<UIButton>(@"Heat'o'meter");
                m_anchor = demand;
                m_temperaturePanel = tempPanel;
                indicatorPanel = infoPanel.AddUIComponent<UIIndicator>();
                indicatorPanel.name = IndicatorPanelName;
                indicatorPanel.StartCoroutine(indicatorPanel.InitialFrameHandler(demand, tempPanel));
                return indicatorPanel;
            }
            return indicatorPanel;
        }

        public void UpdatePosition() {
            UIPanel demand = parent.Find<UIPanel>(@"Demand");
            UIButton uIButton = parent.Find<UIButton>(@"Heat'o'meter");
            if (!(demand is null)) {
                m_anchor = demand;
                m_temperaturePanel = uIButton;
                //cachedTransform.parent = uIButton.cachedTransform;
                //transform.parent = uIButton.transform;
                StartCoroutine(InitialFrameHandler(demand, uIButton));
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
                    icon.relativePosition = new Vector3(anchor.relativePosition.x + anchor.width + 3f, 0f);
                }
                size = new Vector2(size.x + icon.width + 3f, size.y);
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
                    icon.relativePosition = new Vector3(anchor.relativePosition.x + anchor.width + 3f, 0f);
                }
                size = new Vector2(size.x + icon.width + 3f, size.y);
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
            uITextureAtlas.AddTextures(array);
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
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(array);
            return texture;
        }
    }
}
