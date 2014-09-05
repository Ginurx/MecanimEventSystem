using UnityEngine;
using UnityEditor;
//using UnityEditorInternal;
using UnityEditor.Animations;
using System.Collections;
using System;
using System.Reflection;


public class AvatarPreviewWrapper {
	#region Reflection
	
	private static Type realType;
	
	private static ConstructorInfo method_ctor;
	private static PropertyInfo property_OnAvatarChangeFunc;
	private static PropertyInfo property_IKOnFeet;
	private static PropertyInfo property_Animator;
	private static MethodInfo method_DoPreviewSettings;
	private static MethodInfo method_OnDestroy;
	private static MethodInfo method_DoAvatarPreview;
	private static MethodInfo method_ResetPreviewInstance;

//	private static MethodInfo method_CalculatePreviewGameObject;
	private static FieldInfo field_timeControl;
	
	
	public static void InitType() {
		if (realType == null) {
			Assembly assembly = Assembly.GetAssembly(typeof(Editor));
			realType = assembly.GetType("UnityEditor.AvatarPreview");
			
			method_ctor 					= realType.GetConstructor(new Type[] { typeof(Animator), typeof(Motion)});
			property_OnAvatarChangeFunc 	= realType.GetProperty("OnAvatarChangeFunc");
			property_IKOnFeet				= realType.GetProperty("IKOnFeet");
			property_Animator				= realType.GetProperty("Animator");
			method_DoPreviewSettings		= realType.GetMethod("DoPreviewSettings");
			method_OnDestroy				= realType.GetMethod("OnDestroy");
			method_DoAvatarPreview			= realType.GetMethod("DoAvatarPreview", new Type[] {typeof(Rect), typeof(GUIStyle)});
			method_ResetPreviewInstance 	= realType.GetMethod("ResetPreviewInstance");
//			method_CalculatePreviewGameObject = realType.GetMethod("CalculatePreviewGameObject", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
			field_timeControl				= realType.GetField("timeControl");
		}
	}
	
	#endregion
	
	#region Wrapper
	
	private object instance;
	
	public delegate void OnAvatarChange();
	
	public AvatarPreviewWrapper(Animator previewObjectInScene, Motion objectOnSameAsset) {
		InitType();
		
		instance = method_ctor.Invoke( new object[] { previewObjectInScene, objectOnSameAsset } );
	}
	
	public Animator Animator
	{
		get
		{
			return property_Animator.GetValue(instance, null) as Animator;
		}
	}
	public bool IKOnFeet {
		get {
			return (bool)property_IKOnFeet.GetValue(instance, null);
		}
	}
	
	public OnAvatarChange OnAvatarChangeFunc {
		set {
			property_OnAvatarChangeFunc.SetValue(instance, Delegate.CreateDelegate(property_OnAvatarChangeFunc.PropertyType, value.Target, value.Method), null);
		}
	}
	
	public void DoPreviewSettings() {
		method_DoPreviewSettings.Invoke(instance, null);
	}
	
	public void OnDestroy() {
		method_OnDestroy.Invoke(instance, null);
	}
	
	public void DoAvatarPreview(Rect rect, GUIStyle background) {
		method_DoAvatarPreview.Invoke(instance, new object[] { rect, background });
	}

	public void ResetPreviewInstance() {
		method_ResetPreviewInstance.Invoke(instance, null);
	}

//	public static GameObject CalculatePreviewGameobject (Animator selectedAnimator, Motion motion, ModelImporterAnimationType animationType) {
//		InitType();
//		return (GameObject)method_CalculatePreviewGameObject.Invoke(null, new object[] { selectedAnimator, motion, animationType });
//	}
	
	public TimeControlWrapper timeControl {
		get {
			return new TimeControlWrapper(field_timeControl.GetValue(instance));
		}
	}
	#endregion
	
}

public class TimeControlWrapper {
	private static Type realType;
	private object instance;
	
