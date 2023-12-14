using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mighty;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;
using static Mighty.MightyCoreData;

namespace MightyLandmarks
{

    public class LandmarksData : ScriptableObject
    {

        public static void Save()
        {
            string path = $"{corePath}/Modules/Landmarks/Data/LandmarksData.asset";
            if (File.Exists(path))
            {
                DevLog($"{path} already exists...");
                return;
            }

            LandmarksData asset = ScriptableObject.CreateInstance<LandmarksData>();

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
        }

        public static LandmarksData Load()
        {
            string path = $"{corePath}/Modules/Landmarks/Data/LandmarksData.asset";
            if (!File.Exists(path))
            {
                Save();
            }
            //return Resources.Load("LandmarkModuleData", typeof(LandmarkData)) as LandmarkData;
            return AssetDatabase.LoadAssetAtPath<LandmarksData>(path);
        }

        // [SerializeField]
        // public List<Landmark.Root> landmarks;
        [SerializeField]
        public List<Scene> scenes;
        private static bool clickedFrontView = false;

        [Serializable]
        public class Scene
        {
            public string name;
            [SerializeField]
            public List<Landmark.Root> landmarks;
        }

        const string iconAddMappablePath = "mighty_icon_add_landmark";
        Color defaultColor = new Color(1, 0, 0, 1f);
        // float defaultScale = 1.5f;


        [Serializable]
        public class Landmark
        {


            [Serializable]
            public class SceneViewLabel
            {
                public bool fade;
                public bool show;
                public Vector3 offset;
                public int fadeMax;
                public int fadeMin;
            }


            [Serializable]
            public class Root : IMappable
            {

