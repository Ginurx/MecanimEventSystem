using UnityEngine;
using System.Collections.Generic;

public enum MecanimEventParamTypes {
	None,
	Int32,
	Float,
	String,
	Boolean,
}

public enum EventConditionParamTypes {
	Int,
	Float,
	Boolean,
}

public enum EventConditionModes {
	Equal = 0,
	NotEqual = 1,
	GreaterThan = 2,
	LessThan = 3,
	GreaterEqualThan = 4,
	LessEqualThan = 5,
}

[System.Serializable]
public class MecanimEvent {
	public static EventContext Context { protected set; get; }
	
	public MecanimEvent() {
		condition = new EventCondition();
	}
	
	public MecanimEvent(MecanimEvent other) {
		normalizedTime = other.normalizedTime;
		functionName = other.functionName;
		paramType = other.paramType;
		
		switch(paramType) {
		case MecanimEventParamTypes.Int32:
			intParam = other.intParam;
			break;
		case MecanimEventParamTypes.Float:
			floatParam = other.floatParam;
			break;
		case MecanimEventParamTypes.String:
			stringParam = other.stringParam;
			break;
		case MecanimEventParamTypes.Boolean:
			boolParam = other.boolParam;
			break;
		}

		condition = new EventCondition();
		condition.conditions = new List<EventConditionEntry>(other.condition.conditions);
		
		critical = other.critical;

		isEnable = other.isEnable;
	}
	
	public string functionName;
	public float normalizedTime;
	public MecanimEventParamTypes paramType;
	
	public object parameter {
		get {
			switch(paramType) {
			case MecanimEventParamTypes.Int32:
				return intParam;
			case MecanimEventParamTypes.Float:
				return floatParam;
			case MecanimEventParamTypes.String:
				return stringParam;
			case MecanimEventParamTypes.Boolean:
				return boolParam;
			default:
				return null;
			}
		}
	}
	
	public int intParam;
	public float floatParam;
	public string stringParam;
	public bool boolParam;
	
	public EventCondition condition;
	public bool critical = false;

	public bool isEnable = true;

	private EventContext context;
	
	public void SetContext(EventContext context)
	{
		this.context = context;
		this.context.current = this;
	}
	
	public static void SetCurrentContext(MecanimEvent e)
	{
		MecanimEvent.Context = e.context;
	}
}

[System.Serializable]
public class EventCondition {
	public List<EventConditionEntry> conditions = new List<EventConditionEntry>();
	
	public bool Test(Animator animator) {
		if (conditions.Count == 0)
			return true;
		
		foreach(EventConditionEntry entry in conditions) {
			if (string.IsNullOrEmpty(entry.conditionParam))
				continue;
			
			switch(entry.conditionParamType) {
			case EventConditionParamTypes.Int:
				int intTestValue = animator.GetInteger(entry.conditionParam);
				switch(entry.conditionMode) {
				case EventConditionModes.Equal:
					if (intTestValue != entry.intValue)
						return false;
					
					break;
				case EventConditionModes.NotEqual:
					if (intTestValue == entry.intValue)
						return false;
					
					break;
				case EventConditionModes.GreaterThan:
					if (intTestValue <= entry.intValue)
						return false;
					
					break;
				case EventConditionModes.LessThan:
					if (intTestValue >= entry.intValue)
						return false;
					
					break;
				case EventConditionModes.GreaterEqualThan:
					if (intTestValue < entry.intValue)
						return false;
					
					break;
				case EventConditionModes.LessEqualThan:
					if (intTestValue > entry.intValue)
						return false;
					
					break;
				}
				break;
				
			case EventConditionParamTypes.Float:
				float floatTestValue = animator.GetFloat(entry.conditionParam);
				
				switch(entry.conditionMode) {
				case EventConditionModes.GreaterThan:
					if (floatTestValue <= entry.floatValue)
						return false;
					
					break;
				case EventConditionModes.LessThan:
					if (floatTestValue >= entry.floatValue)
						return false;
					
					break;
				}
				
				break;
				
			case EventConditionParamTypes.Boolean:
				bool boolTestValue = animator.GetBool(entry.conditionParam);
				
				if (boolTestValue != entry.boolValue)
					return false;
				
				break;
			}
		}
		
		return true;
	}
}

[System.Serializable]
public class EventConditionEntry {
	public string conditionParam;
	public EventConditionParamTypes conditionParamType;
	public EventConditionModes conditionMode;
	public float floatValue;
	public int intValue;
	public bool boolValue;
}

public struct EventContext {
	public int controllerId;
	public int layer;
	public int stateHash;
	public int tagHash;
	public MecanimEvent current;
}