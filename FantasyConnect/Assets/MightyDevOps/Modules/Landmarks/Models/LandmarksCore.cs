using System;
using System.Collections.Generic;
using Mighty;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static Mighty.MightyCoreData;

namespace MightyLandmarks
{
    [InitializeOnLoad]
    [ExecuteInEditMode]
    public class LandmarksCore
    {
        static public MightyCoreData core;
        static public LandmarksData data;
        static public LandmarksData.Scene sceneData;
        static public int sceneIndex = -1;
        static bool isStarted = false;

        static LandmarksCore()
        {
            DevLog("LandmarkCore");

            StartModules -= StartModule;
            StartModules += StartModule;
        }

        static void StartModule()
        {
            DevLog($"LandmarkCore Starting Module with {sceneAnchor.DataSetName}");

            if (isStarted == false)
            {
                core = MightyCoreData.Load();
                data = LandmarksData.Load();
                isStarted = true;
            }

            EditorSceneManager.sceneOpened -= GetSceneData;
            EditorSceneManager.sceneOpened += GetSceneData;

            EditorSceneManager.sceneClosing -= SceneClosing;
            EditorSceneManager.sceneClosing += SceneClosing;

            EditorApplication.wantsToQuit -= WantsToQuit;
            EditorApplication.wantsToQuit += WantsToQuit;


            //if (sceneData == null) 
            GetSceneData();
        }

        private static bool WantsToQuit()
        {
            DevLog("WantsToQuit");
            if (data == null) return true;
            DevLog($"WantsToQuit: {data.scenes.Count}");
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            // EditorUtility.DisplayDialog("Landmarks", "Landmarks data saved", "OK");
            return true;
        }

        private static void SceneClosing(UnityEngine.SceneManagement.Scene scene, bool removingScene)
        {
            DevLog($"SceneClosing({scene.name}, {removingScene})");
            if (data == null) return;
            DevLog($"SceneClosing: {data.scenes.Count}");
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
        }

        private static void GetSceneData(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
        {
            DevLog($"Landmarks GetSceneData({scene.name}, {mode})");
            GetSceneData();
        }

        public static void GetSceneData()
        {
            DevLog("LandmarksCore.GetSceneData");
            core ??= MightyCoreData.Load();
            if (core == null) return;

            core.CheckSceneData();
            data ??= LandmarksData.Load();
            data.scenes ??= new();

            if (!isSceneAnchored) return;

            sceneIndex = -1;
            for (int i = 0; i < data.scenes.Count; i++)
            {
                DevLog($"Scene {i} is {data.scenes[i].name}");
                DevLog($"Scene {i} is {sceneAnchor}");
                DevLog($"Scene {i} is {sceneAnchor.DataSetName}");
                if (data.scenes[i].name == sceneAnchor.DataSetName) sceneIndex = i;
            }


            if (sceneIndex < 0)
            {
                DevLog("SceneIndex not found");
                data.scenes.Add(new LandmarksData.Scene());
                sceneIndex = data.scenes.Count - 1;
                data.scenes[sceneIndex].name = sceneAnchor.DataSetName;
                data.scenes[sceneIndex].landmarks = new List<LandmarksData.Landmark.Root>();
            }
            DevLog($"SceneIndex is {sceneIndex}");
            sceneData = data.scenes[sceneIndex];
            for (int i = 0; i < sceneData.landmarks.Count; i++)
            {
                sceneData.landmarks[i].RegisterMappable();
                // mappables.Add(data.landmarks[i]);
            }
        }
    }
}
