using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Coffee.UIExtensions
{
	public interface IParametizedTexture
	{
		int index{ get; set; }
	}


	public class ParametizedTexture
	{
		#region ILateUpdatable implementation

		public void OnLateUpdate()
		{
			Upload();
		}

		#endregion

		public Texture2D texture{ get; private set; }

		public int channelCount{ get; private set; }

		public bool needUpload{ get; private set; }

		public int maxInstanceCount{ get; private  set; }

		public byte[] data{ get; private  set; }

		public Stack<int> stack{ get; private  set; }


		public ParametizedTexture (int channel, int maxInstance)
		{
//			Debug.LogFormat("<color=red>ParametizedTexture is generated!</color> {0}, {1}, {2}", Application.isPlaying, UnityEditor.EditorApplication.isPlaying, UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode);

			channelCount = ((channel - 1) / 4 + 1) * 4;
			this.maxInstanceCount = ((maxInstance - 1) / 2 + 1) * 2;
			texture = new Texture2D (channelCount/4, maxInstanceCount, TextureFormat.RGBA32, false, false);
			texture.filterMode = FilterMode.Point;
			texture.wrapMode = TextureWrapMode.Clamp;
			data = new byte[channelCount * maxInstanceCount];

			stack = new Stack<int> (maxInstanceCount);
			for (int i = 0; i < maxInstanceCount; i++) {
				stack.Push (i);
			}

//			UpdateDispatcher.Register(this);
		}



		public void Register (IParametizedTexture target)
		{
			target.index = stack.Pop ();
			Debug.LogFormat("<color=green>Register {0} : {1}</color>", target, target.index);
		}

		public void Unregister (IParametizedTexture target)
		{
			Debug.LogFormat("<color=red>Unregister {0} : {1}</color>", target, target.index);
			if (0 <= target.index) {
				stack.Push (target.index);
				target.index = -1;
			}
		}

		public void SetData (IParametizedTexture target, int channelId, byte color)
		{
			if (0 <= target.index)
			{
				data[target.index * channelCount + channelId] = color;
				needUpload = true;
			}
		}

		public void SetData (IParametizedTexture target, int channelId, float color)
		{
			if (0 <= target.index)
			{
//				Debug.LogFormat("SetData! {0}, {1}, {2}, {3}", data.Length, target.index, channelCount, channelId);

				data[target.index * channelCount + channelId] = (byte)(color * 255);
				needUpload = true;
			}
		}

	
		public void Upload ()
		{
			if (needUpload && texture) {
//				Debug.Log("Upload " + needUpload);
//				if (!texture)
//				{
//					texture = new Texture2D (channelCount/4, maxInstanceCount, TextureFormat.RGBA32, false, false);
//					texture.filterMode = FilterMode.Point;
//					texture.wrapMode = TextureWrapMode.Clamp;
//				}


//				Debug.Log ("Upload!");
				needUpload = false;
				texture.LoadRawTextureData (data);
				texture.Apply (false, false);
			}
		}
	}
}