	private static FieldInfo field_currentTime;
	private static FieldInfo field_loop;
	private static FieldInfo field_startTime;
	private static FieldInfo field_stopTime;
	private static MethodInfo method_Update;
	private static PropertyInfo property_deltaTime;
	private static PropertyInfo property_normalizedTime;
	private static PropertyInfo property_playing;
	private static PropertyInfo property_nextCurrentTime;
	
	public static void InitType() {
		if (realType == null) {
			Assembly assembly = Assembly.GetAssembly(typeof(Editor));
			realType = assembly.GetType("UnityEditor.TimeControl");
	
			field_currentTime = realType.GetField("currentTime");
			field_loop = realType.GetField("loop");
			field_startTime = realType.GetField("startTime");
			field_stopTime = realType.GetField("stopTime");
			method_Update = realType.GetMethod("Update");
			property_deltaTime = realType.GetProperty("deltaTime");
			property_normalizedTime = realType.GetProperty("normalizedTime");
			property_playing = realType.GetProperty("playing");
			property_nextCurrentTime = realType.GetProperty("nextCurrentTime");
		}
	}
	
	public TimeControlWrapper(object realTimeControl) {
		InitType();
		this.instance = realTimeControl;
	}
	
	public float currentTime {
		get {
			return (float)field_currentTime.GetValue(instance);
		}
		set {
			field_currentTime.SetValue(instance, value);
		}
	}
	
	public bool loop {
		get {
			return (bool)field_loop.GetValue(instance);
		}
		set {
			field_loop.SetValue(instance, value);
		}
	}
	
	public float startTime {
		get {
			return (float)field_startTime.GetValue(instance);
		}
		set {
			field_startTime.SetValue(instance, value);
		}
	}
	
	public float stopTime {
		get {
			return (float)field_stopTime.GetValue(instance);
		}
		set {
			field_stopTime.SetValue(instance, value);
		}
	}
	
	public float deltaTime {
		get {
			return (float)property_deltaTime.GetValue(instance, null);
		}
		set {
			property_deltaTime.SetValue(instance, value, null);
		}
	}
	
	public float normalizedTime {
		get {
			return (float)property_normalizedTime.GetValue(instance, null);
		}
		set {
			property_normalizedTime.SetValue(instance, value, null);
		}
	}
	
	public bool playing {
		get {
			return (bool)property_playing.GetValue(instance, null);
		}
		set {
			property_playing.SetValue(instance, value, null);
		}
	}
	
	public float nextCurrentTime {
		set {
			property_nextCurrentTime.SetValue(instance, value, null);
		}
	}
	
	public void Update() {
		method_Update.Invoke(instance, null);
	}
}

public static class HandleUtilityWrapper {
	private static Type realType;
	private static PropertyInfo s_property_handleWireMaterial;
	
	private static void InitType() {
		if (realType == null) {
			Assembly assembly = Assembly.GetAssembly(typeof(Editor));
			realType = assembly.GetType("UnityEditor.HandleUtility");
	
			s_property_handleWireMaterial = realType.GetProperty("handleWireMaterial", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
		}
	}
	
	public static Material handleWireMaterial {
		get {
			InitType();
			return s_property_handleWireMaterial.GetValue(null, null) as Material;
		}
	}
}

public static class AnimatorExtension {
	private static Type realType;
	
	private static MethodInfo method_Update;
	
	public static void InitType() {
		if (realType == null) {
			realType = typeof(Animator);
	
			method_Update = realType.GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		}
	}
	
	
	public static void UpdateWrapper(this Animator animator, float diff) {
		InitType();
		
		method_Update.Invoke(animator, new object[] { diff });
	}
}

public class ReorderableListWrapper {
	private static Type realType;
	private static ConstructorInfo method_ctor;
	
	private static FieldInfo field_drawElementCallback;
	private static FieldInfo field_drawHeaderCallback;
	private static FieldInfo field_onAddCallback;
	private static FieldInfo field_onAddDropdownCallback;
	private static FieldInfo field_onRemoveCallback;
	private static FieldInfo field_onReorderCallback;
	private static FieldInfo field_onSelectCallback;
	
