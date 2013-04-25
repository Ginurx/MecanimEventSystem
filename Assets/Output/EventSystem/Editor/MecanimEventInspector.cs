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
		
		MecanimEventData dataSource = target as MecanimEventData;
		
		data = new Dictionary<int, Dictionary<int, Dictionary<int, List<MecanimEvent>>>>();
		
		if (dataSource.data == null || dataSource.data.Length == 0)
			return;
		
		foreach(MecanimEventDataEntry entry in dataSource.data) {
			
			int animatorControllerId = entry.animatorController.GetInstanceID();
			
			if (!data.ContainsKey(animatorControllerId))
				data[animatorControllerId] = new Dictionary<int, Dictionary<int, List<MecanimEvent>>>();
			
			if (!data[animatorControllerId].ContainsKey(entry.layer)) {
				data[animatorControllerId][entry.layer] = new Dictionary<int, List<MecanimEvent>>();
			}
			
			List<MecanimEvent> events = new List<MecanimEvent>();
			
			if (entry.events != null) {
				foreach (MecanimEvent e in entry.events) {
					
					events.Add(new MecanimEvent(e));
					
				}
			}
			
			data[animatorControllerId][entry.layer][entry.stateNameHash] = events;
			
		}
	}
	
	public void SaveData() {
		
		MecanimEventData targetData = target as MecanimEventData;
		Undo.RegisterUndo(target, "Mecanim Event Data");
		
		List<MecanimEventDataEntry> entries = new List<MecanimEventDataEntry>();

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
					
					MecanimEventDataEntry entry = new MecanimEventDataEntry();
					entry.animatorController = controller;
					entry.layer = layer;
					entry.stateNameHash = stateNameHash;
					entry.events = data[controllerId][layer][stateNameHash].ToArray();;
					
					entries.Add(entry);
				}
			}
		}
		
		targetData.data = entries.ToArray();
		
		EditorUtility.SetDirty(target);
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