using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Mighty;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static Mighty.MightyCore;
using static Mighty.MightyCoreData;
using static Mighty.MightyWindowManagerStateful;

namespace Mighty
{
    public class MightyMap : EditorWindow
    {
        #region Variables


        public static bool isInit, dirty = true, isAdjust, didAdjust, rebuildClusters, is2DMode, GUILoaded = false;

        public static float svOrthSize;
        static bool rebuildingView = false, updatingMappables = false, buildMappables = false, initializing = false;
        static SceneView sceneView;
        static bool sceneLoading = false;

        static StyleSheet mightyStylesheet;

        public static bool isHoveringOnMappable = false;
        public static List<Button> addMappableButtons = new List<Button>();
        private static Debouncer debouncer;

        private static Button sceneCamIcon;

        private static bool firstRun = true;

        #endregion

        #region initialize
        static MightyMap()
        {
            StartWindow -= Start;
            StartWindow += Start;
        }


        public static bool showSceneCamIcon;

        [MenuItem("Window/Mighty Dev Ops/Open Mighty Map")]
        private static void OpenMightyMapWindow()
        {
            window = GetWindow();
        }


        public static EditorWindow GetWindow()
        {
            if (window == null)
            {
                window = GetWindow<MightyMap>(false, "Mighty Map", true);

                window.minSize = new Vector2(400, 600);
                window.Show();
                window.titleContent.text = "Mighty Map";
            }
            return window;
        }

        public static void Start()
        {
            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;

            EditorApplication.quitting -= EditorQuit;
            EditorApplication.quitting += EditorQuit;

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            // SceneView.duringSceneGui -= OnScene;
            // SceneView.duringSceneGui += OnScene;

            RebuildRunIdDropDown -= InitDropDownRunIds;
            RebuildRunIdDropDown += InitDropDownRunIds;

            BuildWindowBar -= PopulateWindowBar;
            BuildWindowBar += PopulateWindowBar;

            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;

            EditorSceneManager.sceneClosing -= OnSceneClosing;
            EditorSceneManager.sceneClosing += OnSceneClosing;

            Rebuild -= RebuildView;
            Rebuild += RebuildView;

            dirty = true;
            isInit = false;
        }

        private static void OnSceneClosing(Scene scene, bool removingScene)
        {
            if (mapIconLayer != null)
                mapIconLayer.Clear();
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            sceneLoading = true;
            MightyCore.data.scenes[data.sceneIndex].StartCollection();
        }

        private static void EditorUpdate()
        {
            if (!isSceneAnchored) return;

            if (sceneLoading && EditorSceneManager.GetActiveScene().isLoaded)
            {
                sceneLoading = false;
                Rebuild();
            };


            if (sceneData != null)
                if (sceneData.CollectedData == null)
                {
                    sceneData.CollectedData = new List<SceneData.GameObjectData>();
                }
            if (sceneData != null && sceneData.CollectedData.Count == 0)
                sceneData.StartCollection();

            sceneView = GetSceneView();
            if (sceneView != null)
                is2DMode = GetSceneView().in2DMode;
            else
                is2DMode = false;

            data.svPos = GetSVCameraPosition();
            data.svRot = GetSVCameraRotation();
            if (Math.Round(data.svPos.x, 3) != Math.Round(data._svPos.x, 3)
                && Math.Round(data.svPos.z, 3) != Math.Round(data._svPos.z, 3)
                && showSceneCamIcon)
            {
                if (followSceneView)
                {
                    sceneData.MiniMap.Position = data.svPos;
                    sceneData.MiniMap.Rotation = data.svRot;

                    sceneData.MiniMap.Position = data.svPos;
                    if (isSceneAnchored)
                        UpdateMarkers();
                    root.MarkDirtyRepaint();
                }
                else
                {
                    sceneCamIcon.MarkDirtyRepaint();
                }

            }
            else if (data.svRot != data._svRot && showSceneCamIcon)
            {
                sceneData.MiniMap.Rotation = data.svRot;
                if (sceneCamIcon == null) sceneCamIcon = window.rootVisualElement.Q<Button>(name: "sceneCamIcon");
                if (sceneCamIcon != null) sceneCamIcon.style.rotate = new StyleRotate(new Rotate(new Angle(data.svRot.eulerAngles.y)));

                sceneCamIcon.MarkDirtyRepaint();
            }

            data._svPos = data.svPos;
            data._svRot = data.svRot;
        }

        private static void EditorQuit()
        {

        }

        public static void Init()
        {
            if (initializing == false) return;
            initializing = true;
            if (window == null) OpenMightyMapWindow();
            if (MightyCore.isInit == false) MightyCore.Init();
            dirty = false;
            isInit = true;

            window.rootVisualElement.RegisterCallback<GeometryChangedEvent>(GeometryChangedCallback);


            CreateGUI();
            RestoreWindows();
            initializing = false;
        }
        #endregion

        #region Commands

        static void RestoreWindows()
        {
            var windowsCopy = windowManagerStateful.serializableWindows.ToList();

            foreach (var windowState in windowsCopy)
            {
                if (windowState.restorationCommandTypeName == null) continue;
                Type commandType = Type.GetType(windowState.restorationCommandTypeName);
                if (commandType != null)
                {
                    ICommand restoreCommand = (ICommand)Activator.CreateInstance(commandType);
                    restoreCommand?.Execute();
                }
                else
                {
                    DevLogError("Could not find command type: " + windowState.restorationCommandTypeName);
                }
            }
        }


        public class OpenSceneGraphWindowCommand : ICommand
        {
            public OpenSceneGraphWindowCommand()
            {
                // this.root = root;
            }

            public void Execute()
            {
                MightySceneBrowser sceneBrowser = MightySceneBrowser.Load();
                sceneBrowser.BuildView();

                var win = new MightyWindowStateful(sceneBrowser.view,
                                                   typeof(PopOutSceneBrowser),
                                                   "Scene Browser",
                                                   new Vector2(32, 32),
                                                   typeof(OpenSceneGraphWindowCommand));

                if (win.content != null)
                {
                    root.Add(win);
                }
                BuildWindowBar?.Invoke();
            }
        }

        #endregion

        #region Util

        public static bool IsDirty(bool verbose = false)
        {
            if (dirty)
            {
                if (verbose) DevLog($"dirty: {dirty}");
                dirty = false;
                return true;
            }
            if (sceneData.MiniMap.Position.x != sceneData.MiniMap.CachePos.x && sceneData.MiniMap.Position.z != sceneData.MiniMap.CachePos.z)
            {
                if (verbose) DevLog($"sceneData.miniMap.pos == sceneData.miniMap.cachePos: {sceneData.MiniMap.Position} == {sceneData.MiniMap.CachePos} = {sceneData.MiniMap.Position == sceneData.MiniMap.CachePos}");
                return true;
            }
            if (screenWidth != sceneData.MiniMap.WidthCache)
            {
                if (verbose) DevLog($"{screenWidth} != {sceneData.MiniMap.WidthCache}");
                return true;
            }
            if (screenHeight != sceneData.MiniMap.HeightCache)
            {
                if (verbose) DevLog($"{screenHeight} != {sceneData.MiniMap.HeightCache}");
                return true;
            }
            if (sceneData.MiniMap.OrthSize != sceneData.MiniMap.OrthSizeCache)
            {
                if (verbose) DevLog($"{sceneData.MiniMap.OrthSize} != {sceneData.MiniMap.OrthSizeCache}");
                return true;
            }
            if (dirty != false)
            {
                if (verbose) DevLog($"dirty != false");
                return true;
            }
            if (map == null)
            {
                if (verbose) DevLog($"map == null");
                return true;
            }

            return false;
        }

        #endregion

        #region sceneData.miniMap


        static float targetYPos = 0;
        static float currentYPos = 0;
        static float lerpSpeed = 0.5f;
        static float threshold = 0.5f;
        static float ceilingOffset = 100f;

