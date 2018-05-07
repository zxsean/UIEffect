using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UITessellationEffect : BaseTessellationEffect
{
	//################################
	// Serialize Members.
	//################################
	[SerializeField]
	bool m_Horizontal = true;
	[SerializeField]
	bool m_Vertical = true;
	[SerializeField][Range(1, 500)]
	float m_TessellationSizeX = 10.0f;
	[SerializeField][Range(1, 500)]
	float m_TessellationSizeY = 10.0f;


	[SerializeField]
	AnimationCurve m_BendCurve = null;
	[SerializeField][Range(1, 500)]
	float m_Multiplier = 10.0f;
	[SerializeField][Range(-1, 1)]
	float m_Offset = 0.0f;

	Rect rect;

	//################################
	// Public Members.
	//################################
	public override void ModifyMesh(VertexHelper vh)
	{
		if (!m_Horizontal && !m_Vertical)
		{
			return;
		}

		rect = graphic.rectTransform.rect;

		base.ModifyMesh(vh);
	}

	public override void TessellateQuad(UIVertex[] quad, List<UIVertex> result)
	{
		// Read the existing quad vertices
		UIVertex v0 = quad[0];	// bottom-left
		UIVertex v1 = quad[1];	// top-left
		UIVertex v2 = quad[2];	// top-right
		UIVertex v3 = quad[3];	// bottom-right

		// Position deltas
		Vector3 deltaX = v2.position - v1.position;
		Vector3 deltaY = v1.position - v0.position;

		// Determine how many tiles there should be
		int xQuads = m_Horizontal ? Mathf.CeilToInt(deltaX.magnitude / m_TessellationSizeX) : 1;
		int yQuads = m_Vertical ? Mathf.CeilToInt(deltaY.magnitude / m_TessellationSizeY) : 1;

//		if (xQuads == 1 && xQuads == 1)
//		{
//			base.TessellateQuad(quad, result);
//			return;
//		}

		// Build the sub quads
		UIVertex[] subquad = new UIVertex[4];
		float startY = 0.0f;
//		for (int y = 0; y < yQuads; y++)
		{
			float endY = 1;
			float startX = 0.0f;
			for (int x = 0; x < xQuads; x++)
			{
				float endX = (float)(x + 1) / xQuads;

				// Append new quad to list
//				UIVertex nv0 = Bilerp(v0, v1, v2, v3, startX, 0);
//				UIVertex nv1 = Bilerp(v0, v1, v2, v3, startX, 1);
//				UIVertex nv2 = Bilerp(v0, v1, v2, v3, endX, 1);
//				UIVertex nv3 = Bilerp(v0, v1, v2, v3, endX, 0);

				subquad[0] = Lerp(v0, v3, startX);
				subquad[1] = Lerp(v1, v2, startX);
				subquad[2] = Lerp(v1, v2, endX);
				subquad[3] = Lerp(v0, v3, endX);

				subquad[0].position = AddPositionY(subquad[0].position);
				subquad[1].position = AddPositionY(subquad[1].position);
				subquad[2].position = AddPositionY(subquad[2].position);
				subquad[3].position = AddPositionY(subquad[3].position);

				AddQuad(subquad);
//				result.Add(nv0);
//				result.Add(nv1);
//				result.Add(nv2);
//				result.Add(nv2);
//				result.Add(nv3);
//				result.Add(nv0);

//				Vector2.

				startX = endX;
			}
			startY = endY;
		}
	}

	Vector3 AddPositionY(Vector3 pos)
	{
		float t = pos.x / rect.width + 0.5f + m_Offset;
		Vector3 v = new Vector2(0.0001f, m_BendCurve.Evaluate(t + 0.0001f) - m_BendCurve.Evaluate(t)).normalized;


		pos.x += v.y * m_Multiplier;
		pos.y += -v.x * m_Multiplier;
		return pos;
	}
}