using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static Mighty.MightyCoreData;


namespace Mighty
{
    [InitializeOnLoad]
    [ExecuteInEditMode]
    [DefaultExecutionOrder(-1000)]
    public static class MightyCore
    {
        public static MightyCoreData data;

        static public bool isInit = false, hasFirstFramePassed = false;

        static public GameObject cameraTopDown;

        // static public Camera cameraScene;
        static public Material matFade;


        // delegate void RefreshAll();
        static event Action RefreshAll;
        // public static Action Rebuild;

        // static public List<IMappable> mapShapes;

        static MightyCore()
        {
            EditorSceneManager.sceneOpened -= SceneOpened;
            EditorSceneManager.sceneSaved += SceneSaved;

            EditorSceneManager.newSceneCreated -= NewSceneCreated;
            EditorSceneManager.newSceneCreated += NewSceneCreated;

            EditorSceneManager.sceneOpened -= SceneOpened;
            EditorSceneManager.sceneOpened += SceneOpened;

            EditorApplication.delayCall -= InitAfterFirstFrame;
            EditorApplication.delayCall += InitAfterFirstFrame;

            EditorSceneManager.sceneClosing -= SceneClosing;
            EditorSceneManager.sceneClosing += SceneClosing;

            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;

            EditorApplication.quitting -= EditorQuitting;
            EditorApplication.quitting += EditorQuitting;

            EditorApplication.playModeStateChanged -= ModeChanged;
            EditorApplication.playModeStateChanged += ModeChanged;

            InitPlaythroughs -= InitRunIds;
            InitPlaythroughs += InitRunIds;
        }

        public static void InitAfterFirstFrame()
        {
            if (hasFirstFramePassed) return;
            DevLog($"InitAfterFirstFrame ----------------------------------------");
            isInit = false;
            isSceneAnchored = false;
            hasFirstFramePassed = true;

            if (!SceneManager.GetActiveScene().isLoaded)
            {
                DevLog($"Scene not loaded");
                return;
            }
            sceneLoaded = true;
            DevLog($"Core");
            data = Load();
            cameraTopDown = Resources.Load("MightyOrthoCam") as GameObject;

            sceneAnchor = null;
            GetSceneData();

            RefreshAll -= Refresh;
            RefreshAll += Refresh;
            Rebuild = MightyMap.RebuildView;
            UpdateMappables -= MightyMap.UpdateMappables;
            UpdateMappables += MightyMap.UpdateMappables;
            RebuildMappables -= MightyMap.BuildMappables;
            RebuildMappables += MightyMap.BuildMappables;
            UpdateMarkers -= _UpdateMarkers;
            UpdateMarkers += _UpdateMarkers;

            BuildTopUI -= TopUI;
            BuildTopUI += TopUI;

            Init();
            StartWindow?.Invoke();

            // if (isSceneAnchored)
            // {
            //     StartModules();
            //     modulesStarted = true;
            //     Rebuild?.Invoke();
            // }

            if (isSceneAnchored)
            {
                // Debug.Log("Scene anchored, starting Modules");
                StartModules?.Invoke();
                modulesStarted = true;
                Rebuild?.Invoke();
            }
            else
            {
                // Debug.Log("Scene not anchored, not starting Modules");
            }

        }

        public static void InitRunIds()
        {
            DevLog("InitRunIds");
            sceneData.RunIDList = new List<string>();

            for (int i = 0; i < data.scenes.Count; i++)
            {
            }
            RebuildRunIdDropDown?.Invoke();
        }

        private static void SceneClosing(UnityEngine.SceneManagement.Scene scene, bool removingScene)
        {
            sceneLoaded = false;
            sceneAnchor = null;
            sceneData = null;
            isInit = false;
            hasFirstFramePassed = false;

            DevLog($"SceneClosing({scene.name}, {removingScene})");
            SaveData();


        }

        static void ModeChanged(PlayModeStateChange state)
        {
            AssetDatabase.SaveAssets();

            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                if (sceneData != null)
                {
                    sceneData.RunID = DateTime.Now.Ticks;
                }
            }

            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                if (sceneData != null)
                {
                    sceneData.RunID = 0;
                }
            }
            //DevLog($"ModeChanged: {state.ToString()}  run_id: {sceneData.run_id}");

