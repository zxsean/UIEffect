using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Coffee.UIExtensions;

public class UIEffectBench : MonoBehaviour
{

	[SerializeField] UIEffectClassic classic;
	[SerializeField] UIEffect uie;
	[SerializeField] UIEffectNew uieNew;

	[SerializeField] Text display;

	List<UIEffectBase> instances = new List<UIEffectBase>();

	float _accum;
	int _frames;
	float _left;

	void Start()
	{
		Application.targetFrameRate = 60;
		classic.gameObject.SetActive(false);
		uie.gameObject.SetActive(false);
		uieNew.gameObject.SetActive(false);
	}

	void Update()
	{
		int i = 0;
		foreach (var o in instances)
		{
			float duration = i % 3 + 1f;
			var val = Mathf.Repeat(Time.realtimeSinceStartup, duration) / duration;
			var uIEffect = o as UIEffect;
			if (uIEffect != null)
			{
				uIEffect.toneLevel = val;
				continue;
			}

			var uIEffectClassic = o as UIEffectClassic;
			if (uIEffectClassic != null)
			{
				uIEffectClassic.toneLevel =val;
				continue;
			}

			var uIEffectNew = o as UIEffectNew;
			if (uIEffectNew != null)
			{
				uIEffectNew.toneLevel = val;
				continue;
			}
			i++;
		}


		_left -= Time.deltaTime;
		_accum += Time.timeScale / Time.deltaTime;
		_frames++;

		if (0 < _left) return;

		display.text = string.Format("Count: {0} FPS: {1:F2}", instances.Count, _accum / _frames);

		_left = 0.5f;
		_accum = 0;
		_frames = 0;



	}


	public void AddClassic(int num)
	{
		for (int i = 0; i < num; i++)
		{
			var inst = Instantiate(classic, transform);

			inst.gameObject.SetActive(true);
			instances.Add(inst);

			var rt = inst.transform as RectTransform;
			var anchorX = Random.value;
			var anchorY = Random.value;

			rt.anchorMax = rt.anchorMin = new Vector2(anchorX, anchorY);
			rt.anchoredPosition3D = Vector3.zero;
		}
	}

	public void Addnew(int num)
	{
		for (int i = 0; i < num; i++)
		{
			var inst = Instantiate(uieNew, transform);

			inst.gameObject.SetActive(true);
			instances.Add(inst);

			var rt = inst.transform as RectTransform;
			var anchorX = Random.value;
			var anchorY = Random.value;

			rt.anchorMax = rt.anchorMin = new Vector2(anchorX, anchorY);
			rt.anchoredPosition3D = Vector3.zero;
		}
	}

	public void Add(int num)
	{
		for (int i = 0; i < num; i++)
		{
			var inst = Instantiate(uie, transform);

			inst.gameObject.SetActive(true);
			instances.Add(inst);

			var rt = inst.transform as RectTransform;
			var anchorX = Random.value;
			var anchorY = Random.value;

			rt.anchorMax = rt.anchorMin = new Vector2(anchorX, anchorY);
			rt.anchoredPosition3D = Vector3.zero;
		}
	}

//	IEnumerator co(GameObject go, System.Action<float> action)
//	{
//		go.SetActive(true);
//		instances.Add(go);
//
//		var rt = go.transform as RectTransform;
//		var anchorX = Random.value;
//		var anchorY = Random.value;
//
//		rt.anchorMax = rt.anchorMin = new Vector2(anchorX, anchorY);
//		rt.anchoredPosition3D = Vector3.zero;
//
////		float duration = Random.Range(1f, 2f);
//
//		while (true)
//		{
//			
//			
////			action(Mathf.Repeat(Time.realtimeSinceStartup, duration) / duration);
//			yield return null;
//		}
//	}

	public void Clear()
	{
		instances.ForEach(x=>Object.Destroy(x.gameObject));
		instances.Clear();
//		StopAllCoroutines();
		Resources.UnloadUnusedAssets();
	}
}
