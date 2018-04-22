using System;
using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
    /// <summary>
    /// Custom effect for UI.
    /// </summary>
    public class UICustomEffect : UIEffectBase
    {
        [SerializeField][Range(0, 1)]private float effectFactor1 = 0;
        [SerializeField][Range(0, 1)]private float effectFactor2 = 0;
        [SerializeField][Range(0, 1)]private float effectFactor3 = 0;
        [SerializeField][Range(0, 1)]private float effectFactor4 = 0;
        [SerializeField][Range(0, 1)]private float effectFactor5 = 0;
        [SerializeField][Range(0, 1)]private float effectFactor6 = 0;
        [SerializeField][Range(0, 1)]private float effectFactor7 = 0;
        [SerializeField][Range(0, 1)]private float effectFactor8 = 0;

        /// <summary>
        /// Gets or sets effect factor 1 (ranged [0-1]).
        /// </summary>
        public float EffectFactor1
        {
            get
            {
                return this.effectFactor1;
            }

            set
            {
                this.effectFactor1 = Mathf.Clamp(value, 0, 1);
                this.SetGraphicDirty();
            }
        }

        /// <summary>
        /// Raises the pre modify mesh event.
        /// </summary>
        protected override void OnPreModifyMesh()
        {
            var x = PackToFloat(this.effectFactor1, this.effectFactor2, this.effectFactor3, this.effectFactor4);
            var y = PackToFloat(this.effectFactor5, this.effectFactor6, this.effectFactor7, this.effectFactor8);
            currentFactor.Set(x, y);
        }
    }
}
