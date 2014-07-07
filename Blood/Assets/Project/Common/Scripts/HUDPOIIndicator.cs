using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class HUDPOIIndicator : MonoBehaviour 
{
	protected SpriteRenderer spriteRenderer = null;

	public void LoadIcon(string key)
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		if( spriteRenderer == null )
		{
			spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
		}

		spriteRenderer.sprite = LugusResources.use.GetSprite(key);

		BoxCollider2D coll = GetComponent<BoxCollider2D>();
		if( coll == null )
		{
			coll = gameObject.AddComponent<BoxCollider2D>();
		}
	}

	public void SetupLocal()
	{
		// assign variables that have to do with this class only
		this.gameObject.layer = LayerMask.NameToLayer("UI");
		this.transform.parent = HUDManager.use.transform;
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

	public bool HasInteraction()
	{
		Transform hit = LugusInput.use.RayCastFromMouseUp( LugusCamera.ui );
		if( hit == this.transform )
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	// returns if the POI is on screen or not
	public bool UpdateScreenPosition(Vector3 worldPosition) 
	{
		// Our goal here is to show an indicator when the POI is offscreen
		// this indicator has to "hug" the side of the screen in the direction of the POI

		// basic algorith:
		// 1. calculate viewport coordinates of the world position (so we can work in a simple 2D plane)
		// 2. "draw a line" from the center of the screen (viewport coords (0.5,0.5)) to the viewport coords of the target POI
		// 3. see where that line intersects with the viewport borders (since the viewport is basically a big rectangle)
		// 4. where the intersection occurs, we need to place the indicator

		// step 1: viewport coordinates of target position
		Vector3 viewportPosition = LugusCamera.game.WorldToViewportPoint( worldPosition );

		// ATTENTION:
		// you might think the Unity3D function WorldToViewportPoint() will always give you the correct viewport estimate
		// however, there is some unexpected behaviour when working with rotated / perspective camera's (ex. isometric camera)
		// the way the function works, is that it projects the worldpoint onto the "near clipping plane" of the camera to get the viewport coords
		// however, when the camera is rotated, this clipping plane will at some point intersect the "ground" plane and possibly end up "beneath" our POI positions
		// if the plane is beneath the position, it's as if the point is "behind" the camera, because it is behind the clipping plane. 
		// when this happens (usually if the POI is far from the camera), the Unity3D function will return a negative z viewport coordinate (indicating the point is "behind" the camera
		// the returned x and y values are "flipped" i.e. bottom right becomes top left, and similar, because that makes sense when viewing "from behind
		// this poses an important problem, since we cannot just use the returned x and y coordinates directly in all circumstances
		// also see Docs/Images/ViewPort_CameraPlane_InvertedCoordsError  for added clarity
		// also see http://forum.unity3d.com/threads/camera-worldtoscreenpoint-bug.85311/ for extra inspiration

		// first idea was to move the worldPosition in the direction of the camera position, so it would be "in front of" the clipping plane and then calculate the viewport coords
		// in theory this should work, since the direction stays the same, and perspective projections normally keep points on the same world direction in the same viewport direction as well
		// however... in practice, I still had some weird offset that I couldn't get rid of... 

		// currently implemented solution is to invert the intersection point back to what we expect (unity3d transform bottomright to topleft, we make it bottomright again)
		// however, to be able to do this, we need to know around what point the wrapping/change occurs... this is very difficult in world coordinates
		// BUT, here's the trick: if we just calculate the intersection first (even for the borked "top left" coordinates, the intersection code still works)
		// and make sure we have viewportcoordinates in/on the edge of the viewport
		// we then know that all values are in the (0, 1.0f) range and inverting them is as easy as doing 1.0f - value. 

		// So, simply:
		// - calculate intersection no matter if the coordinate is "in front" or "behind" the camera (intersection code doesn't know about this)
		// - if it was "behind", flip the intersection coordinates on the viewport edges

		bool revertIntersection = false;
		if( viewportPosition.z < 0.0f )
		{
			revertIntersection = true; 
		}

		bool offScreen = viewportPosition.x < 0.0f || viewportPosition.x > 1.0f ||
						viewportPosition.y < 0.0f || viewportPosition.y > 1.0f;

		//bool screenEdge =   viewportPosition.x < 0.25f || viewportPosition.x > 0.75f ||
		//					viewportPosition.y < 0.25f || viewportPosition.y > 0.75f;

		if( offScreen )
		{
			viewportPosition = IntersectLineWithViewport( new Vector3(0.5f, 0.5f, 0.0f), viewportPosition );
		}
		else
		{
			// viewportPosition stays the same, directly usable
		}

		/*
		if( screenEdge ) 
		{
			//renderer.enabled = true;
			spriteRenderer.color = spriteRenderer.color.a(1.0f);

		}
		else
		{
			spriteRenderer.color = spriteRenderer.color.a(0.5f);
		}
		*/

		if( revertIntersection )
		{
			viewportPosition = viewportPosition.x ( 1.0f - viewportPosition.x ).y( 1.0f - viewportPosition.y );
			spriteRenderer.color = spriteRenderer.color.g(1.0f).a(0.5f);
		}
		else
		{
			spriteRenderer.color = spriteRenderer.color.g(1.0f).a(1.0f);
		}
		
		Vector3 uiPosition = LugusCamera.ui.ViewportToWorldPoint( viewportPosition.z(5.0f) );
		this.transform.position = uiPosition;

		
		return !offScreen;
	}

	[Flags]
	public enum OutCode
	{
		Inside = 0, // 0x0000

		Left = 1, // 0x0001
		Right = 2, // 0x0010
		Bottom = 4, // 0x0100
		Top = 8, // 0x1000

		TopLeft = 9,
		BottomLeft = 5,
		TopRight = 10,
		BottomRight = 6
	}

	// start is inside viewport (x and y between 0 an 1.0f), end is outside viewport
	// viewport is (0,0) in bottom left, (1,1) in top right (Unity3D default)
	protected Vector3 IntersectLineWithViewport( Vector3 start, Vector3 end )
	{
		Vector3 intersection = Vector3.zero;


		// our approach is based upon cohen-sutherland algorithm for line clipping to a rectangle
		// http://en.wikipedia.org/wiki/Cohen%E2%80%93Sutherland_algorithm
		// (https://www.siggraph.org/education/materials/HyperGraph/scanline/clipping/clipsuco.htm)
		// this algorithm is more common-purpose and uses a repetitive approach to deal with all cases
		// we can however make some simplifications, because start is always inside the viewport here


		// Note: we could have skipped cohen sutherland and just used the typical methods of calculating line intersections
		// ex. using functions: http://www.wyrmtale.com/blog/2013/115/2d-line-intersection-in-c
		// ex. using dot/cross products: http://wiki.unity3d.com/index.php/3d_Math_functions
		// we chose cohen-sutherland here for the challenge + simplest to understand + possible performance gain + pure elegance :)


		// Step 1: determine in which quadrant the end point is situated
		OutCode outcode = OutCode.Inside;

		if( end.x < 0.0f )
			outcode |= OutCode.Left;
		else if( end.x > 1.0f )
			outcode |= OutCode.Right;

		if( end.y < 0.0f )
			outcode |= OutCode.Bottom;
		else if( end.y > 1.0f )
			outcode |= OutCode.Top;

		if( outcode == OutCode.Inside )
		{
			// outcode is inside viewport : just return end coordinate (already "clipped")
			return end;
		}

		// Step 2: optimize quadrant

		// in cohen-sutherland, if a point is "diagonal", i.e. top-left, bottom-right, etc.
		// we need at least 2 steps to correctly clip it to the viewport
		// however, we can make an observation in our case (make a drawing, it'll help!)
		// example for TopRight:
		// if the end point is higher vertically than it is to the side horizontally (more or less y > x)
		//    then the intersection will be with the TOP side
		// otherwhise, if the point is more to the side horizontally than it is to the top veritcally ( conceptually x > y )
		//    then the intersection will be at the RIGHT side
		// etc.
		// this way, we can directly pick the right quadrant

		// to make this work in calculations, we need to take into account that our start point is not at (0,0) but rather somewhere in the viewport rectangle
		// so we need to offset the end values with the start values specifically
		// NOTE: this only works in our current setup since we're sure the start point is inside the viewport!!!
		
		// the edge case here is if it's equidistant in both x and y (line from start to tend goes through one of the corners of the viewport rectangle)
		// in those cases, it doesn't really matter, as the intersection will be the corner, and both TOP and RIGHT will give the correct coordinate for that as well

		// NOTE: normally, we wouldn't need Mathf.Abs() on all the numbers, just on the ones we know are negative
		// however, because of some "bug" in Unity's viewport handling (see the InvertIntersection stuff in other functions), it's best to always use mathf.abs here to be sure this logic never breaks

		float vertical = end.y - start.y;
		float horizontal = end.x - start.x;

		vertical = Mathf.Abs(vertical);
		horizontal = Mathf.Abs( horizontal );

		
		//Debug.Log ("Clipping : " + end + " // " + outcode );


		if( outcode == OutCode.TopRight )
		{
			if( vertical > horizontal ) // higher than it is to the right : top intersection
				outcode = OutCode.Top;
			else // farther to the right than it is high: rightside intersection
				outcode = OutCode.Right;
		}
		else if( outcode == OutCode.BottomRight )
		{
			if( vertical > horizontal  ) // lower than it is to the right 
				outcode = OutCode.Bottom;
			else
				outcode = OutCode.Right;
		}
		else if( outcode == OutCode.BottomLeft ) 
		{
			if( vertical > horizontal  ) // lower than it is to the left
				outcode = OutCode.Bottom;
			else
				outcode = OutCode.Left;
		}
		else if( outcode == OutCode.TopLeft )
		{
			if( vertical > horizontal  ) // higher than to the left
				outcode = OutCode.Top;
			else
				outcode = OutCode.Left;
		}

		// step 3 : calculate the intersections (depens on the quadrant / outcode which formula we use)

		// given start = p1 = (x0, y0)    end = p2 = (x1, y1)
		// default equations for lines are then:
		// y = mx + b
		// x = 1/m y + c  (solve the above one for x)
		// where m is the "slope" = dy / dx = (y1 - y0) / (x1 - x0)

		// fill these in (b = y0, c = x0)
		// y = y0 + slope * (x1 - x0)
		// x = x0 + (1 / slope) * (y1 - y0)

		// now, depending on which side of the rectangle we are, we will chose one of the sides for x or y
		// for example: end is TOP: y1 is going to be ymax (maximum height)("slide" endpoint down along the line to the ymax line), and we just need to calculate x.
		// so in the equation for x, we need to replace y1 with ymax
		// but the slope can stay the same, so we keep y1 in the calculations for the slope
		// NOTE: this is the trick: only replace y1 with ymax outside the slope. The slope keeps (y1 - y0)

		// so for TOP, the equation filled in becomes:
		// x = x0 + (1 / slope) * (ymax - y0)
		// x = x0 + (1 / ( (y1 - y0) / (x1 - x0))) * (ymax - y0)
		// x = x0 + ( (x1 - x0) * (ymax - y0) ) / (y1 - y0)

		// if we do these substitions for each of the 4 sides, we get the necessary equations for each intersection point

		if( outcode == OutCode.Top )
		{
			intersection.x = start.x + ( ((end.x - start.x) * (1.0f - start.y)) / (end.y - start.y) );
			intersection.y = 1.0f; // max height of viewport
		}
		else if( outcode == OutCode.Bottom )
		{
			intersection.x = start.x  + ( ((end.x - start.x) * (0.0f - start.y)) / (end.y - start.y) );
			intersection.y = 0.0f; // min height
		}
		else if( outcode == OutCode.Right )
		{
			intersection.x = 1.0f; // max width
			intersection.y = start.y + ( ((end.y - start.y) * (1.0f - start.x)) / (end.x - start.x) );
		}
		else if( outcode == OutCode.Left )
		{
			intersection.x = 0.0f; // min width
			intersection.y = start.y + ( ((end.y - start.y) * (0.0f - start.x)) / (end.x - start.x) );
		}
		
		//Debug.Log ("Clipping 2 : " + end + " // " + outcode + " -> " + intersection );

		return intersection;
	}

	/*
	// http://www.wyrmtale.com/blog/2013/115/2d-line-intersection-in-c
	protected Vector3 LineIntersectionPoint(Vector3 ps1, Vector3 pe1, Vector3 ps2, Vector3 pe2)
	{
		// Get A,B,C of first line - points : ps1 to pe1
		float A1 = pe1.y-ps1.y;
		float B1 = ps1.x-pe1.x;
		
		// Get A,B,C of second line - points : ps2 to pe2
		float A2 = pe2.y-ps2.y;
		float B2 = ps2.x-pe2.x;
		
		// Get delta and check if the lines are parallel
		float delta = A1*B2 - A2*B1;
		if(delta == 0)
		{
			return ps1; // in this case, ps1 is center of the screen
		}

		
		float C1 = A1*ps1.x+B1*ps1.y;
		float C2 = A2*ps2.x+B2*ps2.y;
		
		// now return the Vector2 intersection point
		return new Vector3(
			(B2*C1 - B1*C2)/delta,
			(A1*C2 - A2*C1)/delta,
			0.0f
			);
	}
	*/
}
