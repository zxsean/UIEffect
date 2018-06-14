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
	public class UIEffectNew : UIEffectBase, IParametizedTexture
	{

		//################################
		// Constant or Static Members.
		//################################
		public const string shaderName = "UI/Hidden/UI-Effect-New";

		public static ParametizedTexture ptex = new ParametizedTexture(4, 1024);
//		{
//			get
//			{
//				if (_ptex == null)
//				{
//					_ptex = new ParametizedTexture(4, 1024);
//
//				}
//				return _ptex;
//			}
//		}
//
//		static ParametizedTexture _ptex;

		public int index { get; set; }

//		[InitializeOnLoadMethod]
//		static void Clear()
//		{
//			_ptex = null;
//		}


		//################################
		// Serialize Members.
		//################################
		[SerializeField][Range(0, 1)] float m_ToneLevel = 1;
		[SerializeField][Range(0, 1)] float m_ColorFactor = 1;
		[SerializeField][Range(0, 1)] float m_Blur = 0.25f;
		[SerializeField][Range(0, 1)] float m_ShadowBlur = 0.25f;
		[SerializeField] ToneMode m_ToneMode;
		[SerializeField] ColorMode m_ColorMode;
		[SerializeField] BlurMode m_BlurMode;
		[SerializeField] Color m_EffectColor = Color.white;

		//################################
		// Public Members.
		//################################

		/// <summary>
		/// Tone effect level between 0(no effect) and 1(complete effect).
		/// </summary>
		public float toneLevel
		{
			get { return m_ToneLevel; }
			set
			{
				m_ToneLevel = Mathf.Clamp(value, 0, 1);
				ptex.SetData(this, 0, m_ToneLevel);
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
				ptex.SetData(this, 1, m_Blur);
			}
		}

		/// <summary>
		/// How far is the blurring shadow from the graphic.
		/// </summary>
		public float shadowBlur
		{
			get { return m_ShadowBlur; }
			set
			{
				m_ShadowBlur = Mathf.Clamp(value, 0, 1);
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
		/// Color for the color effect.
		/// </summary>
		public Color effectColor
		{
			get { return m_EffectColor; }
			set
			{
				m_EffectColor = value;
				SetDirty();
			}
		}

//		protected override void OnValidate()
//		{
//			base.OnValidate();
//			SetDirty();
//
//		}

		protected override void SetDirty()
		{
			if (m_EffectMaterial)
			{
				m_EffectMaterial.SetTexture("_ParametizedTexture", ptex.texture);
			}
			ptex.SetData(this, 0, m_ToneLevel);
			ptex.SetData(this, 1, m_ColorFactor);
			ptex.SetData(this, 2, m_Blur);
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			ptex.Register(this);
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
			if (!isActiveAndEnabled)
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
