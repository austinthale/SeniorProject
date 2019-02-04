using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEditorInternal;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace FuzzyTools
{
    #region Enums

    
    public enum ModelFormat
    {
        OBJ//,//TODO
        //FBX
    }
    
    public enum ImageType
    {
        PNG,
        EXR,
        JPG
    }
    
    public enum TopologyMode
    {
        Triangles,
        Quads
    }

    public enum Resolution
    {
        Full,
        Half,
        Quarter,
        Eighth,
        Sixteenth
    }
    
    public enum TerrainPrefabType
    {
        Texture,
        Tree,
        Grass,
        Details,
        Brush
    }
    
    public enum Axis
    {
        X,
        Y,
        Z
    }
    
    public enum CopyMode
    {
        PasteComponentValues,
        AddComponentAsNew
    }
    
    public enum CustomColorType
    {
        Off,
        AutoColors,
        CustomColors,
        Hierarchy,
        VariedColor
    }
    
    #endregion
    
    public class FuzzyTools : EditorWindow
    {
        #region Preferences
        //Group Preferences
        public static string defaultGroupName = "New Group";
        
        //Recent Scenes Preferences
        public static int keepTrackOfRecentScenes = 5;
        //Terrain Splitting Preferences
        public static int maxTerrainSplit = 10;
        //Select and remove Preferences
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
        public static Type[] defaultIgnore = new Type[] {typeof(Component), typeof(MonoBehaviour), typeof(NetworkBehaviour)};
#endif
#if UNITY_2018_3_OR_NEWER
        public static Type[] defaultIgnore = new Type[] {typeof(Component), typeof(MonoBehaviour)};
#endif
        public static List<Type> ignoreTypes = new List<Type>(defaultIgnore);
        //Physics Simulation Preferences
        public static int maxPhysicsIterations = 500;
        //Make Prefab preferences
        public static string subLocation = "Prefabs/";
        //Locator Preferences
        public static string locatorName = "Locator##";
        public static Color locatorColor = Color.blue;
        public static float locatorScale = 1;
        
        //Renaming Prefernces
        public static bool showAllGameObjectsOfName = false;
        public static bool autoAddSpaces = true;
        
        //Converting Preferences
        public static string DefaultImagePath = "Converted/Images/";
        public static string DefaultModelPath = "Converted/Models/";
        public static string DefaultTerrainPath = "Converted/Terrains/";
        public static ModelFormat DefaultModelFormat = ModelFormat.OBJ;//ModelFormat.FBX;
        public static TopologyMode DefaultTopologyMode = TopologyMode.Quads;
        public static Resolution DefaultMeshResolution = Resolution.Eighth;
        

        #region Hierarchy Preferences
        public static CustomColorType colorMode = CustomColorType.Off;
        /*******************AUTO_COLOR_MODE****************************/
        public static Color gameObjectFontColor = Color.black;
        public static Color prefabOrgFontColor = Color.blue;

        public static Color inActiveColor;
        
        public static Color inActiveFontColor = Color.white;
        public static FontStyle standardFont = FontStyle.Normal;
        public static FontStyle prefebFont = FontStyle.Bold;
        public static bool autoInvertColors = true;
        /**************************************************************/
        /**********************************CUSTOM_COLOR_MODE****************************************/
        public static Dictionary<int, Color> iDToBackgroundColor = new Dictionary<int, Color>();
        public static Dictionary<int, Color> iDToFontColor = new Dictionary<int, Color>();
        public static Dictionary<int, FontStyle> iDToFontStyle = new Dictionary<int, FontStyle>();

        public static bool uniformChangeColors = true;
        /*******************************************************************************************/

        /****************HIERARCHY_COLOR_MODE**************************/
        public static Color hierarchyColor1 = new Color(.76f,.76f,.3f);
        public static Color hierarchyColor2 = new Color(.3f,.3f,1);
        public static Color hierarchyColor3 = new Color(1,.3f,.3f);
        public static Color hierarchyColor4 = new Color(.3f, 1, .3f);
        public static Color hierarchyColor5 = new Color(.3f, 1, 1);
        /**************************************************************/
	
        /***************VARIED_COLOR_MODE*****************/
        public static Color PrimaryColor;
        public static Color secondaryColor = PrimaryColor * .975f;
        /*************************************************/
        #endregion

        //Used to display and call the function to open preferences window
        [MenuItem("Window/FuzzyTools/Preferences", priority = 1)]
        private static void Preferences()
        {
            Settings.Init();
        }
        
        #endregion
        #region StaticVars
        
        //Lists used for solo-ing the gameobjects in the scene
        static List<GameObject> allSoloingObjects = new List<GameObject>();
        static List<GameObject> activeSoloingObjects = new List<GameObject>();
        static List<GameObject> soloingParents = new List<GameObject>();
        static List<GameObject> soloSels = new List<GameObject>();
        static bool _soloed = false;

        //Used for Stop And Apply 
        static GameObject _selSAA;
        //static string _locationSAA;
        //static string _nameSAA;
        
        //Used Recent Scenes
        public static List<string> recentScenes = new List<string>();
        public static int sceneCount = 0;
        public static string previousScene = "";
        public static string newScene = "";
        
        //Stores components that are required when using select and remove
        public static List<Type> requiredComponents = new List<System.Type>();
        public static string requirees = "";

        //Lists for keeping track of locators
        public static List<Transform> removedLocators = new List<Transform>(); 
        public static List<Transform> locators = new List<Transform>();

        //Terrain Resolutions
        public static string[] terrainRezOptions = { "33", "65", "129", "257", "513", "1025", "2049", "4097" };

        
        #endregion
        #region MayaHotkeys
        
        //Disables and re-enables the selected GameObjects
        [MenuItem("Window/FuzzyTools/MayaHotkeys/Hide &h")]
        private static void HideSelection()
        {
            //Store the selection into an array
            var sels = Selection.gameObjects;
            
            //Loop through each GameObject in selection
            foreach (var obj in sels)
            {
                //Ensure that the selected GameObject is in the hierarchy
                if (AssetDatabase.Contains(obj)) continue;
                //Register the object to Undo
                Undo.RegisterCompleteObjectUndo(obj, "HideObject");
                //Switches the GameObjects state to the inverse of what it is. (If enabled, disable it. If Disabled, enable it)
                obj.SetActive(!obj.activeSelf);
            }
        }

        //Creates a new empty GameObject and then parents all selected GameObjects to said group
        [MenuItem("Window/FuzzyTools/MayaHotkeys/Group &%g")]
        private static void GroupSelection()
        {
            //Store selected GameObjects
            var sels = Selection.gameObjects;
            var transforms = Selection.transforms;
            //Store system text clipboard
            var clipBoard = EditorGUIUtility.systemCopyBuffer;
            //Create new group naming it based upon user input. If User selects "Yes", The new GameObject's name will be the clipboard contents.
            //Otherwise the new GameObject's name will default to the variable defaultGroupName which can be changed in preferences.
            var group = (EditorUtility.DisplayDialog("Use Clipboard for group name?", "Contents are: " + clipBoard, "Yes", "No")
                ? new GameObject(clipBoard)
                : new GameObject(defaultGroupName));
            group.transform.position = transforms.AveragePosition();
            //Create temporary bool and then cycle through each GameObject in Selection
            var allSame = false;

            //May be used to replace foreach loop in the future. This function is faster but does not check the hierarchy.
            //allSame = (sels.All(sel => sel.transform.parent == sels[0].transform.parent));
            
            foreach (var obj in sels)
            {
                //Ensure that the selected GameObject is in the hierarchy
                if (AssetDatabase.Contains(obj)) continue;
                //Checks the parent of each selected GameObject checking if they all have the same parent. If they do
                //not break from the loop;
                if (sels[0].transform.parent == obj.transform.parent)
                {
                    allSame = true;
                }
                else
                {
                    allSame = false;
                    break;
                }
            }
            //If all the children have the same parent set the parent of the new group to the same parent.
            if (allSame)
                group.transform.SetParent(sels[0].transform.parent);

            //Register Groups creation to Undo.
            Undo.RegisterCreatedObjectUndo(group, "CreateGroup");
            //Loop through each object in the selection setting the parent of the transform to be the new group while
            //registering the action to Undo at the same time
            foreach (var obj in sels)
            {
                Undo.SetTransformParent(obj.transform, group.transform, "GroupObjects");
            }
        }

        //Makes selected objects the child of the Active Object (Active GameObject varies based on selection source.
        //If you select objects using the hierarchy, the first object selected is the Active GameObject. Whereas if
        //you select from the scene view, the last selected GameObject will be considered the Active GameObject.
        [MenuItem("Window/FuzzyTools/MayaHotkeys/Parent &p")]
        private static void ParentSelection()
        {
            //Store selected GameObjects
            var sels = Selection.gameObjects;
            //Ensure that more than one GameObject is selected
            if (sels.Length <= 1) return;
            
            //Store the actively selected transform
            var parent = Selection.activeTransform;
            
            //Loop through the selection skipping the GameObject with the same transform as the parent
            //Registering the parent of each transform to Undo, and assigning the transform parent of each
            //selected GameObject to the stored parent variable
            foreach (var obj in sels)
            {
                //Ensure that the selected GameObject is in the hierarchy
                if (AssetDatabase.Contains(obj)) continue;
                
                if (obj.transform == parent) continue;
                Undo.SetTransformParent(obj.transform, parent, "SetParent");
            }
        }

        //Sets the parent to all selected objects to null effectively "un parenting" the selected GameObjects.
        [MenuItem("Window/FuzzyTools/MayaHotkeys/UnParent &#p")]
        private static void UnParentSelection()
        {
            //Store selected GameObjects
            var sels = Selection.gameObjects;
            //Looping through each GameObject in selection, set the parent to null and register the change to Undo
            foreach (var obj in sels)
            {
                //Ensure that selected GameObject is in scene hierarchy.
                if (AssetDatabase.Contains(obj)) continue;
                Undo.SetTransformParent(obj.transform, null, "Unparent");
            }
        }

        //This function will be completely reconfigured, however currently it cycles through every GameObject
        //in the scene and disables every GameObject not in selection. Full documentation for this function will be
        //delayed until the function is reconfigured.
        [MenuItem("Window/FuzzyTools/MayaHotkeys/Solo &#i")]
        private static void SoloSelection()
        {

            var sels = Selection.gameObjects;
            
            if (!_soloed)
            {
                soloSels.AddRange(sels);
            }

            if (sels.Length > 0)
            {

                _soloed = true;
                allSoloingObjects.AddRange(FindObjectsOfType<GameObject>());
                foreach (var obj in allSoloingObjects)
                {
                    if (obj.activeInHierarchy && !soloSels.Contains(obj))
                    {
                        activeSoloingObjects.Add(obj);
                    }
                }

                if (activeSoloingObjects.Count != 0)
                {
                    foreach (var obj in soloSels)
                    {
                        activeSoloingObjects.Remove(obj);
                        Undo.RegisterCompleteObjectUndo(obj, "SoloThese");
                        obj.SetActive(true);
                        var temp = obj;
                        while (temp.transform.parent)
                        {
                            var _parent = temp.transform.parent.gameObject;
                            if (_parent.GetComponent<MeshRenderer>() && _parent.GetComponent<MeshRenderer>().enabled)
                            {
                                soloingParents.Add(temp.transform.parent.gameObject);
                                Undo.RegisterCompleteObjectUndo(_parent.GetComponent<MeshRenderer>(), "DisableMeshRenderer");
                                _parent.GetComponent<MeshRenderer>().enabled = false;
                            }
                            if (_parent.gameObject.GetComponent<SkinnedMeshRenderer>() && _parent.GetComponent<SkinnedMeshRenderer>().enabled)
                            {
                                soloingParents.Add(temp.transform.parent.gameObject);
                                Undo.RegisterCompleteObjectUndo(_parent.GetComponent<SkinnedMeshRenderer>(), "DisableSkinnedMeshRenderer");
                                _parent.GetComponent<MeshRenderer>().enabled = false;
                            }
                            activeSoloingObjects.Remove(_parent);
                            temp = _parent;
                        }
                    }

                    foreach (var obj in activeSoloingObjects)
                    {
                        Undo.RegisterCompleteObjectUndo(obj, "SoloTurnedOff");
                        obj.SetActive(false);
                    }

                }
            }
            else
            {

                foreach (GameObject obj in activeSoloingObjects)
                {
                    if (obj)
                    {
                        Undo.RegisterCompleteObjectUndo(obj, "UndoSolo");
                        obj.SetActive(true);
                    }
                }
                if (_soloed)
                {
                    _soloed = false;
                    foreach (GameObject obj in soloingParents)
                    {
                        if (obj)
                        {
                            if (obj.GetComponent<MeshRenderer>())
                            {
                                Undo.RegisterCompleteObjectUndo(obj.GetComponent<MeshRenderer>(), "EnableMeshRenderer");
                                obj.GetComponent<MeshRenderer>().enabled = true;
                            }
                            else if (obj.GetComponent<SkinnedMeshRenderer>())
                            {
                                Undo.RegisterCompleteObjectUndo(obj.GetComponent<SkinnedMeshRenderer>(), "EnableSkinnedMeshRenderer");
                                obj.GetComponent<SkinnedMeshRenderer>().enabled = true;
                            }
                        }
                    }
                    soloSels.Clear();
                    activeSoloingObjects.Clear();
                }
            }
        }

        // This is disabled because I want to remember it, but it's fairly useless. It allows you to switch between two
        //different layouts.
        /*[MenuItem("FuzzyTools/MayaHotkeys/4x4Layout & ")]
        private static void ChangeLayout()
        {
            if (AssetDatabase.IsValidFolder("Assets/3PupperStudios/Editor/Layouts"))
            {
                if (EditorWindow.focusedWindow.titleContent.text == "Scene")
                {
                    if (!spaceSplit)
                    {
                        spaceSplit = true;
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "Assets/3PupperStudios/Editor/Layouts/Maya 4 Split.wlt");
                        EditorUtility.LoadWindowLayout(path);
                        EditorUtility.LoadWindowLayout(path);
                    }
                    else
                    {
                        spaceSplit = false;
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "Assets/3PupperStudios/Editor/Layouts/BestLayout.wlt");
                        EditorUtility.LoadWindowLayout(path);
                    }
                    EditorWindow.FocusWindowIfItsOpen<SceneView>();
                }
            }
        }*/

        #endregion    
        #region Gizmos

        //Currently this function is only used to draw each locator
        //DrawGizmo is used to tell a non-monobehaviour script to run during OnDrawGizmo. NonSelected is chosen to make
        //the locators only show when not selected.
        [DrawGizmo(GizmoType.NonSelected)]
        private static void DrawMyLocators(Transform t, GizmoType type)
        {
            //Loop through the Transforms in the list  removeLocators to to remove any transforms contained in the main
            //locator list.
            foreach (var loc in removedLocators)
            {
                if (!locators.Contains(loc)) continue;//If current Transform is null, move on to the next Transform.
                locators.Remove(loc);
            }
            //Once finished updating the locators list, clear the contents of removeLocators.
            removedLocators.Clear();
            
            //TODO ADD SWITCH STATEMENT SO USER CAN CHOOSE WHAT THE LOCATOR WILL LOOK LIKE.
            
            //Loop through Transforms in the locators list. To draw the gizmo for each one
            foreach (var loc in locators)
            {
                //If the current transform is null, add it to the removeLocators list so that it can be removed next
                //time this function is called, and then move on to the next Transform.
                if (loc == null)
                {
                    removedLocators.Add(loc);
                    continue;
                }

                //Get the start and end location points for X, Y, and Z axes based on the preferences variable
                //locatorScale and the Transforms position.
                var x1 = loc.position + (loc.right*locatorScale);
                var x2 = loc.position + (loc.right * -1 * locatorScale);
                var y1 = loc.position + (loc.up * locatorScale);
                var y2 = loc.position + (loc.up * -1 * locatorScale);
                var z1 = loc.position + (loc.forward * locatorScale);
                var z2 = loc.position + (loc.forward * -1 * locatorScale);
                
                //Set the color of the locator based on the preferences variable locatorColor
                Gizmos.color = locatorColor;
                //Draw each gizmo as a line between the start and end locations for each axis.
                Gizmos.DrawLine(x1, x2);
                Gizmos.DrawLine(y1, y2);
                Gizmos.DrawLine(z1, z2);
            }
            
        }

        #endregion 
        #region MayaFunctions
        [MenuItem("Window/FuzzyTools/MayaFunctions/MatchPosition &#m")]
        private static void MatchPosition()
        {
            List<GameObject> sels = new List<GameObject>();
            sels.AddRange(Selection.gameObjects);
            if (sels.Count > 1)
            {

                Transform masterTrans = Selection.activeGameObject.transform;
                sels.Remove(masterTrans.gameObject);
                foreach (GameObject obj in sels)
                {
                    if (obj.transform.position != masterTrans.position)
                    {
                        Undo.RegisterCompleteObjectUndo(obj.transform, "MatchTransform");
                        obj.transform.position = masterTrans.position;
                    }
                }
            }
        }
        [MenuItem("Window/FuzzyTools/MayaFunctions/MatchRotation &#,")]
        private static void MatchRotation()
        {
            List<GameObject> sels = new List<GameObject>();
            sels.AddRange(Selection.gameObjects);

            if (sels.Count > 1)
            {
                Transform masterTrans = Selection.activeGameObject.transform;

                sels.Remove(masterTrans.gameObject);
                foreach (GameObject obj in sels)
                {
                    if (obj.transform.rotation != masterTrans.rotation)
                    {
                        Undo.RegisterCompleteObjectUndo(obj.transform, "MatchTransform");
                        obj.transform.rotation = masterTrans.rotation;
                    }
                }
            }
        }
        [MenuItem("Window/FuzzyTools/MayaFunctions/MatchLocalScale &#.")]
        private static void MatchScale()
        {
            List<GameObject> sels = new List<GameObject>();
            sels.AddRange(Selection.gameObjects);

            if (sels.Count > 1)
            {
                Transform masterTrans = Selection.activeGameObject.transform;

                sels.Remove(masterTrans.gameObject);
                foreach (GameObject obj in sels)
                {
                    if (obj.transform.localScale != masterTrans.localScale)
                    {
                        Undo.RegisterCompleteObjectUndo(obj.transform, "MatchTransform");
                        obj.transform.localScale = masterTrans.localScale;
                    }
                }
            }
        }
        [MenuItem("Window/FuzzyTools/MayaFunctions/MatchTransform &m")]
        private static void MatchTransform()
        {
            MatchPosition();
            MatchRotation();
            MatchScale();
        }
        //Sort Of Maya Functions
        [MenuItem("Window/FuzzyTools/MayaFunctions/RemoveAllAttributes &%x")]
        static void RemoveAttrHotkey()
        {
            List<GameObject> sels = new List<GameObject>();
            sels.AddRange(Selection.gameObjects);
            if (sels.Count == 1)
            {
                GameObject obj = sels[0];
                List<Component> components = new List<Component>();
                components.AddRange(obj.GetComponents<Component>());
                components.Remove(obj.transform);
                foreach (Component comp in components)
                {
                    if (comp.GetType() != typeof(Transform))
                    {
                        //Undo.RegisterCompleteObjectUndo(comp, "RemoveAttr");
                        //When you undo in 2017 it has a bug that it doesn't add properly add the mesh renderer back.
                        //This bug is fixed in 2018.1
                        Undo.DestroyObjectImmediate(comp);
                    }
                }
            }
        }

        /*[MenuItem("FuzzyTools/MayaFunctions/TransferAllAttributes")]
        static void TransferAttrHotkey()
        {
            List<GameObject> sels = new List<GameObject>();
            sels.AddRange(Selection.gameObjects);
            if (sels.Count == 2)
            {
                GameObject obj = Selection.activeGameObject;
                sels.Remove(obj);

                List<Component> _components = new List<Component>();
                _components.AddRange(obj.GetComponents<Component>());
                List<Component> _objComps = new List<Component>();
                _objComps.AddRange(sels[0].GetComponents<Component>());

                foreach (Component component in _components)
                {
                    UnityEditorInternal.ComponentUtility.CopyComponent(component);
                    if (component.GetType() != typeof(Transform))
                    {
                        if (!_objComps.Contains(component))
                        {
                            Undo.RegisterCompleteObjectUndo(component, "TransferAttr");
                            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(sels[0]);
                        }
                        else
                        {
                            Component pasteComponent = _objComps[_objComps.IndexOf(component)];
                            Undo.RegisterCompleteObjectUndo(component, "TransferAttr");
                            UnityEditorInternal.ComponentUtility.PasteComponentValues(pasteComponent);
                        }
                    }
                }
                //MatchPosition();
                //MatchRotation();
                //MatchScale();
            }
        }*/// Duplicate of GameObject version


        #endregion
        #region EfficiencyFunctions
        [MenuItem("Window/FuzzyTools/Auto Snap &%l")]//always on
        private static void OpenAutoSnap()
        {
            AutoSnap.Init();
        }

        [MenuItem("Window/FuzzyTools/Physics Simulator")]
        private static void OpenPhysicsSimulator()
        {
            SimulatePhysics.Init();
        }
        
        [MenuItem("Window/FuzzyTools/Toggle Inspector Lock &l")]
        static void ToggleInspectorLock()
        {
            ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;
            ActiveEditorTracker.sharedTracker.activeEditors[0].Repaint();
        }
        
        [MenuItem("Assets/FuzzyTools/Terrain/Add To Terrain")]
        private static void AddToTerrain()
        {
            ShowTerrainPrefabAdder.Init();
        }
        
        [MenuItem("Assets/FuzzyTools/Terrain/Replace SplatMap")]
        private static void ReplaceSplatMap()
        {
            SplatMapReplacer.Init();
        }
        
        [MenuItem("Assets/FuzzyTools/Convert/Image(s)")]
        private static void ConvertImage()
        {
            ConvertImages.Init();
        }

        [MenuItem("Assets/FuzzyTools/Convert/Model to Terrain")]
        private static void OpenObjToObjAssets()
        {
            ConvertModelToTerrain.Init();
        }

        [MenuItem("Assets/FuzzyTools/Convert/Terrain To Model")]
        private static void OpenTerrainToOBJAssets()
        {
            var terrain = Selection.activeObject as TerrainData;
            ConvertTerrainToModel.Init(null, terrain);
        }
        
        [MenuItem("Assets/FuzzyTools/Convert/Model(s)")]
        private static void ConvertModelsAssets()
        {
            var mesh = Selection.activeObject as Mesh;
            GameObject obj = null;
            if (mesh == null)
            {
                obj = Selection.activeObject as GameObject;
                if (obj == null) return;
            }

            ConvertMeshToModel.Init(obj, mesh);
        }
        
        [MenuItem("Assets/FuzzyTools/Replace Shaders")]
        static void AssetsReplaceMaterials()
        {
            Object[] objs = Selection.objects;
            List<Material> mats = new List<Material>();
            foreach (var obj in objs)
            {
                Material mat = obj as Material;
                if (mat != null)
                {
                    mats.Add((mat));
                }
            }
            
            ChangeShaderToSelected.Init(mats.ToArray());
        }
        
        [MenuItem("Assets/FuzzyTools/Adjust Snapping Grid")]
        private static void AssetsAdjSnapGrid()
        {
            Object myObj =  Selection.activeObject;
            
            GameObject obj = myObj as GameObject;
            if (obj == null)
            {
                if(EditorUtility.DisplayDialog("Reset Snapping Grid?", "No viable object selected. Would you like to reset the snapping grid to the unity default?", "Yes", "No"))
                {
                    SetSnappingScale(null);
                }

                return;
            }

            if (!obj.GetComponent<Renderer>())
            {
                EditorUtility.DisplayDialog("Must select Object with Renderer.",
                    "The selected GameObject does not have a Renderer which is required for this function. Please select a GameObject with a renderer and try again",
                    "Okay");

                return;
            }
            
            SetSnappingScale(obj);
        }
        
        [MenuItem("File/Open Previous Scene", false, 151)]
        private static void ReopenLastScene()
        {
            if (previousScene != SceneManager.GetActiveScene().path && previousScene != "")
            {
                var split = previousScene.Split('/');

                var sceneName = split[split.Length - 1];
                sceneName = sceneName.Substring(0, sceneName.IndexOf("."));
                var option = EditorUtility.DisplayDialogComplex("Open " + sceneName, "What would you like to do?", "Save and Open", "Cancel", "Open Don't Save");
                switch(option)
                {
                    case (0):
                        if (SceneManager.sceneCount > 1 && EditorUtility.DisplayDialog("Save all scenes?", "Would you like to save all open scenes?", "Yes", "No"))
                        {
                            for (var i = 0; i > SceneManager.sceneCount; i++)
                            {
                                EditorSceneManager.SaveScene(SceneManager.GetSceneAt(i));
                            }
                        }else
                        {
                            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                        }
                        EditorSceneManager.OpenScene(previousScene);
                        break;
                    case (1):
                        break;
                    case (2):
                        EditorSceneManager.OpenScene(previousScene);
                        break;
                }
            }
        }
        
        [MenuItem("File/Recent Scenes", false, 152)]
        private static void OpenRecentScenes()
        {
            ShowRecentScenes.Init();
        }
        
        #endregion
        #region ShadingModes
        [MenuItem("Window/FuzzyTools/Shading Mode/Wireframe &4")]
        private static void SetToWireframe()
        {
#if UNITY_2018_1_OR_NEWER
        SceneView.CameraMode mode = new SceneView.CameraMode();
        mode.drawMode = DrawCameraMode.Wireframe;
        SceneView.lastActiveSceneView.cameraMode = mode;
#endif
#if UNITY_2017
            SceneView.lastActiveSceneView.renderMode = DrawCameraMode.Wireframe;
#endif

        }
        [MenuItem("Window/FuzzyTools/Shading Mode/Shaded &5")]
        private static void SetToShaded()
        {
#if UNITY_2018_1_OR_NEWER
        SceneView.CameraMode mode = new SceneView.CameraMode();
        mode.drawMode = DrawCameraMode.Textured;
        SceneView.lastActiveSceneView.cameraMode = mode;
#endif
#if UNITY_2017
            SceneView.lastActiveSceneView.renderMode = DrawCameraMode.Textured;
#endif
        }
        [MenuItem("Window/FuzzyTools/Shading Mode/Shaded Wireframe &6")]
        private static void SetToShadedWireframe()
        {
#if UNITY_2018_1_OR_NEWER
        SceneView.CameraMode mode = new SceneView.CameraMode();
        mode.drawMode = DrawCameraMode.TexturedWire;
        SceneView.lastActiveSceneView.cameraMode = mode;
#endif
#if UNITY_2017
            SceneView.lastActiveSceneView.renderMode = DrawCameraMode.TexturedWire;
#endif
        }
        #endregion
        #region ContextFunctions
        #region Transform Context Functions
        
        //Obsolete function- no documentation will be made unless request is made.
        [MenuItem("CONTEXT/Transform/Transfer All Attributes"), System.Obsolete("This context function has been deprecated in exchange for the GameObject version")]
        private static void TransferAttr(MenuCommand command)
        {
            var t = (Transform)command.context;
            var obj = t.gameObject;
            var sels = Selection.gameObjects;
            if (obj == Selection.activeGameObject) return;
            if (sels.Length != 2) return;
            
            Debug.Log("This function has been deprecated in exchange for the GameObject Version and will be removed in the next release. If you want it back, please submit a request");
            /*var components = sels[0].GetComponents<Component>();
            var objComps = obj.GetComponents<Component>();

            foreach (var component in components)
            {
                ComponentUtility.CopyComponent(component);

                if (!objComps.Contains(component))
                {
                    Undo.RegisterCompleteObjectUndo(component, "TransferAttr");
                    ComponentUtility.PasteComponentAsNew(obj);
                }
                else
                {
                    var pasteComponent = objComps[objComps.IndexOf(component)];
                    Undo.RegisterCompleteObjectUndo(component, "TransferAttr");
                    ComponentUtility.PasteComponentValues(pasteComponent);
                }
            }
            MatchPosition();
            MatchRotation();
            MatchScale();*/
        
        }

        //Obsolete function- no documentation will be made unless a request is made.
        [MenuItem("CONTEXT/Transform/Stop And Apply"), System.Obsolete("This context function has been deprecated in exchange for the GameObject version")]
        private static void StopAndApply(MenuCommand command)
        {
            Debug.Log("This function has been deprecated in exchange for the GameObject Version and will be removed in the next release. If you want it back, please submit a request");
            /*if (EditorApplication.isPlaying)
            {
                if (Selection.objects.Length != 1)
                {
                    var t = command.context;
                    Selection.objects = new Object[] { t };
                }

                if (Selection.objects.Length != 1) return;
                
                _selSAA = Selection.activeGameObject;
                _nameSAA = _selSAA.name;
                var localPath = "Assets/Resources/" + _nameSAA + ".prefab";
                _locationSAA = localPath;
                if (EditorUtility.DisplayDialog("Are you sure?", "This will temporarily create a prefab under 'Assets/Resources' folder with the same name of the object you are apply your changes to and cannot be undone with Undo or Ctrl/Cmd + Z.", "Yes", "No"))
                {
                    if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                    {
                        string guid = AssetDatabase.CreateFolder("Assets", "Resources");
                        /*string newFolderPath = #1#
                        AssetDatabase.GUIDToAssetPath(guid);
                    }
                    CreateNew(_selSAA, localPath);

                    EditorApplication.isPlaying = false;
                    EditorApplication.update += DuplicateAndApply;
                }
            }
            else
            {
                Debug.Log("This command only works in Play Mode");
            }*/
        }

        //Turn GameObject into Locator temporarily, or persistently
        [MenuItem("CONTEXT/Transform/Make Locator")]
        private static void MakeLocator(MenuCommand command)
        {
            //Store the component that ran this function as a Transform.
            var t = command.context as Transform;
            //The variable t should never be null, but just to be surer we make sure, in case it is, return/end function.
            if(t == null) return;
            //Ensure this Transform component is not already in the Locators list. If it is, return.
            if (locators.Contains(t)) return;
            //Compare the name of the stored Transform's GameObject to the preferences variable locatorName
            if (t.name != locatorName)
            {
                //Get user input checking if they would like to change the stored Transform's name to match what the
                //Initialization Gizmo function looks for to recreate locators on scene load and when Unity first Opens.
                if (EditorUtility.DisplayDialog("Change name?",
                    "Locator system looks for GameObjects named '" + locatorName + "'. Change Object name to match convention?",
                    "Yes", "No"))
                {
                    t.name = locatorName;
                }
            }
            //Add the Transform to the locators list so that the DrawGizmos function will draw a locator
            locators.Add(t);
        }
        #endregion
        
        //Allows the user to compy the component and remove the component at the same time effectively "Cutting" the component
        [MenuItem("CONTEXT/Object/Cut Component")]
        private static void CutComponent(MenuCommand command)
        {
            //Store the selected component which you right clicked
            var component = command.context as Component;
            //The variable component should never be null, but just to be surer we make sure, in case it is, return/end function.
            if (component == null) return;
            //Copy the stored component
            ComponentUtility.CopyComponent(component);
            //Remove the component from the gameObject and register the component to Undo
            Undo.DestroyObjectImmediate(component);
        }

        //Search through all components on the GameObject with the selected component and remove all components
        //of the same type.
        [MenuItem("CONTEXT/Object/Remove All Components of Type")]
        private static void RemoveOfType(MenuCommand command)
        {
            //Store the selected component which you right clicked
            var component = command.context as Component;
            //The variable t should never be null, but just to be surer we make sure, in case it is, return/end function.
            if (component == null) return;
            //Check and store the type of the selected component
            var type = component.GetType();
            
            //create a temporary bool that will be used to store user input
            var checkParent = false;
            //Compare the stored type with its own BaseType and check if the preferences list ignoreTypes contains the
            //BaseType of type. If the first check results as different types and ignoreTypes does not already contain
            //The BaseType's Type, run this
            if (type != type.BaseType &&  !ignoreTypes.Contains(type.BaseType))
            {
                //check if user wants to remove components of same Type as stored type or the type of
                //the stored types BaseType. Store the choice as an int.
                var index = EditorUtility.DisplayDialogComplex("Remove all of exact type or base type?",
                    "Exact Type is: "+ component.GetType()+ "\n"+ "Base Type is: "+ component.GetType().BaseType+ "\n" +"What would you like to do?", "Remove Exact Type", "Cancel", "Remove Base Type");
                //Dependant on what the user chooses, break from the switch statement and continue, return and end
                //the function, or set type to equal its BaseType instead and store that we did so in the bool checkParent
                switch (index)
                {
                    default:
                    case 0:
                        break;
                    case 1:
                        return;
                    case 2:
                        type = type.BaseType;
                        checkParent = true;
                        break;
                }
            }

            //Store all components attached to the selected components GameObject
            var components = component.gameObject.GetComponents<Component>();
            //Call CheckIfReuired sending the stored array of components
            CheckIfRequired(components, type);
            //create temporary list of Types that will be used to store and check
            var multipleTypes = new List<Type>();
            //Mark a goto point for if there are requirements found This will be implemented in future versions
            //RequirementFound:
            
            //Check if the static list requiredComponents contains anything and if it does, check if the same list
            //contains the type of the stored type.
            if (requiredComponents.Count > 0 && requiredComponents.Contains(type))
            {
                //Create temporary int that will be used to store the count of 
                var count = 0;
                
                //Loop through the stored array of components first storing the type of the component and then checking
                //if checkParent is true or not and then dependant on if it is or not, check if the compType or BaseType
                //of compType are the same. If they are not the same move immediately to the next component. If they are
                //the same, increase the temporary int count to signal that there is a component of the same type found
                //in the array of components. This count also resembles the number of components of this Type required.
                foreach (var comp in components)
                {
                    var compType = comp.GetType();
                    if (!checkParent && compType != type) continue;
                    if (checkParent && compType.BaseType != type) continue;
                    count++;
                }
                //Another temporary int this will be used to keep track of the users input. It will only be used if
                //count is greater than 1.
                var userChoice = 0;
                //If only one of the required component exists, advise the user that the stored type cannot be removed
                //and list the components that require it. Once the User accepts the notification end the function.
                if (count == 1)
                {
                    EditorUtility.DisplayDialog("Requirements found.", type + " is required by:\n" + requirees + "\n" + "It cannot be removed.", "Okay");
                    return;
                }//If the count of components of the same Type or BaseType is more than one allow the user to select
                //If they want to keep the first component or the last component, or neither. Record the users choice
                //as an int and store it in the previously declared userChoice.
                else if (count > 1)
                {
                    userChoice = EditorUtility.DisplayDialogComplex("Requirements found.", type + " is required by:\n" +
                       requirees + "\n" + "What would you like to do?", "Keep First", "Cancel", "Keep Last");
                }
                //Check the value of userChoice
                switch (userChoice)
                {
                    default://default listed for the sake of all possible cases having options
                    case 0:
                        //temporary bool will be used to skip the first component in stored array components
                        var skip = true;
                        //Loop through the stored array components
                        foreach (var comp in components)
                        {
                            //store the type of the component to reduce get calls
                            var compType = comp.GetType();
                            //Check checkParent and compare the Type or BaseType of the component against the stored type
                            if (!checkParent && compType != type) continue;
                            if (checkParent && compType.BaseType != type) continue;
                            //Because skip is set to true it will run for the first component and then move to the next
                            //component right away effectively skipping the first component.
                            if (skip)
                            {
                                skip = false;
                                continue;
                            }
                            //Immediately destroy the component and register the action to Undo
                            Undo.DestroyObjectImmediate(comp);
                        }
                        return;
                        //This will be implemented in future versions
                        //if(multipleTypes.Count == 0)return;
                        //type = multipleTypes[0];
                        //multipleTypes.RemoveAt(0);
                        //goto RequirementFound;
                    case 1:
                        return;
                        //This will be implemented in future versions
                        //if(multipleTypes.Count == 0)return;
                        //type = multipleTypes[0];
                        //multipleTypes.RemoveAt(0);
                        //goto RequirementFound;
                    case 2:
                        /*//For 100% surety, we make sure type isn't null. It never should be but just in case. // TODO INSPECT LATER
                        if (type == null) return;
                        //If user has selected to remove BaseType, set the stored type to equal the original stored
                        //stored types base.
                        if(checkParent) type = type.BaseType;*/
                        
                        //Store the amount of components set to be removed
                        var compsLeft = count;
                        //Loop through the stored array components
                        foreach (var c in components)
                        {
                            //If the current component is null, immediately move to the next component
                            if (c == null) continue;
                            //We are keeping the last component so if comps left equals one we want to stop removing.
                            if (compsLeft == 1) return;
                            //Store the components type to reduce Get calls.
                            var cType = c.GetType();
                            //Check if user selected to remove type or BaseType. Compare the current components type or
                            //its BaseType against the stored type
                            if (!checkParent && cType != type) continue;
                            if (checkParent && cType.BaseType != type) continue;
                            //If types match, immediately remove the component and register the action to Undo
                            Undo.DestroyObjectImmediate(c);
                            //Reduce the count of components left working toward one left
                            compsLeft--;
                        }
                        return;
                        //This will be implemented in future versions
                        //if(multipleTypes.Count == 0)return;
                        //type = multipleTypes[0];
                        //multipleTypes.RemoveAt(0);
                        //goto RequirementFound;
                }
            }
            else//If nothing requires the stored type
            {
                //Loop through each component
                foreach (var comp in components)
                {
                    //Store the current components type to reduce Get calls
                    var compType = comp.GetType();
                    //Check if user selected to remove type or BaseType. Compare the current components type or
                    //its BaseType against the stored type
                    if (!checkParent && compType != type) continue;
                    if (checkParent && compType.BaseType != type) continue;
                    //This will be implemented in future versions
                    /*if (!multipleTypes.Contains(comp.GetType()))
                    {
                        multipleTypes.Add(comp.GetType());
                    }
                    else*/
                    //Remove the component and register the action to Undo
                    Undo.DestroyObjectImmediate(comp);
                }

                if (multipleTypes.Count == 0)
                {
                    //Advise the user that all components of the selected type have been removed
                    EditorUtility.DisplayDialog("Success!", "All " + type + " successfully Removed!", "Okay");
                    return;
                }
                //This will be implemented in future versions
                //type = multipleTypes[0];
                //multipleTypes.RemoveAt(0);
                //checkParent = false;
                //goto RequirementFound;
                
                
            }
        }
        
        [MenuItem("CONTEXT/Rigidbody/Freeze")]
        private static void FreezeRigid(MenuCommand command)
        {
            var rigid = (Rigidbody)command.context;
            Undo.RegisterCompleteObjectUndo(rigid, "MatchTransform");
            rigid.mass = 0;
            rigid.angularDrag = 0;
            rigid.useGravity = false;
            rigid.isKinematic = true;
            rigid.constraints = RigidbodyConstraints.FreezeAll;
        }
        /*[MenuItem("CONTEXT/Transform/Remove All Components")] // This function will be refactored to check for required components. Until then it will be disabled to prevent errors
        private static void RemoveAllComp(MenuCommand command)
        {
            var t = (Transform)command.context;
            var obj = t.gameObject;
            var components = new List<Component>();
            components.AddRange(obj.GetComponents<Component>());
            components.Remove(t);

            foreach (var comp in components)
            {
                if(comp.GetType() != typeof(Transform))
                {
                    Undo.DestroyObjectImmediate(comp);
                }
            }
        }*/

        [MenuItem("CONTEXT/Transform/Randomize Rotation")]
        static void ContextRotationRandomize(MenuCommand command)
        {
            var t = command.context as Transform;
            if (t == null) return;
            t.rotation = UnityEngine.Random.rotation;
        }
        [MenuItem("CONTEXT/Material/Replace Material")]
        static void ContextRemoveMaterial(MenuCommand command)
        {
            Material[] mat = {command.context as Material};
            ChangeShaderToSelected.Init(mat);
        }
        [MenuItem("CONTEXT/Renderer/Adjust Snapping Grid")]
        private static void ContextAdjSnapGrid(MenuCommand command)
        {
            Renderer rend = command.context as Renderer;
            if (rend == null)
            {
                return;
            }

            GameObject obj = rend.gameObject;
            SetSnappingScale(obj);
        }

        [MenuItem("CONTEXT/MeshFilter/Invert Mesh")]
        private static void ContextInvertMesh(MenuCommand command)
        {
            var filter = command.context as MeshFilter;
            if (filter == null) return;
            var mesh = filter.sharedMesh;
            if (mesh == null) return;
            var meshTriangles = mesh.triangles;
            meshTriangles = meshTriangles.Reverse().ToArray();
            var meshNormals = mesh.normals;
            //meshNormals = meshNormals.Reverse().ToArray();
            //var meshBindPoses = mesh.bindposes.Reverse().ToArray();
            
            
            var newMesh = new Mesh()
            {
                name = mesh.name + "Inverted",
                vertices = mesh.vertices,
                triangles = meshTriangles,
                normals = meshNormals,
                uv = mesh.uv,
                uv2 = mesh.uv2
            };
            Undo.RegisterCompleteObjectUndo(filter, "InvertMesh");
            filter.sharedMesh = newMesh;
        }

        #region Terrain Context Functions
        [MenuItem("CONTEXT/Terrain/Clear Textures")]
        static void ClearSplatMaps(MenuCommand command)
        {
            Terrain t = command.context as Terrain;
            if (t != null)
            {
                Undo.RegisterCompleteObjectUndo(t.gameObject, "ClearSplatMaps");
#if UNITY_2018_3_OR_NEWER
                t.terrainData.terrainLayers = new TerrainLayer[0];
#endif
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
                t.terrainData.splatPrototypes = new List<SplatPrototype>().ToArray();
#endif
            }
        }
        [MenuItem("CONTEXT/Terrain/Clear Trees")]
        static void ClearTrees(MenuCommand command)
        {
            Terrain t = command.context as Terrain;
            if (t != null)
            {
                Undo.RegisterCompleteObjectUndo(t.gameObject, "ClearTrees");
                t.terrainData.treePrototypes = new List<TreePrototype>().ToArray();
            }
        }
        [MenuItem("CONTEXT/Terrain/Clear Detail Meshes")]
        static void ClearDetailMeshes(MenuCommand command)
        {
            Terrain t = command.context as Terrain;
            if (t != null)
            {

                List<DetailPrototype> grassDetails = new List<DetailPrototype>();
                foreach (DetailPrototype proto in t.terrainData.detailPrototypes)
                {
                    if (!proto.usePrototypeMesh)
                    {
                        grassDetails.Add(proto);
                    }
                }
                Undo.RegisterCompleteObjectUndo(t.gameObject, "ClearDetailMeshes");
                t.terrainData.detailPrototypes = grassDetails.ToArray();
            }
        }
        [MenuItem("CONTEXT/Terrain/Clear Grasses")]
        static void CleaGrasses(MenuCommand command)
        {
            Terrain t = command.context as Terrain;
            if (t != null)
            {
                List<DetailPrototype> meshDetails = new List<DetailPrototype>();
                foreach (DetailPrototype proto in t.terrainData.detailPrototypes)
                {
                    if (proto.usePrototypeMesh)
                    {
                        meshDetails.Add(proto);
                    }
                }
                Undo.RegisterCompleteObjectUndo(t.gameObject, "ClearMeshDetails");
                t.terrainData.detailPrototypes = meshDetails.ToArray();
            }
        }
        [MenuItem("CONTEXT/Terrain/Remove And Show Problematic Details")]
        static void RemoveShowProblemDetails(MenuCommand command)
        {
            Terrain t = command.context as Terrain;
            if (t != null)
            {
                List<DetailPrototype> newDetails = new List<DetailPrototype>();
                string removedDetails = "";
                foreach (DetailPrototype proto in t.terrainData.detailPrototypes)
                {
                    //Debug.Log(proto);
                    if (proto.usePrototypeMesh)
                    {
                        bool keep = true;
                        foreach (Renderer rend in proto.prototype.GetComponentsInChildren<Renderer>())
                        {
                            if (rend.sharedMaterials.Length != 1)
                            {
                                removedDetails += proto.prototype.name + "\n";
                                keep = false;
                                break;
                            }
                            else
                            {
                                keep = true;
                            }
                        }
                        if (keep)
                        {
                            newDetails.Add(proto);
                        }
                    }
                    else
                    {
                        newDetails.Add(proto);
                    }
                }

                Undo.RegisterCompleteObjectUndo(t.gameObject, "ClearMeshDetails");
                t.terrainData.detailPrototypes = newDetails.ToArray();
                t.terrainData.RefreshPrototypes();
                t.Flush();
                EditorUtility.DisplayDialog("Removed Mesh Details", "The following details were removed:\n" + removedDetails, "Okay");
            }
        }
        #endregion

        #endregion
        #region GameObject Functions

        [MenuItem("GameObject/FuzzyTools/Basic Light Rig", priority = 30)]
        static void CreateBasicLightRig()
        {
            GameObject lightRig = new GameObject("Light Rig");
            GameObject keyLight = new GameObject("Key Light");
            GameObject fillLight = new GameObject("Fill Light");
            GameObject backLight = new GameObject("Back Light");
            keyLight.transform.parent = lightRig.transform;
            fillLight.transform.parent = lightRig.transform;
            backLight.transform.parent = lightRig.transform;
            
            Light key = keyLight.AddComponent<Light>();
            Light fill = fillLight.AddComponent<Light>();
            Light back = backLight.AddComponent<Light>();
            key.type = LightType.Directional;
            fill.type = LightType.Directional;
            back.type = LightType.Directional;
            key.shadows = LightShadows.Soft;
            back.shadows = LightShadows.Soft;
            back.intensity = .5f;
            fill.intensity = .3f;

            Vector3 keyPos = new Vector3(-5, 10, -10);
            Vector3 fillPos = new Vector3(10, -2, 5);
            Vector3 backPos = new Vector3(-20, 0, 25);
            keyLight.transform.position = keyPos;
            fillLight.transform.position = fillPos;
            backLight.transform.position = backPos;
            keyLight.transform.LookAt(Vector3.zero);
            fillLight.transform.LookAt(Vector3.zero);
            backLight.transform.LookAt(Vector3.zero);
        }
        
        [MenuItem("GameObject/FuzzyTools/Transform/Randomize Rotation")]
        static void GameObjectRotationRandomize()
        {
            foreach (var t in Selection.transforms)
            {
                if (t == null) continue;
                if (AssetDatabase.Contains(t)) continue;
                
                t.rotation = UnityEngine.Random.rotation;
            }
            
        }
        
        [MenuItem("GameObject/FuzzyTools/Components/Transfer All Components &%c")]
        static void TransferAttr2(MenuCommand command)
        {
            if (Selection.gameObjects.Length == 2)
            {
                GameObject orig = Selection.activeGameObject;
                GameObject target = null;
                foreach (GameObject obj in Selection.gameObjects)
                {
                    if (obj != orig)
                    {
                        target = obj;
                    }
                }


                List<Component> _origComponents = new List<Component>();
                _origComponents.AddRange(orig.GetComponents<Component>());
                List<Component> _targetComps = new List<Component>();
                _targetComps.AddRange(target.GetComponents<Component>());

                foreach (Component component in _origComponents)
                {
                    UnityEditorInternal.ComponentUtility.CopyComponent(component);

                    bool _neComponent = true;
                    foreach (Component c in _targetComps)
                    {

                        if (c.GetType() == component.GetType())
                        {
                            _neComponent = false;
                            Undo.RegisterCompleteObjectUndo(component, "TransferAttr");
                            UnityEditorInternal.ComponentUtility.PasteComponentValues(c);
                            break;
                        }
                    }
                    if (_neComponent)
                    {
                        Undo.RegisterCompleteObjectUndo(component, "TransferAttr");
                        UnityEditorInternal.ComponentUtility.PasteComponentAsNew(target);
                    }

                }
                //MatchPosition();
                //MatchRotation();
                //MatchScale();



            }
            else
            {
                Debug.Log("You can only transfer the attributes to one GameObject at a time");
            }
        }
        
        [MenuItem("GameObject/FuzzyTools/Names/Alphabetize Children")]
        static void AlphabetizeChildren()
        {
            foreach (GameObject obj in Selection.gameObjects)
            {
                Undo.RegisterCompleteObjectUndo(obj, "Alphabetize");
                List<Transform> children = new List<Transform>();
                children.AddRange(obj.GetComponentsInChildren<Transform>());
                List<Transform> sortedList = children.OrderBy(go => go.name).ToList();
                for (int i = 0; i < sortedList.Count; i++)
                {
                    sortedList[i].SetSiblingIndex(i);
                }
            }
        }
        
        [MenuItem("GameObject/FuzzyTools/Prefabs/Make Prefab &%P")]
        static void MakePrefab()
        {
            if (Selection.gameObjects.Length != 0)
                MakePrefabPopup.Init();
        }
        
        [MenuItem("GameObject/FuzzyTools/Prefabs/Completely Break Prefab")]
        public static void ExecuteOnSelectedObject()
        {
            var selected = Selection.gameObjects;
            if (selected.Length == 0) return;
            if (!EditorUtility.DisplayDialog("Are you sure?",
                "This will permanently remove each selected prefabs connection to their prefabs.", "Continue",
                "Cancel")) return;
            
            var dirty = false;
            foreach (var gameObject in selected) 
            {
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
                var prefabType = PrefabUtility.GetPrefabType(gameObject);
				   
                //Don't do the thing for PrefabType.None (not a true prefab)
                if (prefabType == PrefabType.None || prefabType == PrefabType.ModelPrefab ||
                    prefabType == PrefabType.ModelPrefabInstance) continue;
			
                dirty = true;
                PrefabUtility.DisconnectPrefabInstance(gameObject);
                CheckIfPathExists("Assets/3PupperStudios/FuzzyTools/");
                var prefab = PrefabUtility.CreateEmptyPrefab("Assets/3PupperStudios/FuzzyTools/dummy.prefab");
                PrefabUtility.ReplacePrefab(gameObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
                PrefabUtility.DisconnectPrefabInstance(gameObject);
                AssetDatabase.DeleteAsset("Assets/3PupperStudios/FuzzyTools/dummy.prefab");
#endif
#if UNITY_2018_3_OR_NEWER
                var prefabType = PrefabUtility.GetPrefabAssetType(gameObject);
                if (prefabType == PrefabAssetType.NotAPrefab || prefabType == PrefabAssetType.Model) continue;
                dirty = true;
                PrefabUtility.SaveAsPrefabAsset(gameObject, "Assets/3PupperStudios/FuzzyTools/dummy.prefab");
                AssetDatabase.DeleteAsset("Assets/3PupperStudios/FuzzyTools/dummy.prefab");
#endif
            }

            if (!dirty) return;
            //UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            var sceneCount = EditorSceneManager.sceneCount;
            for (var i = 0; i < sceneCount; i++) 
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetSceneAt(i));
            }
        }
        
        [MenuItem("GameObject/FuzzyTools/Prefabs/Apply Prefab Changes &a")]
        static void ApplyPrefabChanges()
        {
            var sels = Selection.gameObjects;
            if (sels.Length < 1) return;
            Undo.RegisterCompleteObjectUndo(sels, "ApplyPrefabChanges");
            var successes = "";
            #if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
            foreach (var go in sels)
            {
                var type = PrefabUtility.GetPrefabType(go);
                if (type != PrefabType.PrefabInstance) continue;
                
                var obj = PrefabUtility.GetPrefabObject(go);
                PrefabUtility.ReplacePrefab(go, obj);
                PrefabUtility.RevertPrefabInstance(go);
                successes+=obj.name+"\n";
            }
            #endif
            #if UNITY_2018_3_OR_NEWER
            foreach (var go in sels)
            {
                var type = PrefabUtility.GetPrefabAssetType(go);
                if (type == PrefabAssetType.Model || type == PrefabAssetType.NotAPrefab) continue;
                
                var obj = PrefabUtility.GetPrefabInstanceHandle(go);
                PrefabUtility.SavePrefabAsset(go);
                PrefabUtility.RevertPrefabInstance(go, InteractionMode.AutomatedAction);
                successes+=obj.name+"\n";
            }
            #endif
            
            if (successes == "") return;
            EditorUtility.DisplayDialog("Success", "Your changes were applied to the following prefabs:\n" + successes,
                "Okay");
        }
        
        [MenuItem("GameObject/FuzzyTools/Components/Select And Remove Components")]
        static void SelectAndRemoveComponents()
        {
            SelectRemovableComponents.Init();
        }
        
        [MenuItem("GameObject/FuzzyTools/Split Terrain")]
        static void SplitTerrainV1()
        {
            List<Terrain> terrains = new List<Terrain>();
            if (Selection.gameObjects.Length > 0)
            {
                foreach (GameObject obj in Selection.gameObjects)
                {
                    if (obj.GetComponent<Terrain>())
                    {
                        terrains.Add(obj.GetComponent<Terrain>());
                    }
                }
                if (terrains.Count > 0)
                {
                    SplitTerrain.Init(terrains);
                }
            }
            if (terrains.Count==0)
            {
                SplitTerrain.Init(null);
            }
        }
        
        [MenuItem("GameObject/FuzzyTools/Colliders/Toggle Children Colliders")]
        static void ToggleChildrenColliders()
        {
            foreach(GameObject obj in Selection.gameObjects)
            {
                foreach(Collider col in obj.GetComponentsInChildren<Collider>())
                {
                    col.enabled = !col.enabled;
                }
            }
        }    
        
        [MenuItem("GameObject/FuzzyTools/Replace Shader on all Materials")]
        static void GameObjectReplaceMaterials()
        {
            GameObject[] objs = Selection.gameObjects;
            List<Material> mats = new List<Material>();
            foreach (var obj in objs)
            {
                mats.AddRange(obj.GetComponent<Renderer>().sharedMaterials);
            }
            mats = mats.Distinct().ToList();
            ChangeShaderToSelected.Init(mats.ToArray());
        }      
        
        [MenuItem("GameObject/FuzzyTools/Adjust Snapping Grid")]
        private static void GameObjectAdjSnapGrid()
        {
            
            GameObject obj = Selection.activeGameObject;
            if (obj == null)
            {
                if(EditorUtility.DisplayDialog("Reset Snapping Grid?", "No viable object selected. Would you like to reset the snapping grid to the unity default?", "Yes", "No"))
                {
                    SetSnappingScale(null);
                }

                return;
            }

            if (!obj.GetComponent<Renderer>())
            {
                EditorUtility.DisplayDialog("Must select Object with Renderer.",
                    "The selected GameObject does not have a Renderer which is required for this function. Please select a GameObject with a renderer and try again",
                    "Okay");

                return;
            }
            SetSnappingScale(obj);
        }

        [MenuItem("GameObject/FuzzyTools/Hierarchy/Paste As Child &%v", priority = 19)]
        private static void PasteAsChild()
        {
            if (Selection.gameObjects.Length != 1) return;
            var trans = Selection.activeTransform;
            EditorApplication.ExecuteMenuItem("Edit/Paste");
            var newList = Selection.gameObjects;
            foreach (var obj in newList)
            {
                var thisGameObject = obj;
                thisGameObject.transform.parent = trans;
            }
        }

        [MenuItem("GameObject/FuzzyTools/Create Locator &%n", priority = 18)]
        private static void CreateLocator()
        {
            var locator = new GameObject()
            {
                name = locatorName
            };
            locators.Add(locator.transform);
            EditorSceneManager.MarkAllScenesDirty();
            //locator.AddComponent(typeof(FuzzyToolsLocator));
        }
        
        [MenuItem("GameObject/FuzzyTools/Custom Attributes/Create GetAllComponents")]
        private static void CreateGetter()
        {
            var getter = new GameObject("GetAllComponents");
            getter.AddComponent<MonoBehaviourGetAllComponents>();
        }
        
        [MenuItem("GameObject/FuzzyTools/Hierarchy/Change Color Or Style")]
        private static void ChangeColor()
        {
            var selectedGameObjs = Selection.gameObjects;
            if (selectedGameObjs.Length == 0) return;
		
            ColorPicker.Init(selectedGameObjs);
        }

        [MenuItem("GameObject/FuzzyTools/Names/Find And Replace Or Rename")]
        private static void FindAndReplace()
        {
            FindAndReplaceName.Init();
        }

        [MenuItem("GameObject/FuzzyTools/Components/Copy Multiple Components")]
        private static void CopyMultipleComponents()
        {
            PickAndCopyComponents.Init();
        }
        
        [MenuItem("GameObject/FuzzyTools/Convert/Model to Terrain")]
        private static void OpenObjToTerrainGameObject()
        {
            ConvertModelToTerrain.Init();
        }
        
        [MenuItem("GameObject/FuzzyTools/Convert/Terrain To Model")]
        private static void OpenTerrainToOBJGameObject()
        {
            var terrain = Selection.activeGameObject.GetComponent<Terrain>();
            ConvertTerrainToModel.Init(terrain, null);
        }
        
        [MenuItem("GameObject/FuzzyTools/Convert/Model(s)")]
        private static void ConvertModelsGameObject()
        {
           var obj = Selection.activeGameObject;
           ConvertMeshToModel.Init(obj, null);
            
        }
        
        #endregion
        #region Terrain Functions
        public static void AddToSplatPrototypes()
        {
            Terrain terrain = (Terrain)FindObjectOfType(typeof(Terrain));
#if UNITY_2018_3_OR_NEWER
            var terrainLayers = new List<TerrainLayer>();
            terrainLayers.AddRange(terrain.terrainData.terrainLayers);
            var terrainAlbedoTextures = new List<Texture2D>();
            var terrainNormalTextures = new List<Texture2D>();
            var originalSplatCount = terrainLayers.Count;
            for (var i = 0; i < terrainLayers.Count; i++)
            {
                terrainAlbedoTextures.Insert(i, terrainLayers[i].diffuseTexture);
                terrainNormalTextures.Insert(i, terrainLayers[i].normalMapTexture);
            }
#endif
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
            var terrainSplats = new List<SplatPrototype>();
            terrainSplats.AddRange(terrain.terrainData.splatPrototypes);
            var terrainAlbedoTextures = new List<Texture2D>();
            var terrainNormalTextures = new List<Texture2D>();
            var originalSplatCount = terrainSplats.Count;

            for (var i = 0; i < terrainSplats.Count; i++)
            {
                terrainAlbedoTextures.Insert(i, terrainSplats[i].texture);
                terrainNormalTextures.Insert(i, terrainSplats[i].normalMap);
            }
#endif
            
            
            var selection = Selection.objects;
            foreach (var obj in selection)
            {
                var thisObject = obj as Texture2D;
                if ((thisObject == null))
                {
                    Debug.Log(obj.name + " skipped because not valid target");
                    if (EditorUtility.DisplayDialog(obj.name + " not added.", obj.name + " skipped because it is not a valid target", "Continue", "Cancel"))
                    {
                        continue;
                    }
                    else
                    {
                        return;
                    }
                }
                if (terrainAlbedoTextures.Contains(thisObject) || terrainNormalTextures.Contains(thisObject))
                {
                    var choice = EditorUtility.DisplayDialogComplex("Texture already Exists!", "What would you like to do?", "Replace", "Skip", "Duplicate");
                    switch (choice)
                    {
                        case (0):
                            Debug.Log("Replacing Texture");
                            var path = AssetDatabase.GetAssetPath(obj);
                            var settings = (TextureImporter)AssetImporter.GetAtPath(path);
                            if (settings.textureType == TextureImporterType.NormalMap)
                            {
                                terrainNormalTextures[terrainNormalTextures.IndexOf(obj as Texture2D)] = obj as Texture2D;
                            }
                            else if (settings.textureType == TextureImporterType.Default)
                            {
                                terrainAlbedoTextures[terrainAlbedoTextures.IndexOf(obj as Texture2D)] = obj as Texture2D;
                            }

                            break;
                        case (1):
                            Debug.Log("Skipping Texture");
                            continue;
                        case (2):
                            Debug.Log("Duplicating Texture");
                            break;
                    }
                }
                else
                {
                    var path = AssetDatabase.GetAssetPath(obj);
                    var settings = (TextureImporter)AssetImporter.GetAtPath(path);

                    switch (settings.textureType)
                    {
                        case TextureImporterType.NormalMap:
                            terrainNormalTextures.Add(thisObject);
                            break;
                        case TextureImporterType.Default:
                            terrainAlbedoTextures.Add(thisObject);
                            break;
                    }
                }
            }
            if (terrainAlbedoTextures.Count != originalSplatCount && terrainNormalTextures.Count > 1)
            {
                TerrainSplatManager.Init(terrain, terrainAlbedoTextures, terrainNormalTextures);
            }
            else
            {
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
                var newSplats = new List<SplatPrototype>();
                foreach (var albedo in terrainAlbedoTextures)
                {
                    var prototype = new SplatPrototype();
                    prototype.texture = albedo;
                    if (terrainNormalTextures.Count == 1)
                    {
                        prototype.normalMap = terrainNormalTextures[0];
                    }
                    newSplats.Add(prototype);
                }
                Undo.RegisterCompleteObjectUndo(terrain.gameObject, "AddNormalsToSplats");
                terrain.terrainData.splatPrototypes = newSplats.ToArray();
                terrain.Flush();
                terrain.terrainData.RefreshPrototypes();
#endif
#if UNITY_2018_3_OR_NEWER
                var newSplats = new List<TerrainLayer>();
                foreach (var albedo in terrainAlbedoTextures)
                {
                    var prototype = new TerrainLayer();
                    prototype.diffuseTexture = albedo;
                    if (terrainNormalTextures.Count == 1)
                    {
                        prototype.normalMapTexture = terrainNormalTextures[0];
                    }
                    newSplats.Add(prototype);
                }
                Undo.RegisterCompleteObjectUndo(terrain.gameObject, "AddNormalsToSplats");
                terrain.terrainData.terrainLayers = newSplats.ToArray();
                terrain.Flush();
                terrain.terrainData.RefreshPrototypes();
#endif
            }
        }
        public static void AddToTreePrototypes()
        {
            var terrain = (Terrain)FindObjectOfType(typeof(Terrain));
            var listOfTrees = new List<TreePrototype>();
            listOfTrees.AddRange(terrain.terrainData.treePrototypes);

            var treePrefabs = new List<GameObject>();


            foreach (var obj in listOfTrees)
            {
                treePrefabs.Add(obj.prefab);
            }
            var selection = Selection.objects;
            foreach (var obj in selection)
            {
                var thisObject = obj as GameObject;
                if ((thisObject == null))
                {
                    Debug.Log(obj.name + " skipped because not valid target");
                    if (EditorUtility.DisplayDialog(obj.name + " not added.", obj.name + " skipped because it is not a valid target", "Continue", "Cancel"))
                    {
                        continue;
                    }
                    else
                    {
                        return;
                    }
                }
                if (treePrefabs.Contains(thisObject))
                {
                    var choice = EditorUtility.DisplayDialogComplex("Tree already Exists!", "What would you like to do?", "Replace Prefab", "Cancel", "Make Copy");
                    switch (choice)
                    {
                        case (0):
                            Debug.Log("Replacing Prefab");
                            treePrefabs[treePrefabs.IndexOf(obj as GameObject)] = obj as GameObject;
                            break;
                        case (1):
                            Debug.Log("Cancelling Process");
                            continue;
                        case (2):
                            Debug.Log("Duplicating Prefab");
                            break;
                    }
                }
                else
                {
                    var prototype = new TreePrototype();
                    prototype.prefab = thisObject;
                    listOfTrees.Add(prototype);
                    Undo.RegisterCompleteObjectUndo(terrain, "AddTreeToTerrain");
                    terrain.terrainData.treePrototypes = listOfTrees.ToArray();
                    terrain.Flush();
                    terrain.terrainData.RefreshPrototypes();
                    EditorUtility.DisplayDialog("Success", obj.name + " successfully added to terrain as Tree" + ". The change may take a moment to take effect.", "Continue");

                }
            }
            terrain.terrainData.RefreshPrototypes();
        }
        public static void AddToDetail(DetailRenderMode mode, bool isMesh)
        {
            var terrain = (Terrain)FindObjectOfType(typeof(Terrain));
            var listOfDetails = new List<DetailPrototype>();
            listOfDetails.AddRange(terrain.terrainData.detailPrototypes);
            var detailPrefabs = new List<GameObject>();
            var grassTex = new List<Texture2D>();
            foreach (var obj in listOfDetails)
            {
                detailPrefabs.Add(obj.prototype);
                if (!isMesh)
                    grassTex.Add(obj.prototypeTexture);
            }
            var selection = Selection.objects;
            foreach (var obj in selection)
            {
                var thisObject = obj as GameObject;
                var thisTexture = obj as Texture2D;
                if ((thisObject == null && isMesh) || (thisTexture == null && !isMesh))
                {
                    if (EditorUtility.DisplayDialog(obj.name + " not added.", obj.name + " skipped because it is not a valid target", "Continue", "Cancel"))
                    {
                        continue;
                    }
                    else
                    {
                        return;
                    }
                }
                if (isMesh)
                {
                    foreach (var rend in thisObject.GetComponentsInChildren<Renderer>())
                    {
                        if (rend.sharedMaterials.Length >= 1) continue;
                        
                        if (EditorUtility.DisplayDialog(obj.name + " may cause issue", obj.name + " has multiple materials and will not display on terrain. Would you like to add to terrain anyway?", "Add Anyway", "Camcel"))
                        {
                            Debug.Log(obj.name + " added to terrain anyway");
                        }
                        else
                        {
                            Debug.Log(obj.name + " not added to terrain");
                            return;
                        }
                    }
                }
                if ((detailPrefabs.Contains(thisObject) && isMesh) || (grassTex.Contains(thisTexture) && !isMesh))
                {
                    var choice = EditorUtility.DisplayDialogComplex("Detail Object already Exists!", "What would you like to do?", "Replace Prefab", "Cancel", "Make Copy");
                    switch (choice)
                    {
                        case (0):
                            Debug.Log("Replacing Prefab");
                            if (isMesh)
                            {
                                detailPrefabs[detailPrefabs.IndexOf(obj as GameObject)] = obj as GameObject;
                            }
                            else
                            {
                                grassTex[grassTex.IndexOf(obj as Texture2D)] = obj as Texture2D;
                            }
                            break;
                        case (1):
                            Debug.Log("Cancelling Process");
                            continue;
                        case (2):
                            Debug.Log("Duplicating Prefab");
                            break;
                    }
                }
                var prototype = new DetailPrototype();
                prototype.renderMode = mode;
                prototype.usePrototypeMesh = isMesh;
                if (isMesh)
                {
                    prototype.prototype = obj as GameObject;
                    //if (fileExists)
                    //    prototype.prototype.name = GetUniqueName(prototype.prototype.name, protoNames);
                }
                else
                {
                    prototype.prototypeTexture = obj as Texture2D;
                    //if (fileExists)
                    //     prototype.prototypeTexture.name = GetUniqueName(prototype.prototype.name, protoNames);
                }
                listOfDetails.Add(prototype);
                Undo.RegisterCompleteObjectUndo(terrain, "AddDetailToTerrain");
                terrain.terrainData.detailPrototypes = listOfDetails.ToArray();
                terrain.Flush();
                terrain.terrainData.RefreshPrototypes();
                EditorUtility.DisplayDialog("Success", obj.name + " successfully added to terrain as " + mode + ". The change may take a moment to take effect.", "Continue");
            }
            terrain.Flush();
            terrain.terrainData.RefreshPrototypes();
        }
        public static void AddToBrushes()
        {
            if (Selection.objects.Length > 0 && EditorUtility.DisplayDialog("Add brushes to terrain?", "Adding these files will create duplicates and place them in the Assets/Gizmos folder and will only work on valid textures. This may require restarting Unity to take effect.", "Continue", "Cancel"))
            {
                var path = "/Gizmos/";
                CheckIfPathExists(path);
                foreach (var obj in Selection.objects)
                {
                    var texture = obj as Texture2D;
                    var texName = "brush";
                    if (texture == null)
                    {
                        Debug.Log(obj.name + " skipped because not valid target");
                        if (EditorUtility.DisplayDialog(obj.name + " not added.", obj.name + " skipped because it is not a valid target", "Continue", "Cancel"))
                        {
                            continue;
                        }
                        else
                        {
                            return;
                        }
                    }
                    var texPath = AssetDatabase.GetAssetPath(obj);
                    var texSettings = (TextureImporter)AssetImporter.GetAtPath(texPath);
                    var isReadable = texSettings.isReadable;
                    texSettings.isReadable = true;
                    var compression = texSettings.textureCompression;
                    texSettings.textureCompression = TextureImporterCompression.Uncompressed;
                    AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);

                    var validName = path + texName + "_" + 0;
                    var i = 0;

                    while (File.Exists(Application.dataPath + validName + ".png"))
                    {
                        validName = path + texName + "_" + i;
                        i++;
                    }
                    validName += ".png";
                    // Encode texture into PNG
                    var bytes = texture.EncodeToPNG();

                    File.WriteAllBytes(Application.dataPath + validName, bytes);

                    texSettings.isReadable = isReadable;
                    texSettings.textureCompression = compression;
                    AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);

                    Debug.Log(obj + " successfully added to terrain brushes");

                    /*string settingsPath = Application.dataPath + validName;//AssetDatabase.GetAssetPath(obj);

                    TextureImporter settings = (TextureImporter)TextureImporter.GetAtPath(settingsPath);

                    float timeOut = 0;
                    while (settings == null && timeOut < 1)
                    {
                        settings = (TextureImporter)TextureImporter.GetAtPath(settingsPath);
                        EditorUtility.DisplayProgressBar("Changing brush import settings", timeOut.ToString(), timeOut);
                        timeOut += .0001f;
                    }
                    if (settings == null)
                    {
                        Debug.Log("Settings still null");
                        return;
                    }

                    TextureImporterAlphaSource alphaSource = settings.alphaSource;
                    settings.alphaSource = TextureImporterAlphaSource.FromGrayScale;
                    bool transparency = settings.alphaIsTransparency;
                    settings.alphaIsTransparency = true;
                    AssetDatabase.ImportAsset(settingsPath, ImportAssetOptions.ForceUpdate);*///TODO figure out how to convert brushes import settings before it is imported
                }
            }
        }
        #endregion
        #region Helper Functions
        public static Object CreateNew(GameObject obj, string localPath)
        {
            var prefab = new Object();
            CheckIfPathExists(localPath);
            Undo.RegisterCompleteObjectUndo(obj, "MakePrefab");
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2            
            prefab = (AssetDatabase.LoadAssetAtPath(localPath, typeof(GameObject)) == null) ? 
                PrefabUtility.CreatePrefab(localPath, obj)://true
                AssetDatabase.LoadAssetAtPath(localPath, typeof(GameObject));//false
#endif
#if UNITY_2018_3_OR_NEWER
            prefab = (AssetDatabase.LoadAssetAtPath(localPath, typeof(GameObject)) == null) ? 
                PrefabUtility.SaveAsPrefabAsset(obj, localPath)://true
                AssetDatabase.LoadAssetAtPath(localPath, typeof(GameObject));//false
#endif
            /*if (AssetDatabase.LoadAssetAtPath(localPath, typeof(GameObject)) == null)
            {
                prefab = PrefabUtility.CreatePrefab(localPath, obj);
            }
            else
            {

                prefab = AssetDatabase.LoadAssetAtPath(localPath, typeof(GameObject));
            }*/
            Undo.RegisterCompleteObjectUndo(prefab, "MakePrefab");
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2          
            PrefabUtility.ReplacePrefab(obj, prefab, ReplacePrefabOptions.ConnectToPrefab);
#endif
#if UNITY_2018_3_OR_NEWER
            PrefabUtility.SavePrefabAsset(obj);
#endif
            return prefab;
        }
        /*private static void DuplicateAndApply()
        {
            if (!Application.isPlaying)
            {
                GameObject newObject = (GameObject)Instantiate(Resources.Load(_nameSAA));

                if (_selSAA != null)
                {
                    List<Component> newComponents = new List<Component>();
                    List<Component> removeComponents = new List<Component>();
                    List<Component> origComponents = new List<Component>();
                    List<Component> finalComponents = new List<Component>();

                    newComponents.AddRange(newObject.GetComponents<Component>());
                    origComponents.AddRange(_selSAA.GetComponents<Component>());
                    for (int i = 0; i < newComponents.Count; i++)
                    {
                        UnityEditorInternal.ComponentUtility.CopyComponent(newComponents[i]);
                        foreach (Component c in origComponents)
                        {
                            if (c.GetType() == newComponents[i].GetType())
                            {
                                //Undo.RegisterCompleteObjectUndo(c, "StopAndApply");
                                UnityEditorInternal.ComponentUtility.PasteComponentValues(c);
                                removeComponents.Add(newComponents[i]);
                                finalComponents.Add(c);
                            }
                        }

                    }

                    foreach (Component c in origComponents)
                    {
                        if (!finalComponents.Contains(c))
                        {

                            Undo.DestroyObjectImmediate(c);
                        }
                    }


                    foreach (Component c in removeComponents)
                    {
                        newComponents.Remove(c);
                    }

                    removeComponents.Clear();
                    foreach (Component c in newComponents)
                    {
                        UnityEditorInternal.ComponentUtility.CopyComponent(c);
                        //Undo.RegisterCompleteObjectUndo(_selSAA, "StopAndApply");
                        UnityEditorInternal.ComponentUtility.PasteComponentAsNew(_selSAA);
                    }

                    _selSAA.SetActive(newObject.activeInHierarchy);
                    Undo.DestroyObjectImmediate(newObject);

                }
                AssetDatabase.DeleteAsset(_locationSAA);
                newObject.name = _nameSAA;
                EditorApplication.update -= DuplicateAndApply;
            }
        }*/
        
        // Returns the center of transforms received. DEPRECATED IN FAVOR OF TRANSFORM[] EXTENSION
        /*private static Vector3 GetPivot (Transform[] transforms) 
        {
            //Create temporary vector3 for center.
            var center = Vector3.zero;
            //Check if the received array is null or if it is empty, if it is return center.
            if (transforms == null || transforms.Length == 0)
                return center;
            //set center to be object 0's position
            center = transforms[0].position;
            //If the array only has one object, return that objects position.
            if (transforms.Length == 1)
                return center;
            
            //Using LINQ find the center of all the Transform's positions found in the received array and assign the
            //value to the Vector3 center and then return that value.
            //center = transforms.Average(tran => tran.position); c#6 function
            
            //A custom Transform[] extension that returns the average of all positions in the array. Using this since
            //not everyone is using c#6
            center = transforms.AveragePosition();
            
            return center;
        }*/       
        public static void MakeThatPrefab(string localPath)
        {
            if (!localPath.EndsWith("/"))
                localPath += "/";
            var selection = Selection.gameObjects;
            if (selection.Length == 0) return;
            CheckIfPathExists(localPath);
            var  increment = 1 / selection.Length;
            var tempFloat = increment;
            foreach (GameObject obj in selection)
            {
                if (tempFloat < 1)
                {
                    EditorUtility.DisplayProgressBar("Prefab Maker Progress", "Current object: " + obj.name, tempFloat);
                    tempFloat += increment;
                }
                else
                    EditorUtility.ClearProgressBar();

                //EditorUtility.DisplayProgressBar("Prefab Progress", "words", 0);
                string tempPath = localPath + obj.name + ".prefab";
                if (AssetDatabase.LoadAssetAtPath(tempPath, typeof(GameObject)) == null)
                {
                    CreateNew(obj, tempPath);
                }
                else
                {
                    int i = (EditorUtility.DisplayDialogComplex("Override prefab?", "Please select an option", "Cancel", "Override", "Make Copy"));
                    switch (i)
                    {
                        case (0):
                            break;
                        case (1):
                            CreateNew(obj, tempPath);
                            break;
                        case (2):
                            tempPath = localPath + obj.name + "_Duplicate" + ".prefab";
                            CreateNew(obj, tempPath);
                            break;
                    }
                }
            }
        }
        public static void CheckIfPathExists(string path)
        {
            if (!path.StartsWith("Assets/")) path = "Assets/" + path;
            
            if (path.Contains('\\'))
            {
                path = path.Replace('\\', '/');
            }
            
            var split = path.Split('/');
            var checkedPath = "";
            var folder = "";
            for (var i = 0; i < split.Length; i++)
            {
                if (split[i] == "" || split[i] == " ") continue;
                
                if (i > 0)
                {
                    if (!AssetDatabase.IsValidFolder(checkedPath + split[i]))
                    {
                        var guid = AssetDatabase.CreateFolder(folder, split[i]);
                        AssetDatabase.GUIDToAssetPath(guid);
                    }
                }
                folder = checkedPath + split[i];
                checkedPath += split[i] + "/";
                

            }
        }
        private static string GetUniqueName(string name, List<string> myObject)
        {
            var validatedName = name;
            var tries = 1;
            while (myObject.Contains(validatedName))
            {
                validatedName = string.Format("{0} [{1}]", name, tries++);
            }
            return validatedName;
        }
        private static void SetSnappingScale(GameObject obj)
        {
            if (obj == null)
            {
                Debug.Log("this is null");
                FuzzyHelper.SetEditorPrefFloat("MoveSnapX", 1);
                FuzzyHelper.SetEditorPrefFloat("MoveSnapX", 1);
                FuzzyHelper.SetEditorPrefFloat("MoveSnapY", 1);
                FuzzyHelper.SetEditorPrefFloat("MoveSnapZ", 1);
                AutoSnap.snapValueX = 1;
                AutoSnap.snapValueY = 1;
                AutoSnap.snapValueZ = 1;
                return;
            }
            var scale = obj.GetComponent<Renderer>().bounds.extents * 2;
            //var scaleOffset = obj.transform.localScale;
            Debug.Log(scale);
            AutoSnap.snapValueX = scale.x;
            AutoSnap.snapValueY = scale.y;
            AutoSnap.snapValueZ = scale.z;
            FuzzyHelper.SetEditorPrefFloat("MoveSnapX", scale.x);
            FuzzyHelper.SetEditorPrefFloat("MoveSnapY", scale.y);
            FuzzyHelper.SetEditorPrefFloat("MoveSnapZ", scale.z);
            
            
        }
        
        //Used by Select and Remove and Remove all of Type. This function first clears the static requiredComponents
        //list loops through the component array it receives. Then if any of said components have requirements it adds
        //the required type to requiredComponents
        public static void CheckIfRequired(Component[] components, Type checkingType)
        {
            //Clear the static list requiredComponents
            requiredComponents.Clear();
            //Clear the static string used to store the names of the components that have requirements
            requirees = "";
            //Loop through each component in the received array
            foreach (var comp in components)
            {
                //Verify that the component is not null, if it is, move on to the next component
                if (comp == null) continue;
                //Store the Type of the current component to reduce Get calls to optimise performance
                var type = comp.GetType();
                //Check if the the Type or BaseType of the component has the Custom Attribute "RequireComponent".
                //If it does not, move on to the next component
                if (type.GetCustomAttributes(typeof(RequireComponent), true).Length == 0) continue;
                
                //Loop through the 
                foreach (RequireComponent required in type.GetCustomAttributes(typeof(RequireComponent), true))
                {
                    
                    if (required.m_Type0 == null) continue;
                    
                    if (required.m_Type0 == checkingType || checkingType == null)
                    {
                        if (required.m_Type0 != typeof(Transform))
                        {
                            if (!requiredComponents.Contains(required.m_Type0))
                            {
                                requiredComponents.Add(required.m_Type0);
                            }

                            if (!requirees.Contains(type.ToString()))
                                requirees += type + "\n";
                        }
                    }
                    
                    if (required.m_Type1 == null) continue;
                    if (required.m_Type1 == checkingType || checkingType == null)
                    {
                        if (required.m_Type1 != typeof(Transform))
                        {
                            if (!requiredComponents.Contains(required.m_Type1))
                            {
                                requiredComponents.Add(required.m_Type1);
                            }

                            if (!requirees.Contains(type.ToString()))
                                requirees += type + "\n";
                        }
                    }

                    if (required.m_Type2 == null) continue;
                    if (required.m_Type2 == checkingType || checkingType == null)
                    {
                        if (required.m_Type2 != typeof(Transform))
                        {
                            if (!requiredComponents.Contains(required.m_Type2))
                            {
                                requiredComponents.Add(required.m_Type2);
                            }

                            if (!requirees.Contains(type.ToString()))
                                requirees += type + "\n";
                        }
                    }
                }
            }
        }

        
        /*public static void ReplaceStrings(GameObject[] objs, string oldName, string newName)
        {
            foreach (var obj in objs)
            {
                if (obj == null) continue;
                if (!obj.name.Contains(oldName)) continue;
                Undo.RegisterCompleteObjectUndo(obj, "Rename");
                obj.name = obj.name.Replace(oldName, newName);
            }
        }*/
        
        

        /*public static void UpdateWindowListLength<T>(List<T> list, int length)
        {
            if(length>list.Count)
            {
                for(var i = list.Count; i<length; i++)
                {
                    list.Add(null);
                }
            }else
            {
                while (length != list.Count)
                {
                    list.RemoveAt(list.Count - 1);
                }
            }
        }*/
        /*[MenuItem("Change/Inpector")]
        public static void InspectorIsLock()
        {
            var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");
            var window = GetWindow(type);
            var windows = AppDomain.CurrentDomain.GetAssemblies();
            //window.titleContent.
            foreach (var w in windows)
            {
                var types = w.GetTypes();
                foreach (var t in types)
                {
                    if (t.IsSubclassOf(type))
                    {
                        if(t.GetProperty("isLocked") != null)
                            
                            Debug.Log("Has Locked");
                    }
                }
            }
            Debug.Log(windows.Length);
            return;
            if (!ActiveEditorTracker.sharedTracker.isLocked) return;
            if (window.titleContent.image != EditorGUIUtility.IconContent("InspectorLock").image)
            {
                window.titleContent.image = EditorGUIUtility.IconContent("InspectorLock").image;
            }
            else
            {
                window.titleContent.image = EditorGUIUtility.IconContent("UnityEditor.InspectorWindow").image;;
            }

            //Debug.Log(window.titleContent.text);
            //var info = type.GetProperty("isLocked", BindingFlags.Public | BindingFlags.Instance);
            //return (bool)info.GetValue(window, null);
        }*/
        /*[MenuItem("Test/Test")]
        public static void GetInspector()
        {
            if (ActiveEditorTracker.sharedTracker.isLocked)
            {
                Debug.Log(ActiveEditorTracker.sharedTracker.inspectorMode);
            }
        }*/
        #endregion
        #region Validators
        /*#region MayaHotKeys
        [MenuItem("Window/FuzzyTools/MayaHotkeys/Hide", true)]
        private static bool HideSelectionValidation()
        {
            if (Selection.gameObjects.Length > 0)
                return true;
            return false;
        }
        [MenuItem("Window/FuzzyTools/MayaHotkeys/Group", true)]
        private static bool GroupSelectionValidation()
        {
            if (Selection.gameObjects.Length > 0)
                return true;
            return false;
        }
        [MenuItem("Window/FuzzyTools/MayaHotkeys/Parent", true)]
        private static bool ParentSelectionValidation()
        {
            if (Selection.gameObjects.Length > 1)
                return true;
            return false;
        }
        [MenuItem("Window/FuzzyTools/MayaHotkeys/UnParent", true)]
        private static bool UnParentSelectionValidation()
        {
            foreach (var obj in Selection.gameObjects)
            {
                if (obj.transform.parent != null)
                {
                    return true;
                }
            }

            return false;
        }
        [MenuItem("Window/FuzzyTools/MayaHotkeys/Solo", true)]
        private static bool SoloSelectionValidation()
        {
            if (Selection.gameObjects.Length > 0)
                return true;
            return false;
        }
        #endregion
        #region MayaFunctions
        [MenuItem("Window/FuzzyTools/MayaFunctions/MatchPosition", true)]
        private static bool MatchPositionValidation()
        {
            GameObject[] objs = Selection.gameObjects;
            GameObject obj = Selection.activeGameObject;
            if (objs.Length < 2) return false;
            foreach (var o in objs)
            {
                if (o.transform.position != obj.transform.position) return true;
            }
            return false;
        }
        [MenuItem("Window/FuzzyTools/MayaFunctions/MatchRotation", true)]
        private static bool MatchRotationValidation()
        {
            GameObject[] objs = Selection.gameObjects;
            GameObject obj = Selection.activeGameObject;
            if (objs.Length < 2) return false;
            foreach (var o in objs)
            {
                if (o.transform.rotation != obj.transform.rotation) return true;
            }
            return false;
        }
        [MenuItem("Window/FuzzyTools/MayaFunctions/MatchLocalScale", true)]
        private static bool MatchScaleValidation()
        {
            var objs = Selection.gameObjects;
            var obj = Selection.activeGameObject;
            if (objs.Length < 2) return false;
            foreach (var o in objs)
            {
                if (o.transform.localScale != obj.transform.localScale) return true;
            }
            return false;
        }
        [MenuItem("Window/FuzzyTools/MayaFunctions/MatchTransform", true)]
        private static bool MatchTransformValidation()
        {
            GameObject[] objs = Selection.gameObjects;
            GameObject obj = Selection.activeGameObject;
            if (objs.Length < 2) return false;
            foreach (var o in objs)
            {
                if (o.transform.position != obj.transform.position || o.transform.rotation != obj.transform.rotation ||
                    o.transform.localScale != obj.transform.localScale)
                    return true;
            }

            

            return false;
        }
        [MenuItem("Window/FuzzyTools/MayaFunctions/RemoveAllAttributes",true)]
        private static bool RemoveAttrHotkeyValidation()
        {
            GameObject[] objs = Selection.gameObjects;
            if (objs.Length == 0) return false;
            foreach (var obj in objs)
            {
                if (obj.GetComponents<Component>().Length > 1)
                {
                    return true;
                }
            }
            return false;
        }
        
        #endregion*/
        #region CONTEXT Validators
        [MenuItem("CONTEXT/Transform/Stop And Apply", true), System.Obsolete("This validator is useless because the context function has been dedprecated in exchange for the gameobject version")]
        private static bool StopAndApplyValidation()
        {
            return EditorApplication.isPlaying;
        }
        [MenuItem("CONTEXT/Transform/Transfer All Attributes", true), System.Obsolete("This validator is useless because the context function has been dedprecated in exchange for the gameobject version")]
        private static bool TransferAttrValidation()
        {
            if (Selection.gameObjects.Length == 2 && Selection.activeGameObject.GetComponents<Component>().Length > 1)
            {
                return true;
            }
            return false;
        }
        [MenuItem("CONTEXT/Rigidbody/Freeze", true)]
        private static bool FreezeRigidValidation(MenuCommand command)
        {
            Rigidbody t = (Rigidbody)command.context;
            if (t.mass == 0 && t.angularDrag == 0 && t.useGravity == false &&
                t.isKinematic == true && t.constraints == RigidbodyConstraints.FreezeAll)
            {
                return false;
            }
            return true;

        }
        [MenuItem("CONTEXT/Object/Cut Component", true)]
        private static bool CutComponentValidation(MenuCommand command)
        {
            var component = command.context as Component;
            if (component == null || component.GetType() == typeof(Transform)) return false;
            
            var components = component.gameObject.GetComponents<Component>();
            CheckIfRequired(components, component.GetType());
            if (requiredComponents.Count == 0 || !requiredComponents.Contains(component.GetType())) return true;

            return false;
        }
        [MenuItem("CONTEXT/Object/Remove All Components of Type", true)]
        private static bool RemoveOfTypeValidation(MenuCommand command)
        {
            var component = (command.context as Component);
            if (component != null && component.GetType() != typeof(Transform))
            {
                return true;
            }

            return false;
        }

        #endregion
        #region GameObject Validators
        [MenuItem("GameObject/FuzzyTools/Transfer All Attributes", true)]
        static bool TransferAttr2Validation()
        {
            if (Selection.gameObjects.Length == 2 && Selection.activeGameObject.GetComponents<Component>().Length > 1)
            {
                return true;
            }
            return false;
        }
        [MenuItem("GameObject/FuzzyTools/Alphabetize Children", true)]
        static bool AlphabetizeChildrenValidation(MenuCommand command)
        {
            if (Selection.gameObjects.Length > 0)
            {
                foreach (GameObject obj in Selection.gameObjects)
                {
                    if (obj.transform.childCount > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        [MenuItem("GameObject/FuzzyTools/MakePrefab", true)]
        static bool MakePrefabValidation()
        {
            if (Selection.gameObjects.Length != 0)
            {
                return true;
            }
            return false;
        }
        [MenuItem("GameObject/FuzzyTools/Select And Remove Components", true)]
        static bool SelectAndRemoveComponentsValidation()
        {
            if(Selection.gameObjects.Length == 1 && Selection.activeGameObject.GetComponents<Component>().Length > 1)
            {
                return true;
            }
            return false;
        }
        [MenuItem("GameObject/FuzzyTools/Replace Shader on all Materials", true)]
        static bool GameObjectReplaceMaterialsValidation()
        {
            var objs = Selection.gameObjects;
            if (objs.Length == 0)
            {
                return false;
            }
            var mats = new List<Material>();
            foreach (var obj in objs)
            {
                if (obj.GetComponent<Renderer>() == null) continue;
                mats.AddRange(obj.GetComponent<Renderer>().sharedMaterials);
            }

            if (mats.Count > 0)
            {
                return true;
            }

            return false;
        }
        [MenuItem("GameObject/FuzzyTools/Colliders/Toggle Children Colliders", true)]
        static bool ToggleChildrenCollidersValidation()
        {
            if (Selection.gameObjects.Length > 0)
            {
                foreach (GameObject obj in Selection.gameObjects)
                {
                    if (obj.GetComponentInChildren<Collider>() != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        //Validator for the CreateGetter function.
        [MenuItem("GameObject/FuzzyTools/Custom Attributes/Create GetAllComponents", true)]
        private static bool CreateGetterValidation()
        {
            //Search entire scene for existence of the script "MonoBehaviourGetAllComponents" return if found or not.
            return (FindObjectOfType(typeof(MonoBehaviourGetAllComponents)) == null);
        }
        
        //Validator for Converting Terrain To Model
        [MenuItem("GameObject/FuzzyTools/Convert/Terrain To Model", true)]
        private static bool OpenTerrainToOBJGameObjectValidation()
        {
            if (Selection.activeGameObject == null) return false;
            var terrain = Selection.activeGameObject.GetComponent<Terrain>();
            return terrain != null;
        }
        
        #endregion
        #region Assets Verifiers
        [MenuItem("Assets/FuzzyTools/Terrain/Add To Terrain", true)]
        private static bool AddToTerrainValidation()
        {
            if (Selection.objects.Length > 0)
            {
                foreach (var obj in Selection.objects)
                {
                    GameObject myObj = obj as GameObject;
                    Texture2D myTex = obj as Texture2D;
                    if (myObj != null || myTex != null)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        [MenuItem("Assets/FuzzyTools/Convert/Image(s)", true)]
        private static bool ConvertImageValidation()
        {
            foreach (var obj in Selection.objects)
            {
                var texture = obj as Texture2D;
                if (texture != null)
                {
                    return true;
                }
            }

            return false;
        }
        [MenuItem("Assets/FuzzyTools/Convert/Model to Terrain", true)]
        private static bool OpenObjToObjAssetsValidation()
        {
            var obj = Selection.activeObject as GameObject;
            return obj != null ? obj.GetComponentInChildren<Renderer>() : false;
        }
        [MenuItem("GameObject/FuzzyTools/Convert/Model to Terrain", true)]
        private static bool OpenObjToTerrainGameObjectValidation()
        {
            var obj = Selection.activeGameObject;
            return obj != null ? obj.GetComponentInChildren<Renderer>() : false;
        }
        [MenuItem("Assets/FuzzyTools/Convert/Model(s)", true)]
        private static bool ConvertModelValidation()
        {
            var mesh = Selection.activeObject as Mesh;
            GameObject obj = null;
            if (mesh == null)
            {
                obj = Selection.activeObject as GameObject;
            }

            return (mesh != null || obj != null);
        }
        [MenuItem("Assets/FuzzyTools/Convert/Terrain To Model", true)]
        private static bool OpenTerrainToOBJAssetsValidation()
        {
            var terrain = Selection.activeObject as TerrainData;
            if (terrain == null) return false;
            return true;
        }      
        [MenuItem("Assets/FuzzyTools/Replace Shaders", true)]
        static bool AssetsReplaceMaterialsValidation()
        {
            Object[] objs = Selection.objects;
            if (objs.Length == 0)
            {
                return false;
            }
            List<Material> mats = new List<Material>();
            foreach (var obj in objs)
            {
                Material mat = obj as Material;
                if (mat != null)
                {
                    mats.Add((mat));
                }
            }
            if (mats.Count != 0)
            {
                return true;
            }

            return false;
        }
        [MenuItem("Assets/FuzzyTools/Adjust Snapping Grid", true)]
        private static bool AssetsAdjSnapGridValidation()
        {
            GameObject obj = Selection.activeGameObject;
            
            if (obj != null && obj.GetComponent<Renderer>())
            {
                return true;
            }

            return false;
        }
        
        
        #endregion

        #region MyRegion
        [MenuItem("File/Open Previous Scene", true)]
        private static bool ReopenLastSceneValidation()
        {
            if (previousScene != SceneManager.GetActiveScene().path && previousScene != "")
            {
                return true;
            }
            return false;
        }
        [MenuItem("File/Recent Scenes", true)]
        private static bool OpenRecentScenesValidation()
        { 
            if(recentScenes[0]!= null)
            {
                return true;
            }
            return false;
        }
        #endregion
        #region Terrain Context Verifiers
        [MenuItem("CONTEXT/Terrain/Clear Splatmaps", true)]
        static bool ClearSplatMapsValiidation(MenuCommand command)
        {
            var t = command.context as Terrain;
            if (t != null)
            {
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
                return t.terrainData.splatPrototypes.Length > 0;
#endif
#if UNITY_2018_3_OR_NEWER
                return t.terrainData.terrainLayers.Length > 0;
#endif


            }
            else return false;
        }
        [MenuItem("CONTEXT/Terrain/Clear Trees", true)]
        static bool ClearTreesValidation(MenuCommand command)
        {
            Terrain t = command.context as Terrain;
            if (t != null && t.terrainData.treePrototypes.Length > 0)
            {
                return true;
            }
            return false;
        }
        [MenuItem("CONTEXT/Terrain/Clear Detail Meshes", true)]
        static bool ClearDetailMeshValidation(MenuCommand command)
        {
            Terrain t = command.context as Terrain;
            if (t != null && t.terrainData.detailPrototypes.Length > 0)
            {

//                List<DetailPrototype> grassDetails = new List<DetailPrototype>();
                foreach (DetailPrototype proto in t.terrainData.detailPrototypes)
                {
                    if (proto.usePrototypeMesh)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        [MenuItem("CONTEXT/Terrain/Clear Grasses", true)]
        static bool CleaGrassesValidation(MenuCommand command)
        {
            Terrain t = command.context as Terrain;
            if (t != null && t.terrainData.detailPrototypes.Length > 0)
            {
//                List<DetailPrototype> meshDetails = new List<DetailPrototype>();
                foreach (DetailPrototype proto in t.terrainData.detailPrototypes)
                {
                    if (!proto.usePrototypeMesh)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        [MenuItem("CONTEXT/Terrain/Remove And Show Problematic Details", true)]
        static bool RemoveShowProblemDetailsValidation(MenuCommand command)
        {
            Terrain t = command.context as Terrain;
            if (t != null && t.terrainData.detailPrototypes.Length > 0)
            {
                foreach (DetailPrototype proto in t.terrainData.detailPrototypes)
                {
                    if (proto.usePrototypeMesh)
                    {
                        foreach (Renderer rend in proto.prototype.GetComponentsInChildren<Renderer>())
                        {
                            if (rend.sharedMaterials.Length != 1)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        #endregion
        #endregion
    }
    #region EditorWindows

    public class Settings : EditorWindow
    {
        private bool _grouping;
        private bool _recentScenes;
        private bool _terrainSplitter;
        private bool _removeComponents;
        private bool _physicsSimulator;
        private bool _makePrefab;
        private bool _locators;
        private bool _renaming;
        private bool _converting;
        private bool _hierarchy;
        
        private static Vector2 scrollPos = Vector2.zero;

        private int _ignoreTypesCount;
        
        
        private static string prevLocatorName = "";

        private static GUIStyle _boldtext = new GUIStyle();
        private static GUIStyle _headder = new GUIStyle();
        
        public static void Init()
        {
            scrollPos = Vector2.zero;
            var window = GetWindow(typeof(Settings));
            window.name = "Settings";
            window.minSize = new Vector2(260, 300);
            _boldtext.fontStyle = FontStyle.Bold;
            _headder.fontSize = 15;
            _headder.fontStyle = FontStyle.Bold;
            prevLocatorName = FuzzyTools.locatorName;
        }

        private void UpdateList(List<Type> list, int length)
        {
            if(length>list.Count)
            {
                for(var i = list.Count; i<length; i++)
                {
                    list.Add(null);
                }
            }else
            {
                while (length != list.Count)
                {
                    list.RemoveAt(list.Count - 1);
                }
            }
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("FuzzyTools Preferences", _headder);
            EditorGUILayout.Space();
            EditorGUILayout.Separator();
            _grouping = EditorGUILayout.Foldout(_grouping, "Group Settings");
            if (_grouping)
            {
                EditorGUILayout.BeginVertical("box");
                FuzzyTools.defaultGroupName =
                    EditorGUILayout.TextField("Default Group Name", FuzzyTools.defaultGroupName);
                FuzzyHelper.SetEditorPrefString("DefaultGroupName", FuzzyTools.defaultGroupName);
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.Separator();
            _recentScenes = EditorGUILayout.Foldout(_recentScenes, "Recent Scenes Settings");
            if (_recentScenes)
            {
                EditorGUILayout.BeginVertical("box");
                FuzzyTools.keepTrackOfRecentScenes =
                    EditorGUILayout.IntField("Tracked Scenes", FuzzyTools.keepTrackOfRecentScenes);
                FuzzyHelper.SetEditorPrefInt("TrackedScenesCount", FuzzyTools.keepTrackOfRecentScenes);
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.Separator();
            _terrainSplitter = EditorGUILayout.Foldout(_terrainSplitter, "Terrain Splitter Settings");
            if (_terrainSplitter)
            {
                EditorGUILayout.BeginVertical("box");
                FuzzyTools.maxTerrainSplit = EditorGUILayout.IntField("Max Terrain Slices", FuzzyTools.maxTerrainSplit);
                FuzzyHelper.SetEditorPrefInt("MaxTerrainSlices", FuzzyTools.maxTerrainSplit);
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.Separator();
            _removeComponents = EditorGUILayout.Foldout(_removeComponents, "Remove Component Settings");
            if (_removeComponents)
            {
                EditorGUILayout.BeginVertical("box");
                _ignoreTypesCount = EditorGUILayout.IntField("Ignore Types", _ignoreTypesCount);
                UpdateList(FuzzyTools.ignoreTypes, _ignoreTypesCount);
                FuzzyHelper.SetEditorPrefInt("IgnoreTypesCount", _ignoreTypesCount);
                for (var i = 0; i < _ignoreTypesCount; i++)
                {
                    if (i >= FuzzyTools.ignoreTypes.Count) break;
                    var type = Type.GetType(EditorGUILayout.TextField(FuzzyTools.ignoreTypes[i].ToString(),
                        FuzzyTools.ignoreTypes[i].ToString()), false, true);
                    if (type == null) continue;
                    FuzzyTools.ignoreTypes[i] = type;
                    FuzzyHelper.SetEditorPrefString(("IgnoreType" + i), FuzzyTools.ignoreTypes[i].ToString());
                }
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.Separator();
            _physicsSimulator = EditorGUILayout.Foldout(_physicsSimulator, "Physics Simulator Settings");
            if (_physicsSimulator)
            {
                EditorGUILayout.BeginVertical("box");
                FuzzyTools.maxPhysicsIterations =
                    EditorGUILayout.IntField("Max Physics Iterations", FuzzyTools.maxPhysicsIterations);
                FuzzyHelper.SetEditorPrefInt("MaxPhysicsIterations", FuzzyTools.maxPhysicsIterations);
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.Separator();
            _makePrefab = EditorGUILayout.Foldout(_makePrefab, "Make Prefab Settings");
            if (_makePrefab)
            {
                EditorGUILayout.BeginVertical("box");
                FuzzyTools.subLocation = EditorGUILayout.TextField("Default Prefab Location", FuzzyTools.subLocation);
                FuzzyHelper.SetEditorPrefString("PrefabLocation", FuzzyTools.subLocation);
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.Separator();
            _locators = EditorGUILayout.Foldout(_locators, "Locator Settings");
            if (_locators)
            {
                EditorGUILayout.BeginVertical("box");
                FuzzyTools.locatorName = EditorGUILayout.TextField("Locator Name", FuzzyTools.locatorName);
                FuzzyHelper.SetEditorPrefString("LocatorName", FuzzyTools.locatorName);
                
                FuzzyTools.locatorColor = EditorGUILayout.ColorField("Locator Color", FuzzyTools.locatorColor);
                FuzzyHelper.SetEditorPrefColor("LocatorColor", FuzzyTools.locatorColor);
                
                FuzzyTools.locatorScale = EditorGUILayout.FloatField("Locator Size", FuzzyTools.locatorScale);
                FuzzyHelper.SetEditorPrefFloat("LocatorSize", FuzzyTools.locatorScale);
                if (prevLocatorName != FuzzyTools.locatorName && GUILayout.Button("Rename Active Locators"))
                {
                    RenameLocators();
                    prevLocatorName = FuzzyTools.locatorName;
                }
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.Separator();
            _renaming = EditorGUILayout.Foldout(_renaming, "Renaming Settings");
            if (_renaming)
            {
                EditorGUILayout.BeginVertical("box");
                FuzzyTools.showAllGameObjectsOfName =
                    EditorGUILayout.Toggle("Show Search Results", FuzzyTools.showAllGameObjectsOfName);
                FuzzyHelper.SetEditorPrefBool("ShowAllGameObjectsOfName", FuzzyTools.showAllGameObjectsOfName);

                FuzzyTools.autoAddSpaces = EditorGUILayout.Toggle("Add spaces when renaming", FuzzyTools.autoAddSpaces);
                FuzzyHelper.SetEditorPrefBool("AutoAddSpaces", FuzzyTools.autoAddSpaces);
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.Separator();
            _converting = EditorGUILayout.Foldout(_converting, "Converting Settings");
            if (_converting)
            {
                EditorGUILayout.BeginVertical("box");
                FuzzyTools.DefaultTerrainPath =
                    EditorGUILayout.TextField("Converted Terrain Path", FuzzyTools.DefaultTerrainPath);
                if (FuzzyTools.DefaultTerrainPath.Length > 0 && !FuzzyTools.DefaultTerrainPath.EndsWith("/"))
                    FuzzyTools.DefaultTerrainPath += "/";
                FuzzyHelper.SetEditorPrefString("ConvertTerrainPath", FuzzyTools.DefaultTerrainPath);

                FuzzyTools.DefaultImagePath =
                    EditorGUILayout.TextField("Converted Image Path", FuzzyTools.DefaultImagePath);
                if (FuzzyTools.DefaultImagePath.Length > 0 && !FuzzyTools.DefaultImagePath.EndsWith("/"))
                    FuzzyTools.DefaultImagePath += "/";
                FuzzyHelper.SetEditorPrefString("ConvertImagePath", FuzzyTools.DefaultImagePath);

                FuzzyTools.DefaultModelPath =
                    EditorGUILayout.TextField("Converted Model Path", FuzzyTools.DefaultModelPath);
                if (FuzzyTools.DefaultModelPath.Length > 0 && !FuzzyTools.DefaultModelPath.EndsWith("/"))
                    FuzzyTools.DefaultModelPath += "/";
                FuzzyHelper.SetEditorPrefString("ConvertModelPath", FuzzyTools.DefaultModelPath);

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Currently OBJ is the only supported model format.");
                EditorGUILayout.LabelField("However we hope to support FBX in the future");
                FuzzyTools.DefaultModelFormat =
                    (ModelFormat) EditorGUILayout.EnumPopup("Convert Format", FuzzyTools.DefaultModelFormat);
                FuzzyHelper.SetEditorPrefInt("ConvertFormat", (int)FuzzyTools.DefaultModelFormat);
                EditorGUILayout.EndVertical();

                FuzzyTools.DefaultTopologyMode =
                    (TopologyMode) EditorGUILayout.EnumPopup("Default Topology",
                        FuzzyTools.DefaultTopologyMode);
                FuzzyHelper.SetEditorPrefInt("DefaultTopologyMode", (int)FuzzyTools.DefaultTopologyMode);

                FuzzyTools.DefaultMeshResolution =
                    (Resolution) EditorGUILayout.EnumPopup("Default Mesh Resolution",
                        FuzzyTools.DefaultMeshResolution);
                FuzzyHelper.SetEditorPrefInt("DefaultMeshResolution", (int)FuzzyTools.DefaultMeshResolution);
                EditorGUILayout.EndVertical();
            }
            
            
            EditorGUILayout.Separator();
            _hierarchy = EditorGUILayout.Foldout(_hierarchy, "Hierarchy Settings");
            if (_hierarchy)
            {
                EditorGUILayout.BeginVertical("box");
                FuzzyTools.colorMode = 
                    (CustomColorType)EditorGUILayout.EnumPopup("ColorMode", FuzzyTools.colorMode);
                FuzzyHelper.SetEditorPrefInt("colorMode", (int)FuzzyTools.colorMode);
                switch (FuzzyTools.colorMode)
                {
                    case CustomColorType.AutoColors:
                        FuzzyTools.gameObjectFontColor =
                            EditorGUILayout.ColorField("Default Font Color", FuzzyTools.gameObjectFontColor);
                        FuzzyHelper.SetEditorPrefColor("gameObjectFontColor", FuzzyTools.gameObjectFontColor);
                        
                        FuzzyTools.prefabOrgFontColor =
                            EditorGUILayout.ColorField("Default Prefab Font Color", FuzzyTools.prefabOrgFontColor);
                        FuzzyHelper.SetEditorPrefColor("prefabOrgFontColor", FuzzyTools.prefabOrgFontColor);
                        
                        FuzzyTools.inActiveColor =
                            EditorGUILayout.ColorField("Inactive Background Color", FuzzyTools.inActiveColor);
                        FuzzyHelper.SetEditorPrefColor("inActiveColor", FuzzyTools.inActiveColor);
                        
                        FuzzyTools.inActiveFontColor =
                            EditorGUILayout.ColorField("Inactive Font Color", FuzzyTools.inActiveFontColor);
                        FuzzyHelper.SetEditorPrefColor("inActiveFontColor", FuzzyTools.inActiveFontColor);
                        
                        FuzzyTools.standardFont = 
                            (FontStyle)EditorGUILayout.EnumPopup("Default Font", FuzzyTools.standardFont);
                        FuzzyHelper.SetEditorPrefInt("standardFont", (int)FuzzyTools.standardFont);
                        
                        FuzzyTools.prefebFont = 
                            (FontStyle)EditorGUILayout.EnumPopup("Prefab Font", FuzzyTools.prefebFont);
                        FuzzyHelper.SetEditorPrefInt("prefebFont", (int)FuzzyTools.prefebFont);
                        
                        FuzzyTools.autoInvertColors =
                            EditorGUILayout.Toggle("Auto Invert Font", FuzzyTools.autoInvertColors);
                        FuzzyHelper.SetEditorPrefBool("autoInvertColors", FuzzyTools.autoInvertColors);
                        
                        break;
                    case CustomColorType.CustomColors:
                        FuzzyTools.uniformChangeColors = EditorGUILayout.Toggle("Uniform Change Selection",
                            FuzzyTools.uniformChangeColors);
                        FuzzyHelper.SetEditorPrefBool("UniformChangeColors", FuzzyTools.uniformChangeColors);
                        if (GUILayout.Button("Clear Custom Colors"))
                        {
                            FuzzyTools.iDToBackgroundColor.Clear();
                            FuzzyTools.iDToFontColor.Clear();
                            FuzzyTools.iDToFontStyle.Clear();
                        }
                        break;
                    case CustomColorType.Hierarchy:
                        FuzzyTools.hierarchyColor1 =
                            EditorGUILayout.ColorField("First Color", FuzzyTools.hierarchyColor1);
                        FuzzyHelper.SetEditorPrefColor("hierarchyColor1", FuzzyTools.hierarchyColor1);
                        
                        FuzzyTools.hierarchyColor2 =
                            EditorGUILayout.ColorField("Second Color", FuzzyTools.hierarchyColor2);
                        FuzzyHelper.SetEditorPrefColor("hierarchyColor2", FuzzyTools.hierarchyColor2);
                        
                        FuzzyTools.hierarchyColor3 =
                            EditorGUILayout.ColorField("Third Color", FuzzyTools.hierarchyColor3);
                        FuzzyHelper.SetEditorPrefColor("hierarchyColor3", FuzzyTools.hierarchyColor3);
                        
                        FuzzyTools.hierarchyColor4 =
                            EditorGUILayout.ColorField("Fourth Color", FuzzyTools.hierarchyColor4);
                        FuzzyHelper.SetEditorPrefColor("hierarchyColor4", FuzzyTools.hierarchyColor4);
                        
                        FuzzyTools.hierarchyColor5 =
                            EditorGUILayout.ColorField("Fifth Color", FuzzyTools.hierarchyColor5);
                        FuzzyHelper.SetEditorPrefColor("hierarchyColor5", FuzzyTools.hierarchyColor5);
                        
                        break;
                    case CustomColorType.VariedColor:
                        FuzzyTools.PrimaryColor =
                            EditorGUILayout.ColorField("Primary Color", FuzzyTools.PrimaryColor);
                        FuzzyHelper.SetEditorPrefColor("primaryColor", FuzzyTools.PrimaryColor);
                        
                        FuzzyTools.secondaryColor =
                            EditorGUILayout.ColorField("Secondary Color", FuzzyTools.secondaryColor);
                        FuzzyHelper.SetEditorPrefColor("secondaryColor", FuzzyTools.secondaryColor);
                        
                        break;
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }

        
        

        private void RenameLocators()
        {
            var objects = FindObjectsOfType<GameObject>();
            foreach (var obj in objects)
            {
                if (obj.name != prevLocatorName) continue;
                obj.name = FuzzyTools.locatorName;

            }
        }
    }
    public class MakePrefabPopup : EditorWindow
    {
        private string prefabFolder = "Assets/";
        
        public static void Init()
        {
            var window = GetWindow(typeof(MakePrefabPopup));
            window.titleContent.text = "PrefabMaker";
            window.position = new Rect(Screen.width, Screen.height, 400, 100);
            window.ShowPopup();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("If you want your prefab to be in a folder inside 'Assets/' enter the location here." 
                , EditorStyles.wordWrappedLabel);
            FuzzyTools.subLocation = EditorGUILayout.TextField(FuzzyTools.subLocation);
            GUILayout.Space(20);
            if (GUILayout.Button("Confirm"))
            {
                //FuzzyTools.CheckIfPathExists(prefabFolder + subLocation);
                FuzzyTools.MakeThatPrefab(prefabFolder + FuzzyTools.subLocation);
                Close();
            }
        }
    }
    public class ShowTerrainPrefabAdder : EditorWindow
    {
        public TerrainPrefabType type;
        static EditorWindow window;
        bool scaleReset = false;
        bool setDetailsScale = false;
        
        public static void Init()
        {
            window = GetWindow(typeof(ShowTerrainPrefabAdder));
            window.titleContent.text = "TerrainPrefabAdder";
            window.position = new Rect(mouseOverWindow.position.x, mouseOverWindow.position.y, 400, 100);
            window.ShowPopup();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Please select type of asset you would like to add.", EditorStyles.wordWrappedLabel);
            type = (TerrainPrefabType)EditorGUILayout.EnumPopup(type);
            DetailRenderMode mode = DetailRenderMode.Grass;
            if (type == TerrainPrefabType.Details || type == TerrainPrefabType.Grass)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Please select detail render mode", EditorStyles.wordWrappedLabel);
                mode = (DetailRenderMode)EditorGUILayout.EnumPopup(mode);
                if (!setDetailsScale)
                {
                    scaleReset = false;
                    setDetailsScale = true;
                    window.position = new Rect(window.position.x, window.position.y, 400, 140);
                }
            }
            else
            {
                if (!scaleReset)
                {
                    scaleReset = true;
                    setDetailsScale = false;
                    window.position = new Rect(window.position.x, window.position.y, 400, 100);
                }
            }
            GUILayout.Space(20);
            if (GUILayout.Button("Confrim"))
            {
                switch (type)
                {
                    case (TerrainPrefabType.Texture):
                        FuzzyTools.AddToSplatPrototypes();
                        break;
                    case (TerrainPrefabType.Tree):
                        FuzzyTools.AddToTreePrototypes();
                        break;
                    case (TerrainPrefabType.Grass):
                        FuzzyTools.AddToDetail(mode, false);
                        break;
                    case (TerrainPrefabType.Details):
                        FuzzyTools.AddToDetail(mode, true);
                        break;
                    case (TerrainPrefabType.Brush):
                        FuzzyTools.AddToBrushes();
                        break;
                }

                this.Close();
            }
        }
    }
    public class TerrainSplatManager : EditorWindow
    {
        static EditorWindow window;

        static List<Texture2D> albedos = new List<Texture2D>();
        static List<Texture2D> normals = new List<Texture2D>();
        static List<int> normalCount = new List<int>();
        static List<bool> increaseWindow = new List<bool>();
        static Terrain terrain = null;
        [ExecuteInEditMode]
        public static void Init(Terrain thisTerrain, List<Texture2D> albedoMaps, List<Texture2D> normalMaps)
        {


            window = GetWindow(typeof(TerrainSplatManager));
            window.titleContent.text = "SplatManager";
            window.position = new Rect(mouseOverWindow.position.x, mouseOverWindow.position.y, 400, 100);

            normals = normalMaps;
            albedos = albedoMaps;
            terrain = thisTerrain;
            for (int i = 0; i < albedos.Count; i++)
            {
                normalCount.Add(i);
                increaseWindow.Add(true);
            }
            window.ShowPopup();
        }

        void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Please select the normal map for each texture.", EditorStyles.wordWrappedLabel);

            List<string> normalNames = new List<string>();
            foreach (Texture2D tex in normals)
                normalNames.Add(tex.name);
            for (int i = 0; i < albedos.Count; i++)
            {
                EditorGUILayout.SelectableLabel(albedos[i].name);
                normalCount[i] = EditorGUILayout.Popup(normalCount[i], normalNames.ToArray());
                if (increaseWindow[i])
                {
                    increaseWindow[i] = false;
                    float height = window.position.height + 40;
                    window.position = new Rect(window.position.x, window.position.y, window.position.width, height);
                }
            }
            GUILayout.Space(20);
            if (GUILayout.Button("Confrim"))
            {
                SetNormals();

            }
        }

        void SetNormals()
        {
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
            var splatProtos = new List<SplatPrototype>();
            for (int i = 0; i < albedos.Count; i++)
            {
                SplatPrototype prototype = new SplatPrototype();
                prototype.texture = albedos[i];
                prototype.normalMap = normals[normalCount[i]];
                splatProtos.Insert(i, prototype);

            }
            Undo.RegisterCompleteObjectUndo(terrain.gameObject, "AddNormalsToSplats");
            terrain.terrainData.splatPrototypes = splatProtos.ToArray();
#endif
#if UNITY_2018_3_OR_NEWER
            var splatProtos = new List<TerrainLayer>();
            for (var i = 0; i < albedos.Count; i++)
            {
                var prototype = new TerrainLayer();
                prototype.diffuseTexture = albedos[i];
                prototype.normalMapTexture = normals[normalCount[i]];
                splatProtos.Insert(i, prototype);

            }
            Undo.RegisterCompleteObjectUndo(terrain.gameObject, "AddNormalsToSplats");
            terrain.terrainData.terrainLayers = splatProtos.ToArray();
#endif
            terrain.Flush();
            terrain.terrainData.RefreshPrototypes();
            Close();
        }
    }
    public class SelectRemovableComponents : EditorWindow
    {
        static GameObject _obj;
        static List<Component> components = new List<Component>();
        private static List<bool> delete = new List<bool>();
        string failedToRemove = "The following could not be removed due to other component requirements:\n";

        public static void Init()
        {
            _obj = Selection.activeGameObject;
            components.Clear();
            delete.Clear();
            FuzzyTools.requiredComponents.Clear();

            components.AddRange(_obj.GetComponents<Component>());
            var window = GetWindow(typeof(SelectRemovableComponents));
            window.titleContent.text = "ComponentSelection";
            var screenHeight = 100;
            
            foreach(var component in components)
            {
                if (component.GetType() == typeof(Transform)) continue;
                delete.Add(false);
                screenHeight += 20;
                if (screenHeight >= 500) break;
            }

            window.position = new Rect(Screen.width, Screen.height, 400, screenHeight);
            window.ShowPopup();

        }
        private void TryRemoveComponents()
        {
            var failed = false;
            var i = 0;
            var componentsLeft = GetToDestroyCount();
            var failedComponents = "";

  

            FuzzyTools.CheckIfRequired(components.ToArray(), null);

            foreach(var comp in components)
            {
                
                if(comp.GetType() == typeof(Transform)) continue;
                if (delete[i] && !FuzzyTools.requiredComponents.Contains(comp.GetType()))
                {
                    Undo.DestroyObjectImmediate(comp);
                    if (comp!=null)
                    {
                        failed = true;
                    }else
                    {
                        componentsLeft--;
                    }
                }
                i++;
            }
            
            if(componentsLeft !=0)
            {
                FuzzyTools.CheckIfRequired(components.ToArray(), null);
                i = 0;
                foreach (var comp in components)
                {
                    if (comp == null || comp.GetType() == typeof(Transform)) continue;
                    if (delete[i])
                    {
                        if (!FuzzyTools.requiredComponents.Contains(comp.GetType()))
                        {
                            Undo.DestroyObjectImmediate(comp);
                            if (comp != null)
                            {
                                failed = true;
                                failedComponents += comp.GetType().ToString() + "\n";
                            }
                            else
                            {
                                componentsLeft--;
                            }
                        }
                        else
                        {
                            failed = true;
                            failedComponents += comp.GetType().ToString() + "\n";
                        }
                    }

                    i++;
                }
            }

            if (componentsLeft > 0)
            {
                
                failed = true;
            }
            

            if(failed)
            { 
                if (EditorUtility.DisplayDialog("Some Components Failed to Remove", failedToRemove + failedComponents + 
                    "\n The following components require the components that failed to remove:\n" + FuzzyTools.requirees, "Okay"))
                {
                    
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Success!", "All selected components removed!", "Okay");
            }
        }

        private static int GetToDestroyCount()
        {
            var i = 0;
            foreach(var c in delete)
            {
                if (c) i++;
            }
            return i;
        }
        
        private void OnGUI()
        {
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Please select which components of " + _obj.name + " you would like to remove.", EditorStyles.wordWrappedLabel);
            GUILayout.Space(20);
            var i = 0;
            foreach(var c in components)
            {
                if (c.GetType() == typeof(Transform)) continue;
                
                delete[i] = EditorGUILayout.ToggleLeft(c.GetType().ToString(), delete[i]);
                i++;
                
            }

            GUILayout.Space(20);
            if (GUILayout.Button("Remove Selected"))
            {
                TryRemoveComponents();
                this.Close();

            }
        }
    }
    public class ShowRecentScenes:EditorWindow
    {
        static EditorWindow window;

        public static void Init()
        {
            window = GetWindow(typeof(ShowRecentScenes));
            window.titleContent.text = "Recent Scenes";
            window.Show();
        }
        private void OnGUI()
        {
            for (var i = 0; i < FuzzyTools.recentScenes.Count; i++)
            {
                if (FuzzyTools.recentScenes[i] != null && FuzzyTools.recentScenes[i] != "")
                {
                    string[] split = FuzzyTools.recentScenes[i].Split('/');
                    string sceneName = split[split.Length - 1];
                    if (sceneName.IndexOf(".") >= 0)
                    {
                        sceneName = sceneName.Substring(0, sceneName.IndexOf("."));
                    }

                    if (GUILayout.Button(sceneName))
                    {
                        if (FuzzyTools.recentScenes[i] != EditorSceneManager.GetActiveScene().path)
                        {
                            LoadNewScene(i);
                        }
                        else
                        {
                            if (EditorUtility.DisplayDialog("Scene Currently Open.", "The selected scene is already open. Would you like to reopen it?", "Yes", "No"))
                            {
                                LoadNewScene(i);
                            }
                        }
                    }
                }
            }
            if (FuzzyTools.recentScenes[0] != null)
            {
                GUILayout.Space(20);
                if (GUILayout.Button("Clear Recent Scenes"))
                {
                    for (int j = 0; j < FuzzyTools.recentScenes.Count; j++)
                    {
                        FuzzyTools.recentScenes[j] = null;
                        FuzzyHelper.RemoveKey(Application.productName + "RecentScenes" + j);
                    }
                }
            }
            else
            {
                GUILayout.Space(20);
                GUILayout.Label("There are no recent scenes");
                GUILayout.Space(20);
                if (GUILayout.Button("Close"))
                {
                    window.Close();
                }
            }

        }
        private void LoadNewScene(int sceneToLoad)
        {
            string[] split = FuzzyTools.recentScenes[sceneToLoad].Split('/');
            string sceneName = split[split.Length - 1];
            sceneName = sceneName.Substring(0, sceneName.IndexOf("."));
            int option = EditorUtility.DisplayDialogComplex("Open " + sceneName, "What would you like to do?", "Save and Open", "Cancel", "Open Don't Save");
            switch (option)
            {
                case (0):
                    if (EditorSceneManager.sceneCount > 1 && EditorUtility.DisplayDialog("Save all scenes?", "Would you like to save all open scenes?", "Yes", "No"))
                    {
                        for (int i = 0; i > EditorSceneManager.sceneCount; i++)
                        {
                            EditorSceneManager.SaveScene(EditorSceneManager.GetSceneAt(i));
                        }
                    }
                    else
                    {
                        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                    }
                    EditorSceneManager.OpenScene(FuzzyTools.recentScenes[sceneToLoad]);
                    break;
                case (1):
                    break;
                case (2):
                    EditorSceneManager.OpenScene(FuzzyTools.recentScenes[sceneToLoad]);
                    break;
            }

            window.Close();
        }
    }
    public class SplitTerrain : EditorWindow
    {

        static List<Terrain> origTerrain = new List<Terrain>();
        int xLen = 6; // Match with TerrainManager if using
        int zLen = 6;
        // Must be power of two plus 1
        int newHeightRes = 65; // Started with 513 in New Terrain
        int newDetailRes = 256; // Started with 1024 in New Terrain
        int newSplatRes = 128; // Started with 512 in New Terrain
        static List<string> newName = new List<string>();
        static string path = "/Resources/";
        static int numberOfTerrains = 0;

        public static void Init(List<Terrain> terrains)
        {
            origTerrain.Clear();
            
            if(terrains != null)
            {
                origTerrain.AddRange(terrains);
            }else
            {
                origTerrain.AddRange((Terrain[])GameObject.FindObjectsOfType(typeof(Terrain)));
            }
            
            foreach(Terrain t in origTerrain)
            {
                newName.Add(t.name);
            }
            numberOfTerrains = origTerrain.Count;
            GetWindow<SplitTerrain>();
            //newName = origTerrain.name;
        }

        int terrainSelection = 1;
        bool showTerrains = false;

        public void OnGUI()
        {
            numberOfTerrains = EditorGUILayout.IntSlider(numberOfTerrains, 0, 10);
            if(numberOfTerrains>origTerrain.Count)
            {
                for(int i = origTerrain.Count; i<numberOfTerrains; i++)
                {
                    origTerrain.Add(null);
                    newName.Add("");
                }
            }else
            {
                while (numberOfTerrains != origTerrain.Count)
                {
                    origTerrain.RemoveAt(origTerrain.Count - 1);
                    newName.RemoveAt(newName.Count - 1);
                }
            }
            

            showTerrains = EditorGUILayout.Foldout(showTerrains, "Terrains");
            if (showTerrains)
            {
                for (int i = 0; i < origTerrain.Count; i++)
                {
                    origTerrain[i] = (Terrain)EditorGUILayout.ObjectField("Terrain " + i + " to Split", origTerrain[i], typeof(Terrain), true);
                    newName[i] = EditorGUILayout.TextField("New Terrain " + i + " Name", newName[i]);
                }
            }
            //origTerrain = (Terrain)EditorGUILayout.ObjectField("Terrain to Split", origTerrain, typeof(Terrain), true);
            //newName = EditorGUILayout.TextField("New Terrain Name", newName);
            xLen = EditorGUILayout.IntField("X Axis Pieces", xLen);
            xLen = Mathf.Clamp(xLen, 1, FuzzyTools.maxTerrainSplit);
            zLen = EditorGUILayout.IntField("Z Axis Pieces", zLen);
            zLen = Mathf.Clamp(zLen, 1, FuzzyTools.maxTerrainSplit);

            terrainSelection = EditorGUILayout.Popup("New Terrain Rez", terrainSelection, FuzzyTools.terrainRezOptions);
            Int32.TryParse(FuzzyTools.terrainRezOptions[terrainSelection], out newHeightRes);
            //newHeightRes = EditorGUILayout.IntField("New Terrain Rez", newHeightRes);
            //newHeightRes = Mathf.Clamp(newHeightRes, 33, 4097);

            newDetailRes = EditorGUILayout.IntField("New Detail Rez", newDetailRes);
            newDetailRes = Mathf.Clamp(newDetailRes, 0, 4048);

            newSplatRes = EditorGUILayout.IntField("New Splat Rez", newSplatRes);
            newSplatRes = Mathf.Clamp(newSplatRes, 16, 2048);

            path = EditorGUILayout.TextField("Save Location:     Assets/", path);
            if (!path.EndsWith("/") && path.Length > 0) path += "/";
            if (!path.StartsWith("/")) path = "/" + path;


            if (GUILayout.Button("Split!"))
            {
                foreach (var t in origTerrain)
                {
                    if (t == null) origTerrain.Remove(t);
                }
                if (origTerrain.Count== 0)
                {
                    EditorUtility.DisplayDialog("No Terrain", "Please add a Terrain to split.", "Okay");
                    return;
                }
                for (int x = 0; x < xLen; x++)
                {
                    for (int z = 0; z < zLen; z++)
                    {
                        foreach (var t in origTerrain)
                        {
                            EditorUtility.DisplayProgressBar("Splitting Terrain " + t.name, "Copying heightmap, detail, splat, and trees", (float)((x * zLen) + z) / (xLen * zLen));
                            float xMin = t.terrainData.size.x / xLen * x;
                            float xMax = t.terrainData.size.x / xLen * (x + 1);
                            float zMin = t.terrainData.size.z / zLen * z;
                            float zMax = t.terrainData.size.z / zLen * (z + 1);
                            copyTerrain(t, string.Format("{0}{1}_{2}", newName, x, z), xMin, xMax, zMin, zMax, newHeightRes, newDetailRes, newSplatRes);
                        }
                    }
                }
                EditorUtility.ClearProgressBar();

                for (int x = 0; x < xLen; x++)
                {
                    for (int z = 0; z < zLen; z++)
                    {
                        foreach (Terrain t in origTerrain)
                        {
                            GameObject center = GameObject.Find(string.Format("{0}{1}_{2}", t.name, x, z));
                            GameObject left = GameObject.Find(string.Format("{0}{1}_{2}", t.name, x - 1, z));
                            GameObject top = GameObject.Find(string.Format("{0}{1}_{2}", t.name, x, z + 1));
                            stitchTerrain(center, left, top);
                        }
                    }
                }
            }
        }
        void copyTerrain(Terrain origTerrain, string newName, float xMin, float xMax, float zMin, float zMax, int heightmapResolution, int detailResolution, int alphamapResolution)
        {
            if (File.Exists(Application.dataPath + path + newName + ".assets"))
            {
                if (!EditorUtility.DisplayDialog("Terrain Already Exists", "A Terrain with the name " + newName + " already exists. What would you like to do?", "Continue", "Cancel"))
                {
                    return;
                }
            }

            TerrainData td = new TerrainData();
            GameObject gameObject = Terrain.CreateTerrainGameObject(td);
            Terrain newTerrain = gameObject.GetComponent<Terrain>();

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            // Must do this before Splat
            AssetDatabase.CreateAsset(td, "Assets/" + path + newName + ".asset");

            // Copy over all vars
            newTerrain.bakeLightProbesForTrees = origTerrain.bakeLightProbesForTrees;
            newTerrain.basemapDistance = origTerrain.basemapDistance;
            newTerrain.castShadows = origTerrain.castShadows;
            newTerrain.collectDetailPatches = origTerrain.collectDetailPatches;
            newTerrain.detailObjectDensity = origTerrain.detailObjectDensity;
            newTerrain.detailObjectDistance = origTerrain.detailObjectDistance;
            newTerrain.drawHeightmap = origTerrain.drawHeightmap;
            newTerrain.drawTreesAndFoliage = origTerrain.drawTreesAndFoliage;
            newTerrain.editorRenderFlags = origTerrain.editorRenderFlags;
            newTerrain.heightmapMaximumLOD = origTerrain.heightmapMaximumLOD;
            newTerrain.heightmapPixelError = origTerrain.heightmapPixelError;
            newTerrain.legacyShininess = origTerrain.legacyShininess;
            newTerrain.legacySpecular = origTerrain.legacySpecular;
            newTerrain.lightmapIndex = origTerrain.lightmapIndex;
            newTerrain.lightmapScaleOffset = origTerrain.lightmapScaleOffset;
            newTerrain.materialTemplate = origTerrain.materialTemplate;
            newTerrain.materialType = origTerrain.materialType;
            newTerrain.realtimeLightmapIndex = origTerrain.realtimeLightmapIndex;
            newTerrain.realtimeLightmapScaleOffset = origTerrain.realtimeLightmapScaleOffset;
            newTerrain.reflectionProbeUsage = origTerrain.reflectionProbeUsage;
            newTerrain.treeBillboardDistance = origTerrain.treeBillboardDistance;
            newTerrain.treeCrossFadeLength = origTerrain.treeCrossFadeLength;
            newTerrain.treeDistance = origTerrain.treeDistance;
            newTerrain.treeMaximumFullLODCount = origTerrain.treeMaximumFullLODCount;
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
            td.splatPrototypes = origTerrain.terrainData.splatPrototypes;
#endif
#if UNITY_2018_3_OR_NEWER
            td.terrainLayers = origTerrain.terrainData.terrainLayers;
#endif
            td.treePrototypes = origTerrain.terrainData.treePrototypes;
            td.detailPrototypes = origTerrain.terrainData.detailPrototypes;

            // Get percent of original
            var xMinNorm = xMin / origTerrain.terrainData.size.x;
            var xMaxNorm = xMax / origTerrain.terrainData.size.x;
            var zMinNorm = zMin / origTerrain.terrainData.size.z;
            var zMaxNorm = zMax / origTerrain.terrainData.size.z;
            float dimRatio1, dimRatio2;

            // Height
            td.heightmapResolution = heightmapResolution;
            var newHeights = new float[heightmapResolution, heightmapResolution];
            dimRatio1 = (xMax - xMin) / heightmapResolution;
            dimRatio2 = (zMax - zMin) / heightmapResolution;
            for (var i = 0; i < heightmapResolution; i++)
            {
                for (var j = 0; j < heightmapResolution; j++)
                {
                    // Divide by size.y because height is stored as percentage
                    // Note this is [j, i] and not [i, j] (Why?!)
                    newHeights[j, i] = origTerrain.SampleHeight(new Vector3(xMin + (i * dimRatio1), 0, zMin + (j * dimRatio2))) / origTerrain.terrainData.size.y;
                }
            }
            td.SetHeightsDelayLOD(0, 0, newHeights);

            // Detail
            td.SetDetailResolution(detailResolution, 8); // Default? Haven't messed with resolutionPerPatch
            for (var layer = 0; layer < origTerrain.terrainData.detailPrototypes.Length; layer++)
            {
                var detailLayer = origTerrain.terrainData.GetDetailLayer(
                        Mathf.FloorToInt(xMinNorm * origTerrain.terrainData.detailWidth),
                        Mathf.FloorToInt(zMinNorm * origTerrain.terrainData.detailHeight),
                        Mathf.FloorToInt((xMaxNorm - xMinNorm) * origTerrain.terrainData.detailWidth),
                        Mathf.FloorToInt((zMaxNorm - zMinNorm) * origTerrain.terrainData.detailHeight),
                        layer);
                var newDetailLayer = new int[detailResolution, detailResolution];
                dimRatio1 = (float)detailLayer.GetLength(0) / detailResolution;
                dimRatio2 = (float)detailLayer.GetLength(1) / detailResolution;
                for (int i = 0; i < newDetailLayer.GetLength(0); i++)
                {
                    for (int j = 0; j < newDetailLayer.GetLength(1); j++)
                    {
                        newDetailLayer[i, j] = detailLayer[Mathf.FloorToInt(i * dimRatio1), Mathf.FloorToInt(j * dimRatio2)];
                    }
                }
                td.SetDetailLayer(0, 0, layer, newDetailLayer);
            }

            // Splat
            td.alphamapResolution = alphamapResolution;
            var alphamaps = origTerrain.terrainData.GetAlphamaps(
                Mathf.FloorToInt(xMinNorm * origTerrain.terrainData.alphamapWidth),
                Mathf.FloorToInt(zMinNorm * origTerrain.terrainData.alphamapHeight),
                Mathf.FloorToInt((xMaxNorm - xMinNorm) * origTerrain.terrainData.alphamapWidth),
                Mathf.FloorToInt((zMaxNorm - zMinNorm) * origTerrain.terrainData.alphamapHeight));
            // Last dim is always origTerrain.terrainData.splatPrototypes.Length so don't ratio
            var newAlphaMaps = new float[alphamapResolution, alphamapResolution, alphamaps.GetLength(2)];
            dimRatio1 = (float)alphamaps.GetLength(0) / alphamapResolution;
            dimRatio2 = (float)alphamaps.GetLength(1) / alphamapResolution;
            for (var i = 0; i < newAlphaMaps.GetLength(0); i++)
            {
                for (var j = 0; j < newAlphaMaps.GetLength(1); j++)
                {
                    for (var k = 0; k < newAlphaMaps.GetLength(2); k++)
                    {
                        newAlphaMaps[i, j, k] = alphamaps[Mathf.FloorToInt(i * dimRatio1), Mathf.FloorToInt(j * dimRatio2), k];
                    }
                }
            }
            td.SetAlphamaps(0, 0, newAlphaMaps);

            // Tree
            for (var i = 0; i < origTerrain.terrainData.treeInstanceCount; i++)
            {
                var ti = origTerrain.terrainData.treeInstances[i];
                if (ti.position.x < xMinNorm || ti.position.x >= xMaxNorm)
                    continue;
                if (ti.position.z < zMinNorm || ti.position.z >= zMaxNorm)
                    continue;
                ti.position = new Vector3(((ti.position.x * origTerrain.terrainData.size.x) - xMin) / (xMax - xMin), ti.position.y, ((ti.position.z * origTerrain.terrainData.size.z) - zMin) / (zMax - zMin));
                newTerrain.AddTreeInstance(ti);
            }

            gameObject.transform.position = new Vector3(origTerrain.transform.position.x + xMin, origTerrain.transform.position.y, origTerrain.transform.position.z + zMin);
            gameObject.name = newName;

            // Must happen after setting heightmapResolution
            td.size = new Vector3(xMax - xMin, origTerrain.terrainData.size.y, zMax - zMin);

            AssetDatabase.SaveAssets();
        }

        void stitchTerrain(GameObject center, GameObject left, GameObject top)
        {
            if (center == null)
                return;
            var centerTerrain = center.GetComponent<Terrain>();
            var centerHeights = centerTerrain.terrainData.GetHeights(0, 0, centerTerrain.terrainData.heightmapWidth, centerTerrain.terrainData.heightmapHeight);
            if (top != null)
            {
                var topTerrain = top.GetComponent<Terrain>();
                var topHeights = topTerrain.terrainData.GetHeights(0, 0, topTerrain.terrainData.heightmapWidth, topTerrain.terrainData.heightmapHeight);
                if (topHeights.GetLength(0) != centerHeights.GetLength(0))
                {
                    Debug.Log("Terrain sizes must be equal");
                    return;
                }
                for (int i = 0; i < centerHeights.GetLength(1); i++)
                {
                    centerHeights[centerHeights.GetLength(0) - 1, i] = topHeights[0, i];
                }
            }
            if (left != null)
            {
                var leftTerrain = left.GetComponent<Terrain>();
                var leftHeights = leftTerrain.terrainData.GetHeights(0, 0, leftTerrain.terrainData.heightmapWidth, leftTerrain.terrainData.heightmapHeight);
                if (leftHeights.GetLength(0) != centerHeights.GetLength(0))
                {
                    Debug.Log("Terrain sizes must be equal");
                    return;
                }
                for (var i = 0; i < centerHeights.GetLength(0); i++)
                {
                    centerHeights[i, 0] = leftHeights[i, leftHeights.GetLength(1) - 1];
                }
            }
            centerTerrain.terrainData.SetHeights(0, 0, centerHeights);
        }
    }
    public class ChangeShaderToSelected : EditorWindow
    {

        private static List<Material> mats = new List<Material>();
        private static Shader _shader;
        private static int _matCount;
        private bool _showMats = true;
        private static bool _getShaderFromMat = false;
        private static Material _materialToShader;
        Vector2 _pos = new Vector2(0,0);

        //[MenuItem("Test/testWindow")]
        public static void Init(Material[] myMats)
        {
            mats.Clear();
            mats.AddRange(myMats);
            _matCount = mats.Count;
            _shader = Shader.Find(("Standard"));
            _materialToShader = new Material(_shader);
            _getShaderFromMat = false;
            var window = GetWindow<ChangeShaderToSelected>();
            window.titleContent.text = "ChangeShaders";
        }
        private void OnGUI()
        {
            _pos = EditorGUILayout.BeginScrollView(_pos);
            _matCount = EditorGUILayout.IntField("Number of Materials",_matCount);
            if (_matCount <= 0) _matCount = 1;
            //FuzzyTools.UpdateWindowListLength<Component>(mats, _matCount);
            if(_matCount>mats.Count)
            {
                for(var i = mats.Count; i<_matCount; i++)
                {
                    mats.Add(null);
                }
            }else
            {
                while (_matCount != mats.Count)
                {
                    mats.RemoveAt(mats.Count - 1);
                }
            }

            _showMats = EditorGUILayout.Foldout(_showMats, "Materials");
            if (_showMats)
            {
                GUILayout.BeginVertical();
                for (var i = 0; i < _matCount; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    var matName = "";
                    if (mats[i] != null)
                    {
                        matName = mats[i].name;
                    }
                    
                    var tempMat = (Material) EditorGUILayout.ObjectField(matName,mats[i], typeof(Material), true);
                    if (mats.Contains(tempMat) && tempMat!=null && mats[i] != tempMat)
                    {
                        EditorUtility.DisplayDialog("Duplicate Material Detected.",
                            "The material you just tried to add is already in the list of materials to change and will not be added.",
                            "Okay");
                    }
                    else
                    {
                        mats[i] = tempMat;
                    }
                    
                    if (mats[i] != null)
                    {
                        var texture = AssetPreview.GetAssetPreview(mats[i]);
                        if(texture != null)
                            EditorGUI.DrawPreviewTexture(GUILayoutUtility.GetRect(64,64), texture);
                    }
                    EditorGUILayout.EndHorizontal();
                    
                }
                GUILayout.EndVertical();
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            if (Selection.activeObject != null)
            {
                if (Selection.activeObject.GetType() != typeof(Material))
                {
                    _shader = (Shader) EditorGUILayout.ObjectField("New Shader", _shader, typeof(Shader), true);
                }
                else

                {
                    //Material materialToShader = new Material(Shader.Find("Standard"));
                    _materialToShader = (Material) EditorGUILayout.ObjectField("New Shader", _materialToShader, typeof(Material), true);
                    if (_materialToShader != null)
                    {
                        _shader = _materialToShader.shader;
                    }

                }
            }
            else
            {
                _shader = (Shader) EditorGUILayout.ObjectField("New Shader", _shader, typeof(Shader), true);
            }

            if (_getShaderFromMat && !_shader)
            {
                var mat = (EditorGUILayout.ObjectField("Source Material", _shader, typeof(Material), true) as Material);
                if (mat != null)
                {
                    _shader = mat.shader;
                }
            }
            GUILayout.Space((20));

            if (GUILayout.Button("Apply Shader"))
            {
                if (_shader != null &&
                    EditorUtility.DisplayDialog("Change materials?",
                        "Change all listed materials to " + _shader.name + "?", "Continue", "Cancel"))
                {
                    mats = mats.Distinct().ToList();
                    //DestroyImmediate(_materialToShader);
                    ChangeShaders();
                    Close();
                }else if (!_shader)
                {
                    if(EditorUtility.DisplayDialog("No shader selected",
                        "No shader is selected, would you like to add a shader using another material?", "Yes", "No"))
                    {
                        _getShaderFromMat = true;
                    }
                }
            }
            EditorGUILayout.EndScrollView();
            /*if(mats.Count != mats.Distinct().ToList().Count && mats.Count >1);
            {
                EditorUtility.DisplayDialog("Duplicate Material Detected.",
                    "The material you just tried to add is already in the list of materials to change and will not be added.",
                    "Okay");
                mats = mats.Distinct().ToList();
            }*/

        }

       

        void ChangeShaders()
        {
            foreach (var m in mats)
            {
                if (m == null)
                {
                    continue;
                }
                Undo.RegisterCompleteObjectUndo(m, "ChangingShaders");
                m.shader = _shader;
            }
        }
    }
    public class AutoSnap : EditorWindow
    {
        List<Vector3> prevPoses = new List<Vector3>();
        private static bool doSnap = false;
        public static float snapValueX;
        public static float snapValueY;
        public static float snapValueZ;
        public static EditorWindow window;
//        private static float prefX;
//        private static float prefY;
//        private static float prefZ;

        

        
        public static void Init()
        {
            GetEditorPrefs();
            if (window == null)
            {
                window = GetWindow<AutoSnap>();
                window.titleContent.text = "AutoSnap Tool";
                doSnap = true;
            }
            else
            {
                if (doSnap)
                {
                    doSnap = false;
                }
                window.Close();
                window = null;
            }
        }

        void OnGUI()
        {
            doSnap = EditorGUILayout.Toggle("Auto Snap", doSnap);
            
            snapValueX = EditorGUILayout.FloatField("X Snap Value", snapValueX);
            snapValueY = EditorGUILayout.FloatField("Y Snap Value", snapValueY);
            snapValueZ = EditorGUILayout.FloatField("Z Snap Value", snapValueZ);
        }

        private void OnSelectionChange()
        {
            prevPoses.Clear();
            foreach (var t in Selection.transforms)
            {
                prevPoses.Add(t.position);
            }
        }

        private void Update()
        {
            if ( doSnap && !EditorApplication.isPlaying && Selection.transforms.Length > 0
                 && !prevPoses.Contains(Selection.transforms[0].position) )
            {
                foreach ( var t in Selection.transforms )
                {
                    Snap(t);
                    prevPoses.Add(Selection.transforms[0].position);
                }
                
                
            }
        }

        private void Snap(Transform t)
        {
            var pos = t.transform.position;
            pos.x = Round( pos.x, Axis.X);
            pos.y = Round( pos.y, Axis.Y);
            pos.z = Round( pos.z, Axis.Z);
            Undo.RegisterCompleteObjectUndo(t, "SnapObjectPos");
            t.transform.position = pos;
        }

        private float Round(float number, Axis myAxis)
        {
            switch (myAxis)
            {
                    case(default(Axis)):
                        return snapValueX * Mathf.Round((number / snapValueX));

                    case(Axis.Y):
                        return snapValueY * Mathf.Round((number / snapValueY));
                        
                    case(Axis.Z):
                        return snapValueZ * Mathf.Round((number / snapValueZ));
                        
            }

            return 0;
        }
        static void GetEditorPrefs()
        {
            snapValueX = FuzzyHelper.GetEditorPrefFloat("MoveSnapX");
//            prefX = snapValueX;
            snapValueY = FuzzyHelper.GetEditorPrefFloat("MoveSnapY");
//            prefY = snapValueY;
            snapValueZ = FuzzyHelper.GetEditorPrefFloat("MoveSnapZ");
//            prefZ = snapValueZ;
        }
        void SetEditorPrefs()
        {
            FuzzyHelper.SetEditorPrefFloat("MoveSnapX", snapValueX);
            FuzzyHelper.SetEditorPrefFloat("MoveSnapY", snapValueY);
            FuzzyHelper.SetEditorPrefFloat("MoveSnapZ", snapValueZ);
        }
        
    }
    public class SimulatePhysics : EditorWindow
    {
        private static int totalIterations = 0;
        private SimulatedBody[] simulatedBodies;
        private List<GameObject> selected = new List<GameObject>();
        private List<Rigidbody> generatedRigidBodies = new List<Rigidbody>();
        private List<Collider> generatedColliders = new List<Collider>();
        private Vector2 randomForceRange = Vector2.zero;
        public bool randomizeDir = true;
        public Vector2 randomDirRange = new Vector2(0,360);
        public float forceAngleInDegrees;
        
        
        public static void Init()
        {
            var window = GetWindow<SimulatePhysics>();
            window.titleContent.text = "Physics Simulator";
            window.minSize = new Vector2(220,220);
        }

        private void OnGUI()
        {
            var fullWindowLayout = GUILayout.Width(this.position.width - 10);
            var quarterWindowLayout = GUILayout.Width((this.position.width * .25f) - 10);
            //EditorGUILayout.BeginHorizontal();
            FuzzyTools.maxPhysicsIterations =
                EditorGUILayout.IntField("Max Iterations:", FuzzyTools.maxPhysicsIterations, fullWindowLayout);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.IntField("Total Iterations:", totalIterations, fullWindowLayout);
            EditorGUI.EndDisabledGroup();
            //EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField("Random Force Range");
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(false);
            randomForceRange.x = EditorGUILayout.FloatField(randomForceRange.x, quarterWindowLayout);
            EditorGUILayout.LabelField("Min", GUILayout.Width((this.position.width*.25f) - 10));
            randomForceRange.y = EditorGUILayout.FloatField(randomForceRange.y, quarterWindowLayout);
            EditorGUILayout.LabelField("Max", GUILayout.Width((this.position.width*.25f) - 10));
            randomForceRange.x = Mathf.Clamp(randomForceRange.x, -100, randomForceRange.y);
            randomForceRange.y = Mathf.Clamp(randomForceRange.y, randomForceRange.x, 100);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.MinMaxSlider(ref randomForceRange.x, ref randomForceRange.y, -100f, 100f, fullWindowLayout);
            EditorGUILayout.LabelField("Force Direction");
            forceAngleInDegrees = EditorGUILayout.Slider(forceAngleInDegrees, 0, 360, fullWindowLayout);
            forceAngleInDegrees = Mathf.Clamp(forceAngleInDegrees, 0, 360);
            randomizeDir = EditorGUILayout.ToggleLeft("Randomize Force Direction", randomizeDir);
            if (randomizeDir)
            {
                EditorGUILayout.LabelField("Random Force Range");
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(false);
                randomDirRange.x = EditorGUILayout.FloatField(randomDirRange.x, quarterWindowLayout);
                EditorGUILayout.LabelField("Min", quarterWindowLayout);
                randomDirRange.y = EditorGUILayout.FloatField(randomDirRange.y, quarterWindowLayout);
                EditorGUILayout.LabelField("Max", quarterWindowLayout);
                randomDirRange.x = Mathf.Clamp(randomDirRange.x, 0, randomDirRange.y);
                randomDirRange.y = Mathf.Clamp(randomDirRange.y, randomDirRange.x, 360);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.MinMaxSlider(ref randomDirRange.x, ref randomDirRange.y, 0, 360, fullWindowLayout);
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Run Simulation"))
            {
                if (Selection.gameObjects.Length == 0)
                {
                    return;
                }
                else
                {
                    selected.Clear();
                    foreach (var obj in Selection.gameObjects)
                    {
                        if (AssetDatabase.Contains(obj)) continue;
                        
                        selected.Add(obj);
                    }

                    if (selected.Count == 0)
                    {
                        return;
                    }
                    AutoGenerateRigidBodies();
                    SimulateMyPhysics();
                }
            }

            if (GUILayout.Button("Reset Simulation"))
            {
                ResetRigidBodies();
            }

        }

        private void AutoGenerateRigidBodies()
        {
            foreach (var obj in selected)
            {
                if (!obj.GetComponent<Rigidbody>())
                {
                    generatedRigidBodies.Add(obj.AddComponent<Rigidbody>());
                }

                if (obj.GetComponent<Collider>()) continue;
                
                if (obj.GetComponent<MeshFilter>())
                {
                    var col = obj.AddComponent<MeshCollider>();
                    col.sharedMesh = obj.GetComponent<MeshFilter>().sharedMesh;
                    col.convex = true;
                    generatedColliders.Add(col);
                }
                else if (obj.GetComponentInChildren<MeshFilter>())
                {
                    var col = obj.AddComponent<MeshCollider>();
                    col.sharedMesh = obj.GetComponentInChildren<MeshFilter>().sharedMesh;
                    col.convex = true;
                    generatedColliders.Add(col);
                }
                else
                {
                    generatedColliders.Add(obj.AddComponent<BoxCollider>());
                }
            }
        }

        private void RemoveGeneratedRigidBodies()
        {
            foreach (var rigidbody in generatedRigidBodies)
            {
                DestroyImmediate(rigidbody);
            }

            foreach (var collider in generatedColliders)
            {
                DestroyImmediate(collider);
            }
        }

        private void SimulateMyPhysics()
        {
            simulatedBodies = FindObjectsOfType<Rigidbody>().Select(rb => new SimulatedBody(rb, selected.Contains(rb.gameObject))).ToArray();
            foreach (var body in simulatedBodies)
            {
                if (!body.selected) continue;
                
                var randomForce = UnityEngine.Random.Range(randomForceRange.x, randomForceRange.y);
                var forceAngle = ((randomizeDir)
                                     ? UnityEngine.Random.Range(randomDirRange.x, randomDirRange.y)
                                     : forceAngleInDegrees) * Mathf.Deg2Rad;
                Vector3 forceDir = new Vector3(Mathf.Sin(forceAngle),0,Mathf.Cos(forceAngle));
                body.rigidbody.AddForce(forceDir * randomForce, ForceMode.Impulse);
            }
            Physics.autoSimulation = false;
            for (int i = 0; i < FuzzyTools.maxPhysicsIterations; i++)
            {
                Physics.Simulate(Time.fixedDeltaTime);
                if (simulatedBodies.All(rigid => rigid.rigidbody.IsSleeping() || !rigid.selected))
                {
                    totalIterations = i;
                    break;
                }
            }

            foreach (var body in simulatedBodies)
            {
                if (!body.selected)
                {
                    body.Reset();
                }
            }
            Physics.autoSimulation = true;
            RemoveGeneratedRigidBodies();
        }

        public void ResetRigidBodies()
        {
            if (simulatedBodies == null) return;
            foreach (var body in simulatedBodies)
            {
                body.Reset();
            }
        }

        private struct SimulatedBody
        {
            public readonly Rigidbody rigidbody;
            public readonly bool selected;
            private readonly Vector3 origPos;
            private readonly Quaternion origRot;
            private readonly Transform trans;

            public SimulatedBody(Rigidbody rb, bool selected)
            {
                rigidbody = rb;
                this.selected = selected;
                trans = rb.transform;
                origRot = rb.rotation;
                origPos = rb.position;
            }

            public void Reset()
            {
                trans.position = origPos;
                trans.rotation = origRot;
                if (rigidbody == null) return;
                
                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
                
            }
        }
    }
    public class FindAndReplaceName : EditorWindow
    {
        private static EditorWindow window;
        private static Vector2 minWindowSize = new Vector2(300,300);
        private static Vector2 scrollPos = Vector2.zero;
        
        private static GameObject[] objs;
        private static List<GameObject> listOfObjs = new List<GameObject>();
        private GameObject[] allObjs;

        private static int whichReplace = 0;
        private string[] whichSelectionNames = {
            "Only Selected", "Only Listed", "All In Scene"
        };

        private static int howReplace = 0;
        private string[] howSelectionNames =
        {
            "Replace Search", "Full Rename"
        };

        private const string Prefix = "Prefix:";
        private const string Suffix = "Suffix:";
        private const string Name = "Name";
        private static string prefix = "";
        private static string suffix = "";

        private const string SearchFor = "Search For: ";
        private const string ReplaceWith = "Replace With: ";
        private const string IfContains = "If contains: ";
        private const string RenameTo = "RenameTo: ";
        
        
        private static string oldName = "";
        private static string newName = "";
        
        public static void Init()
        {
            whichReplace = 0;
            oldName = "";
            newName = "";
            prefix = "";
            suffix = "";
            scrollPos = Vector2.zero;
            objs = null;
            window = GetWindow(typeof(FindAndReplaceName), true, "Find And Replace Renammer");
            window.minSize = minWindowSize;
        }

        private void OnGUI()
        {
            //var halfWindowLayout = GUILayout.Width((position.width *.5f) - 10);
            var quarterWindowLayout = GUILayout.Width((position.width * .25f) - 10);
            //var sixthWindowLayout = GUILayout.Width((position.width * .15f) - 10);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            whichReplace = GUILayout.SelectionGrid(whichReplace, whichSelectionNames, whichSelectionNames.Length);
            EditorGUILayout.TextArea("",GUI.skin.horizontalSlider);

            howReplace = GUILayout.SelectionGrid(howReplace, howSelectionNames, howSelectionNames.Length);

            switch (howReplace)
            {
                  case 0:
                      EditorGUILayout.BeginHorizontal();
            
                      EditorGUILayout.LabelField(SearchFor, quarterWindowLayout);
                      oldName = EditorGUILayout.TextField(oldName, quarterWindowLayout);
                      EditorGUILayout.LabelField(ReplaceWith, quarterWindowLayout);
                      newName = EditorGUILayout.TextField(newName, quarterWindowLayout);
            
                      EditorGUILayout.EndHorizontal();
                      break;
                  case 1:
                      EditorGUILayout.BeginHorizontal();
            
                      EditorGUILayout.LabelField(IfContains, quarterWindowLayout);
                      oldName = EditorGUILayout.TextField(oldName);
                      
                      EditorGUILayout.EndHorizontal();
                      EditorGUILayout.BeginHorizontal();
                      EditorGUILayout.LabelField(RenameTo, quarterWindowLayout);
                      newName = EditorGUILayout.TextField(newName);
                      EditorGUILayout.EndHorizontal();
                      EditorGUILayout.BeginHorizontal();
                      
                      EditorGUILayout.LabelField(Prefix, quarterWindowLayout);
                      prefix = EditorGUILayout.TextField(prefix, quarterWindowLayout);
                      
                      
                       EditorGUILayout.LabelField(Suffix, quarterWindowLayout);
                      suffix = EditorGUILayout.TextField(suffix, quarterWindowLayout);
                      
                      
                      EditorGUILayout.EndHorizontal();
                      break;
            }
            
            switch (whichReplace)
            {
                case 0:
                    var selected = Selection.gameObjects;
                    if (selected.Length == 0) break;
                    
                    EditorGUI.BeginDisabledGroup(true);
                    
                    for (var i = 0; i< selected.Length; i++)
                    {
                        if (!selected[i].name.Contains(oldName)) continue;
                        var obj = (Object)selected[i];
                        selected[i] = (GameObject)EditorGUILayout.ObjectField(obj.name, obj, typeof(GameObject), false);
                    }
                    EditorGUI.EndDisabledGroup();

                    switch (howReplace)
                    {
                        case 0:
                            if (GUILayout.Button("Replace") && oldName!=""&& newName!="")
                            {
                                objs = selected;
                                objs.ReplaceStrings(oldName, newName);
                                Close();
                            }
                            break;
                        case 1:
                            if (GUILayout.Button("Rename") && newName!="")
                            {
                                objs = selected;
                                if (FuzzyTools.autoAddSpaces)
                                {
                                    if (prefix != "" && !prefix.EndsWith(" ") && !prefix.EndsWith("_"))
                                    {
                                        prefix += " ";
                                    }

                                    if (suffix != "" && !suffix.StartsWith(" ") && !suffix.StartsWith("_"))
                                    {
                                        suffix = " " + suffix;
                                    }
                                }
                                newName = prefix + newName + suffix;
                                objs.RenameObjectsIfContain(oldName, newName);
                                Close();
                            }
                            break;
                    }
                    
                    break;
                case 1:
                    
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Add Selected"))
                    {
                        listOfObjs.AddIfDoesNotContain(Selection.gameObjects);
                    }
                    if (GUILayout.Button("Clear List"))
                    {
                        listOfObjs.Clear();
                        
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Separator();

                    for (var i = 0; i< listOfObjs.Count; i++)
                    {
                        if (listOfObjs[i] != null && !listOfObjs[i].name.Contains(oldName)) continue;
                        var obj = (Object)listOfObjs[i];
                        listOfObjs[i] = (GameObject) EditorGUILayout.ObjectField(obj == null ? "Blank" : obj.name, obj,
                            typeof(GameObject), true);
                    }
                    
                    switch (howReplace)
                    {
                        case 0:
                            if (GUILayout.Button("Replace") && oldName!=""&& newName!="")
                            {
                                listOfObjs.ReplaceStrings(oldName, newName);
                                Close();
                            }
                            break;
                        case 1:
                            if (GUILayout.Button("Rename") && newName!="")
                            {
                                var toArray = listOfObjs.ToArray();
                                if (FuzzyTools.autoAddSpaces)
                                {
                                    if (prefix != "" && !prefix.EndsWith(" ") && !prefix.EndsWith("_"))
                                    {
                                        prefix += " ";
                                    }

                                    if (suffix != "" && !suffix.StartsWith(" ") && !suffix.StartsWith("_"))
                                    {
                                        suffix = " " + suffix;
                                    }
                                }
                                newName = prefix + newName + suffix;
                                toArray.RenameObjectsIfContain(oldName, newName);
                                Close();
                            }
                            break;
                    }
                    break;
                case 2:
                    FuzzyTools.showAllGameObjectsOfName =
                        EditorGUILayout.Toggle("Show Search Results", FuzzyTools.showAllGameObjectsOfName);
                    
                    FuzzyHelper.SetEditorPrefBool("ShowAllGameObjectsOfName", FuzzyTools.showAllGameObjectsOfName);
                    
                    if (FuzzyTools.showAllGameObjectsOfName)
                    {
                        allObjs = FindObjectsOfType<GameObject>();
                        EditorGUI.BeginDisabledGroup(true);
                        
                        for (var i = 0; i< allObjs.Length; i++)
                        {
                            if (!allObjs[i].name.Contains(oldName)) continue;
                            var obj = (Object)allObjs[i];
                            EditorGUILayout.LabelField(obj.GetInstanceID().ToString());
                            allObjs[i] = (GameObject)EditorGUILayout.ObjectField(obj.name, obj, typeof(GameObject), false);
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    
                    switch (howReplace)
                    {
                        case 0:
                            if (GUILayout.Button("Replace") && oldName!=""&& newName!="")
                            {
                                allObjs = FindObjectsOfType<GameObject>();
                                allObjs.ReplaceStrings(oldName, newName);
                                Close();
                            }
                            break;
                        case 1:
                            if (GUILayout.Button("Rename") && oldName!=""&& newName!="")
                            {
                                allObjs = FindObjectsOfType<GameObject>();
                                if (FuzzyTools.autoAddSpaces)
                                {
                                    if (prefix != "" && !prefix.EndsWith(" ") && !prefix.EndsWith("_"))
                                    {
                                        prefix += " ";
                                    }

                                    if (suffix != "" && !suffix.StartsWith(" ") && !suffix.StartsWith("_"))
                                    {
                                        suffix = " " + suffix;
                                    }
                                }
                                newName = prefix + newName + suffix;
                                allObjs.RenameObjectsIfContain(oldName, newName);
                                Close();
                            }
                            break;
                    }
                    if (GUILayout.Button("Replace") && oldName!=""&& newName!="")
                    {
                        
                    }
                    break;
            }
            
            EditorGUILayout.EndScrollView();
        }
    }
    public class PickAndCopyComponents : EditorWindow
    {
        private static EditorWindow window;
        

        private static GameObject _sourceObject;
        private static Component[] _components;
        private static List<GameObject> _targets = new List<GameObject>();
        private static Dictionary<Component, bool> _selectedComponents = new Dictionary<Component, bool>();

        private const string Source = "Source (Copy From)";
        private const int SourceWindowID = 1;
        private const string Target = "Targets (Copy To)";
        private const int TargetWindowID = 2;

        private const string SelectDesired = "Select the components you would like to copy";
        private const string HowToHandle = "For Existing Components:";
        
        private static readonly Vector2 MinSize = new Vector2(750, 250);
        
        private Rect _copyFromWindowRect = new Rect(10, 10, 200, 200);
        private Rect _pasteToWindowRect = new Rect(100, 10, 200, 200);

        private CopyMode _howToCopy = CopyMode.AddComponentAsNew;

        private static Vector2 _leftScrollView = Vector2.zero;
        private static Vector2 _rightScrollView = Vector2.zero;

        
        
        public static void Init()
        {
            _targets.Clear();
            _leftScrollView = Vector2.zero;
            _rightScrollView = Vector2.zero;
            window = GetWindow(typeof(PickAndCopyComponents), false, "Copy Components");
            window.minSize = MinSize;
        }

        private void OnGUI()
        {
            BeginWindows();

            _copyFromWindowRect.Set(10,10, (position.width *.5f) - 10, position.height - 25);
            _pasteToWindowRect.Set((position.width *.5f) + 10, 10, (position.width *.5f) - 20, position.height - 25);
            
            GUI.Window(SourceWindowID, _copyFromWindowRect, CopyFromWindow, Source);
            GUI.Window(TargetWindowID, _pasteToWindowRect, PastToWindow, Target);
            
            EndWindows();
        }
        
        private void CopyFromWindow(int WindowID)
        {
            var obj = (Object) _sourceObject;
            _sourceObject = (GameObject) EditorGUILayout.ObjectField(obj, typeof(GameObject), true);
            if (_sourceObject == null) return;
            _components = _sourceObject.GetComponents(typeof(Component));
            
            EditorGUILayout.TextArea("",GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(SelectDesired);
            GUILayout.Space(5);
            _leftScrollView = EditorGUILayout.BeginScrollView(_leftScrollView);
            foreach (var comp in _components)
            {
                if(!_selectedComponents.ContainsKey(comp)) _selectedComponents.Add(comp, false);
                EditorGUILayout.BeginHorizontal();
                _selectedComponents[comp] = EditorGUILayout.Toggle(_selectedComponents[comp]);
                var componentName = comp.GetType().ToString().Replace("UnityEngine.", "");
                EditorGUILayout.LabelField(componentName);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

        }

        private void PastToWindow(int windowID)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Selected"))
            {
                _targets.AddIfDoesNotContain(Selection.gameObjects);
            }
            if (GUILayout.Button("Clear List"))
            {
                _targets.Clear();
                
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();
            
            _howToCopy = (CopyMode)EditorGUILayout.EnumPopup(HowToHandle, _howToCopy);
            
            if (GUILayout.Button("Copy Components") && _targets.Count >0 && _selectedComponents.ContainsValue(true))
            {
                CopyComponents();
            }
            EditorGUILayout.Separator();

            _rightScrollView = EditorGUILayout.BeginScrollView(_rightScrollView);
            for (var i = 0; i< _targets.Count; i++)
            {
                if (_targets[i] == null) continue;
                var obj = (Object)_targets[i];
                _targets[i] = (GameObject) EditorGUILayout.ObjectField(obj.name, obj,
                    typeof(GameObject), true);
            }
            EditorGUILayout.EndScrollView();
        }

        private void CopyComponents()
        {
            foreach (var comp in _components)
            {
                if (!_selectedComponents[comp]) continue;
                ComponentUtility.CopyComponent(comp);
                
                foreach (var target in _targets)
                {
                    Undo.RegisterCompleteObjectUndo(target, "CopyMultipleComponents");
                    switch (_howToCopy)
                    {
                        case CopyMode.PasteComponentValues:
                            if (target.GetComponents(typeof(Component)).ContainsType(comp.GetType()))
                            {
                                var targetComp = target.GetComponents(typeof(Component))
                                    .GetComponentOfType(comp.GetType());
                                ComponentUtility.PasteComponentValues(targetComp);
                            }
                            else
                            {
                                ComponentUtility.PasteComponentAsNew(target);
                            }
                            break;
                        case CopyMode.AddComponentAsNew:
                            ComponentUtility.PasteComponentAsNew(target);
                            break;
                    }
                    
                }
            }
        }
    }
    public class ColorPicker : EditorWindow
{
	private static GameObject[] _selectedGameObjs;
	private static Color[] _backgroundColors;
	private static Color[] _fontColors;
	private static FontStyle[] _fontStyles;

	private static Color _skinDefault;
	private static Color _fontDefault;
    private static FontStyle _styleDefault;
	
	private static Vector2 scrollPos = Vector2.zero;

   public static void Init(GameObject[] selected)
	{
	    scrollPos = Vector2.zero;
	    _selectedGameObjs = selected;
        _backgroundColors = new Color[_selectedGameObjs.Length];
        _fontColors = new Color[_selectedGameObjs.Length];
        _fontStyles = new FontStyle[_selectedGameObjs.Length];
	    _styleDefault = FontStyle.Normal;

        _skinDefault = EditorGUIUtility.isProSkin? HierarchyTools.DefaultProSkin : HierarchyTools.DefaultSkin;
		_fontDefault = EditorGUIUtility.isProSkin ? Color.white : Color.black;
		
		for (var i = 0; i < _backgroundColors.Length; i++)
		{
			_backgroundColors[i] = _skinDefault;
			_fontColors[i] = _fontDefault;
		    _fontStyles[i] = _styleDefault;
		}
		GetWindow(typeof(ColorPicker), true, "Change Style");
	}

	private void OnGUI()
	{
	    scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
	    FuzzyTools.uniformChangeColors =
	        EditorGUILayout.Toggle("Change Colors in Uniform", FuzzyTools.uniformChangeColors);
	    FuzzyHelper.SetEditorPrefBool("UniformChangeColors", FuzzyTools.uniformChangeColors);

	    if (FuzzyTools.uniformChangeColors)
	    {
	        _skinDefault = EditorGUILayout.ColorField("Background Color", _skinDefault);
	        _fontDefault = EditorGUILayout.ColorField("Font Color", _fontDefault);
	        _styleDefault = (FontStyle)EditorGUILayout.EnumPopup("Font Style", _styleDefault);   
	    }
	    
	        
		for(var i = 0; i<_selectedGameObjs.Length; i++)
		{
		    if(!FuzzyTools.uniformChangeColors)
		    {
		        EditorGUILayout.LabelField(_selectedGameObjs[i].name);
		        _backgroundColors[i] = EditorGUILayout.ColorField("Background Color", _backgroundColors[i]);
		        _fontColors[i] = EditorGUILayout.ColorField("Font Color", _fontColors[i]);
		        _fontStyles[i] = (FontStyle) EditorGUILayout.EnumPopup("Font Style", _fontStyles[i]);
		    }
		    else
		    {
		        _backgroundColors[i] = _skinDefault;
		        _fontColors[i] = _fontDefault;
		        _fontStyles[i] = _styleDefault;
		    }
			var id = _selectedGameObjs[i].GetInstanceID();
			
            if (FuzzyTools.iDToBackgroundColor.ContainsKey(id))
            {
                FuzzyTools.iDToBackgroundColor[id] = _backgroundColors[i];
            }
            else
            {
                FuzzyTools.iDToBackgroundColor.Add(id, _backgroundColors[i]);
            }
			
            if (FuzzyTools.iDToFontColor.ContainsKey(id))
            {
                FuzzyTools.iDToFontColor[id] = _fontColors[i];
            }
            else
            {
                FuzzyTools.iDToFontColor.Add(id, _fontColors[i]);
            }
			
            if (FuzzyTools.iDToFontStyle.ContainsKey(id))
            {
                FuzzyTools.iDToFontStyle[id] = _fontStyles[i];
            }
            else
            {
                FuzzyTools.iDToFontStyle.Add(id, _fontStyles[i]);
            }
			EditorGUILayout.Separator();
		}
		EditorGUILayout.EndScrollView();
	}
}
    public class SplatMapReplacer : EditorWindow
    {
        private static Texture2D _splatMap;
        private static Texture2D _newSplat;
        private static readonly Vector2 MinSize = new Vector2(300, 187);

        public static void Init()
        {
            _splatMap = Selection.activeObject as Texture2D;
            if (_splatMap != null && _splatMap.format != TextureFormat.ARGB32)
            {
                _splatMap = null;
            }

            var window = GetWindow(typeof(SplatMapReplacer), false, "SplatMap Replacer");
            window.minSize = MinSize;
        }

        private void OnGUI()
        {
            var splatAsObj = _newSplat as Object;
            var splatAsObj2 = _splatMap as Object;
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Please Select the Target and new Splat Maps");
            EditorGUILayout.Separator();
            _splatMap = (Texture2D) EditorGUILayout.ObjectField("Current SplatMap", splatAsObj2, typeof(Texture2D), false);
            _newSplat = (Texture2D) EditorGUILayout.ObjectField("New SplatMap", splatAsObj, typeof(Texture2D), false);
            if(_splatMap == null) return;
            if(!_splatMap.name.Contains("SplatAlpha") && _splatMap.format != TextureFormat.ARGB32)
            {
                _splatMap = null;
                EditorUtility.DisplayDialog("No SplatMap Found",
                    "The selected texture for current SplatMap does not appear to be a valid SplatMap and was automatically removed.",
                    "Okay");
            }
            if (GUILayout.Button("Switch") && _splatMap != null && _newSplat != null)
            {
                ReplaceSplat();
            }
        }

        private static void ReplaceSplat()
        {
            var path = AssetDatabase.GetAssetPath(_newSplat);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;

            importer.isReadable = true;

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            _newSplat.ScaleTexture(_splatMap.width, _splatMap.height, TextureFormat.ARGB32);

            var texture = _newSplat.GetPixels(0);

            if (texture.Length / _splatMap.width != _splatMap.height) return;
            Undo.RegisterCompleteObjectUndo(_splatMap, "ReplaceSplatMap");
            _splatMap.SetPixels(texture);
            _splatMap.Apply();
            AssetDatabase.Refresh();
        }
    }

    
    
    public class ConvertImages : EditorWindow
    {
        private const string DesiredType = "Desired Type: ";
        private const string KeepOriginal = "Keep Original: ";
        private const string SameName = "Same Name: ";
        private const string NewName = "New Name";
        private const string AddToList = "Add Selection";
        private const string ClearList = "Clear List";
        private const string Convert = "Convert";
        private const string Destructive = "Warning this is destructive and cannot be undone!";
        private const string MessagePt1 = "This will create a duplicate of the image in ";
        private const string MessagePt2 = " format and destroy the original \nThis cannot be undone.";
        private const string Continue = "Continue";
        private const string Cancel = "Cancel";
        private const string Keep = "Keep Originals";
        
        private static readonly Vector2 MinSize = new Vector2(400, 240);

        private static readonly GUIStyle _warningFont = new GUIStyle ();

        
        
        
        private static Vector2 _scrollPos = Vector2.zero;
        private static List<Texture2D> _textures = new List<Texture2D>();
        private static ImageType _chosenType = ImageType.PNG;
        private static bool _keepOriginal = true;
        private static bool _sameName = true;
        private static List<string> _newNames = new List<string>();
        private static EditorWindow _window;

        private static int _radioSelection = 0;
        private const string ConvertedLocation = "Save Converted File to:";
        private static  readonly string[] RadioOptions = {"Origin File Location", "Custom Location"};
        private static string _saveLocation = FuzzyTools.DefaultImagePath;
        private static GUIStyle _bold = new GUIStyle();
        
        

        public static void Init()
        {
            _bold.fontStyle = FontStyle.Bold;
            _warningFont.fontStyle = FontStyle.Bold;
            _warningFont.normal.textColor = Color.red;
            _textures.Clear();
            AddSelectionToTextures();
            _chosenType = ImageType.PNG;
            _keepOriginal = true;
            _sameName = true;
            _newNames = new List<string>();
            _scrollPos = Vector2.zero;
            _radioSelection = 0;
            _window = GetWindow(typeof(ConvertImages), true, "Convert Images");
            _saveLocation = FuzzyTools.DefaultImagePath;
            _window.minSize = MinSize;
        }

        private void OnGUI()
        {
            
            EditorGUILayout.Space();
            _chosenType = (ImageType) EditorGUILayout.EnumPopup(DesiredType, _chosenType);
            EditorGUILayout.BeginHorizontal();
            var textDimensions = GUI.skin.label.CalcSize(new GUIContent(KeepOriginal));
            EditorGUIUtility.labelWidth = textDimensions.x;
            _keepOriginal = EditorGUILayout.Toggle(KeepOriginal, _keepOriginal);
            textDimensions = GUI.skin.label.CalcSize(new GUIContent(SameName));
            EditorGUIUtility.labelWidth = textDimensions.x;
            _sameName = EditorGUILayout.Toggle(SameName, _sameName);
            //EditorGUILayout.LabelField(SameName, quarterWindowLayout);
            EditorGUILayout.EndHorizontal();
            if(!_keepOriginal) EditorGUILayout.LabelField(Destructive, _warningFont);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(ConvertedLocation, _bold);
            _radioSelection = GUILayout.SelectionGrid(_radioSelection, RadioOptions, RadioOptions.Length, EditorStyles.radioButton);
            _saveLocation = _radioSelection == 1 ? EditorGUILayout.TextField(_saveLocation): _saveLocation;
            if (_saveLocation.Length > 0 && !_saveLocation.EndsWith("/")) _saveLocation += "/";
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(AddToList))
            {
                AddSelectionToTextures();
            }

            if (GUILayout.Button(ClearList))
            {
                _textures.Clear();
            }
            EditorGUILayout.EndHorizontal();
            
            for (var i = 0; i < _textures.Count; i++)
            {
                if (_textures[i] != null) continue;
                
                _textures.Remove(_textures[i]);
                i--;
                
            }

            _newNames.SetLength(_textures.Count, "");
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            for (var i = 0; i < _textures.Count; i++)
            {
                var texture = _textures[i];
                var obj = (Object)texture;
                EditorGUILayout.TextArea("",GUI.skin.horizontalSlider);
                
                if(_sameName)
                {
                    texture = (Texture2D) EditorGUILayout.ObjectField(texture != null ? texture.name : "", obj,
                        typeof(Texture2D), false);
                }
                else
                {
                    
                    _newNames[i] = EditorGUILayout.TextField(_newNames[i]);
                    
                    texture = (Texture2D) EditorGUILayout.ObjectField(_newNames[i], obj,
                        typeof(Texture2D), false);
                    
                    if (_newNames[i] == "") _newNames[i] = texture != null ? texture.name : "";
                    if (_newNames[i] == "///") _newNames[i] = "Pupper 1 is Penny Potato";
                    
                    
                }
            }
            EditorGUILayout.EndScrollView();
            
            if (GUILayout.Button(Convert))
            {
                ConvertImage();
            }
        }

        private static void AddSelectionToTextures()
        {
            var objects = Selection.objects;
            foreach (var obj in objects)
            {
                var image = obj as Texture2D;
                if (image != null)
                {
                    _textures.AddIfDoesNotContain(image);
                }
            }
        }
        
        private static void ConvertImage()
        {
            if (!_keepOriginal)
            {
                var i = EditorUtility.DisplayDialogComplex(Destructive, MessagePt1 + _chosenType + MessagePt2,
                    Continue, Cancel, Keep);
                switch(i)
                {
                    case (0):
                        break;
                    case (1):
                        return;
                    case (2):
                        _keepOriginal = false;
                        break;
                }
            }

            var j = 0;
            foreach (var tex in _textures)
            {
                if (tex == null) continue;
                
                var texName = tex.name;
                var texPath = AssetDatabase.GetAssetPath(tex);
                
                var pos = texPath.LastIndexOf(".") + 1;
                if (texPath.Substring(pos, texPath.Length - pos).Equals(_chosenType.ToString(), 
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    if(EditorUtility.DisplayDialog("Already " + _chosenType + " file", 
                        texName + " is already a " + _chosenType + ". What would you like to do?",
                        "Skip", "Convert Anyway"))
                    {
                        continue;
                    }
                }
                var texSettings = (TextureImporter)AssetImporter.GetAtPath(texPath);
                var originalSettings = texSettings;
                texSettings.isReadable = true;
                texSettings.textureCompression = TextureImporterCompression.Uncompressed;
                AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);

                texPath = texPath.Substring(0, texPath.LastIndexOf("/") + 1);
                //texName = _sameName?texName:
                var validName = _radioSelection == 1 ? "Assets/" +  _saveLocation: texPath;
                FuzzyTools.CheckIfPathExists(validName);
                validName += _sameName? texName: _newNames[j];
                var l = 0;


                while (File.Exists(validName + "." + _chosenType.ToString().ToLowerInvariant()))
                {
                    validName = (_radioSelection == 1 ? "Assets/" +  _saveLocation: texPath) + texName + "_" + l;
                    l++;
                }
                validName += "." + _chosenType.ToString().ToLowerInvariant();
                
                var bytes = new byte[0];
                // Encode texture into PNG
                switch (_chosenType)
                {
                    case ImageType.EXR:
                        var hdrFormat = new Texture2D(tex.width, tex.height, TextureFormat.RGBAFloat, false);
                        hdrFormat.SetPixels(tex.GetPixels());
                        hdrFormat.Apply();
                        
                        bytes = hdrFormat.EncodeToEXR(Texture2D.EXRFlags.CompressZIP);
                        DestroyImmediate(hdrFormat);
                        break;
                    case ImageType.JPG:
                        bytes = tex.EncodeToJPG();
                        break;
                    case ImageType.PNG:
                        bytes = tex.EncodeToPNG();
                        break;
                }

                File.WriteAllBytes(validName, bytes);
                texSettings = originalSettings;
                //Debug.Log(validName);
                //Debug.Log(texPath);
                AssetDatabase.ImportAsset(validName, ImportAssetOptions.ForceUpdate);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(tex), ImportAssetOptions.ForceUpdate);
                if (!_keepOriginal)
                {
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(tex));
                }
                j++;
            }

        }
    }
    public class ConvertModelToTerrain : EditorWindow
	{
		private static GameObject _sourceGeo;
		private const string SourceGeometry = "Source Geometry: ";
		private const string Resolution = "Heightmap Resolution: ";
		private const string TerrainPosition = "Terrain Position: ";
		private const string TerrainOffset = "Terrain Y Offset: ";
		private const string ConvertToTerrain = "Convert To Terrain";
		private const string AssetsFolder = "Assets/";
		private const string Asset = ".asset";
		private const string TerrainName = "New Terrain Name: ";
		private const string DefaultTerrainName = "New Terrain";
		private const string ScanMode = "Scan Mode:";
	    //private const string RegenerateCollider = "Temporarily Regenerate Collider";
	    
	    private static readonly Vector3 PlusTenth = new Vector3(.1f, 0, .1f);
		
		private static readonly Vector2 MinSize = new Vector2(300, 225);
	    private static readonly Vector2 OtherMinSize = new Vector2(300, 240);

		private static int _resolution = 512;
		private static int _rezSelection = 4;
		private static Vector3 _addTerrain;
		private static int _bottomTopRadioSelected = 0;
		private static readonly string[] BottomTopRadio = new string[] {"Bottom Up", "Top Down"};
		private static float _shiftHeight = 0f;

		private static string _terrainName = "";
		private static GameObject _storedAsset = null;
		private static bool _addedToScene = false;
	    
	    private static List<MeshCollider> newMeshColliders = new List<MeshCollider>();

	    private static int _radioSelection = 0;
	    private const string ConvertedLocation = "Save Converted File to:";
	    private static readonly string[] RadioOptions = {"Origin File Location", "Custom Location"};
	    private static string _saveLocation = FuzzyTools.DefaultTerrainPath;
	    private static GUIStyle _bold = new GUIStyle();

		

		public static void Init()
		{
			_terrainName = "";
			_rezSelection = 4;
			_sourceGeo = Selection.activeObject as GameObject;
		    _bold.fontStyle = FontStyle.Bold;
		    _radioSelection = 0;
			var window = GetWindow<ConvertModelToTerrain>(true, ConvertToTerrain);
			window.minSize = MinSize;
		}

		void OnGUI()
		{

			_sourceGeo = (GameObject) EditorGUILayout.ObjectField(SourceGeometry, _sourceGeo, typeof(GameObject), true);
			_terrainName = EditorGUILayout.TextField(TerrainName, _terrainName);
			if (_terrainName == "")
			{
				_terrainName = _sourceGeo != null ? _sourceGeo.name : DefaultTerrainName;
			}

		    if (_terrainName == "///") _terrainName = "Pupper 2 is Hiccup";
		    EditorGUILayout.Space();
		    EditorGUILayout.LabelField(ConvertedLocation, _bold);
		    _radioSelection = GUILayout.SelectionGrid(_radioSelection, RadioOptions, RadioOptions.Length,
		        EditorStyles.radioButton);
		    if (_radioSelection == 1)
		    {
		        minSize = OtherMinSize;
		        _saveLocation = EditorGUILayout.TextField(_saveLocation);
		    }
		    else
		    {
		        minSize = MinSize;
		    }
		    if (_saveLocation.Length > 0 && !_saveLocation.EndsWith("/")) _saveLocation += "/";
		    
		    EditorGUILayout.Space();
		    
			_rezSelection = EditorGUILayout.Popup(Resolution, _rezSelection, FuzzyTools.terrainRezOptions);
			_resolution = int.Parse(FuzzyTools.terrainRezOptions[_rezSelection]);//EditorGUILayout.IntField(Resolution, _resolution);
			_addTerrain = EditorGUILayout.Vector3Field(TerrainPosition, _addTerrain);
			_shiftHeight = EditorGUILayout.Slider(TerrainOffset, _shiftHeight, -1f, 1f);
			EditorGUILayout.LabelField(ScanMode);
		    //EditorGUILayout.BeginHorizontal();
			_bottomTopRadioSelected = GUILayout.SelectionGrid(_bottomTopRadioSelected, BottomTopRadio,
				BottomTopRadio.Length, EditorStyles.radioButton);
		    //_regenerateCollider = EditorGUILayout.ToggleLeft(RegenerateCollider, _regenerateCollider);
		    //EditorGUILayout.BeginToggleGroup("",_sourceGeo != null);
		    //EditorGUILayout.EndHorizontal();
		    
		    EditorGUI.BeginDisabledGroup(_sourceGeo == null);

			if (GUILayout.Button("Create Terrain"))
			{

				if (_sourceGeo == null)
				{

					EditorUtility.DisplayDialog("No source GameObject", "Please select an object.", "Ok");
					return;
				}

				else
				{
					CreateTerrain();
				}
			}
		    EditorGUI.EndDisabledGroup();
		    //EditorGUILayout.EndToggleGroup();
		}
/*

		private delegate void CleanUp();*/

		private static void CreateTerrain()
		{
		    CheckIfExistsInScene(_sourceGeo);

		    var meshFilters = _sourceGeo.GetComponentsInChildren<MeshFilter>();
		    var meshColliders = _sourceGeo.GetComponentsInChildren<MeshCollider>();
		    newMeshColliders.Clear();
		    foreach (var mesh in meshFilters)
		    {
		        if (mesh.GetComponent<MeshCollider>() || mesh.sharedMesh == null) continue;
		        var collider = mesh.gameObject.AddComponent<MeshCollider>();
		        if (collider == null) Debug.Log("What the what?");
		        collider.sharedMesh = mesh.sharedMesh;
		        collider.inflateMesh = mesh.sharedMesh.triangles.Length < 256;
		        newMeshColliders.Add(collider);
		    }

		    var myCollider = _sourceGeo.GetComponent<MeshCollider>();
		    if (myCollider == null)
		        myCollider = _sourceGeo.GetComponentInChildren<MeshCollider>();

		    if (myCollider == null)
		    {
		        EditorUtility.DisplayDialog("Cannot Generate Collider",
		            "We were unable to generate a mesh collider, most likely due to a lack of MeshFilters." +"\n" +
		            "A Terrain will not be created.", "Okay");
		        Init();
		        if (_addedToScene)
		        {
		            DestroyImmediate(_sourceGeo);
		            _sourceGeo = _storedAsset;
		            _addedToScene = false;
		        }
		        return;
		    }
		    
			
			
			//fire up the progress bar
			ShowProgressBar(1, 100);

			var terrain = new TerrainData()
			{
				heightmapResolution = _resolution, 
				name = _terrainName
			};
			var terrainObject = Terrain.CreateTerrainGameObject(terrain);

			

			

		    
		    
		    

			/*var myCollider = _sourceGeo.GetComponent<MeshCollider>();
			if (myCollider == null)
			{
				myCollider = _sourceGeo.AddComponent<MeshCollider>();
				cleanUp = () => DestroyImmediate(myCollider);
			}

			//Add a collider to our source object if it does not exist.
			//Otherwise raycasting doesn't work.
			foreach (Transform child in _sourceGeo.transform)
			{
				if (child.GetComponent<MeshCollider>()) continue;

				var collider = child.GetComponent<MeshFilter>() != null
					? _sourceGeo.AddComponent<MeshCollider>()
					: null;
				if (!collider)
				{
					cleanUp = () => DestroyImmediate(collider);
				}
			}*/

			//Bounds bounds = collider.bounds;
			var bounds = new Bounds(Vector3.zero, Vector3.zero);
			bounds = bounds.GetBounds(_sourceGeo);

			var sizeFactor = bounds.size.y / (bounds.size.y + _addTerrain.y);
			terrain.size = bounds.size + PlusTenth + _addTerrain;
		    
			//var sizeFactor = collider.bounds.size.y / (collider.bounds.size.y + addTerrain.y);
			//terrain.size = collider.bounds.size + addTerrain;
			bounds.size = new Vector3(terrain.size.x, bounds.size.y, terrain.size.z);

			// Do raycasting samples over the object to see what terrain heights should be
			var heightMaps = new float[terrain.heightmapWidth, terrain.heightmapHeight];

			var ray = new Ray(new Vector3(bounds.min.x, bounds.max.y + bounds.size.y, bounds.min.z), -Vector3.up);
			var hit = new RaycastHit();
			var meshHeightInverse = 1 / bounds.size.y;
			var rayOrigin = ray.origin;

			var maxLength = terrain.heightmapWidth;
			var maxHeight = terrain.heightmapHeight;

			var stepXZ = new Vector2(bounds.size.x / maxLength, bounds.size.z / maxHeight);
            
			for (var zCount = 0; zCount < maxHeight; zCount++)
			{

				ShowProgressBar(zCount, maxHeight);

				for (var xCount = 0; xCount < maxLength; xCount++)
				{

					var height = 0.0f;
				    foreach (var collider in newMeshColliders)
				    {
				        if (height != 0) break;
				        if (collider.Raycast(ray, out hit, bounds.size.y * 3))
				        {
				            //if(meshColliders.Contains(hit.collider) || newMeshColliders.Contains(hit.collider))
				            //{
				            height = (hit.point.y - bounds.min.y) * meshHeightInverse;
				            height += _shiftHeight;

				            //bottom up
				            if (_bottomTopRadioSelected == 0)
				            {

				                height *= sizeFactor;
				            }

				            //clamp
				            if (height < 0)
				            {

				                height = 0;
				            }
				            //}
				        }
				    }
                    
				    foreach (var collider in meshColliders)
				    {
				        if (height != 0) break;
				        if (collider.Raycast(ray, out hit, bounds.size.y * 3))
				        {
				            //if(meshColliders.Contains(hit.collider) || newMeshColliders.Contains(hit.collider))
				            //{
				            height = (hit.point.y - bounds.min.y) * meshHeightInverse;
				            height += _shiftHeight;

				            //bottom up
				            if (_bottomTopRadioSelected == 0)
				            {

				                height *= sizeFactor;
				            }

				            //clamp
				            if (height < 0)
				            {

				                height = 0;
				            }
				            //}
				        }

				    }
					/*if (Collider.Raycast(ray, out hit, bounds.size.y * 3))
					{
                        //if(meshColliders.Contains(hit.collider) || newMeshColliders.Contains(hit.collider))
                        //{
                            height = (hit.point.y - bounds.min.y) * meshHeightInverse;
                            height += _shiftHeight;

                            //bottom up
                            if (_bottomTopRadioSelected == 0)
                            {

                                height *= sizeFactor;
                            }

                            //clamp
                            if (height < 0)
                            {

                                height = 0;
                            }
                        //}
					}
*/

					heightMaps[zCount, xCount] = height;
					rayOrigin.x += stepXZ[0];
					ray.origin = rayOrigin;
				}

				rayOrigin.z += stepXZ[1];
				rayOrigin.x = bounds.min.x;
				ray.origin = rayOrigin;
			}

			terrain.SetHeights(0, 0, heightMaps);

		    
			CreateTerrainAsset(terrain);
		    Undo.RegisterCreatedObjectUndo(terrainObject, "Model to Terrain");

			EditorUtility.ClearProgressBar();

			if (_addedToScene)
			{
				DestroyImmediate(_sourceGeo);
				_sourceGeo = _storedAsset;
				_addedToScene = false;
			}

		    for (var i = 0; newMeshColliders.Count != 0; i = 0)
		    {
		        var collider = newMeshColliders[i];
		        
		        newMeshColliders.Remove(collider);
		        if (collider == null)
		        {
		            continue;
		        }
		        DestroyImmediate(collider.gameObject.GetComponent<MeshCollider>(), true);
		    }
		    newMeshColliders.Clear();
		}
	    
/*	    public static void GetFullMeshCollider(GameObject obj)
	    {
            
	        var meshFilters = obj.GetComponentsInChildren<MeshFilter>();
	        var combine = new CombineInstance[meshFilters.Length];

	        var i = 0;
	        while (i < meshFilters.Length)
	        {
	            combine[i].mesh = meshFilters[i].sharedMesh;
	            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
	            meshFilters[i].gameObject.SetActive(false);

	            i++;
	        }

	        var addedMesh = false;

	        _oldMeshFilter = obj.GetComponent<MeshFilter>();
	        if (_oldMeshFilter == null)
	        {
	            addedMesh = true;
	            _newMeshFilter = obj.AddComponent<MeshFilter>();
	        }
	        
	        obj.GetComponent<MeshFilter>().sharedMesh = new Mesh();
	        obj.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);
	        _oldMeshCollider = obj.GetComponent<MeshCollider>();
	        if (_oldMeshCollider == null)
	        {
	            _newMeshCollider = obj.AddComponent<MeshCollider>();
	            _newMeshCollider.enabled = true;
	            _newMeshCollider.inflateMesh = true;
	            _newMeshCollider.sharedMesh = addedMesh ? _newMeshFilter.sharedMesh : _oldMeshFilter.sharedMesh;
	        }
	        else
	        {
	            _newMeshCollider = new MeshCollider()
	            {
	                enabled = true,
	                inflateMesh = true,
	                sharedMesh = addedMesh ? _newMeshFilter.sharedMesh : _oldMeshFilter.sharedMesh
	            };
	        }
	        obj.gameObject.SetActive(true);
	    }*/

		private static void CheckIfExistsInScene(GameObject obj)
		{
			if (obj.scene.IsValid()) return;
			_storedAsset = obj;
			_sourceGeo = Instantiate(obj);
			_addedToScene = true;
		}

		private static void CreateTerrainAsset(TerrainData terrainData)
		{
		    var texPath = AssetDatabase.GetAssetPath(_storedAsset == null ? _sourceGeo : _storedAsset);
            texPath = texPath.Substring(0, texPath.LastIndexOf("/") + 1);
		    if (texPath == "") texPath = AssetsFolder;
		    var path = _radioSelection == 0 ? texPath : AssetsFolder + _saveLocation;
			FuzzyTools.CheckIfPathExists(path);
		    
			path += _terrainName + Asset;
		    
			AssetDatabase.CreateAsset(terrainData, AssetDatabase.GenerateUniqueAssetPath(path));
		}

		public static void ShowProgressBar(float progress, float maxProgress)
		{
			var p = progress / maxProgress;
			EditorUtility.DisplayProgressBar("Creating Terrain...", Mathf.RoundToInt(p * 100f) + " %", p);
		}
	}
    public class ConvertTerrainToModel : EditorWindow
    {
        private const string PickOne = "Add Terrain Source: ";
        private const string UsingTerrain = "Using Terrain";
        private const string UsingTerrainData = "Using TerrainData";
        private const string OrThis = "     Or      ";
        private const string SourceTerrain = "Source Terrain (Scene) ";
        private const string SourceTerrainData = "Source TerrainData (Assets)";
        private const string Convert = "Convert";
        private const string SaveFormat = "Save Format";
        private const string TopologyFormat = "Topology Mode";
        private const string SaveResolution = "Converted Resolution";
        private const string ModelName = "New Model Name: ";
        private const string DefaultModelName = "New Model";
        
        private static Terrain _sourceTerrain;
        private static TerrainData _sourceTerrainData;
        
        private static ModelFormat _saveFormat;
        private static TopologyMode _topologyMode;
        private static Resolution _resolution;

        private static Vector3 _terrainPos;
        
        private static int _radioSelection = 0;
        //private const string Instruction = "If you are wanting to add a terrain from Assets, add it to the TerrainData. If you are wanting to add it from the scene, add it directly to the "
        private const string ConvertedLocation = "Save Converted File to:";
        private static readonly string[] RadioOptions = {"Origin File Location", "Custom Location"};
        private static string _saveLocation = FuzzyTools.DefaultModelPath;
        private static GUIStyle _bold = new GUIStyle();
        private static GUIStyle _smallFont = new GUIStyle();
        private static GUIStyle _largeFont = new GUIStyle();
        
        //private static readonly Vector2 MinSize1 = new Vector2(300, 240);
        private static readonly Vector2 OtherMinSize1 = new Vector2(300, 259);
        //private static readonly Vector2 MinSize2 = new Vector2(300, 204);
        private static readonly Vector2 OtherMinSize2 = new Vector2(300, 222);
        

        //private static int _changeNameSelection = 0;
        private static string _modelName = "";
        //private static GameObject _storedAsset = null;
        //private static bool _addedToScene = false;
        private static bool _noTerrainOrData = true;

        
        
        
        
        public static void Init(Terrain source, TerrainData sourceData)
        {
            _sourceTerrain = source;
            _sourceTerrainData = sourceData;
            _saveFormat = FuzzyTools.DefaultModelFormat;
            _topologyMode = FuzzyTools.DefaultTopologyMode;
            _resolution = FuzzyTools.DefaultMeshResolution;
            _radioSelection = 0;
            _saveLocation = FuzzyTools.DefaultModelPath;
            _bold.fontStyle = FontStyle.Bold;
            _smallFont.fontSize = 6;
            _largeFont.fontSize = 15;
            _largeFont.fontStyle = FontStyle.Bold;
            //_changeNameSelection = 0;
            //_storedAsset = null;
            //_addedToScene = false;
            _modelName = _sourceTerrain != null ? _sourceTerrain.name : DefaultModelName;
            
            GetWindow(typeof(ConvertTerrainToModel), true, "Terrain To Model");
        }

        private void OnGUI()
        {
            var textDimensions = GUI.skin.label.CalcSize(new GUIContent(SourceTerrainData));
            EditorGUIUtility.labelWidth = textDimensions.x;
            if(_sourceTerrainData == null && _sourceTerrain == null)
            {
                EditorGUILayout.LabelField(PickOne, _largeFont);
                _sourceTerrain =
                    (Terrain) EditorGUILayout.ObjectField(SourceTerrain, _sourceTerrain, typeof(Terrain), true);
                
                EditorGUILayout.LabelField(OrThis);
                
                _sourceTerrainData = (TerrainData) EditorGUILayout.ObjectField(SourceTerrainData, _sourceTerrainData,
                    typeof(TerrainData), true);
                
                //EditorGUILayout.TextArea("",GUI.skin.horizontalSlider);
                _noTerrainOrData = true;
            }
            else if(_sourceTerrain != null)
            {
                EditorGUILayout.LabelField(UsingTerrain, _largeFont);
                _sourceTerrain =
                    (Terrain) EditorGUILayout.ObjectField(SourceTerrain, _sourceTerrain, typeof(Terrain), true);
                _noTerrainOrData = false;
            }
            else if (_sourceTerrainData != null)
            {
                EditorGUILayout.LabelField(UsingTerrainData, _largeFont);
                _sourceTerrainData = (TerrainData) EditorGUILayout.ObjectField(SourceTerrainData, _sourceTerrainData,
                    typeof(TerrainData), true);
                _noTerrainOrData = false;
            }

            EditorGUILayout.Space();
            
            _modelName = EditorGUILayout.TextField(ModelName, _modelName);
            
            if(_modelName == "") _modelName = _sourceTerrain != null ? _sourceTerrain.name : DefaultModelName;
            if (_modelName == "///") _modelName = "Pupper 3 is Ben The Creator";
            
            //_changeNameSelection = EditorGUILayout.SelectableLabel()
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(ConvertedLocation, _bold);
            _radioSelection = GUILayout.SelectionGrid(_radioSelection, RadioOptions, RadioOptions.Length,
                EditorStyles.radioButton);
            
            minSize = _noTerrainOrData ? OtherMinSize1 : OtherMinSize2;
            if (_radioSelection == 1)
            {
                //minSize = _noTerrainOrData ? OtherMinSize1 : OtherMinSize2;
                _saveLocation = EditorGUILayout.TextField(_saveLocation);
            }
            else
            {
                var texPath = _sourceTerrain  != null
                        ? AssetDatabase.GetAssetPath(_sourceTerrain.terrainData)
                        : AssetDatabase.GetAssetPath(_sourceTerrainData);

                    EditorGUI.BeginDisabledGroup(true);
                if (texPath == "") EditorGUILayout.TextField("");
                else
                {
                    if (texPath.StartsWith("Assets/")) texPath = texPath.Substring(6, texPath.Length -6);
                    texPath = texPath.Substring(0, texPath.LastIndexOf("/") + 1);
                    
                    EditorGUILayout.TextField(texPath);
                }
                EditorGUI.EndDisabledGroup();
                //minSize = _noTerrainOrData ? MinSize1 : MinSize2;
            }
            if (_saveLocation.Length > 0 && !_saveLocation.EndsWith("/")) _saveLocation += "/";
		    
            EditorGUILayout.Space();
            
            _saveFormat = (ModelFormat) EditorGUILayout.EnumPopup(SaveFormat, _saveFormat);
            _topologyMode = (TopologyMode) EditorGUILayout.EnumPopup(TopologyFormat, _topologyMode);
            _resolution = (Resolution) EditorGUILayout.EnumPopup(SaveResolution, _resolution);
            EditorGUILayout.Space();
            
            EditorGUI.BeginDisabledGroup(_sourceTerrain == null && _sourceTerrainData == null);
            if (GUILayout.Button(Convert))
            {
                ConvertTerrain();
            }
            
            EditorGUI.EndDisabledGroup();
            
        }

        private void ConvertTerrain()
        {
            //string fileName = EditorUtility.SaveFilePanel("Export .obj file", "", "Terrain", "obj");
            if (_sourceTerrainData == null && _sourceTerrain != null) _sourceTerrainData = _sourceTerrain.terrainData;
            if (_sourceTerrainData == null) return;

            var path = _saveLocation;
            if(_radioSelection == 0)
            {
                path = AssetDatabase.GetAssetPath(_sourceTerrainData);
                path = path.Substring(0, path.LastIndexOf("/") + 1);
            }
            FuzzyTools.CheckIfPathExists(path);
            if (path.StartsWith("Assets/"))
                path = path.Substring(6, path.Length - 6);
            if (!path.StartsWith("/")) path = "/" + path;
            
            _terrainPos = _sourceTerrain != null ? _sourceTerrain.transform.position : Vector3.zero;
            
            var width = _sourceTerrainData.heightmapWidth;// - 1;
            var height = _sourceTerrainData.heightmapHeight;// - 1;
            var terrainSize = _sourceTerrainData.size;
            var saveRes = (int)Mathf.Pow(2, (int) _resolution);
            terrainSize = new Vector3(terrainSize.x / (width - 1) * saveRes, terrainSize.y,
                terrainSize.z / (height - 1) * saveRes);
            var uvScale = new Vector2(1.0f / (width - 1), 1.0f / height - 1);
            var heights = _sourceTerrainData.GetHeights(0, 0, width, height);

            width = (width - 1) / saveRes + 1;
            height = (height - 1) / saveRes + 1;

            var vertCount = width * height;
            
            var verts = new Vector3[vertCount];
            var uVs = new Vector2[vertCount];
            //var normals = new Vector3[vertCount];

            var polygons = _topologyMode == TopologyMode.Triangles
                ? new int[(width - 1) * (height - 1) * 6]
                : new int[(width - 1) * (height - 1) * 4];
            
            //Find Verts positions and UVs
            for (var y = 0; y < height - 1; y++)
            {
                for (var x = 0; x < width - 1; x++)
                {
                    verts[y * width + x] =
                        Vector3.Scale(terrainSize, new Vector3(-y, heights[x * saveRes, y * saveRes], x)) + _terrainPos;
                    uVs[y * width + x] = Vector2.Scale(new Vector2(x*saveRes, y*saveRes), uvScale);
                }
            }
            
            var polygonIndex = 0;
            if (_topologyMode == TopologyMode.Triangles)
            {
                // Build triangle indices: 3 indices into vertex array for each triangle
                for (int y = 0; y < height - 1; y++)
                {
                    for (int x = 0; x < width - 1; x++)
                    {
                        // For each grid cell output two triangles
                        polygons[polygonIndex++] = (y * width) + x;
                        polygons[polygonIndex++] = ((y + 1) * width) + x;
                        polygons[polygonIndex++] = (y * width) + x + 1;
 
                        polygons[polygonIndex++] = ((y + 1) * width) + x;
                        polygons[polygonIndex++] = ((y + 1) * width) + x + 1;
                        polygons[polygonIndex++] = (y * width) + x + 1;
                    }
                }
            }
            else
            {
                // Build quad indices: 4 indices into vertex array for each quad
                for (var y = 0; y < height - 1; y++)
                {
                    for (var x = 0; x < width - 1; x++)
                    {
                        // For each grid cell output one quad
                        polygons[polygonIndex++] = (y * width) + x;
                        polygons[polygonIndex++] = ((y + 1) * width) + x;
                        polygons[polygonIndex++] = ((y + 1) * width) + x + 1;
                        polygons[polygonIndex++] = (y * width) + x + 1;
                    }
                }
            }
            
            switch(_saveFormat)
            {
                //case ModelFormat.FBX://TODO
                //    break;
                case ModelFormat.OBJ:
                    path += _modelName;
                    CreateModel.ObjFile<ConvertTerrainToModel>(verts, polygons, null, uVs, path, _topologyMode);
                    break;
            }

            if ((int) _resolution > 2)
            {
                //EditorUtility.DisplayDialog("Saving Model", "Depending on your settings, this may take some time.",
                //    "Okay");
                EditorUtility.DisplayDialog("Saving model",
                    "Depending on your settings, this may take some time. \n" +
                    " Smaller Resolutions may not show a progress bar.", "Okay");
            }
            
            _sourceTerrain = null;
            _sourceTerrainData = null;
        }
    }
    public class ConvertMeshToModel : EditorWindow
    {
        private static GameObject _sourceObj;
        private static Mesh _sourceMesh;

        private static string _modelName = "New Model";
        private static string _saveLocation = FuzzyTools.DefaultModelPath;
        private static ModelFormat _saveFormat = FuzzyTools.DefaultModelFormat;
        
        private static readonly GUIStyle Bold = new GUIStyle();
        private static readonly GUIStyle SmallFont = new GUIStyle();
        private static readonly GUIStyle LargeFont = new GUIStyle();
        
        private const string PickOne = "Add Mesh Source: ";
        private const string UsingGameObj = "Using GameObject";
        private const string UsingMesh = "Using Mesh";
        private const string OrThis = "     Or      ";
        private const string SourceGameObj = "Source GameObject ";
        private const string SourceMesh = "Source Mesh";
        private const string ModelName = "New Model Name: ";
        private const string DefaultModelName = "New Model";
        private const string ConvertedLocation = "Save Converted File to:";
        private const string SaveFormat = "Save Format";
        private const string ConvertButton = "Convert";
        
        private static readonly string[] RadioOptions = {"Origin File Location", "Custom Location"};
        private static int _radioSelection = 0;
        
        private static readonly Vector2 OtherMinSize1 = new Vector2(300, 215);
        private static readonly Vector2 OtherMinSize2 = new Vector2(300, 178);

        
        public static void Init(GameObject obj, Mesh mesh)
        {
            _sourceObj = obj;
            _sourceMesh = mesh;
            _saveLocation = FuzzyTools.DefaultModelPath;
            Bold.fontStyle = FontStyle.Bold;
            SmallFont.fontSize = 6;
            LargeFont.fontSize = 15;
            LargeFont.fontStyle = FontStyle.Bold;
            _modelName = DefaultModelName;
            _radioSelection = 0;
            _saveFormat = FuzzyTools.DefaultModelFormat;
            GetWindow(typeof(ConvertMeshToModel), true, "Mesh to Model");
        }
        
        private void OnGUI()
        {
            var noMesh = false;
            if(_sourceObj == null && _sourceMesh == null)
            {
                EditorGUILayout.LabelField(PickOne, LargeFont);
                _sourceObj =
                    (GameObject) EditorGUILayout.ObjectField(SourceGameObj, _sourceObj, typeof(GameObject), true);
                EditorGUILayout.LabelField(OrThis);
                _sourceMesh = (Mesh) EditorGUILayout.ObjectField(SourceMesh, _sourceMesh,
                    typeof(Mesh), false);
                
                noMesh = true;
            }
            else if (_sourceObj != null)
            {
                EditorGUILayout.LabelField(UsingGameObj, LargeFont);
                _sourceObj =
                    (GameObject) EditorGUILayout.ObjectField(SourceGameObj, _sourceObj, typeof(GameObject), true);
                noMesh = false;
            }
            else if (_sourceMesh != null)
            {
                EditorGUILayout.LabelField(UsingMesh, LargeFont);
                _sourceMesh = (Mesh) EditorGUILayout.ObjectField(SourceMesh, _sourceMesh, typeof(Mesh), false);
                noMesh = false;
            }
            EditorGUILayout.Space();
            
            _modelName = EditorGUILayout.TextField(ModelName, _modelName);
            if (_modelName == "")
                _modelName = _sourceMesh != null ? _sourceMesh.name :
                    _sourceObj != null ? _sourceObj.name : DefaultModelName;
            
            if (_modelName == "///") _modelName = "Secret Catto is Russel";
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(ConvertedLocation, Bold);
            _radioSelection = GUILayout.SelectionGrid(_radioSelection, RadioOptions, RadioOptions.Length,
                EditorStyles.radioButton);
            
            minSize = noMesh ? OtherMinSize1 : OtherMinSize2;

            var path = "";
            if (_radioSelection == 1)
            {
                _saveLocation = EditorGUILayout.TextField(_saveLocation);
                path = _saveLocation;
            }
            else
            {
                var texPath = _sourceMesh  != null
                    ? AssetDatabase.GetAssetPath(_sourceMesh)
                    : AssetDatabase.GetAssetPath(_sourceObj);
                

                EditorGUI.BeginDisabledGroup(true);
                if (texPath == "") EditorGUILayout.TextField("");
                else
                {
                    if (texPath.StartsWith("Assets/")) texPath = texPath.Substring(6, texPath.Length -6);
                    texPath = texPath.Substring(0, texPath.LastIndexOf("/") + 1);
                    
                    EditorGUILayout.TextField(texPath);
                }
                EditorGUI.EndDisabledGroup();
                path = texPath;
                //minSize = _noTerrainOrData ? MinSize1 : MinSize2;
            }
            if (_saveLocation.Length > 0 && !_saveLocation.EndsWith("/")) _saveLocation += "/";

            EditorGUILayout.Space();
            
            _saveFormat = (ModelFormat) EditorGUILayout.EnumPopup(SaveFormat, _saveFormat);            
            
            EditorGUI.BeginDisabledGroup(noMesh);
            if (GUILayout.Button(ConvertButton))
            {
                
                ConvertModel(_sourceObj != null? GetFullMesh(_sourceObj) : _sourceMesh, path, _modelName);

            }
            EditorGUI.EndDisabledGroup();
        }

        private static Mesh GetFullMesh(GameObject obj)
        {
            var meshFilters = obj.GetComponentsInChildren<MeshFilter>();
            var combine = new CombineInstance[meshFilters.Length];

            var i = 0;
            while (i < meshFilters.Length)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                i++;
            }
            var myMesh = new Mesh();
            
            myMesh.CombineMeshes(combine);
            
            return myMesh;
        }

        private static void ConvertModel(Mesh newMesh, string path, string name)
        {
            
            var verts = newMesh.vertices;
            var newObj = new GameObject();
            var newFilter = newObj.AddComponent<MeshFilter>();
            newFilter.mesh = newMesh;
            
            for (var j = 0; j < verts.Length; j++)
            {
                var newPos = newFilter.transform.TransformPoint(verts[j]);
                
                verts[j] = new Vector3(-newPos.x, newPos.y, newPos.z);
            }

            var normals = newMesh.normals;
            for (var i = 0; i < normals.Length; i++)
            {
                var newPos = newFilter.transform.TransformDirection(normals[i]);
                
                normals[i] = new Vector3(-newPos.x, newPos.y, newPos.z);
            }
            
            DestroyImmediate(newObj);
            
            FuzzyTools.CheckIfPathExists(path);
            if (path.StartsWith("Assets/"))
                path = path.Substring(6, path.Length - 6);
            if (!path.StartsWith("/")) path = "/" + path;
            
            path += name;
            
            if(_saveFormat == ModelFormat.OBJ)
            {
                CreateModel.ObjFile<ConvertMeshToModel>(verts, newMesh.triangles.Reverse().ToArray(), normals,
                    newMesh.uv, path, TopologyMode.Triangles);
            }

            Init(null, null);
            AssetDatabase.Refresh();
        }
    }
    
    #endregion
    #region InitilizeOnLoad Functions
    [InitializeOnLoad]
    public class Initialize : MonoBehaviour
    {
        static bool newSceneNotSaved = false;
        static bool comingFromNewScene = false;
        
        static Initialize()
        {
            if (SceneManager.GetActiveScene().path != null)
            {
                FuzzyTools.previousScene = SceneManager.GetActiveScene().path;
                FuzzyTools.newScene = SceneManager.GetActiveScene().path;
                
/*#if UNITY_2018_2_OR_NEWER
EditorApplication.quitting += Closing;
#endif*/

            }
            FuzzyTools.recentScenes.Capacity = FuzzyTools.keepTrackOfRecentScenes;
            for(int i = FuzzyTools.keepTrackOfRecentScenes; i>=0; i--)
            {
                FuzzyTools.recentScenes.Add(null);
            }
            //EditorSceneManager.sceneClosed += SceneClosed; //If the bug making this useless ever gets fixed I may replace the rest with it.
            EditorSceneManager.sceneOpened += OpenedScene;
            EditorSceneManager.newSceneCreated += NewSceneCreated;
            EditorSceneManager.sceneSaved += SceneSaved;
            SetEditorPrefs();
            FindLocators();
        }

        private static void SetEditorPrefs()
        {
            FuzzyTools.keepTrackOfRecentScenes =
                FuzzyHelper.GetEditorPrefInt("TrackedScenesCount", FuzzyTools.keepTrackOfRecentScenes);
            
            FuzzyTools.maxTerrainSplit = FuzzyHelper.GetEditorPrefInt("MaxTerrainSlices", FuzzyTools.maxTerrainSplit);

            if (EditorPrefs.HasKey("IgnoreTypesCount"))
            {
                var count = FuzzyHelper.GetEditorPrefInt("IgnoreTypesCount", FuzzyTools.ignoreTypes.Count);
                
                UpdateList(FuzzyTools.ignoreTypes, count);
                for (var i = 0; i < count; i++)
                {
                    if (!EditorPrefs.HasKey("IgnoreType" + i)) continue;
                    FuzzyTools.ignoreTypes[i] = Type.GetType(EditorPrefs.GetString("IgnoreType" + i));
                }
            }

            FuzzyTools.maxPhysicsIterations =
                FuzzyHelper.GetEditorPrefInt("MaxPhysicsIterations", FuzzyTools.maxPhysicsIterations);
            
            FuzzyTools.subLocation = FuzzyHelper.GetEditorPrefString("PrefabLocation", FuzzyTools.subLocation);

            FuzzyTools.locatorName = FuzzyHelper.GetEditorPrefString("LocatorName", FuzzyTools.locatorName);

            FuzzyTools.locatorColor = FuzzyHelper.GetEditorPrefColor("LocatorColor_R", FuzzyTools.locatorColor);

            FuzzyTools.locatorScale = FuzzyHelper.GetEditorPrefFloat("LocatorSize", FuzzyTools.locatorScale);

            FuzzyTools.previousScene =
                FuzzyHelper.GetEditorPrefString(Application.productName + "PreviousScene", FuzzyTools.previousScene);

            FuzzyTools.keepTrackOfRecentScenes =
                FuzzyHelper.GetEditorPrefInt("RecentScenesCount", FuzzyTools.keepTrackOfRecentScenes);

            for (var i = 0; i < FuzzyTools.keepTrackOfRecentScenes; i++)
            {
                if (i < FuzzyTools.recentScenes.Count)
                {
                    FuzzyTools.recentScenes[i] =
                        FuzzyHelper.GetEditorPrefString(Application.productName + "RecentScenes" + i, null);
                }
                else
                {
                    FuzzyTools.recentScenes.Add(
                        FuzzyHelper.GetEditorPrefString(Application.productName + "RecentScenes" + i, null));
                }

                if (i >= FuzzyTools.recentScenes.Count) continue;
                if(FuzzyTools.recentScenes[i] == null) FuzzyTools.recentScenes.RemoveAt(i);
            }

            FuzzyTools.showAllGameObjectsOfName = FuzzyHelper.GetEditorPrefBool("ShowAllGameObjectsOfName");
            FuzzyTools.autoAddSpaces = FuzzyHelper.GetEditorPrefBool("AutoAddSpaces");

            FuzzyTools.DefaultTerrainPath = FuzzyHelper.GetEditorPrefString("ConvertTerrainPath");
            FuzzyTools.DefaultImagePath = FuzzyHelper.GetEditorPrefString("ConvertImagePath");
            FuzzyTools.DefaultModelPath = FuzzyHelper.GetEditorPrefString("ConvertModelPath");
            FuzzyTools.DefaultModelFormat = (ModelFormat)FuzzyHelper.GetEditorPrefInt("ConvertFormat");
            FuzzyTools.DefaultTopologyMode =
                (TopologyMode) FuzzyHelper.GetEditorPrefInt("DefaultTopologyMode");
            FuzzyTools.DefaultMeshResolution =
                (Resolution) FuzzyHelper.GetEditorPrefInt("DefaultMeshResolution");
        }
        
        private static void UpdateList(List<Type> list, int length)
        {
            if(length>list.Count)
            {
                for(var i = list.Count; i<length; i++)
                {
                    list.Add(null);
                }
            }else
            {
                while (length != list.Count)
                {
                    list.RemoveAt(list.Count - 1);
                }
            }
        }

        private static void FindLocators()
        {
            var objs = FindObjectsOfType<GameObject>();
            foreach (var obj in objs)
            {
                if (obj.name != FuzzyTools.locatorName) continue;
                FuzzyTools.locators.Add(obj.transform);
            }
        }
        /*static void SceneClosed(Scene closedScene)
        {
            if (closedScene.path != null)
            {
                FuzzyTools.previousScene = closedScene.path;
                if (!FuzzyTools.recentScenes.Contains(closedScene.path) && closedScene != EditorSceneManager.GetActiveScene())
                {
                    FuzzyTools.recentScenes.Insert(FuzzyTools.sceneCount, FuzzyTools.previousScene);
                    for (int i = FuzzyTools.keepTrackOfRecentScenes; i >= 0; i--)
                    {

                        if (i < FuzzyTools.keepTrackOfRecentScenes)
                        {
                            FuzzyTools.recentScenes.Insert(i + 1, FuzzyTools.recentScenes[i]);

                        }
                        else
                        {
                            FuzzyTools.recentScenes.RemoveAt(i);
                        }
                    }
                    FuzzyTools.recentScenes.Insert(0, closedScene.path);
                }
            }
        }*///If the bug making this useless ever gets fixed I may replace the rest with it.

        private static void NewSceneCreated(Scene newScene, NewSceneSetup setup, NewSceneMode mode)
        {
            FuzzyTools.previousScene = FuzzyTools.newScene;
            newSceneNotSaved = true;
            UpdatePreviousScenes();
        }

        private static void SceneSaved(Scene savedScene)
        {
            if (newSceneNotSaved)
            {
                FuzzyTools.newScene = savedScene.path;
                newSceneNotSaved = false;
                comingFromNewScene = true;
            }
        }

        private static void OpenedScene(Scene newScene, OpenSceneMode mode)
        {
            if (FuzzyTools.newScene != newScene.path || comingFromNewScene)
            {
                FuzzyTools.previousScene = FuzzyTools.newScene;
                FuzzyTools.newScene = newScene.path;
                

                UpdatePreviousScenes();
            }

            FindLocators();
        }

        private static void UpdatePreviousScenes()
        {
            
            FuzzyHelper.SetEditorPrefString(Application.productName + "PreviousScene", FuzzyTools.previousScene);
            
            if (!FuzzyTools.recentScenes.Contains(FuzzyTools.previousScene))
            {
                if (FuzzyTools.recentScenes.Count == FuzzyTools.keepTrackOfRecentScenes)
                    FuzzyTools.recentScenes.RemoveAt(FuzzyTools.keepTrackOfRecentScenes-1);
                    
                FuzzyTools.recentScenes.Insert(0, FuzzyTools.previousScene);

            }else
            {
                FuzzyTools.recentScenes.RemoveAt(FuzzyTools.recentScenes.IndexOf(FuzzyTools.previousScene));
                FuzzyTools.recentScenes.Insert(0, FuzzyTools.previousScene);
            }
            
            for (var i = 0; i < FuzzyTools.keepTrackOfRecentScenes; i++)
            {
                if (i>=FuzzyTools.recentScenes.Count || FuzzyTools.recentScenes[i] == null) continue;
                FuzzyHelper.SetEditorPrefString(Application.productName + "RecentScenes" + i,
                    FuzzyTools.recentScenes[i]);
            }
        }

        /*private void Closing()
        {
            FuzzyHelper.RemoveKey("PreviousScene");
        }*/
        
        
    }
    
    [InitializeOnLoad]
    public class HierarchyTools
    {
        private static bool _everyOther = true;
        
        /*******************************READONLY_DEFAULTS*******************************/
        public static readonly Color DefaultSkin = new Color(.76f, .76f, .76f);
        public static readonly Color DefaultProSkin = new Color(.26f,.26f,.26f);
        public static readonly Color DefaultSelected = new Color(0.24f, 0.48f, 0.90f);
        public static readonly Color DefaultSelectedInactive = new Color(.56f,.56f,.56f);
        public static readonly Color DefaultFontColor = Color.black;
        public static readonly Color DefaultPrefabFontColor = new Color(.07f,.2f,.5f, 1);
        public const float DefaultInactiveAlpha = .6f;
        private static readonly Vector2 Offset = new Vector2(0, 2);
        /*******************************************************************************/
        
        
        
        static HierarchyTools()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
            GetPreferences();
        }
    
        private static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj == null) return;
    
            var fontColor = FuzzyTools.gameObjectFontColor;
            var defaultColor = EditorGUIUtility.isProSkin ? DefaultProSkin : DefaultSkin;
            var backgroundColor = defaultColor;
            var styleFont = FuzzyTools.standardFont;
            var gameObj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (gameObj == null) return;
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
            var prefabType = PrefabUtility.GetPrefabType(obj);
#endif
#if UNITY_2018_3_OR_NEWER
            var prefabType = PrefabUtility.GetPrefabAssetType(obj);
#endif
            var offsetRect = new Rect(selectionRect.position + Offset, selectionRect.size);
            
            //var activeWindow = EditorWindow.focusedWindow;
            
            switch (FuzzyTools.colorMode)
            {
                case CustomColorType.Off:
                    
                    break;
                case CustomColorType.AutoColors:
                    /*******************AUTO_COLOR_MODE****************************/
                    var gameObjectFontColor = FuzzyTools.gameObjectFontColor;
                    var prefabOrgFontColor = FuzzyTools.prefabOrgFontColor;
                    var inActiveColor = FuzzyTools.inActiveColor;
                    var inActiveFontColor = FuzzyTools.inActiveFontColor;
                    var standardFont = FuzzyTools.standardFont;
                    var prefebFont = FuzzyTools.prefebFont;
                    var autoInvertColors = FuzzyTools.autoInvertColors;
                    /**************************************************************/
                    
                    if (Selection.instanceIDs.Contains(instanceID))
                    {
                        backgroundColor = DefaultSelected;
                        /*if(activeWindow == null || activeWindow.ToString() != "(UnityEditor.SceneHierarchyWindow)")
                        {
                            backgroundColor = DefaultSelectedInactive;
                        }*/
                    }
                    else if (gameObj.activeInHierarchy == false)
                    {
                        backgroundColor = inActiveColor;
                        fontColor = autoInvertColors
                            ? new Color(1 - inActiveColor.r, 1 - inActiveColor.g, 1 - inActiveColor.b, 1)
                            : inActiveFontColor;
                        if (fontColor == inActiveColor) fontColor += new Color(.25f, .25f, .25f);
                    }
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
                    else if (prefabType == PrefabType.PrefabInstance)
#endif
#if UNITY_2018_3_OR_NEWER
                    else if(prefabType == PrefabAssetType.Regular)
#endif
                    {
                        styleFont = prefebFont;
                        fontColor = gameObj.activeInHierarchy? prefabOrgFontColor: new Color(prefabOrgFontColor.r,
                            prefabOrgFontColor.g, prefabOrgFontColor.b, DefaultInactiveAlpha);
                    }
                    else
                    {
                        fontColor = gameObjectFontColor;
                        styleFont = standardFont;
                    }
                    EditorGUI.DrawRect(selectionRect, backgroundColor);
                    EditorGUI.LabelField(offsetRect, obj.name, new GUIStyle()
                        {
                            normal = new GUIStyleState() {textColor = fontColor},
                            fontStyle = styleFont
                        }
                    );
                    break;
                case CustomColorType.CustomColors:
                    
                    styleFont = FontStyle.Normal;
                    
                    if (Selection.instanceIDs.Contains(instanceID))
                    {
                        backgroundColor = DefaultSelected;
                        /*if(activeWindow == null || activeWindow.ToString() != "(UnityEditor.SceneHierarchyWindow)")
                        {
                            backgroundColor = DefaultSelectedInactive;
                        }*/
                    }
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
                    else if (prefabType == PrefabType.None)
#endif
#if UNITY_2018_3_OR_NEWER              
                    else if(prefabType == PrefabAssetType.NotAPrefab)
#endif
                    {
                        fontColor = DefaultFontColor;
                        fontColor.a = gameObj.activeInHierarchy ? 1 : DefaultInactiveAlpha;
                        if (fontColor == backgroundColor) fontColor += new Color(.25f, .25f, .25f);
                    }
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
                    else if (prefabType == PrefabType.PrefabInstance)
#endif
#if UNITY_2018_3_OR_NEWER
                    else if(prefabType == PrefabAssetType.Regular)
#endif
                    {
                        fontColor = DefaultPrefabFontColor;
                        fontColor.a = gameObj.activeInHierarchy ? 1 : DefaultInactiveAlpha;
                    }
                    if (backgroundColor != DefaultSelected && FuzzyTools.iDToBackgroundColor.ContainsKey(instanceID))
                    {
                        backgroundColor = FuzzyTools.iDToBackgroundColor[instanceID];
                    }
                    if (FuzzyTools.iDToFontColor.ContainsKey(instanceID))
                    {
                        fontColor = FuzzyTools.iDToFontColor[instanceID];
                    }
                    if(FuzzyTools.iDToFontStyle.ContainsKey(instanceID))
                    {
                        styleFont = FuzzyTools.iDToFontStyle[instanceID];
                    }
                    EditorGUI.DrawRect(selectionRect, backgroundColor);
                    EditorGUI.LabelField(offsetRect, obj.name, new GUIStyle()
                        {
                            normal = new GUIStyleState() {textColor = fontColor},
                            fontStyle = styleFont
                        }
                    );
                    break;
                case CustomColorType.Hierarchy:
                    /****************HIERARCHY_COLOR_MODE**************************/
                    var hierarchyColor1 = FuzzyTools.hierarchyColor1;
                    var hierarchyColor2 = FuzzyTools.hierarchyColor2;
                    var hierarchyColor3 = FuzzyTools.hierarchyColor3;
                    var hierarchyColor4 = FuzzyTools.hierarchyColor4;
                    var hierarchyColor5 = FuzzyTools.hierarchyColor5;
                    /**************************************************************/
                    
                    styleFont = FontStyle.Normal;
                    if (Selection.instanceIDs.Contains(instanceID))
                    {
                        backgroundColor = DefaultSelected;
                        /*if(activeWindow == null || activeWindow.ToString() != "(UnityEditor.SceneHierarchyWindow)")
                        {
                            backgroundColor = DefaultSelectedInactive;
                        }*/
                    }
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
                    else if (prefabType == PrefabType.None)
#endif
#if UNITY_2018_3_OR_NEWER              
                    else if(prefabType == PrefabAssetType.NotAPrefab)
#endif
                    {
                        fontColor = DefaultFontColor;
                        fontColor.a = gameObj.activeInHierarchy ? 1 : DefaultInactiveAlpha;
                        if (fontColor == backgroundColor) fontColor += new Color(.25f, .25f, .25f);
                    }
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
                    else if (prefabType == PrefabType.PrefabInstance)
#endif
#if UNITY_2018_3_OR_NEWER
                    else if(prefabType == PrefabAssetType.Regular)
#endif
                    {
                        fontColor = DefaultPrefabFontColor;
                        fontColor.a = gameObj.activeInHierarchy ? 1: DefaultInactiveAlpha;
                    }
                    if (backgroundColor != DefaultSelected)
                    {
                        var parentCount = 0.0f;
                        var parent = gameObj.transform.parent;
                        while (parent != null)
                        {
                            parentCount += .05f;
                            parent = parent.parent;
                        }
                        if (parentCount > 0 && parentCount <= .25)
                        {
                            backgroundColor = hierarchyColor1 - (hierarchyColor1 * parentCount);
                        }
                        else if (parentCount > .25f && parentCount <= .5f)
                        {
                            parentCount -= .25f;
                            backgroundColor = hierarchyColor2 - (hierarchyColor2 * (parentCount *2));
                        }
                        else if (parentCount > .5f && parentCount <= .75f)
                        {
                            parentCount -= .5f;
                            backgroundColor = hierarchyColor3 - (hierarchyColor3 *(parentCount *2));
                        }
                        else if (parentCount > .75f && parentCount <= 1f)
                        {
                            parentCount -= .75f;
                            backgroundColor = hierarchyColor4 - (hierarchyColor4 * (parentCount *2));
                        }else if (parentCount > 1f && parentCount <= 1.5f)
                        {
                            parentCount -= 1f;
                            backgroundColor = hierarchyColor5 - (hierarchyColor5 * (parentCount *2));
                        }
                    }
                    EditorGUI.DrawRect(selectionRect, backgroundColor);
                    EditorGUI.LabelField(offsetRect, obj.name, new GUIStyle()
                        {
    
                            normal = new GUIStyleState() {textColor = fontColor},
                            fontStyle = styleFont
                        }
                    );
                    break;
                case CustomColorType.VariedColor:
                    /***************VARIED_COLOR_MODE*****************/
                    var primaryColor = FuzzyTools.PrimaryColor;
                    var secondaryColor = FuzzyTools.secondaryColor;
                    /*************************************************/
                    
                    styleFont = FontStyle.Normal;
                    if (Selection.instanceIDs.Contains(instanceID))
                    {
                        backgroundColor = DefaultSelected;
                        /*if(activeWindow == null || activeWindow.ToString() != "(UnityEditor.SceneHierarchyWindow)")
                        {
                            backgroundColor = DefaultSelectedInactive;
                        }*/
                    }
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
                    else if (prefabType == PrefabType.None)
#endif
#if UNITY_2018_3_OR_NEWER              
                    else if(prefabType == PrefabAssetType.NotAPrefab)
#endif
                    {
                        fontColor = DefaultFontColor;
                        fontColor.a = gameObj.activeInHierarchy ? 1 : DefaultInactiveAlpha;
                        if (fontColor == backgroundColor) fontColor += new Color(.25f, .25f, .25f);
                    }
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
                    else if (prefabType == PrefabType.PrefabInstance)
#endif
#if UNITY_2018_3_OR_NEWER
                    else if(prefabType == PrefabAssetType.Regular)
#endif
                    {
                        fontColor = DefaultPrefabFontColor;
                        fontColor.a = gameObj.activeInHierarchy ? 1 : DefaultInactiveAlpha;
                    }
                    if (backgroundColor != DefaultSelected)
                    {
                        backgroundColor = _everyOther ? primaryColor : secondaryColor;
                    }
                    _everyOther = !_everyOther;
                    EditorGUI.DrawRect(selectionRect, backgroundColor);
                    EditorGUI.LabelField(offsetRect, obj.name, new GUIStyle()
                        {
    
                            normal = new GUIStyleState() {textColor = fontColor},
                            fontStyle = styleFont
                        }
                    );
                    break;
            }
        }

        private static void GetPreferences()
        {
            FuzzyTools.inActiveColor = EditorGUIUtility.isProSkin ? DefaultSkin : DefaultProSkin;
            FuzzyTools.PrimaryColor = EditorGUIUtility.isProSkin ? DefaultProSkin : DefaultSkin;
            FuzzyTools.colorMode =
                (CustomColorType) FuzzyHelper.GetEditorPrefInt("colorMode", (int) FuzzyTools.colorMode);
            FuzzyTools.gameObjectFontColor =
                FuzzyHelper.GetEditorPrefColor("gameObjectFontColor", FuzzyTools.gameObjectFontColor);
            FuzzyTools.prefabOrgFontColor =
                FuzzyHelper.GetEditorPrefColor("prefabOrgFontColor", FuzzyTools.prefabOrgFontColor);
            FuzzyTools.inActiveColor = FuzzyHelper.GetEditorPrefColor("inActiveColor", FuzzyTools.inActiveColor);
            FuzzyTools.inActiveFontColor =
                FuzzyHelper.GetEditorPrefColor("inActiveFontColor", FuzzyTools.inActiveFontColor);
            FuzzyTools.standardFont =
                (FontStyle) FuzzyHelper.GetEditorPrefInt("standardFont", (int) FuzzyTools.standardFont);
            FuzzyTools.prefebFont = (FontStyle) FuzzyHelper.GetEditorPrefInt("prefebFont", (int)FuzzyTools.prefebFont);
            FuzzyTools.autoInvertColors =
                FuzzyHelper.GetEditorPrefBool("autoInvertColors", FuzzyTools.autoInvertColors);
            FuzzyTools.hierarchyColor1 = FuzzyHelper.GetEditorPrefColor("hierarchyColor1", FuzzyTools.hierarchyColor1);
            FuzzyTools.hierarchyColor2 = FuzzyHelper.GetEditorPrefColor("hierarchyColor2", FuzzyTools.hierarchyColor2);
            FuzzyTools.hierarchyColor3 = FuzzyHelper.GetEditorPrefColor("hierarchyColor3", FuzzyTools.hierarchyColor3);
            FuzzyTools.hierarchyColor4 = FuzzyHelper.GetEditorPrefColor("hierarchyColor4", FuzzyTools.hierarchyColor4);
            FuzzyTools.hierarchyColor5 = FuzzyHelper.GetEditorPrefColor("hierarchyColor5", FuzzyTools.hierarchyColor5);
            FuzzyTools.PrimaryColor = FuzzyHelper.GetEditorPrefColor("primaryColor", FuzzyTools.PrimaryColor);
            FuzzyTools.secondaryColor = FuzzyHelper.GetEditorPrefColor("secondaryColor", FuzzyTools.secondaryColor);
            FuzzyTools.uniformChangeColors =
                FuzzyHelper.GetEditorPrefBool("UniformChangeColors", FuzzyTools.uniformChangeColors);
        }
    }
    #endregion
    #region ModelFunctions

    public class CreateModel
    {
        private const int Two = 2;
        private const int Tris = 3;
        private const int Quads = 4;
        private const int VertCap = 20;
        private const int UVCap = 22;
        private const int TrisCap = 43;

        private const string OBJ = ".obj";
        private const string MakingVerts = "Calculating Vertecies";
        private const string MakingUVs = "Generating UVs";
        private const string MakingTris = "Creating Tris";
        private const string MakingQuads = "Creating Quads";
        
        private static int _counter = 0;
        private static int _currentCount = 0;
        private static int _maxCount = 0;
        private const int UpdateSpeed = 10000;
        
        public static void ObjFile<T>(IList<Vector3> verts, int[] polys, Vector3[] normals, IList<Vector2> uVs, string newName, TopologyMode topoMode) where T:EditorWindow
        {
            var validName = "/" + newName;
            var nameCount = 0;
            //Debug.Log("Checking: " + Application.dataPath + validName + OBJ);
            while (File.Exists(Application.dataPath + validName + OBJ))
            {
                //Debug.Log(Application.dataPath + validName + OBJ + " Already Exists");
                nameCount++;
                validName = "/" +  newName + " " + nameCount.ToString("00");
                
               // Debug.Log("Trying: " + Application.dataPath + validName + OBJ);
            }
            var saveTo = Application.dataPath + validName + OBJ;
            //var savingModel = false;
            var streamWriter = new StreamWriter(saveTo);
            try
            {
                streamWriter.WriteLine("# Convert Terrain to OBJ File");
                //Debug.Log("Starting To Write");
                //write verts
                Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                _counter = _currentCount = 0;
                _maxCount = (verts.Count * Two +
                             (topoMode == TopologyMode.Triangles
                                 ? polys.Length / Tris
                                 : polys.Length / Quads)) / UpdateSpeed;
                for (var i = 0; i < verts.Count; i++)
                {
                    ProgressBar(MakingVerts);
                    var stringB = new StringBuilder("v ", VertCap);

                    
                    stringB.Append(verts[i].x.ToString()).Append(" ").Append(verts[i].y.ToString()).Append(" ")
                        .Append(verts[i].z.ToString());
                    streamWriter.WriteLine(stringB);
                }
                
                //Write normals
                if(normals != null)
                {
                    for (var i = 0; i < normals.Length; i++)
                    {
                        ProgressBar("Writing Normals");
                        var stringB = new StringBuilder("vn ");
                        stringB.Append(normals[i].x.ToString()).Append(" ").Append(normals[i].y.ToString()).Append(" ")
                            .Append(normals[i].z.ToString());
                        streamWriter.WriteLine(stringB);
                    }
                }
                
                //writeUVs
                for (var i = 0; i < uVs.Count; i++)
                {
                    ProgressBar(MakingUVs);
                    var stringB = new StringBuilder("vt ", UVCap);
                    stringB.Append(uVs[i].x.ToString()).Append(" ").Append(uVs[i].y.ToString()).Append(" ");
                    streamWriter.WriteLine(stringB);
                }
                
                //FillPolys
                if (topoMode == TopologyMode.Triangles)
                {
                    //Write Tris
                    for (var i = 0; i < polys.Length; i += 3)
                    {
                        ProgressBar(MakingTris);
                        var stringB = new StringBuilder("f ", TrisCap);
                        stringB.Append(polys[i] + 1).Append("/").Append(polys[i] + 1).Append(" ").
                            Append(polys[i + 1] + 1).Append("/").Append(polys[i + 1] + 1).Append(" ").
                            Append(polys[i + 2] + 1).Append("/").Append(polys[i + Two] + 1);
                        streamWriter.WriteLine(stringB);
                    }
                }
                else
                {
                    //Write Quads
                    for (var i = 0; i < polys.Length; i += 4)
                    {
                        ProgressBar(MakingQuads);
                        var stringB = new StringBuilder("f ", 57);
                        stringB.Append(polys[i] + 1).Append("/").Append(polys[i] + 1).Append(" ").
                            Append(polys[i + 1] + 1).Append("/").Append(polys[i + 1] + 1).Append(" ").
                            Append(polys[i + 2] + 1).Append("/").Append(polys[i + 2] + 1).Append(" ").
                            Append(polys[i + 3] + 1).Append("/").Append(polys[i + 3] + 1);
                        streamWriter.WriteLine(stringB);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("Encountered error converting Terrain: " + e);
            }
            streamWriter.Close();
            
            EditorUtility.DisplayProgressBar("Saving model",
                "Depending on your settings, this may take some time.", 1f);
            //AssetDatabase.ImportAsset(validName);
            EditorWindow.GetWindow<T>();
            EditorUtility.ClearProgressBar();
        }
        
        
        private static void ProgressBar(string currentStep)
        {
            if (_counter++ == UpdateSpeed)
            {
                _counter = 0;
                EditorUtility.DisplayProgressBar("Saving New Model.", currentStep,
                    Mathf.InverseLerp(0, _maxCount, ++_currentCount));
            }
        }
    }

    #endregion

    
}