        static public void UpdateMiniMap()
        {

            sceneData.MiniMap.CachePos = sceneData.MiniMap.Position;
            sceneData.MiniMap.WidthCache = screenWidth;
            sceneData.MiniMap.HeightCache = screenHeight;
            sceneData.MiniMap.OrthSizeCache = sceneData.MiniMap.OrthSize;

            GameObject cameraGO = cameraTopDown;
            sceneCamera = cameraGO.GetComponent<Camera>();

            if (sceneCamera.orthographic)
            {
                if (GetSceneView().in2DMode)
                {
                    sceneData.MiniMap.Rotation = Quaternion.Euler(0f, 0f, 0f);

                    sceneData.MiniMap.Position = new Vector3(sceneData.MiniMap.Position.x, sceneData.MiniMap.Position.y, -10f);
                }
                else
                {
                    sceneData.MiniMap.Rotation = Quaternion.Euler(90f, 0f, 0f);
                    RaycastHit hit;
                    float upwardRayLength = 700;
                    Vector3 rayOrigin = GetSVCameraPosition();
                    Vector3 rayDirection = Vector3.up;

                    //Debug.DrawRay(rayOrigin, rayDirection * upwardRayLength, Color.red);

                    if (Physics.Raycast(rayOrigin, rayDirection, out hit, upwardRayLength))
                    {
                        float newYPos = hit.point.y - 0.1f;
                        float deltaY = Mathf.Abs(newYPos - targetYPos);
                        if (deltaY > threshold)
                        {
                            targetYPos = newYPos;
                        }
                    }
                    else
                    {
                        if (Physics.Raycast(rayOrigin + new Vector3(0, 1, 0), -rayDirection, out hit, upwardRayLength))
                        {
                            targetYPos = hit.point.y + ceilingOffset;
                        }
                    }

                    sceneCamera.orthographicSize = sceneData.MiniMap.OrthSize;

                    currentYPos = Mathf.Lerp(currentYPos, targetYPos, Time.deltaTime * lerpSpeed);
                    sceneData.MiniMap.Position = new Vector3(sceneData.MiniMap.Position.x,
                                                            currentYPos,
                                                            sceneData.MiniMap.Position.z);
                }
            }
            else
            {
                sceneData.MiniMap.Position = new Vector3(sceneData.MiniMap.Position.x,
                                                        sceneData.MiniMap.OrthSize,
                                                        sceneData.MiniMap.Position.z);
            }

            if (sceneData.MiniMap.OrthSize < 1)
            {
                sceneData.MiniMap.OrthSize = 1;
                Debug.LogWarning("Mighty Dev Ops: Mighty Map: Orthographic size is too small. Setting to 1.");
            }
            sceneCamera.orthographicSize = sceneData.MiniMap.OrthSize;
            sceneCamera.transform.position = sceneData.MiniMap.Position;
            sceneCamera.transform.rotation = sceneData.MiniMap.Rotation;

            sceneData.MiniMap.PixelHeight = sceneCamera.pixelHeight;
            sceneData.MiniMap.PixelWidth = sceneCamera.pixelWidth;

            sceneData.MiniMap.Topleft = sceneCamera.ScreenToWorldPoint(new Vector3(0, sceneCamera.pixelHeight, sceneCamera.nearClipPlane));
            sceneData.MiniMap.Topright = sceneCamera.ScreenToWorldPoint(new Vector3(sceneCamera.pixelWidth, sceneCamera.pixelHeight, sceneCamera.nearClipPlane));
            sceneData.MiniMap.Botleft = sceneCamera.ScreenToWorldPoint(new Vector3(0, 0, sceneCamera.nearClipPlane));
            sceneData.MiniMap.Botright = sceneCamera.ScreenToWorldPoint(new Vector3(sceneCamera.pixelWidth, 0, sceneCamera.nearClipPlane));

            if (screenWidth != 0 || screenHeight != 0)
            {
                RenderTexture currentRT = new RenderTexture((int)screenWidth, (int)screenHeight, 24);
                sceneCamera.targetTexture = currentRT;
                sceneCamera.Render();

                RenderTexture.active = currentRT;

                sceneData.MiniMap.map = new Texture2D((int)screenWidth, (int)screenHeight, TextureFormat.RGB24, false);
                sceneData.MiniMap.map.ReadPixels(new Rect(0, 0, screenWidth, screenHeight), 0, 0);

                sceneData.MiniMap.map.Apply();
                sceneCamera.targetTexture = null;
                RenderTexture.active = null;
            }
            if (sceneData.MiniMap.map == null) sceneData.MiniMap.map = MakeTex((int)screenWidth, (int)screenHeight, Color.blue);

            map.style.backgroundImage = sceneData.MiniMap.map;

            if (isSceneAnchored)
            {
                if (firstRun)
                {
                    sceneData.MiniMap.SaveImage();
                    firstRun = false;
                }
                else if (sceneData.MiniMap.MapPath == "")
                {
                    sceneData.MiniMap.SaveImage();
                }
            }
            MightyCoreData.sceneData.MiniMap.SaveImage();
        }

        #endregion

        #region GUI

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    cameraTopDown.SetActive(false);
                    Rebuild();
                    break;
                case PlayModeStateChange.EnteredPlayMode:

                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    cameraTopDown.SetActive(true);

