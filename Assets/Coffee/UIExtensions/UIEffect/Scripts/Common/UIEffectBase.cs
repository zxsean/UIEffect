using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
	/// <summary>
	/// Abstract effect base for UI.
	/// </summary>
	[ExecuteInEditMode]
	[RequireComponent(typeof(Graphic))]
	[DisallowMultipleComponent]
	public abstract class UIEffectBase : BaseMeshEffect, IMaterialModifier
#if UNITY_EDITOR
		, ISerializationCallbackReceiver
#endif
	{
		protected static readonly Vector2[] splitedCharacterPosition = { Vector2.up, Vector2.one, Vector2.right, Vector2.zero };
		protected static readonly List<UIVertex> tempVerts = new List<UIVertex>();

		[SerializeField] protected Material m_EffectMaterial;

		/// <summary>
		/// Gets target graphic for effect.
		/// </summary>
		public Graphic targetGraphic { get { return graphic; } }

		/// <summary>
		/// Gets material for effect.
		/// </summary>
		public Material effectMaterial { get { return m_EffectMaterial; } }

		public virtual Material GetModifiedMaterial(Material baseMaterial)
		{
			if (!isActiveAndEnabled)
			{
				return baseMaterial;
			}
			return m_EffectMaterial;
		}

#if UNITY_EDITOR
		/// <summary>
		/// Raises the validate event.
		/// </summary>
		protected override void OnValidate ()
		{
			base.OnValidate ();
			UnityEditor.EditorApplication.delayCall += () => UpdateMaterial(false);
		}

		/// <summary>
		/// Raises the before serialize event.
		/// </summary>
		public virtual void OnBeforeSerialize()
		{
		}

		/// <summary>
		/// Raises the after deserialize event.
		/// </summary>
		public virtual void OnAfterDeserialize()
		{
			UnityEditor.EditorApplication.delayCall += () => UpdateMaterial(true);
		}

		/// <summary>
		/// Updates the material.
		/// </summary>
		/// <param name="ignoreInPlayMode">If set to <c>true</c> ignore in play mode.</param>
		protected void UpdateMaterial(bool ignoreInPlayMode)
		{
			if(!this || ignoreInPlayMode && Application.isPlaying)
			{
				return;
			}

			var mat =  GetMaterial();
			if (m_EffectMaterial != mat)
			{
				m_EffectMaterial = mat;
				SetDirty();

				UnityEditor.EditorUtility.SetDirty(this);
			}
		}

		/// <summary>
		/// Gets the material.
		/// </summary>
		/// <returns>The material.</returns>
		protected virtual Material GetMaterial()
		{
			return null;
		}
#endif


		/// <summary>
		/// This function is called when the object becomes enabled and active.
		/// </summary>
		protected override void OnEnable()
		{
//			targetGraphic.material = m_EffectMaterial;
			base.OnEnable();
			SetDirty();
		}

//		/// <summary>
//		/// This function is called when the behaviour becomes disabled () or inactive.
//		/// </summary>
//		protected override void OnDisable()
//		{
////			targetGraphic.material = null;
//			base.OnDisable();
//		}


		/// <summary>
		/// Mark the UIEffect as dirty.
		/// </summary>
		protected virtual void SetDirty()
		{
			if (targetGraphic)
			{
				targetGraphic.SetVerticesDirty();
			}
		}

		protected override void OnDidApplyAnimationProperties()
		{
			SetDirty();
		}
	}
}
