using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MecanimEventEmitterWithData : MonoBehaviour {

	public UnityEngine.Object animatorController;
	public Animator animator;
	public MecanimEventEmitTypes emitType = MecanimEventEmitTypes.Default;
	public MecanimEventData data;
	
	private Dictionary<int, Dictionary<int, Dictionary<int, List<MecanimEvent>>>> loadedData;
	
	private Dictionary<int, Dictionary<int, AnimatorStateInfo>> lastStates = new Dictionary<int, Dictionary<int, AnimatorStateInfo>>();
	
	void Start() {
		if (animator == null) {
			Debug.LogWarning(string.Format("GameObject:{0} cannot find animator component.",this.transform.name));
			this.enabled = false;
			return;
		}
				
		if (animatorController == null) {
			Debug.LogWarning("Please assgin animator in editor. Add emitter at runtime is not currently supported.");
			this.enabled = false;
			return;
		}
		
		if (data == null){
			this.enabled = false;
			return;
		}
			
		loadedData = MecanimEventManager.LoadData(data);
	}
	
	void Update () {
		MecanimEvent[] events = MecanimEventManager.GetEvents(loadedData, lastStates, animatorController.GetInstanceID(), animator);
		
		foreach (MecanimEvent e in events) {
			
			MecanimEvent.SetCurrentContext(e);
			
			switch(emitType)
			{
			case MecanimEventEmitTypes.Upwards:
				if (e.paramType != MecanimEventParamTypes.None)
					SendMessageUpwards(e.functionName, e.parameter, SendMessageOptions.DontRequireReceiver);
				else
					SendMessageUpwards(e.functionName, SendMessageOptions.DontRequireReceiver);				
				break;
				
			case MecanimEventEmitTypes.Broadcast:
				if (e.paramType != MecanimEventParamTypes.None)
					BroadcastMessage(e.functionName, e.parameter, SendMessageOptions.DontRequireReceiver);
				else
					BroadcastMessage(e.functionName, SendMessageOptions.DontRequireReceiver);				
				break;
				
			default:
				if (e.paramType != MecanimEventParamTypes.None)
					SendMessage(e.functionName, e.parameter, SendMessageOptions.DontRequireReceiver);
				else
					SendMessage(e.functionName, SendMessageOptions.DontRequireReceiver);
				break;
			}
		}
	}
}
