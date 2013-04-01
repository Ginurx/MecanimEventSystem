
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

public class MecanimEventEditor : EditorWindow {
	public static MecanimEventInspector eventInspector;
	
	public float PlaybackTime {
		get { return playbackTime; }
	}
	
	public UnityEngine.Object TargetController {
		set {
			targetController = value as AnimatorController;
		}
	}
	
	private AnimatorController targetController;
	private StateMachine targetStateMachine;
	private State targetState;
	
	
	private int selectedLayer = 0;
	private int selectedState = 0;
	private bool showEventName = true;
	
	private List<MecanimEvent> displayEvents;
	
	[MenuItem ("Window/Mecanim Event Editor")]
	static void Init () {
		EditorWindow.GetWindow<MecanimEventEditor>();
	}
	
	void OnEnable() {
		minSize = new Vector2(600,320);
	}
	
	void OnDisable() {
		MecanimEventEditorPopup.Destroy();
		
		if (eventInspector != null) {
			eventInspector.SetPreviewMotion(null);
			eventInspector.SaveData();
		}
	}
	
	void OnInspectorUpdate() {
		Repaint();
	}
	
	public void DelEvent(MecanimEvent e) {
		if (displayEvents != null) {
			displayEvents.Remove(e);
			SaveState();
		}
	}
	
	void Reset() {
		displayEvents = null;
		targetController = null;
		targetStateMachine = null;
		targetState = null;
		selectedLayer = 0;
		selectedState = 0;
		
		MecanimEventEditorPopup.Destroy();
	}
	
	public KeyValuePair<string, EventConditionParamTypes>[] GetConditionParameters() {
		List<KeyValuePair<string, EventConditionParamTypes>> ret = new List<KeyValuePair<string, EventConditionParamTypes>>();
		if (targetController != null) {
			for (int i = 0; i < targetController.GetEventCount(); i++) {
				switch(targetController.GetEventType(i)) {
				case 1:		// float
					ret.Add(new KeyValuePair<string, EventConditionParamTypes>(targetController.GetEventName(i), EventConditionParamTypes.Float));
					break;
				case 3:		// int
					ret.Add(new KeyValuePair<string, EventConditionParamTypes>(targetController.GetEventName(i), EventConditionParamTypes.Int));
					break;
				case 4:		// bool
					ret.Add(new KeyValuePair<string, EventConditionParamTypes>(targetController.GetEventName(i), EventConditionParamTypes.Boolean));
					break;
				}
			}
		}
		
		return ret.ToArray();
	}
	
	private void SaveState() {
		eventInspector.SetEvents(targetController.GetInstanceID(), selectedLayer, targetState.GetUniqueNameHash(), displayEvents.ToArray());
	}
	
	void OnGUI() {
		if (eventInspector == null) {
			Reset();
			ShowNotification(new GUIContent("Select a MecanimEventData object first."));
			return;
		}
		
		RemoveNotification();
		
		EditorGUI.BeginChangeCheck();
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Put an AnimatorController here");
		targetController = EditorGUILayout.ObjectField(targetController, typeof(AnimatorController), false) as AnimatorController;
		EditorGUILayout.EndHorizontal();
		
		if (targetController == null)
			return;
		
		eventInspector.SaveLastEditController(targetController);
		
		int layerCount = targetController.GetLayerCount();		
		EditorGUILayout.LabelField(layerCount + " layer be found. Please select a layer to continue.");
		
		string[] layerNames = new string[layerCount];
		
		for (int layer = 0; layer < layerCount; layer++) {
			layerNames[layer] = layer.ToString() + " " + targetController.GetLayerName(layer);
		}
		
		selectedLayer = Mathf.Clamp(selectedLayer, 0, layerCount - 1);
		selectedLayer = GUILayout.Toolbar(selectedLayer, layerNames);
		
		targetStateMachine = targetController.GetLayerStateMachine(selectedLayer);
		
		List<State> availabeStates = targetStateMachine.statesRecursive;
		List<string> stateNames = new List<string>();
		
		foreach (State s in availabeStates) {
			stateNames.Add(s.GetUniqueName());
		}
		
		if (availabeStates.Count == 0) {
			EditorGUILayout.LabelField("No state available in this layer.");
			return;
		}
			
		EditorGUILayout.LabelField(availabeStates.Count + " state be found. Please select a state, where events will be inserted into.");
		
		selectedState = Mathf.Clamp(selectedState, 0, availabeStates.Count - 1);
		
		selectedState = GUILayout.SelectionGrid(selectedState, stateNames.ToArray(), 5);
		
		targetState = availabeStates[selectedState];
		
		if (targetState.GetMotion(0) != null) {
			eventInspector.SetPreviewMotion(targetState.GetMotion(0));
			displayEvents = new List<MecanimEvent>(eventInspector.GetEvents(targetController.GetInstanceID(), selectedLayer, targetState.GetUniqueNameHash()));
			
			if (EditorGUI.EndChangeCheck())
				MecanimEventEditorPopup.Destroy();
			
			EditorGUILayout.Separator();
			EditorGUILayout.Separator();
			
			BeginWindows();
			GUI.Window(0, new Rect(0, position.height - 180, position.width, 180), TimelineWindow, targetState.GetName());
			EndWindows();
		}
		else {
			eventInspector.SetPreviewMotion(null);
		}
	}
	
