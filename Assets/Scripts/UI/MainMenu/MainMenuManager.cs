using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace MainMenu
{
	public class MainMenuManager : MenuManagerFactory<MainMenuManager>
	{
		[Header("References")]
		[SerializeField] private TMP_Text playButtonText;
		[SerializeField] private GameObject playPanel;

		[SerializeField] private TMP_Text playerNameText;
		[SerializeField] private TMP_InputField addFriendText;	
		
		protected override void Awake()
		{
			base.Awake();
			if (Instance != this) { return; }
			OnSwitchPage("Play");
		}

		protected override void OnSwitchPage(string _page, int _index = 0)
		{
			playPanel.SetActive(_page == "Play");
			switch (_page)
			{
				case "Play":
					break;
				case "Settings":
					break;
				case "Friends":

					break;
				case "Loadout":
					break;
			}
		}
		public void StartDevHost()
		{
			NetworkManager.Singleton.StartHost();
			NetworkManager.Singleton.SceneManager.LoadScene("PracticeTool", LoadSceneMode.Single);
		}
		public void StartDevClient()
		{
			NetworkManager.Singleton.StartClient();
		}
	}
}