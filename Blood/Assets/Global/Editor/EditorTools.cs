using UnityEngine;
using UnityEditor;
using System.Collections;

public class EditorTools : MonoBehaviour 
{
    // Add context menu to Gameobject menu
    [MenuItem ("GameObject/Drop to floor #q")]
    static void DropToFloor () 
    {
        Undo.RegisterSceneUndo("Drop to floor");

        foreach(GameObject go in Selection.gameObjects)
        {
	        Vector3 floor = go.transform.position;
	        RaycastHit hit = new RaycastHit();
			
	        if (Physics.Raycast(go.transform.position, Vector3.down, out hit))
	        {
		        go.transform.position = hit.point;		
	        }
        }
    }
    // Add context menu to Gameobject menu
    [MenuItem("GameObject/Count selected objects")]
    static void CountSelectedObjecT()
    {
        Debug.Log(Selection.gameObjects.Length);
    }
	
	// Add context menu to Transform's context menu
	[MenuItem ("GameObject/Parent under empty")]
	static void ParentUnderEmpty () 
	{
		Undo.RegisterSceneUndo("Parent under empty");
	
		GameObject go = new GameObject ("Parent");
		Vector3 middle = Vector3.zero;
		
		for (int i = 0; i < Selection.gameObjects.Length; i++)
		{
			middle += Selection.gameObjects[i].transform.position;
		}
		
		middle /= Selection.gameObjects.Length;
		go.transform.position = middle;
	
		for (int j = 0; j < Selection.gameObjects.Length; j++)
		{
			Selection.gameObjects[j].transform.parent = go.transform;
		}
			
			
		Selection.objects = new Object[]{go};	
	}

    [MenuItem("GameObject/Select Parent %w")]
    static void SelectParent()
    {
        Selection.activeGameObject = Selection.activeGameObject.transform.parent.gameObject;
    }
}
