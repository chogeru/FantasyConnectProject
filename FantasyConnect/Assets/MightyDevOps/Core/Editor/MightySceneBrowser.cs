using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MightyLandmarks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static Mighty.MightyCoreData;

namespace Mighty
{
    public class MightySceneBrowser : ScriptableObject
    {
        static Action saveState;
        public static void Save()
        {
            string path = $"{corePath}/Core/Data/MightySceneBrowserData.asset";
            if (File.Exists(path))
            {
                DevLog($"{path} already exists...");
                return;
            }

            MightySceneBrowser asset = ScriptableObject.CreateInstance<MightySceneBrowser>();
            DevLog($"Saving MightySceneBrowserData to {path}");
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
        }


        public static MightySceneBrowser Load()
        {
            string path = $"{corePath}/Core/Data/MightySceneBrowserData.asset";
            if (!File.Exists(path))
            {
                Save();
            }
            DevLog($"Loading MightySceneBrowserData from {path}");
            return AssetDatabase.LoadAssetAtPath<MightySceneBrowser>(path);
        }

        private void OnEnable()
        {
            EditorApplication.quitting += SaveState;
            AssemblyReloadEvents.beforeAssemblyReload += SaveState;
            saveState += SaveState;
        }

        void SaveState()
        {
            DevLog($"SaveState");

            if (this != null)
                EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public VisualElement view;
        ToolbarPopupSearchField sceneSelectSearchField;
        List<VisualElement> sceneCards = new();


        [System.Serializable]
        public class State
        {
            public string selectedScene;
            public int selectedTab;
        }

        [SerializeField]
        public State state;

        private int itemsLoaded = 0;
        private const int itemsPerLoad = 10;


        [System.Serializable]
        public enum SceneType
        {
            Environment,
            Overlay,
            // Add more types as needed.
        }

        List<String> sceneNames = new();
        public void BuildView()
        {
            MightyCore.data.SceneDupeCheck();
            state ??= new State();
            view = new VisualElement();
            view.name = "SceneBrowser";
            view.style.height = Length.Percent(100);
            view.style.flexGrow = 0;
            view.style.flexDirection = FlexDirection.Column;
            view.style.flexShrink = 0;
            view.style.overflow = Overflow.Hidden;
            view.style.flexWrap = Wrap.Wrap;
            view.style.justifyContent = Justify.SpaceAround;
            view.style.minHeight = 256;
            view.style.maxHeight = 512;
            view.style.minWidth = 256;
            view.style.maxWidth = 512;
            view.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            VisualElement top = new()
            {
                name = "SceneSelectTop",
                style = {
                flexDirection = FlexDirection.Row,
                flexGrow = 0,
                flexShrink = 0,
                height = 20,
                width = Length.Percent(100),
            }
            };

            ScrollView mid = new()
            {
                name = "SceneSelectMid",
                style = {
                flexDirection = FlexDirection.Row,
                flexGrow = 1,
                flexShrink = 1,
                height = Length.Percent(100),
                width = Length.Percent(100),
            }
            };

            VisualElement midContent = new()
            {
                name = "SceneSelectMidContent",
                style = {
                flexDirection = FlexDirection.Row,
                flexWrap = Wrap.Wrap,
                flexGrow = 1,
                flexShrink = 1,
                height = Length.Percent(100),
                width = Length.Percent(100),
            }
            };
            mid.Add(midContent);

            // VisualElement mid = new()
            // {
            //     name = "SceneSelectMid",
            //     style = {
            //         flexDirection = FlexDirection.Row,
            //         flexGrow = 1,
            //         flexShrink = 1,
            //         height = Length.Percent(100),
            //         width = Length.Percent(100),
            //     }
            // };

            VisualElement bottom = new()
            {
                name = "SceneSelectBottom",
                style = {
                flexDirection = FlexDirection.Row,
                flexGrow = 0,
                flexShrink = 0,
                height = 20,
                width = Length.Percent(100),
            }
            };

            view.Add(top);
            view.Add(mid);
            view.Add(bottom);


            VisualElement sceneDetails;
            sceneDetails = new()
            {
                name = "SceneBrowserDetails",
                style = {
                        //backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f),
                        color = Color.white,
                        height = Length.Percent(100),
                        width = Length.Percent(100),
                        flexGrow = 1,
                        flexShrink = 0,
                        position = Position.Absolute,
                        display = DisplayStyle.None,
                        flexDirection = FlexDirection.Column,
                    }
            };
            VisualElement sceneDetailsHeader = new()
            {
                name = "SceneBrowserDetailsHeader",
                style = {
                        backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f),
                        color = Color.white,
                        maxHeight = 20,
                        width = Length.Percent(100),
                        flexGrow = 1,
                        flexShrink = 1,
                        flexDirection = FlexDirection.Row,
                    }
            };
            Label sceneDetailsHeaderLabel = new()
            {
                name = "SceneBrowserDetailsHeaderLabel",
                text = "Scene Details",
                style = {
                        backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f),
                        color = Color.white,
                        height = 20,
                        flexGrow = 1,
                        flexShrink = 1,
                        fontSize = 16,
                        unityFontStyleAndWeight = FontStyle.Bold,
                    }
            };
            Button back = new()
            {
                name = "X",
                text = "Back",
                style = {
                        backgroundColor = Color.white,
                        color = Color.black,

                        flexGrow = 0,
                        flexShrink = 0,
                    }
            };
            back.clicked += () =>
            {
                sceneDetails.style.display = DisplayStyle.None;
                state.selectedScene = "";
            };

