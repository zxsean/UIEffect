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
	public class UIEffect : UIEffectBase
	{
		//################################
		// Constant or Static Members.
		//################################
		public const string shaderName = "UI/Hidden/UI-Effect";


		//################################
		// Serialize Members.
		//################################
		[SerializeField][Range(0, 1)] float m_ToneLevel = 1;
		[SerializeField][Range(0, 1)] float m_ColorFactor = 1;
		[SerializeField][Range(0, 1)] float m_Blur = 0.25f;
		[SerializeField] ToneMode m_ToneMode;
		[SerializeField] ColorMode m_ColorMode;
		[SerializeField] BlurMode m_BlurMode;

		//################################
		// Public Members.
		//################################
		/// <summary>
		/// Graphic affected by the UIEffect.
		/// </summary>
		[System.Obsolete("Use targetGraphic instead (UnityUpgradable) -> targetGraphic")]
		new public Graphic graphic { get { return base.graphic; } }

		/// <summary>
		/// Tone effect level between 0(no effect) and 1(complete effect).
		/// </summary>
		public float toneLevel
		{
			get { return m_ToneLevel; }
			set
			{
				m_ToneLevel = Mathf.Clamp(value, 0, 1);
				SetDirty();
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
				m_ColorFactor = Mathf.Clamp(value, 0, 1);
				SetDirty();
			}
		}

		/// <summary>
		/// How far is the blurring from the graphic.
		/// </summary>
		public float blur
		{
			get { return m_Blur; }
			set
			{
				m_Blur = Mathf.Clamp(value, 0, 1);
				SetDirty();
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

		/// <summary>
		/// Modifies the mesh.
		/// </summary>
		public override void ModifyMesh(VertexHelper vh)
		{
			if (!isActiveAndEnabled)
			{
				return;
			}

			UIVertex vt;
			vh.GetUIVertexStream(tempVerts);

			// Pack some effect factors to 1 float.
			Vector2 factor = new Vector2(
				                 Packer.ToFloat(m_ToneLevel, m_ColorFactor, m_Blur, 0),
				                 0
			                 );

			for (int i = 0; i < tempVerts.Count; i++)
			{
				vt = tempVerts[i];

				// Set UIEffect prameters to vertex.
				vt.uv1 = factor;
				tempVerts[i] = vt;
			}

			vh.Clear();
			vh.AddUIVertexTriangleStream(tempVerts);

			tempVerts.Clear();
		}

#if UNITY_EDITOR
		/// <summary>
		/// Gets the material.
		/// </summary>
		/// <returns>The material.</returns>
		protected override Material GetMaterial()
		{
			return MaterialResolver.GetOrGenerateMaterialVariant(Shader.Find(shaderName), m_ToneMode, m_ColorMode, m_BlurMode);
		}
#endif

		//################################
		// Private Members.
		//################################
	}
}
