using UnityEngine;
using System.Collections;
using System;
using UnityEditor;
using Object = UnityEngine.Object;


[CustomEditor(typeof(QuickSetObjectPlacer))]
public class QuickObjectEditor : Editor
{
	int quickSetHash = "quickSet".GetHashCode();
	Vector3 targetPoint;
	Vector3 pointNormal;
	int controlID;

	Tool lastTool = Tool.None;

	bool paintingEnabled = false;
	Vector2 disabledId = new Vector2(-1, -1);
	
	string PressToStart = "START OBJECT PAINTING";
	string PressToStop = "STOP PAINTING";
	string currentButtonString;
	bool toggleEditing = false;

	// vars used to dislay the pos scale and rot of the last placed game object
	Vector3 objPos;
	Vector3 objRot;
	Vector3 objScale;

	
	// serialized props
	SerializedProperty hasBeenInitialized;
	SerializedProperty rayMask;
	SerializedProperty offset;
	SerializedProperty rotation;
	SerializedProperty scale;
	SerializedProperty objectGroups;
	SerializedProperty selectedObjectId;
	SerializedProperty snapX;
	SerializedProperty snapY;
	SerializedProperty snapZ;
	SerializedProperty autoSetX;
	SerializedProperty autoSetY;
	SerializedProperty autoSetZ;
	SerializedProperty autoXPos;
	SerializedProperty autoYPos;
	SerializedProperty autoZPos;
	SerializedProperty gridSize;
	SerializedProperty drawXY;
	SerializedProperty drawXZ;
	SerializedProperty drawYZ;
	
	bool showOffsetSettings
	{
		get{return EditorPrefs.GetBool("QOP_showOffsetSettings", false );}
		set{EditorPrefs.SetBool("QOP_showOffsetSettings", value);}
	}
	bool showSnappingSettings
	{
		get{return EditorPrefs.GetBool("QOP_showSnappingSettings", false );}
		set{EditorPrefs.SetBool("QOP_showSnappingSettings", value);}
	}
	bool showGridSettings
	{
		get{return EditorPrefs.GetBool("QOP_showGridSettings", false );}
		set{EditorPrefs.SetBool("QOP_showGridSettings", value);}
	}


	void OnEnable()
	{
		// fetch the serialized props
		hasBeenInitialized = serializedObject.FindProperty("HasBeenInitialized");
		rayMask = serializedObject.FindProperty("RayMask");
		objectGroups = serializedObject.FindProperty("ObjectGroups");
		selectedObjectId = serializedObject.FindProperty("SelectedObjectId");
		offset = serializedObject.FindProperty("Offset");
		scale = serializedObject.FindProperty("Scale");
		rotation = serializedObject.FindProperty("Rotation");
		snapX = serializedObject.FindProperty("SnapX");
		snapY = serializedObject.FindProperty("SnapY");
		snapZ = serializedObject.FindProperty("SnapZ");
		autoSetX = serializedObject.FindProperty("AutoSetX");
		autoSetY = serializedObject.FindProperty("AutoSetY");
		autoSetZ = serializedObject.FindProperty("AutoSetZ");
		autoXPos = serializedObject.FindProperty("AutoXPos");
		autoYPos = serializedObject.FindProperty("AutoYPos");
		autoZPos = serializedObject.FindProperty("AutoZPos");
		gridSize = serializedObject.FindProperty("GridSize");
		drawXY = serializedObject.FindProperty("DrawXY");
		drawXZ = serializedObject.FindProperty("DrawXZ");
		drawYZ = serializedObject.FindProperty("DrawYZ");

	
		currentButtonString = PressToStart;

		serializedObject.Update();

		QuickSetObjectPlacer qop = target as QuickSetObjectPlacer;

		if(hasBeenInitialized.boolValue == false)
		{
			hasBeenInitialized.boolValue = true;
			rayMask.intValue = 0;
			rayMask.intValue = ~rayMask.intValue;
		}


		serializedObject.ApplyModifiedProperties();


		lastTool = Tools.current;
		Tools.current = Tool.None;

		qop.transform.position = new Vector3(0,0,0);
		qop.transform.rotation = Quaternion.Euler(Vector3.zero);
		qop.transform.hideFlags = HideFlags.NotEditable;
	}

