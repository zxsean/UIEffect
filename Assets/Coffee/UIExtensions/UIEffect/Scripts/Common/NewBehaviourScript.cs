using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Coffee.UIExtensions;

[ExecuteInEditMode]
public class NewBehaviourScript : MonoBehaviour, ILateUpdatable {

	int count;

	public void OnLateUpdate()
	{
		count++;
//		Debug.Log("NewBehaviourScript.OnLateUpdate! " + count + ", " + UnityEditor.EditorApplication.timeSinceStartup);

//		if (0.5f < Random.value)
//		{
//			throw new System.Exception("フーハハハハ！");
//		}
//		GUILayout.Label(count.ToString());
	}

	void OnEnable()
	{
		count = 0;
		UpdateDispatcher.Register(this);
	}

	void OnDisable()
	{
		UpdateDispatcher.Unregister(this);
	}

//	// Use this for initialization
//	void Start () {
//		
//	}
//	
//	// Update is called once per frame
//	void Update () {
//		
//	}
}
