using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;

[CustomEditor(typeof(MecanimEventEmitter))]
public class MecanimEventEmitterInspector : Editor {
	SerializedProperty controller;
	SerializedProperty animator;
	SerializedProperty emitType;
	
	void OnEnable() {
		controller = serializedObject.FindProperty("animatorController");
		animator = serializedObject.FindProperty("animator");
		emitType = serializedObject.FindProperty("emitType");
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

		serializedObject.ApplyModifiedProperties();
	}
}