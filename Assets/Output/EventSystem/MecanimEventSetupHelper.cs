using UnityEngine;
using System.Collections;

public class MecanimEventSetupHelper : MonoBehaviour {
	public MecanimEventData dataSource;
	
	void Awake() {
		if (dataSource == null) {
			Debug.Log("Please setup data source of event system.");
			return;
		}
		
		MecanimEventManager.SetEventDataSource(dataSource);
	}
}
