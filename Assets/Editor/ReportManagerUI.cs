using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(ReportManager))]
public class ReportManagerUI : Editor {

    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ReportManager myReportManager = (ReportManager)target;
        if (GUILayout.Button ("Generate Report"))
        {
            myReportManager.SaveReport();
        }
    }




}
