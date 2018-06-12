using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using System;

namespace Coffee.UIExtensions
{
	public interface IUpdatable
	{
		void OnUpdate();
	}

	public class UpdateDispatcher : MonoBehaviour
	{
		const int kInitialSize = 16;

		int _tail = 0;
		IUpdatable[] _array = new IUpdatable[kInitialSize];

//		[SerializeField] bool m_ReduceArraySizeWhenNeed = false;
//
//		public static bool reduceArraySizeWhenNeed { get { return manager.m_ReduceArraySizeWhenNeed; } set { manager.m_ReduceArraySizeWhenNeed = value; } }

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
						_instance.gameObject.hideFlags = HideFlags.DontSave;
					}
				}
				return _instance;
			}
		}

		static UpdateDispatcher _instance;

		void Awake()
		{
			if (instance && _instance != this)
				Destroy(gameObject);
		}

		void Update()
		{
			for (int i = 0; i < _tail; i++)
			{
				if (_array[i] == null)
					continue;
				_array[i].OnUpdate();
			}
		}

		/// <summary>
		/// Update 対象の追加.
		/// </summary>
		public static void Register(IUpdatable updatable)
		{
			if (updatable == null)
				return;
			instance.register(updatable);
		}

		void register(IUpdatable updatable)
		{
			if (_array.Length == _tail)
			{
				Array.Resize(ref _array, checked(_tail * 2));
			}
			_array[_tail++] = updatable;
		}

		/// <summary>
		/// 指定した Updatable を Update 対象から除外する.
		/// </summary>
		public static void Unregister(IUpdatable updatable)
		{
			if (updatable == null)
				return;
			instance.unregister(updatable);
		}

		void unregister(IUpdatable updatable)
		{
			for (int i = 0; i < _array.Length; i++)
			{
				if (_array[i] == updatable)
				{
					_array[i] = null;
					refresh();
					return;
				}
			}
		}

		/// <summary>
		/// 配列整理.
		/// </summary>
		public static void Reflesh()
		{
			instance.refresh();
		}

		void refresh()
		{
			var j = _tail - 1;

			// 指定した部分は null に,
			// null の部分には配列内の一番後ろにある要素を代入.
			for (int i = 0; i < _array.Length; i++)
			{
				if (_array[i] == null)
				{
					while (i < j)
					{
						var fromTail = _array[j];
						if (fromTail != null)
						{
							_array[i] = fromTail;
							_array[j] = null;
							j--;
							goto NEXTLOOP;
						}
						j--;
					}

					_tail = i;
					break;
				}

				NEXTLOOP:
				continue;
			}

//			if (m_ReduceArraySizeWhenNeed && _tail < _array.Length / 2)
//				Array.Resize(ref _array, _array.Length / 2);
		}
	}



	public class UpdateDispatcher<T,T2> : MonoBehaviour where T : UpdateDispatcher<T,T2>
	{
		const int kInitialSize = 16;

		int _tail = 0;
		T2[] _array = new T2[kInitialSize];

		//		[SerializeField] bool m_ReduceArraySizeWhenNeed = false;
		//
		//		public static bool reduceArraySizeWhenNeed { get { return manager.m_ReduceArraySizeWhenNeed; } set { manager.m_ReduceArraySizeWhenNeed = value; } }

		static T instance
		{
			get
			{
				if (!_instance)
				{
					_instance = FindObjectOfType<T>();
					if (!_instance)
					{
						_instance = new GameObject("UpdateDispatcher").AddComponent<T>();
						_instance.gameObject.hideFlags = HideFlags.DontSave;
					}
				}
				return _instance;
			}
		}

		static T _instance;

		void Awake()
		{
			if (instance && _instance != this)
				Destroy(gameObject);
		}

//		void Update()
//		{
//			for (int i = 0; i < _tail; i++)
//			{
//				if (_array[i] == null)
//					continue;
//				_array[i].OnUpdate();
//			}
//		}

		/// <summary>
		/// Update 対象の追加.
		/// </summary>
		public static void Register(T2 updatable)
		{
			if (Equals(updatable, null))
				return;
			instance.register(updatable);
		}

		void register(T2 updatable)
		{
			if (_array.Length == _tail)
			{
				Array.Resize(ref _array, checked(_tail * 2));
			}
			_array[_tail++] = updatable;
		}

		/// <summary>
		/// 指定した Updatable を Update 対象から除外する.
		/// </summary>
		public static void Unregister(T2 updatable)
		{
			if (Equals(updatable, null))
				return;
			instance.unregister(updatable);
		}

		void unregister(T2 updatable)
		{
			for (int i = 0; i < _array.Length; i++)
			{
				if (Equals(_array[i], updatable))
				{
					_array[i] = default(T2);
					refresh();
					return;
				}
			}
		}

		/// <summary>
		/// 配列整理.
		/// </summary>
		public static void Reflesh()
		{
			instance.refresh();
		}

		void refresh()
		{
			var j = _tail - 1;

			// 指定した部分は null に,
			// null の部分には配列内の一番後ろにある要素を代入.
			for (int i = 0; i < _array.Length; i++)
			{
				if (_array[i] == null)
				{
					while (i < j)
					{
						var fromTail = _array[j];
						if (!Equals(fromTail, null))
						{
							_array[i] = fromTail;
							_array[j] = default(T2);
							j--;
							goto NEXTLOOP;
						}
						j--;
					}

					_tail = i;
					break;
				}

				NEXTLOOP:
				continue;
			}

			//			if (m_ReduceArraySizeWhenNeed && _tail < _array.Length / 2)
			//				Array.Resize(ref _array, _array.Length / 2);
		}
	}
}
