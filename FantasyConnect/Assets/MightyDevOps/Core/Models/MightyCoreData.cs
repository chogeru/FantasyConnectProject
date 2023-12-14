using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static Mighty.MightyWindowManagerStateful;


namespace Mighty
{
    [DefaultExecutionOrder(-1000)]
    public class MightyCoreData : ScriptableObject
    {
        private static bool DevLogs = false;
        //private static long DevTicks = 0;


        public static void DevLog(object message, long ticks = 10)
        {
            //if (ticks != 0 || (DateTime.Now.Ticks > ticks && DevTicks > ticks))
            if (DevLogs)
            {
                if (message is IConvertible convertible)
                {
                    Debug.Log(convertible.ToString());
                }
                else
                {
                    Debug.Log(message);
                }
            }
        }

        public static void DevLogWarning(object message, long ticks = 0)
        {
            // if (ticks == 0 || DateTime.Now.Ticks > ticks)

            if (DevLogs) Debug.LogWarning(message);
        }

        public static void DevLogError(object message, long ticks = 0)
        {
            // if (ticks == 0 || DateTime.Now.Ticks > ticks)

            if (DevLogs) Debug.LogError(message);
        }

        [SerializeField]
        static public void Save()
        {
            string path = $"{corePath}/Core/Data/MightyCoreData.asset";
            if (File.Exists(path))
            {
                DevLog($"{path} already exists...");
                return;
            }

            DevLog($"{path} does not exist, creating...");
            MightyCoreData asset = ScriptableObject.CreateInstance<MightyCoreData>();

            asset.scenes = new List<SceneData>
            {
                new SceneData { Name = "Project" }
            };

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
        }

        static public MightyCoreData Load()
        {
            string path = $"{corePath}/Core/Data/MightyCoreData.asset";
            if (!File.Exists(path))
            {
                Save();
            }
            DevLog($"Loading Core Data at {path}");

            windowManagerStateful = AssetDatabase.LoadAssetAtPath<MightyWindowManagerStateful>($"{corePath}/Core/Data/MightyWindowManagerStateful.asset");

            return AssetDatabase.LoadAssetAtPath<MightyCoreData>(path);
        }




        static public Action Rebuild, RebuildMappables, UpdateMappables, UpdateMarkers, ClearMarkers, InitPlaythroughs,
        RebuildRunIdDropDown, StartModules, RefreshSceneView, StartWindow, BuildTopUI, BuildWindowBar, TaskListPopulate, OpenModuleSubMenu, CloseModuleSubMenu;
        [SerializeField]
        static public List<IMappable> mappables = new List<IMappable>();
        [field: SerializeField]
        public List<MappableTypeInfo> MappableTypesInfo;
        static public CustomToggleButton selectedModule;
        static public bool moduleSubMenuActive = false;
        /// <summary>
        /// 
        /// </summary>/
        static public float transitionSpeed = 500;

        static public MightySceneAnchor sceneAnchor;
        static public Camera sceneCamera;
        static public Icons icons;
        public static EditorWindow window;
        public static MightyWindowManagerStateful windowManagerStateful;
        static public SceneData sceneData;
        static public bool isSceneAnchored = false, sceneLoaded = false, modulesStarted = false;
        [SerializeField]
        public int sceneIndex = -1, projectIndex = -1;
        [SerializeField]
        public static bool followSceneView;

        static public float screenWidth, screenHeight, screenPrev,
        targetRatio, hh, ww, xOffset, x1, x2, z1, z2;

        static public VisualElement root, ux, map, mapIconLayer, mapMarkerLayer, top, mid, bot, addSceneAnchor, sideMenu, sceneCamIcon, windowBar;

        public enum SearchType
        {
            Name,
            Deep
        }

        public static SearchType currentSearchType = SearchType.Name;
        public static bool isCaseSensitive = false;
        public static string searchQuery = "";

        public class ProjectDossier
        {
            public static string name;
            public static string genre;
            public static string description;
            public static string plot;
            public static string platforms;

            public class Scenes
            {
                public string name;
                public string description;

                override public string ToString() => $"{name} - {description}";
            }
            public static List<Scenes> scenes;

            static ProjectDossier()
            {
                name = "Space Blasters 3000";
                genre = "3D 3rd Person Shooter";
                description = "Lighthearted 3rd person shooter with a retro feel, platformer elements, puzzle solving, and a humorous story.";
                plot = "Becky is a space cadet who must save the universe from the evil space aliens.  Her spaceship crashed so she must salvage and explore so that she can repair her ship and get back to Earth.  However, the evil space aliens have taken over the planet and are trying to stop her.  She must fight her way through the aliens and their minions to get to the mothership and destroy it.  Once the mothership is destroyed, she can repair her ship and get back to Earth.";
                platforms = "PC, Android, iOS";
                scenes = new List<Scenes>
                {
                    new Scenes { name = "Space Station", description = "Becky's ship has crashed on an abandoned space station.  She must explore the station to find the parts she needs to repair her ship." },
                    new Scenes { name = "Slime Caves", description = "Becky has found the parts she needs to repair her ship, but the evil space aliens have taken over the planet and are trying to stop her.  She must fight her way through the aliens and their minions to get to the mothership and destroy it." },
                    new Scenes { name = "Mothership", description = "Becky has destroyed the mothership and can now repair her ship and get back to Earth." },
                    new Scenes { name = "Earth", description = "Becky has returned to Earth and is hailed as a hero." }
                };
            }


        }
        //
        // public static ProjectDossier projectDossier;

