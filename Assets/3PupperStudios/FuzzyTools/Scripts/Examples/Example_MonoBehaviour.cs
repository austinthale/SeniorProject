using UnityEngine;
using FuzzyTools;

namespace FuzzyTools
{
	public class Example_MonoBehaviour : MonoBehaviour
	{

		/// <summary>
		/// This demonstrates the custom attributes:
		/// ReadOnly, - Works with every serialized variable
		/// GetComponentAwake, - Relies on a monoBehaviour. In this script, a second script is required for it to do anything.
		/// GetComponentStart, - Relies on a monoBehaviour. In this script, a second script is required for it to do anything.
		/// GetComponentsAwake, - Relies on a monoBehaviour. In this script, a second script is required for it to do anything.
		/// GetComponentsStart - Relies on a monoBehaviour. In this script, a second script is required for it to do anything.
		/// </summary>
		
		[ReadOnly] [GetComponentAwake] public Camera cam;
		[ReadOnly] [GetComponentStart] public AudioListener listener;
		[ReadOnly] [GetComponentsAwake] public SphereCollider[] spheres;
		[ReadOnly] [GetComponentsStart] public Collider[] colliders;
	}
}