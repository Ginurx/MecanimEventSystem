using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;


[CustomEditor(typeof(MecanimEventData))]
public class MecanimEventInspector : Editor {
	// Controller -> Layer -> State
	private Dictionary<int, Dictionary<int, Dictionary<int, List<MecanimEvent>>>> data;
	
	void OnEnable() {
		LoadData();
		
		MecanimEventEditor.eventInspector = this;	
	}
	
	void OnDisable() {
		//SaveData();
		MecanimEventEditor.eventInspector = null;
		
		OnPreviewDisable();
	}
	
	public override void OnInspectorGUI ()
	{
		if (GUILayout.Button("Open Event Editor")) {
			MecanimEventEditor editor = EditorWindow.GetWindow<MecanimEventEditor>();
			editor.TargetController = serializedObject.FindProperty("lastEdit").objectReferenceValue;
		}
		
		if (previewedMotion != null && previewedMotion is BlendTree && avatarPreview != null) {
			EditorGUILayout.Separator();
			GUILayout.Label("BlendTree Parameter(s)", GUILayout.ExpandWidth(true));
			
			BlendTree bt = previewedMotion as BlendTree;
			
			for (int i = 0; i < bt.GetRecursiveBlendEventCount(); i++) {
				float min = bt.GetRecursiveBlendEventMin(i);
				float max = bt.GetRecursiveBlendEventMax(i);
				
				string paramName = bt.GetRecursiveBlendEvent(i);
				float value = Mathf.Clamp(avatarPreview.Animator.GetFloat(paramName), min, max);
				value = EditorGUILayout.Slider(paramName, value, min, max);
				avatarPreview.Animator.SetFloat(paramName, value);
			}
		}
	}
	
	public MecanimEvent[] GetEvents(int controllerId, int layer, int stateNameHash) {
		try {
			return data[controllerId][layer][stateNameHash].ToArray();
		}
		catch {
			return new MecanimEvent[0];
		}
	}
	
	public void SetEvents(int controllerId, int layer, int stateNameHash, MecanimEvent[] events) {
		if (!data.ContainsKey(controllerId)) {
			data[controllerId] = new Dictionary<int, Dictionary<int, List<MecanimEvent>>>();
		}
		
		if (!data[controllerId].ContainsKey(layer)) {
			data[controllerId][layer] = new Dictionary<int, List<MecanimEvent>>();
		}
		
		if (!data[controllerId][layer].ContainsKey(stateNameHash)) {
			data[controllerId][layer][stateNameHash] = new List<MecanimEvent>();
		}
		
		data[controllerId][layer][stateNameHash] = new List<MecanimEvent>(events);
	}
	
	private Motion previewedMotion;
	private AvatarPreviewWrapper avatarPreview;
	private StateMachine stateMachine;
	private State state;
	private AnimatorController controller;
	
	private bool PrevIKOnFeet = false;
	//
	
	public void SetPreviewMotion(Motion motion) {
		if (previewedMotion == motion)
			return;
		
		previewedMotion = motion;
		
		ClearStateMachine();
		
		if (avatarPreview == null)
		{
			avatarPreview = new AvatarPreviewWrapper(null, previewedMotion);
			avatarPreview.OnAvatarChangeFunc = this.OnPreviewAvatarChanged;
			PrevIKOnFeet = avatarPreview.IKOnFeet;
		}
		
		if (motion != null)
			CreateStateMachine();
		
		Repaint();
	}
	
	public float GetPlaybackTime() {
		if (avatarPreview != null)
			return avatarPreview.timeControl.normalizedTime;
		else
			return 0;
	}
	
	public void SetPlaybackTime(float time) {
		avatarPreview.timeControl.nextCurrentTime = Mathf.Lerp(avatarPreview.timeControl.startTime,
																avatarPreview.timeControl.stopTime,
																time);
		Repaint();
	}
	
	public bool IsPlaying() {
		return avatarPreview.timeControl.playing;
	}
	
	public void StopPlaying() {
		avatarPreview.timeControl.playing = false;
	}
	
	public override bool HasPreviewGUI ()
	{
		//return previewedMotion != null;
		return true;
	}
	
	public override void OnPreviewSettings ()
	{
		if (avatarPreview != null)
			avatarPreview.DoPreviewSettings();
	}
	