                public void InitViews()
                {
                    if (Pic == null) LoadImage();
                    //if (view == null) 
                    View = new VisualElement();
                    //                    views.SetRoot(view);
                    ViewUI = new Views();
                    ViewUI.AddView("landmark_front", 96, 96);
                    ViewUI.AddView("landmark_back", 128, 128);
                    ViewUI.AddView("landmark_back_s1", 192, 192);



                    var frontView = ViewUI.GetView("landmark_front");
                    var backView = ViewUI.GetView("landmark_back");
                    var backs1View = ViewUI.GetView("landmark_back_s1");

                    DevLog($"InitViews {frontView}");
                    DevLog($"InitViews {backView}");
                    DevLog($"InitViews {backs1View}");


                    frontView.AddToClassList("lm_anchor_" + anchorTo);
                    //frontView.AddToClassList("mappable");
                    frontView.name = Pic.filename;
                    frontView.styleSheets.Add(Resources.Load("UI/mightystyles", typeof(StyleSheet)) as StyleSheet);
                    //frontView.style.maxWidth = 128;

                    frontView.RegisterCallback<MouseEnterEvent>((evt) =>
                    {
                        //DevLog("Hovering!");
                        frontView.BringToFront();
                        //isHoveringOnMappable = true;
                        frontView.Query(className: "showOnHover")
                            .ForEach((element) =>
                            {
                                element.RemoveFromClassList("fadeOut");
                                element.AddToClassList("fadeIn");

                                string c = "";
                                foreach (var x in element.GetClasses())
                                {
                                    c += "," + x.ToString();
                                }
                                //DevLog($"{element.name}: {c}");
                            });
                    }
                    );

                    frontView.RegisterCallback<MouseLeaveEvent>((evt) =>
                    {
                        //                        DevLog("Leaving!");
                        //isHoveringOnMappable = false;
                        frontView.Query(className: "showOnHover").ForEach((element) =>
                        {
                            element.RemoveFromClassList("fadeIn");
                            element.AddToClassList("fadeOut");
                            string c = "";
                            foreach (var x in element.GetClasses())
                            {
                                c += "," + x.ToString();
                            }
                            //                          DevLog($"{element.name}: {c}");
                        });
                    }
                    );

                    frontView.visible = true;

                    var button = new Button();
                    button = frontView.Q<Button>(className: "delete");
                    {
                        button.tooltip = "Delete Landmark";
                        button.name = "X";

                        button.clicked += () =>
                        {
                            Delete();
                            var v = root.Q<VisualElement>(name: Pic.filename);
                            if (v != null) v.RemoveFromHierarchy();
                            UpdateMappables();
                        };
                        button.visible = true;
                    }


                    button = frontView.Q<Button>(className: "gotoLandmark");
                    if (button != null)
                    {
                        button.tooltip = "Go To Landmark";
                        button.name = "GO";


                        button.clicked += () =>
                        {
                            var mp = MapLocation.worldPosition;
                            var mr = MapLocation.worldRotation;
                            DevLog((object)Name);
                            var sv = GetSceneView();
                            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            cube.transform.position = new Vector3(mp.x, mp.y, mp.z);
                            cube.transform.rotation = new Quaternion(mr.x, mr.y, mr.z, mr.w);

                            sv.AlignViewToObject(cube.transform);
                            sv.Repaint();
                            GameObject.DestroyImmediate(cube);

                        };
                        button.visible = true;
                    }

                    button = frontView.Q<Button>(className: "thumbnail");
                    {
                        Front = !Front;
                        button.tooltip = Name; //description;
                        button.name = Pic.filename;
                        //                        DevLog($"Creating button {pic.filename}");

                        if (Pic.img == null) LoadImage();
                        button.style.backgroundImage = Pic.img;


                        button.clicked += () =>
                        {
                            if (clickedFrontView == true) return;
                            clickedFrontView = true;
                            Dirty = true;

                            ViewUI.PushView("landmark_back");

                            var landmarkName = backView.Q<TextField>(name: "LandmarkName");
                            if (landmarkName != null)
                            {
                                landmarkName.value = Name;
                                new EditableLabel(landmarkName, newValue => Name = newValue);
                            }

                            var landmarkDescription = backView.Q<TextField>(name: "LandmarkDescription");
                            if (landmarkDescription != null)
                            {
                                landmarkDescription.value = Description;
                                new EditableLabel(landmarkDescription, newValue => Description = newValue);
                            }
                            //backView.AddToClassList("openHorizontal");


                            View.style.top = View.style.top.value.value + ((View.style.height.value.value - backView.style.height.value.value) / 2);
                            View.style.left = View.style.left.value.value + ((View.style.width.value.value - backView.style.width.value.value) / 2);
                            View.MarkDirtyRepaint();
                            RebuildMappables();
                            //Rebuild();
                            clickedFrontView = false;
                        };
                        button.visible = true;
                    }


                    backView.RegisterCallback<WheelEvent>((evt) =>
                    {
                        evt.StopImmediatePropagation();
                    });

                    button = backView.Q<Button>(className: "back");
                    {
                        DevLog($"button is null? {button == null}");
                        Front = !Front;
                        button.name = Pic.filename;
                        //                        DevLog($"Creating button {pic.filename}");

                        if (Pic.img == null) LoadImage();
                        button.style.backgroundColor = Color.white;
                        button.style.color = Color.black;
                        //button.style.backgroundImage = Pic.img;

                        button.clicked += () =>
                        {
                            Dirty = true;
                            DevLog("back");

                            ViewUI.PopView();
                            frontView.Q<Button>(className: "thumbnail").tooltip = Name;
                            View.MarkDirtyRepaint();
                            RebuildMappables();
                        };
                        button.visible = true;
                    }





                    button = backs1View.Q<Button>(className: "back");
                    {
                        Front = !Front;
                        button.tooltip = Pic.filename; //description;
                        button.name = Pic.filename;
                        //                        DevLog($"Creating button {pic.filename}");

                        if (Pic.img == null) LoadImage();
                        button.style.backgroundImage = Pic.img;

                        button.clicked += () =>
                        {
                            Dirty = true;
                            DevLog("back");

                            ViewUI.PopView();

                            View.MarkDirtyRepaint();
                            Rebuild();
                        };
                        button.visible = true;
                    }


                    backView.AddToClassList(Pic.filename);

                    backView.name = Pic.filename;
                    backView.styleSheets.Add(Resources.Load("UI/mightystyles", typeof(StyleSheet)) as StyleSheet);
                    backView.visible = true;

                    View.Add(frontView);
                    View.Add(backView);

                    ViewUI.PushView("landmark_front");
                    View.MarkDirtyRepaint();
                }

                public void RegisterMappable()
                {
                    if (mappables.Contains(this)) return;
                    if (sceneData == null) LandmarksCore.GetSceneData();
                    if (AnchorTo == sceneData.Name)
                    {
                        DevLog($"Adding Mappable: {this.Name}");
                        mappables.Add(this);
                        InitViews();
                    }
                    else
                    {
                        DevLog($"Not Adding Mappable: {this.Name} as it is anchored to {AnchorTo} and not {sceneData.Name}");
                    }
                }

                public void OnGenerateVisualContent(MeshGenerationContext mgc)
                {
                    DevLog($"Landmark OnGenerateVisualContent - {ViewUI.GetView().ToString()}");
                }