        //public long run_id = 0;
        //public string run_selected = "";
        //public HashSet<string> run_ids = new HashSet<string>();
        //public long run_playbackCursor = 0, run_playbackCount = 0, run_playbackMax = 0, run_playbackMin = 0;
        //public Vector2 run_playbackRange = new Vector2(0, 0);
        //
        public void CheckSceneData()
        {
            DevLog("CheckSceneData");

            if (sceneAnchor == null)
            {

                GameObject go = GameObject.Find("MightySceneAnchor");
                if (go != null)
                {
                    DevLog($"sceneData: {sceneAnchor}");
                    sceneAnchor = go.GetComponent<MightySceneAnchor>();
                    DevLog($"sceneData: {sceneAnchor}");
                    DevLog($"sceneData: {sceneAnchor.DataSetName}");
                    isSceneAnchored = true;
                }
                else
                {
                    isSceneAnchored = false;
                }
            }

            bool sceneFound = false;
            if (isSceneAnchored)
                foreach (var scene in scenes)
                {
                    DevLog($"Scene: {scene.Name}");

                    if (scene.Name == sceneAnchor.DataSetName)
                    {
                        sceneData = scene;
                        sceneFound = true;
                        DevLog($"Scene Data: {sceneData.Name}");
                    }
                }
            if (!sceneFound && isSceneAnchored)
            {
                DevLog($"Scene not found...");
                scenes.Add(new SceneData { Name = sceneAnchor.DataSetName });
            }
        }

        [SerializeField]
        public List<SceneData> scenes;

        public int GetSceneIndex(String sceneName)
        {
            for (int i = 0; i < scenes.Count; i++)
            {
                if (scenes[i].Name == sceneName)
                {
                    return i;
                }
            }
            return -1;
        }

        public void SceneDupeCheck()
        {
            List<string> sceneNames = new List<string>();
            for (int i = 0; i < scenes.Count; i++)
            {
                if (sceneNames.Contains(scenes[i].Name))
                {
                    DevLog($"Scene {i} is a duplicate");
                    scenes.RemoveAt(i);
                    i--;
                    continue;
                }
                sceneNames.Add(scenes[i].Name);
            }

        }

        //
        [Serializable]
        public class SceneData
        {
            [SerializeField] private string name;
            [SerializeField] private MiniMapData miniMap;
            [SerializeField] private string scenePath;
            [SerializeField] private long runID;
            [SerializeField] private string selectedRun;
            [SerializeField] private List<string> runIDList;
            [SerializeField] private long runPlaybackCursor;
            [SerializeField] private long runPlaybackCount;
            [SerializeField] private long runPlaybackMax;
            [SerializeField] private long runPlaybackMin;
            [SerializeField] private Vector2 runPlaybackRange;
            [SerializeField] private bool deleteMe = false;

            // Properties with backing fields
            public string Name
            {
                get => name;
                set
                {
                    if (name != value)
                    {
                        DevLog($"Scene name changed from {name} to {value}");
                        name = value;
                    }
                }
            }
            public MiniMapData MiniMap { get => miniMap; set => miniMap = value; }
            public string ScenePath { get => scenePath; set => scenePath = value; }
            public long RunID { get => runID; set => runID = value; }
            public string SelectedRun { get => selectedRun; set => selectedRun = value; }
            public List<string> RunIDList { get => runIDList; set => runIDList = value; }
            public long RunPlaybackCursor { get => runPlaybackCursor; set => runPlaybackCursor = value; }
            public long RunPlaybackCount { get => runPlaybackCount; set => runPlaybackCount = value; }
            public long RunPlaybackMax { get => runPlaybackMax; set => runPlaybackMax = value; }
            public long RunPlaybackMin { get => runPlaybackMin; set => runPlaybackMin = value; }
            public Vector2 RunPlaybackRange { get => runPlaybackRange; set => runPlaybackRange = value; }
            public bool DeleteMe { get => deleteMe; set => deleteMe = value; }

            private VisualElement indexContainer;
            //private ProgressBar progressBar;
            //public List<Trackable.Root> trackables;
            public SceneData()
            {
                DevLog("SceneData constructor");
                Name = "Default";
                MiniMap = new MiniMapData();
                RunIDList = new List<string>();


            }

            public VisualElement GetProgressBar()
            {
                indexContainer = new()
                {
                    name = "indexContainer",
                    pickingMode = PickingMode.Ignore,
                    style =
            {
                flexDirection = FlexDirection.Row,
                justifyContent = Justify.Center,
                alignItems = Align.Center,
                position = Position.Absolute,
                flexGrow = 1,
                flexShrink = 0,
                width = Length.Percent(100),
                height = Length.Percent(100),
            }
                };

                //     progressBar = new()
                //     {
                //         name = "progressBar",
                //         title = "",

                //         pickingMode = PickingMode.Ignore,
                //         style =
                // {

                //     width = 200,
                //     height = 64,

                // }
                //     };
                //indexContainer.Add(progressBar);
                indexContainer.style.display = DisplayStyle.None;

                return indexContainer;
            }

            [Serializable]
            public class PropertyData
            {
                [SerializeField]
                public string Key;
                [SerializeField]
                public string Type; // Renamed from Value to Type
                [SerializeField]
                public string Value; // New field for actual value
            }


            [Serializable]
            public class ComponentData
            {
                [SerializeField]
                public string TypeName;
                //public GameObject ParentGameObject; // New field
                [SerializeField]
                public List<PropertyData> Properties;
            }

            [Serializable]
            public class GameObjectData
            {
                [SerializeField]
                public string Name;
                [SerializeField]
                public string Tag;
                [SerializeField]
                public string Layer;
                [SerializeField]
                public bool IsPrefab;
                [SerializeField]
                public bool IsStatic;
                [SerializeField]
                public bool IsActive;
                [SerializeField]
                public List<ComponentData> Components = new List<ComponentData>();
            }


            [HideInInspector]
            [SerializeField]
            public List<GameObjectData> CollectedData;

            public int currentIndex = 0;
            private GameObject[] allObjects;
            public bool isCollecting = false;
            [SerializeField]
            public int totalPolyCount;  // New variable for total polygon count
            [SerializeField]
            public int meshFilterCount; // New variable for counting MeshFilter components


            public void UpdateDeepDive()
            {
                if (isCollecting)
                {
                    ProcessBatch();
                }
            }

            public void StartCollection()
            {
                DevLog("Starting data collection");
                allObjects = GameObject.FindObjectsOfType<GameObject>();
                DevLog($"Found {allObjects.Length} objects");
                currentIndex = 0;
                CollectedData = new List<GameObjectData>();
                isCollecting = true;
                //indexContainer.style.display = DisplayStyle.Flex;
            }

