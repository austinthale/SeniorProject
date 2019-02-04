using UnityEngine;

namespace FuzzyTools
{
    public class MonoBehaviourGetAllComponents : MonoBehaviour
    {
        private MonoBehaviour[] _allMonoBehavioursInTheSceneStoredAsArray;// Absurdly long name to try to prevent user from ever needing this variable name
        
        protected virtual void Awake()
        {
            _allMonoBehavioursInTheSceneStoredAsArray = FindObjectsOfType<MonoBehaviour>();
            Getter.GetThatComponent<GetComponentAwakeAttribute>(_allMonoBehavioursInTheSceneStoredAsArray, "GetComponent");
            Getter.GetThatComponent<GetComponentsAwakeAttribute>(_allMonoBehavioursInTheSceneStoredAsArray, "GetComponents");
        }
        protected virtual void Start()
        {
            Getter.GetThatComponent<GetComponentStartAttribute>(_allMonoBehavioursInTheSceneStoredAsArray, "GetComponent");
            Getter.GetThatComponent<GetComponentsStartAttribute>(_allMonoBehavioursInTheSceneStoredAsArray, "GetComponents");
            _allMonoBehavioursInTheSceneStoredAsArray = null;
        }

        
    }
}