	public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
	{
		if (avatarPreview == null || previewedMotion == null)
			return;
		
		UpdateAvatarState();
		avatarPreview.DoAvatarPreview(r, background);
	}
	
	private void OnPreviewDisable() {
		previewedMotion = null;
		
		ClearStateMachine();
		if (avatarPreview != null) {
			avatarPreview.OnDestroy();
			avatarPreview = null;
		}
	}
	
	private void OnPreviewAvatarChanged()
	{
		ResetStateMachine();
	}
	
	private void CreateStateMachine() {
		if (controller == null)
		{
			controller = new AnimatorController();
			controller.AddLayer("preview");
			CreateEvents();
			controller.hideFlags = HideFlags.DontSave;
		}
		
		if (stateMachine == null)
		{
			stateMachine = new StateMachine();
			stateMachine.hideFlags = HideFlags.DontSave;
			controller.SetLayerStateMachine(0, this.stateMachine);
		}
		
		if (state == null)
		{
			state = stateMachine.AddState("preview");
			state.SetMotion(0, previewedMotion);
			state.SetIKOnFeet(avatarPreview.IKOnFeet);
			state.hideFlags = HideFlags.DontSave;
		}
		
		AnimatorController.SetAnimatorController(avatarPreview.Animator, controller);
	}
	
	private void CreateEvents()
	{
		int eventCount = controller.GetEventCount();
		for (int i = 0; i < eventCount; i++)
		{
			controller.RemoveEvent(0);
		}
		
		if (previewedMotion is BlendTree)
		{
			BlendTree blendTree = previewedMotion as BlendTree;
			
			for (int j = 0; j < blendTree.GetRecursiveBlendEventCount(); j++)
			{
				controller.AddEvent(blendTree.GetRecursiveBlendEvent(j), AnimatorControllerEventType.Float);
			}
		}
	}
	
	private void ClearStateMachine()
	{
		if (avatarPreview != null && avatarPreview.Animator != null)
		{
			AnimatorController.SetAnimatorController(avatarPreview.Animator, null);
		}
		Object.DestroyImmediate(this.controller);
		Object.DestroyImmediate(this.stateMachine);
		Object.DestroyImmediate(this.state);
		stateMachine = null;
		controller = null;
		state = null;
	}
	
	public void ResetStateMachine()
	{
		ClearStateMachine();
		CreateStateMachine();
	}
	
	private void UpdateAvatarState()
	{
		Animator animator = avatarPreview.Animator;
		if (animator)
		{
			if (PrevIKOnFeet != avatarPreview.IKOnFeet)
			{
				PrevIKOnFeet = avatarPreview.IKOnFeet;
				Vector3 rootPosition = avatarPreview.Animator.rootPosition;
				Quaternion rootRotation = avatarPreview.Animator.rootRotation;
				ResetStateMachine();
				avatarPreview.Animator.UpdateWrapper(avatarPreview.timeControl.currentTime);
				avatarPreview.Animator.UpdateWrapper(0f);
				avatarPreview.Animator.rootPosition = rootPosition;
				avatarPreview.Animator.rootRotation = rootRotation;
			}

			avatarPreview.timeControl.loop = true;
			float num = 1f;
			float num2 = 0f;
			if (animator.layerCount > 0)
			{
				AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
				num = currentAnimatorStateInfo.length;
				num2 = currentAnimatorStateInfo.normalizedTime;
			}
			
			avatarPreview.timeControl.startTime = 0f;
			avatarPreview.timeControl.stopTime = num;
			avatarPreview.timeControl.Update();
			
			float num3 = this.avatarPreview.timeControl.deltaTime;
			if (!previewedMotion.isLooping)
			{
				if (num2 >= 1f)
				{
					num3 -= num;
				}
				else
				{
					if (num2 < 0f)
					{
						num3 += num;
					}
				}
			}
			animator.UpdateWrapper(num3);
		}
	}
	
