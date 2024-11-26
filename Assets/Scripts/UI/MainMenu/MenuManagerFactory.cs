using UnityEngine;
namespace MainMenu
{
	public abstract class MenuManagerFactory<T> : MonoBehaviour where T : class
	{
		public static T Instance { get; private set; }
		public SwitchPage SwitchPage;
		protected string currentPage = "Menu";
		protected virtual void Awake()
		{
			if (Instance != null)
			{
				Destroy(gameObject);
				return;
			}
			Instance = this as T;
			SwitchPage += OnSwitchPage;
		}
		protected virtual void OnDestroy()
		{
			Instance = null;
			SwitchPage -= OnSwitchPage;
		}
		protected abstract void OnSwitchPage(string _page, int _index);

	}
	public delegate void SwitchPage(string _page, int _index = 0);
}