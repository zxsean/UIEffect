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
    /// Tone effect mode.
    /// </summary>
    public enum ToneMode
    {
        None = 0,
        Grayscale,
        Sepia,
        Nega,
        Pixel,
        Mono,
        Cutoff,
        Hue,
    }

    /// <summary>
    /// Color effect mode.
    /// </summary>
    public enum ColorMode
    {
        None = 0,
        Set,
        Add,
        Sub,
    }

    /// <summary>
    /// Blur effect mode.
    /// </summary>
    public enum BlurMode
    {
        None = 0,
        Fast,
        Medium,
        Detail,
    }

    /// <summary>
    /// Composite effect for UI.
    /// </summary>
    public class UIEffect : UIEffectBase
    {
        public const string shaderName = "UI/Hidden/UI-Effect";

        [FormerlySerializedAs("m_ToneLevel")]
        [SerializeField][Range(0, 1)] private float effectFactor = 1;
        [FormerlySerializedAs("m_ColorFactor")]
        [SerializeField][Range(0, 1)] private float colorFactor = 1;
        [FormerlySerializedAs("m_Blur")]
        [SerializeField][Range(0, 1)] private float blurFactor = 0.25f;
        [FormerlySerializedAs("m_ToneMode")]
        [SerializeField] private ToneMode effectMode;
        [FormerlySerializedAs("m_ColorMode")]
        [SerializeField] private ColorMode colorMode;
        [FormerlySerializedAs("m_BlurMode")]
        [SerializeField] private BlurMode blurMode;

        /// <summary>
        /// Gets or sets effect factor between 0 to 1.
        /// </summary>
        public float EffectFactor
        {
            get
            {
                return this.effectFactor;
            }

            set
            {
                this.effectFactor = Mathf.Clamp01(value);
                this.SetGraphicDirty();
            }
        }

        /// <summary>
        /// Gets or sets color effect factor between 0 to 1.
        /// </summary>
        public float ColorFactor
        {
            get
            {
                return this.colorFactor;
            }

            set
            {
                this.colorFactor = Mathf.Clamp01(value);
                this.SetGraphicDirty();
            }
        }

        /// <summary>
        /// Gets or sets how far is the blurring from the graphic.
        /// </summary>
        public float BlurFactor
        {
            get
            {
                return this.blurFactor;
            }

            set
            {
                this.blurFactor = Mathf.Clamp01(value);
                this.SetGraphicDirty();
            }
        }

        /// <summary>
        /// Gets effect mode.
        /// </summary>
        public ToneMode EffectMode
        {
            get
            {
                return this.effectMode;
            }
        }

        /// <summary>
        /// Gets color effect mode.
        /// </summary>
        public ColorMode ColorMode
        {
            get
            {
                return this.colorMode;
            }
        }

        /// <summary>
        /// Gets blur effect mode.
        /// </summary>
        public BlurMode BlurMode
        {
            get
            {
                return this.blurMode;
            }
        }

        /// <summary>
        /// Raises the pre modify mesh event.
        /// </summary>
        protected override void OnPreModifyMesh()
        {
            var x = PackToFloat(this.effectFactor, this.ColorFactor, this.BlurFactor, 0);
            currentFactor.Set(x, 0);
        }

        /// <summary>
        /// Gets the material.
        /// </summary>
        /// <returns>The material.</returns>
        protected override Material GetEffectMaterial()
        {
            #if UNITY_EDITOR
            return (this.EffectMode == 0) && (this.colorMode == 0) && (this.blurMode == 0)
                ? null
                    : UIEffect.GetOrGenerateMaterialVariant(Shader.Find(shaderName), this.EffectMode, this.colorMode, this.blurMode);
            #else
            return base.GetEffectMaterial();
            #endif
        }

        #if UNITY_EDITOR

        //		public void OnBeforeSerialize()
        //		{
        //		}

//        public void OnAfterDeserialize()
//        {
//			var obj = this;
//			EditorApplication.delayCall += () =>
//			{
//				if (Application.isPlaying || !obj)
//					return;
//
//				var mat = (0 == toneMode) && (0 == colorMode) && (0 == blurMode)
//						? null
//						: GetOrGenerateMaterialVariant(Shader.Find(shaderName), toneMode, colorMode, blurMode);
//
//				if(EffectMaterial == mat && graphic.material == mat)
//					return;
//					
//				graphic.material = effectMaterial = mat;
//				EditorUtility.SetDirty(this);
//				EditorUtility.SetDirty(graphic);
//				EditorApplication.delayCall +=AssetDatabase.SaveAssets;
//			};
//        }

        public static Material GetMaterial(Shader shader, ToneMode tone, ColorMode color, BlurMode blur)
        {
            string variantName = GetVariantName(shader, tone, color, blur);
            return AssetDatabase.FindAssets("t:Material " + Path.GetFileName(shader.name))
				.Select(x => AssetDatabase.GUIDToAssetPath(x))
				.SelectMany(x => AssetDatabase.LoadAllAssetsAtPath(x))
				.OfType<Material>()
				.FirstOrDefault(x => x.name == variantName);
        }


        public static Material GetOrGenerateMaterialVariant(Shader shader, ToneMode tone, ColorMode color, BlurMode blur)
        {
            if (!shader)
                return null;

            Material mat = GetMaterial(shader, tone, color, blur);

            if (!mat)
            {
                Debug.Log("Generate material : " + GetVariantName(shader, tone, color, blur));
                mat = new Material(shader);

                if (0 < tone)
                    mat.EnableKeyword("UI_TONE_" + tone.ToString().ToUpper());
                if (0 < color)
                    mat.EnableKeyword("UI_COLOR_" + color.ToString().ToUpper());
                if (0 < blur)
                    mat.EnableKeyword("UI_BLUR_" + blur.ToString().ToUpper());

                mat.name = GetVariantName(shader, tone, color, blur);
                mat.hideFlags |= HideFlags.NotEditable;

#if UIEFFECT_SEPARATE
				bool isMainAsset = true;
				string dir = Path.GetDirectoryName(GetDefaultMaterialPath (shader));
				string materialPath = Path.Combine(Path.Combine(dir, "Separated"), mat.name + ".mat");
#else
                bool isMainAsset = (0 == tone) && (0 == color) && (0 == blur);
                string materialPath = GetDefaultMaterialPath(shader);
#endif
                if (isMainAsset)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(materialPath));
                    AssetDatabase.CreateAsset(mat, materialPath);
                    AssetDatabase.SaveAssets();
                }
                else
                {
                    mat.hideFlags |= HideFlags.HideInHierarchy;
                    AssetDatabase.AddObjectToAsset(mat, materialPath);
                }
            }
            return mat;
        }

        public static string GetDefaultMaterialPath(Shader shader)
        {
            var name = Path.GetFileName(shader.name);
            return AssetDatabase.FindAssets("t:Material " + name)
				.Select(x => AssetDatabase.GUIDToAssetPath(x))
				.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x) == name)
            ?? ("Assets/UIEffect/Materials/" + name + ".mat");
        }

        public static string GetVariantName(Shader shader, ToneMode tone, ColorMode color, BlurMode blur)
        {
            return
#if UIEFFECT_SEPARATE
				"[Separated] " + Path.GetFileName(shader.name)
#else
				Path.GetFileName(shader.name)
#endif
            + (0 < tone ? "-" + tone : "")
            + (0 < color ? "-" + color : "")
            + (0 < blur ? "-" + blur : "");
        }