	void OnDisable()
	{
		Tools.current = lastTool;
	}


	public override void OnInspectorGUI()
	{
		serializedObject.Update ();

		EditorGUILayout.Space();

		// Activate and deactivate object placement
		bool oldEnabled = GUI.enabled;
		GUI.enabled = selectedObjectId.vector2Value != disabledId;
		if(!GUI.enabled)
		{
			currentButtonString = PressToStart;
		}
		// the user hit tab to switch into edit mode
		if(toggleEditing)
		{
			if(GUI.enabled)
			{
				currentButtonString = currentButtonString == PressToStart ? PressToStop : PressToStart;
				paintingEnabled = !paintingEnabled;
			}
			toggleEditing = false;
		}
		if(GUILayout.Button(currentButtonString, GUILayout.Height(40)))
		{
			currentButtonString = currentButtonString == PressToStart ? PressToStop : PressToStart;
			paintingEnabled = !paintingEnabled;

		}

		GUI.enabled = oldEnabled;

		EditorGUILayout.Space();

		// add object group
		if(GUILayout.Button("Add an object group"))
		{
			objectGroups.InsertArrayElementAtIndex(objectGroups.arraySize);	
			SetSelectedGameObjectAfterAddingNewGroup();
		}

		EditorGUILayout.Space();

		GameObjectGroupsGUI ();

		EditorGUILayout.Space();
		EditorGUILayout.Space();

		EditorGUILayout.PropertyField(rayMask);

		EditorGUILayout.Space();
		
		OffsetRoationAndScaleGUI ();

		EditorGUILayout.Space();

		SnappingGUI();

		EditorGUILayout.Space();

		GridGUI();

		serializedObject.ApplyModifiedProperties ();
	}
	
	
	void OnSceneGUI()
	{
		Event evt = Event.current;
		controlID = GUIUtility.GetControlID(quickSetHash, FocusType.Passive);
		//Draw3DGrid(gridsize.intvalue,unitValue.floatValue);

		if(evt.Equals(Event.KeyboardEvent("Tab")))
		{
			toggleEditing = true;
			Repaint();
			evt.Use();
		}

		if(selectedObjectId.vector2Value == disabledId)
			return;

		if(!paintingEnabled)
			return;

		if(evt.alt)
			return;

		pointNormal = SetTargetPoint(evt);
		SetTargetVisualization(targetPoint, pointNormal);
		
		if (evt.type == EventType.Layout)
		{
			HandleUtility.AddDefaultControl(controlID);
			return;
		}
		
		if(evt.type == EventType.MouseDown && evt.button == 0)
		{
			PlaceGameObject ();
			evt.Use();
		}

		HandleUtility.Repaint();
	}

	#region gui methods

	void OffsetRoationAndScaleGUI ()
	{
		if (showOffsetSettings = EditorGUILayout.Foldout (showOffsetSettings, "Offset Rotation and Scale")) {
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField (offset);
			EditorGUILayout.PropertyField (rotation);
			EditorGUILayout.PropertyField(scale);
			EditorGUI.indentLevel--;
		}
	}

