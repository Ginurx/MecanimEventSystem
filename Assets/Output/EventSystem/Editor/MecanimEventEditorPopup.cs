using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MecanimEventEditorPopup : EditorWindow {
	private static MecanimEventEditorPopup actived;
	
	private MecanimEvent eventTemp;
	private UnityEditorInternal.ReorderableList conditionList;
	private MecanimEvent eventEditing;
	private MecanimEventEditor editor;
	private KeyValuePair<string, EventConditionParamTypes>[] availableParameters;
	
	private GUIContent[] booleanPopup = new GUIContent[] {new GUIContent("False"), new GUIContent("True")};
	
	public static MecanimEventEditorPopup Show(MecanimEventEditor editor, MecanimEvent e, KeyValuePair<string, EventConditionParamTypes>[] availableParameters) {
		actived = EditorWindow.GetWindow<MecanimEventEditorPopup>(false, "Edit Event");
		actived.eventEditing = e;

		actived.eventTemp = new MecanimEvent(e);
		actived.editor = editor;
		actived.availableParameters = availableParameters;
		actived.conditionList = new UnityEditorInternal.ReorderableList(actived.eventTemp.condition.conditions, typeof(EventConditionEntry));
		actived.conditionList.drawElementCallback = new UnityEditorInternal.ReorderableList.ElementCallbackDelegate(actived.DrawConditionsElement);
		actived.conditionList.drawHeaderCallback = new UnityEditorInternal.ReorderableList.HeaderCallbackDelegate(actived.DrawConditionsHeader);
		return actived;
	}
	
	public static void Destroy() {
		if (actived != null) {
			actived.Close();
		}
	}
	
	void OnEnable() {
		minSize = new Vector2(450, 190);
	}
	
	void OnDestroy() {
		actived = null;
	}
	
	void Update() {
		minSize = new Vector2(minSize.x, 160 + conditionList.count * conditionList.elementHeight);
	}
	
	void OnGUI() {
		EditorGUI.BeginDisabledGroup(!eventTemp.isEnable); {

			EditorGUILayout.BeginHorizontal();
			eventTemp.normalizedTime = Mathf.Clamp(EditorGUILayout.FloatField("Normalized Time", eventTemp.normalizedTime), 0.0f, 1.0f);
			
			if (GUILayout.Button("Current", GUILayout.MaxWidth(60))) {
				eventTemp.normalizedTime = editor.PlaybackTime;
			}
			
			EditorGUILayout.EndHorizontal();
			
			eventTemp.functionName = EditorGUILayout.TextField("Message", eventTemp.functionName);
			eventTemp.paramType = (MecanimEventParamTypes)EditorGUILayout.EnumPopup("Parameter Type", eventTemp.paramType);
			
			switch(eventTemp.paramType) {
			case MecanimEventParamTypes.Int32:
				eventTemp.intParam = EditorGUILayout.IntField("Parameter", eventTemp.intParam);
				break;
			case MecanimEventParamTypes.Float:
				eventTemp.floatParam = EditorGUILayout.FloatField("Parameter", eventTemp.floatParam);
				break;
			case MecanimEventParamTypes.String:
				eventTemp.stringParam = EditorGUILayout.TextField("Parameter", eventTemp.stringParam);
				break;
			case MecanimEventParamTypes.Boolean:
				eventTemp.boolParam = EditorGUILayout.Popup(new GUIContent("Parameter"), eventTemp.boolParam == true ? 1 : 0, booleanPopup) == 1 ? true : false;
				break;
			}
			
			GUIContent toggleLabel = new GUIContent("Critical", "A critical event won't be missed even state was interrupted.");
			eventTemp.critical = EditorGUILayout.Toggle(toggleLabel, eventTemp.critical);
			
			if (availableParameters.Length > 0)
				conditionList.DoLayoutList();
			else
				eventTemp.condition.conditions.Clear();
			
			GUILayout.Space(10);
			GUILayout.FlexibleSpace();
		
		}
		EditorGUI.EndDisabledGroup();

		GUILayout.BeginHorizontal();
		
		if (GUILayout.Button("Copy", GUILayout.MinWidth(60))) {
			MecanimEventEditor.clipboard = new MecanimEvent(eventTemp);
		}
		
		EditorGUI.BeginDisabledGroup(MecanimEventEditor.clipboard == null);
		
		if (GUILayout.Button("Paste", GUILayout.MinWidth(60))) {
			eventTemp = new MecanimEvent(MecanimEventEditor.clipboard);
		}
		
		EditorGUI.EndDisabledGroup();

		if (GUILayout.Button(eventTemp.isEnable?"Disable":"Enable")) {
			eventTemp.isEnable = !eventTemp.isEnable;
		}
		
		GUILayout.FlexibleSpace();
		GUILayout.Space(20);
		
		if (GUILayout.Button("Save", GUILayout.MinWidth(80))) {
			eventEditing.normalizedTime = eventTemp.normalizedTime;
			eventEditing.functionName = eventTemp.functionName;
			eventEditing.paramType = eventTemp.paramType;
			eventEditing.intParam = eventTemp.intParam;
			eventEditing.floatParam = eventTemp.floatParam;
			eventEditing.stringParam = eventTemp.stringParam;
			eventEditing.boolParam = eventTemp.boolParam;
			eventEditing.condition = eventTemp.condition;
			eventEditing.critical = eventTemp.critical;
			eventEditing.isEnable = eventTemp.isEnable;
			Close();
		}
		
//		GUILayout.Space(20);
//		
//		if (GUILayout.Button("Delete", GUILayout.MinWidth(80))) {
//			editor.DelEvent(eventEditing);
//			Close();
//		}
		
		GUILayout.Space(20);
		
		if (GUILayout.Button("Cancel", GUILayout.MinWidth(80))) {
			Close();
		}
		GUILayout.Space(20);
		
		GUILayout.EndHorizontal();
		GUILayout.Space(10);
	}
	
	private void DrawConditionsElement(Rect rect, int index, bool selected, bool focused) {
		EventConditionEntry conditionAtIndex = eventTemp.condition.conditions[index];
		EditorGUIUtility.LookLikeControls();
		Rect paramRect = new Rect(rect.x, rect.y, rect.width/3, rect.height);
		
		string[] paramPopup = new string[availableParameters.Length];
		int paramSelected = 0;
		
		for (int i = 0; i < availableParameters.Length; i++) {
			paramPopup[i] = availableParameters[i].Key;
			
			if (paramPopup[i] == conditionAtIndex.conditionParam)
				paramSelected = i;
		}
		
		paramSelected = EditorGUI.Popup(paramRect, paramSelected, paramPopup);
		conditionAtIndex.conditionParam = paramPopup[paramSelected];
		
		switch(availableParameters[paramSelected].Value) {
		case EventConditionParamTypes.Int:
			{
				conditionAtIndex.conditionParamType = EventConditionParamTypes.Int;
				Rect modeRect = new Rect(rect.x+rect.width/3, rect.y, rect.width/3, rect.height);
				Rect valueRect = new Rect(rect.x+rect.width*2/3, rect.y, rect.width/3, rect.height-4);
				
				conditionAtIndex.conditionMode = (EventConditionModes)EditorGUI.EnumPopup(modeRect, conditionAtIndex.conditionMode);
				conditionAtIndex.intValue = EditorGUI.IntField(valueRect, conditionAtIndex.intValue);
			}
			
			break;
		case EventConditionParamTypes.Float:
			{	
				conditionAtIndex.conditionParamType = EventConditionParamTypes.Float;
				Rect modeRect = new Rect(rect.x+rect.width/3, rect.y, rect.width/3, rect.height);
				Rect valueRect = new Rect(rect.x+rect.width*2/3, rect.y, rect.width/3, rect.height-4);
			
				string[] floatConditionMode = new string[] { EventConditionModes.GreaterThan.ToString(), EventConditionModes.LessThan.ToString() };
				int selectMode = conditionAtIndex.conditionMode == EventConditionModes.LessThan ? 1 : 0;
				selectMode = EditorGUI.Popup(modeRect, selectMode, floatConditionMode);
				conditionAtIndex.conditionMode = selectMode == 0 ? EventConditionModes.GreaterThan : EventConditionModes.LessThan;
				conditionAtIndex.floatValue = EditorGUI.FloatField(valueRect, conditionAtIndex.floatValue);
			}
			break;
		case EventConditionParamTypes.Boolean:
			{
				conditionAtIndex.conditionParamType = EventConditionParamTypes.Boolean;
				Rect valueRect = new Rect(rect.x+rect.width/3, rect.y, rect.width*2/3, rect.height-4);
				
				string[] boolConditionValue = new string[] { "true", "false" };
				conditionAtIndex.conditionMode = EventConditionModes.Equal;
				int selectedValue = conditionAtIndex.boolValue ? 0 : 1;
				conditionAtIndex.boolValue = EditorGUI.Popup(valueRect, selectedValue, boolConditionValue) == 0 ? true : false;
			}
			break;
		}
	}
	
	private void DrawConditionsHeader(Rect headerRect)
	{
		EditorGUIUtility.LookLikeControls();
		GUI.Label(headerRect, new GUIContent("Conditions"));
	}
}