                    foreach (var mappable in mappables)
                    {
                        mappable.RegisterMappable();
                    }
                    Rebuild();
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    break;
            }
        }

        void OnGUI()
        {
            // if (EditorApplication.playModeStateChanged == )
            // {
            //     cameraTopDown.SetActive(false);
            //     Rebuild();
            // }
            // if(EditorApplication.is)
            if (!enabled) return;

            if (!isInit)
            {
                Init();
            }
            if (sceneData == null) return;

            if (sceneData.isCollecting)
                sceneData.UpdateDeepDive();

            screenWidth = window.position.width;
            screenHeight = window.position.height;
            Camera camera = Application.isPlaying ? Camera.main : GetSceneView().camera;
            data.svPos = camera.transform.position;
            data.svRot = camera.transform.rotation;
            svOrthSize = GetSVCOrthographicSize();

            if (!IsDirty()) UpdateMiniMap();


            var targetRatio = (float)screenWidth / (float)screenHeight;

            var hh = sceneData.MiniMap.Topleft.z - sceneData.MiniMap.Botleft.z;
            var ww = hh * targetRatio;
            float xOffset = ((sceneData.MiniMap.Topright.x - sceneData.MiniMap.Topleft.x) / 2) - (ww / 2);
            float x1 = sceneData.MiniMap.Topleft.x + xOffset;
            float x2 = sceneData.MiniMap.Topright.x - xOffset;
            float z1 = sceneData.MiniMap.Botright.z;
            float z2 = sceneData.MiniMap.Topleft.z;

            sceneCamIcon ??= window.rootVisualElement.Q<Button>(name: "sceneCamIcon");

            if (sceneCamIcon != null)
                if (data.svPos.x >= x1 && data.svPos.x <= x2 &&
                    data.svPos.z >= z1 && data.svPos.z <= z2)
                {
                    showSceneCamIcon = true;
                    var xx = (1 - ((x2 - data.svPos.x) / ww)) * screenWidth;
                    var zz = (1 - ((data.svPos.z - z1) / hh)) * screenHeight;

                    sceneCamIcon.style.display = DisplayStyle.Flex;
                    if (!isHoveringOnMappable) sceneCamIcon.BringToFront();
                    sceneCamIcon.style.top = zz - 8;
                    sceneCamIcon.style.left = xx - 8;
                    sceneCamIcon.style.rotate = new StyleRotate(new Rotate(new Angle(camera.transform.rotation.eulerAngles.y)));
                    sceneCamIcon.MarkDirtyRepaint();
                }
                else
                {
                    sceneCamIcon.style.display = DisplayStyle.None;

                }

            if (IsDirty(true)) UpdateView();
        }

        static bool enabled = true;

        static void InitDropDownRunIds()
        {
            // DevLog("InitDropDownRunIds");
            // DropdownField dropDown = root.Q<DropdownField>("dd_run_ids");
            // dropDown.choices = sceneData.run_ids.OrderBy(x => x).ToList();

            // dropDown.RegisterValueChangedCallback((evt) =>
            //         {

            //             sceneData.run_selected = evt.newValue;
            //             DevLog(sceneData.run_selected);
            //             ClearMarkers();
            //             UpdateMarkers();
            //             RefreshSceneView();
            //         });

        }
        static void CreateGUI()
        {
            // DevLog($"CreateGUI map is null: {map == null}");
            // if (map != null) return;

            DevLog("CreateGUI");
            if (mappables == null)
            {
                MightyCore.Init();
            }

            //if (root != null) root.Clear();
            root = GetWindow().rootVisualElement;

            if (mightyStylesheet == null)
                mightyStylesheet = Resources.Load<StyleSheet>("UI/mightystyles");

            if (!root.styleSheets.Contains(mightyStylesheet))
            {
                root.styleSheets.Add(mightyStylesheet);
            }

            root.AddToClassList("root");

            if (map == null)
            {
                map = new()
                {
                    name = "map",
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        justifyContent = Justify.FlexStart,
                        position = Position.Relative,
                        alignItems = Align.FlexStart,
                        flexGrow = 1,
                        flexShrink = 1,
                    }
                };
                root.Add(map);
            }

            if (root.Q<VisualElement>(name: "container") != null) root.Q<VisualElement>(name: "container").RemoveFromHierarchy();
            VisualElement container = new()
            {
                name = "container",
                pickingMode = PickingMode.Ignore,
                style =
            {
                flexDirection = FlexDirection.Column,
                justifyContent = Justify.FlexStart,
                position = Position.Absolute,
                alignItems = Align.FlexStart,
                flexGrow = 1,
                flexShrink = 1,
                height = Length.Percent(100),
                width = Length.Percent(100),
            }
            };

            top = new()
            {
                name = "top",
                style =
            {
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.FlexStart,
                alignItems = Align.FlexStart,
                height = 32,
                flexGrow = 0,
                flexShrink = 0,
                backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.1f),
                width = Length.Percent(100)
            }
            };

            container.Add(top);

            mid = new()
            {
                name = "middle",
                pickingMode = PickingMode.Ignore,
                style =
            {
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.FlexStart,
                alignItems = Align.FlexStart,
                flexGrow = 1,
                flexShrink = 0,
                width = Length.Percent(100),
            }
            };

            Button addNewMappable = new()
            {
                name = "addNewMappable",
                text = "+",
                style =
            {
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.FlexStart,
                alignItems = Align.FlexStart,
                position = Position.Absolute,
                height = 64,
                width = 64,
                flexGrow = 0,
                flexShrink = 0,
                backgroundColor = Color.white,
                unityTextAlign = TextAnchor.MiddleCenter,
                fontSize = 64,
                bottom = 16,
                right = 16,
            }
            };
            addNewMappable.style.borderTopLeftRadius = addNewMappable.style.borderTopRightRadius = addNewMappable.style.borderBottomLeftRadius = addNewMappable.style.borderBottomRightRadius = 360;
            //mid.Add(addNewMappable);
            addNewMappable.style.color = Color.black;

            container.Add(mid);


            //mid.Add(sceneData.GetProgressBar());

            bot = new()
            {
                name = "bottom",
                style =
        {
            flexDirection = FlexDirection.RowReverse,
            justifyContent = Justify.FlexStart,
            alignItems = Align.FlexStart,
            height = 32,
            width = Length.Percent(100),
            flexGrow = 0,
            flexShrink = 0,
        }
            };

            container.Add(bot);

            windowBar = new()
            {
                name = "WindowBar",
                style =
            {
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.FlexStart,
                alignItems = Align.FlexStart,
                height = 32,
                width = Length.Percent(100),
                flexGrow = 0,
                flexShrink = 0,
                backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f)
            }
            };
            //container.Add(windowBar);



            root.Add(container);

            var mappablesCopy = new List<IMappable>(mappables);
            foreach (var mappable in mappablesCopy)
            {
                mappable.RegisterMappable();
            }


            var button = new Button
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexStart,
                    alignItems = Align.Center,
                    height = 30,
                    // marginTop = 10,
                    // marginBottom = 10,
                    // marginLeft = 20,
                    // marginRight = 20,
                    paddingLeft = 10,
                    paddingRight = 10,
                    paddingTop = 5,
                    paddingBottom = 5,
                    borderLeftWidth = 2,
                    borderRightWidth = 2,
                    borderTopWidth = 2,
                    borderBottomWidth = 2,
                    borderLeftColor = Color.gray,
                    borderRightColor = Color.gray,
                    borderTopColor = Color.gray,
                    borderBottomColor = Color.gray,
                    backgroundColor = Color.black
                }
            };

            var icon = new Image
            {
                image = icons.mightyeye,
                style =
                {
                    width = 24,
                    height = 24,
                    marginRight = 5
                }
            };
            button.Add(icon);

            var label = new Label
            {
                text = "Scene Browser",
                style =
                {
                    fontSize = 14,
                    unityFontStyleAndWeight = FontStyle.Bold,
                }
            };

            button.Add(label);
            button.clicked += () =>
            {
                ICommand command = new OpenSceneGraphWindowCommand();
                command.Execute();
            };
            top.Add(button);



            // }
            // else
            // {
            //     mid.Q<Button>(name: "addSceneAnchor")?.RemoveFromHierarchy();
            // }

            button = new Button()
            {
                name = "followSceneView",
                tooltip = "Follow Scene View",
                style = {
                    width = 32,
                    height = 32,
                    top = 16,
                    right = 16,
                    backgroundColor= new Color(0,0,0,0),
                    borderTopWidth = 0,
                    borderRightWidth = 0,
                    borderBottomWidth = 0,
                    borderLeftWidth = 0,
                    position = Position.Absolute,
                }
            };

            if (icons.map_follow_sceneview == null) icons = new();
            button.style.backgroundImage = icons.map_follow_sceneview;

            button.clicked += () =>
            {
                followSceneView = true;
            };
            top.Add(button);

            float toggleIconWidth = 96 + 15;
            #region sideMenu
            sideMenu = new()
            {
                name = "sideMenu",
                style = {
                    display = DisplayStyle.Flex,
                    flexDirection = FlexDirection.Column,
                    justifyContent = Justify.FlexStart,
                    alignItems = Align.FlexStart,
                    overflow = Overflow.Hidden,
                    height = Length.Percent(100),
                    width = toggleIconWidth,
                    left = -toggleIconWidth,
                    flexGrow = 0,
                    flexShrink = 0,
                    backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.9f),
                    transitionProperty = new List<StylePropertyName>
                    {
                        new StylePropertyName("left"),
                        new StylePropertyName("width")
                    },
                    transitionDuration = new List<TimeValue>()
                    {
                        new TimeValue(transitionSpeed, TimeUnit.Millisecond)
                    }
                }
            };


            VisualElement sideMenuContentContainer = new()
            {
                name = "sideMenuContentContainer",
                style = {
                    flexDirection = FlexDirection.Column,
                    justifyContent = Justify.FlexStart,
                    alignItems = Align.FlexStart,
                    overflow = Overflow.Hidden,
                    height = 1,
                    width = 196,
                    backgroundColor = new Color(0.5f, 0.5f, 1.0f, 0.9f),
                    flexGrow = 1,
                    flexShrink = 0,
            }
            };


            VisualElement sideMenuTitleBar = new()
            {
                name = "sideMenuTitleBar",
                style = {
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.FlexStart,
                alignItems = Align.FlexStart,
                height = 0,
                width = 196,
                top=-48,
                flexGrow = 0,
                flexShrink = 0,
                backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f),
            }
            };


            VisualElement sideMenuTitleIcon = new()
            {
                name = "sideMenuTitleIcon",
                style = {
                width = 48,
                height = 48,
                flexGrow = 0,
                flexShrink = 0,
                backgroundColor = new Color(1f, 0.1f, 0.1f, 0.9f),
            }
            };
            sideMenuTitleBar.Add(sideMenuTitleIcon);

            sideMenuTitleIcon.RegisterCallback<MouseDownEvent>((evt) =>
                    {
                        CloseModuleSubMenu?.Invoke();
                    });

            Label sideMenuTitle = new()
            {
                text = "Modules",
                style = {
                fontSize = 24,
                color = Color.white,
                height= Length.Percent(100),
                flexGrow = 1,
                unityTextAlign = TextAnchor.MiddleCenter,
                transitionProperty = new List<StylePropertyName>
                {
                    new StylePropertyName("top"),
                    new StylePropertyName("height"),
                },
                transitionDuration = new List<TimeValue>()
                {
                    new TimeValue(transitionSpeed, TimeUnit.Millisecond)
                }
            }
            };
            sideMenuTitleBar.Add(sideMenuTitle);


            sideMenuContentContainer.Add(sideMenuTitleBar);

            ScrollView sideMenuContent = new()
            {
                name = "sideMenuContent",
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
                style = {
                flexDirection = FlexDirection.Column,
                justifyContent = Justify.FlexStart,
                alignItems = Align.FlexStart,
                overflow = Overflow.Hidden,
                width = 0,
                flexGrow = 0,
                flexShrink = 1,
                backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.9f),
                transitionProperty = new List<StylePropertyName>
                    {
                        new StylePropertyName("width")
                    },
                    transitionDuration = new List<TimeValue>()
                    {
                        new TimeValue(transitionSpeed, TimeUnit.Millisecond)
                    }
            }
            };

            sideMenuContentContainer.Add(sideMenuContent);

            sideMenu.Add(sideMenuContentContainer);


            mid.Add(sideMenu);


            VisualElement sideMenuButton = new()
            {
                name = "sideMenuButton",
                style = {
                height = Length.Percent(100),
                width = 32,
                left=-toggleIconWidth,
                backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f),
                justifyContent = Justify.Center,
                alignItems = Align.Center,
            }
            };

            label = new()
            {
                text = "≡",
                style = {
                fontSize = 32,
                color = Color.white,
            }
            };
            sideMenuButton.Add(label);


            sideMenuButton.RegisterCallback<MouseDownEvent>((evt) =>
                    {
                        if (sideMenu.resolvedStyle.left == 0)
                        {
                            sideMenu.style.left = -toggleIconWidth;
                            sideMenuButton.style.left = -toggleIconWidth;
                            CloseModuleSubMenu?.Invoke();
                        }
                        else
                        {
                            sideMenu.style.left = 0;
                            sideMenuButton.style.left = 0;
                        }
                    });

            sideMenuButton.style.transitionProperty = new List<StylePropertyName>
            {
                new StylePropertyName("left")
            };

            sideMenuButton.style.transitionDuration = new List<TimeValue>()
            {
                new TimeValue(transitionSpeed, TimeUnit.Millisecond)
            };

            mid.Add(sideMenuButton);

            ScrollView sideMenuScrollView = new()
            {
                name = "sideMenuScrollView",
                verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible,
                style = {
                height = Length.Percent(100),
                width = toggleIconWidth,
            }
            };

            DevLog($"data.mapShapes is Null?? {data.MappableTypesInfo == null}");
            int count = 0;
            foreach (var typeInfo in data.MappableTypesInfo)
            {
                if (typeInfo.Mappable == null)
                {
                    DevLogError($"TypeInfo Mappable is null for {typeInfo.Name}");
                    continue; // Skip this iteration as Mappable is null
                }

                DevLog($"typeInfo.IsActive: {typeInfo.IsActive}");
                DevLog($"toggleInfo.Name: {typeInfo.Name}");

                DevLog($"typeInfo.name = {typeInfo.Name}");
                DevLog($"typeInfo.Mappable is null? {typeInfo.Mappable == null}");

                CustomToggleButton toggleModule = typeInfo.Mappable.AddModuleToggle(typeInfo);
                toggleModule.style.width = toggleIconWidth - 14;
                toggleModule.style.height = toggleIconWidth - 14;
                toggleModule.style.position = Position.Absolute;
                toggleModule.style.top = toggleModule.topCache = count * (toggleIconWidth - 15);
                toggleModule.isToggledOn = typeInfo.IsActive;

                string localName = typeInfo.Name;

                toggleModule.RegisterValueChangedCallback((ChangeEvent<bool> evt) =>
                {
                    var infoToUpdate = data.MappableTypesInfo.Find(info => info.Name == localName);
                    if (infoToUpdate != null)
                    {
                        infoToUpdate.IsActive = evt.newValue;
                        DevLog($"Mappable Updated {infoToUpdate.TypeName} to {infoToUpdate.IsActive}");
                    }
                    else
                    {
                        DevLog($"mappable infoToUpdate is Null?? {infoToUpdate == null}");
                    }
                    DevLog($"Mappable Updated {localName} to {evt.newValue}");
                    UpdateMiniMap();
                    if (isSceneAnchored)
                    {
                        UpdateMappables();
                        UpdateMarkers();
                    }
                });

                sideMenuScrollView.Add(toggleModule);
                count++;
                // data.mapShapes[i].active = GUILayout.Toggle(data.mapShapes[i].active,
                //                                                 data.mapShapes[i].ToString());
            }

            sideMenu.Add(sideMenuScrollView);

            OpenModuleSubMenu += () =>
            {
                sideMenu.style.width = 196;
                sideMenuTitleBar.style.top = 0;
                sideMenuTitleBar.style.height = 48;
                sideMenuContent.style.width = 196;
                sideMenuScrollView.style.height = 0;
                sideMenuScrollView.style.width = 0;
                sideMenuTitle.text = selectedModule.mappableTypeInfo.Name;
                sideMenuTitleIcon.style.backgroundImage = selectedModule.mappableTypeInfo.Mappable.Icon;

                sideMenuContent.Clear();
                sideMenuContent.Add(selectedModule.mappableTypeInfo.Mappable.SettingsView());
            };

            CloseModuleSubMenu += () =>
            {
                sideMenu.style.width = toggleIconWidth;
                sideMenuTitleBar.style.top = -48;
                sideMenuTitleBar.style.height = 0;
                sideMenuContent.style.width = 0;
                sideMenuScrollView.style.height = Length.Percent(100);
                sideMenuScrollView.style.width = toggleIconWidth;
                sideMenuScrollView.MarkDirtyRepaint();

                sideMenuContent = new()
                {
                    name = "sideMenuContent",
                    horizontalScrollerVisibility = ScrollerVisibility.Hidden,
                    style = {
                flexDirection = FlexDirection.Column,
                justifyContent = Justify.FlexStart,
                alignItems = Align.FlexStart,
                overflow = Overflow.Hidden,
                // height = 200,
                // height = Length.Percent(100),
                width = 0,
                flexGrow = 0,
                flexShrink = 1,
                backgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.9f),
                transitionProperty = new List<StylePropertyName>
                    {
                        new StylePropertyName("width")
                    },
                    transitionDuration = new List<TimeValue>()
                    {
                        new TimeValue(transitionSpeed, TimeUnit.Millisecond)
                    }
                }
                };
                sideMenuContent.MarkDirtyRepaint();
                sideMenu.MarkDirtyRepaint();
            };
            #endregion

            #region sceneAnchorCheck
            // if (!isSceneAnchored)
            // {
            //     DevLog($"isSceneAnchored: {isSceneAnchored} creating button on minimap");
            addSceneAnchor = new()
            {
                name = "addSceneAnchor",
                style =
                {
                    backgroundColor = Color.black,
                    width = Length.Percent(100),
                    height = Length.Percent(100),
                    fontSize = 12,
                    display = DisplayStyle.None,
                    justifyContent = Justify.Center, // Center horizontally
                    alignItems = Align.Center, // Center vertically
                }
            };

            if (icons.mightybot == null) icons = new();
            VisualElement mightyBot = new()
            {
                name = "mightyBot",
                style = {
                    backgroundImage = icons.mightybot,
                    width=64,
                    height=64,
                    flexGrow = 0,
                    flexShrink = 0,
                }
            };
            addSceneAnchor.Add(mightyBot);
            Label addSceneAnchorLabel = new()
            {
                name = "addSceneAnchorLabel",
                text = "Enable Mighty DevOps on this Scene?",
                style = {
                    color = Color.white,
                    // width= Length.Percent(100),
                    // height= Length.Percent(100),
                    flexGrow = 0,
                    flexShrink = 0,
                    unityTextAlign = TextAnchor.MiddleCenter,
                }
            };
            addSceneAnchor.Add(addSceneAnchorLabel);
            addSceneAnchor.RegisterCallback<MouseDownEvent>((evt) =>
            {
                if (EditorUtility.DisplayDialog("Create Scene Anchor", "Mighty Dev Ops needs to anchor to this scene.  This creates a small Editor Only reference object within your scene.  Would you like to anchor now?", "Yes", "No"))
                {
                    var go = GameObject.Find("MightySceneAnchor");
                    var sceneIndex = 0;
                    if (go != null)
                    {
                        sceneAnchor = go.GetComponent<MightySceneAnchor>();
                        if (sceneAnchor == null)
                        {
                            go.AddComponent<MightySceneAnchor>();
                            sceneAnchor = go.GetComponent<MightySceneAnchor>();
                            sceneAnchor.DataSetName = $"{SceneManager.GetActiveScene().name}___{SceneManager.GetActiveScene().path.Replace("/", "_").Replace(".unity", "")}";
                        }

                        bool hasEntry = data.scenes.Any(scene => scene.Name == sceneAnchor.DataSetName);
                        if (!hasEntry)
                            data.scenes.Add(new SceneData() { Name = sceneAnchor.DataSetName });

                        isSceneAnchored = true;
                        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                        UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
                        return;

                    }
                    go ??= new GameObject("MightySceneAnchor")
                    {
                        tag = "EditorOnly",
                    };

                    go.hideFlags = HideFlags.HideInHierarchy;

                    EditorUtility.SetDirty(go);
                    go.AddComponent<MightySceneAnchor>();
                    sceneAnchor = go.GetComponent<MightySceneAnchor>();
                    sceneAnchor.DataSetName = $"{SceneManager.GetActiveScene().name}___{SceneManager.GetActiveScene().path.Replace("/", "_").Replace(".unity", "")}";
                    data.scenes.Add(new SceneData());
                    sceneIndex = MightyCore.data.scenes.Count - 1;
                    data.scenes[sceneIndex].Name = sceneAnchor.DataSetName;
                    //UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
                    isSceneAnchored = true;
                    EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                    UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
                }
            });
            container.Add(addSceneAnchor);
            #endregion


            #region playthroughs
            var dropDown = new DropdownField
            {
                name = "dd_run_ids",
                style ={
                width=64
            }
            };

            //top.Add(dropDown);
            #endregion

            #region searchMap
            ToolbarPopupSearchField searchField = new ToolbarPopupSearchField();

            searchField.menu.AppendAction("Name Search", (a) =>
            {
                currentSearchType = SearchType.Name;
                DevLog("Changed to Name Search.");
            }, (a) => currentSearchType == SearchType.Name ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

            searchField.menu.AppendAction("Deep Search", (a) =>
            {
                currentSearchType = SearchType.Deep;
                DevLog("Changed to Deep Search.");
            }, (a) => currentSearchType == SearchType.Deep ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

            searchField.menu.AppendSeparator();

            searchField.menu.AppendAction("Case Sensitive", (a) =>
            {
                isCaseSensitive = !isCaseSensitive;
                DevLog($"Case sensitivity is now {(isCaseSensitive ? "enabled" : "disabled")}.");
            }, (a) => isCaseSensitive ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

            searchField.RegisterValueChangedCallback((evt) =>
                    {
                        searchQuery = evt.newValue;
                        DevLog($"Search string changed to {searchQuery}.");

                        sceneData.PerformSearch(searchQuery);
                    });

            PopulateWindowBar();

            for (int i = 0; i < data.MappableTypesInfo.Count; i++)
            {
                Button x = data.MappableTypesInfo[i].Mappable.AddMappable();
                DevLog($"mappable {i} button is null = {x == null}");
                x.style.width = 48;
                x.style.height = 48;
                x.style.bottom = 24;
                x.style.right = 8;
                x.style.transitionProperty = new List<StylePropertyName>
                {
                    new StylePropertyName("width"),
                    new StylePropertyName("height"),
                    new StylePropertyName("bottom"),
                    new StylePropertyName("right"),
                };
                x.style.transitionDuration = new List<TimeValue>()
                {
                    new TimeValue(transitionSpeed, TimeUnit.Millisecond)
                };
                x.style.transitionTimingFunction = new List<EasingFunction> { EasingMode.Linear };
                x.visible = true;
                addMappableButtons.Add(x);
                bot.Add(x);

                x.RegisterCallback<MouseEnterEvent>(e =>
                {
                    x.style.width = 64;
                    x.style.height = 64;
                    x.style.bottom = 32;
                    x.style.right = 0;
                });

                x.RegisterCallback<MouseLeaveEvent>(e =>
                {
                    x.style.width = 48;
                    x.style.height = 48;
                    x.style.bottom = 24;
                    x.style.right = 8;
                });

                var slider = new MinMaxSlider();
                slider.lowLimit = 0;
                slider.highLimit = 100;
                slider.minValue = 0;
                slider.maxValue = 100;
                slider.style.width = 200;

                slider.RegisterValueChangedCallback((evt) =>
                {
                    //run_playbackCursor = evt.newValue;
                    sceneData.RunPlaybackRange = evt.newValue;
                    DevLog("Slider value changed to " + evt.newValue);
                    UpdateMiniMap();
                    if (isSceneAnchored)
                    {
                        UpdateMappables();
                        UpdateMarkers();
                    }
                });
                //bot.Add(slider);
            }
            #endregion

            map.RegisterCallback<MouseLeaveEvent>(e =>
            {
                isAdjust = false;
            }, TrickleDown.NoTrickleDown);

            map.RegisterCallback<MouseEnterEvent>(e =>
            {
                isAdjust = false;
            }, TrickleDown.NoTrickleDown);


            map.RegisterCallback<WheelEvent>(e =>
                {
                    // if (zoomMarker != null)
                    // {
                    //     root.Remove(zoomMarker);
                    // }
                    followSceneView = false;

                    Vector3 mouseWorldPosBefore = GetWorldPositionFromMouse(e.mousePosition, sceneData.MiniMap.Position, sceneData.MiniMap.OrthSize, screenWidth, screenHeight);

                    if (e.delta.y > 0)
                    {
                        sceneData.MiniMap.OrthSize *= 1.05f;
                    }
                    else
                    {
                        sceneData.MiniMap.OrthSize /= 1.05f;
                    }

                    Vector3 mouseWorldPosAfter = GetWorldPositionFromMouse(e.mousePosition, sceneData.MiniMap.Position, sceneData.MiniMap.OrthSize, screenWidth, screenHeight);

                    sceneData.MiniMap.Position += (mouseWorldPosBefore - mouseWorldPosAfter);
                    sceneData.MiniMap.CachePos = sceneData.MiniMap.Position;

                    DevLog($"New Camera Position: {sceneData.MiniMap.Position}");
                    DevLog($"Mouse Position: {e.mousePosition}");
                    DevLog($"Old Camera Position: {sceneData.MiniMap.Position}");
                    DevLog($"New Camera Position: {sceneData.MiniMap.Position}");
                    DevLog($"Orthographic Size: {sceneData.MiniMap.OrthSize}");
                    DevLog($"World Position Before: {mouseWorldPosBefore}");
                    DevLog($"World Position After: {mouseWorldPosAfter}");
                    if (isSceneAnchored)
                    {
                        UpdateMappables();
                        UpdateMarkers();
                    }
                    UpdateMiniMap();

                    // zoomMarker = new VisualElement();
                    // zoomMarker.style.width = 10;
                    // zoomMarker.style.height = 10;
                    // zoomMarker.style.backgroundColor = new StyleColor(Color.red);
                    // zoomMarker.style.position = Position.Absolute;
                    // zoomMarker.style.left = e.mousePosition.x - 5;
                    // zoomMarker.style.top = e.mousePosition.y - 5;

                    // root.Add(zoomMarker);
                }, TrickleDown.NoTrickleDown);




            map.RegisterCallback<MouseDownEvent>(e =>
            {
                isAdjust = true;
            }, TrickleDown.NoTrickleDown);

            map.RegisterCallback<MouseUpEvent>(e =>
            {
                isAdjust = false;
            }, TrickleDown.TrickleDown);

            map.RegisterCallback<MouseMoveEvent>(e =>
            {
                if (isAdjust)
                {
                    if (GetSceneView().in2DMode)
                    {
                        sceneData.MiniMap.Position = new Vector3(sceneData.MiniMap.Position.x + -e.mouseDelta.x * (sceneData.MiniMap.OrthSize / 100),
                            sceneData.MiniMap.Position.y + e.mouseDelta.y * (sceneData.MiniMap.OrthSize / 100),
                            sceneData.MiniMap.Position.z);
                    }
                    else
                    {
                        sceneData.MiniMap.Position = new Vector3(sceneData.MiniMap.Position.x + -e.mouseDelta.x * (sceneData.MiniMap.OrthSize / 100),
                            sceneData.MiniMap.Position.y,
                            sceneData.MiniMap.Position.z + e.mouseDelta.y * (sceneData.MiniMap.OrthSize / 100));
                    }

                    followSceneView = false;

                    UpdateMiniMap();
                    if (isSceneAnchored)
                    {
                        UpdateMappables();
                        UpdateMarkers();
                    }

                }
            }, TrickleDown.NoTrickleDown);
            //mapIconLayer.style.backgroundImage = sceneData.miniMap.map;

            //map.generateVisualContent += OnGenerateVisualContent;

            if (map.Q<Button>(name: "sceneCamIcon") == null)
            {
                sceneCamIcon = new Button();
                sceneCamIcon.style.position = Position.Absolute;
                sceneCamIcon.style.width = sceneCamIcon.style.height = 16;
                sceneCamIcon.name = "sceneCamIcon";
                if (icons.mmCamera == null) icons = new Icons();
                sceneCamIcon.style.backgroundImage = icons.mmCamera;
                sceneCamIcon.style.backgroundColor =
                sceneCamIcon.style.borderTopColor =
                sceneCamIcon.style.borderBottomColor =
                sceneCamIcon.style.borderLeftColor =
                sceneCamIcon.style.borderRightColor =
                new Color(0, 0, 1, 1);

                sceneCamIcon.clicked += () => followSceneView = true;


                map.Add(sceneCamIcon);
            }

            root.RegisterCallback<MouseMoveEvent>(e =>
                    {
                        e.PreventDefault();
                    });


            map.RegisterCallback<MouseDownEvent>(evt =>
                        {
                            // if (evt.button == 1)  // Right mouse button
                            // {
                            //     DevLog($"Right mouse button clicked at {evt.localMousePosition} / {evt.mousePosition}");

                            //     VisualElement radialMenu = CreateRadialMenu(evt.localMousePosition, addMappableButtons);
                            //     map.Add(radialMenu);
                            //     evt.StopPropagation();
                            // }
                        });
            BuildMappables();
            if (BuildTopUI != null) BuildTopUI();

            if (isSceneAnchored)
                InitPlaythroughs();

            GUILoaded = true;
        }

        public static VisualElement CreateRadialMenu(Vector2 clickPosition, List<Button> menuItems)
        {
            var container = new VisualElement
            {
                style =
    {
        position = Position.Absolute,
        left = clickPosition.x - 16,
        top = clickPosition.y - 16
    }
            };

            var centralCircle = new VisualElement
            {
                style =
    {
        width = 32,
        height = 32,
        backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1.0f),
        position = Position.Absolute,
    }
            };

            centralCircle.style.borderTopLeftRadius = centralCircle.style.borderTopRightRadius = centralCircle.style.borderBottomLeftRadius = centralCircle.style.borderBottomRightRadius = 16;
            container.Insert(0, centralCircle);

            int numberOfItems = menuItems.Count;
            float angleStep = 360f / numberOfItems;

            for (int i = 0; i < numberOfItems; i++)
            {

                var item = menuItems[i];
                item.style.position = Position.Absolute;
                item.style.left = 0;
                item.style.top = 0;
                item.style.width = 0;
                item.style.height = 0;
                item.style.borderTopLeftRadius = item.style.borderTopRightRadius = item.style.borderBottomLeftRadius = item.style.borderBottomRightRadius = 32;


                container.Add(item);
            }
            for (int i = 0; i < numberOfItems; i++)
            {
                float angleRad = Mathf.Deg2Rad * (i * angleStep);
                float xPos = Mathf.Cos(angleRad) * 50 - 4;
                float yPos = Mathf.Sin(angleRad) * 50 - 4;

                var item = menuItems[i];
                item.name = menuItems[i].text;
                item.AddToClassList("menu-item");
                item.style.left = xPos;
                item.style.top = yPos;
                item.style.width = 32;
                item.style.height = 32;

                container.Add(item);
            }

            var styleSheet = Resources.Load("RadialMenu") as StyleSheet;
            container.styleSheets.Add(styleSheet);

            return container;
        }

        //static VisualElement zoomMarker;

        static Vector3 GetWorldPositionFromMouse(Vector2 mousePosition, Vector3 camPos, float orthSize, float screenWidth, float screenHeight)
        {
            bool is2DMode = GetSceneView().in2DMode;
            float x1 = camPos.x - orthSize;
            float x2 = camPos.x + orthSize;
            float z1 = is2DMode ? (camPos.y - orthSize) : (camPos.z - orthSize);
            float z2 = is2DMode ? (camPos.y + orthSize) : (camPos.z + orthSize);
            float ww = x2 - x1;
            float hh = z2 - z1;

            float xx = x1 + ((mousePosition.x) / screenWidth * ww);
            float zz = z1 + ((screenHeight - mousePosition.y) / screenHeight * hh);

            DevLog($"x1: {x1} / x2: {x2} / z1: {z1} / z2: {z2} / ww: {ww} / hh: {hh} / xx: {xx} / zz: {zz} / camPos: {camPos} / mousePosition: {mousePosition} / orthSize: {orthSize} / screenWidth: {screenWidth} / screenHeight: {screenHeight} / is2DMode: {is2DMode}");
            if (is2DMode)
            {
                return new Vector3(xx, zz, camPos.z);
            }
            else
            {
                return new Vector3(xx, camPos.y, zz);
            }
        }

        private static void PopulateWindowBar()
        {
            windowBar.Clear();
            windowBar.Add(windowManagerStateful.PopulateWindowBar());
        }

        public static void BuildMappables()
        {
            if (buildMappables == true) return;
            buildMappables = true;

            if (mappables == null)
            {
                MightyCore.Init();
            }

            if (map != null && mapIconLayer != null && root.Q<VisualElement>(name: "MapIconLayer") != null && root.Q<VisualElement>(name: "map") != null)
            {
                mapIconLayer.Clear();
            }

            if (mapIconLayer == null)
                mapIconLayer = new()
                {
                    name = "MapIconLayer"
                };

            map ??= new();

            var mappablesToRemove = new List<IMappable>();

            foreach (var mappable in mappables)
            {
                if (mappable.AnchorTo != sceneData.Name)
                {
                    mappablesToRemove.Add(mappable);
                }
            }

            foreach (var mappableToRemove in mappablesToRemove)
            {
                mappables.Remove(mappableToRemove);
            }


            var ae = mapIconLayer.Query<VisualElement>("[class^='lm_anchor_']");
            if (ae == null) ae = new();
            var anchorElements = ae.ToList();

            foreach (var element in anchorElements)
            {
                var className = element.GetClasses().FirstOrDefault();
                if (className != null && !className.Equals("lm_anchor_" + sceneData.Name))
                {
                    element.RemoveFromHierarchy();
                }
            }


            int i = 0;
            foreach (var mappable in mappables)
            {
                i++;
                var view = mappable.ViewUI.GetView();
                if (view == null) continue;
                if (view == mappable.PrevView) continue;

                var _mappable = view.Q<VisualElement>(className: "mappable");
                if (_mappable == null)
                {
                    DevLog("_mappable is null");
                    continue;
                }

                mappable.ViewUI.SetRoot(mapIconLayer);
                view.style.position = Position.Absolute;

                if (mappable.HasVisualContent & map != null)
                {
                    map.generateVisualContent -= mappable.OnGenerateVisualContent;
                    map.generateVisualContent += mappable.OnGenerateVisualContent;
                }
                mapIconLayer.Add(view);
            }

            mapIconLayer.style.backgroundColor = new Color(0, 0, 0, 0.5f);
            mapIconLayer.style.position = Position.Absolute;

            map.Add(mapIconLayer);
            mapIconLayer.BringToFront();
            if (mapMarkerLayer == null)
            {
                mapMarkerLayer = new VisualElement();
                mapMarkerLayer.name = "MapMarkerLayer";
                mapMarkerLayer.style.position = Position.Absolute;
                map.Add(mapMarkerLayer);
            }

            buildMappables = false;
            UpdateMappables();
        }

        class Cluster
        {
            public VisualElement view;
            public float zoomAtCreation = 0f;

            public Cluster(VisualElement view, float zoomAtCreation)
            {
                this.view = view;
                this.zoomAtCreation = zoomAtCreation;
            }
        }

        private static List<Cluster> clusterList = new();
        public static void UpdateMappables()
        {
            if (!isSceneAnchored) return;
            if (GetSceneView() == null) return;
            if (updatingMappables == true) return;

            updatingMappables = true;

            bool is2DMode = GetSceneView().in2DMode;
            if (window != null)
            {
                screenWidth = window.position.width;
                screenHeight = window.position.height;
            }

            var targetRatio = screenWidth / screenHeight;
            hh = is2DMode ? sceneData.MiniMap.Topleft.y - sceneData.MiniMap.Botleft.y : sceneData.MiniMap.Topleft.z - sceneData.MiniMap.Botleft.z;
            ww = hh * targetRatio;

            if (hh == 0 || ww == 0)
            {
                DevLogError("Invalid values for hh or ww!");
                return;
            }

            float xOffset = ((sceneData.MiniMap.Topright.x - sceneData.MiniMap.Topleft.x) / 2) - (ww / 2);
            float x1 = sceneData.MiniMap.Topleft.x + xOffset;
            float x2 = sceneData.MiniMap.Topright.x - xOffset;
            float z1 = is2DMode ? sceneData.MiniMap.Botright.y : sceneData.MiniMap.Botright.z;
            float z2 = is2DMode ? sceneData.MiniMap.Topleft.y : sceneData.MiniMap.Topleft.z;

            HashSet<string> filterShow = new HashSet<string>();

            // DevLog($"mappable data.MapppableTypesInfo: {data.MappableTypesInfo.Count}");
            foreach (MappableTypeInfo typeInfo in data.MappableTypesInfo)
            {
                // DevLog($"mappable typeInfo: {typeInfo.Name} / {typeInfo.IsActive}");
                if (typeInfo.IsActive)
                {
                    filterShow.Add(typeInfo.Name);
                    // DevLog($"mappable typeInfo isactive");
                }
            }

            // DevLog($"mappable filterShow: {filterShow.Count}");
            // foreach (var item in filterShow)
            // {
            //     DevLog($"mappable filterShow: {item}");
            // }

            //pass one
            HashSet<IMappable> passOne = new();
            HashSet<Vector2> occupiedCoordinates = new();

            foreach (var mappable in mappables)
            {
                var view = mappable.ViewUI.GetView();
                if (view == null) continue;

                if (!filterShow.Contains(mappable.ToString()))
                {
                    view.style.display = DisplayStyle.None;
                    continue;
                }

                var mp = mappable.MapLocation?.worldPosition + mappable.Offset ?? new Vector3();

                float mpx = mp.x;
                float mpz = is2DMode ? mp.y : mp.z;


                var _mappable = view.Q<VisualElement>(className: "mappable");
                if (_mappable == null) continue;

                float layoutHeight = view.style.height.value.value;
                float layoutWidth = view.style.width.value.value;

                if (mpx >= x1 - layoutHeight && mpx <= x2 + layoutHeight
                        && mpz >= z1 - layoutWidth && mpz <= z2 + layoutWidth)
                {
                    float xx = (1 - ((x2 - mpx) / ww)) * screenWidth;
                    float zz = (1 - ((mpz - z1) / hh)) * screenHeight;
                    if (layoutHeight == 0) layoutHeight = mappable.ViewUI.maxHeight;
                    if (layoutWidth == 0) layoutWidth = mappable.ViewUI.maxWidth;

                    Vector2 jitteredCoordinate = new Vector2(xx, zz);
                    // int jitterAttempts = 0;
                    // while (occupiedCoordinates.Contains(jitteredCoordinate))
                    // {
                    //     float jitterAmount = 0.1f * jitterAttempts;
                    //     jitteredCoordinate += new Vector2(jitterAmount, jitterAmount);
                    //     jitterAttempts++;
                    // }
                    occupiedCoordinates.Add(jitteredCoordinate);

                    view.style.top = mappable.MapLocation.top = jitteredCoordinate.y - (layoutHeight / 2);
                    view.style.left = mappable.MapLocation.left = jitteredCoordinate.x - (layoutWidth / 2);
                    mappable.MapLocation.rect = new Rect(
                        mappable.MapLocation.left,
                        mappable.MapLocation.top,
                        layoutWidth,
                        layoutHeight);
                    // DevLog($"drawmappable mappabletype: {mappable} / rect: {mappable.location.rect} / location.top: {mappable.location.top} / location.left: {mappable.location.left} / layoutHeight: {layoutHeight} / layoutWidth: {layoutWidth} / xx: {xx} / zz: {zz} / mapCoordinate1: {mapCoordinate1} / mapCoordinate2: {mapCoordinate2} / x1: {x1} / x2: {x2} / z1: {z1} / z2: {z2} / ww: {ww} / hh: {hh} / screenWidth: {screenWidth} / screenHeight: {screenHeight} / is2DMode: {is2DMode}");
                    view.style.display = DisplayStyle.Flex;
                    passOne.Add(mappable);
                }
                else
                {
                    view.style.display = DisplayStyle.None;
                }
            }

            //pass two
            Dictionary<IMappable, List<IMappable>> overlapMap = new Dictionary<IMappable, List<IMappable>>();
            List<HashSet<IMappable>> clusters = new List<HashSet<IMappable>>();

            foreach (var mappable in passOne)
            {
                var currentView = mappable.ViewUI.GetView();
                if (currentView == null) continue;

                // DevLog($"{mappable} / {mappable.location.rect}");
                foreach (var otherMappable in passOne)
                {
                    if (mappable == otherMappable) continue;
                    if (SignificantOverlap(mappable.MapLocation.rect, otherMappable.MapLocation.rect, 0.5f))
                    {
                        // DevLog($"{mappable.pic.filename} overlaps {otherMappable.pic.filename}");
                        currentView.style.borderTopColor = currentView.style.borderBottomColor = currentView.style.borderLeftColor = currentView.style.borderRightColor = new Color(1, 0, 0, 1);
                        currentView.style.borderTopWidth = currentView.style.borderBottomWidth = currentView.style.borderLeftWidth = currentView.style.borderRightWidth = 2;

                        if (!overlapMap.ContainsKey(mappable))
                        {
                            overlapMap[mappable] = new List<IMappable>();
                        }
                        overlapMap[mappable].Add(otherMappable);
                        currentView.style.display = DisplayStyle.None;
                    }
                    else
                    {
                        currentView.style.borderTopColor = currentView.style.borderBottomColor = currentView.style.borderLeftColor = currentView.style.borderRightColor = new Color(0, 0, 0, 0);
                        currentView.style.borderTopWidth = currentView.style.borderBottomWidth = currentView.style.borderLeftWidth = currentView.style.borderRightWidth = 0;
                    }
                }
            }

            //pass three
            foreach (var cluster in clusterList) cluster.view.RemoveFromHierarchy();

            // Create or update clusters
            foreach (var keyValuePair in overlapMap)
            {
                var mappable = keyValuePair.Key;
                var overlappingMappables = keyValuePair.Value;

                HashSet<IMappable> foundCluster = null;
                List<HashSet<IMappable>> mergeClusters = new List<HashSet<IMappable>>();

                // Find clusters that should merge
                foreach (var cluster in clusters)
                {
                    if (cluster.Contains(mappable) || overlappingMappables.Any(x => cluster.Contains(x)))
                    {
                        mergeClusters.Add(cluster);
                    }
                }

                // Merge clusters or create a new one
                if (mergeClusters.Count > 0)
                {
                    foundCluster = new HashSet<IMappable>(mergeClusters.SelectMany(x => x));
                    // Remove the old clusters that we're merging
                    foreach (var oldCluster in mergeClusters)
                    {
                        clusters.Remove(oldCluster);
                    }
                }
                else
                {
                    foundCluster = new HashSet<IMappable>();
                }

                // Add the mappables to the cluster
                foundCluster.Add(mappable);
                foreach (var overlappingMappable in overlappingMappables)
                {
                    foundCluster.Add(overlappingMappable);
                }

                // Add the updated or new cluster back to the list
                clusters.Add(foundCluster);
            }

            // DevLog("--------------------");
            foreach (var cluster in clusters)
            {
                // DevLog($"{cluster.Count} / {cluster}");
                if (cluster.Count < 2) continue;  // Skip single-item "clusters"

                float minX = float.MaxValue, minY = float.MaxValue;
                float maxX = float.MinValue, maxY = float.MinValue;

                // Calculate the bounding box for the cluster
                foreach (var mappable in cluster)
                {
                    float left = mappable.MapLocation.left;
                    float top = mappable.MapLocation.top;
                    float right = left + mappable.MapLocation.rect.width;
                    float bottom = top + mappable.MapLocation.rect.height;

                    if (left < minX) minX = left;
                    if (top < minY) minY = top;
                    if (right > maxX) maxX = right;
                    if (bottom > maxY) maxY = bottom;
                }

                // Create a VisualElement to represent the cluster
                VisualElement clusterElement = new VisualElement();
                clusterElement.style.width = maxX - minX;  // Set the width to the cluster's width
                clusterElement.style.height = maxY - minY; // Set the height to the cluster's height
                clusterElement.style.backgroundColor = new Color(1, 1, 1, 0.1f); // Set to desired color

                clusterElement.style.borderTopLeftRadius = clusterElement.style.borderTopRightRadius = clusterElement.style.borderBottomLeftRadius = clusterElement.style.borderBottomRightRadius = 90;

                // Position the element
                clusterElement.style.left = minX;
                clusterElement.style.top = minY;
                clusterElement.style.position = Position.Absolute;

                string id = DateTime.Now.Ticks.ToString();
                clusterElement.name = id;

                // clusterElement.RegisterCallback<MouseOverEvent>(e =>
                // {
                //     DevLog($"MouseOverEvent: {id}");
                //     clusterElement.style.backgroundColor = new Color(1, 1, 1, 0.5f);
                //     clusterElement.BringToFront();
                // }, TrickleDown.NoTrickleDown);

                // Add it to the UI hierarchy
                clusterList.Add(new Cluster(clusterElement, sceneData.MiniMap.OrthSize));
                mapIconLayer.Add(clusterElement);

                foreach (var mappable in cluster)
                {
                    VisualElement mark = new();
                    float w = mappable.MapLocation.rect.width / 3;
                    float h = mappable.MapLocation.rect.height / 3;
                    mark.style.left = mappable.MapLocation.left + (mappable.MapLocation.rect.width / 2) - (w / 2);
                    mark.style.top = mappable.MapLocation.top + (mappable.MapLocation.rect.height / 2) - (h / 2);
                    mark.style.width = w;
                    mark.style.height = h;
                    mark.style.backgroundColor = new Color(1, 0, 0, 0.5f);
                    mark.style.backgroundImage = mappable.Pic.img;
                    mark.style.borderTopLeftRadius = mark.style.borderTopRightRadius = mark.style.borderBottomLeftRadius = mark.style.borderBottomRightRadius = 45;
                    mark.style.borderBottomColor = mark.style.borderTopColor = mark.style.borderLeftColor = mark.style.borderRightColor = StringToColor(mappable.ToString());
                    mark.style.borderLeftWidth = mark.style.borderRightWidth = mark.style.borderTopWidth = mark.style.borderBottomWidth = 5;

                    mark.style.position = Position.Absolute;

                    mark.RegisterCallback<MouseDownEvent>(e =>
                    {
                        DevLog("Mark Clicked");
                        zoomCount = 0;
                        StartZoomUntilFreedFromCluster(mappable);
                    }, TrickleDown.NoTrickleDown);

                    mapIconLayer.Add(mark);
                    clusterList.Add(new Cluster(mark, sceneData.MiniMap.OrthSize));
                }
            }

            updatingMappables = false;
        }

        static private IEnumerator ZoomUntilFree(IMappable targetMappable, float zoomFactor)
        {
            int i = 0;

            sceneData.MiniMap.Position = targetMappable.MapLocation.worldPosition;
            int ii = 0;
            bool t = true;
            while (t)
            {
                if (zoomCount++ > 30) t = false;
                i++;

                UpdateMappables();
                // UpdateView();

                // Check if the target is still in a cluster
                if (!IsMappableInAnyCluster(targetMappable))
                {
                    if (ii == 0) ii = i + 10;
                    if (i == ii)
                        break;
                }

                float newOrthoSize = sceneData.MiniMap.OrthSize * zoomFactor;
                sceneData.MiniMap.OrthSize = newOrthoSize;
                sceneData.MiniMap.Position = targetMappable.MapLocation.worldPosition;

                yield return null;
            }
            mapIconLayer.style.display = DisplayStyle.Flex;
        }

        static private bool IsMappableInAnyCluster(IMappable targetMappable)
        {
            var x = mapIconLayer.Q<VisualElement>(name: targetMappable.Pic.filename);
            if (x == null)
            {
                DevLog($"IsMappableInAnyCluster: {targetMappable.Pic.filename} is null");
                return false;
            }

            if (x.style.display == DisplayStyle.None)
            {
                return true;
            }

            return false;
        }

        static int zoomCount = 0;
        static private void StartZoomUntilFreedFromCluster(IMappable targetMappable)
        {
            float zoomFactor = 0.95f;
            EditorCoroutineUtility.StartCoroutineOwnerless(ZoomUntilFree(targetMappable, zoomFactor));
        }

        private static void GeometryChangedCallback(GeometryChangedEvent evt)
        {
            window.rootVisualElement.UnregisterCallback<GeometryChangedEvent>(GeometryChangedCallback);
            DevLog("GeometryChangedCallback");
        }

        public static void RebuildView()
        {
            if (rebuildingView == true) return;
            DevLog("Rebuilding view");
            rebuildingView = true;


            CreateGUI();

            if (!sceneLoaded) DevLogWarning("Scene not loaded");
            if (sceneData == null) MightyCore.GetSceneData();
            if (sceneData == null) return;
            //CreateGUI();
            UpdateMiniMap();

            if (isSceneAnchored)
            {
                BuildMappables();
                UpdateMappables();
                UpdateMarkers();
            }
            rebuildingView = false;
        }

        public static void UpdateView()
        {
            //DevLog("Rebuilding view");
            //CreateGUI();
            //BuildMappables();
            UpdateMiniMap();
            if (isSceneAnchored)
            {
                UpdateMappables();
                UpdateMarkers();
            }

            dirty = false;
        }

        private static void DrawClusters()
        {

        }

        private static void CalculateClusters()
        {

        }

        private static void CalculateMappables()
        {

        }
        #endregion
    }
}