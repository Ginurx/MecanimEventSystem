using UnityEngine;
using System.Collections.Generic;

public static class MecanimEventManager {
	private static MecanimEventData[] eventDataSources;
	
	private static Dictionary<int, Dictionary<int, Dictionary<int, List<MecanimEvent>>>> globalLoadedData;
	
	private static Dictionary<int, Dictionary<int, AnimatorStateInfo>> globalLastStates = new Dictionary<int, Dictionary<int, AnimatorStateInfo>>();
	
	
	public static void SetEventDataSource(MecanimEventData dataSource) {
		if (dataSource != null) {
			eventDataSources = new MecanimEventData[1];
			eventDataSources[0] = dataSource;
			
			LoadGlobalData();
		}
	}
	
	public static void SetEventDataSource(MecanimEventData[] dataSources) {
		if (dataSources != null) {
			
			eventDataSources = dataSources;
			
			LoadGlobalData();
		}
	}
	
	public static void OnLevelLoaded() {
		globalLastStates.Clear();
	}
	
	public static MecanimEvent[] GetEvents(int animatorControllerId, Animator animator) {
		return GetEvents(MecanimEventManager.globalLoadedData, MecanimEventManager.globalLastStates, animatorControllerId, animator);
	}

	public static MecanimEvent[] GetEvents(Dictionary<int, Dictionary<int, Dictionary<int, List<MecanimEvent>>>> contextLoadedData,
	                                       Dictionary<int, Dictionary<int, AnimatorStateInfo>> contextLastStates,
	                                       int animatorControllerId, Animator animator)
	{
		List<MecanimEvent> allEvents = new List<MecanimEvent>();
		
		int animatorHash = animator.GetHashCode();
		if (!contextLastStates.ContainsKey(animatorHash))
			contextLastStates[animatorHash] = new Dictionary<int, AnimatorStateInfo>();
		
		int layerCount = animator.layerCount;
		
		Dictionary<int, AnimatorStateInfo> lastLayerState = contextLastStates[animatorHash];
		
		for (int layer = 0; layer < layerCount; layer++) {
			if (!lastLayerState.ContainsKey(layer)) {
				lastLayerState[layer] = new AnimatorStateInfo();
			}
			
			AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
			
			int lastLoop = (int)lastLayerState[layer].normalizedTime;
			int currLoop = (int)stateInfo.normalizedTime;
			float lastNormalizedTime = lastLayerState[layer].normalizedTime - lastLoop;
			float currNormalizedTime = stateInfo.normalizedTime - currLoop;
			
			if (lastLayerState[layer].fullPathHash == stateInfo.fullPathHash) {
				if (stateInfo.loop == true) {
					if (lastLoop == currLoop) {
						allEvents.AddRange(CollectEvents(contextLoadedData, animator, animatorControllerId, layer, stateInfo.fullPathHash, stateInfo.tagHash, lastNormalizedTime, currNormalizedTime));
					}
					else {
						allEvents.AddRange(CollectEvents(contextLoadedData, animator, animatorControllerId, layer, stateInfo.fullPathHash, stateInfo.tagHash, lastNormalizedTime, 1.00001f));
						allEvents.AddRange(CollectEvents(contextLoadedData, animator, animatorControllerId, layer, stateInfo.fullPathHash, stateInfo.tagHash, 0.0f, currNormalizedTime));
					}
				}
				else {
					float start = Mathf.Clamp01(lastLayerState[layer].normalizedTime);
					float end = Mathf.Clamp01(stateInfo.normalizedTime);
					
					if (lastLoop == 0 && currLoop == 0) {
						if (start != end)
							allEvents.AddRange(CollectEvents(contextLoadedData, animator, animatorControllerId, layer, stateInfo.fullPathHash, stateInfo.tagHash, start, end));
					}
					else if (lastLoop == 0 && currLoop > 0) {
						allEvents.AddRange(CollectEvents(contextLoadedData, animator, animatorControllerId, layer, lastLayerState[layer].fullPathHash, lastLayerState[layer].tagHash, start, 1.00001f));
					}
					else {
						
					}
				}
			}
			else {
				
				allEvents.AddRange(CollectEvents(contextLoadedData, animator, animatorControllerId, layer, stateInfo.fullPathHash, stateInfo.tagHash, 0.0f, currNormalizedTime));
				
				if (!lastLayerState[layer].loop) {
					allEvents.AddRange(CollectEvents(contextLoadedData, animator, animatorControllerId, layer, lastLayerState[layer].fullPathHash, lastLayerState[layer].tagHash, lastNormalizedTime, 1.00001f, true));
				}
			}
			
			lastLayerState[layer] = stateInfo;
		}
		
		return allEvents.ToArray();
	}
	