            VisualElement sceneDetailsContent = new()
            {
                name = "SceneBrowserDetailsContent",
                style = {
                        backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f),
                        color = Color.white,
                        //height = Length.Percent(100),
                        //width = Length.Percent(100),
                        flexGrow = 2,
                        flexShrink = 1,
                    }
            };
            sceneDetailsHeader.Add(back);
            sceneDetailsHeader.Add(sceneDetailsHeaderLabel);
            sceneDetails.Add(sceneDetailsHeader);
            sceneDetails.Add(sceneDetailsContent);

            // if (!MightyCoreData.isSceneAnchored)
            // {
            //     Button createData = new()
            //     {
            //         name = "createData",
            //         text = "Enable Mighty Map data for this scene",
            //         style = {
            //             height = 100,
            //             backgroundColor = Color.red,
            //             color = Color.white,
            //         }
            //     };
            //     createData.clicked += () =>
            //             {
            //                 var go = new GameObject("TAO");
            //                 //go.name = "TAO";
            //                 go.AddComponent<MightySceneAnchor>();
            //                 MightyCoreData.sceneAnchor = go.GetComponent<MightySceneAnchor>();
            //                 MightyCoreData.sceneAnchor.DataSetName = SceneManager.GetActiveScene().name;
            //                 MightyCore.data.scenes.Add(new MightyCoreData.SceneData());
            //                 var sceneIndex = MightyCore.data.scenes.Count - 1;
            //                 MightyCore.data.scenes[sceneIndex].Name = EditorSceneManager.GetActiveScene().name;
            //                 UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            //                 MightyCoreData.isSceneAnchored = true;
            //             };
            //     midContent.Add(createData);
            // }


            List<SceneData.GameObjectData> results = new List<SceneData.GameObjectData>();//MightyCoreData.sceneData.SearchCollectedData("", currentSearchType, isCaseSensitive);

            sceneSelectSearchField = new()
            {
                name = "SearchField",
                style =
            {
                flexGrow = 0,
                flexShrink = 0,
                width = Length.Percent(100),
            }
            };

