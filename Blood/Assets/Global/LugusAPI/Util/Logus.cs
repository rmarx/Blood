#define DEBUG
using UnityEngine;
using System.Collections;

using System;
using System.Diagnostics;

public class Logus 
{
   // [Conditional("UNITY_EDITOR")]
    public static bool Assert( bool condition )
    {	
        if (!condition) 
		{
			UnityEngine.Debug.LogError("LoGus:Assert failed! : " + condition);
			//throw new Exception(); 
			return false;
		}
		else
			return true;
    }
}