            private void ProcessBatch()
            {
                int processedObjects = 0;
                DevLog($"Processing batch {currentIndex} of {allObjects.Length} objects");

                // DevLog($"Processing batch {currentIndex} of {allObjects.Length} objects");
                // progressBar.lowValue = 0;
                // progressBar.highValue = allObjects.Length;
                // progressBar.value = (float)currentIndex;

                while (currentIndex < allObjects.Length && processedObjects < 5000)
                {
                    GameObject currentGO = allObjects[currentIndex];
                    //DevLog($"Processing {currentGO.name} {currentIndex} of {allObjects.Length} objects");

                    // Skip hidden or inactive GameObjects
                    if (currentGO == null || ShouldSkipGameObject(currentGO))
                    {
                        currentIndex++;
                        continue;
                    }

                    GameObjectData goData = new GameObjectData
                    {
                        Name = currentGO.name,
                        Tag = currentGO.tag,
                        Layer = LayerMask.LayerToName(currentGO.layer),
                        IsPrefab = PrefabUtility.IsPartOfAnyPrefab(currentGO),
                        IsStatic = currentGO.isStatic,
                        IsActive = currentGO.activeSelf,
                        Components = new List<ComponentData>()
                    };
                    DevLog($"Processing goData {goData.Name} {currentIndex} of {allObjects.Length} objects");

                    Component[] components = allObjects[currentIndex].GetComponents<Component>();

                    foreach (Component component in components)
                    {
                        if (component == null) continue;
                        if (component is MeshFilter meshFilter)
                        {
                            if (meshFilter.sharedMesh != null)
                            {
                                int polyCount = meshFilter.sharedMesh.triangles.Length / 3;
                                totalPolyCount += polyCount; // Update the total polygon count
                                meshFilterCount++;           // Increment the MeshFilter component count
                            }
                        }

                        ComponentData componentData = new()
                        {
                            TypeName = component.GetType().Name,
                            Properties = new List<PropertyData>(),
                        };

                        SerializedObject so = new SerializedObject(component);
                        SerializedProperty sp = so.GetIterator();

                        // Step to the first field
                        sp.Next(true);

                        while (sp.NextVisible(false))
                        {
                            // You could use the serialized property's type here to determine how to output it
                            string propertyKey = sp.name;
                            string propertyType = sp.propertyType.ToString();
                            string propertyValue = AsStringValue(sp); // We'll define this method to convert SerializedProperty to string

                            componentData.Properties.Add(new PropertyData
                            {
                                Key = propertyKey,
                                Type = propertyType,
                                Value = propertyValue // Setting actual value
                            });
                        }

                        goData.Components.Add(componentData);
                    }


                    CollectedData.Add(goData);
                    currentIndex++;
                    processedObjects++;
                }

                if (currentIndex >= allObjects.Length)
                {
                    isCollecting = false;
                    DevLog("Data collection completed.");
                    DevLog($"Total Poly Count: {totalPolyCount}, Average Poly Count: {(float)totalPolyCount / meshFilterCount}");
                    //indexContainer.style.display = DisplayStyle.None;
                    //progressBar.value = 0;
                    // AuditData();
                }
            }

            private bool ShouldSkipGameObject(GameObject go)
            {
                // Example condition for skipping a GameObject
                if (!go.activeInHierarchy)
                {
                    return true;
                }

                // Add more conditions as needed, for example:
                // if (go.layer == LayerMask.NameToLayer("HiddenLayer"))
                // {
                //     return true;
                // }

                // if (go.tag == "HiddenTag")
                // {
                //     return true;
                // }

                return false;
            }

            public void AuditData()
            {
                if (CollectedData.Count == 0)
                {
                    DevLog("No data to audit. Please collect scene data first.");
                    return;
                }

                foreach (GameObjectData goData in CollectedData)
                {
                    DevLog($"GameObject: {goData.Name}");
                    foreach (ComponentData componentData in goData.Components)
                    {
                        DevLog($"--Component: {componentData.TypeName}");
                        foreach (PropertyData propertyData in componentData.Properties)
                        {
                            DevLog($"----Property: {propertyData.Key}, Value: {propertyData.Value}");
                        }
                    }
                }

                if (meshFilterCount > 0) // Avoid division by zero
                {
                    float avgPolyCount = (float)totalPolyCount / meshFilterCount;
                    DevLog($"Total Poly Count: {totalPolyCount}, Average Poly Count: {avgPolyCount}");
                }

                PerformSearch("Camera");
            }


            public List<GameObjectData> SearchCollectedData(string query, SearchType searchType, bool isCaseSensitive)
            {
                List<GameObjectData> searchResults = new List<GameObjectData>();
                if (CollectedData == null) return searchResults;

                foreach (GameObjectData goData in CollectedData)
                {
                    bool matchFound = false;

                    // Adjust the query and name according to the case sensitivity
                    string adjustedQuery = isCaseSensitive ? query : query.ToLower();
                    string adjustedName = isCaseSensitive ? goData.Name : goData.Name.ToLower();

                    if (searchType == SearchType.Name)
                    {
                        matchFound = adjustedName.Contains(adjustedQuery);
                    }
                    else if (searchType == SearchType.Deep)
                    {
                        matchFound = adjustedName.Contains(adjustedQuery);

                        if (!matchFound)
                        {
                            foreach (ComponentData componentData in goData.Components)
                            {
                                foreach (PropertyData propertyData in componentData.Properties)
                                {
                                    // Adjust the key and value according to the case sensitivity
                                    string adjustedKey = isCaseSensitive ? propertyData.Key : propertyData.Key.ToLower();
                                    string adjustedValue = isCaseSensitive ? propertyData.Value : propertyData.Value.ToLower();

                                    if (adjustedKey.Contains(adjustedQuery) || adjustedValue.Contains(adjustedQuery))
                                    {
                                        DevLog($"Deep search match: {goData.Name} - {componentData.TypeName} - {propertyData.Key} - {propertyData.Value}");
                                        matchFound = true;
                                        break;
                                    }
                                }
                                if (matchFound) break;
                            }
                        }
                    }

                    if (matchFound)
                    {
                        searchResults.Add(goData); // Add the GameObject, not the component
                    }
                }

                return searchResults;
            }