	private void LoadData() {
		data = new Dictionary<int, Dictionary<int, Dictionary<int, List<MecanimEvent>>>>();
		
		serializedObject.Update();
		
		SerializedProperty entries = serializedObject.FindProperty("data");
		
		for (int i = 0; i < entries.arraySize; i++) {
			SerializedProperty entry = entries.GetArrayElementAtIndex(i);
			
			SerializedProperty controllerProperty		= entry.FindPropertyRelative("animatorController");
			SerializedProperty layerProperty			= entry.FindPropertyRelative("layer");
			SerializedProperty stateNameHashProperty 	= entry.FindPropertyRelative("stateNameHash");
			SerializedProperty eventsProperty 			= entry.FindPropertyRelative("events");
			
			
			UnityEngine.Object controller = controllerProperty.objectReferenceValue;
			int layer		 = layerProperty.intValue;
			int stateNameHash = stateNameHashProperty.intValue;
			
			if (controller == null)
				continue;
			
			int controllerId = controller.GetInstanceID();
			
			if (!ValidateState(controllerId, layer, stateNameHash))
				continue;
			
			List<MecanimEvent> eventList = new List<MecanimEvent>();
			
			for (int j = 0; j < eventsProperty.arraySize; j++) {
				SerializedProperty e = eventsProperty.GetArrayElementAtIndex(j);
				
				MecanimEvent newEvent = new MecanimEvent();
				
				newEvent.normalizedTime = e.FindPropertyRelative("normalizedTime").floatValue;
				newEvent.functionName = e.FindPropertyRelative("functionName").stringValue;
				newEvent.paramType = (MecanimEventParamTypes)e.FindPropertyRelative("paramType").enumValueIndex;
				newEvent.intParam = e.FindPropertyRelative("intParam").intValue;
				newEvent.floatParam = e.FindPropertyRelative("floatParam").floatValue;
				newEvent.stringParam = e.FindPropertyRelative("stringParam").stringValue;
				newEvent.boolParam = e.FindPropertyRelative("boolParam").boolValue;
				
				SerializedProperty conditionProperty = e.FindPropertyRelative("condition");
				SerializedProperty conditionsProperty = conditionProperty.FindPropertyRelative("conditions");
				
				for (int k = 0; k < conditionsProperty.arraySize; k++) {
					SerializedProperty conditionEntryProperty = conditionsProperty.GetArrayElementAtIndex(k);
					
					EventConditionEntry conditionEntry = new EventConditionEntry();
					
					conditionEntry.conditionParam = conditionEntryProperty.FindPropertyRelative("conditionParam").stringValue;
					conditionEntry.conditionParamType = (EventConditionParamTypes)conditionEntryProperty.FindPropertyRelative("conditionParamType").enumValueIndex;
					conditionEntry.conditionMode = (EventConditionModes)conditionEntryProperty.FindPropertyRelative("conditionMode").enumValueIndex;
					conditionEntry.intValue = conditionEntryProperty.FindPropertyRelative("intValue").intValue;
					conditionEntry.floatValue = conditionEntryProperty.FindPropertyRelative("floatValue").floatValue;
					conditionEntry.boolValue = conditionEntryProperty.FindPropertyRelative("boolValue").boolValue;
					
					newEvent.condition.conditions.Add(conditionEntry);
				}
				
				eventList.Add(newEvent);
			}
			
			if (!data.ContainsKey(controllerId))
				data[controllerId] = new Dictionary<int, Dictionary<int, List<MecanimEvent>>>();
			
			if (!data[controllerId].ContainsKey(layer))
				data[controllerId][layer] = new Dictionary<int, List<MecanimEvent>>();
			
			data[controllerId][layer][stateNameHash] = eventList;
		}
	}
	
