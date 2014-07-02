#define DEBUG_CameraMover
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraMover : MonoBehaviour 
{
	public Transform CameraTarget = null;
	protected Vector3 targetOffset = Vector3.zero;

	public void SetupLocal()
	{
		// assign variables that have to do with this class only
		if( CameraTarget == null )
		{
			CameraTarget = transform.FindChild("CameraTarget");
		}

		if( CameraTarget == null )
		{
			Debug.LogError("CameraMover:SetupLocal : CameraTarget not found!");
			this.enabled = false;
		}

		targetOffset = this.transform.position - CameraTarget.position;
		targetOffset = targetOffset.y( 0 ); // ignore height offset
	}

	public void MoveTo(Vector3 groundPosition)
	{
		// keep our current height
		groundPosition = groundPosition.y( this.transform.position.y ) + targetOffset;
		
		this.gameObject.StopTweens();
		this.gameObject.MoveTo( groundPosition ).Time( 1.0f ).Execute();
	}
	
	public void SetupGlobal()
	{
		// lookup references to objects / scripts outside of this script
	}
	
	protected void Awake()
	{
		SetupLocal();
	}

	protected void Start () 
	{
		SetupGlobal();
	}
	
	protected void Update () 
	{
	
	}

#if DEBUG_CameraMover
	protected void OnGUI()
	{
		if( !LugusDebug.debug )
			return;

		GUILayout.BeginArea(new Rect(0, 0, 200, 200), GUI.skin.box);

		for( int i = 0; i < 4; ++i )
		{
			if( GUILayout.Button("Move to " + i) )
			{
				GameObject target = GameObject.Find("TestTarget" + i);
				MoveTo( target.transform.position );
			}
		}

		GUILayout.EndArea();
	}
#endif

}
