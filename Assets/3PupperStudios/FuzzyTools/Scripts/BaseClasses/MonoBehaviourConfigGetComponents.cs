using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace FuzzyTools
{
	public class MonoBehaviourConfigGetComponents : MonoBehaviour
	{
		public WhichMonoBehavioursShouldGetComponents autoGetComponentWhich;
		public WhichTypeOfGetComponentShouldRun autoGetComponentType;
		public WhenShouldGetComponentsRun autoGetComponentWhen;
		private MonoBehaviour[] _theMonoBehavioursToRunGetComponent;// Long name to try to prevent user from ever needing this variable name

		public enum WhichMonoBehavioursShouldGetComponents
		{
			ThisScriptOnly,
			ThisGameObject,
			AllGameObjectsInScene
		}
		
		public enum WhichTypeOfGetComponentShouldRun
		{
			Both,
			GetComponent,
			GetComponents
		}

		public enum WhenShouldGetComponentsRun
		{
			AwakeStartOnEnable,
			Awake,
			Start,
			OnEnable,
			AwakeStart,
			AwakeOnEnable,
			StartOnEnable
			
		}
		protected virtual void Awake()
		{
			switch (autoGetComponentWhich)
			{
					default:
					case WhichMonoBehavioursShouldGetComponents.ThisGameObject:
						_theMonoBehavioursToRunGetComponent = GetComponents<MonoBehaviour>();
						break;
					case WhichMonoBehavioursShouldGetComponents.ThisScriptOnly:
						_theMonoBehavioursToRunGetComponent = new []{this as MonoBehaviour};
						break;
					case WhichMonoBehavioursShouldGetComponents.AllGameObjectsInScene:
						_theMonoBehavioursToRunGetComponent = FindObjectsOfType<MonoBehaviour>();
						break;
			}
			
			switch (autoGetComponentWhen)
			{
					default:
					case WhenShouldGetComponentsRun.Awake:
					case WhenShouldGetComponentsRun.AwakeStart:
					case WhenShouldGetComponentsRun.AwakeOnEnable:
					case WhenShouldGetComponentsRun.AwakeStartOnEnable:
						switch (autoGetComponentType)
						{
							default:
							case WhichTypeOfGetComponentShouldRun.Both:
								Getter.GetThatComponent<GetComponentAwakeAttribute>(_theMonoBehavioursToRunGetComponent, "GetComponent");
								Getter.GetThatComponent<GetComponentsAwakeAttribute>(_theMonoBehavioursToRunGetComponent, "GetComponents");
								break;
							case WhichTypeOfGetComponentShouldRun.GetComponent:
								Getter.GetThatComponent<GetComponentAwakeAttribute>(_theMonoBehavioursToRunGetComponent, "GetComponent");
								break;
							case WhichTypeOfGetComponentShouldRun.GetComponents:
								Getter.GetThatComponent<GetComponentsAwakeAttribute>(_theMonoBehavioursToRunGetComponent, "GetComponents");
								break;
						}
						break;
					case WhenShouldGetComponentsRun.Start:
					case WhenShouldGetComponentsRun.OnEnable:
					case WhenShouldGetComponentsRun.StartOnEnable:
						break;
			}
			
			
		}

		protected virtual void Start()
		{
			switch (autoGetComponentWhen)
			{
				default:
				case WhenShouldGetComponentsRun.Start:
				case WhenShouldGetComponentsRun.AwakeStart:
				case WhenShouldGetComponentsRun.StartOnEnable:
				case WhenShouldGetComponentsRun.AwakeStartOnEnable:
					switch (autoGetComponentType)
					{
						default:
						case WhichTypeOfGetComponentShouldRun.Both:
							Getter.GetThatComponent<GetComponentStartAttribute>(_theMonoBehavioursToRunGetComponent, "GetComponent");
							Getter.GetThatComponent<GetComponentsStartAttribute>(_theMonoBehavioursToRunGetComponent, "GetComponents");
							break;
						case WhichTypeOfGetComponentShouldRun.GetComponent:
							Getter.GetThatComponent<GetComponentStartAttribute>(_theMonoBehavioursToRunGetComponent, "GetComponent");
							break;
						case WhichTypeOfGetComponentShouldRun.GetComponents:
							Getter.GetThatComponent<GetComponentsStartAttribute>(_theMonoBehavioursToRunGetComponent, "GetComponents");
							break;
					}
					break;
				case WhenShouldGetComponentsRun.Awake:
				case WhenShouldGetComponentsRun.AwakeOnEnable:
				case WhenShouldGetComponentsRun.OnEnable:
					break;
			}
		}

		protected virtual void OnEnable()
		{
			switch (autoGetComponentWhen)
			{
				default:
				case WhenShouldGetComponentsRun.OnEnable:
				case WhenShouldGetComponentsRun.AwakeOnEnable:
				case WhenShouldGetComponentsRun.StartOnEnable:
				case WhenShouldGetComponentsRun.AwakeStartOnEnable:
					switch (autoGetComponentType)
					{
						default:
						case WhichTypeOfGetComponentShouldRun.Both:
							Getter.GetThatComponent<GetComponentOnEnableAttribute>(_theMonoBehavioursToRunGetComponent, "GetComponent");
							Getter.GetThatComponent<GetComponentsOnEnableAttribute>(_theMonoBehavioursToRunGetComponent, "GetComponents");
							break;
						case WhichTypeOfGetComponentShouldRun.GetComponent:
							Getter.GetThatComponent<GetComponentOnEnableAttribute>(_theMonoBehavioursToRunGetComponent, "GetComponent");
							break;
						case WhichTypeOfGetComponentShouldRun.GetComponents:
							Getter.GetThatComponent<GetComponentsOnEnableAttribute>(_theMonoBehavioursToRunGetComponent, "GetComponents");
							break;
					}
					break;
				case WhenShouldGetComponentsRun.Awake:
				case WhenShouldGetComponentsRun.Start:
				case WhenShouldGetComponentsRun.AwakeStart:
					break;
			}
		}
	}
}