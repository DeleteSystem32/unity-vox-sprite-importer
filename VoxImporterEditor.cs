using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;

[CustomEditor(typeof(VoxImporter))]
public class VoxImporterEditor : ScriptedImporterEditor {

	public override void OnInspectorGUI(){
		
		SerializedProperty importModeProperty = serializedObject.FindProperty("importMode");
		importModeProperty.intValue = EditorGUILayout.Popup("Import Mode", importModeProperty.intValue, Enum.GetNames(typeof(VoxImporter.ImportMode)));

		VoxImporter.ImportMode importMode = (VoxImporter.ImportMode)importModeProperty.intValue;

		// get all properties
		SerializedProperty ppu = serializedObject.FindProperty("pixelsPerUnit");
		SerializedProperty animated = serializedObject.FindProperty("animated");
		SerializedProperty generateShadow = serializedObject.FindProperty("generateShadow");
		SerializedProperty shadowColor = serializedObject.FindProperty("shadowColor");
		SerializedProperty atlasSize = serializedObject.FindProperty("atlasSize");
		SerializedProperty useOutline = serializedObject.FindProperty("useOutline");
		SerializedProperty outlineSize = serializedObject.FindProperty("outlineSize");
		SerializedProperty outlineColor = serializedObject.FindProperty("outlineColor");
		SerializedProperty layeredShader = serializedObject.FindProperty("layeredShader");
		SerializedProperty shadowAngle = serializedObject.FindProperty("shadowAngle");
		SerializedProperty startingLayer = serializedObject.FindProperty("startingLayer");

		// gui elements for common properties
		EditorGUILayout.PropertyField(animated, new GUIContent("Is Animated"));

		if(importMode == VoxImporter.ImportMode.Sliced){
			EditorGUILayout.PropertyField(layeredShader, new GUIContent("Layered Shader (unimplemented)"));
		}		

		// gui elements for individual properties
		switch(importMode){
			case VoxImporter.ImportMode.Sliced:
			case VoxImporter.ImportMode.FixedSprite:
				/* to show:
				pixels per unit
				animated
				generate shadow
				shadow color
				atlas size 
				*/
				EditorGUILayout.PropertyField(atlasSize, new GUIContent("Atlas Size"));
				EditorGUILayout.PropertyField(ppu, new GUIContent("Pixels per Unit"));
				EditorGUILayout.PropertyField(generateShadow, new GUIContent("Generate Shadow"));
				EditorGUILayout.PropertyField(startingLayer, new GUIContent("Starting Layer"));

				if(generateShadow.boolValue){
					EditorGUILayout.PropertyField(shadowColor, new GUIContent("Shadow Color"));
					EditorGUILayout.PropertyField(shadowAngle, new GUIContent("Shadow Angle (unimplemented)"));
				}		
						
				EditorGUILayout.PropertyField(useOutline, new GUIContent("Use Outline (unimplemented)"));
				if(useOutline.boolValue){
					EditorGUILayout.PropertyField(outlineSize, new GUIContent("Outline Width"));
					EditorGUILayout.PropertyField(outlineColor, new GUIContent("Outline Color"));
				}
				break;

			case VoxImporter.ImportMode.Mesh:
				/* to show:
				voxel size/scale */
				break;

			default:
				break;
		}


		base.ApplyRevertGUI();
	}
}
