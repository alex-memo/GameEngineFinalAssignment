using System.Net.Sockets;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// @alex-memo 2023
/// This class is responsible for 
/// </summary>
public class ImmediateHost : MonoBehaviour
{
	private void Start()
	{
		if (checkLocalHostPort())
		{
			NetworkManager.Singleton.StartHost();
		}
		else
		{
			NetworkManager.Singleton.StartClient();
		}
	}
	private bool checkLocalHostPort()
	{
		try
		{
			TcpListener _listener = new(System.Net.IPAddress.Loopback, 7777);
			_listener.Start();
			return true;
		}
		catch
		{
			return false;
		}

	}
}