using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
#endif

namespace Coffee.UIExtensions
{
	/// <summary>
	/// UIEffect.
	/// </summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(Graphic))]
	[DisallowMultipleComponent]
	public class UIEffectClassic : UIEffectBase, IMaterialModifier
	{

		//################################
		// Constant or Static Members.
		//################################
		public const string shaderName = "UI/Hidden/UI-Effect-Classic";


		//################################
		// Serialize Members.
		//################################
		[SerializeField][Range(0, 1)] float m_ToneLevel = 1;
		[SerializeField][Range(0, 1)] float m_Blur = 0.25f;
		[SerializeField][Range(0, 1)] float m_ShadowBlur = 0.25f;
		[SerializeField] ShadowStyle m_ShadowStyle;
		[SerializeField] ToneMode m_ToneMode;
		[SerializeField] ColorMode m_ColorMode;
		[SerializeField] BlurMode m_BlurMode;
		[SerializeField] Color m_ShadowColor = Color.black;
		[SerializeField] Vector2 m_EffectDistance = new Vector2(1f, -1f);
		[SerializeField] bool m_UseGraphicAlpha = true;
		[SerializeField] Color m_EffectColor = Color.white;


		//################################
		// Public Members.
		//################################
		/// <summary>
		/// Tone effect level between 0(no effect) and 1(complete effect).
		/// </summary>
		public float toneLevel{ get { return m_ToneLevel; } set { m_ToneLevel = Mathf.Clamp(value, 0, 1); SetDirty(); } }

		/// <summary>
		/// How far is the blurring from the graphic.
		/// </summary>
		public float blur { get { return m_Blur; } set { m_Blur = Mathf.Clamp(value, 0, 1); SetDirty(); } }

		/// <summary>
		/// How far is the blurring shadow from the graphic.
		/// </summary>
		public float shadowBlur { get { return m_ShadowBlur; } set { m_ShadowBlur = Mathf.Clamp(value, 0, 1); SetDirty(); } }

		/// <summary>
		/// Shadow effect mode.
		/// </summary>
		public ShadowStyle shadowStyle { get { return m_ShadowStyle; } set { m_ShadowStyle = value; SetDirty(); } }

		/// <summary>
		/// Tone effect mode.
		/// </summary>
		public ToneMode toneMode { get { return m_ToneMode; } }

		/// <summary>
		/// Color effect mode.
		/// </summary>
		public ColorMode colorMode { get { return m_ColorMode; } }

		/// <summary>
		/// Blur effect mode.
		/// </summary>
		public BlurMode blurMode { get { return m_BlurMode; } }

		/// <summary>
		/// Color for the shadow effect.
		/// </summary>
		public Color shadowColor { get { return m_ShadowColor; } set { m_ShadowColor = value; SetDirty(); } }

		/// <summary>
		/// How far is the shadow from the graphic.
		/// </summary>
		public Vector2 effectDistance { get { return m_EffectDistance; } set { m_EffectDistance = value; SetDirty(); } }

		/// <summary>
		/// Should the shadow inherit the alpha from the graphic?
		/// </summary>
		public bool useGraphicAlpha { get { return m_UseGraphicAlpha; } set { m_UseGraphicAlpha = value; SetDirty(); } }

		/// <summary>
		/// Color for the color effect.
		/// </summary>
		public Color effectColor { get { return m_EffectColor; } set { m_EffectColor = value; SetDirty(); } }

		protected override void OnEnable()
		{
			if (targetGraphic && m_EffectMaterial)
			{
				targetGraphic.material = new Material(m_EffectMaterial);
			}
		}

		protected override void OnDisable()
		{
			if (targetGraphic)
			{
				targetGraphic.material = null;
			}
		}

		protected override void SetDirty()
		{
			if (targetGraphic)
			{
				targetGraphic.SetMaterialDirty();
			}
		}

		/// <summary>
		/// Gets the modified material.
		/// </summary>
		public Material GetModifiedMaterial(Material baseMaterial)
		{
			if (isActiveAndEnabled && m_EffectMaterial)
			{
				baseMaterial.SetVector("_EffectFactor", new Vector4(toneLevel, 0, blur, 0));
			}
			return baseMaterial;
		}

		public override void ModifyMesh(VertexHelper vh)
		{
		}

#if UNITY_EDITOR
		/// <summary>
		/// Gets the material.
		/// </summary>
		/// <returns>The material.</returns>
		protected override Material GetMaterial ()
		{
			return MaterialResolver.GetOrGenerateMaterialVariant(Shader.Find(shaderName), m_ToneMode, m_ColorMode, m_BlurMode);
		}
#endif

		//################################
		// Private Members.
		//################################
	}
}
