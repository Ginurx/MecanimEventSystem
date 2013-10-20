using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MecanimEventDataTransfer : EditorWindow {
	
	private MecanimEventData transferFrom;
	private MecanimEventData transferTo;
	
	private Dictionary<UnityEngine.Object, bool> toggleTable = new Dictionary<UnityEngine.Object, bool>();

	
	private Vector2 leftWindowScroll;
	private Vector2 rightWindowScroll;
	
	class DisplayInfo {
		public int eventCount = 0;
	}
	
	[MenuItem("Window/Mecanim Event Data Transfer")]
	public static void Init() {
		EditorWindow.GetWindow<MecanimEventDataTransfer>();
	}
	
	void OnGUI() {
		
		EditorGUILayout.BeginHorizontal(); {
		
			EditorGUI.BeginChangeCheck();
			
			transferFrom = EditorGUILayout.ObjectField("Transfer From", transferFrom, typeof(MecanimEventData), false) as MecanimEventData;
			
			if (EditorGUI.EndChangeCheck()) {
				toggleTable = new Dictionary<Object, bool>();
			}
			
			transferTo = EditorGUILayout.ObjectField("Transfer To", transferTo, typeof(MecanimEventData), false) as MecanimEventData;
			
		}
		EditorGUILayout.EndHorizontal();
	
		
		EditorGUILayout.BeginHorizontal(); {
			
			EditorGUILayout.BeginVertical("Window", GUILayout.MaxWidth(position.width/2));
			leftWindowScroll = EditorGUILayout.BeginScrollView(leftWindowScroll); {
			
				DisplayDataSource();
				
			}
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();	
			
			EditorGUILayout.BeginVertical("Window", GUILayout.MaxWidth(position.width/2));
			rightWindowScroll = EditorGUILayout.BeginScrollView(rightWindowScroll); {
			
				DisplayDataDest();
				
			}
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();	
			
		}
		EditorGUILayout.EndHorizontal();
		
		
		EditorGUILayout.BeginHorizontal(); {
			
			GUILayout.FlexibleSpace();
			
			EditorGUI.BeginDisabledGroup(transferFrom == null || transferTo == null || 
											transferFrom.GetInstanceID() == transferTo.GetInstanceID());
			
			if (GUILayout.Button("Transfer", GUILayout.Width(80), GUILayout.Height(30))) {
				Transfer();
			}
			
			EditorGUI.EndDisabledGroup();
			
			if (GUILayout.Button("Cancel", GUILayout.Width(80), GUILayout.Height(30))) {
				Close();
			}
		}
		EditorGUILayout.EndHorizontal();
	}
	
	void DisplayDataSource() {
		
		if (transferFrom == null)
			return;

		MecanimEventDataEntry[] data = transferFrom.data;
		
		if (data == null)
			return;
		
		Dictionary<UnityEngine.Object, DisplayInfo> controllers = new Dictionary<UnityEngine.Object, DisplayInfo>();
		
		foreach (MecanimEventDataEntry entry in data) {
			
			if (entry == null || entry.animatorController == null)
				continue;
			
			if (!controllers.ContainsKey(entry.animatorController)) {
				controllers[entry.animatorController] = new DisplayInfo();
			}
			
			if (entry.events != null)
				controllers[entry.animatorController].eventCount += entry.events.Length;
			
		}
		
		EditorGUILayout.BeginVertical();
		
		
		foreach (UnityEngine.Object controller in controllers.Keys) {
			
			if (!toggleTable.ContainsKey(controller)) {
				toggleTable[controller] = false;
			}
			
			EditorGUILayout.BeginHorizontal();
			
			toggleTable[controller] = GUILayout.Toggle(toggleTable[controller], controller.name, GUILayout.ExpandWidth(true));
			
			GUILayout.Label("("+controllers[controller].eventCount+")", GUILayout.ExpandWidth(false));
			
			EditorGUILayout.EndHorizontal();
			
		}
		
		EditorGUILayout.EndVertical();
	}
	
	void DisplayDataDest() {
		
		if (transferTo == null)
			return;
		
		MecanimEventDataEntry[] data = transferTo.data;
		
		if (data == null)
			return;
		
		Dictionary<UnityEngine.Object, DisplayInfo> controllers = new Dictionary<UnityEngine.Object, DisplayInfo>();
		
		foreach (MecanimEventDataEntry entry in data) {
			if (entry == null || entry.animatorController == null)
				continue;
			
			if (!controllers.ContainsKey(entry.animatorController)) {
				controllers[entry.animatorController] = new DisplayInfo();
			}
			
			if (entry.events != null)
				controllers[entry.animatorController].eventCount += entry.events.Length;
			
		}
		
		EditorGUILayout.BeginVertical();
		
		
		foreach (UnityEngine.Object controller in controllers.Keys) {
			
			if (!toggleTable.ContainsKey(controller)) {
				toggleTable[controller] = false;
			}
			
			EditorGUILayout.BeginHorizontal();
			
			if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false))) {
				DeleteEntry(controller);
			}
			
			GUILayout.Label(controller.name, GUILayout.ExpandWidth(true));
			
			GUILayout.Label("("+controllers[controller].eventCount+")", GUILayout.ExpandWidth(false));
			
			EditorGUILayout.EndHorizontal();
			
		}
		
		EditorGUILayout.EndVertical();
	}
	
	void Transfer() {
		
		if (transferFrom.data == null || transferFrom.data.Length == 0)
			return;
		
		bool toggleAny = false;
		foreach (bool f in toggleTable.Values) {
			if (f) {
				toggleAny = true;
				break;
			}
		}
		if (!toggleAny)
			return;
		
		
		
		
		if (transferTo.data == null)
			transferTo.data = new MecanimEventDataEntry[0];
		
		List<MecanimEventDataEntry> destEntries = new List<MecanimEventDataEntry>(transferTo.data);
		
		for (int i = 0; i < destEntries.Count; ) {
			
			if (toggleTable.ContainsKey(destEntries[i].animatorController) &&
				toggleTable[destEntries[i].animatorController] == true) {
				
				destEntries.RemoveAt(i);
				
			}
			else {
				
				i++;
				
			}
		}
		
		foreach (MecanimEventDataEntry srcEntry in transferFrom.data) {
			
			if (toggleTable.ContainsKey(srcEntry.animatorController) &&
				toggleTable[srcEntry.animatorController] == true) {
				
				// copy this entry;
				destEntries.Add(new MecanimEventDataEntry(srcEntry));
			}
			
		}
		
		transferTo.data = destEntries.ToArray();
		
		EditorUtility.SetDirty(transferTo);
		
		Repaint();
	}
	
	void DeleteEntry(UnityEngine.Object controller) {
	
		Undo.RecordObject(transferTo, "Mecanim Event Data");
		
		if (transferTo.data == null)
			transferTo.data = new MecanimEventDataEntry[0];
		
		List<MecanimEventDataEntry> destEntries = new List<MecanimEventDataEntry>(transferTo.data);
		
		for (int i = 0; i < destEntries.Count; ) {
			
			if (destEntries[i] != null && destEntries[i].animatorController == controller) {
				
				destEntries.RemoveAt(i);
				
			}
			else {
				
				i++;
				
			}
		}
		
		transferTo.data = destEntries.ToArray();
		
		EditorUtility.SetDirty(transferTo);
		Repaint();
	}
}
