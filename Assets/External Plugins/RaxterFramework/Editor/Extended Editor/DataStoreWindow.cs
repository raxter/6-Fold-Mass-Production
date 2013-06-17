// C# example:
using UnityEngine;
using UnityEditor;
public class DataStoreWindow : EditorWindow 
{
    
	// Add menu named "My Window" to the Window menu
	[MenuItem ("Window/Data Store Window")]
    static void Init () 
    {
        // Get existing open window or if none, make a new one:
       	EditorWindow.GetWindow (typeof (DataStoreWindow));
    }
	
	
	Vector3 savePosition;
	Quaternion saveRotation;
    
    void OnGUI () 
	{
		Object target = Selection.activeObject;
		
		GameObject targetObject = null;
		
		if (target is GameObject)
			targetObject = target as GameObject;
			
		if (targetObject)
		{
			
			GUILayout.Label("Saved Transform\nPosition:\t"+savePosition+"\nRotation:\t"+saveRotation);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Save Transform Data"))
			{
				savePosition = targetObject.transform.localPosition;
				saveRotation = targetObject.transform.localRotation;
			}
			if (GUILayout.Button("Write Transform Data"))
			{
				targetObject.transform.localPosition = savePosition;
				targetObject.transform.localRotation = saveRotation;
			}
			GUILayout.EndHorizontal();
			
		}
		else
		{
			GUILayout.Label("The selected object must be a GameObject component");
		}
    }
}