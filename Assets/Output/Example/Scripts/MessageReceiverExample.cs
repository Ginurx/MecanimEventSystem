using UnityEngine;
using System.Collections;

public class MessageReceiverExample : MonoBehaviour {
	string msg;
	
	void OnIdleUpdate(float param) {
		msg = string.Format("OnIdleUpdate received with parameter: {0} {1}", param.GetType(), param);
		
		// You can also get event context by accessing ...
		// MecanimEvent.Context
		
		//Debug.Log(MecanimEvent.Context.stateHash);
	}
	
	void OnLeftFootGrounded(int param) {
		msg = string.Format("OnLeftFootGrounded received with parameter: {0} {1}", param.GetType(), param);
	}
	
	void OnRightFootGrounded(int param) {
		msg = string.Format("OnRightFootGrounded received with parameter: {0} {1}", param.GetType(), param);
	}
	
	void OnJumpGrounded(string param) {
		msg = string.Format("OnJumpGrounded received with parameter: {0} {1}", param.GetType(), param);
	}
	
	void OnJumpStarted(bool param) {
		msg = string.Format("OnJumpGrounded received with parameter: {0} {1}", param.GetType(), param);
	}
	
	void JumpEndCritical() {
		msg = string.Format("JumpEndCritical received with no parameter");
	}
	
	void OnWaved() {
		msg = string.Format("OnWaved received");
	}
	
	void OnGUI() {
		GUI.Label(new Rect(20,40,600,20), msg);
	}
}