            public void PerformSearch(string query)
            {
                DevLogWarning($"Performing search for \"{query}\"");
                List<GameObjectData> results = SearchCollectedData(query, currentSearchType, isCaseSensitive);

                if (currentSearchType == SearchType.Deep)
                    foreach (GameObjectData goData in results)
                    {
                        DevLog($"{query}/{goData.Name}: GameObject: {goData.Name}");
                        foreach (ComponentData componentData in goData.Components)
                        {
                            DevLog($"{query}/{goData.Name}: --Component: {componentData.TypeName}");
                            foreach (PropertyData propertyData in componentData.Properties)
                            {
                                DevLog($"{query}/{goData.Name}: ----Property: {propertyData.Key}, Value: {propertyData.Value}");
                            }
                        }
                    }

                // Display the results or handle them as you wish
                foreach (GameObjectData goData in results)
                {
                    DevLog($"Found GameObject: {goData.Name}");
                }
            }


            // public GameObjectData PerformSearch(string query)
            // {
            //     DevLogWarning($"Performing search for \"{query}\"");
            //     List<GameObjectData> results = SearchCollectedData(query, currentSearchType, isCaseSensitive);

            //     if (currentSearchType == SearchType.Deep)
            //         foreach (GameObjectData goData in results)
            //         {
            //             DevLog($"{query}/{goData.Name}: GameObject: {goData.Name}");
            //             foreach (ComponentData componentData in goData.Components)
            //             {
            //                 DevLog($"{query}/{goData.Name}: --Component: {componentData.TypeName}");
            //                 foreach (PropertyData propertyData in componentData.Properties)
            //                 {
            //                     DevLog($"{query}/{goData.Name}: ----Property: {propertyData.Key}, Value: {propertyData.Value}");
            //                 }
            //             }
            //         }

            //     GameObjectData data = new GameObjectData();
            //     // Display the results or handle them as you wish
            //     foreach (GameObjectData goData in results)
            //     {
            //         DevLog($"Found GameObject: {goData.Name}");
            //         data.
            //     }
            // }




        }


        public enum UIType
        {
            TypeA,
            TypeB,
            // add more types as needed
        }

        [System.Serializable]
        public class CustomWindowContainer
        {
            public UIType uiType;
            public string data;  // data required to rebuild the UI
        }

        // public class WindowManagerStateful
        // {
        //     public interface ICommand
        //     {
        //         void Execute();
        //     }

        //     public class RegisterWindowCommand : ICommand
        //     {
        //         private string id;
        //         private CustomWindowStateful window;
        //         private static WindowManagerStateful manager;

        //         public RegisterWindowCommand(string id, CustomWindowStateful window, WindowManagerStateful manager)
        //         {
        //             this.id = id;
        //             this.window = window;
        //             this.manager = manager;
        //         }

        //         public void Execute()
        //         {
        //             manager.serializableWindows.Add(new WindowManagerStateful.SerializableWindowState { id = id, window = window });
        //         }
        //     }

        //     public class DeregisterWindowCommand : ICommand
        //     {
        //         private string id;
        //         private WindowManagerStateful manager;

        //         public DeregisterWindowCommand(string id, WindowManagerStateful manager)
        //         {
        //             this.id = id;
        //             this.manager = manager;
        //         }

        //         public void Execute()
        //         {
        //             manager.serializableWindows.RemoveAll(w => w.id == id);
        //         }
        //     }


        //     [System.Serializable]
        //     public struct SerializableWindowState
        //     {
        //         public string id;
        //         public CustomWindowStateful window;
        //     }

        //     // Serializable list to store window states
        //     [SerializeField]
        //     public List<SerializableWindowState> serializableWindows = new List<SerializableWindowState>();

        //     // Command Queue
        //     private Queue<ICommand> commandQueue = new Queue<ICommand>();

        //     // Register a new window
        //     public bool RegisterWindow(string id, CustomWindowStateful window)
        //     {
        //         // Validation logic (simplified for example)
        //         if (serializableWindows.Exists(w => w.id == id))
        //         {
        //             DevLogWarning("A window with this ID already exists: " + id);
        //             return false;
        //         }

        //         // Queue the registration command
        //         QueueCommand(new RegisterWindowCommand(id, window, this));

        //         // Execute commands to actually perform the registration
        //         ExecuteCommands();

        //         return true;
        //     }

        //     // Deregister a window
        //     public void DeregisterWindow(string id)
        //     {
        //         // Validation logic (simplified for example)
        //         if (!serializableWindows.Exists(w => w.id == id))
        //         {
        //             DevLogWarning("No window registered with this ID: " + id);
        //             return;
        //         }

        //         // Queue the deregistration command
        //         QueueCommand(new DeregisterWindowCommand(id, this));

        //         // Execute commands to actually perform the deregistration
        //         ExecuteCommands();
        //     }

        //     // Queue a command for execution
        //     private void QueueCommand(ICommand command)
        //     {
        //         commandQueue.Enqueue(command);
        //     }

        //     // Execute all queued commands
        //     private void ExecuteCommands()
        //     {
        //         while (commandQueue.Count > 0)
        //         {
        //             ICommand command = commandQueue.Dequeue();
        //             command.Execute();
        //         }
        //     }

        //     // Clear the command queue (optional, for debugging/testing)
        //     public void ClearCommands()
        //     {
        //         commandQueue.Clear();
        //     }
        // }


        // public static class WindowManager
        // {
        //     public static Dictionary<string, CustomWindowStateful> WindowDictionary = new Dictionary<string, CustomWindowStateful>();
        //     public static bool IsRegistered(string id)
        //     {
        //         return WindowDictionary.ContainsKey(id);
        //     }
        //     public static bool RegisterWindow(string id, CustomWindowStateful window)
        //     {
        //         if (WindowDictionary.ContainsKey(id))
        //         {
        //             DevLogWarning("A window with this ID already exists: " + id);
        //             return false;
        //         }
        //         // Check for any open EditorWindow instances with a title matching the provided id
        //         foreach (var openWindows in Resources.FindObjectsOfTypeAll<EditorWindow>())
        //         {
        //             if (openWindows.titleContent.text == id)
        //             {
        //                 DevLogWarning("An editor window with this ID already exists: " + id);
        //                 return false;
        //             }
        //         }

