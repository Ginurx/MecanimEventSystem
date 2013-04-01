using UnityEngine;
using System.Collections;
using System.Reflection;


public class MecanimEventData : MonoBehaviour {
	public MecanimEventDataEntry[] data;
	
	public UnityEngine.Object lastEdit;
}

[System.Serializable]
public class MecanimEventDataEntry {
	public UnityEngine.Object animatorController;
	public int layer;
	public int stateNameHash;
	public MecanimEvent[] events;
}