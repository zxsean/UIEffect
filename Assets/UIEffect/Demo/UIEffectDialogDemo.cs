using UnityEngine;

namespace Coffee.UIExtensions
{
	public class UIEffectDialogDemo : MonoBehaviour
	{
		[SerializeField] Animator animator; 
		[SerializeField] UIEffectCapturedImage background; 

		public void Open()
		{
			gameObject.SetActive(true);
			animator.SetTrigger("Open");
			background.Capture();
		}

		public void Close()
		{
			animator.SetTrigger("Close");
		}

		public void Closed()
		{
			gameObject.SetActive(false);
		}
	}
}