        //         DevLog("Registering window with ID: " + id);
        //         WindowDictionary[id] = window;
        //         BuildWindowBar();
        //         return true;
        //     }

        //     public static void DeregisterWindow(string id)
        //     {
        //         if (!WindowDictionary.ContainsKey(id))
        //         {
        //             DevLogWarning("No window registered with this ID: " + id);
        //             return;
        //         }
        //         WindowDictionary.Remove(id);
        //         BuildWindowBar();
        //     }
        // }

        [Serializable]
        public class MiniMapData
        {
            public Texture2D map;
            [SerializeField] private string mapPath;
            [SerializeField] private Vector3 cachePos;
            [SerializeField] private float widthCache;
            [SerializeField] private float heightCache;
            [SerializeField] private float orthSizeCache;
            [SerializeField] private Vector3 position;
            [SerializeField] private Quaternion rotation;
            [SerializeField] private float orthSize = 200f;
            [SerializeField] private float pixelWidth;
            [SerializeField] private float pixelHeight;
            [SerializeField] private Vector3 topleft;
            [SerializeField] private Vector3 topright;
            [SerializeField] private Vector3 botleft;
            [SerializeField] private Vector3 botright;

            // Properties with backing fields
            public string MapPath { get => mapPath; set => mapPath = value; }
            public Vector3 CachePos { get => cachePos; set => cachePos = value; }
            public float WidthCache { get => widthCache; set => widthCache = value; }
            public float HeightCache { get => heightCache; set => heightCache = value; }
            public float OrthSizeCache { get => orthSizeCache; set => orthSizeCache = value; }
            public Vector3 Position { get => position; set => position = value; }
            public Quaternion Rotation { get => rotation; set => rotation = value; }
            public float OrthSize { get => orthSize; set => orthSize = value; }
            public float PixelWidth { get => pixelWidth; set => pixelWidth = value; }
            public float PixelHeight { get => pixelHeight; set => pixelHeight = value; }
            public Vector3 Topleft { get => topleft; set => topleft = value; }
            public Vector3 Topright { get => topright; set => topright = value; }
            public Vector3 Botleft { get => botleft; set => botleft = value; }
            public Vector3 Botright { get => botright; set => botright = value; }

            public void SaveImage()
            {
                // DevLog("SaveImage()", 638374131298693101);
                if (map == null)
                {
                    map = new Texture2D(1, 1);
                }

                //
                byte[] bytes = map.EncodeToPNG();
                MapPath = $"map_{sceneAnchor.DataSetName}";
                File.WriteAllBytes($"{MightyCoreData.GetCache()}map_{sceneAnchor.DataSetName}.png", bytes);
                // DevLog($"Saved Map Image {MapPath} and it is {map != null}", 638374131298693101);
            }

            public Texture2D GetMapTexture()
            {
                // DevLog("LoadImage()", 638374131298693101);

                Texture2D mapTexture = Resources.Load($"Cache/{MapPath}", typeof(Texture2D)) as Texture2D;
                // DevLog($"Loaded Landmark Image {MapPath} and it is {mapTexture != null}", 638374131298693101);

                if (mapTexture == null)
                {
                    mapTexture = new Texture2D(1, 1);
                    //setpixels to red
                    mapTexture.SetPixel(0, 0, Color.red);
                }
                return mapTexture;
            }


        }

        //public  int GetMapXCoord(float x, MiniMap miniMap)
        //{
        //    targetRatio = (float)screenWidth / (float)screenHeight;
        //    hh = miniMap.topleft.z - miniMap.botleft.z;
        //    ww = hh * targetRatio;
        //    xOffset = ((miniMap.topright.x - miniMap.topleft.x) / 2) - (ww / 2);
        //    x1 = miniMap.topleft.x + xOffset;
        //    x2 = miniMap.topright.x - xOffset;
        //    z1 = miniMap.botright.z;
        //    z2 = miniMap.topleft.z;
        //    return (int)((1 - ((x2 - x) / ww)) * screenWidth);
        //}

        //public  int GetMapZCoord(float z)
        //{
        //    return (int)((1 - ((z - z1) / hh)) * screenHeight);
        //}

        public Vector2 GetMapCoords(float x, float z)
        {
            targetRatio = (float)screenWidth / (float)screenHeight;
            hh = sceneData.MiniMap.Topleft.z - sceneData.MiniMap.Botleft.z;
            ww = hh * targetRatio;
            xOffset = ((sceneData.MiniMap.Topright.x - sceneData.MiniMap.Topleft.x) / 2) - (ww / 2);
            x1 = sceneData.MiniMap.Topleft.x + xOffset;
            x2 = sceneData.MiniMap.Topright.x - xOffset;
            z1 = sceneData.MiniMap.Botright.z;
            z2 = sceneData.MiniMap.Topleft.z;

            return
                new Vector2((int)((1 - ((x2 - x) / ww)) * screenWidth),
                            (int)((1 - ((z - z1) / hh)) * screenHeight));
        }


        [Serializable]
        public class ColorTexture
        {
            [SerializeField]
            public Texture2D texture;
            [SerializeField]
            public Color color;
        }


        [Serializable]
        public class MappableTypeInfo
        {
            public string TypeName; // Store the type name as a string
            public string Name;     // Store the human-readable name
            public bool IsActive = true;   // Store the active state
            public IMappable Mappable; // Store the actual type

            public MappableTypeInfo(string typeName, string name, bool isActive, IMappable mappable)
            {
                DevLog($"Mappable TypeInfo {typeName} {name} {isActive} {mappable.ToString()}");
                TypeName = typeName;
                Name = name;
                IsActive = isActive;
                Mappable = mappable;
            }
        }


        public interface IMappable
        {

            int ID { get; set; }
            int ParentId { get; set; }
            string AnchorTo { get; set; }
            string Name { get; set; }
            string Description { get; set; }
            bool Active { get; set; }
            bool Front { get; set; }
            bool Dirty { get; set; }
            bool HasVisualContent { get; set; }

            Views ViewUI { get; set; }
            VisualElement PrevView { get; set; }


            Vector3 Offset { get; set; }
            Attributes MapAttributes { get; set; }
            Location MapLocation { get; set; }
            Picture Pic { get; set; }

