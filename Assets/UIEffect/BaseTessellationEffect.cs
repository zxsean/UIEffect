using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public abstract class BaseTessellationEffect : BaseMeshEffect
{
	static readonly List<UIVertex> s_InputVerts = new List<UIVertex>();
	static readonly List<UIVertex> s_OutputVerts = new List<UIVertex>();
	static readonly UIVertex[] s_QuadVerts = new UIVertex[4];

	public override void ModifyMesh(VertexHelper vh)
	{
		if (vh.currentVertCount == 0 || vh.currentVertCount % 4 != 0)
		{
			return;
		}

		s_InputVerts.Clear();
		s_OutputVerts.Clear();

		vh.GetUIVertexStream(s_InputVerts);

		// Tessellate
		for (int i = 0; i < s_InputVerts.Count; i += 6)
		{
			if (graphic is Text)
			{
				s_QuadVerts[0] = s_InputVerts[i];	// bottom-left
				s_QuadVerts[1] = s_InputVerts[i+4];	// top-left
				s_QuadVerts[2] = s_InputVerts[i+2];	// top-right
				s_QuadVerts[3] = s_InputVerts[i+1];	// bottom-right
			}
			else
			{
				s_QuadVerts[0] = s_InputVerts[i];	// bottom-left
				s_QuadVerts[1] = s_InputVerts[i+1];	// top-left
				s_QuadVerts[2] = s_InputVerts[i+2];	// top-right
				s_QuadVerts[3] = s_InputVerts[i+4];	// bottom-right
			}
			TessellateQuad(s_QuadVerts, s_OutputVerts);
		}

		vh.Clear();
		vh.AddUIVertexTriangleStream(s_OutputVerts);

		s_InputVerts.Clear();
		s_OutputVerts.Clear();
	}

	public virtual void TessellateQuad(UIVertex[] quad, List<UIVertex> result)
	{
		AddQuad(quad);
		/*
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

		// Build the sub quads
		float startY = 0.0f;
		for (int y = 0; y < yQuads; y++)
		{
			float endY = (float)(y + 1) / yQuads;
			float startX = 0.0f;
			for (int x = 0; x < xQuads; x++)
			{
				float endX = (float)(x + 1) / xQuads;

				// Append new quad to list
				UIVertex nv0 = Bilerp(v0, v1, v2, v3, startX, startY);
				UIVertex nv1 = Bilerp(v0, v1, v2, v3, startX, endY);
				UIVertex nv2 = Bilerp(v0, v1, v2, v3, endX, endY);
				UIVertex nv3 = Bilerp(v0, v1, v2, v3, endX, startY);

				result.Add(nv0);
				result.Add(nv1);
				result.Add(nv2);
				result.Add(nv2);
				result.Add(nv3);
				result.Add(nv0);

				startX = endX;
			}
			startY = endY;
		}
		*/
	}

	protected void AddQuad(UIVertex[] quad)
	{
		if (graphic is Text)
		{
			s_OutputVerts.Add(quad[0]);
			s_OutputVerts.Add(quad[3]);
			s_OutputVerts.Add(quad[2]);
			s_OutputVerts.Add(quad[2]);
			s_OutputVerts.Add(quad[1]);
			s_OutputVerts.Add(quad[0]);
		}
		else
		{
			s_OutputVerts.Add(quad[0]);
			s_OutputVerts.Add(quad[1]);
			s_OutputVerts.Add(quad[2]);
			s_OutputVerts.Add(quad[2]);
			s_OutputVerts.Add(quad[3]);
			s_OutputVerts.Add(quad[0]);
		}

	}

	protected static UIVertex Lerp(UIVertex v0, UIVertex v1, float a)
	{
		UIVertex output = default(UIVertex);
		output.position = Vector3.Lerp(v0.position, v1.position, a);
		output.normal = Vector3.Lerp(v0.normal, v1.normal, a);

		output.tangent = Vector4.Lerp(v0.tangent, v1.tangent, a);

		output.uv0 = Vector2.Lerp(v0.uv0, v1.uv0, a);
		output.uv1 = Vector2.Lerp(v0.uv1, v1.uv1, a);
		output.color = Color.Lerp(v0.color, v1.color, a);
		return output;
	}

	protected static UIVertex Bilerp(UIVertex v0, UIVertex v1, UIVertex v2, UIVertex v3, float a, float b)
	{
		UIVertex output = default(UIVertex);
		output.position = Bilerp(v0.position, v1.position, v2.position, v3.position, a, b);
		output.normal = Bilerp(v0.normal, v1.normal, v2.normal, v3.normal, a, b);

		output.tangent = Bilerp(v0.tangent, v1.tangent, v2.tangent, v3.tangent, a, b);

		output.uv0 = Bilerp(v0.uv0, v1.uv0, v2.uv0, v3.uv0, a, b);
		output.uv1 = Bilerp(v0.uv1, v1.uv1, v2.uv1, v3.uv1, a, b);
		output.color = Bilerp(v0.color, v1.color, v2.color, v3.color, a, b);
		return output;
	}

	protected static Vector2 Bilerp(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3, float a, float b)
	{
		Vector2 top = Vector2.Lerp(v1, v2, a);
		Vector2 bottom = Vector2.Lerp(v0, v3, a);
		return Vector2.Lerp(bottom, top, b);
	}

	protected static Vector3 Bilerp(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, float a, float b)
	{
		Vector3 top = Vector3.Lerp(v1, v2, a);
		Vector3 bottom = Vector3.Lerp(v0, v3, a);
		return Vector3.Lerp(bottom, top, b);
	}

	protected static Vector4 Bilerp(Vector4 v0, Vector4 v1, Vector4 v2, Vector4 v3, float a, float b)
	{
		Vector4 top = Vector4.Lerp(v1, v2, a);
		Vector4 bottom = Vector4.Lerp(v0, v3, a);
		return Vector4.Lerp(bottom, top, b);
	}

	protected static Color Bilerp(Color v0, Color v1, Color v2, Color v3, float a, float b)
	{
		Color top = Color.Lerp(v1, v2, a);
		Color bottom = Color.Lerp(v0, v3, a);
		return Color.Lerp(bottom, top, b);
	}

//	[SerializeField]
//	bool m_Horizontal = true;
//	[SerializeField]
//	bool m_Vertical = true;
//	[SerializeField][Range(1, 500)]
//	float m_TessellationSizeX = 10.0f;
//	[SerializeField][Range(1, 500)]
//	float m_TessellationSizeY = 10.0f;
}