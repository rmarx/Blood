using UnityEngine;
using System.Collections;
using System;

public class WaitForAnimationState : IEnumerator 
{
	public Animator animator = null;
	public string stateName = "";
	public int stateHash = -1;
	
	public int layerIndex = 0;
	
	protected bool justStarted = true;
	
	public static UnityEngine.Coroutine New(MonoBehaviour animatorSibling, string stateName)
	{
		return animatorSibling.StartCoroutine( new WaitForAnimationState(animatorSibling, stateName) );
	}
	
	public WaitForAnimationState(MonoBehaviour animatorSibling, string stateName)
	{
		//Debug.LogError("WaitForAnimationState CTOR");
		
		animator = animatorSibling.GetComponent<Animator>();
		this.stateName = stateName;
		this.stateHash = Animator.StringToHash(stateName);
	}
	
	public WaitForAnimationState(Animator animator, string stateName)
	{
		this.animator = animator;
		this.stateName = stateName;
		this.stateHash = Animator.StringToHash(stateName);
	}
	
	public void Reset()
	{
		justStarted = true;
	}
	
	public bool MoveNext()
	{
		//Debug.LogError("MoveNext");
		
		if( animator == null )
		{
			//Debug.LogError("ANIMATOR FALSE");
			return false;
		}
		
		if( justStarted )
		{
			return true;
		}
		
		if( animator.GetCurrentAnimatorStateInfo(layerIndex).nameHash == stateHash )
		{
			//Debug.Log ("WaitForAnimationState : state has been reached");
			return false;
		}
		else
		{
			//Debug.Log ("WaitForAnimationState : state not yet reached");
			return true;
		}
		
		//.GetCurrentAnimatorStateInfo(0).nameHash == Animator.StringToHash("Base Layer.Idle")
	}
	
	public object Current
	{
		get
		{
			//Debug.LogError("WaitForAnimationState Current");
			
			if( this.justStarted )
			{
				//Debug.Log ("WaitForAnimationState : started : waiting 0.3f seconds");
				
				justStarted = false;
				return new WaitForSeconds(0.3f); // Animator state transitions aren't direct... slight delay... we need to compensate!
			}
			
			//if( NotRenamedAttribute stateHash )
			//{
				return null;
			//}
			
			//return null;
		}
	}
}
