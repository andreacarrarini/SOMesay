using UnityEngine;
using System.Collections;

public class QuickSetObjectPlacer : MonoBehaviour 
{
	public static string QSName = "_QuickSet";

	[System.Serializable]
	public class QOPobjectGroup
	{
		public QOPgameObject[] GOs;
		public Transform GroupParent;
		public bool IsOpen;
		public string GroupName;
	}
	
	[System.Serializable]
	public class QOPgameObject
	{
		public bool IsSelected;
		public GameObject GO;
	}

	public bool HasBeenInitialized = false;
	
	public QOPobjectGroup[] ObjectGroups;

	public Vector2 SelectedObjectId = -1 * Vector2.one;

	public LayerMask RayMask;

	public Vector3 Offset;
	public Vector3 Rotation;
	public Vector3 Scale = Vector3.one;

	// vars snapping
	public bool SnapX = false;
	public bool SnapY = false;
	public bool SnapZ = false;
	public bool AutoSetX = false;
	public bool AutoSetY = false;
	public bool AutoSetZ = false;

	public float AutoXPos = 0;
	public float AutoYPos = 0;
	public float AutoZPos = 0;
	
	public int GridSize = 10;
	public bool DrawXY = false;
	public bool DrawXZ = false;
	public bool DrawYZ = false;
	
	void OnDrawGizmos()
	{
		DrawGrid(GridSize, 1);
	}

	void DrawGrid(int gridSize, float unitSize)
	{

		float gridByUnit = gridSize * unitSize;

		Vector3 gridPos = Vector3.zero;
		Vector3 gridUp = Vector3.up;
		Vector3 gridRight = Vector3.right;
		Vector3 gridForward = Vector3.forward;

		Color oldCol = Gizmos.color;
		Gizmos.color = new Color(oldCol.r, oldCol.g, oldCol.b, .2f);
		
		if(DrawXY)
		{
			Vector3 vertPos = gridPos + gridUp * gridByUnit;
			Vector3 negVertPos = gridPos + gridUp * - gridByUnit;
			Vector3 horzPos = gridPos + gridRight * gridByUnit;
			Vector3 negHorzPos = gridPos +  gridRight *-gridByUnit;
			// xy plane
			for (int i = 0; i <= gridSize; i++) 
			{
				if(i==0)
				{
					Gizmos.color = new Color(0, 1, 0, .3f);
					Gizmos.DrawLine(negVertPos, vertPos);
					Gizmos.color = new Color(1, 0, 0, .3f);
					Gizmos.DrawLine(negHorzPos, horzPos);
					Gizmos.color = new Color(oldCol.r, oldCol.g, oldCol.b, .2f);
				}
				else
				{
					//xy plane
					// vert lines
					Gizmos.DrawLine(negVertPos + gridRight * i * unitSize, vertPos + gridRight * i * unitSize);
					Gizmos.DrawLine(negVertPos + gridRight * -i * unitSize,  vertPos + gridRight * -i * unitSize);
					// horz lines
					Gizmos.DrawLine(negHorzPos + gridUp * i * unitSize, horzPos + gridUp * i * unitSize);
					Gizmos.DrawLine(negHorzPos + gridUp * -i * unitSize, horzPos + gridUp * -i * unitSize);
				}
			}
		}
		
		if(DrawXZ)
		{
			Vector3 forPos = gridPos + gridForward * gridByUnit;
			Vector3 negForPos = gridPos + gridForward * -gridByUnit;
			Vector3 horzPos = gridPos + gridRight * gridByUnit;
			Vector3 negHorzPos = gridPos +  gridRight *-gridByUnit;
			for (int i = 0; i <= gridSize; i++) 
			{
				if(i==0)
				{
					// xz
					Gizmos.color = new Color(0, 0, 1, .3f);
					Gizmos.DrawLine(negForPos, forPos);
					Gizmos.color = new Color(1, 0, 0, .3f);
					Gizmos.DrawLine(negHorzPos, horzPos);
					Gizmos.color = new Color(oldCol.r, oldCol.g, oldCol.b, .2f);
				}
				else
				{
					//xz plane
					Gizmos.DrawLine(negForPos + gridRight * i * unitSize, forPos + gridRight * i * unitSize);
					Gizmos.DrawLine(negForPos + gridRight * -i * unitSize, forPos + gridRight * -i * unitSize);
					
					Gizmos.DrawLine(negHorzPos + gridForward * i *unitSize, horzPos + gridForward * i *unitSize);
					Gizmos.DrawLine(negHorzPos + gridForward * -i *unitSize, horzPos + gridForward * -i *unitSize);
				}
			}
		}

		if(DrawYZ)
		{
			Vector3 forPos = gridPos + gridForward * gridByUnit;
			Vector3 negForPos = gridPos + gridForward * -gridByUnit;
			Vector3 vertPos = gridPos + gridUp * gridByUnit;
			Vector3 negVertPos = gridPos + gridUp * - gridByUnit;
			// yz plane
			for (int i = 0; i <= gridSize; i++) 
			{
				if(i==0)
				{
					// yz
					Gizmos.color = new Color(0, 0, 1, .3f);
					Gizmos.DrawLine(negForPos, forPos);
					Gizmos.color = new Color(0, 1, 0, .3f);
					Gizmos.DrawLine(negVertPos, vertPos);
					Gizmos.color = new Color(oldCol.r, oldCol.g, oldCol.b, .2f);
					
				}
				else
				{
					//xz plane
					Gizmos.DrawLine(negForPos + gridUp * i * unitSize, forPos + gridUp * i * unitSize);
					Gizmos.DrawLine(negForPos + gridUp * -i * unitSize, forPos + gridUp * -i * unitSize);
					
 					Gizmos.DrawLine(negVertPos + gridForward * i * unitSize, vertPos + gridForward * i * unitSize);
					Gizmos.DrawLine(negVertPos + gridForward * -i * unitSize, vertPos + gridForward * -i * unitSize);
				}
			}
		}
		
		
		Gizmos.color = oldCol;
	}

}
