#endif

        //		//################################
        //		// Private Members.
        //		//################################
        //		static readonly List<UIVertex> s_Verts = new List<UIVertex>();
        //
        //		/// <summary>
        //		/// Mark the UIEffect as dirty.
        //		/// </summary>
        //		void _SetDirty()
        //		{
        //			if(graphic)
        //				graphic.SetVerticesDirty();
        //		}
        //
        //		/// <summary>
        //		/// Pack 4 low-precision [0-1] floats values to a float.
        //		/// Each value [0-1] has 64 steps(6 bits).
        //		/// </summary>
        //		static float _PackToFloat(float x, float y, float z, float w)
        //		{
        //			const int PRECISION = (1 << 6) - 1;
        //			return (Mathf.FloorToInt(w * PRECISION) << 18)
        //			+ (Mathf.FloorToInt(z * PRECISION) << 12)
        //			+ (Mathf.FloorToInt(y * PRECISION) << 6)
        //			+ Mathf.FloorToInt(x * PRECISION);
        //		}
        //
        //		/// <summary>
        //		/// Pack 4 low-precision [0-1] floats values to a float.
        //		/// Each value [0-1] has 64 steps(6 bits).
        //		/// </summary>
        //		static float _PackToFloat(Vector4 factor)
        //		{
        //			return _PackToFloat(Mathf.Clamp01(factor.x), Mathf.Clamp01(factor.y), Mathf.Clamp01(factor.z), Mathf.Clamp01(factor.w));
        //		}
    }
}
