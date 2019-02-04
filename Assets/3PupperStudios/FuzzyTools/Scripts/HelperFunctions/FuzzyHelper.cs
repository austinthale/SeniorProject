using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace FuzzyTools
{
    public class FuzzyHelper : MonoBehaviour
    {
        #region EditorPrefs
        #if UNITY_EDITOR
        /*******************************BOOLS**************************************/
        public static void SetEditorPrefBool(string key, bool value)
        {
            if (!EditorPrefs.HasKey(key) || value != EditorPrefs.GetBool(key))
            {
                EditorPrefs.SetBool(key, value);
            }
        }

        public static bool GetEditorPrefBool(string key)
        {
            return EditorPrefs.HasKey(key) && EditorPrefs.GetBool(key);
        }
        
        public static bool GetEditorPrefBool(string key, bool defaultValue)
        {
            return EditorPrefs.HasKey(key) ? EditorPrefs.GetBool(key) : defaultValue;
        }
        /**************************************************************************/
        
        /*******************************INTS***************************************/
        public static void SetEditorPrefInt(string key, int value)
        {
            if (!EditorPrefs.HasKey(key) || value != EditorPrefs.GetInt(key))
            {
                EditorPrefs.SetInt(key, value);
            }
        }
        
        public static int GetEditorPrefInt(string key)
        {
            return EditorPrefs.HasKey(key) ? EditorPrefs.GetInt(key) : 0;
        }
        
        public static int GetEditorPrefInt(string key, int defaultValue)
        {
            return EditorPrefs.HasKey(key) ? EditorPrefs.GetInt(key) : defaultValue;
        }
        /**************************************************************************/
        
        /*******************************FLOATS**************************************/
        public static void SetEditorPrefFloat(string key, float value)
        {
            if (!EditorPrefs.HasKey(key) || value != EditorPrefs.GetFloat(key))
            {
                EditorPrefs.SetFloat(key, value);
            }
        }

        public static float GetEditorPrefFloat(string key)
        {
            return EditorPrefs.HasKey(key) ? EditorPrefs.GetFloat(key) : 0.0f;
        }
        
        public static float GetEditorPrefFloat(string key, float defaultValue)
        {
            return EditorPrefs.HasKey(key) ? EditorPrefs.GetFloat(key) : defaultValue;
        }
        /**************************************************************************/
        
        /********************************STRINGS***********************************/
        public static void SetEditorPrefString(string key, string value)
        {
            if (!EditorPrefs.HasKey(key) || value != EditorPrefs.GetString(key))
            {
                EditorPrefs.SetString(key, value);
            }
        }

        public static string GetEditorPrefString(string key)
        {
            return EditorPrefs.HasKey(key) ? EditorPrefs.GetString(key) : "";
        }
        
        public static string GetEditorPrefString(string key, string defaultValue)
        {
            return EditorPrefs.HasKey(key) ? EditorPrefs.GetString(key) : defaultValue;
        }
        /**************************************************************************/
        
        /*******************************COLORS**************************************/
        public static void SetEditorPrefColor(string key, Color value)
        {
            if (!EditorPrefs.HasKey(key + "r") || value.r != EditorPrefs.GetFloat(key + "r"))
            {
                EditorPrefs.SetFloat(key + "r", value.r);
            }

            if (!EditorPrefs.HasKey(key + "g") || value.g != EditorPrefs.GetFloat(key + "g"))
            {
                EditorPrefs.SetFloat(key + "g", value.g);
            }

            if (!EditorPrefs.HasKey(key + "b") || value.b != EditorPrefs.GetFloat(key + "b"))
            {
                EditorPrefs.SetFloat(key + "b", value.b);
            }

            if (!EditorPrefs.HasKey(key + "a") || value.a != EditorPrefs.GetFloat(key + "a"))
            {
                EditorPrefs.SetFloat(key + "a", value.a);
            }
        }

        public static Color GetEditorPrefColor(string key)
        {
            var red = EditorPrefs.HasKey(key + "r") ? EditorPrefs.GetFloat(key + "r") : 0.0f;
            var green = EditorPrefs.HasKey(key + "g") ? EditorPrefs.GetFloat(key + "g") : 0.0f;
            var blue = EditorPrefs.HasKey(key + "b") ? EditorPrefs.GetFloat(key + "b") : 0.0f;
            var alpha = EditorPrefs.HasKey(key + "a") ? EditorPrefs.GetFloat(key + "a") : 0.0f;
            return new Color(red, green, blue, alpha);
        }
        
        public static Color GetEditorPrefColor(string key, Color defaultValue)
        {
            var red = EditorPrefs.HasKey(key + "r") ? EditorPrefs.GetFloat(key + "r") : defaultValue.r;
            var green = EditorPrefs.HasKey(key + "g") ? EditorPrefs.GetFloat(key + "g") : defaultValue.g;
            var blue = EditorPrefs.HasKey(key + "b") ? EditorPrefs.GetFloat(key + "b") : defaultValue.b;
            var alpha = EditorPrefs.HasKey(key + "a") ? EditorPrefs.GetFloat(key + "a") : defaultValue.a;
            return new Color(red, green, blue, alpha);
        }
        /**************************************************************************/

        public static void RemoveKey(string key)
        {
            if(EditorPrefs.HasKey(key)) EditorPrefs.DeleteKey(key);
        }
        #endif
        #endregion
        
    }
    
    public static class DictionaryExtensions
    {
        public static void SetKeyValue <T1, T2>(this Dictionary<T1,T2> dictionary, T1 key, T2 content)
        {
            if (dictionary.ContainsKey(key) && !EqualityComparer<T2>.Default.Equals(dictionary[key], content))
            {
                dictionary[key] = content;
            }
            else if(!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, content);
            }
        }
    }

    public static class ListExtensions
    {
        public static void AddIfDoesNotContain<T>(this List<T> myList, IEnumerable<T> input)
        {
            foreach (var obj in input)
            {
                if (myList.Contains(obj)) continue;
                myList.Add(obj);
            }
        }
        public static void AddIfDoesNotContain<T>(this List<T> myList, T input)
        {
            if (myList.Contains(input)) return;
            myList.Add(input);
        }
        
        public static void SetLength<T>(this List<T> myList, int length, T defaultNew)
        {
            if(length>myList.Count)
            {
                for(var i = myList.Count; i<length; i++)
                {
                    myList.Add(defaultNew);
                }
            }else
            {
                while (length != myList.Count)
                {
                    myList.RemoveAt(myList.Count - 1);
                }
            }
        }
        public static void SetLength<T>(this List<T> myList, int length) where T : class
        {
            if(length>myList.Count)
            {
                for(var i = myList.Count; i<length; i++)
                {
                    myList.Add(null);
                }
            }else
            {
                while (length != myList.Count)
                {
                    myList.RemoveAt(myList.Count - 1);
                }
            }
        }
    }
   
    public static class TransformExtensions
    {
        /// <summary>
        /// // Returns the center or averaged position of Transform positions received. If null, returns Vector3.zero.
        /// </summary>
        /// <param name="transforms"></param>
        /// <returns></returns>
        public static Vector3 AveragePosition(this Transform[] transforms)
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
           
            //instead loop through the array getting their positions and adding them together
            //we will skip the first transform in the array because we've already assigned its value to center.
            for (var i = 1; i < transforms.Length; i++)
            {
                center += transforms[i].position;
            }
            //Divide to sum of transforms positions by transforms length.
            center /= transforms.Length;
            //Return the averaged position.
            return center; 
        }
        
        /// <summary>
        /// // Returns the center or averaged position of Transform positions received. If null, returns Vector3.zero.
        /// </summary>
        /// <param name="transforms"></param>
        /// <returns></returns>
        public static Vector3 AveragePosition(this List<Transform> transforms)
        {
            //Create temporary vector3 for center.
            var center = Vector3.zero;
            //Check if the received array is null or if it is empty, if it is return center.
            if (transforms == null || transforms.Count == 0)
                return center;
            //set center to be object 0's position
            center = transforms[0].position;
            //If the array only has one object, return that objects position.
            if (transforms.Count == 1)
                return center;
           
            //instead loop through the array getting their positions and adding them together
            //we will skip the first transform in the array because we've already assigned its value to center.
            for (var i = 1; i < transforms.Count; i++)
            {
                center += transforms[i].position;
            }
            //Divide to sum of transforms positions by transforms length.
            center /= transforms.Count;
            //Return the averaged position.
            return center; 
        }

        public static Vector3 AverageEualarAngles(this Transform[] transforms)
        {
            var position = transforms[0].eulerAngles;
            for (var i = 1; i < transforms.Length; i++)
            {
                position += transforms[i].eulerAngles;
            }

            position /= transforms.Length;
            
            return position;
        }
        
        public static Vector3 AverageEualarAngles(this List<Transform> transforms)
        {
            var position = transforms[0].eulerAngles;
            for (var i = 1; i < transforms.Count; i++)
            {
                position += transforms[i].eulerAngles;
            }

            position /= transforms.Count;
            
            return position;
        }
    }

    public static class GameObjectExtensions
    {
        public static GameObject[] RenameObjectsIfContain(this GameObject[] objs, string oldName, string newName)
        {
            
            if (objs.Length == 1)
            {
                objs[0].name = newName;
                return objs;
            }
            
            var i = 1;
            newName += " ";
            
            foreach (var obj in objs)
            {
                if (!obj.name.Contains(oldName)) continue;
                if (i < 10)
                {
                    newName += "0";
                }
                #if UNITY_EDITOR
                Undo.RegisterCompleteObjectUndo(obj, "Rename");
                #endif
                obj.name = newName + i;

            }
            return objs;
        }

        public static void ReplaceStrings(this IEnumerable<GameObject> objs, string oldName, string newName)
        {
            foreach (var obj in objs)
            {
                if (obj == null) continue;
                if (!obj.name.Contains(oldName)) continue;
                #if UNITY_EDITOR
                Undo.RegisterCompleteObjectUndo(obj, "Rename");
                #endif
                obj.name = obj.name.Replace(oldName, newName);
            }
        }
    }

    public static class RectExtensions
    {
        public static Rect SetRect(this Rect rect, float x, float y, float width, float height)
        {
            var pos = rect.position;
            pos.x = x;
            pos.y = y;
            rect.position = pos;
            rect.width = width;
            rect.height = height;
            return rect;
        }
    }

    public static class ComponentExtensions
    {
        public static bool ContainsType(this Component[] components, Type type)
        {
            return components.Any(comp => comp.GetType() == type);
        }

        public static Component GetComponentOfType(this Component[] components, Type type)
        {
            foreach (var comp in components)
            {
                if (comp.GetType() == type) return comp;
            }

            return null;
        }
    }

    public static class Texture2DExtensions
    {
        public static Texture2D ScaleTexture(this Texture2D myTexture,int newWidth,int newHeight, TextureFormat format)
        {
            var result = new Texture2D(newWidth, newHeight, format, true)
            {
                name = myTexture.name,
            };
            Debug.Log(result.height + "   " + result.width);
            var pixels = result.GetPixels(0);
            var xIncrease=((float)1 / myTexture.width) * ((float)myTexture.width / newWidth);
            var yIncrease=((float)1 / myTexture.height) * ((float)myTexture.height / newHeight);
            for(var i=0; i < pixels.Length; i++) {
                pixels[i] = myTexture.GetPixelBilinear (xIncrease * ((float)i % newWidth),
                    yIncrease* (Mathf.Floor(i / newWidth)));
            }
            result.SetPixels(pixels,0);
            result.Apply();
            return result;
        }
    }

    public static class BoundsExtensions
    {
        public static Bounds GetBounds( this Bounds bounds, GameObject source)
        {
            bounds = bounds.GetRenderBounds(source);
            if (bounds.extents.x != 0) return bounds;
            
            bounds = new Bounds(source.transform.position,Vector3.zero);
            foreach (Transform child in source.transform) 
            {
                var childRender = child.GetComponent<Renderer>();
                bounds.Encapsulate(childRender ? childRender.bounds : bounds.GetBounds(child.gameObject));
            }
            return bounds;
        }
        
        public static Bounds GetRenderBounds(this Bounds bounds, GameObject source){
            bounds = new Bounds(Vector3.zero,Vector3.zero);
            var render = source.GetComponent<Renderer>();
            return render == null ? bounds : render.bounds;
        }
        
    }
    /*public static class MeshColliderExtensions
    {
        public static MeshCollider GetFullMeshCollider(this MeshCollider collider, GameObject obj)
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
            obj.GetComponent<MeshFilter>().mesh = new Mesh();
            obj.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
            obj.gameObject.SetActive(true);
            return new MeshCollider();
        }
    }*/
}

