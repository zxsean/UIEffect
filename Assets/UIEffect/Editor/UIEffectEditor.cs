using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Coffee.UIExtensions
{
	/// <summary>
	/// UIEffect editor.
	/// </summary>
	[CustomEditor(typeof(UIEffect))]
	[CanEditMultipleObjects]
	public class UIEffectEditor : Editor
	{
		//################################
		// Constant or Static Members.
		//################################
		/// <summary>
		/// Draw effect properties.
		/// </summary>
		public static void DrawEffectProperties(string shaderName, SerializedObject serializedObject)
		{
			bool changed = false;

			//================
			// Effect material.
			//================
			var spMaterial = serializedObject.FindProperty("m_EffectMaterial");
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.PropertyField(spMaterial);
			EditorGUI.EndDisabledGroup();

			//================
			// Tone setting.
			//================
			var spToneMode = serializedObject.FindProperty("m_ToneMode");
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(spToneMode);
			changed |= EditorGUI.EndChangeCheck();

			// When tone is enable, show parameters.
			if (spToneMode.intValue != (int)ToneMode.None)
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ToneLevel"));
				EditorGUI.indentLevel--;
			}

			//================
			// Color setting.
			//================
			var spColorMode = serializedObject.FindProperty("m_ColorMode");
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(spColorMode);
			changed |= EditorGUI.EndChangeCheck();

			// When color is enable, show parameters.
			if (spColorMode.intValue != (int)ColorMode.None)
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ColorFactor"));
				EditorGUI.indentLevel--;
			}

			//================
			// Blur setting.
			//================
			var spBlurMode = serializedObject.FindProperty("m_BlurMode");
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(spBlurMode);
			changed |= EditorGUI.EndChangeCheck();

			// When blur is enable, show parameters.
			if (spBlurMode.intValue != (int)BlurMode.None)
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Blur"));
				EditorGUI.indentLevel--;
			}

			// Set effect material.
			if (!serializedObject.isEditingMultipleObjects && spToneMode.intValue == 0 && spColorMode.intValue == 0 && spBlurMode.intValue == 0)
			{
				spMaterial.objectReferenceValue = null;
			}
			else if (changed || !serializedObject.isEditingMultipleObjects)
			{
				spMaterial.objectReferenceValue = UIEffect.GetOrGenerateMaterialVariant(Shader.Find(shaderName),
					(ToneMode)spToneMode.intValue,
					(ColorMode)spColorMode.intValue,
					(BlurMode)spBlurMode.intValue
				);
			}
		}

		//################################
		// Private Members.
		//################################
		SerializedProperty _spCustomEffect;
		SerializedProperty _spEffectMaterial;
		SerializedProperty _spEffectColor;
		SerializedProperty _spCustomFactorX;
		SerializedProperty _spCustomFactorY;
		SerializedProperty _spCustomFactorZ;
		SerializedProperty _spCustomFactorW;

		void OnEnable()
		{
            return;

			_spEffectColor = serializedObject.FindProperty("m_EffectColor");

			_spCustomEffect = serializedObject.FindProperty("m_CustomEffect");
			_spEffectMaterial = serializedObject.FindProperty("m_EffectMaterial");
			var spFactor = serializedObject.FindProperty("m_CustomFactor");
			_spCustomFactorX = spFactor.FindPropertyRelative("x");
			_spCustomFactorY = spFactor.FindPropertyRelative("y");
			_spCustomFactorZ = spFactor.FindPropertyRelative("z");
			_spCustomFactorW = spFactor.FindPropertyRelative("w");
		}

		/// <summary>
		/// Implement this function to make a custom inspector.
		/// </summary>
		public override void OnInspectorGUI()
		{
            base.OnInspectorGUI();
            return;

			serializedObject.Update();

			// Custom effect.
			EditorGUILayout.PropertyField(_spCustomEffect);
			if(_spCustomEffect.boolValue)
			{
				EditorGUILayout.PropertyField(_spEffectMaterial);

				EditorGUI.indentLevel++;
				EditorGUILayout.Slider(_spCustomFactorX, 0, 1, new GUIContent("Effect Factor X"));
				EditorGUILayout.Slider(_spCustomFactorY, 0, 1, new GUIContent("Effect Factor Y"));
				EditorGUILayout.Slider(_spCustomFactorZ, 0, 1, new GUIContent("Effect Factor Z"));
				EditorGUILayout.Slider(_spCustomFactorW, 0, 1, new GUIContent("Effect Factor W"));
				EditorGUILayout.PropertyField(_spEffectColor);
				EditorGUI.indentLevel--; 
			}
			else
			{
				DrawEffectProperties(UIEffect.shaderName, serializedObject);
			}

			serializedObject.ApplyModifiedProperties();

#if UNITY_5_6_OR_NEWER
			var graphic = (target as UIEffect).graphic;
			if(graphic)
			{
				var canvas = graphic.canvas;
				if( canvas && 0 == (canvas.additionalShaderChannels & AdditionalCanvasShaderChannels.TexCoord1))
				{
					using (new GUILayout.HorizontalScope())
					{
						EditorGUILayout.HelpBox("[Unity5.6+] Enable TexCoord1 of Canvas.additionalShaderChannels to use UIEffect.", MessageType.Warning);
						if (GUILayout.Button("Fix"))
							canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
					}
				}
			}
#endif
		}
	}
}