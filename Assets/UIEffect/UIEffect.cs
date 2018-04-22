﻿using System;
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
    public enum EffectMode
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
        [SerializeField] private EffectMode effectMode;
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
        public EffectMode EffectMode
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


        public static Material GetMaterial(Shader shader, EffectMode tone, ColorMode color, BlurMode blur)
        {
            string variantName = GetVariantName(shader, tone, color, blur);
            return AssetDatabase.FindAssets("t:Material " + Path.GetFileName(shader.name))
				.Select(x => AssetDatabase.GUIDToAssetPath(x))
				.SelectMany(x => AssetDatabase.LoadAllAssetsAtPath(x))
				.OfType<Material>()
				.FirstOrDefault(x => x.name == variantName);
        }


        public static Material GetOrGenerateMaterialVariant(Shader shader, EffectMode tone, ColorMode color, BlurMode blur)
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

        public static string GetVariantName(Shader shader, EffectMode tone, ColorMode color, BlurMode blur)
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
    }
}