	public void SaveData() {
		SerializedProperty entries = serializedObject.FindProperty("data");
		entries.ClearArray();
		
		foreach(int controllerId in data.Keys) {
			foreach(int layer in data[controllerId].Keys) {
				foreach(int stateNameHash in data[controllerId][layer].Keys) {
					if (data[controllerId][layer][stateNameHash].Count == 0)
						continue;
					
					UnityEngine.Object controller = EditorUtility.InstanceIDToObject(controllerId);
					if (controller == null) {
						Debug.Log("Controller whose ID " + controllerId + " is no longer exist when saving data.");
						continue;
					}
					
					entries.InsertArrayElementAtIndex(entries.arraySize);
					SerializedProperty entry = entries.GetArrayElementAtIndex(entries.arraySize-1);
					
					entry.FindPropertyRelative("animatorController").objectReferenceValue = controller;
					entry.FindPropertyRelative("layer").intValue = layer;
					entry.FindPropertyRelative("stateNameHash").intValue = stateNameHash;
					
					SerializedProperty events = entry.FindPropertyRelative("events");
					events.ClearArray();
						
					foreach(MecanimEvent e in data[controllerId][layer][stateNameHash]) {
						events.InsertArrayElementAtIndex(events.arraySize);
						SerializedProperty eventProperty = events.GetArrayElementAtIndex(events.arraySize-1);
						
						eventProperty.FindPropertyRelative("normalizedTime").floatValue = e.normalizedTime;
						eventProperty.FindPropertyRelative("functionName").stringValue = e.functionName;
						eventProperty.FindPropertyRelative("paramType").enumValueIndex = (int)e.paramType;
						eventProperty.FindPropertyRelative("intParam").intValue = e.intParam;
						eventProperty.FindPropertyRelative("floatParam").floatValue = e.floatParam;
						eventProperty.FindPropertyRelative("stringParam").stringValue = e.stringParam;
						eventProperty.FindPropertyRelative("boolParam").boolValue = e.boolParam;
						
						SerializedProperty conditionProperty = eventProperty.FindPropertyRelative("condition");
						SerializedProperty conditionArrayProperty = conditionProperty.FindPropertyRelative("conditions");
						conditionArrayProperty.ClearArray();
						
						foreach (EventConditionEntry conditionEntry in e.condition.conditions) {
							conditionArrayProperty.InsertArrayElementAtIndex(conditionArrayProperty.arraySize);
							SerializedProperty conditionEntryProperty = conditionArrayProperty.GetArrayElementAtIndex(conditionArrayProperty.arraySize-1);
							
							conditionEntryProperty.FindPropertyRelative("conditionParam").stringValue = conditionEntry.conditionParam;
							conditionEntryProperty.FindPropertyRelative("conditionParamType").enumValueIndex = (int)conditionEntry.conditionParamType;
							conditionEntryProperty.FindPropertyRelative("conditionMode").enumValueIndex = (int)conditionEntry.conditionMode;
							conditionEntryProperty.FindPropertyRelative("floatValue").floatValue = conditionEntry.floatValue;
							conditionEntryProperty.FindPropertyRelative("intValue").intValue = conditionEntry.intValue;
							conditionEntryProperty.FindPropertyRelative("boolValue").boolValue = conditionEntry.boolValue;
						}
					}
				}
			}
		}
		
		serializedObject.ApplyModifiedProperties();
	}
	
	public void SaveLastEditController(UnityEngine.Object controller) {
		serializedObject.FindProperty("lastEdit").objectReferenceValue = controller;;
	}
	
	private bool ValidateState(int controllerId, int layer, int stateNameHash) {
		if (!ValidateControllerId(controllerId))
			return false;
		
		if (!ValidateLayer(controllerId, layer))
			return false;
		
		AnimatorController controller = EditorUtility.InstanceIDToObject(controllerId) as AnimatorController;
		StateMachine sm = controller.GetLayerStateMachine(layer);
		
		return FindStateRecursively(sm, stateNameHash);
	}
	
		
	private bool ValidateControllerId(int controllerId) {
		AnimatorController controller = EditorUtility.InstanceIDToObject(controllerId) as AnimatorController;
		
		if (controller == null)
			return false;
		
		return true;
	}
	
	private bool ValidateLayer(int controllerId, int layer) {
		AnimatorController controller = EditorUtility.InstanceIDToObject(controllerId) as AnimatorController;
		
		if (controller == null)
			return false;
		
		if (layer >= 0 && layer < controller.GetLayerCount())
			return true;
		else
			return false;
	}
	
	private bool FindStateRecursively(StateMachine stateMachine, int nameHash) {
		for (int i = 0; i < stateMachine.GetStateCount(); i++) {
			if (stateMachine.GetState(i).GetUniqueNameHash() == nameHash)
				return true;
		}
		
		for (int i = 0; i < stateMachine.GetStateMachineCount(); i++) {
			StateMachine tempSM = stateMachine.GetStateMachine(i);
			if (FindStateRecursively(tempSM, nameHash))
				return true;
		}
		
		return false;
	}
}