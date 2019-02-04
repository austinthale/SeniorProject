using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Object = UnityEngine.Object;

namespace FuzzyTools
{
	public class BetterFindReferences : EditorWindow
	{
		private const string windowTitle = "Find References";
		private const string referenceObj = "Reference Object";
		private const string findRefs = "Find Scene References";
		private const string selectAll = "Select All References";
		private const string NoSolo = "Solo not supported for non-assets";
		private static readonly string[] FunctionModes = {"Standard", "Solo"};
		private static readonly string[] OnlyStandard = {"Standard"};
		private const string _ref = "ref:";

		private static int _modeSelection = 0;
		private const int PickerID = 455454425;
		private static bool _changeSearch = false;
		
		private static Vector2 _scrollPos = Vector2.zero;
		
		private static Object _selectedObj;
		private static EditorWindow _window;
		private static List<Object> _references = new List<Object>();
		private static GameObject[] _allGameObjects;
		private static Dictionary<int, Component> _sourceComp = new Dictionary<int, Component>();

		private static readonly Color Orange = new Color(1,.7f,0,1);
		private static Texture2D _refBackground;

		private static GUIStyle _boxes = new GUIStyle();
		private static GUIStyle _warningFont = new GUIStyle();
		private static GUILayoutOption _bigButton = GUILayout.Height(35);
		
		private static void Init()
		{
			_references.Clear();
			_sourceComp.Clear();
			_boxes = EditorStyles.toolbarButton;
			_boxes.fontSize = 10;
			_warningFont.normal.textColor = Orange;
			_warningFont.normal.background = _refBackground;
			_warningFont.alignment = TextAnchor.MiddleCenter;
			_warningFont.fontStyle = FontStyle.Bold;
			_warningFont.fontSize = 15;
			_refBackground = new Texture2D(20,20);
			
			_window = GetWindow(typeof(BetterFindReferences), false, windowTitle);
		}

		private void OnGUI()
		{
			if (_window == null)
			{
				Init();
			}
			GUILayoutOption[] fullWindowLayout = {GUILayout.Width(_window.position.width)};
			var obj = _selectedObj;
			_boxes.fontStyle = FontStyle.Bold;
			_boxes.fontSize += 5;
			_selectedObj =
				EditorGUILayout.ObjectField(referenceObj, _selectedObj, typeof(Object), true, fullWindowLayout);
			if (_selectedObj != obj || _selectedObj == null)
			{
				_sourceComp.Clear();
				_references.Clear();
			}

			//if (AssetDatabase.Contains(_selectedObj))
			var asset = _selectedObj != null? AssetDatabase.Contains(_selectedObj) : false;
			if(asset)
			{
				_modeSelection =
					GUILayout.SelectionGrid(_modeSelection, FunctionModes, 1, _boxes);
			}
			else
			{
				_modeSelection =
					GUILayout.SelectionGrid(_modeSelection, OnlyStandard, 1, _boxes);
				
				GUILayout.Label(NoSolo, _warningFont, fullWindowLayout);
			}
			
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
			if (_references != null && _references.Count > 0)
			{
				//var prefType = PrefabUtility.GetPrefabType(_selectedObj);
				GUILayout.BeginVertical("box");
				GUILayout.Space(5);
				if (!AssetDatabase.Contains(_selectedObj))
				{
					/*if (_modeSelection == 1)
					{
						_changeSearch = true;
						var compName = AssetDatabase.GetAssetPath(_selectedObj);
						compName = compName.Substring(compName.LastIndexOf("/")+1);
						var sb = new StringBuilder();
						foreach (var refer in _references)
						{
							sb.Append(" ");
							sb.Append(refer.name);
						}
						var searchFor = sb.ToString();
						SetSearchFilter(searchFor, 0);
					}
					else
					{
						if(_changeSearch)
						{
							_changeSearch = false;
							SetSearchFilter("", 1);
						}
					}*/
					foreach (var refer in _references)
					{
						//GUILayout.BeginHorizontal("box");
						var comp = _sourceComp[refer.GetInstanceID()];
						var label = comp.ToString();
						ProObjectField(label, comp, typeof(Object), GUI.skin.button, true);
						GUILayout.Space(2);
					}
				}
				else
				{
					if (_modeSelection == 1)
					{
						_changeSearch = true;
						var texPath = AssetDatabase.GetAssetPath(_selectedObj);
						texPath = texPath.Substring(texPath.LastIndexOf("/")+1);
						var sb = new StringBuilder(_ref);
						var searchFor = sb.Append(texPath).ToString();
						SetSearchFilter(searchFor, 0);
					}
					else
					{
						if(_changeSearch)
						{
							_changeSearch = false;
							SetSearchFilter("", 1);
						}
					}
					foreach (var refer in _references)
					{
						var label = refer.name;
						ProObjectField(label, refer, typeof(Object), GUI.skin.button ,true);
						GUILayout.Space(2);
						//EditorGUILayout.ObjectField(name, refer, typeof(Object), true);
					}
				}
				GUILayout.Space(3);
				GUILayout.EndVertical();
				
				//EditorGUI.EndDisabledGroup();
			}
			EditorGUILayout.EndScrollView();
			
			EditorGUILayout.Space();
			GUILayout.BeginVertical("box");
			if(_references != null)
			{
				EditorGUI.BeginDisabledGroup(_references.Count == 0);
				if (GUILayout.Button(selectAll, _bigButton))
				{
					Selection.objects = _references.ToArray();
				}

				EditorGUI.EndDisabledGroup();
			}
			GUILayout.Space(5);
			
			EditorGUI.BeginDisabledGroup(_selectedObj == null);
			
			if (GUILayout.Button(findRefs, _bigButton))
			{
				FindSceneReferences();
			}
			EditorGUI.EndDisabledGroup();
			GUILayout.EndVertical();

			_boxes.fontStyle = FontStyle.Normal;
			_boxes.fontSize -= 5;
			var mini = EditorStyles.miniButton;
			mini.normal.background = Texture2D.blackTexture;
		}