            Texture2D Icon { get; set; }

            void RegisterMappable();

            void OnGenerateVisualContent(MeshGenerationContext mgc);

            void Delete();

            void LoadImage();



            Button AddMappable(bool setClickedCallback = true);
            CustomToggleButton AddModuleToggle(MappableTypeInfo mappableTypeInfo);

            VisualElement SceneSummary(SceneData scene);
            VisualElement SettingsView();

        }

        // public class ModuleToggle : VisualElement
        // {
        //     public Texture2D iconOn, iconOff;
        //     public bool isOn;


        // }

        [Serializable]
        public class Views
        {
            private Stack<VisualElement> viewStack;
            public VisualElement root;
            private Dictionary<string, VisualElement> views;
            //initial values for width and height because UI objects don't immediately populate
            public float maxWidth, maxHeight;

            public Views()
            {

                Clear();
            }

            public void Init()
            {
                viewStack = new Stack<VisualElement>();
                root = new VisualElement();
                views = new Dictionary<string, VisualElement>();
            }

            public void SetRoot(VisualElement root)
            {
                this.root = root;
                //DevLog($"Setting Root of this Mappable to {root}");
            }

            public VisualElement GetRoot()
            {
                DevLog($"GetRoot {root}");
                return root;
            }

            public bool AddView(string uxml, int mw, int mh)
            {
                DevLog($"AddView {uxml}   {mw}/{mh}");
                var vta = Resources.Load<VisualTreeAsset>(uxml);
                DevLog($"vta {vta}");
                if (vta == null) return false;

                if (views == null) views = new Dictionary<string, VisualElement>();

                DevLog(uxml);

                if (!views.ContainsKey(uxml))
                {
                    var ve = vta.CloneTree().Query<VisualElement>().First();
                    ve.style.width = maxWidth = mw;
                    ve.style.height = maxHeight = mh;
                    views.Add(uxml, ve);
                    DevLog(ve);
                }

                DevLog(uxml);


                // DebugDictionary();
                // load all predefined views from UXML files and store them in predefinedViews dictionary
                return true;
            }

            IEnumerator WaitForLayout(string uxml)
            {
                var visualElement = new VisualElement();
                // load visual elements from UXML file
                var uxmlFile = Resources.Load<VisualTreeAsset>(uxml);
                uxmlFile.CloneTree(visualElement);
                yield return null;
                var width = visualElement.layout.width;
            }


            public void PushView(string viewName)
            {
                DevLog($"PushView {viewName}");
                // clear the root element before adding the new view
                // root.Clear();
                // push the new view to the stack
                if (viewStack == null) viewStack = new Stack<VisualElement>();
                viewStack.Push(views[viewName]);
                DevLog($"Pushview viewstack count: {viewStack.Count}");
                DevLog($"viewStack.Peek().name {viewStack.Peek().name}");
                // add the new view to the root element
                // root.Add(views[viewName]);
                // root.MarkDirtyRepaint();
            }

            public void PopView()
            {
                DevLog("PopView");
                if (viewStack == null) return;
                viewStack.Pop();
                // remove the current view from the root element
                // root.Remove(viewStack.Pop());
                // // add the previous view to the root element
                // root.Add(viewStack.Peek());
                // root.MarkDirtyRepaint();
            }

            public VisualElement GetView()
            {
                if (viewStack == null) viewStack = new Stack<VisualElement>();
                if (viewStack.Count == 0)
                {
                    DevLog($"GetView stack is empty");
                    return null;
                }
                // DevLog("GetView");

                DevLog($"---GetView() viewStack.Peek().Children().First().name {viewStack.Peek().Children().First().name}");
                var r = viewStack.Peek();
                if (r == null)
                {
                    DevLog($"---GetView() r is null");
                    r = new VisualElement();
                }
                return r;
            }

            public VisualElement GetView(string viewName)
            {
                if (views == null) views = new Dictionary<string, VisualElement>();
                if (views.Count == 0)
                {
                    DevLog($"views is empty");
                    return null;
                }
                //DevLog("GetView");
                if (views.ContainsKey(viewName))
                    return views[viewName];
                return null;
            }

            //
            public void RefreshView()
            {
                //DevLog($"RefreshView viewstack: {viewStack.Count} items");

                root.Clear();
                if (viewStack.Count == 0)
                {
                    foreach (var item in views)
                    {
                        DevLog(item.Key + " : " + item.Value);
                    }
                    return;
                }

                root.Add(viewStack.Peek());
                root.MarkDirtyRepaint();
            }

            public void ResetViewsToFirst()
            {
                var firstView = viewStack.First();
                viewStack.Clear();
                viewStack.Push(firstView);
                root.Clear();
                root.Add(firstView);
                root.MarkDirtyRepaint();
            }

            public void Clear()
            {
                if (viewStack != null)
                    viewStack.Clear();
                if (root != null)
                {
                    root.Clear();
                    root.MarkDirtyRepaint();
                }
            }

            public void DebugDictionary()
            {
                string[] items = new string[views.Count];
                int i = 0;
                foreach (var item in views)
                {
                    items[i] = item.Key + " : " + item.Value;
                    i++;
                }
                DevLog($"Debug Dictionary {i}: {string.Join(", ", items)}");
            }
            // private VisualTreeAsset visualTreeAsset;
            // private VisualElement view;
            // [SerializeField]
            // private string uxml;
            // private int maxWidth, maxHeight;

            // public Views(string u, int mw, int mh)
            // {
            //     uxml = u;
            //     visualTreeAsset = Resources.Load<VisualTreeAsset>(uxml);
            //     view = visualTreeAsset.CloneTree().Query<VisualElement>().First();
            //     view.style.maxWidth = maxWidth = mw;
            //     view.style.maxHeight = maxHeight = mh;
            // }

            // public VisualElement GetView()
            // {
            //     return view;
            // }

            // public void Rebuild()
            // {
            //     visualTreeAsset = Resources.Load<VisualTreeAsset>(uxml);
            //     view = visualTreeAsset.CloneTree().Query<VisualElement>().First();
            //     view.style.maxWidth = maxWidth;
            //     view.style.maxHeight = maxHeight;
            // }
        }

