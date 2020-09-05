using UnityEngine;
using System.Collections;
using UnityEditor;


public class QuickSetMenuItem 
{
	[MenuItem("Window/QuickSet/Add QuickSetObjectPlacer")]
	public static void AddObjectPlacer()
	{
		GameObject go = GameObject.Instantiate( AssetDatabase.LoadAssetAtPath<GameObject>("Assets/QuickSetObjectPlacer/_QuickSet.prefab"), Vector3.zero, Quaternion.identity) as GameObject;
		go.name = QuickSetObjectPlacer.QSName;
		
		QuickSetObjectPlacer objPlacer = go.GetComponent<QuickSetObjectPlacer>();
		if(objPlacer == null)
			go.AddComponent<QuickSetObjectPlacer>();
		
		Selection.activeGameObject = go;
	}
	
	[MenuItem("Window/QuickSet/Add QuickSetObjectPlacer", true)]
	public static bool ValidateAddObjectPlacer()
	{
		GameObject objPlacer = GameObject.Find(QuickSetObjectPlacer.QSName);
		if(objPlacer)
		{
			return false;
		}
		
		return true;
	}
}