	private static FieldInfo field_elementHeight;
	private static FieldInfo field_footerHeight;
	private static FieldInfo field_headerHeight;
	
	private static PropertyInfo property_count;
	
	private static MethodInfo method_DoLayoutList;
	
	
	private object instance;
	
	public delegate void AddCallbackDelegate(object list);
	public delegate void AddDropdownCallbackDelegate(Rect buttonRect, object list);
	public delegate void ElementCallbackDelegate(Rect rect, int index, bool isActive, bool isFocused);
	public delegate void HeaderCallbackDelegate(Rect rect);
	public delegate void RemoveCallbackDelegate(object list);
	public delegate void ReorderCallbackDelegate(object list);
	public delegate void SelectCallbackDelegate(object list);
	
	public ReorderableListWrapper(object instance) {
		InitType();
		
		this.instance = instance;
	}
	
	public ReorderableListWrapper(IList elements, Type elementType) {
		InitType();
		
		instance = method_ctor.Invoke(new object[] {elements, elementType});
	}
	
	public static void InitType() {
		if (realType == null) {
			Assembly assembly = Assembly.GetAssembly(typeof(Editor));
			realType = assembly.GetType("UnityEditorInternal.ReorderableList");
			
			method_ctor = realType.GetConstructor(new Type[] { typeof(IList), typeof(Type)});
			
			field_drawElementCallback = realType.GetField("drawElementCallback");
			field_drawHeaderCallback = realType.GetField("drawHeaderCallback");
			field_onAddCallback = realType.GetField("onAddCallback");
			field_onAddDropdownCallback = realType.GetField("onAddDropdownCallback");
			field_onRemoveCallback = realType.GetField("onRemoveCallback");
			field_onReorderCallback = realType.GetField("onReorderCallback");
			field_onSelectCallback = realType.GetField("onSelectCallback");
			
			field_elementHeight = realType.GetField("elementHeight");
			field_footerHeight = realType.GetField("footerHeight");
			field_headerHeight = realType.GetField("headerHeight");
			
			property_count = realType.GetProperty("count");
			
			method_DoLayoutList = realType.GetMethod("DoLayoutList");
		}
	}
	
	public ElementCallbackDelegate drawElementCallback {
		set {
			field_drawElementCallback.SetValue(instance, Delegate.CreateDelegate(field_drawElementCallback.FieldType, value.Target, value.Method));
		}
	}
	
	public HeaderCallbackDelegate drawHeaderCallback {
		set {
			field_drawHeaderCallback.SetValue(instance, Delegate.CreateDelegate(field_drawHeaderCallback.FieldType, value.Target, value.Method));
		}
	}
	
	public AddCallbackDelegate onAddCallback {
		set {
			field_onAddCallback.SetValue(instance, Delegate.CreateDelegate(field_onAddCallback.FieldType, value.Target, value.Method));
		}
	}
	
	public AddDropdownCallbackDelegate onAddDropdownCallback {
		set {
			field_onAddDropdownCallback.SetValue(instance, Delegate.CreateDelegate(field_onAddDropdownCallback.FieldType, value.Target, value.Method));
		}
	}
	
	public RemoveCallbackDelegate onRemoveCallback {
		set {
			field_onRemoveCallback.SetValue(instance, Delegate.CreateDelegate(field_onRemoveCallback.FieldType, value.Target, value.Method));
		}
	}
	
	public ReorderCallbackDelegate onReorderCallback {
		set {
			field_onReorderCallback.SetValue(instance, Delegate.CreateDelegate(field_onReorderCallback.FieldType, value.Target, value.Method));
		}
	}
	
	public SelectCallbackDelegate onSelectCallback {
		set {
			field_onSelectCallback.SetValue(instance, Delegate.CreateDelegate(field_onSelectCallback.FieldType, value.Target, value.Method));
		}
	}
	
	public float elementHeight {
		get {
			return (float)field_elementHeight.GetValue(instance);
		}
	}
	
