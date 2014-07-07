using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class POI : MonoBehaviour 
{
	public bool removeOnDiscovery = false;
	public string iconKey = "";

	public HUDPOIIndicator indicator = null;

	public void SetupLocal()
	{
	}
	
	public void SetupGlobal()
	{
		if( indicator == null )
		{
			GameObject indicatorGO = new GameObject("POIIndicator_" + this.name);
			indicator = indicatorGO.AddComponent<HUDPOIIndicator>();
		}
		
		indicator.LoadIcon( iconKey );
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
		bool onScreen = indicator.UpdateScreenPosition( this.transform.position );

		if( removeOnDiscovery && onScreen )
		{
			Destroy( indicator.gameObject );
			Destroy ( this.gameObject ); 
		}

		if( indicator.HasInteraction() )
		{
			CameraMover.use.MoveTo( this.transform.position );
		}
	}



}
