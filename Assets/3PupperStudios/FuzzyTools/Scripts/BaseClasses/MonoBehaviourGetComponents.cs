using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FuzzyTools
{
	public class MonoBehaviourGetComponents : MonoBehaviour
	{
		private MonoBehaviour[] _thisScriptStoredAsArray;// Long name to try to prevent user from ever needing this variable name
		protected virtual void Awake()
		{
			_thisScriptStoredAsArray = new []{this as MonoBehaviour};
			Getter.GetThatComponent<GetComponentAwakeAttribute>(_thisScriptStoredAsArray, "GetComponent");
			Getter.GetThatComponent<GetComponentsAwakeAttribute>(_thisScriptStoredAsArray, "GetComponents");
		}

		protected virtual void Start()
		{
			Getter.GetThatComponent<GetComponentStartAttribute>(_thisScriptStoredAsArray, "GetComponent");
			Getter.GetThatComponent<GetComponentsStartAttribute>(_thisScriptStoredAsArray, "GetComponents");
		}

		protected virtual void OnEnable()
		{
			Getter.GetThatComponent<GetComponentOnEnableAttribute>(_thisScriptStoredAsArray, "GetComponent");
			Getter.GetThatComponent<GetComponentsOnEnableAttribute>(_thisScriptStoredAsArray, "GetComponents");
		}

	}
}