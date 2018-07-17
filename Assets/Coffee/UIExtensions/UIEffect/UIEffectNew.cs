using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
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
	public class UIEffectNew : UIEffectBase, IMaterialModifier, IParametizedTexture
	{

		//################################
		// Constant or Static Members.
		//################################
		public const string shaderName = "UI/Hidden/UI-Effect-New";

		public static ParametizedTexture ptex = new ParametizedTexture(4, 1024);

		public int index { get; set; }


		//################################
		// Serialize Members.
		//################################
		[FormerlySerializedAs("m_ToneLevel")]
		[SerializeField][Range(0, 1)] float m_EffectFactor = 1;
		[SerializeField][Range(0, 1)] float m_ColorFactor = 1;
		[FormerlySerializedAs("m_Blur")]
		[SerializeField][Range(0, 1)] float m_BlurFactor = 0.25f;
		[SerializeField] ToneMode m_ToneMode;
		[SerializeField] ColorMode m_ColorMode;
		[SerializeField] BlurMode m_BlurMode;

		//################################
		// Public Members.
		//################################

		/// <summary>
		/// Tone effect factor between 0(no effect) and 1(complete effect).
		/// </summary>
		[System.Obsolete("Use effectFactor instead (UnityUpgradable) -> effectFactor")]
		public float effectFactor
		{
			get { return m_EffectFactor; }
			set
			{
				value = Mathf.Clamp(value, 0, 1);
				if (!Mathf.Approximately(m_EffectFactor, value))
				{
					m_EffectFactor = value;
					SetDirty();
				}
			}
		}

		/// <summary>
		/// Color effect factor between 0(no effect) and 1(complete effect).
		/// </summary>
		public float colorFactor
		{
			get { return m_ColorFactor; }
			set
			{
				value = Mathf.Clamp(value, 0, 1);
				if (!Mathf.Approximately(m_ColorFactor, value))
				{
					m_ColorFactor = value;
					SetDirty();
				}
			}
		}

		/// <summary>
		/// How far is the blurring from the graphic.
		/// </summary>
		[System.Obsolete("Use blurFactor instead (UnityUpgradable) -> blurFactor")]
		public float blurFactor
		{
			get { return m_BlurFactor; }
			set
			{
				value = Mathf.Clamp(value, 0, 1);
				if (!Mathf.Approximately(m_BlurFactor, value))
				{
					m_BlurFactor = value;
					SetDirty();
				}
			}
		}

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

		protected override void SetDirty()
		{
			if (m_EffectMaterial)
			{
				m_EffectMaterial.SetTexture(ParametizedTexture.PropertyId, ptex.texture);
			}
			ptex.SetData(this, 0, m_EffectFactor);	// param1.x : effect factor
			ptex.SetData(this, 1, m_ColorFactor);	// param1.y : color factor
			ptex.SetData(this, 2, m_BlurFactor);	// param1.z : blur factor
		}

		protected override void OnEnable()
		{
			ptex.Register(this);
			base.OnEnable();
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			ptex.Unregister(this);
		}

		/// <summary>
		/// Modifies the mesh.
		/// </summary>
		public override void ModifyMesh(VertexHelper vh)
		{
			if (!isActiveAndEnabled || (m_ToneMode == ToneMode.None && m_ColorMode == ColorMode.Multiply && m_BlurMode == BlurMode.None))
			{
				return;
			}

			UIVertex vt = default(UIVertex);
			int count = vh.currentVertCount;

			for (int i = 0; i < count; i++)
			{
				// Set prameters to vertex.
				vh.PopulateUIVertex(ref vt, i);
				vt.uv0 = new Vector2(
					Packer.ToFloat(vt.uv0.x, vt.uv0.y),
					((float)index + 0.5f) / ptex.maxInstanceCount
				);
				vh.SetUIVertex(vt, i);
			}
		}

		public override Material GetModifiedMaterial(Material baseMaterial)
		{
			if (!isActiveAndEnabled || (m_ToneMode == ToneMode.None && m_ColorMode == ColorMode.Multiply && m_BlurMode == BlurMode.None))
			{
				return baseMaterial;
			}
			return m_EffectMaterial;
		}

		#if UNITY_EDITOR
		/// <summary>
		/// Gets the material.
		/// </summary>
		/// <returns>The material.</returns>
		protected override Material GetMaterial()
		{
			if (m_ToneMode == ToneMode.None && m_ColorMode == ColorMode.Multiply && m_BlurMode == BlurMode.None)
			{
				return null;
			}
			
			return MaterialResolver.GetOrGenerateMaterialVariant(Shader.Find(shaderName), m_ToneMode, m_ColorMode, m_BlurMode);
		}
		#endif
	}
}