	private static MecanimEvent[] CollectEvents(Dictionary<int, Dictionary<int, Dictionary<int, List<MecanimEvent>>>> contextLoadedData,
	                                            Animator animator, 
	                                            int animatorControllerId, 
	                                            int layer, int nameHash, 
	                                            int tagHash, 
	                                            float normalizedTimeStart, 
	                                            float normalizedTimeEnd, 
	                                            bool onlyCritical = false) {
		List<MecanimEvent> events;
		
		if (contextLoadedData.ContainsKey(animatorControllerId) &&
		    contextLoadedData[animatorControllerId].ContainsKey(layer) && 
		    contextLoadedData[animatorControllerId][layer].ContainsKey(nameHash)) {
			
			events = contextLoadedData[animatorControllerId][layer][nameHash];
		}
		else {
			return new MecanimEvent[0];
		}
		
		List<MecanimEvent> ret = new List<MecanimEvent>();
		
		foreach (MecanimEvent e in events) {

			if (!e.isEnable)
				continue;
			
			if (e.normalizedTime >= normalizedTimeStart && e.normalizedTime < normalizedTimeEnd) {
				if (e.condition.Test(animator)) {
					
					if (onlyCritical && !e.critical)
						continue;
					
					MecanimEvent finalEvent = new MecanimEvent(e);
					EventContext context = new EventContext();
					context.controllerId = animatorControllerId;
					context.layer = layer;
					context.stateHash = nameHash;
					context.tagHash = tagHash;
					
					finalEvent.SetContext(context);
					
					ret.Add(finalEvent);
				}
			}
		}
		
		return ret.ToArray();
	}
	
	private static void LoadGlobalData() {
		
		if (eventDataSources == null)
			return;
		
		globalLoadedData = new Dictionary<int, Dictionary<int, Dictionary<int, List<MecanimEvent>>>>();
		
		foreach (MecanimEventData dataSource in eventDataSources) {
		
			if (dataSource == null)
				continue;
			
			MecanimEventDataEntry[] entries = dataSource.data;
			
			foreach(MecanimEventDataEntry entry in entries) {
				int animatorControllerId = entry.animatorController.GetInstanceID();
				
				if (!globalLoadedData.ContainsKey(animatorControllerId))
					globalLoadedData[animatorControllerId] = new Dictionary<int, Dictionary<int, List<MecanimEvent>>>();
				
				if (!globalLoadedData[animatorControllerId].ContainsKey(entry.layer)) {
					globalLoadedData[animatorControllerId][entry.layer] = new Dictionary<int, List<MecanimEvent>>();
				}
				
				globalLoadedData[animatorControllerId][entry.layer][entry.stateNameHash] = new List<MecanimEvent>(entry.events);
			}
			
		}
	}

	public static Dictionary<int, Dictionary<int, Dictionary<int, List<MecanimEvent>>>> LoadData(MecanimEventData data)
	{
		Dictionary<int, Dictionary<int, Dictionary<int, List<MecanimEvent>>>> retData = new Dictionary<int, Dictionary<int, Dictionary<int, List<MecanimEvent>>>>();
		
		MecanimEventDataEntry[] entries = data.data;
		
		foreach(MecanimEventDataEntry entry in entries) {
			int animatorControllerId = entry.animatorController.GetInstanceID();
			
			if (!retData.ContainsKey(animatorControllerId))
				retData[animatorControllerId] = new Dictionary<int, Dictionary<int, List<MecanimEvent>>>();
			
			if (!retData[animatorControllerId].ContainsKey(entry.layer)) {
				retData[animatorControllerId][entry.layer] = new Dictionary<int, List<MecanimEvent>>();
			}
			
			retData[animatorControllerId][entry.layer][entry.stateNameHash] = new List<MecanimEvent>(entry.events);
		}

		return retData;
	}
	
}
