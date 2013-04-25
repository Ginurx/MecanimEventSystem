using UnityEngine;
using System.Collections;

public class MecanimEventSetupHelper : MonoBehaviour {
	public MecanimEventData dataSource;
	
	public MecanimEventData[] dataSources;
	
	void Awake() {
		if (dataSource == null && (dataSources == null || dataSources.Length == 0)) {
			Debug.Log("Please setup data source of event system.");
			return;
		}
		
		if (dataSource != null)
			MecanimEventManager.SetEventDataSource(dataSource);
		else
			MecanimEventManager.SetEventDataSource(dataSources);
	}
}