                public void LoadImage()
                {
                    DevLog("LoadImage()");
                    if (Pic == null)
                    {
                        DevLogError($"Pic is Null for Landmark {Name}");
                        Pic = new Picture();
                    }
                    Pic.img = new Texture2D(1, 1);
                    //
                    Pic.filename = $"lm_{CreatedAt.ToString()}.jpg";
                    DevLog($"Landmark Pic Filename {Pic.filename}");
                    string fileName;
                    int lastIndex = Pic.filename.LastIndexOf('.');
                    if (lastIndex > -1)
                    {
                        fileName = Pic.filename.Substring(0, lastIndex);
                    }
                    else
                    {
                        fileName = Pic.filename;
                    }

                    Pic.img = Resources.Load($"Cache/{fileName}", typeof(Texture2D)) as Texture2D;
                    DevLog($"Loaded Landmark Image {fileName} and it is {Pic.img}");



                }


                public Button AddMappable(bool setClickedCallback = true)
                {
                    DevLog($"mappable adding creation button");
                    if (iconAddMappable == null)
                        iconAddMappable = Resources.Load<Texture2D>(iconAddMappablePath);
                    DevLog($"mappable adding creation button {iconAddMappablePath} {iconAddMappable != null}");
                    Button b = new();
                    b.name = "addLandmark";
                    b.tooltip = "Add Landmark";
                    b.style.backgroundColor = new Color(0, 0, 0, 0);
                    b.style.borderBottomColor = b.style.borderLeftColor = b.style.borderRightColor = b.style.borderTopColor = new Color(0, 0, 0, 0);

                    if (iconAddMappable != null)
                        b.style.backgroundImage = iconAddMappable;
                    else
                        b.text = "LMRK";

                    if (setClickedCallback)
                        b.clicked += () =>
                        {
                            DevLog("Landmark YAY");
                            DevLog("New Landmark");
                            if (LandmarksCore.sceneData.landmarks == null) LandmarksCore.sceneData.landmarks = new List<LandmarksData.Landmark.Root>();
                            MightyCoreData.sceneCamera = GetSceneView().camera;
                            LandmarksCore.sceneData.landmarks.Add(new LandmarksData.Landmark.Root(GetSVCameraPosition().ToString(), MightyCoreData.sceneCamera));
                            Rebuild();

                        };

                    //
                    return b;
                }

                public CustomToggleButton AddModuleToggle(MappableTypeInfo mappableTypeInfo)
                {
                    DevLog($"AddModuleToggle named {mappableTypeInfo.Name}");
                    return new(Icon, mappableTypeInfo);
                }