	void SnappingGUI()
	{
		if (showSnappingSettings = EditorGUILayout.Foldout (showSnappingSettings, "Snap placed objects")) 
		{
			bool oldEnabled = GUI.enabled;

			EditorGUI.indentLevel++;

			GUILayout.BeginHorizontal();

			GUILayout.Label("SnapX", GUILayout.Width(40));
			snapX.boolValue = GUILayout.Toggle(snapX.boolValue, "", GUILayout.Width(20));
			if(snapX.boolValue) autoSetX.boolValue = false;
			GUILayout.Space(40);
			GUILayout.Label("SnapY", GUILayout.Width(40));
			snapY.boolValue = GUILayout.Toggle(snapY.boolValue, "", GUILayout.Width(20));
			if(snapY.boolValue) autoSetY.boolValue = false;
			GUILayout.Space(40);
			GUILayout.Label("SnapZ", GUILayout.Width(40));
			snapZ.boolValue = GUILayout.Toggle(snapZ.boolValue, "", GUILayout.Width(20));
			if(snapZ.boolValue) autoSetZ.boolValue = false;

			GUILayout.EndHorizontal();
			EditorGUILayout.Space();


			// auto settings X
			GUILayout.BeginHorizontal();
			GUILayout.Label("Auto set X", GUILayout.MaxWidth(60));
			GUI.enabled = (!snapX.boolValue);
			autoSetX.boolValue = GUILayout.Toggle(autoSetX.boolValue, "", GUILayout.Width(20));
			GUI.enabled = autoSetX.boolValue;
			autoXPos.floatValue = EditorGUILayout.FloatField(autoXPos.floatValue);
			GUILayout.EndHorizontal();

			GUI.enabled = oldEnabled;

			// auto settings Y
			GUILayout.BeginHorizontal();
			GUILayout.Label("Auto set Y", GUILayout.MaxWidth(60));
			GUI.enabled = (!snapY.boolValue);
			autoSetY.boolValue = GUILayout.Toggle(autoSetY.boolValue, "", GUILayout.Width(20));
			GUI.enabled = autoSetY.boolValue;
			autoYPos.floatValue = EditorGUILayout.FloatField(autoYPos.floatValue);
			GUILayout.EndHorizontal();
			
			GUI.enabled = oldEnabled;

			// auto settings Z
			GUILayout.BeginHorizontal();
			GUILayout.Label("Auto set Z", GUILayout.MaxWidth(60));
			GUI.enabled = (!snapZ.boolValue);
			autoSetZ.boolValue = GUILayout.Toggle(autoSetZ.boolValue, "", GUILayout.Width(20));
			GUI.enabled = autoSetZ.boolValue;
			autoZPos.floatValue = EditorGUILayout.FloatField(autoZPos.floatValue);
			GUILayout.EndHorizontal();
			
			GUI.enabled = oldEnabled;

			EditorGUI.indentLevel--;

		}
	}

	void GridGUI()
	{

		if (showGridSettings = EditorGUILayout.Foldout(showGridSettings, "Grid"))
		{
			GUILayout.BeginHorizontal();
			
			GUILayout.Label("DrawXY", GUILayout.Width(50));
			drawXY.boolValue = GUILayout.Toggle(drawXY.boolValue, "", GUILayout.Width(20));
			GUILayout.Space(40);
			GUILayout.Label("DrawXZ", GUILayout.Width(50));
			drawXZ.boolValue = GUILayout.Toggle(drawXZ.boolValue, "", GUILayout.Width(20));
			GUILayout.Space(40);
			GUILayout.Label("DrawYZ", GUILayout.Width(50));
			drawYZ.boolValue = GUILayout.Toggle(drawYZ.boolValue, "", GUILayout.Width(20));
			
			
			GUILayout.EndHorizontal();


			EditorGUILayout.PropertyField(gridSize);
			if(gridSize.intValue < 0) gridSize.intValue = 0;

		}

	}