            // Add search type options
            sceneSelectSearchField.menu.AppendAction("Name Search", (a) =>
            {
                currentSearchType = SearchType.Name;
                DevLog("Changed to Name Search.");
                results = MightyCoreData.sceneData.SearchCollectedData(searchQuery, currentSearchType, isCaseSensitive);

            }, (a) => currentSearchType == SearchType.Name ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

            sceneSelectSearchField.menu.AppendAction("Deep Search", (a) =>
            {
                currentSearchType = SearchType.Deep;
                DevLog("Changed to Deep Search.");
                results = MightyCoreData.sceneData.SearchCollectedData(searchQuery, currentSearchType, isCaseSensitive);

            }, (a) => currentSearchType == SearchType.Deep ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

            // Add a separator
            sceneSelectSearchField.menu.AppendSeparator();

            // Add case-sensitive toggle
            sceneSelectSearchField.menu.AppendAction("Case Sensitive", (a) =>
            {
                isCaseSensitive = !isCaseSensitive;
                DevLog($"Case sensitivity is now {(isCaseSensitive ? "enabled" : "disabled")}.");
                results = MightyCoreData.sceneData.SearchCollectedData(searchQuery, currentSearchType, isCaseSensitive);

            }, (a) => isCaseSensitive ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

            sceneSelectSearchField.RegisterValueChangedCallback((evt) =>
            {
                searchQuery = evt.newValue;
                DevLog($"Search string changed to {searchQuery}.");

                sceneNames.Clear();

                foreach (var scene in MightyCore.data.scenes)
                {
                    if (scene.DeleteMe) continue;
                    DevLog($"Search: Searching {scene.Name} with {scene.CollectedData.Count} objects");
                    results = scene.SearchCollectedData(searchQuery, currentSearchType, isCaseSensitive);
                    if (results.Count > 0)
                    {
                        sceneNames.Add(scene.Name);
                    }
                }

                DevLog($"Search: sceneButtons.Count: {sceneCards.Count} sceneSelectSearchField.value: {searchQuery}");

                foreach (var card in sceneCards)
                {
                    if (searchQuery != "" && sceneNames.Contains(card.name))
                    {
                        card.style.display = DisplayStyle.Flex;
                    }
                    else
                    {
                        card.style.display = searchQuery == "" ? DisplayStyle.Flex : DisplayStyle.None;
                    }
                }
            });
            top.Add(sceneSelectSearchField);


            foreach (var scene in MightyCore.data.scenes)
            {
                if (scene.Name == "Project" || scene.DeleteMe)
                {
                    continue;
                }

                VisualElement sceneCard = new()
                {
                    name = scene.Name,
                    style = {
                        width = Length.Percent(100),
                        height = 80,
                        marginBottom = 5,
                        flexGrow = 0,
                        flexShrink = 0,
                        flexDirection = FlexDirection.Row,
                        paddingBottom = 5,
                        paddingLeft = 5,
                        paddingRight = 5,
                        paddingTop = 5,
                    }
                };

                sceneCard.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);

                if (sceneData != null)
                    if (scene.Name == sceneData.Name)
                        sceneCard.style.backgroundColor = new Color(0.6f, 0.8f, 0.6f, 0.5f);


                Button sceneButton = new()
                {
                    name = scene.Name,
                    //text = scene.Name.Split("___").First(),
                    style = {
                        color = Color.white,
                        width = 64,
                        height = 64,
                        //backgroundImage = scene.MiniMap.GetMapTexture(),
                        flexGrow =0,
                        flexShrink=0,
                        borderTopRightRadius = 8,
                        borderBottomLeftRadius = 8,
                    }
                };
                icons ??= new();
                sceneButton.style.backgroundImage = icons.mightybot;


                // if (LandmarksCore.data != null)
                if (LandmarksCore.data == null) LandmarksCore.GetSceneData();

                foreach (var anchor in LandmarksCore.data.scenes)
                {
                    if (anchor.landmarks == null) continue;
                    if (anchor.name == scene.Name)
                    {
                        if (anchor.landmarks.Count > 0)
                        {
                            anchor.landmarks[0].LoadImage();
                            sceneButton.style.backgroundImage = anchor.landmarks[0].Pic.img;
                        }
                    }
                }

                sceneButton.clicked += () =>
                        {
                            state.selectedScene = scene.Name;
                            sceneDetails.style.display = DisplayStyle.Flex;
                            sceneDetails.style.backgroundImage = scene.MiniMap.map;
                            sceneDetailsHeaderLabel.text = scene.Name.Split("___").First();
                            ShowSceneInfoPanel(sceneDetailsContent, scene);
                            openDetails();
                        };
                VisualElement sceneLabelContainer = new()
                {
                    name = scene.Name + " Label Container",
                    style = {
                                flexDirection = FlexDirection.Column,
                                flexGrow = 1,
                                flexShrink = 1,
                                backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.5f),
                            }
                };
                Label sceneLabel = new()
                {
                    name = scene.Name + " Label",
                    text = scene.Name.Split("___").First(),
                    style = {
                                color = Color.white,
                                width = Length.Percent(100),

                                fontSize = 20,
                                unityFontStyleAndWeight = FontStyle.Bold,
                            }
                };
                VisualElement sceneLabelContainerData = new()
                {
                    name = scene.Name + " Label Container Data",
                    style = {
                                flexDirection = FlexDirection.Row,
                                flexGrow = 0,
                                flexShrink = 0,
                            }
                };

                scene.CollectedData ??= new();
                Label sceneObjectsCountRoundEdges = new()
                {
                    name = scene.Name + " Objects Count Round Edges",
                    text = scene.CollectedData.Count.ToString() + " Objects",
                    style = {
                                backgroundColor = new Color(0.1f, 0.1f, 0.3f),
                                color = Color.white,
                                //width = 64,
                                //height = 64,
                                flexGrow = 0,
                                flexShrink = 0,
                                fontSize = 9,
                                unityFontStyleAndWeight = FontStyle.Bold,
                                borderTopLeftRadius = 8,
                                borderTopRightRadius = 8,
                                borderBottomLeftRadius = 8,
                                borderBottomRightRadius = 8,
                            }
                };
                Label scenePolyCountRoundEdges = new()
                {
                    name = scene.Name + " Poly Count Round Edges",
                    text = scene.totalPolyCount.ToString() + " Polygons",
                    style = {
                                backgroundColor = new Color(0.5f, 0f, 0f),
                                color = Color.white,
                                //width = 64,
                                //height = 64,
                                flexGrow = 0,
                                flexShrink = 0,
                                fontSize = 9,
                                unityFontStyleAndWeight = FontStyle.Bold,
                                borderTopLeftRadius = 8,
                                borderTopRightRadius = 8,
                                borderBottomLeftRadius = 8,
                                borderBottomRightRadius = 8,
                            }
                };
                sceneCard.Add(sceneButton);
                sceneLabelContainer.Add(sceneLabel);
                sceneLabelContainerData.Add(sceneObjectsCountRoundEdges);
                sceneLabelContainerData.Add(scenePolyCountRoundEdges);
                sceneLabelContainer.Add(sceneLabelContainerData);
                sceneCard.Add(sceneLabelContainer);

                Button deleteAnchor = new()
                {
                    name = scene.Name + " Delete Anchor",
                    text = "X",
                    style = {
                        backgroundColor = new Color(0.5f, 0f, 0f),
                        color = Color.white,
                        //width = 64,
                        //height = 64,
                        flexGrow = 0,
                        flexShrink = 0,
                        fontSize = 9,
                        unityFontStyleAndWeight = FontStyle.Bold,
                        borderTopLeftRadius = 8,
                        borderTopRightRadius = 8,
                        borderBottomLeftRadius = 8,
                        borderBottomRightRadius = 8,
                    }
                };
                deleteAnchor.clicked += () =>
                        {
                            Debug.Log($"Deleting {scene.Name} / {scene.ScenePath}");
                            var thisScene = scene;
                            var thisSceneCard = sceneCard;
                            if (EditorUtility.DisplayDialog("Remove Mighty DevOps?", $"Do you want to unanchor this scene from Mighty DevOps?  It will remove any and all data (such as landmarks and any other module data) and will remove the hidden anchor object next time you load this scene.  It will NOT delete your actual scene just our reference to it.", "Yes", "No"))
                            {
                                scene.DeleteMe = true;
                                sceneCard.RemoveFromHierarchy();
                            }
                        };
                sceneCard.Add(deleteAnchor);

                sceneCards.Add(sceneCard);
                midContent.Add(sceneCard);

                if (state.selectedScene != null && state.selectedScene == scene.Name)
                {
                    sceneDetails.style.display = DisplayStyle.Flex;
                    sceneDetails.style.backgroundImage = scene.MiniMap.map;
                    sceneDetailsHeaderLabel.text = scene.Name;
                    ShowSceneInfoPanel(sceneDetailsContent, scene);
                    openDetails();
                }
            }

            view.Add(sceneDetails);
        }

