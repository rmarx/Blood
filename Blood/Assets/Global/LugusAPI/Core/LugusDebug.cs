using UnityEngine;
using System.Collections;

public class LugusDebug : MonoBehaviour 
{
	public static bool allowDebugging = true; // at BUILD time

	public static bool debug = false; // at RUN time. Changing this prior to build has little effect on debuggable-ness, only whether debugging menus are enabled from the start or not. 
}
