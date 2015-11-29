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
            return AtLeastOneTextureIsSelected ();
        }

        private SpriteSettings currentSelectedSettings;
        private int selectedSettingIndex;
        private bool showSettings;
        private bool changePackingTag;
        private SpriteSettingsConfig config;
        private Vector2 bodyScrollPos;
        SpriteSettingsConfigWindow configWindow;
        
        public SpriteFileSettings FileSettings;
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
            
            FileSettings = LoadFileSettingsForObject (Selection.activeObject);
            slicingOptionsLoaded = true;
        }
        
        SpriteFileSettings LoadFileSettingsForObject (Object obj)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            return SpriteSettingsUtility.GetFileSettings(path, currentSelectedSettings.SpritesheetDataFile);
        }
        
        static bool IsObjectValidTexture (Object obj)
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
            EditorGUILayout.Space ();
            bodyScrollPos = EditorGUILayout.BeginScrollView(bodyScrollPos);

            if (!slicingOptionsLoaded) {
                LoadSlicingOptions ();
            }
            DrawFileSpecificSettings ();

            EditorGUILayout.Space();
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        
        void DrawNoSelection ()
        {
            EditorGUILayout.HelpBox ("No texture is selected. Select a texture to apply saved settings.", MessageType.Warning);
        }
        
        static bool AtLeastOneTextureIsSelected ()
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
                        // Load currently set fileSettings for multi-select. Otherwise use saved fileSettings
                        SpriteFileSettings settings = Selection.objects.Length == 1 ? 
                            FileSettings : LoadFileSettingsForObject (obj);
                        if (!settings.SlicingOptions.IsValid ()) {
                            Debug.LogWarning ("Skipping ApplyingTextureSettings to object due to invalid "
                                 + "Slicing Options. Object: " + obj.name);
                            continue;
                        }
                        string path = AssetDatabase.GetAssetPath (obj);
                        var selectedTexture = AssetDatabase.LoadAssetAtPath <Texture2D> (path);
                        SpriteSettingsUtility.ApplyDefaultTextureSettings(selectedTexture,
                            currentSelectedSettings, settings);
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

        void DrawFileSpecificSettings ()
        {
            if (Selection.objects.Length > 1)
            {
                string slicingInfo = "";
                foreach (var obj in Selection.objects)
                {
                    if (IsObjectValidTexture (obj))
                    {
                        FileSettings = LoadFileSettingsForObject (obj);
                        slicingInfo += "\n" + obj.name + ": " + FileSettings.SlicingOptions.ToDisplayString ();
                    }
                }
                EditorGUILayout.HelpBox ("File Settings do not support Multiple-Object editing. "
                    + " The following settings will be used:\n" + slicingInfo, MessageType.Info);
                return;
            }
            
            DrawSlicingOptions ();
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField ("Override Settings", EditorStyles.boldLabel);
            if (FileSettings.SlicingOptions.ImportMode != SpriteImportMode.None)
            {
                DrawPivotOverride ();
            }
            DrawPackingTagOverride ();
        }
        
        void DrawPackingTagOverride ()
        {
            FileSettings.OverridePackingTag = EditorGUILayout.BeginToggleGroup("Override Packing Tag", 
                FileSettings.OverridePackingTag);

            var tagToUse = FileSettings.OverridePackingTag ? 
                FileSettings.PackingTag : currentSelectedSettings.PackingTag;
            FileSettings.PackingTag = EditorGUILayout.TextField(tagToUse);

            if (FileSettings.OverridePackingTag)
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
            
            FileSettings.SlicingOptions.ImportMode = (SpriteImportMode) EditorGUILayout.EnumPopup 
                ("Sprite Import Mode", FileSettings.SlicingOptions.ImportMode);
                
            EditorGUILayout.Space();
            
            if (FileSettings.SlicingOptions.ImportMode == SpriteImportMode.Multiple)
            {
                FileSettings.SlicingOptions.GridSlicing = (SpriteSlicingOptions.GridSlicingMethod) 
                EditorGUILayout.EnumPopup ("Grid Slicing Method", FileSettings.SlicingOptions.GridSlicing);
            
                if (FileSettings.SlicingOptions.GridSlicing == SpriteSlicingOptions.GridSlicingMethod.Bogdan)
                {
                    FileSettings.SlicingOptions.Frames = EditorGUILayout.IntField ("Frames",
                        FileSettings.SlicingOptions.Frames);
                }
                
                FileSettings.SlicingOptions.CellSize = EditorGUILayout.Vector2Field ("Cell Size (X/Y)",
                    FileSettings.SlicingOptions.CellSize);
                
                EditorGUILayout.Space();
            }
        }
        
        void DrawPivotOverride ()
        {
            FileSettings.SlicingOptions.OverridePivot = EditorGUILayout.BeginToggleGroup("Override Pivot", 
                FileSettings.SlicingOptions.OverridePivot);
            var pivotToUse = FileSettings.SlicingOptions.OverridePivot ? 
                FileSettings.SlicingOptions.Pivot : currentSelectedSettings.Pivot;
            FileSettings.SlicingOptions.Pivot = (SpriteAlignment)EditorGUILayout.EnumPopup(pivotToUse);

            bool showCustomPivot = FileSettings.SlicingOptions.Pivot == SpriteAlignment.Custom;
            EditorGUI.BeginDisabledGroup (!showCustomPivot);
            var customPivotToUse = FileSettings.SlicingOptions.OverridePivot ? 
                FileSettings.SlicingOptions.CustomPivot : currentSelectedSettings.CustomPivot;
            FileSettings.SlicingOptions.CustomPivot = EditorGUILayout.Vector2Field("Custom Pivot", customPivotToUse);
            EditorGUI.EndDisabledGroup ();
            EditorGUILayout.EndToggleGroup();
        }
    }
}