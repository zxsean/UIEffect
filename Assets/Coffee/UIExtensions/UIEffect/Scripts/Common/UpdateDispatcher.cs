using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Coffee.UIExtensions
{
	public interface IUpdatable
	{
		void OnUpdate();
	}

	public interface ILateUpdatable
	{
		void OnLateUpdate();
	}

	public interface IFixedUpdatable
	{
		void OnFixedUpdate();
	}

	//	[ExecuteInEditMode]
	public class UpdateDispatcher : MonoBehaviour
	{
		const int kInitialSize = 16;

		int _tail = 0;
		int _lateTail = 0;
		int _fixedTail = 0;
		IUpdatable[] _array = new IUpdatable[kInitialSize];
		ILateUpdatable[] _lateArray = new ILateUpdatable[kInitialSize];
		IFixedUpdatable[] _fixedArray = new IFixedUpdatable[kInitialSize];

		static UpdateDispatcher instance
		{
			get
			{
				if (!_instance)
				{
					_instance = FindObjectOfType<UpdateDispatcher>();
					if (!_instance)
					{
						_instance = new GameObject("UpdateDispatcher").AddComponent<UpdateDispatcher>();
//						_instance.gameObject.hideFlags = HideFlags.DontSave;
					}
				}
				return _instance;
			}
		}

		static UpdateDispatcher _instance;

		void Awake()
		{
			if (instance && _instance != this)
			{
				Destroy(gameObject);
			}
			if (_instance == this)
			{
				DontDestroyOnLoad(gameObject);
			}
		}

		void Update()
		{
			for (int i = 0; i < _tail; i++)
			{
				if (_array[i] == null)
					continue;
				try{
					_array[i].OnUpdate();
				}
				catch(System.Exception ex)
				{
					Debug.LogException(ex);
				}
			}
		}

		void LateUpdate()
		{
			for (int i = 0; i < _lateTail; i++)
			{
				if (_lateArray[i] == null)
					continue;

				try{
					_lateArray[i].OnLateUpdate();
				}
				catch(System.Exception ex)
				{
					Debug.LogException(ex);
				}

			}
		}

		void FixedUpdate()
		{
			for (int i = 0; i < _fixedTail; i++)
			{
				if (_fixedArray[i] == null)
					continue;

				try{
					_fixedArray[i].OnFixedUpdate();
				}
				catch(System.Exception ex)
				{
					Debug.LogException(ex);
				}

			}
		}

		/// <summary>
		/// Update 対象の追加.
		/// </summary>
		public static void Register(object updatable)
		{
			if (updatable == null)
				return;

#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying && !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
			{
				EditorUpdateDispatcher.Register(updatable);
				return;
			}
#endif
//			Debug.LogFormat("<color=green>UpdateDispatcher.Register {0}, {1}, {2}, {3}</color>", updatable, Application.isPlaying, UnityEditor.EditorApplication.isPlaying, UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode);

			var inst = instance;

			inst.Register(updatable as IUpdatable, ref inst._array, ref inst._tail);
			inst.Register(updatable as ILateUpdatable, ref inst._lateArray, ref inst._lateTail);
			inst.Register(updatable as IFixedUpdatable, ref inst._fixedArray, ref inst._fixedTail);
		}

		void Register<T>(T updatable, ref T[] array, ref int tail)
		{
			if (Equals(updatable, null))
			{
				return;
			}
			if (array.Length == tail)
			{
				Array.Resize(ref array, checked(tail * 2));
			}
			array[tail++] = updatable;
		}

		/// <summary>
		/// 指定した Updatable を Update 対象から除外する.
		/// </summary>
		public static void Unregister(object updatable)
		{
			if (updatable == null)
				return;

#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying && !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
			{
				EditorUpdateDispatcher.Unregister(updatable);
				return;
			}
#endif
//			Debug.LogFormat("<color=green>UpdateDispatcher.Unregister {0}, {1}, {2}, {3}, {4}, {5}</color>", updatable, Application.isPlaying, UnityEditor.EditorApplication.isPlaying, UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode, _instance, !_instance);

			if (!_instance)
				return;

			var inst = _instance;
			inst.Unregister(updatable as IUpdatable, inst._array, ref inst._tail);
			inst.Unregister(updatable as ILateUpdatable, inst._lateArray, ref inst._lateTail);
			inst.Unregister(updatable as IFixedUpdatable, inst._fixedArray, ref inst._fixedTail);
		}

		void Unregister<T>(T updatable, T[] array, ref int tail)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (Equals(array[i], updatable))
				{
					array[i] = default(T);
					Refresh(array, ref tail);
					return;
				}
			}
		}

		void Refresh<T>(T[] array, ref int tail)
		{
			var j = tail - 1;

			// 指定した部分は null に,
			// null の部分には配列内の一番後ろにある要素を代入.
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == null)
				{
					while (i < j)
					{
						var fromTail = array[j];
						if (!Equals(fromTail, null))
						{
							array[i] = fromTail;
							array[j] = default(T);
							j--;
							goto NEXTLOOP;
						}
						j--;
					}

					tail = i;
					break;
				}

				NEXTLOOP:
				continue;
			}
		}



	}

	#if UNITY_EDITOR
	public class EditorUpdateDispatcher
	{
		static EditorUpdateDispatcher _instance;

		public static EditorUpdateDispatcher instance { get { return _instance ?? (_instance = new EditorUpdateDispatcher()); } }

		EditorUpdateDispatcher()
		{
//			Debug.LogFormat("<color=red>EditorUpdateDispatcher is generated!</color> {0}, {1}, {2}", Application.isPlaying, UnityEditor.EditorApplication.isPlaying, UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode);
			UnityEditor.EditorApplication.update += () => Update(OnUpdate);
			UnityEditor.EditorApplication.update += () => Update(OnFixedUpdate);
			UnityEditor.EditorApplication.update += () => Update(OnLateUpdate);
		}

		//			~EditorUpdateDispatcher()
		//			{
		//				UnityEditor.EditorApplication.update += () => Update(OnUpdate);
		//				UnityEditor.EditorApplication.update += () => Update(OnFixedUpdate);
		//				UnityEditor.EditorApplication.update += () => Update(OnLateUpdate);
		//			}

		public static void Register(object updatable)
		{
//			if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
//			{
//				return;
//			}
//			Debug.LogFormat("<color=cyan>EditorUpdateDispatcher.Register {0}, {1}, {2}, {3}</color>", updatable, Application.isPlaying, UnityEditor.EditorApplication.isPlaying, UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode);

			var inst = instance;
			if (updatable is IUpdatable)
				inst.OnUpdate += (updatable as IUpdatable).OnUpdate;
			if (updatable is ILateUpdatable)
				inst.OnLateUpdate += (updatable as ILateUpdatable).OnLateUpdate;
			if (updatable is IFixedUpdatable)
				inst.OnFixedUpdate += (updatable as IFixedUpdatable).OnFixedUpdate;
		}

		public static void Unregister(object updatable)
		{
			if (_instance == null)
			{
				return;
			}
//			Debug.LogFormat("<color=cyan>EditorUpdateDispatcher.Unregister {0}, {1}, {2}, {3}</color>", updatable, Application.isPlaying, UnityEditor.EditorApplication.isPlaying, UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode);
			var inst = _instance;
			if (updatable is IUpdatable)
				inst.OnUpdate -= (updatable as IUpdatable).OnUpdate;
			if (updatable is ILateUpdatable)
				inst.OnLateUpdate -= (updatable as ILateUpdatable).OnLateUpdate;
			if (updatable is IFixedUpdatable)
				inst.OnFixedUpdate -= (updatable as IFixedUpdatable).OnFixedUpdate;
		}

		System.Action OnUpdate;
		System.Action OnLateUpdate;
		System.Action OnFixedUpdate;

		//			void Update()
		//			{
		//				Update(OnUpdate);
		//			}
		//			void LateUpdate()
		//			{
		//				Update(OnLateUpdate);
		//			}
		//			void FixedUpdate()
		//			{
		//				Update(OnUpdate);
		//			}

		double time = 0;
		const double interval = 1 / 30d;

		void Update(System.Action action)
		{
			if (action == null || Application.isPlaying || UnityEditor.EditorApplication.timeSinceStartup - time < interval)
				return;
			time = UnityEditor.EditorApplication.timeSinceStartup;

//			Debug.LogFormat("<color=orange>EditorUpdateDispatcher.Update {0}, {1}, {2}, {3}</color>", action, Application.isPlaying, UnityEditor.EditorApplication.isPlaying, UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode);

			foreach (var d in action.GetInvocationList())
			{
				try
				{
					d.DynamicInvoke(new object[0]);
				}
				catch (System.Exception ex)
				{
					Debug.LogException(ex);
				}
			}
		}
	}
	#endif
}
