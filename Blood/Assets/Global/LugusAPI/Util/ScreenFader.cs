using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScreenFader : LugusSingletonExisting<ScreenFader>
{
	public Sprite fadeImage = null;

	protected SpriteRenderer fadeRenderer = null;
	protected ILugusCoroutineHandle fadeRoutine = null;

	public void SetupLocal()
	{

	}
	
	public void SetupGlobal()
	{
		if( fadeImage == null )
		{
			fadeImage = LugusResources.use.Shared.GetSprite("ScreenFaderBlack");
		}
		
		GameObject fadeObject = GameObject.Find("ScreenFader");
		if( fadeObject == null )
		{
			fadeObject = new GameObject("ScreenFader");
			fadeObject.transform.parent = this.transform;
			fadeObject.transform.localPosition = new Vector3(LugusUtil.UIWidth / 2.0f, LugusUtil.UIHeight / 2.0f, LugusCamera.ui.transform.position.z + 1.0f );
			// * 100.0f because the image should be 1px wide
			fadeObject.transform.localScale = new Vector3(LugusUtil.UIWidth * 100.0f, LugusUtil.UIHeight * 100.0f, 0.1f ); 
		}
		
		fadeObject.layer = LayerMask.NameToLayer("UI");
		
		fadeRenderer = fadeObject.GetComponent<SpriteRenderer>();
		if( fadeRenderer == null )
		{
			fadeRenderer = fadeObject.AddComponent<SpriteRenderer>();
			fadeRenderer.sprite = fadeImage;
		}

		fadeRenderer.color = fadeRenderer.color.a (0.0f); // set overlay to invisible
		fadeRenderer.enabled = false;
	}
	
	protected void Awake()
	{
		SetupLocal();
	}

	protected void Start () 
	{
		SetupGlobal();
	}

	public void FadeOut(float time)
	{
		//Debug.Log("ScreenFader: Fading out.");

		fadeRenderer.color = fadeRenderer.color.a(0.0f);

		if (fadeRoutine != null && fadeRoutine.Running)
		{
			fadeRoutine.StopRoutine();
		}

		fadeRoutine = LugusCoroutines.use.StartRoutine(FadeRoutine(1.0f, time));
	}

	public void FadeIn(float time)
	{
		//Debug.Log("ScreenFader: Fading in.");

		fadeRenderer.color = fadeRenderer.color.a(1.0f);

		if (fadeRoutine != null && fadeRoutine.Running)
		{
			fadeRoutine.StopRoutine();
		}

		fadeRoutine = LugusCoroutines.use.StartRoutine(FadeRoutine(0.0f, time));
	}

	protected IEnumerator FadeRoutine(float targetAlpha, float duration)
	{
		fadeRenderer.enabled = true;
		
		if (duration <= 0)
		{
			fadeRenderer.color = fadeRenderer.color.a(targetAlpha);
			yield break;
		}
		
		float startAlpha = fadeRenderer.color.a;
		float timerStart = Time.realtimeSinceStartup;
		
		while ((Time.realtimeSinceStartup - timerStart) <= duration)
		{
			fadeRenderer.color = fadeRenderer.color.a( Mathf.Lerp(startAlpha, targetAlpha, (Time.realtimeSinceStartup - timerStart) / duration ));
			yield return null;
		}
		
		fadeRenderer.color = fadeRenderer.color.a(targetAlpha);	// this will ensure the fade always reaches perfect completion

		if (fadeRenderer.color.a <= 0)
		{
			fadeRenderer.enabled = false;
		}

		yield break;
	}
}