            sceneCamera = null;
            isInit = false;
            Init();
        }


        private static void EditorUpdate()
        {
            if (!hasFirstFramePassed || (hasFirstFramePassed & data == null))
            {
                // DevLog("Has not passed first frame");
                InitAfterFirstFrame();
                hasFirstFramePassed = true;
                return;
            }

            if (sceneCamera == null)
                sceneCamera = Application.isPlaying ? Camera.main : GetSceneView().camera;


            data.svPos = sceneCamera.transform.position;
            data.svRot = sceneCamera.transform.rotation;

            if (data.svPos != data._svPos ||
                data.svRot != data._svRot)
                RefreshAll?.Invoke();
            // if (svPos != _svPos) DevLog("Moved");
            // DevLog($"{svPos} != {_svPos}");
            //
            data._svPos = data.svPos;
            data._svRot = data.svRot;

            // DevLog($"isSceneAnchored {isSceneAnchored}  isInit {isInit}  modulesStarted {modulesStarted}  sceneLoaded {sceneLoaded}  hasFirstFramePassed {hasFirstFramePassed}  MightyMap.GUILoaded {MightyMap.GUILoaded}");

            if (sceneData != null)
                if (sceneData.DeleteMe)
                {
                    var go = GameObject.Find("MightySceneAnchor");
                    if (go != null)
                    {
                        GameObject.DestroyImmediate(go);
                        foreach (var typeInfo in data.MappableTypesInfo)
                        {
                            if (typeInfo.Mappable == null)
                            {
                                DevLogError($"TypeInfo Mappable is null for {typeInfo.Name}");
                                continue; // Skip this iteration as Mappable is null
                            }
                            Debug.Log($"Deleting {typeInfo.Name}");
                            typeInfo.Mappable.Delete();
                            isSceneAnchored = false;
                            sceneAnchor = null;
                            MightyMap.isInit = false;
                            hasFirstFramePassed = false;
                            InitAfterFirstFrame();
                            MightyMap.RebuildView();
                            //UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
                        }
                    }
                    sceneData = null;
                    data.scenes.RemoveAt(data.sceneIndex);
                }
            if (addSceneAnchor != null && mid != null && bot != null)
                if (isSceneAnchored != MightyMap.GUILoaded)
                {
                    addSceneAnchor.style.display = DisplayStyle.Flex;
                    mid.style.display = DisplayStyle.None;
                    bot.style.display = DisplayStyle.None;
                }
                else
                {
                    addSceneAnchor.style.display = DisplayStyle.None;
                    mid.style.display = DisplayStyle.Flex;
                    bot.style.display = DisplayStyle.Flex;
                }
        }

        // DevLog($"EditorUpdate: scenePath {sceneData.scenePath}  sceneName {sceneData.Name}  sceneAnchor {sceneAnchor.DataSetName}  isSceneAnchored {isSceneAnchored}  isInit {isInit}  modulesStarted {modulesStarted}  sceneLoaded {sceneLoaded}  hasFirstFramePassed {hasFirstFramePassed}");

        private static void GetSceneData(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
        {
            GetSceneData();
        }

        public static void GetSceneData()
        {
            DevLog($"SceneManager.GetActiveScene().isLoaded = {SceneManager.GetActiveScene().isLoaded}");
            if (sceneAnchor == null)
            {
                DevLog($"sceneData is null? {sceneData == null}");
                var go = GameObject.Find("MightySceneAnchor");
                if (go != null)
                {
                    sceneAnchor = go.GetComponent<MightySceneAnchor>();
                }
                if (sceneAnchor == null)
                {
                    DevLog("sceneAnchor is null");
                    // if (EditorUtility.DisplayDialog("Create Scene Anchor", "Mighty Dev Ops needs to anchor to this scene.  This creates a small Editor Only reference object within your scene.  Would you like to anchor now?", "Yes", "No"))
                    // {
                    //     go = new GameObject("MightySceneAnchor")
                    //     {
                    //         tag = "EditorOnly"
                    //     };
                    //     EditorUtility.SetDirty(go);
                    //     go.AddComponent<MightySceneAnchor>();
                    //     sceneAnchor = go.GetComponent<MightySceneAnchor>();
                    //     sceneAnchor.DataSetName = $"{SceneManager.GetActiveScene().name}___{SceneManager.GetActiveScene().path.Replace("/", "_").Replace(".unity", "")}";
                    //     data.scenes.Add(new SceneData());
                    //     var sceneIndex = MightyCore.data.scenes.Count - 1;
                    //     data.scenes[sceneIndex].Name = sceneAnchor.DataSetName;
                    //     //UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
                    //     isSceneAnchored = true;
                    //     EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                    // }
                    // else
                    // {
                    //     DevLog("User chose No");
                    // }
                }
                else
                {
                    isSceneAnchored = true;
                }
                if (isSceneAnchored) DevLog($"sceneAnchor is {sceneAnchor.DataSetName}");
            }
            DevLog($"sceneData is null? {sceneData == null}");

            if (!isSceneAnchored) return;

            data.CheckSceneData();
            data.sceneIndex = -1;
            data.projectIndex = -1;
            DevLog($"data.scenes.Count {data.scenes.Count}");

            data.SceneDupeCheck();

            for (int i = 0; i < data.scenes.Count; i++)
            {
                DevLog($"Scene {i} is {data.scenes[i].Name}");
                DevLog($"Scene {i} is {sceneAnchor}");
                DevLog($"Scene {i} is {sceneAnchor.DataSetName}");

                if (data.scenes[i].Name == sceneAnchor.DataSetName) data.sceneIndex = i;
                if (data.scenes[i].Name == "Project") data.projectIndex = i;
            }

            if (data.projectIndex < 0)
            {
                DevLog("ProjectIndex not found");
                data.scenes.Add(new SceneData());
                data.projectIndex = data.scenes.Count - 1;
                data.scenes[data.projectIndex].Name = "Project";
                data.scenes[data.projectIndex].MiniMap = new MiniMapData();
            }

            if (data.scenes[data.sceneIndex].ScenePath == null || data.scenes[data.sceneIndex].ScenePath == "")
            {
                data.scenes[data.sceneIndex].ScenePath = SceneManager.GetActiveScene().path;
                DevLog($"scenePath is {data.scenes[data.sceneIndex].ScenePath}");
            }

            if (data.sceneIndex < 0)
            {
                DevLog("SceneIndex not found");
                if (data.projectIndex >= 0)
                {
                    DevLog("ProjectIndex found");
                    data.sceneIndex = data.projectIndex;
                }

                if (sceneAnchor != null)
                {
                    data.scenes.Add(new SceneData());
                    data.sceneIndex = data.scenes.Count - 1;
                }
                data.scenes[data.sceneIndex].Name = sceneAnchor.DataSetName;
                data.scenes[data.sceneIndex].MiniMap = new MiniMapData();

            }
            DevLog($"SceneIndex is {data.sceneIndex}");
            isSceneAnchored = true;
            if (data.sceneIndex == data.projectIndex)
            {
                DevLog("ProjectIndex is SceneIndex");
                isSceneAnchored = false;
            }
            sceneData = data.scenes[data.sceneIndex];
            sceneData.currentIndex = 0;
            ////////
        }


        static void TopUI()
        {
        }
        public static void Init()
        {
            if (isInit == true) return;

            DevLog("Init Core");
            GetPath();
            if (mappables == null) mappables = new List<IMappable>();

            //InitMappables();


            icons = new Icons();

            // DevLog($"DataSet {sceneData.DataSetName}");
            cameraTopDown = Resources.Load("MightyOrthoCam") as GameObject;
            matFade = Resources.Load("MightyFade", typeof(Material)) as Material;

            cameraTopDown.SetActive(true);
            cameraTopDown.SetActive(false);
            isInit = true;

            //StartModules();

            if (data == null) data = Load();


            data.MappableTypesInfo ??= new();
            var mapShapeTypes = TypeCache.GetTypesDerivedFrom(typeof(IMappable));
            DevLog($"Found {mapShapeTypes.Count} Adding new mappable type");
            DevLog($"Found {mappables.Count} mappables");
            foreach (var shapeType in mapShapeTypes)
            {
                // Create an instance to get the human-readable name
                IMappable mappable = (IMappable)Activator.CreateInstance(shapeType);
                // Check if this type info already exists in MappableTypesInfo
                MappableTypeInfo existingInfo = null;
                foreach (var info in data.MappableTypesInfo)
                {
                    DevLog($"Found existing mappable type {info.TypeName} ");
                    if (info.TypeName == shapeType.FullName)
                    {
                        DevLog($"Found existing mappable type {shapeType.FullName} ");
                        existingInfo = info;
                        break;
                    }
                }


                if (existingInfo == null)
                {
                    // If not, add a new entry
                    data.MappableTypesInfo.Add(new MappableTypeInfo(shapeType.FullName, mappable.ToString(), true, mappable));
                }
                else
                {//
                    // If so, update the instance
                    existingInfo.Mappable = mappable;
                }
            }
            isInit = true;

        }



        public static GameObject GetOrthoCam()
        {
            GameObject r = cameraTopDown;
            if (cameraTopDown == null) r = GameObject.Find("MightyOrthoCam");
            if (r == null) DevLog("Could not find MightyOrthoCam");
            return r;
        }

        //
        static void Refresh()
        {
            if (EditorWindow.HasOpenInstances<MightyMap>())
                MightyMap.GetWindow().Repaint();
        }


        private static void EditorQuitting()
        {
            SaveData();
        }

        private static void SaveData()
        {
            // DevLog("SaveData");
            if (data == null) return;
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
        }

        private static void SceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
        {
            DevLog($"SceneOpened({scene.name}, {mode})");
            // isInit = false;
            // isSceneAnchored = false;
            hasFirstFramePassed = false;
            InitAfterFirstFrame();


            // GetSceneData();
            // Init();
            // window.rootVisualElement.Q<VisualElement>("MapIconLayer").Clear();
            // if (isSceneAnchored)
            //     StartModules();
            // Rebuild?.Invoke();

            // sceneLoaded = true;
            // DevLog($"Core");
            // data = Load();
            // cameraTopDown = Resources.Load("MightyOrthoCam") as GameObject;

            // sceneAnchor = null;
            // GetSceneData();

            // RefreshAll -= Refresh;
            // RefreshAll += Refresh;
            // Rebuild = MightyMap.RebuildView;


            // BuildTopUI -= TopUI;
            // BuildTopUI += TopUI;

            // //DevLog($"UpdateMarkers: {UpdateMarkers.GetInvocationList().Length}");
            // Init();
            // StartWindow();


            // EditorApplication.update += EditorUpdate;

            // EditorApplication.quitting += EditorQuitting;
            // EditorApplication.playModeStateChanged += ModeChanged;

            // if (isSceneAnchored)
            // {
            //     Debug.Log("Scene anchored, starting Modules");
            //     StartModules();
            //     Rebuild?.Invoke();
            // }
            // else
            // {
            //     Debug.Log("Scene not anchored, not starting Modules");
            // }
        }

        private static void _UpdateMarkers()
        {
            // Debug.Log($"UpdateMarkers:");

            // if (UpdateMarkers == null)
            // {
            //     Debug.Log("UpdateMarkers Delegate is null");
            //     return;
            // }
            // int i = 0;
            // foreach (Delegate singleCast in UpdateMarkers.GetInvocationList())
            // {
            //     Debug.Log($"   {i++}: UpdateMarkers Method: {singleCast.Method.DeclaringType}/{singleCast.Method}, Target: {singleCast.Target}");
            // }
        }

        private static void NewSceneCreated(UnityEngine.SceneManagement.Scene scene, NewSceneSetup setup, NewSceneMode mode)
        {

        }

        private static void SceneSaved(UnityEngine.SceneManagement.Scene scene)
        {
            SaveData();
        }

    }
}