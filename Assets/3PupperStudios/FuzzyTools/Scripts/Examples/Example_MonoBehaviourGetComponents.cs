using FuzzyTools;
using UnityEngine;

namespace FuzzyTools
{
	public class Example_MonoBehaviourGetComponents : MonoBehaviourGetComponents
	{
		[GetComponentAwake] public Camera cam;
		[GetComponentsAwake] public Collider[] colliders;
		[GetComponentStart] public AudioListener listner;
		[GetComponentsStart] public SphereCollider[] spheres;
		[GetComponentOnEnable] public MeshFilter filter;
		[GetComponentsOnEnable] public BoxCollider[] boxes;
	}
}