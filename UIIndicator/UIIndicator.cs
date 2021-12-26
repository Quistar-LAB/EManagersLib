using ColossalFramework;
using ColossalFramework.UI;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace UI {
    public sealed class UIIndicator : UIPanel {
        public sealed class UIIcon : UIPanel {
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
            public void UpdateState() {
                tooltip = m_state ? m_enabledTooltip : m_disabledTooltip;
                backgroundSprite = m_state ? name + @"Enabled" : name + @"Disabled";
            }
            public void InvalidateTooltip() {
                OnLocaleChanged();
                RefreshTooltip();
            }
        }

        public sealed class UISnapMenu : UILabel {
            private const string bgSprite = @"ButtonMenu";

            public override void Awake() {
                base.Awake();
                textAlignment = UIHorizontalAlignment.Left;
                padding = new RectOffset(4, 1, 1, 1);
                textScale = 0.90f;
                text = @"Snap to Demand Panel";
            }

            protected override void OnMouseEnter(UIMouseEventParameter p) {
                backgroundSprite = bgSprite;
            }

            protected override void OnMouseLeave(UIMouseEventParameter p) {
                backgroundSprite = @"";
            }
        }

        private const string SettingsFile = @"IndicatorPanelSettings";
        private const float indicatorWidth = 36f;
        private const float indicatorHeight = 36f;
        private const int spriteMaxSize = 128;
        private const string IndicatorPanelName = @"IndicatorPanel";
        private const string SnappingIconName = @"Snap";
        private const string AnarchyIconName = @"Anarchy";
        private const string LockForestryIconName = @"LockForestry";
        private static UIComponent m_anchor;
        private static UIComponent m_temperaturePanel;
        private static UIPanel m_menuPanel;
        private static SavedFloat m_indicatorXPos;
        private static SavedFloat m_indicatorYPos;
        private static SavedBool m_snapToDemand;
        public static UIIcon SnapIndicator { get; private set; } = null;
        public static UIIcon AnarchyIndicator { get; private set; } = null;
        public static UIIcon LockForestryIndicator { get; private set; } = null;
        private Vector3 m_lastPosition;

        public UIIndicator() {
            if (GameSettings.FindSettingsFileByName(SettingsFile) == null) {
                GameSettings.AddSettingsFile(new SettingsFile[] {
                        new SettingsFile() { fileName = SettingsFile }
                    });
            }
            m_indicatorXPos = new SavedFloat(@"indicatorXPos", SettingsFile);
            m_indicatorYPos = new SavedFloat(@"indicatorYPos", SettingsFile);
            m_snapToDemand = new SavedBool(@"snapToDemand", SettingsFile, true, true);
        }

        private bool m_startDrag;
        private IEnumerator StartDragDelay() {
            yield return new WaitForSeconds(1.3f);
            m_startDrag = true;
        }
        protected override void OnMouseDown(UIMouseEventParameter p) {
            p.Use();
            Plane plane = new Plane(transform.TransformDirection(Vector3.back), transform.position);
            Ray ray = p.ray;
            plane.Raycast(ray, out float d);
            m_lastPosition = ray.origin + ray.direction * d;
            m_startDrag = false;
            StartCoroutine(StartDragDelay());
            base.OnMouseDown(p);
        }

        protected override void OnMouseMove(UIMouseEventParameter p) {
            p.Use();
            if (m_startDrag && p.buttons.IsFlagSet(UIMouseButton.Left)) {
                Ray ray = p.ray;
                Vector3 inNormal = GetUIView().uiCamera.transform.TransformDirection(Vector3.back);
                Plane plane = new Plane(inNormal, m_lastPosition);
                plane.Raycast(ray, out float d);
                Vector3 vector = (ray.origin + ray.direction * d).Quantize(PixelsToUnits());
                Vector3 b = vector - m_lastPosition;
                Vector3[] corners = GetUIView().GetCorners();
                Vector3 position = (transform.position + b).Quantize(PixelsToUnits());
                Vector3 a = pivot.TransformToUpperLeft(size, arbitraryPivotOffset);
                Vector3 a2 = a + new Vector3(size.x, -size.y);
                a *= PixelsToUnits();
                a2 *= PixelsToUnits();
                if (position.x + a.x < corners[0].x) {
                    position.x = corners[0].x - a.x;
                }
                if (position.x + a2.x > corners[1].x) {
                    position.x = corners[1].x - a2.x;
                }
                if (position.y + a.y > corners[0].y) {
                    position.y = corners[0].y - a.y;
                }
                if (position.y + a2.y < corners[2].y) {
                    position.y = corners[2].y - a2.y;
                }
                transform.position = position;
                m_lastPosition = vector;
                m_snapToDemand.value = false;
                m_indicatorXPos.value = position.x;
                m_indicatorYPos.value = position.y;
            }
            base.OnMouseMove(p);
        }

        protected override void OnMouseUp(UIMouseEventParameter p) {
            base.OnMouseUp(p);
            m_startDrag = false;
            MakePixelPerfect();
        }

        protected override void OnResolutionChanged(Vector2 previousResolution, Vector2 currentResolution) {
            base.OnResolutionChanged(previousResolution, currentResolution);
            StartCoroutine(RepositionCoroutine(m_anchor, m_temperaturePanel));
        }

        /* Hate this hack, but the only way I can think of, since UIResolution mod repositions
         * almost everything on the panel, but at a very slow rate.
         * If anyone has a better solution, please don't hesitate to send a pull request
         * or email me. Thanks in advance */
        private void RepositionIndicator(UIComponent anchor, UIComponent tempPanel) {
            Vector3 demandAbsPos = anchor.relativePosition + anchor.parent.relativePosition + anchor.parent.parent.relativePosition;
            relativePosition = new Vector3(demandAbsPos.x + anchor.size.x + 8f, demandAbsPos.y + (anchor.size.y - size.y) / 2f);
            if (tempPanel) {
                Vector3 newPos = relativePosition - anchor.parent.relativePosition - anchor.parent.parent.relativePosition;
                float offsetX = newPos.x + size.x + 5f;
                if (offsetX > tempPanel.relativePosition.x) {
                    tempPanel.relativePosition = new Vector3(offsetX, tempPanel.relativePosition.y);
                }
            }
        }

        private IEnumerator RepositionCoroutine(UIComponent anchor, UIComponent tempPanel) {
            yield return new WaitForSeconds(0.3f); // new WaitForEndOfFrame();
            RepositionIndicator(anchor, tempPanel);
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

        private static bool m_menuPanelResetFade = false;
        private static IEnumerator FadeMenu(UIPanel menuPanel) {
startNew:
            yield return new WaitForSeconds(1.0f);
            while (menuPanel.isVisible) {
                if (m_menuPanelResetFade) {
                    m_menuPanelResetFade = false;
                    menuPanel.opacity = 1f;
                    goto startNew;
                }
                if (menuPanel.opacity > 0) {
                    menuPanel.opacity -= 0.2f;
                } else {
                    menuPanel.isVisible = false;
                }
                yield return new WaitForEndOfFrame();
            }
        }

        public static UIIndicator Setup() {
            UIIndicator indicatorPanel = UIView.GetAView().FindUIComponent<UIIndicator>(IndicatorPanelName);
            if (indicatorPanel is null) {
                Debug.Log($"UIIndicator: IndicatorPanel is NULL");
            } else {
                Debug.Log($"UIIndicator: IndicatorPanel {indicatorPanel.name}");
            }
            if (indicatorPanel is null && (Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.MapEditor) == ItemClass.Availability.None) {
                UIPanel infoPanel = UIView.GetAView().FindUIComponent<UIPanel>(@"InfoPanel");
                UIPanel demand = infoPanel.Find<UIPanel>(@"Demand");
                UIButton tempPanel = infoPanel.Find<UIButton>(@"Heat'o'meter");
                m_anchor = demand;
                m_temperaturePanel = tempPanel;
                indicatorPanel = UIView.GetAView().AddUIComponent(typeof(UIIndicator)) as UIIndicator;
                indicatorPanel.name = IndicatorPanelName;
                // Add menu
                UIPanel menuPanel = UIView.GetAView().AddUIComponent(typeof(UIPanel)) as UIPanel;
                menuPanel.autoSize = false;
                menuPanel.autoLayoutDirection = LayoutDirection.Vertical;
                menuPanel.autoLayout = true;
                menuPanel.padding = new RectOffset(5, 5, 5, 5);
                menuPanel.backgroundSprite = @"InfoPanelBack";
                UISnapMenu snapDemand = menuPanel.AddUIComponent<UISnapMenu>();
                menuPanel.width = snapDemand.width + 10;
                menuPanel.height = snapDemand.height + 10;
                menuPanel.relativePosition = new Vector3(indicatorPanel.relativePosition.x + indicatorPanel.width - 20f, indicatorPanel.relativePosition.y - menuPanel.height);
                menuPanel.isVisible = false;
                menuPanel.eventVisibilityChanged += (c, v) => {
                    if (v) {
                        menuPanel.opacity = 1f;
                        menuPanel.StartCoroutine(FadeMenu(menuPanel));
                    }
                };
                menuPanel.eventMouseMove += (c, p) => {
                    m_menuPanelResetFade = true;
                };
                indicatorPanel.eventSizeChanged += (c, size) => {
                    menuPanel.relativePosition = new Vector3(indicatorPanel.relativePosition.x + indicatorPanel.width - 20f, indicatorPanel.relativePosition.y - menuPanel.height);
                };
                indicatorPanel.eventPositionChanged += (c, pos) => {
                    menuPanel.relativePosition = new Vector3(indicatorPanel.relativePosition.x + indicatorPanel.width - 20f, indicatorPanel.relativePosition.y - menuPanel.height);
                };
                snapDemand.eventClicked += (c, p) => {
                    menuPanel.isVisible = false;
                    m_snapToDemand.value = true;
                    indicatorPanel.RepositionIndicator(demand, tempPanel);
                };
                m_menuPanel = menuPanel;
                demand.parent.eventPositionChanged += (c, p) => {
                    if (m_snapToDemand) indicatorPanel.RepositionIndicator(demand, tempPanel);
                };
                if (m_snapToDemand) {
                    indicatorPanel.StartCoroutine(indicatorPanel.RepositionCoroutine(demand, tempPanel));
                } else {
                    indicatorPanel.transform.position = new Vector3(m_indicatorXPos, m_indicatorYPos);
                }
            }
            return indicatorPanel;
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
                icon.eventMouseDown += (c, p) => {
                    if ((p.buttons & UIMouseButton.Right) == UIMouseButton.Right) {
                        m_menuPanel.isVisible = true;
                    }
                };
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
                icon.eventMouseDown += (c, p) => {
                    if ((p.buttons & UIMouseButton.Right) == UIMouseButton.Right) {
                        m_menuPanel.isVisible = true;
                    }
                };
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
                icon.eventMouseDown += (c, p) => {
                    if ((p.buttons & UIMouseButton.Right) == UIMouseButton.Right) {
                        m_menuPanel.isVisible = true;
                    }
                };
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
