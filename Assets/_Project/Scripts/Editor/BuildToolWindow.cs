using System.IO;
using Scripts.Runtime.Configs;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NetPhys.EditorTools
{
    public class BuildToolWindow : EditorWindow
    {
        private const string AppIdFileName = "steam_appid.txt";
        private const string ConnectionConfigFileName = "connection_config.json";
        private const string DefaultAppId = "480";

        [MenuItem("NetPhys/Build Tool")]
        public static void Open()
        {
            GetWindow<BuildToolWindow>("Build Tool").Show();
        }

        [SerializeField] private string _buildDirectory = "Builds/Standalone";
        [SerializeField] private string _appId = DefaultAppId;

        private void OnEnable()
        {
            string projectAppIdPath = Path.Combine(Directory.GetCurrentDirectory(), AppIdFileName);
            if (File.Exists(projectAppIdPath))
                _appId = File.ReadAllText(projectAppIdPath).Trim();
        }

        private void OnGUI()
        {
            _buildDirectory = EditorGUILayout.TextField("Build directory", _buildDirectory);
            _appId = EditorGUILayout.TextField("Steam App ID", _appId);

            GUILayout.Space(10);

            using (new EditorGUI.DisabledScope(!uint.TryParse(_appId, out _)))
            {
                if (GUILayout.Button("Build"))
                    Build();
            }
        }

        private void Build()
        {
            string buildDirectory = Path.Combine(Directory.GetCurrentDirectory(), _buildDirectory);
            string exeName = $"{PlayerSettings.productName}.exe";
            string exePath = Path.Combine(buildDirectory, exeName);

            var options = new BuildPlayerOptions
            {
                scenes = GetEnabledScenePaths(),
                locationPathName = exePath,
                target = EditorUserBuildSettings.activeBuildTarget,
                options = BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"[BuildTool] Build failed: {report.summary.result}. See Console for details.");
                return;
            }

            WritePostBuildFiles(buildDirectory);

            EditorUtility.RevealInFinder(exePath);

            Debug.Log($"[BuildTool] Build succeeded: {exePath}");
        }

        private void WritePostBuildFiles(string buildDirectory)
        {
            File.WriteAllText(Path.Combine(buildDirectory, AppIdFileName), _appId.Trim());

            // Serialize the ConnectionConfig asset as-is - the runtime deserializes this file back into ConnectionSettings on bootstrap.
            ConnectionConfig connectionConfig = LoadConnectionConfigAsset();
            string json = JsonUtility.ToJson(connectionConfig.Settings, prettyPrint: true);
            File.WriteAllText(Path.Combine(buildDirectory, ConnectionConfigFileName), json);
        }

        private static ConnectionConfig LoadConnectionConfigAsset()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:ConnectionConfig"))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var config = AssetDatabase.LoadAssetAtPath<ConnectionConfig>(assetPath);

                if (config != null)
                    return config;
            }

            Debug.LogWarning("[BuildTool] No ConnectionConfig asset found, writing default connection settings.");
            return CreateInstance<ConnectionConfig>();
        }

        private static string[] GetEnabledScenePaths()
        {
            var scenes = EditorBuildSettings.scenes;
            var paths = new System.Collections.Generic.List<string>();

            foreach (var scene in scenes)
            {
                if (scene.enabled)
                    paths.Add(scene.path);
            }

            return paths.ToArray();
        }
    }
}
