using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
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
	
	public static MecanimEvent clipboard;
	public static MecanimEvent[] stateClipboard;
	public static Dictionary<int, Dictionary<int, MecanimEvent[]>> controllerClipboard;
	
	private AnimatorController targetController;
	private AnimatorStateMachine targetStateMachine;
	private AnimatorState targetState;
	private MecanimEvent targetEvent;
	
	private List<MecanimEvent> displayEvents;
	
	static void Init () {
		EditorWindow.GetWindow<MecanimEventEditor>();
	}
	
	void OnEnable() {
		minSize = new Vector2(850,320);
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
	
	void SortEvents() {
		if (displayEvents != null) {
			displayEvents.Sort(
				delegate(MecanimEvent a, MecanimEvent b) 
				{
					return a.normalizedTime.CompareTo(b.normalizedTime); 
				} 
			);
		}
	}
	
	void Reset() {
		displayEvents = null;
		
		targetController = null;
		targetStateMachine = null;
		targetState = null;
		targetEvent = null;
		
		selectedLayer = 0;
		selectedState = 0;
		selectedEvent = 0;
		
		MecanimEventEditorPopup.Destroy();
	}
	
	public KeyValuePair<string, EventConditionParamTypes>[] GetConditionParameters() {
		List<KeyValuePair<string, EventConditionParamTypes>> ret = new List<KeyValuePair<string, EventConditionParamTypes>>();
		if (targetController != null) {
			foreach (AnimatorControllerParameter animatorParam in targetController.parameters) {
				switch(animatorParam.type) {
				case AnimatorControllerParameterType.Float:		// float
					ret.Add(new KeyValuePair<string, EventConditionParamTypes>(animatorParam.name, EventConditionParamTypes.Float));
					break;
				case AnimatorControllerParameterType.Int:		// int
					ret.Add(new KeyValuePair<string, EventConditionParamTypes>(animatorParam.name, EventConditionParamTypes.Int));
					break;
				case AnimatorControllerParameterType.Bool:		// bool
					ret.Add(new KeyValuePair<string, EventConditionParamTypes>(animatorParam.name, EventConditionParamTypes.Boolean));
					break;
				}
			}
		}
		
		return ret.ToArray();
	}
	
	private void SaveState() {
		if (targetController != null && targetState != null)
		{
			eventInspector.SetEvents(targetController, selectedLayer, targetState.GetFullPathHash(targetStateMachine), displayEvents.ToArray());
		}
	}
	
	Vector2 controllerPanelScrollPos;
	int selectedController = 0;
	AnimatorController controllerToAdd;
	
	void DrawControllerPanel() {
		
		GUILayout.BeginVertical(GUILayout.Width(200));
		
		// controller to add field.
		GUILayout.BeginHorizontal(); {
			
			controllerToAdd = EditorGUILayout.ObjectField(controllerToAdd, typeof(AnimatorController), false) as AnimatorController;
			
			EditorGUI.BeginDisabledGroup(controllerToAdd == null);
			
			if (GUILayout.Button("Add", GUILayout.ExpandWidth(true), GUILayout.Height(16))) {
				eventInspector.AddController(controllerToAdd);
			}
			
			EditorGUI.EndDisabledGroup();

			//GUILayout.Button("Del", EditorStyles.toolbarButton, GUILayout.Width(38), GUILayout.Height(16));
			
			GUILayout.Space(4);
		
		}
		GUILayout.EndHorizontal();
		
		// controller list
		
		GUILayout.BeginVertical("Box");
		controllerPanelScrollPos = GUILayout.BeginScrollView(controllerPanelScrollPos);
		
		AnimatorController[] controllers = eventInspector.GetControllers();
			
		string [] controllerNames = new string[controllers.Length];
		
		for (int i = 0; i < controllers.Length; i++) {
			
			controllerNames[i] = controllers[i].name;
			
		}
		
		selectedController = GUILayout.SelectionGrid(selectedController, controllerNames, 1);
		
		if (selectedController >= 0 && selectedController < controllers.Length) {
			
			targetController = controllers[selectedController];
			
			eventInspector.SaveLastEditController(targetController);
			
		}
		else {
			targetController = null;
			targetStateMachine = null;
			targetState = null;
			targetEvent = null;
		}
			

		GUILayout.EndScrollView();
		GUILayout.EndVertical();
		
		
		GUILayout.EndVertical();
		
	}
	
	Vector2 layerPanelScrollPos;
	int selectedLayer = 0;

	AnimatorControllerLayer[] layers;
	
	void DrawLayerPanel() {
		
		GUILayout.BeginVertical(GUILayout.Width(200));
		
		if (targetController != null) {
		
			int layerCount = targetController.layers.Length;	
			GUILayout.Label(layerCount + " layer(s) in selected controller");

			if (Event.current.type == EventType.Layout || layers == null) {
				layers = targetController.layers;
			}

			GUILayout.BeginVertical("Box");
			layerPanelScrollPos = GUILayout.BeginScrollView(layerPanelScrollPos);
			
			string[] layerNames = new string[layerCount];
			
			for (int layer = 0; layer < layerCount; layer++) {
				layerNames[layer] = "[" + layer.ToString() + "]" + layers[layer].name;
			}
			
			selectedLayer = GUILayout.SelectionGrid(selectedLayer, layerNames, 1);
			
			if (selectedLayer >= 0 && selectedLayer < layerCount) {

				if (layers[selectedLayer].syncedLayerIndex != -1)
				{
					targetStateMachine = layers[layers[selectedLayer].syncedLayerIndex].stateMachine;
				}
				else
				{
					targetStateMachine = layers[selectedLayer].stateMachine;
				}
			}
			else {
				targetStateMachine = null;
				targetState = null;
				targetEvent = null;
			}
			
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			
		}
		else {
			GUILayout.Label("No layer available.");
		}
		
		GUILayout.EndVertical();
	}
	
	Vector2 statePanelScrollPos;
	int selectedState = 0;
	
	void DrawStatePanel() {
		
		GUILayout.BeginVertical(GUILayout.Width(200));
		
		if (targetStateMachine != null) {
			
			List<AnimatorState> availableStates = GetStatesRecursive(targetStateMachine);
			List<string> stateNames = new List<string>();
			
			foreach (AnimatorState s in availableStates) {
				stateNames.Add(s.name);
			}
			
			GUILayout.Label(availableStates.Count + " state(s) in selected layer.");
			
			GUILayout.BeginVertical("Box");
			statePanelScrollPos = GUILayout.BeginScrollView(statePanelScrollPos);
			
			selectedState = GUILayout.SelectionGrid(selectedState, stateNames.ToArray(), 1);
			
			if (selectedState >= 0 && selectedState < availableStates.Count) {
				targetState = availableStates[selectedState];
			}
			else {
				targetState = null;
				targetEvent = null;
			}
			
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			
		}
		else {
			
			GUILayout.Label("No state machine available.");
		}
		
		GUILayout.EndVertical();
	}
	
	Vector2 eventPanelScrollPos;
	int selectedEvent = 0;
	
	void DrawEventPanel() {
		
		GUILayout.BeginVertical();
		
		if (targetState != null) {

			displayEvents = new List<MecanimEvent>(eventInspector.GetEvents(targetController, selectedLayer, targetState.GetFullPathHash(targetStateMachine)));
			SortEvents();
			
			GUILayout.Label(displayEvents.Count + " event(s) in this state.");
			
			List<string> eventNames = new List<string>();
			
			foreach (MecanimEvent e in displayEvents) {
				eventNames.Add(string.Format("{3}{0}({1})@{2}", e.functionName, e.parameter, e.normalizedTime.ToString("0.0000"), e.isEnable?"":"[DISABLED]"));
			}
			
			GUILayout.BeginVertical("Box");
			eventPanelScrollPos = GUILayout.BeginScrollView(eventPanelScrollPos);
			
			selectedEvent = GUILayout.SelectionGrid(selectedEvent, eventNames.ToArray(), 1);
			
			if (selectedEvent >= 0 && selectedEvent < displayEvents.Count) {
				targetEvent = displayEvents[selectedEvent];
			}
			else {
				targetEvent = null;
			}
			
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			
		}
		else {
			GUILayout.Label("No event.");
		}
		
		GUILayout.EndVertical();
	}
	
	private float playbackTime = 0.0f;
	
	private bool enableTempPreview = false;
	private float tempPreviewPlaybackTime = 0.0f;
	
	private static int timelineHash = "timelinecontrol".GetHashCode();
	
	void DrawTimelinePanel() {
		
		if (!enableTempPreview)
			playbackTime = eventInspector.GetPlaybackTime();
		
		
		GUILayout.BeginVertical(); {
			
			GUILayout.Space(10);
		
			GUILayout.BeginHorizontal(); {
				
				GUILayout.Space(20);
				
				playbackTime = Timeline(playbackTime);
				
				GUILayout.Space(10);
				
			}
			GUILayout.EndHorizontal();
			
			GUILayout.FlexibleSpace();
			
			GUILayout.BeginHorizontal(); {
				
                if (GUILayout.Button("Tools")) {
					GenericMenu menu = new GenericMenu();
					
					GenericMenu.MenuFunction2 callback = delegate(object obj) {
						int id = (int)obj;
						
						switch(id)
						{
							case 1:
							{
								stateClipboard = eventInspector.GetEvents(targetController, selectedLayer, targetState.GetFullPathHash(targetStateMachine));
								break;
							}
								
							case 2:
							{
								eventInspector.InsertEventsCopy(targetController, selectedLayer, targetState.GetFullPathHash(targetStateMachine), stateClipboard);
								break;
							}
								
							case 3:
							{
								controllerClipboard = eventInspector.GetEvents(targetController);
								break;
							}
							
							case 4:
							{
								eventInspector.InsertControllerEventsCopy(targetController, controllerClipboard);
								break;
							}
						}
					};
					
					if (targetState == null)
						menu.AddDisabledItem(new GUIContent("Copy All Events From Selected State"));
					else
						menu.AddItem(new GUIContent("Copy All Events From Selected State"), false, callback, 1);
					
					if (targetState == null || stateClipboard == null || stateClipboard.Length == 0)
						menu.AddDisabledItem(new GUIContent("Paste All Events To Selected State"));
					else
						menu.AddItem(new GUIContent("Paste All Events To Selected State"), false, callback, 2);
					
					if (targetController == null)
						menu.AddDisabledItem(new GUIContent("Copy All Events From Selected Controller"));
					else
						menu.AddItem(new GUIContent("Copy All Events From Selected Controller"), false, callback, 3);
					
					if (targetController == null || controllerClipboard == null || controllerClipboard.Count == 0)
						menu.AddDisabledItem(new GUIContent("Paste All Events To Selected Controller"));
					else
						menu.AddItem(new GUIContent("Paste All Events To Selected Controller"), false, callback, 4);
					
					
					
					menu.ShowAsContext();
				}
				
				GUILayout.FlexibleSpace();
				
				if (GUILayout.Button("Add", GUILayout.Width(80))) {
					MecanimEvent newEvent = new MecanimEvent();
					newEvent.normalizedTime = playbackTime;
					newEvent.functionName = "MessageName";
					newEvent.paramType = MecanimEventParamTypes.None;
					
					displayEvents.Add(newEvent);
					SortEvents();
					
					SetActiveEvent(newEvent);
					
					MecanimEventEditorPopup.Show(this, newEvent, GetConditionParameters());
				}
				
				if (GUILayout.Button("Del", GUILayout.Width(80))) {
					DelEvent(targetEvent);
				}
				
				EditorGUI.BeginDisabledGroup(targetEvent == null);
				
				if (GUILayout.Button("Copy", GUILayout.Width(80))) {
					clipboard = new MecanimEvent(targetEvent);
				}
				
				EditorGUI.EndDisabledGroup();
				
				EditorGUI.BeginDisabledGroup(clipboard == null);
				
				if (GUILayout.Button("Paste", GUILayout.Width(80))) {
					MecanimEvent newEvent = new MecanimEvent(clipboard);
					displayEvents.Add(newEvent);
					SortEvents();
					
					SetActiveEvent(newEvent);
				}
				
				EditorGUI.EndDisabledGroup();
				
				EditorGUI.BeginDisabledGroup(targetEvent == null);
				
				if (GUILayout.Button("Edit", GUILayout.Width(80))) {
					MecanimEventEditorPopup.Show(this, targetEvent, GetConditionParameters());
				}
				
				EditorGUI.EndDisabledGroup();
				
				if (GUILayout.Button("Save", GUILayout.Width(80))) {
					eventInspector.SaveData();
				}
				
				if (GUILayout.Button("Close", GUILayout.Width(80))) {
					Close();
				}
				
			}
			GUILayout.EndHorizontal();
		
		}
		GUILayout.EndVertical();
		
		if (enableTempPreview) {
			eventInspector.SetPlaybackTime(tempPreviewPlaybackTime);
			eventInspector.StopPlaying();
		}
		else {
			eventInspector.SetPlaybackTime(playbackTime);
		}
		
		SaveState();
	}
	
	void OnGUI() {
		if (eventInspector == null) {
			Reset();
			ShowNotification(new GUIContent("Select a MecanimEventData object first."));
			return;
		}
		
		RemoveNotification();
		
		GUILayout.BeginHorizontal(); {
			
			EditorGUI.BeginChangeCheck();
			
			DrawControllerPanel();
			
			DrawLayerPanel();
			
			DrawStatePanel();
			
			if (EditorGUI.EndChangeCheck()) {
				MecanimEventEditorPopup.Destroy();
			}
			
			DrawEventPanel();
			
		}
		GUILayout.EndHorizontal();

		if (targetState != null && targetState.motion != null) {
			eventInspector.SetPreviewMotion(targetState.motion);
		}
		else {
			eventInspector.SetPreviewMotion(null);
		}
		
		GUILayout.Space(5);
		
		GUILayout.BeginHorizontal(GUILayout.MaxHeight(100)); {
			
			DrawTimelinePanel();
			
		}
		GUILayout.EndHorizontal();
		
	}
	
	private float Timeline(float time) {
		
		Rect rect = GUILayoutUtility.GetRect(500, 10000, 50, 50);
		
		int timelineId = GUIUtility.GetControlID(timelineHash, FocusType.Passive, rect);
		
		Rect thumbRect = new Rect(rect.x + rect.width * time - 5, rect.y + 2, 10, 10);
		
		Event e = Event.current;
		
		switch(e.type) {
		case EventType.Repaint:
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
		
		if (displayEvents != null) {
		
			foreach(MecanimEvent me in displayEvents) {
				
				if (me == targetEvent)
					continue;
				
				DrawEventKey(rect, me);
			}
			
			if (targetEvent != null)
				DrawEventKey(rect, targetEvent);
			
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
	
	private void SetActiveEvent(MecanimEvent key) {
		int i =  displayEvents.IndexOf(key);
		if (i >= 0) {
			selectedEvent = i;
			targetEvent = key;
		}
	}
	
	private int hotEventKey = 0;
	
	private void DrawEventKey(Rect rect, MecanimEvent key) {
		float keyTime = key.normalizedTime;
		
		Rect keyRect = new Rect(rect.x + rect.width * keyTime - 3, rect.y+25, 6, 18);
		
		int eventKeyCtrl = key.GetHashCode();
		
		Event e = Event.current;
		
		switch(e.type) {
		case EventType.Repaint:
			Color savedColor = GUI.color;
			
			if (targetEvent == key)
				GUI.color = Color.red;
			else
				GUI.color = Color.green;
			
			GUI.skin.button.Draw(keyRect, new GUIContent(), eventKeyCtrl);
			
			GUI.color = savedColor;
			
			if (hotEventKey == eventKeyCtrl || (hotEventKey == 0 && keyRect.Contains(e.mousePosition))) {
				string labelString = string.Format("{0}({1})@{2}", key.functionName, key.parameter, key.normalizedTime.ToString("0.0000"));
				Vector2 size = EditorStyles.largeLabel.CalcSize(new GUIContent(labelString));
				
				Rect infoRect= new Rect(rect.x + rect.width * keyTime - size.x/2, rect.y + 50, size.x, size.y);
				EditorStyles.largeLabel.Draw(infoRect, new GUIContent(labelString), eventKeyCtrl);
			}
			break;
			
		case EventType.MouseDown:
			if (keyRect.Contains(e.mousePosition)) {
				
				hotEventKey = eventKeyCtrl;
				enableTempPreview =true;
				tempPreviewPlaybackTime = key.normalizedTime;
				
				SetActiveEvent(key);
				
				if (e.clickCount > 1)
					MecanimEventEditorPopup.Show(this, key, GetConditionParameters());
				
				e.Use();	
			}
			break;
			
		case EventType.MouseDrag:
			if (hotEventKey == eventKeyCtrl) {
				
				if (e.button == 0) {
					Vector2 guiPos = e.mousePosition;
					float clampedX = Mathf.Clamp(guiPos.x, rect.x, rect.x + rect.width);
					key.normalizedTime = (clampedX - rect.x) / rect.width;
					tempPreviewPlaybackTime = key.normalizedTime;
					
					SetActiveEvent(key);
				}
				
				e.Use();
			}
			break;
			
		case EventType.MouseUp:
			if (hotEventKey == eventKeyCtrl) {
				
				hotEventKey = 0;
				enableTempPreview = false;
				eventInspector.SetPlaybackTime(playbackTime);		// reset to original time
				
				if (e.button == 1)
					MecanimEventEditorPopup.Show(this, key, GetConditionParameters());
				
				e.Use();
			}
			break;
		}
	}
	
	private List<AnimatorState> GetStates(AnimatorStateMachine sm)
	{
		List<AnimatorState> stateArray = new List<AnimatorState>();
		foreach (ChildAnimatorState childState in sm.states) {
			stateArray.Add(childState.state);
		}

		return stateArray;
	}
	
	private List<AnimatorState> GetStatesRecursive(AnimatorStateMachine sm)
	{
		List<AnimatorState> list = new List<AnimatorState>();
		list.AddRange(GetStates(sm));

		foreach (ChildAnimatorStateMachine childStateMachine in sm.stateMachines) {
			list.AddRange(GetStatesRecursive(childStateMachine.stateMachine));
		}

		return list;
	}
}