	public float footerHeight {
		get {
			return (float)field_footerHeight.GetValue(instance);
		}
	}
	
	public float headerHeight {
		get {
			return (float)field_headerHeight.GetValue(instance);
		}
	}
	
	public int count {
		get {
			return (int)property_count.GetValue(instance, null);
		}
	}
	
	public void DoLayoutList() {
		method_DoLayoutList.Invoke(instance, null);
	}
}

public static class BlendTreeExtension {
	
	public static int GetRecursiveBlendParamCount(this BlendTree bt) {
		object val = bt.GetType().GetProperty("recursiveBlendParameterCount", BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public).GetValue(bt, new object[]{});
		return (int)val;
	}
	public static string GetRecursiveBlendParam(this BlendTree bt, int index) {
		object val = bt.GetType().GetMethod("GetRecursiveBlendParameter", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Invoke(bt, new object[]{index});
		return (string)val;
	}
	public static float GetRecursiveBlendParamMax(this BlendTree bt, int index) {
		object val = bt.GetType().GetMethod("GetRecursiveBlendParameterMax", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Invoke(bt, new object[]{index});
		return (float)val;
	}
	public static float GetRecursiveBlendParamMin(this BlendTree bt, int index) {
		object val = bt.GetType().GetMethod("GetRecursiveBlendParameterMin", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Invoke(bt, new object[]{index});
		return (float)val;
	}
	public static float GetInputBlendVal(this BlendTree bt, string blendValueName) {
		object val = bt.GetType().GetMethod("GetInputBlendValue", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Invoke(bt, new object[] { blendValueName });
		return (float)val;
	}
}

public static class AnimatorControllerExtension {

	private static Type realType;
	private static MethodInfo method_GetEffectiveAnimatorController;
	private static FieldInfo field_OnAnimatorControllerDirty;

	public static void InitType() {
		if (realType == null) {
			realType = typeof(AnimatorController);

			method_GetEffectiveAnimatorController = realType.GetMethod("GetEffectiveAnimatorController", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
			field_OnAnimatorControllerDirty = realType.GetField("OnAnimatorControllerDirty", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
		}
	}

	public static AnimatorController GetEffectiveAnimatorController(Animator animator) {
		InitType();
		object val = (AnimatorController)(method_GetEffectiveAnimatorController.Invoke(null, new object[] { animator }));
		return (AnimatorController)val;
	}

	public static void AppendOnAnimatorControllerDirtyCallback(this AnimatorController controller, System.Action callback) {
		InitType();
		System.Action oldCallback = (System.Action)field_OnAnimatorControllerDirty.GetValue(controller);
		System.Action newCallback = (System.Action)Delegate.Combine(oldCallback, new System.Action(callback));

		field_OnAnimatorControllerDirty.SetValue(controller, newCallback);
	}

	public static void RemoveOnAnimatorControllerDirtyCallback(this AnimatorController controller, System.Action callback) {
		InitType();
		System.Action oldCallback = (System.Action)field_OnAnimatorControllerDirty.GetValue(controller);
		System.Action newCallback = (System.Action)Delegate.Remove(oldCallback, new System.Action(callback));

		field_OnAnimatorControllerDirty.SetValue(controller, newCallback);
	}
}

public static class AnimatorStateMachineExtension {

	private static Type realType;
	private static MethodInfo method_GetStatePath;

	public static void InitType() {
		if (realType == null) {
			realType = typeof(AnimatorController);

			method_GetStatePath = typeof(AnimatorStateMachine).GetMethod("GetStatePath", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
		}
	}

	public static string GetStatePathWrapper(this AnimatorStateMachine stateMachine, AnimatorState state) {
		InitType();
		object val = method_GetStatePath.Invoke(stateMachine, new object[] { state });
		return (string)val;
	}
}

public static class AnimatorStateExtension {

	public static int GetFullPathHash(this AnimatorState state, AnimatorStateMachine parentSM)
	{
		string fullpath = parentSM.GetStatePathWrapper(state);
		return Animator.StringToHash(fullpath);
	}

}