	private float playbackTime = 0.0f;
	
	private bool enableTempPreview = false;
	private float tempPreviewPlaybackTime = 0.0f;
	
	private static int timelineHash = "timelinecontrol".GetHashCode();
	private static int eventKeyHash = "eventkeycontrol".GetHashCode();
	private Vector2 scrollPos = new Vector2(0,0);
	
	private void TimelineWindow(int id) {
		// List
		Rect listRect = new Rect(10,20,position.width * 0.3f,150);
		
		GUILayout.BeginArea(listRect, "List of Events",GUI.skin.window);
		scrollPos = GUILayout.BeginScrollView(scrollPos);
		
		displayEvents.Sort(delegate(MecanimEvent a, MecanimEvent b) { return a.normalizedTime.CompareTo(b.normalizedTime); } );
		
		foreach(MecanimEvent e in displayEvents) {
			string labalName = string.Format("{0}({1})@{2}", e.functionName, e.parameter, e.normalizedTime.ToString("0.0000"));
			if (GUILayout.Button(labalName)) {
				MecanimEventEditorPopup.Show(this, e, GetConditionParameters());
			}
		}
		
		GUILayout.EndScrollView();
		GUILayout.EndArea();
		
		// Timeline
		
		Rect rect = new Rect(position.width * 0.3f + 20, 50, position.width*0.7f-30, 180);	
		
		if (!enableTempPreview)
			playbackTime = eventInspector.GetPlaybackTime();
		
		playbackTime = Timeline(rect, playbackTime);
		
		if (enableTempPreview) {
			eventInspector.SetPlaybackTime(tempPreviewPlaybackTime);
			eventInspector.StopPlaying();
		}
		else {
			eventInspector.SetPlaybackTime(playbackTime);
		}

		GUI.Label(new Rect(position.width - 50, 40, 50, 15), playbackTime.ToString("0.0000"));
		
		foreach(MecanimEvent e in displayEvents) {
			DrawEventKey(rect, e);
		}
		
		if (GUI.Button(new Rect(position.width * 0.3f + 20 + 5, 140, 80, 20), "Add Event")) {
			MecanimEvent newEvent = new MecanimEvent();
			newEvent.normalizedTime = playbackTime;
			newEvent.functionName = "MessageName";
			newEvent.paramType = MecanimEventParamTypes.None;
			
			displayEvents.Add(newEvent);
			MecanimEventEditorPopup.Show(this, newEvent, GetConditionParameters());
		}
		
		if (GUI.Button(new Rect(position.width * 0.3f + 20 + 105, 140, 80, 20), "Save")) {
			eventInspector.SaveData();
		}
		
		EditorGUI.LabelField(new Rect(position.width - 150, 140, 150, 20), "Show Event Name");
		showEventName = EditorGUI.Toggle(new Rect(position.width - 30, 140, 30, 20), showEventName);
		
		SaveState();
	}
	
	private float Timeline(Rect rect, float time) {
		int timelineId = GUIUtility.GetControlID(timelineHash, FocusType.Native);
		
		Rect thumbRect = new Rect(rect.x + rect.width * time - 5, rect.y + 2, 10, 10);
		
		Event e = Event.current;
		
		switch(e.type) {
		case EventType.Repaint:
			//GUI.skin.horizontalSlider.Draw(rect, new GUIContent(), timelineId);
			Rect lineRect = new Rect(rect.x, rect.y+10, rect.width, 1.5f);
			DrawTimeLine(lineRect, time);
			GUI.skin.horizontalSliderThumb.Draw(thumbRect, new GUIContent(), timelineId);
			break;
			
		case EventType.MouseDown:
			if (thumbRect.Contains(e.mousePosition)) {
				GUIUtility.hotControl = timelineId;
				e.Use();
			}
			break;
			
		case EventType.MouseUp:
			if (GUIUtility.hotControl == timelineId) {
				GUIUtility.hotControl = 0;
				e.Use();
			}
			break;
			
		case EventType.MouseDrag:
			if (GUIUtility.hotControl == timelineId) {
				Vector2 guiPos = e.mousePosition;
				
				float clampedX = Mathf.Clamp(guiPos.x, rect.x, rect.x + rect.width);
				time = (clampedX - rect.x) / rect.width;
				
				e.Use();
			}
			break;
		}
		
		return time;
	}
	