	void GameObjectGroupsGUI ()
	{
		for (int i = 0; i < objectGroups.arraySize; i++) 
		{
			SerializedProperty gameobjects = objectGroups.GetArrayElementAtIndex (i).FindPropertyRelative ("GOs");
			SerializedProperty groupParent = objectGroups.GetArrayElementAtIndex (i).FindPropertyRelative ("GroupParent");
			SerializedProperty foldoutBool = objectGroups.GetArrayElementAtIndex (i).FindPropertyRelative ("IsOpen");
			SerializedProperty groupName = objectGroups.GetArrayElementAtIndex (i).FindPropertyRelative ("GroupName");

			GUILayout.Box(GUIContent.none, GUILayout.Height(2), GUILayout.ExpandWidth(true));

			if (groupName.stringValue == "")
				groupName.stringValue = "ObjectGroup" + i;
			// start foldout for object group
			if (foldoutBool.boolValue = EditorGUILayout.Foldout (foldoutBool.boolValue, groupName.stringValue, EditorStyles.foldout)) 
			{
				EditorGUI.indentLevel++;
				GUILayout.BeginVertical ();

				// group name, remove group button
				GUILayout.BeginHorizontal ();

				groupName.stringValue = EditorGUILayout.TextField ("Group Name", groupName.stringValue);
				if (groupName.stringValue == "")
					groupName.stringValue = " ";
				if (GUILayout.Button ("Remove Object Group"))
				{
					objectGroups.DeleteArrayElementAtIndex (i);
					if(i == selectedObjectId.vector2Value.x)
					{
						selectedObjectId.vector2Value = disabledId;
					}

					break;
				}
				GUILayout.EndHorizontal ();

				// parent transform
				EditorGUILayout.PropertyField (groupParent);
//				if(groupParent.objectReferenceValue != null)
//				{
//					Transform parentTrans = groupParent.objectReferenceValue as Transform;
//				}

				AddObjectDropZone (gameobjects);

				// draw the game objects
				GameObjectListGUI (i, gameobjects);


				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Add a game object")) {
					gameobjects.InsertArrayElementAtIndex (gameobjects.arraySize);
				}
				if (GUILayout.Button ("Remove all objects")) {
					for (int k = gameobjects.arraySize - 1; k >= 0 ; k--) 
					{
						RemoveGameObjectFromList(gameobjects, i, k);
						groupParent.objectReferenceValue = null;
					}
				}
				GUILayout.EndHorizontal ();

				GUILayout.EndVertical ();
				EditorGUI.indentLevel--;
			}
			GUILayout.Box(GUIContent.none, GUILayout.Height(2), GUILayout.ExpandWidth(true));
			//EditorGUILayout.Space ();
		}
	}

	void GameObjectListGUI (int i, SerializedProperty gameobjects)
	{
		// draw the game objects 
		for (int j = 0; j < gameobjects.arraySize; j++) 
		{
			SerializedProperty go = gameobjects.GetArrayElementAtIndex (j).FindPropertyRelative ("GO");
			SerializedProperty selected = gameobjects.GetArrayElementAtIndex (j).FindPropertyRelative ("IsSelected");
			bool isGOfieldEmpty = go.objectReferenceValue == null;

			// if the game object field is empty, and the selectedobjectid matches our current array position
			// the selected game object no longer exists
			if (selectedObjectId.vector2Value == new Vector2 (i, j)) {
				if(isGOfieldEmpty)
				{
					selectedObjectId.vector2Value = disabledId;
					paintingEnabled = false;
					selected.boolValue = false;
				}
			}
			EditorGUILayout.BeginHorizontal ();
			// turn off the toggle if there isn't an object in the object field
			bool oldEnabled = GUI.enabled;
			GUI.enabled = isGOfieldEmpty ? false : true;
			selected.boolValue = GUILayout.Toggle (selected.boolValue, "", GUILayout.Width (20));
			GUI.enabled = oldEnabled;

			if (selected.boolValue)
			{
				selectedObjectId.vector2Value = new Vector2 (i, j);
				DeselectOtherGameObjects (i, j);
			}
			// the toggle has been unselected
			else if(selectedObjectId.vector2Value == new Vector2 (i, j))
			{
				selectedObjectId.vector2Value = disabledId;
				paintingEnabled = false;
			}

			EditorGUILayout.PropertyField (go, GUIContent.none);
			// delete game object from list
			if (GUILayout.Button ("-", GUILayout.Width (20)))
			{
				RemoveGameObjectFromList (gameobjects, i, j);
				break;
			}
			EditorGUILayout.EndHorizontal ();
		}
	}

	void AddObjectDropZone(SerializedProperty gos)
	{
		Event e = Event.current;

		Rect dropArea = GUILayoutUtility.GetRect(0, 25, GUILayout.ExpandWidth(true));
		GUI.Box(dropArea, "Drag game objects here");

		switch (e.type) 
		{
		case EventType.DragUpdated:
		case EventType.DragPerform:
			if(!dropArea.Contains(e.mousePosition))
				break;

			DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

			if(e.type == EventType.DragPerform)
			{
				foreach (var item in DragAndDrop.objectReferences) 
				{
					if(item is GameObject)
					{
						var newgo = item as GameObject;
						int index = gos.arraySize;
						gos.InsertArrayElementAtIndex(index);
						gos.GetArrayElementAtIndex(index).FindPropertyRelative("GO").objectReferenceValue = newgo;
					}
				}
			}

			e.Use();
			break;
		}

	}



	#endregion

	void PlaceGameObject ()
	{
		int i = (int)selectedObjectId.vector2Value.x;
		int j = (int)selectedObjectId.vector2Value.y;
		SerializedProperty parent = objectGroups.GetArrayElementAtIndex (i).FindPropertyRelative ("GroupParent");

		Object newObj = FindObjectAtCoordinates (i, j);

		if(newObj == null)
		{
			Debug.Log("You can't instantialte a null object");
			return;
		}

		if(PrefabUtility.GetPrefabType(newObj) == PrefabType.Prefab ||
		   PrefabUtility.GetPrefabType(newObj) == PrefabType.ModelPrefab)
		{
			GameObject go = PrefabUtility.InstantiatePrefab (newObj) as GameObject;
			Undo.RegisterCreatedObjectUndo (go, "Instantiated object");
			go.transform.position = targetPoint + offset.vector3Value;
			go.transform.rotation = Quaternion.Euler(rotation.vector3Value);
			go.transform.localScale = scale.vector3Value;
			if(parent.objectReferenceValue)
			{
				Transform par = parent.objectReferenceValue as Transform;
				if(par.gameObject.activeInHierarchy)
				{
					go.transform.parent = par;
				}
				else
					Debug.Log("An object group's parent should be an active game object in the heirarchy");
			}

		}
		else if(PrefabUtility.GetPrefabType(newObj) == PrefabType.PrefabInstance ||
		        PrefabUtility.GetPrefabType(newObj) == PrefabType.ModelPrefabInstance)
		{
			Object objRoot = PrefabUtility.GetPrefabParent(newObj);
			GameObject go = PrefabUtility.InstantiatePrefab (objRoot) as GameObject;
			Undo.RegisterCreatedObjectUndo (go, "Instantiated object");
			go.transform.position = targetPoint + offset.vector3Value;
			go.transform.rotation = Quaternion.Euler(rotation.vector3Value);
			go.transform.localScale = scale.vector3Value;
			if(parent.objectReferenceValue)
				go.transform.parent = parent.objectReferenceValue as Transform;
		}
		else
		{
			//Debug.Log("the object is a go");
			GameObject go = GameObject.Instantiate(newObj, targetPoint , Quaternion.identity) as GameObject;
			Undo.RegisterCreatedObjectUndo (go, "Instantiated object");
			go.transform.position = targetPoint + offset.vector3Value;
			go.transform.rotation = Quaternion.Euler(rotation.vector3Value);
			go.transform.localScale = scale.vector3Value;
			if(parent.objectReferenceValue)
				go.transform.parent = parent.objectReferenceValue as Transform;
		}
	}


	Object FindObjectAtCoordinates(int groupIndex, int objectIndex)
	{
		SerializedProperty gameobjects = objectGroups.GetArrayElementAtIndex (groupIndex).FindPropertyRelative ("GOs");
		SerializedProperty go = gameobjects.GetArrayElementAtIndex (objectIndex).FindPropertyRelative ("GO");
		return go.objectReferenceValue;
	}

	void DeselectOtherGameObjects(int groupIndex, int objectIndex)
	{
		for (int i = 0; i < objectGroups.arraySize; i++)
		{
			SerializedProperty gameobjects = objectGroups.GetArrayElementAtIndex (i).FindPropertyRelative ("GOs");
			// draw the game objects 
			for (int j = 0; j < gameobjects.arraySize; j++) 
			{
				//SerializedProperty go = gameobjects.GetArrayElementAtIndex (j).FindPropertyRelative ("GO");
				SerializedProperty selected = gameobjects.GetArrayElementAtIndex (j).FindPropertyRelative ("IsSelected");
				if(selected.boolValue && (groupIndex != i || objectIndex != j))
				{
					selected.boolValue = false;
				}
			}	
		}
	}

	void SetSelectedGameObjectAfterAddingNewGroup()
	{
		Vector2 vec = selectedObjectId.vector2Value;

		if(vec == disabledId)
			return;

		int x = (int)vec.x;
		int y = (int)vec.y;
		
		SerializedProperty gameobjects = objectGroups.GetArrayElementAtIndex (x).FindPropertyRelative ("GOs");
		SerializedProperty selected = gameobjects.GetArrayElementAtIndex (y).FindPropertyRelative ("IsSelected");
		selected.boolValue = true;
		DeselectOtherGameObjects(x, y);
	}



	void RemoveGameObjectFromList(SerializedProperty gos, int groupIndex, int objectIndex)
	{
		Vector2 vec = new Vector2(groupIndex, objectIndex);
		if(selectedObjectId.vector2Value == vec)
		{
			selectedObjectId.vector2Value = disabledId;
			paintingEnabled = false;
		}
		gos.DeleteArrayElementAtIndex(objectIndex); 
	}
	

	Vector3 SetTargetPoint(Event e)
	{
		Camera cam = Camera.current;
		if(cam != null)
		{
			RaycastHit hit;
			Ray ray = HandleUtility.GUIPointToWorldRay(new Vector2(e.mousePosition.x, e.mousePosition.y));
			

			if(Physics.Raycast(ray, out hit, Mathf.Infinity, rayMask.intValue ))
			{
				targetPoint = hit.point;
				targetPoint = SnapToCustomGrid(targetPoint);
				return hit.normal;
			}
			else
			{
				targetPoint = cam.ScreenToWorldPoint(new Vector3(e.mousePosition.x, cam.pixelHeight - e.mousePosition.y, 20));
				targetPoint = SnapToCustomGrid(targetPoint);
				return Vector3.up;
			}
		}
		return Vector3.up;
	}
	void SetTargetVisualization(Vector3 pos, Vector3 normal)
	{
		Color oldColor = Handles.color;
		Handles.color = new Color(1,0,0,.5f);
		//Handles.DrawSolidDisc(targetPoint, normal, .25f);
		Handles.SphereCap(0, pos, Quaternion.identity, .2f);
		Handles.color = oldColor;
	}

	#region snapping


	float SnapNumber(float unitSize, float numToSnap)
	{
		return Mathf.Round(numToSnap / unitSize) * unitSize;
	}
	float SnapToHalfUnit(float unitSize, float numToSnap)
	{
		return Mathf.Floor(numToSnap / unitSize) + .5f * unitSize;
	}
	
	
	Vector3 SnapToCustomGrid(Vector3 positionToSnap)
	{
//		if(gridTransform.objectReferenceValue == null)
//			return Vector3.zero;

		float x = positionToSnap.x;
		float y = positionToSnap.y;
		float z = positionToSnap.z;

		bool resetXinWorldSpace = false;
		bool resetYinWorldSpace = false;
		bool resetZinWorldSpace = false;

		if(snapX.boolValue)
			x = SnapNumber(1, x);
		if(snapY.boolValue)
			y = SnapNumber(1, y);
		if(snapZ.boolValue)
			z = SnapNumber(1, z);

		if(autoSetX.boolValue)
		{
			resetXinWorldSpace = true;

			x = autoXPos.floatValue * 1;
		}
		if(autoSetY.boolValue)
		{
				resetYinWorldSpace = true;

			y = autoYPos.floatValue * 1;
		}
		if(autoSetZ.boolValue)
		{
				resetZinWorldSpace = true;

			z = autoZPos.floatValue * 1;
		}

		Vector3 snappedPoint = new Vector3(x, y, z);

		if(!(resetXinWorldSpace || resetYinWorldSpace || resetZinWorldSpace))
			return snappedPoint;

		if(autoSetX.boolValue)
		{
				snappedPoint = new Vector3(autoXPos.floatValue, snappedPoint.y, snappedPoint.z);
		}
		if(autoSetY.boolValue)
		{
				snappedPoint = new Vector3(snappedPoint.x, autoYPos.floatValue, snappedPoint.z);
		}
		if(autoSetZ.boolValue)
		{
				snappedPoint = new Vector3(snappedPoint.x, snappedPoint.y, autoZPos.floatValue);
		}

		return snappedPoint;
	}

	#endregion



}

















