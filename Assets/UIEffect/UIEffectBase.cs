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
    public abstract class UIEffectBase : BaseMeshEffect, ISerializationCallbackReceiver
    {
        protected static readonly List<UIVertex> TempVertex = new List<UIVertex>();

        [SerializeField] private Material effectMaterial;

        /// <summary>
        /// Gets target graphic for effect.
        /// </summary>
        public Graphic TargetGraphic
        {
            get
            {
                return this.graphic;
            }
        }

        /// <summary>
        /// Gets material for effect.
        /// </summary>
        public Material EffectMaterial
        {
            get
            {
                return this.effectMaterial;
            }
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
            this.effectMaterial = this.GetEffectMaterial();
        }

        /// <summary>
        /// Pack 4 low-precision [0-1] floats values to a float.
        /// Each value [0-1] has 64 steps(6 bits).
        /// </summary>
        /// <returns>Packed value.</returns>
        /// <param name="x">First [0-1] value.</param>
        /// <param name="y">Second [0-1] value.</param>
        /// <param name="z">Third [0-1] value.</param>
        /// <param name="w">Fourth [0-1] value.</param>
        protected static float PackToFloat(float x, float y, float z, float w)
        {
            const int PRECISION = (1 << 6) - 1;
            return (Mathf.FloorToInt(w * PRECISION) << 18)
            + (Mathf.FloorToInt(z * PRECISION) << 12)
            + (Mathf.FloorToInt(y * PRECISION) << 6)
            + Mathf.FloorToInt(x * PRECISION);
        }

        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        protected override void OnEnable()
        {
            this.TargetGraphic.material = this.EffectMaterial;
            base.OnEnable();
        }

        /// <summary>
        /// This function is called when the behaviour becomes disabled () or inactive.
        /// </summary>
        protected override void OnDisable()
        {
            this.TargetGraphic.material = null;
            base.OnDisable();
        }

        /// <summary>
        /// Gets the material.
        /// </summary>
        /// <returns>The material.</returns>
        protected virtual Material GetEffectMaterial()
        {
            return null;
        }

        /// <summary>
        /// Mark the UIEffect as dirty.
        /// </summary>
        protected void SetDirty()
        {
            if (this.TargetGraphic)
            {
                this.TargetGraphic.SetVerticesDirty();
            }
        }
    }
}
