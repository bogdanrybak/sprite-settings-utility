using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Staple.EditorScripts
{
    public class SpriteSettingsWindow : EditorWindow
    {
        [MenuItem("Assets/Quick Sprite Settings")]
        public static void Init()
        {
            EditorWindow.GetWindow<SpriteSettingsWindow>("Sprite Settings", true);
        }

        [MenuItem("Assets/Quick Sprite Settings", true)]
        public static bool TextureSelected()
        {
            foreach (var obj in Selection.objects)
            {
                if (!AssetDatabase.Contains(obj)) continue;

                var asset =
                    AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(obj), typeof(Texture2D)) as Texture2D;

                if (asset != null)
                    return true;
            }

            return false;
        }

        private SpriteSettings currentSelectedSettings;
        private int selectedSettingIndex;
        private bool showSettings;
        private bool changePivot = true;
        private bool changePackingTag;
        private SpriteSettingsConfig config;
        private Vector2 bodyScrollPos;
        SpriteSettingsConfigWindow configWindow;
        SpriteSlicingOptions slicingOptions;
        bool slicingOptionsLoaded = false;

        void OnEnable()
        {
            config = AssetDatabase.LoadAssetAtPath(GetPathToConfig (), typeof(SpriteSettingsConfig)) as SpriteSettingsConfig;
            LoadSlicingOptions ();
            Selection.selectionChanged += HandleSelectionChanged;
        }
        
        void OnDisable()
        {
            Selection.selectionChanged -= HandleSelectionChanged;
        }
        
        void HandleSelectionChanged ()
        {
            LoadSlicingOptions ();
        }
        
        void LoadSlicingOptions ()
        {
            if (currentSelectedSettings == null) 
            {
                return;
            }
            
            if (Selection.objects.Length == 0)
            {
                return;
            }
            
            // No need to load with multiple selections without multiobject support
            if (Selection.objects.Length > 1)
            {
                return;
            }
            
            if (!IsObjectValidTexture (Selection.activeObject))
            {
                return;
            }
            
            slicingOptions = LoadSlicingOptionForObject (Selection.activeObject);
            slicingOptionsLoaded = true;
        }
        
        SpriteSlicingOptions LoadSlicingOptionForObject (Object obj)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            return SpriteSettingsUtility.GetSlicingOptions(path, currentSelectedSettings.SpritesheetDataFile);
        }
        
        bool IsObjectValidTexture (Object obj)
        {
            if (!AssetDatabase.Contains(obj))
            {
                return false;
            }
            
            string path = AssetDatabase.GetAssetPath(obj);
            var asset = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
            if (asset == null)
            {
                return false;
            }
            
            return true;
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        void OnGUI()
        {
            if (config == null || config.SettingsSets == null || config.SettingsSets.Count == 0) {
                DrawEmptySaveSettings ();
                return;
            }
            
            if (Selection.objects.Length == 0)
            {
                DrawNoSelection ();
                return;
            }
            
            if (!AtLeastOneTextureIsSelected ())
            {
                DrawSelectionNotTexture ();
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.inspectorFullWidthMargins);
            EditorGUILayout.Space();

            DrawApplyButton();

            EditorGUILayout.Space();
            
            DrawSaveSettingSelect ();
            
            EditorGUILayout.Space ();

            DrawQuickSettings();

            EditorGUILayout.Space();

            bodyScrollPos = EditorGUILayout.BeginScrollView(bodyScrollPos);
            
            if (!slicingOptionsLoaded) {
                LoadSlicingOptions ();
            }
            DrawSlicingOptions ();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        
        void DrawNoSelection ()
        {
            EditorGUILayout.HelpBox ("No texture is selected. Select a texture to apply saved settings.", MessageType.Warning);
        }
        
        bool AtLeastOneTextureIsSelected ()
        {
            foreach (var obj in Selection.objects)
            {
                if (IsObjectValidTexture (obj))
                {
                    return true;
                }
            }
            return false;
        }
        void DrawSelectionNotTexture ()
        {
            EditorGUILayout.HelpBox ("None of the selected objects are textures. Select at least one texture" + 
                "to apply saved settings.", MessageType.Warning);
        }

        void DrawApplyButton()
        {
            Color defaultBg = GUI.backgroundColor;
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Apply"))
            {
                foreach (var obj in Selection.objects)
                {
                    if (IsObjectValidTexture (obj))
                    {
                        // Load existing options for multi-select. Otherwise use recent settings
                        SpriteSlicingOptions options = Selection.objects.Length == 1 ? slicingOptions :
                            LoadSlicingOptionForObject (obj);
                        if (!options.IsValid ()) {
                            Debug.LogWarning ("Skipping ApplyingTextureSettings to object due to invalid "
                                 + "Slicing Options. Object: " + obj.name);
                            continue;
                        }
                        SpriteSettingsUtility.ApplyDefaultTextureSettings((Texture2D) obj,
                            currentSelectedSettings, options, changePivot, changePackingTag);
                    }
                }
                
                if (currentSelectedSettings.PackOnApply)
                {
                    UnityEditor.Sprites.Packer.RebuildAtlasCacheIfNeeded(
                    EditorUserBuildSettings.activeBuildTarget, true);
                }
                
                Close();
                if (configWindow != null) 
                {
                    configWindow.Close ();
                }
            }
            GUI.backgroundColor = defaultBg;
        }
        
        void DrawEmptySaveSettings ()
        {
            EditorGUILayout.Space ();
            EditorGUILayout.LabelField ("Create a Saved SpriteSetting to start applying SpriteSettings.");
            if (GUILayout.Button ("Create Setting")) {
                if (config == null) {
                    CreateConfig ();
                }
                
                ShowConfigWindow (0);
                
                if (config.SettingsSets.Count == 0) {
                    config.AddDefaultSpriteSetting ();
                }
            }
            EditorGUILayout.Space ();
        }
        
        void CreateConfig ()
        {
            config = ScriptableObjectUtility.CreateAssetAtPath<SpriteSettingsConfig>(GetPathToConfig ());
        }
        
        string GetPathToConfig ()
        {
            MonoScript script = MonoScript.FromScriptableObject (this);
            string scriptPath = AssetDatabase.GetAssetPath (script);
            string scriptDirectory = System.IO.Path.GetDirectoryName (scriptPath);
            string filename = SpriteSettingsConfig.DefaultFilename;
            return scriptDirectory + System.IO.Path.DirectorySeparatorChar + filename;
        }
        
        void ShowConfigWindow (int indexToFocus)
        {
            configWindow = EditorWindow.GetWindow<SpriteSettingsConfigWindow>("Saved SpriteSettings", true);
            configWindow.SetConfig (config);
            configWindow.SelectSetting (indexToFocus);
        }
        
        void DrawSaveSettingSelect ()
        {
            string[] savedSetNames = new string[config.SettingsSets.Count];
            int[] savedSetIndeces = new int[savedSetNames.Length];
            for (int i = 0; i < savedSetNames.Length; i++) {
                savedSetNames[i] = config.SettingsSets[i].Name;
                savedSetIndeces[i] = i;
            }
            // Make sure a previously selected index still exists
            if (selectedSettingIndex >= savedSetNames.Length) {
                selectedSettingIndex = savedSetNames.Length -1;
            }
            EditorGUILayout.BeginHorizontal ();
            selectedSettingIndex = EditorGUILayout.IntPopup ("Setting to Apply", selectedSettingIndex, 
                                                        savedSetNames, savedSetIndeces);
            currentSelectedSettings = config.SettingsSets [selectedSettingIndex];
            if (GUILayout.Button ("Edit", GUILayout.MaxWidth(80.0f))) {
                ShowConfigWindow (selectedSettingIndex);
            }
            EditorGUILayout.EndHorizontal ();
        }

        void DrawQuickSettings()
        {
            changePivot = EditorGUILayout.BeginToggleGroup("Apply Pivot", changePivot);

            currentSelectedSettings.Pivot = (SpriteAlignment)EditorGUILayout.EnumPopup(currentSelectedSettings.Pivot);

            if (currentSelectedSettings.Pivot == SpriteAlignment.Custom)
                currentSelectedSettings.CustomPivot = EditorGUILayout.Vector2Field("Custom Pivot", currentSelectedSettings.CustomPivot);

            EditorGUILayout.EndToggleGroup();

            EditorGUILayout.Space();

            changePackingTag = EditorGUILayout.BeginToggleGroup("Apply Packing Tag", changePackingTag);

            currentSelectedSettings.PackingTag = EditorGUILayout.TextField(currentSelectedSettings.PackingTag);

            if (changePackingTag)
            {
                foreach (var name in UnityEditor.Sprites.Packer.atlasNames)
                {
                    if (GUILayout.Button(name, EditorStyles.miniButton))
                    {
                        currentSelectedSettings.PackingTag = name;
                        EditorGUI.FocusTextInControl(name);
                    }
                }
            }
            
            EditorGUILayout.EndToggleGroup();
        }
        
        void DrawSlicingOptions ()
        {
            EditorGUILayout.LabelField ("Slicing Options", EditorStyles.boldLabel);
            if (Selection.objects.Length > 1)
            {
                string slicingInfo = "";
                foreach (var obj in Selection.objects)
                {
                    if (IsObjectValidTexture (obj))
                    {
                        slicingOptions = LoadSlicingOptionForObject (obj);
                        slicingInfo += "\n" + obj.name + ": " + slicingOptions.ToDisplayString ();
                    }
                }
                EditorGUILayout.HelpBox ("Slicing Options does not support Multiple-Object editing. "
                    + " The following settings will be used:\n" + slicingInfo, MessageType.Info);
                return;
            }
            
            slicingOptions.ImportMode = (SpriteImportMode) EditorGUILayout.EnumPopup 
                ("Sprite Import Mode", slicingOptions.ImportMode);
            
            if (slicingOptions.ImportMode != SpriteImportMode.Multiple)
            {
                return;
            }
            
            slicingOptions.GridSlicing = (SpriteSlicingOptions.GridSlicingMethod) 
                EditorGUILayout.EnumPopup ("Grid Slicing Method", slicingOptions.GridSlicing);
            
            slicingOptions.CellSize = EditorGUILayout.Vector2Field ("Cell Size (X/Y)", slicingOptions.CellSize);
            
            slicingOptions.Pivot = (SpriteAlignment) EditorGUILayout.EnumPopup ("Pivot", slicingOptions.Pivot);
            
            bool enableCustomPivot = slicingOptions.Pivot == SpriteAlignment.Custom;
            EditorGUI.BeginDisabledGroup (!enableCustomPivot);
            slicingOptions.CustomPivot = EditorGUILayout.Vector2Field ("Custom Pivot", slicingOptions.CustomPivot);
            EditorGUI.EndDisabledGroup ();
            
            slicingOptions.Frames = EditorGUILayout.IntField ("Frames", slicingOptions.Frames);
        }
    }
}