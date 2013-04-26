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
	
	public MecanimEventDataEntry() {
		events = new MecanimEvent[0];
	}
	
	public MecanimEventDataEntry(MecanimEventDataEntry other) {
		
		this.animatorController = other.animatorController;
		this.layer = other.layer;
		this.stateNameHash = other.stateNameHash;
		
		if (other.events == null)
			this.events = new MecanimEvent[0];
		else {
			this.events = new MecanimEvent[other.events.Length];
			
			for (int i = 0; i < this.events.Length; i++) {
				
				this.events[i] = new MecanimEvent(other.events[i]);
				
			}
		}
	}
}