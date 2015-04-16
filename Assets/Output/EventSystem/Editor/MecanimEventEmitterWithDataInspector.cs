using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
//using UnityEditorInternal;
using System.Collections.Generic;

[CustomEditor(typeof(MecanimEventEmitterWithData))]
public class MecanimEventEmitterWithDataInspector : Editor {
	SerializedProperty controller;
	SerializedProperty animator;
	SerializedProperty emitType;
	SerializedProperty data;
	
	void OnEnable() {
		controller = serializedObject.FindProperty("animatorController");
		animator = serializedObject.FindProperty("animator");
		emitType = serializedObject.FindProperty("emitType");
		data = serializedObject.FindProperty("data");
	}
	
	public override void OnInspectorGUI ()
	{
		serializedObject.UpdateIfDirtyOrScript();
		
		EditorGUILayout.PropertyField(animator);
		
		if (animator.objectReferenceValue != null) {
			AnimatorController animatorController = AnimatorControllerExtension.GetEffectiveAnimatorController((Animator)animator.objectReferenceValue);
			controller.objectReferenceValue = animatorController;
		}
		else {
			controller.objectReferenceValue = null;
		}
		
		EditorGUILayout.ObjectField("AnimatorController", controller.objectReferenceValue, typeof(AnimatorController), false);
		
		EditorGUILayout.PropertyField(emitType);
		
		EditorGUILayout.PropertyField(data);

		serializedObject.ApplyModifiedProperties();
	}
}