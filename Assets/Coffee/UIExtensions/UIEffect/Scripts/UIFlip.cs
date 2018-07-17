﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{

	[RequireComponent(typeof(Graphic))]
	[DisallowMultipleComponent]
	public class UIFlip : BaseMeshEffect
	{
		//################################
		// Serialize Members.
		//################################
		[SerializeField] private bool m_Horizontal = false;
		[SerializeField] private bool m_Veritical = false;

		//################################
		// Public Members.
		//################################
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Coffee.UIExtensions.UIFlip"/> should be flipped horizontally.
		/// </summary>
		/// <value><c>true</c> if be flipped horizontally; otherwise, <c>false</c>.</value>
		public bool horizontal { get { return this.m_Horizontal; } set { this.m_Horizontal = value; } }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Coffee.UIExtensions.UIFlip"/> should be flipped vertically.
		/// </summary>
		/// <value><c>true</c> if be flipped horizontally; otherwise, <c>false</c>.</value>
		public bool vertical { get { return this.m_Veritical; } set { this.m_Veritical = value; } }

		/// <summary>
		/// Call used to modify mesh.
		/// </summary>
		/// <param name="vh">VertexHelper.</param>
		public override void ModifyMesh(VertexHelper vh)
		{
			RectTransform rt = graphic.rectTransform;
			UIVertex vt = default(UIVertex);
			Vector3 pos;
			Vector2 center = rt.rect.center;
			for (int i = 0; i < vh.currentVertCount; i++)
			{
				vh.PopulateUIVertex(ref vt, i);
				pos = vt.position;
				vt.position = new Vector3(
					m_Horizontal ? -pos.x : pos.x,
					m_Veritical ? -pos.y : pos.y
				);
				vh.SetUIVertex(vt, i);
			}
		}
	}
}