        void openDetails()
        {
            DevLog($"Opening details for {state.selectedScene}");



            DevLog($"view.name: {view.name}");

        }

        private void ShowSceneInfoPanel(VisualElement sceneInfo, MightyCoreData.SceneData scene)
        {
            DevLog("Left-clicked on scene node");
            view.style.borderBottomColor = view.style.borderLeftColor = view.style.borderRightColor = view.style.borderTopColor = Color.white;
            sceneInfo.Clear();
            sceneInfo.style.display = DisplayStyle.Flex;
            VisualElement loadButtons = new()
            {
                name = "LoadButtons",
                style = {
                        flexDirection = FlexDirection.Row,
                    }
            };


            Button loadScene = new()
            {
                name = "LoadScene",
                text = "Load Scene",
                style ={
                        width = 100,
                        backgroundColor = Color.black,
                        color = Color.white,
                    }
            };

            loadScene.clicked += () =>
            {
                DevLog("Clicked on load buttons");

                DevLog("Loading scene");
                EditorSceneManager.OpenScene(scene.ScenePath, OpenSceneMode.Single);
            };

            loadButtons.Add(loadScene);

            Button loadSceneAdd = new()
            {
                name = "LoadScene",
                text = "Add To Scene",
                style = {
                        width = 100,
                        backgroundColor = Color.black,
                        color = Color.white,
                    }
            };

            loadSceneAdd.clicked += () =>
            {
                DevLog("Clicked on load buttons");

                DevLog("Loading scene");
                EditorSceneManager.OpenScene(scene.ScenePath, OpenSceneMode.Additive);
            };

            loadButtons.Add(loadSceneAdd);

            sceneInfo.Add(loadButtons);

            VisualElement tabContainer = new()
            {
                name = "TabContainer",
                style = {
                flexGrow = 1,
                flexShrink = 0,
            }
            };

            ScrollView tabScrollView = new()
            {
                name = "TabScrollView",
                verticalScrollerVisibility = ScrollerVisibility.Hidden,
                style = {
                        flexGrow = 0,
                        //flexShrink = 1,
                        //maxWidth = 300,
                        //width=Length.Percent(100),
                        flexDirection = FlexDirection.Row,
                        overflow = Overflow.Hidden,
                        maxHeight = 64
                    }
            };
            sceneInfo.Add(tabScrollView);

            VisualElement tabBar = new()
            {
                name = "TabBar",
                style = {
                        flexDirection = FlexDirection.Row,
                        flexGrow = 0,
                        flexShrink = 1,
                    }
            };
            tabScrollView.Add(tabBar);


            VisualElement tabContentArea = new()
            {
                name = "TabContentArea",
                style = {
                        flexDirection = FlexDirection.Row,
                        flexGrow = 1,
                        flexShrink = 0,
                    }
            };
            tabContainer.Add(tabContentArea);

            Button sceneData = new()
            {
                name = "sceneData",
                text = "Scene Data",
                style = {
                        // height = 42,
                        // width = 42,
                        backgroundColor = Color.white,
                        color = Color.black,
                    }
            };

            VisualElement sceneDataContent = new()
            {
                name = "SceneDataContent",
                style = {
                        flexDirection = FlexDirection.Column,
                        flexGrow = 1,
                        flexShrink = 0,
                        //height = Length.Percent(100),
                    }
            };

            sceneData.clicked += () =>
    {
        DevLog("Clicked on scene data tab");
        state.selectedTab = 1;
        foreach (VisualElement child in tabContentArea.Children())
        {
            child.style.display = DisplayStyle.None;
        }
        sceneDataContent.style.display = DisplayStyle.Flex;

        tabContentArea.Add(sceneDataContent);
    };
            tabBar.Add(sceneData);

            ScrollView sceneDataContentObjects = new ScrollView()
            {
                name = "sceneDataContentObjects",
                style =
            {
                flexDirection = FlexDirection.Column,
                flexGrow = 1,
                flexShrink = 1,
                //height = 200,
                width = Length.Percent(100),
                overflow = Overflow.Hidden,
            },
            };
            List<SceneData.GameObjectData> results = scene.SearchCollectedData("", currentSearchType, isCaseSensitive);
            sceneDataContentObjects.verticalScroller.slider.RegisterValueChangedCallback(e => OnScrollChanged(e.newValue));

            ToolbarPopupSearchField searchField = new()
            {
                name = "SearchField",
                style =
            {
                flexGrow = 0,
                flexShrink = 0,
                width = 200,
            }
            };

            // Add search type options
            searchField.menu.AppendAction("Name Search", (a) =>
            {
                currentSearchType = SearchType.Name;
                DevLog("Changed to Name Search.");
                results = scene.SearchCollectedData(searchQuery, currentSearchType, isCaseSensitive);
                sceneDataContentObjects.Clear();
                itemsLoaded = 0;
                LoadMoreData();
            }, (a) => currentSearchType == SearchType.Name ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

            searchField.menu.AppendAction("Deep Search", (a) =>
            {
                currentSearchType = SearchType.Deep;
                DevLog("Changed to Deep Search.");
                results = scene.SearchCollectedData(searchQuery, currentSearchType, isCaseSensitive);
                sceneDataContentObjects.Clear();
                itemsLoaded = 0;
                sceneDataContentObjects.verticalScroller.slider.value = 0;
                LoadMoreData();
            }, (a) => currentSearchType == SearchType.Deep ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

            // Add a separator
            searchField.menu.AppendSeparator();

            // Add case-sensitive toggle
            searchField.menu.AppendAction("Case Sensitive", (a) =>
            {
                isCaseSensitive = !isCaseSensitive;
                DevLog($"Case sensitivity is now {(isCaseSensitive ? "enabled" : "disabled")}.");
                results = scene.SearchCollectedData(searchQuery, currentSearchType, isCaseSensitive);
                sceneDataContentObjects.Clear();
                itemsLoaded = 0;
                sceneDataContentObjects.verticalScroller.slider.value = 0;
                LoadMoreData();
            }, (a) => isCaseSensitive ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

            Label sceneDataObjectInfo = new Label()
            {
                name = "sceneDataObjectInfo",
                text = $"Showing {itemsLoaded} of {results.Count} objects",
                style =
            {
                flexGrow = 0,
                flexShrink = 0,
                height = 20,
            }
            };

            searchField.RegisterValueChangedCallback((evt) =>
                    {
                        searchQuery = evt.newValue;
                        DevLog($"Search string changed to {searchQuery}.");

                        results = scene.SearchCollectedData(searchQuery, currentSearchType, isCaseSensitive);
                        sceneDataContentObjects.Clear();
                        itemsLoaded = 0;
                        sceneDataContentObjects.verticalScroller.slider.value = 0;
                        sceneDataObjectInfo.text = $"{results.Count} objects found!";
                        LoadMoreData();
                    });
            sceneDataContent.Add(searchField);

            sceneDataContent.Add(sceneDataContentObjects);


            sceneDataContent.Add(sceneDataObjectInfo);

            tabContentArea.RegisterCallback<GeometryChangedEvent>(e =>
            {
                DevLog($"sceneDataContentObjects geometry changed {e.newRect} {e.oldRect}");
                sceneDataContentObjects.style.height = sceneInfo.resolvedStyle.height - 96 - 64;

            });

            itemsLoaded = 0;
            LoadMoreData();

            void LoadMoreData()
            {
                // Check if we have already loaded all the items to prevent loading past the max.
                if (itemsLoaded >= results.Count)
                {
                    // Optionally, disable the event or UI element that triggers loading more data.
                    // For example:
                    // loadMoreButton.SetEnabled(false);
                    // or hide the scrollbar if necessary
                    sceneDataContentObjects.verticalScroller.SetEnabled(false);

                    return; // Exit the method as there's nothing more to load.
                }

                var toLoad = results.Skip(itemsLoaded).Take(itemsPerLoad);
                int i = itemsLoaded; // Start numbering from the current count of items loaded
                foreach (var gameObjectData in toLoad)
                {
                    VisualElement gameObjectElement = CreateGameObjectElement(gameObjectData);
                    Label text = new Label()
                    {
                        name = "text",
                        text = $"{i}: {gameObjectData.Name}",
                        style =
                    {
                        unityTextAlign = TextAnchor.MiddleLeft,
                        fontSize = 12,
                        color = new Color(1, 1, 1, 1),
                        paddingBottom = 2,
                        paddingLeft = 2,
                        paddingRight = 2,
                        paddingTop = 2,
                    }
                    };
                    // sceneDataContentObjects.Add(text);
                    sceneDataContentObjects.Add(gameObjectElement);
                    i++;
                }
                itemsLoaded += toLoad.Count(); // Only increment by the number of items actually loaded
            }


            void OnScrollChanged(float newValue)
            {
                // Check if the scroll is near the bottom
                if (sceneDataContentObjects.verticalScroller.highValue - newValue < 10)
                {
                    DevLog($"Loading more data... itemsLoaded: {itemsLoaded} results.Count: {results.Count} ");
                    // Load more data if there are still items left to load
                    if (itemsLoaded < results.Count)
                    {
                        LoadMoreData();
                    }
                }
            }



            VisualElement CreateGameObjectElement(SceneData.GameObjectData gameObjectData)
            {
                Color backgroundColor = StringToColor(gameObjectData.Layer, 0.4f);

                VisualElement gameObjectElement = new VisualElement()
                {
                    name = "GameObjectElement",
                    style =
        {
            flexDirection = FlexDirection.Row,
            backgroundColor = backgroundColor,
                        paddingBottom = 5,
            paddingLeft = 5,
            paddingRight = 5,
            paddingTop = 5,
            marginBottom = 5,
            marginLeft = 5,
            marginRight = 5,
            marginTop = 5,
        }
                };

                VisualElement infoContainer = new VisualElement()
                {
                    style =
        {
            flexDirection = FlexDirection.Column,
            flexGrow = 1,
        }
                };
                gameObjectElement.Add(infoContainer);

                Label nameLabel = new Label(gameObjectData.Name)
                {
                    style =
        {
            unityTextAlign = TextAnchor.MiddleLeft,
            fontSize = 14,
            unityFontStyleAndWeight = FontStyle.Bold,
            color = new Color(1, 1, 1, 1),
            flexGrow = 1,
        }
                };
                infoContainer.Add(nameLabel);

                Label tagLayerLabel = new Label($"Tag: {gameObjectData.Tag}, Layer: {gameObjectData.Layer}")
                {
                    style =
        {
            unityTextAlign = TextAnchor.MiddleLeft,
            fontSize = 12,
            color = new Color(1, 1, 1, 1),
            paddingBottom = 2,
            paddingLeft = 2,
            paddingRight = 2,
            paddingTop = 2,
        }
                };
                infoContainer.Add(tagLayerLabel);

                // Flags on the right side, absolutely positioned
                VisualElement flagsContainer = new VisualElement()
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        position = Position.Absolute,
                        color = new Color(1, 1, 1, 1),
                        top = 5,
                        right = 5,
                    }
                };

                flagsContainer.Add(CreateFlagIcon(gameObjectData.IsPrefab, "Prefab"));
                flagsContainer.Add(CreateFlagIcon(gameObjectData.IsStatic, "Static"));
                flagsContainer.Add(CreateFlagIcon(gameObjectData.IsActive, "Active"));

                gameObjectElement.Add(flagsContainer);

                // Components collapsible section
                Foldout componentsFoldout = new Foldout()
                {
                    text = $"[{gameObjectData.Components.Count}] Components",
                    value = false,
                    style =
                    {
                        borderTopWidth = 1,
                        borderTopColor = Color.gray,
                        color = new Color(1, 1, 1, 1),
                        marginTop = 5,
                    }
                };
                foreach (var component in gameObjectData.Components)
                {
                    Label componentLabel = new Label(component.TypeName)
                    {
                        style =
                        {
                            unityTextAlign = TextAnchor.MiddleLeft,
                            fontSize = 12,
                            color = new Color(1, 1, 1, 1),
                            borderBottomWidth = 1,
                            borderBottomColor = Color.gray,
                            paddingBottom = 2,
                            paddingLeft = 2,
                            paddingRight = 2,
                            paddingTop = 2,
                        }
                    };
                    foreach (var property in component.Properties)
                    {
                        string propertyString = $"{property.Key}: {property.Value} ({property.Type})";
                        componentLabel.tooltip += propertyString + "\n";
                    }
                    infoContainer.Add(componentsFoldout);

                    componentsFoldout.Add(componentLabel);
                }
                infoContainer.Add(componentsFoldout);

                return gameObjectElement;
            }

            VisualElement CreateFlagIcon(bool flagValue, string name)
            {
                Label flagIcon = new() //new Label(flagValue ? "[X]" : "[ ]")
                {
                    name = "FlagIcon",
                    text = name,
                    style =
                    {
                        unityTextAlign = TextAnchor.MiddleCenter,
backgroundColor = flagValue ? StringToColor(name,0.4f) : StringToColor(name,0.1f),
                        fontSize = 9,
                        color = new Color(1, 1, 1, 1),
                        paddingBottom = 2,
                        paddingLeft = 2,
                        paddingRight = 2,
                        paddingTop = 2,
                        marginLeft = 2,
                        marginRight = 2,
                        borderTopLeftRadius = 8,
                        borderTopRightRadius = 8,
                        borderBottomLeftRadius = 8,
                        borderBottomRightRadius = 8,
                    }
                };
                return flagIcon;
            }


            Button sceneDossier = new()
            {
                name = "sceneData",
                text = "Details",
                style = {
                        // height = 42,
                        // width = 42,
                        backgroundColor = Color.white,
                        color = Color.black,
                    }
            };



            VisualElement sceneDossierContent = new()
            {
                name = "SceneDataContent",
                style = {
                        flexDirection = FlexDirection.Row,
                        flexGrow = 1,
                        flexShrink = 0,
                    }
            };

            sceneDossier.clicked += () =>
            {
                DevLog("Clicked on scene dossier tab");
                state.selectedTab = 2;
                foreach (VisualElement child in tabContentArea.Children())
                {
                    child.style.display = DisplayStyle.None;
                }
                sceneDossierContent.style.display = DisplayStyle.Flex;

                tabContentArea.Add(sceneDossierContent);
            };

            //tabBar.Add(sceneDossier);

            ScrollView sceneDossierProjectLevel = new()
            {
                name = "SceneDossierProjectLevel",
                style = {
                        flexDirection = FlexDirection.Column,
                        flexGrow = 1,
                        flexShrink = 0,

                    },

            };
            sceneDossierContent.Add(sceneDossierProjectLevel);

            Label projectNameLabel = new()
            {
                name = "ProjectNameLabel",
                text = "Project Name",
                style = {
                        flexGrow = 0,
                        flexShrink = 0,
                        fontSize = 20,
                        unityFontStyleAndWeight = FontStyle.Bold,
                    }
            };
            sceneDossierProjectLevel.Add(projectNameLabel);

            TextField projectName = new()
            {
                name = "ProjectName",
                value = MightyCoreData.ProjectDossier.name,
                style = {
                        flexGrow = 0,
                        flexShrink = 0,
                    }
            };
            sceneDossierProjectLevel.Add(projectName);

            // static ProjectDossier()
            // {
            //     name = "Space Blasters 3000";
            //     genre = "3D 3rd Person Shooter";
            //     description = "Lighthearted 3rd person shooter with a retro feel, platformer elements, puzzle solving, and a humorous story.";
            //     plot = "Becky is a space cadet who must save the universe from the evil space aliens.  Her spaceship crashed so she must salvage and explore so that she can repair her ship and get back to Earth.  However, the evil space aliens have taken over the planet and are trying to stop her.  She must fight her way through the aliens and their minions to get to the mothership and destroy it.  Once the mothership is destroyed, she can repair her ship and get back to Earth.";
            //     platforms = "PC, Android, iOS";
            //     scenes = new List<Scenes>
            //         {
            //             new Scenes { name = "Space Station", description = "Becky's ship has crashed on an abandoned space station.  She must explore the station to find the parts she needs to repair her ship." },
            //             new Scenes { name = "Slime Caves", description = "Becky has found the parts she needs to repair her ship, but the evil space aliens have taken over the planet and are trying to stop her.  She must fight her way through the aliens and their minions to get to the mothership and destroy it." },
            //             new Scenes { name = "Mothership", description = "Becky has destroyed the mothership and can now repair her ship and get back to Earth." },
            //             new Scenes { name = "Earth", description = "Becky has returned to Earth and is hailed as a hero." }
            //         };
            // }


            int i = 2;
            foreach (var typeInfo in MightyCore.data.MappableTypesInfo)
            {
                int ii = i;
                //Button typeTab = typeInfo.Mappable.AddMappable(false);
                Button typeTab = new()
                {
                    name = typeInfo.Name,
                    text = typeInfo.Name,
                    style = {
                        // height = 42,
                        // width = 42,
                        backgroundColor = Color.white,
                        color = Color.black,
                    }
                };

                string localName = typeInfo.Name;  // Create a local variable to capture

                static void ShowContent(SceneData scene, VisualElement tabContentArea, MappableTypeInfo typeInfo)
                {
                    foreach (VisualElement child in tabContentArea.Children())
                    {
                        child.style.display = DisplayStyle.None;
                    }
                    VisualElement content = typeInfo.Mappable.SceneSummary(scene);

                    if (content != null)
                    {
                        content.style.display = DisplayStyle.Flex;
                        tabContentArea.Add(content);
                    }
                }

                typeTab.clicked += () =>
                {
                    DevLog($"Clicked on tab {localName} ii: {ii}");
                    state.selectedTab = ii;
                    ShowContent(scene, tabContentArea, typeInfo);

                };
                DevLog($"state.selectedTab: {state.selectedTab} i: {i}");
                if (i == state.selectedTab)
                {
                    ShowContent(scene, tabContentArea, typeInfo);
                }

                tabBar.Add(typeTab);
                i++;
            }

            sceneInfo.Add(tabContainer);
            sceneInfo.style.height = Length.Percent(100);
        }

    }
}