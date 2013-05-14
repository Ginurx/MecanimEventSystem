using UnityEngine;
using System.Collections;

public enum MecanimEventEmitTypes {
	Default,
	Upwards,
	Broadcast,
}

public class MecanimEventEmitter : MonoBehaviour {

	public UnityEngine.Object animatorController;
	public Animator animator;
	public MecanimEventEmitTypes emitType = MecanimEventEmitTypes.Default;	
	
	void Start() {
		if (animator == null) {
			Debug.LogWarning("Do not find animator component.");
			this.enabled = false;
			return;
		}
				
		if (animatorController == null) {
			Debug.LogWarning("Please assgin animator in editor. Add emitter at runtime is not currently supported.");
			this.enabled = false;
			return;
		}
	}
	
	void Update () {
		MecanimEvent[] events = MecanimEventManager.GetEvents(animatorController.GetInstanceID(), animator);
		
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