        [Serializable]
        public class Picture
        {
            [SerializeField]
            public string path;
            [SerializeField]
            public string filename;
            [SerializeField]
            public int width;
            [SerializeField]
            public int height;
            [SerializeField]
            public string format;
            [SerializeField]
            public bool rotateWithMappable = false;
            [NonSerialized]
            public Texture2D img, background;
            [NonSerialized]
            public bool imgLoaded = false;

            public Picture()
            {
                path = "";
                filename = "none.jpg";
                width = 1;
                height = 1;
                format = "jpeg";
            }
        }

        public class Icons
        {
            public Texture2D mmCamera, trackableIcon, blueGearIcon,
            window_close, window_maximize, window_minimize, window_popout, window_resize,
            map_follow_sceneview, mightybot, mightyeye;

            public Icons()
            {
                mmCamera = Resources.Load("ui/mighty_icon_mmcamera") as Texture2D;
                trackableIcon = Resources.Load("trackable_icon") as Texture2D;
                blueGearIcon = Resources.Load("ui/mighty_icon_toggle_gear") as Texture2D;
                window_close = Resources.Load("ui/btn_window_close") as Texture2D;
                window_maximize = Resources.Load("ui/btn_window_maximize") as Texture2D;
                window_minimize = Resources.Load("ui/btn_window_minimize") as Texture2D;
                window_popout = Resources.Load("ui/btn_window_popout") as Texture2D;
                window_resize = Resources.Load("ui/btn_window_resize") as Texture2D;
                map_follow_sceneview = Resources.Load("ui/btn_follow_sceneview") as Texture2D;
                mightybot = Resources.Load("ui/mightybot") as Texture2D;
                mightyeye = Resources.Load("ui/mightyeye") as Texture2D;
            }
        }
        //
        public const string corePath = "Assets/MightyDevOps";

        public Vector3 svPos, _svPos;
        public Quaternion svRot, _svRot;

        //public int sceneIndex = 0, sceneIndexPrev = 0;



        // [Serializable]
        // public class Offset
        // {
        //     public float x;
        //     public float y;
        //     public float z;

        //     public override string ToString()
        //     {
        //         return $"x: {x}, y: {y}, z: {z}";
        //     }
        // }

        [Serializable]
        public class Location
        {
            public Vector3 worldPosition;
            public float top, left;
            public Rect rect;
            public Quaternion worldRotation;
        }

        [Serializable]
        public class Attributes
        {
            public Color textMainColor;
            public Color backgroundColor;
            public Color textAccentColor;
            public Color backgroundAccentColor;
        }


        [SerializeField]
        public int landmarkMaxId;
        public int selectedID;

        static public Vector3 GetSVCameraPosition()
        {
            var cameras = SceneView.GetAllSceneCameras();
            Vector3 r = Vector3.zero;
            for (int i = 0; i < cameras.Length; i++)
            {
                if (SceneView.currentDrawingSceneView != null)
                {
                    if (SceneView.currentDrawingSceneView.camera.transform.position == cameras[i].transform.position) r = cameras[i].transform.position;
                    break;
                }

                if (SceneView.lastActiveSceneView != null)
                    if (SceneView.lastActiveSceneView.camera.transform.position == cameras[i].transform.position) r = cameras[i].transform.position;

            }
            return r;
        }

        static public Quaternion GetSVCameraRotation()
        {
            var cameras = SceneView.GetAllSceneCameras();
            Quaternion r = Quaternion.identity;
            for (int i = 0; i < cameras.Length; i++)
            {
                if (SceneView.currentDrawingSceneView != null)
                {
                    if (SceneView.currentDrawingSceneView.camera.transform.position == cameras[i].transform.position) r = cameras[i].transform.rotation;
                    break;
                }

                if (SceneView.lastActiveSceneView != null)
                    if (SceneView.lastActiveSceneView.camera.transform.position == cameras[i].transform.position) r = cameras[i].transform.rotation;
            }
            return r;
        }

        static public float GetSVCOrthographicSize()
        {
            var cameras = SceneView.GetAllSceneCameras();
            float r = 0;
            for (int i = 0; i < cameras.Length; i++)
            {
                if (SceneView.currentDrawingSceneView != null)
                {
                    if (SceneView.currentDrawingSceneView.camera.transform.position == cameras[i].transform.position) r = SceneView.currentDrawingSceneView.camera.orthographicSize;
                    break;
                }

                if (SceneView.lastActiveSceneView != null)
                    if (SceneView.lastActiveSceneView.camera.transform.position == cameras[i].transform.position) r = SceneView.lastActiveSceneView.camera.orthographicSize;
            }
            return r;
        }

        static public SceneView GetSceneView()
        {
            if (SceneView.lastActiveSceneView != null) return SceneView.lastActiveSceneView;
            if (SceneView.currentDrawingSceneView != null) return SceneView.currentDrawingSceneView;
            DevLogError("No SceneView found");
            return null;
        }

        static string m_ScriptFilePath;
        static string m_ScriptFolder;
        static public string MightyPath, MightyCache;
        static public MightyCoreData dataCore;

        static public string GetAssetPath()
        {
            //m_ScriptFilePath = AssetDatabase.GetAssetPath(dataCore);
            ////DevLog($"m_ScriptFilePath: {m_ScriptFilePath}");
            //FileInfo fi = new FileInfo(m_ScriptFilePath);
            //m_ScriptFolder = fi.Directory.ToString();

            //string x = m_ScriptFolder;
            //DevLog(x.IndexOf("TaskAtlasOnline\\"));
            ////if (x != null && x.Length > 0)
            //DevLog(x.Substring(0, x.IndexOf("TaskAtlasOnline")) + "TaskAtlasOnline\\");
            //x = x.Substring(0, x.IndexOf("TaskAtlasOnline")) + "TaskAtlasOnline\\";
            //x = x.Replace("\\", "/");

            //DevLog(x);
            var x = "Assets/MightyDevOps/";
            return x; //.Replace("//", "/").Replace("Assets/", "");//
        }

        static public void SetPath()
        {
            MightyPath = GetAssetPath();
            MightyCache = MightyPath + "Resources/Cache/";
            DevLog("Path set to " + MightyPath + "|" + Application.dataPath);
        }

