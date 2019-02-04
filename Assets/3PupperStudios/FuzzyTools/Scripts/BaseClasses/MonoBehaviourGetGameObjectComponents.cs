using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuzzyTools
{
	public class MonoBehaviourGetGameObjectComponents : MonoBehaviour
	{
		private MonoBehaviour[] _allMonoBehavioursOnGameObjectStoredAsArray;// Absurdly long name to try to prevent user from ever needing this variable name
        
		protected virtual void Awake()
		{
			_allMonoBehavioursOnGameObjectStoredAsArray = GetComponents<MonoBehaviour>();
			Getter.GetThatComponent<GetComponentAwakeAttribute>(_allMonoBehavioursOnGameObjectStoredAsArray, "GetComponent");
			Getter.GetThatComponent<GetComponentsAwakeAttribute>(_allMonoBehavioursOnGameObjectStoredAsArray, "GetComponents");
		}
		protected virtual void Start()
		{
			Getter.GetThatComponent<GetComponentStartAttribute>(_allMonoBehavioursOnGameObjectStoredAsArray, "GetComponent");
			Getter.GetThatComponent<GetComponentsStartAttribute>(_allMonoBehavioursOnGameObjectStoredAsArray, "GetComponents");
			_allMonoBehavioursOnGameObjectStoredAsArray = null;
		}

        
	}
}