	private void DrawTimeLine(Rect rect, float currentFrame) {
		if (Event.current.type != EventType.Repaint)
		{
			return;
		}
		
		HandleUtilityWrapper.handleWireMaterial.SetPass(0);
		Color c = new Color(1f, 0f, 0f, 0.75f);
		GL.Color(c);
		
		GL.Begin(GL.LINES);
		GL.Vertex3(rect.x, rect.y, 0);
		GL.Vertex3(rect.x + rect.width, rect.y, 0);
		
		GL.Vertex3(rect.x, rect.y+25, 0);
		GL.Vertex3(rect.x + rect.width, rect.y+25, 0);

		
		for(int i = 0; i <= 100; i+=1) {
			if (i % 10 == 0) {
				GL.Vertex3(rect.x + rect.width*i/100f, rect.y, 0);
				GL.Vertex3(rect.x + rect.width*i/100f, rect.y + 15, 0);
			}
			else if (i % 5 == 0){
				GL.Vertex3(rect.x + rect.width*i/100f, rect.y, 0);
				GL.Vertex3(rect.x + rect.width*i/100f, rect.y + 10, 0);
			}
			else {
				GL.Vertex3(rect.x + rect.width*i/100f, rect.y, 0);
				GL.Vertex3(rect.x + rect.width*i/100f, rect.y + 5, 0);
			}
		}
		
		c = new Color(1.0f, 1.0f, 1.0f, 0.75f);
		GL.Color(c);
		
		GL.Vertex3(rect.x + rect.width*currentFrame, rect.y, 0);
		GL.Vertex3(rect.x + rect.width*currentFrame, rect.y + 20, 0);
		
		GL.End();
	}
	
	private void DrawEventKey(Rect rect, MecanimEvent key) {
		float keyTime = key.normalizedTime;
		
		int eventKeyCtrl = GUIUtility.GetControlID(eventKeyHash, FocusType.Native);
		
		Rect keyRect = new Rect(rect.x + rect.width * keyTime - 3, rect.y+25, 6, 18);
		
		Event e = Event.current;
		
		switch(e.type) {
		case EventType.Repaint:
			Color savedColor = GUI.color;
			GUI.color = Color.green;
			GUI.skin.button.Draw(keyRect, new GUIContent(), eventKeyCtrl);
			GUI.color = savedColor;
			
			if (showEventName || keyRect.Contains(e.mousePosition)) {
				Vector2 size = GUI.skin.textField.CalcSize(new GUIContent(key.functionName));
				
				Rect infoRect= new Rect(rect.x + rect.width * keyTime - size.x/2, rect.y - 30, size.x, size.y);
				GUI.skin.textField.Draw(infoRect, new GUIContent(key.functionName), eventKeyCtrl);
			}
			break;
			
		case EventType.MouseDown:
			if (keyRect.Contains(e.mousePosition)) {
				GUIUtility.hotControl = eventKeyCtrl;
				enableTempPreview =true;
				tempPreviewPlaybackTime = key.normalizedTime;
				e.Use();	
			}
			break;
		case EventType.MouseDrag:
			if (GUIUtility.hotControl == eventKeyCtrl) {
				Vector2 guiPos = e.mousePosition;
				float clampedX = Mathf.Clamp(guiPos.x, rect.x, rect.x + rect.width);
				key.normalizedTime = (clampedX - rect.x) / rect.width;
				tempPreviewPlaybackTime = key.normalizedTime;
				e.Use();
			}
			break;
		case EventType.MouseUp:
			if (GUIUtility.hotControl == eventKeyCtrl) {
				GUIUtility.hotControl = 0;
				enableTempPreview = false;
				eventInspector.SetPlaybackTime(playbackTime);		// reset to original time
				MecanimEventEditorPopup.Show(this, key, GetConditionParameters());
				e.Use();
			}
			break;
		}
	}
}
