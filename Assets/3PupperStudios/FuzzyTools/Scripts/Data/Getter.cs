using UnityEngine;
using System.Linq;
using System.Reflection;
using System;

namespace FuzzyTools
{
	public class Getter : MonoBehaviour
	{

		public static void GetThatComponent<T>(MonoBehaviour[] behaviours, string methodName) where T : Attribute
		{
			foreach (var mono in behaviours)
			{
				var objFields = mono.GetType()
					.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
					.Where(objField => objField.GetCustomAttributes(typeof(T), true).FirstOrDefault() != null);
                
				foreach (var field in objFields)
				{
					var attribute =
						Attribute.GetCustomAttribute(field, typeof(T)) as T;
					if (attribute == null) continue;
					var type = field.FieldType;
					var tempType = type;
					if (type.IsArray)
					{
						tempType = type.GetElementType();
					}
					var method = typeof(GameObject).GetMethod(methodName,
						BindingFlags.Instance | BindingFlags.Public,
						null, Type.EmptyTypes, null);
					if (method == null) continue;
					var generic = method.MakeGenericMethod(tempType);
					var obj = generic.Invoke(mono.gameObject, null);
					var newObj = Convert.ChangeType(obj, type);
					field.SetValue(mono, newObj);
				}
			}
		}
	}
}