		private void OnDestroy()
		{
			_references.Clear();
			_sourceComp.Clear();
		}

		[MenuItem("CONTEXT/Component/Find Scene References")]
		private static void FindComponentReferences(MenuCommand command)
		{
			var component = command.context as Component;
			if (component == null) return;
			_selectedObj = command.context;
			Init();
			FindSceneReferences();
		}
 
		[MenuItem("Assets/FuzzyTools/Find Scene References")]
		private static void FindReferencesToAsset()
		{
			_selectedObj = Selection.activeObject;
			if (_selectedObj == null) return;
			
			Init();
			FindSceneReferences();
		}

		[MenuItem("GameObject/FuzzyTools/Find Scene References")]
		private static void FindReferencesToGameObject()
		{
			_selectedObj = Selection.activeGameObject;
			if (_selectedObj == null) return;
			
			Init();
			FindSceneReferences();
		}
 
		private static void FindSceneReferences()
		{
			_references.Clear();
			if (_selectedObj == null) return;
			
			_allGameObjects = FindObjectsOfType<GameObject>();
			
/*			for (int j = 0; j < _allGameObjects.Length; j++)
			{
				var go = _allGameObjects[j];
 
				if (PrefabUtility.GetPrefabType(go) == PrefabType.PrefabInstance)
				{
					if (PrefabUtility.GetPrefabParent(go) == _selectedObj)
					{
						Debug.Log(string.Format("referenced by {0}, {1}", go.name, go.GetType()), go);
						_references.Add(go);
					}
				}
 
				var components = go.GetComponents<Component>();
				for (int i = 0; i < components.Length; i++)
				{
					var c = components[i];
					if (!c) continue;
 
					var so = new SerializedObject(c);
					var sp = so.GetIterator();
 
					while (sp.NextVisible(true))
						if (sp.propertyType == SerializedPropertyType.ObjectReference)
						{
							if (sp.objectReferenceValue == _selectedObj)
							{
								Debug.Log(string.Format("referenced by {0}, {1}", c.name, c.GetType()), c);
								_references.Add(c.gameObject);
							}
						}
				}
			}*/
			foreach (var gObj in _allGameObjects)
			{
#if UNITY_2017 || UNITY_2018_1 || UNITY_2018_2
				if (PrefabUtility.GetPrefabType(gObj) == PrefabType.PrefabInstance)
				{
					if (PrefabUtility.GetCorrespondingObjectFromSource(gObj) == _selectedObj)
					{
						_references.Add((gObj));
					}
				}

				var components = gObj.GetComponents<Component>();

				foreach (var comp in components)
				{
					if (comp == null) continue;
					var serialized = new SerializedObject(comp);
					var property = serialized.GetIterator();

					while (property.NextVisible(true))
					{
						if (property.propertyType != SerializedPropertyType.ObjectReference) continue;
						if (property.objectReferenceValue != _selectedObj) continue;
						var go = comp.gameObject;
						_references.Add(go);
						_sourceComp.Add(go.GetInstanceID(), comp);
					}
				}
#endif
			}

			if (_references.Any())
				Selection.objects = _references.ToArray();
			else
				EditorUtility.DisplayDialog("No References Found.", "No references to " + _selectedObj + " found",
					"Okay");
		}
		


		public static void SetSearchFilter(string filter, int filterMode)
		{
			EditorWindow hierarchy = null;
			var windows = (SearchableEditorWindow[])Resources.FindObjectsOfTypeAll (typeof(SearchableEditorWindow));

			foreach (var window in windows)
			{
				if (window.GetType().ToString() != "UnityEditor.SceneHierarchyWindow") continue;

				hierarchy = window;
				break;
			}

			if (hierarchy == null)
				return;

			var setSearchType =
				typeof(SearchableEditorWindow).GetMethod("SetSearchFilter",
					BindingFlags.NonPublic | BindingFlags.Instance);      
			var parameters = new object[]{filter, filterMode, false};
			if (setSearchType == null) return;
			setSearchType.Invoke(hierarchy, parameters);
		}
		
		
		public static Object ProObjectField(
			string label, 
			Object value, 
			Type objType, 
			GUIStyle style, 
			bool allowSceneObjects, 
			Texture objIcon = null,
			params GUILayoutOption[] options)
		{

			if (objIcon == null) 
			{
				objIcon = EditorGUIUtility.FindTexture("PrefabNormal Icon");
			} 
			style.imagePosition = ImagePosition.ImageLeft;
 
			
 
			var centered = EditorStyles.miniButton;
			centered.normal.background = _refBackground;
			centered.alignment = TextAnchor.MiddleCenter;
			
			//centered.fixedHeight = 10;
			GUILayoutOption[] fullWindowLayout = {GUILayout.Width(_window.position.width - 15)};
			//centered.fixedHeight = 1;
			var button = GUILayout.Button(label, centered, fullWindowLayout);
			
			if (button)
			{
				Selection.activeObject = value;
			}

			if (Event.current.commandName != "ObjectSelectorUpdated") return value;

			if (EditorGUIUtility.GetObjectPickerControlID() != PickerID) return value;
			
			value = EditorGUIUtility.GetObjectPickerObject() as Object;
			
			

			return value;
		}
	}
}