                public VisualElement SceneSummary(MightyCoreData.SceneData scene)
                {
                    VisualElement summary = new VisualElement
                    {
                        name = "Landmarks",
                        style =
                        {
                            flexDirection = FlexDirection.Row,
                            flexGrow = 0,
                            flexShrink = 0,
                            height = 80,
                        }
                    };

                    int count = 0;
                    foreach (var anchor in LandmarksCore.data.scenes)
                    {
                        if (anchor.name == scene.Name)
                        {
                            DevLog($"SceneSummary Found {anchor.name} in {scene.Name}");
                            foreach (var landmark in anchor.landmarks)
                            {
                                count++;
                                DevLog($"SceneSummary Found {landmark.Name} in {scene.Name}");
                                landmark.LoadImage();

                                VisualElement pic = new()
                                {
                                    name = landmark.Name,
                                    style = {
                                        minWidth = 64,
                                        minHeight = 64,
                                        height = 64,
                                        width = 64,
                                        marginLeft = 4,
                                        marginRight = 4,
                                        flexGrow=0,
                                        flexShrink=0,
                                        backgroundImage = landmark.Pic.img
                                    }
                                };

                                // Create a larger version of the image
                                VisualElement largePic = new()
                                {
                                    name = landmark.Name,
                                    style = {
                                        width = 128,
                                        height = 128,
                                        backgroundImage = landmark.Pic.img,
                                        position = Position.Absolute,
                                        display = DisplayStyle.None
                                    }
                                };

                                Label landmarkName = new()
                                {
                                    text = landmark.Name,
                                    style = {
                                        fontSize = 14,
                                        backgroundColor = new Color(0, 0, 0, 0.5f),
                                        color = Color.white,
                                        unityTextAlign = TextAnchor.MiddleCenter,
                                    }
                                };

                                largePic.Add(landmarkName);
                                pic.RegisterCallback<MouseEnterEvent>((evt) =>
                                {
                                    largePic.style.display = DisplayStyle.Flex;
                                    root.Add(largePic);
                                });

                                pic.RegisterCallback<MouseMoveEvent>((evt) =>
                                {
                                    // Position the larger pic relative to the mouse cursor
                                    largePic.style.left = evt.mousePosition.x + 10;
                                    largePic.style.top = evt.mousePosition.y + 10;
                                });

                                pic.RegisterCallback<MouseLeaveEvent>((evt) =>
                                {
                                    largePic.style.display = DisplayStyle.None;
                                });

                                pic.RegisterCallback<MouseDownEvent>((evt) =>
                                {
                                    if (evt.button == 0)
                                    {
                                        if (EditorSceneManager.GetActiveScene().path != scene.ScenePath)
                                        {
                                            DevLog($"Opening Scene {scene.Name} from {EditorSceneManager.GetActiveScene().name}"); ;
                                            //need a dialog window to confirm if we open the scene or not
                                            if (EditorUtility.DisplayDialog(
                                                "Open Scene",
                                                $"Are you sure you want to open {scene.Name}?",
                                                "Yes",
                                                "No"
                                            ))
                                                EditorSceneManager.OpenScene(scene.ScenePath, OpenSceneMode.Single);
                                        }
                                        DevLog($"Landmark {landmark.Name} clicked");
                                        var sv = GetSceneView();
                                        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                        cube.transform.position = new Vector3(landmark.MapLocation.worldPosition.x, landmark.MapLocation.worldPosition.y, landmark.MapLocation.worldPosition.z);
                                        cube.transform.rotation = new Quaternion(landmark.MapLocation.worldRotation.x, landmark.MapLocation.worldRotation.y, landmark.MapLocation.worldRotation.z, landmark.MapLocation.worldRotation.w);

                                        sv.AlignViewToObject(cube.transform);
                                        sv.Repaint();
                                        GameObject.DestroyImmediate(cube);
                                    }
                                });

                                summary.Add(pic);
                            }
                        }
                    }

                    ScrollView scrollView = new ScrollView(ScrollViewMode.Horizontal);
                    scrollView.style.height = Length.Percent(100);
                    scrollView.name = $"[{count}] Landmarks";
                    scrollView.Add(summary);
                    return scrollView;
                }


                public VisualElement SettingsView()
                {
                    VisualElement settingsView = new VisualElement();
                    settingsView.name = "TaskablesSettingsView";
                    settingsView.style.width = Length.Percent(100);

                    Label label = new()
                    {
                        text = "Default Color",
                        style = {
                            unityTextAlign = TextAnchor.MiddleCenter,
                            fontSize = 14,
                            color = Color.white
                        }
                    };
                    settingsView.Add(label);
                    ColorField colorField = new()
                    {
                        name = "TaskablesColorField",
                        value = Color.white
                    };
                    settingsView.Add(colorField);

                    colorField.RegisterCallback<ChangeEvent<Color>>((evt) =>
                    {
                        DevLog($"Color changed to {evt.newValue}");

                        var landmarkableElements = root.Query<VisualElement>(className: "landmarkable").ToList();

                        // Change the background color of each element to green
                        foreach (var element in landmarkableElements)
                        {
                            DevLog($"Changing color of {element.name} to {evt.newValue}");
                            element.style.backgroundColor = evt.newValue;
                        }
                    });


                    return settingsView;
                }

                [SerializeField] private string name;
                [SerializeField] private string description;
                [SerializeField] private string anchorTo;
                [SerializeField] private Location mapLocation;
                [SerializeField] private long createdAt;
                [SerializeField] private long lastModified;
                [SerializeField] private long lastQueried;
                [SerializeField] private string status;
                [SerializeField] private SceneViewLabel label;
                [SerializeField] private int id;
                [SerializeField] private int parentId;
                [SerializeField] private bool active;
                [SerializeField] private bool front;
                [SerializeField] private bool dirty;
                [SerializeField] private bool hasVisualContent;
                [SerializeField] private Views viewUI;
                [SerializeField] private VisualElement prevView;

                [SerializeField] private VisualElement view;
                [SerializeField] private Vector3 offset;
                [SerializeField] private Attributes mapAttributes;
                [SerializeField] private Picture pic;