        static public string GetPath()
        {
            DevLog(MightyPath);
            if (MightyPath == null || MightyPath == "") SetPath();
            DevLog("Getting path to " + MightyPath + "|" + Application.dataPath);
            return MightyPath;
        }
        static public string GetCache()
        {
            if (MightyCache == null || MightyCache == "") SetPath();
            return MightyCache;
        }

        public static VisualElement GetUXML(string uxml)
        {
            DevLog($"GetUXML {uxml}");
            var vta = Resources.Load<VisualTreeAsset>(uxml);
            var ve = vta.CloneTree().Query<VisualElement>().First();
            return ve;
        }

        static public Color StringToColor(string inputString, float brightness = 1.0f)
        {
            // Create a hash of the input string
            int hash = inputString.GetHashCode();

            // Use bitwise operations to get the first, second, and third bytes of the hash
            // Each byte is an integer between 0 and 255, which we then normalize to a float between 0 and 1
            float r = ((hash >> 24) & 0xFF) / 255f;
            float g = ((hash >> 16) & 0xFF) / 255f;
            float b = ((hash >> 8) & 0xFF) / 255f;

            // We're ignoring the least significant byte, as it won't have much visual impact

            // To ensure we don't exceed 0.6 in intensity for any of the RGB components, 
            // we find the max RGB value and divide all the RGB components by it to normalize them between 0 and 1
            // then multiply them by 0.6
            float maxRGB = Mathf.Max(Mathf.Max(r, g), b);
            r = r / maxRGB * 0.6f;
            g = g / maxRGB * 0.6f;
            b = b / maxRGB * 0.6f;

            // Adjust the brightness of the color
            float maxBrightness = Mathf.Max(r, Mathf.Max(g, b));
            if (maxBrightness > brightness)
            {
                float brightnessScale = brightness / maxBrightness;
                r *= brightnessScale;
                g *= brightnessScale;
                b *= brightnessScale;
            }

            return new Color(r, g, b, 1);
        }


        static public List<ColorTexture> colorTextures;

        static public Texture2D MakeTex(int width, int height, Color col)
        {
            if (width == 0 | height == 0 | width < 0 | height < 0)
                width = height = 1;

            if (colorTextures == null) colorTextures = new List<ColorTexture>();
            for (int i = 0; i < colorTextures.Count; i++)
            {
                if (col == colorTextures[i].color && colorTextures[i].texture != null)
                {
                    return colorTextures[i].texture;
                }
            }
            //DevLog("w: " + width + " h: " + height);
            Color[] pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            colorTextures.Add(new ColorTexture() { texture = result, color = col });
            colorTextures[colorTextures.Count - 1].texture.name = "Core Texture " + col.ToString();

            return result;
        }

        // Helper method to desaturate colors for subtler backgrounds
        public static Color Desaturate(Color color, float amount)
        {
            float h, s, v;
            Color.RGBToHSV(color, out h, out s, out v);
            s -= amount;
            return Color.HSVToRGB(h, Mathf.Clamp01(s), v);
        }

        static public bool Overlaps(Rect rect1, Rect rect2)
        {
            return rect1.x < rect2.xMax && rect1.xMax > rect2.x && rect1.y < rect2.yMax && rect1.yMax > rect2.y;
        }

        public static bool SignificantOverlap(Rect rect1, Rect rect2, float threshold)
        {
            // Find intersection rectangle
            Rect intersection = Rect.MinMaxRect(
                Mathf.Max(rect1.xMin, rect2.xMin),
                Mathf.Max(rect1.yMin, rect2.yMin),
                Mathf.Min(rect1.xMax, rect2.xMax),
                Mathf.Min(rect1.yMax, rect2.yMax)
            );

            // If there is no intersection, return false
            if (intersection.width <= 0 || intersection.height <= 0)
            {
                return false;
            }

            // Calculate the areas
            float area1 = rect1.width * rect1.height;
            float area2 = rect2.width * rect2.height;
            float intersectionArea = intersection.width * intersection.height;

            // Check if the intersection is significant based on the threshold
            // DevLog($"SignificantOverlap {intersectionArea / area1} {intersectionArea / area2} {threshold}");
            return (intersectionArea / area1 >= threshold) || (intersectionArea / area2 >= threshold);
        }

        public static string AsStringValue(SerializedProperty sp)
        {
            switch (sp.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return sp.intValue.ToString();
                case SerializedPropertyType.Boolean:
                    return sp.boolValue.ToString();
                case SerializedPropertyType.Float:
                    return sp.floatValue.ToString();
                case SerializedPropertyType.String:
                    return sp.stringValue;
                case SerializedPropertyType.Color:
                    return sp.colorValue.ToString();
                case SerializedPropertyType.ObjectReference:
                    return sp.objectReferenceValue ? sp.objectReferenceValue.name : "null";
                case SerializedPropertyType.Vector2:
                    return sp.vector2Value.ToString();
                case SerializedPropertyType.Vector3:
                    return sp.vector3Value.ToString();
                case SerializedPropertyType.Enum:
                    return sp.enumNames[sp.enumValueIndex];
                // ... (handle other types as needed)
                default:
                    return sp.ToString();
            }
        }

        public class Debouncer
        {
            private float debounceTime;
            private double lastInvokeTime;
            private System.Action debouncedAction;

            public Debouncer(float debounceTimeInSeconds)
            {
                this.debounceTime = debounceTimeInSeconds;
            }

            public void Invoke(System.Action action)
            {
                debouncedAction = action;
                double currentTime = EditorApplication.timeSinceStartup;
                float timeDifference = (float)(currentTime - lastInvokeTime);

                if (timeDifference < debounceTime)
                {
                    EditorApplication.delayCall -= ExecuteAction;
                    EditorApplication.delayCall += ExecuteAction; // Re-registering the callback
                }
                else
                {
                    ExecuteAction();
                }

                lastInvokeTime = currentTime;
            }

            private void ExecuteAction()
            {
                debouncedAction?.Invoke();
                EditorApplication.delayCall -= ExecuteAction; // Unregister the callback after execution
            }
        }

    }

}