                public string Name { get => name; set => name = value; }
                public string Description { get => description; set => description = value; }
                public string AnchorTo { get => anchorTo; set => anchorTo = value; }
                public Location MapLocation { get => mapLocation; set => mapLocation = value; }
                public long CreatedAt { get => createdAt; set => createdAt = value; }
                public long LastModified { get => lastModified; set => lastModified = value; }
                public long LastQueried { get => lastQueried; set => lastQueried = value; }
                public string Status { get => status; set => status = value; }
                public SceneViewLabel Label { get => label; set => label = value; }
                public int ID { get => id; set => id = value; }
                public int ParentId { get => parentId; set => parentId = value; }
                public bool Active { get => active; set => active = value; }
                public bool Front { get => front; set => front = value; }
                public bool Dirty { get => dirty; set => dirty = value; }
                public VisualElement PrevView { get => prevView; set => prevView = value; }
                public bool HasVisualContent { get => hasVisualContent; set => hasVisualContent = value; }
                public Views ViewUI { get => viewUI; set => viewUI = value; }
                public VisualElement View { get => view; set => view = value; }
                public Vector3 Offset { get => offset; set => offset = value; }

                public Attributes MapAttributes { get => mapAttributes; set => mapAttributes = value; }
                public Picture Pic { get => pic; set => pic = value; }

                private Texture2D _icon;

                public Texture2D Icon
                {
                    get
                    {
                        if (_icon == null)
                        {
                            _icon = Resources.Load<Texture2D>("mighty_icon_toggle_landmark2");
                        }
                        return _icon;
                    }
                    set
                    {
                        _icon = value;
                    }
                }



                public Texture2D iconAddMappable { get; set; }

                public override string ToString()
                {
                    return "Landmarks";
                }


                public void Delete()
                {
                    MightyCoreData.mappables.Remove(this);
                    LandmarksCore.sceneData.landmarks.Remove(this);
                }

                public Root()
                {

                }
                public Root(string n, Camera camera)
                {
                    DevLog($"Creating Landmark {n} at {camera.transform.position} with rotation {camera.transform.rotation} Camera Name: {camera.name}");
                    ID = 0;
                    LandmarksCore.sceneData.landmarks ??= new List<Root>();
                    if (LandmarksCore.sceneData.landmarks.Count > 0)
                        ID = LandmarksCore.sceneData.landmarks.Max(lm => lm.ID) + 1;
                    ParentId = 0;

                    AnchorTo = sceneData.Name;
                    Name = n;
                    Description = "New Landmark";
                    Status = "active";

                    CreatedAt = LastModified = LastQueried = DateTime.Now.Ticks;

                    HasVisualContent = false;




                    MapAttributes = new Attributes
                    {
                        textMainColor = new Color(1, 1, 1, 1f),
                        backgroundColor = new Color(0, 0, 0, 1f),
                        textAccentColor = new Color(0, 1, 1, 1f),
                        backgroundAccentColor = new Color(0, 1, 1, 1.0f)
                    };

                    MapLocation = new Location
                    {
                        worldPosition = camera.transform.position,//new Vector3(pos.x, pos.y, pos.z);
                        worldRotation = camera.transform.rotation//new Quaternion(rot.x, rot.y, rot.z, rot.w);
                    };
                    //transform.rotation = new Rotation();


                    Label = new SceneViewLabel
                    {
                        show = true,
                        fade = true,
                        fadeMin = 100,
                        fadeMax = 1000,
                        offset = new Vector3()
                    };
                    Label.offset.x = Label.offset.y = Label.offset.z = 0;


                    RenderTexture currentRT = new RenderTexture(1024, 1024, 24);
                    camera.targetTexture = currentRT;
                    camera.Render();

                    RenderTexture.active = currentRT;

                    //
                    Texture2D texture2D = new Texture2D(1024, 1024, TextureFormat.RGB24, false);
                    texture2D.ReadPixels(new Rect(0, 0, 1024, 1024), 0, 0);
                    texture2D.Apply();

                    camera.targetTexture = null;
                    RenderTexture.active = null;

                    var texture2DBytes = texture2D.EncodeToJPG(10);

                    Pic = new Picture
                    {
                        path = $"",
                        filename = $"lm_{CreatedAt.ToString()}.jpg",
                        format = "jpeg"
                    };
                    Pic.width = Pic.height = 1024;
                    Pic.img = texture2D;//new Texture2D(texture2D);
                    Pic.imgLoaded = true;


                    DevLog("Writing to " + Pic.path);

                    File.WriteAllBytes($"{MightyCoreData.GetCache()}{Pic.filename}",
                                       texture2DBytes);

                    RegisterMappable();
                    //DestroyImmediate(texture2D);
                }
            }

            // [Serializable]
            // public class Rotation
            // {
            //     public float x;
            //     public float y;
            //     public float z;
            //     public float w;
            // }

            // [Serializable]
            // public class Transform
            // {
            //     public Position position;
            //     public Rotation rotation;
            // }


        }
    }
}