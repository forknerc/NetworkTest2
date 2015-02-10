using UnityEngine;
using System.Collections;
using System.Diagnostics;

public class networkMgr : MonoBehaviour {

	private const string typeName = "UASS_Server";
	private const string gameName = "ECSL_Lab";
	private string OperatingSystem = "N/A";
	private string path;
	private Process ServerProcess = null;
	private HostData[] hostList;


	private void StartUnityServer()
	{
		Network.InitializeServer(8, 25000, !Network.HavePublicAddress());
		MasterServer.RegisterHost(typeName, gameName);
		MasterServer.updateRate = 2;
	}

	private void StartUserServer()
	{
		MasterServer.ipAddress = "127.0.0.1";
		MasterServer.port = 23467;
		//Run Server Executable depending on OS
		if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor) {
			path += "/../";
		}
		else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor) {
			path += "/../MasterServer/VisualStudio/Debug/MasterServer";
		}

		ServerProcess = new Process();
		ServerProcess.StartInfo.FileName = path;
		ServerProcess.Start();		

		Network.InitializeServer(8, 25001, !Network.HavePublicAddress());
		MasterServer.RegisterHost(typeName,"UserGame");
	}


	public GameObject playerPrefab1;
	public GameObject playerPrefab2;

	
	void OnServerInitialized()
	{
		SpawnPlayer();
	}

	void OnGUI()
	{
		GUI.TextField(new Rect(0, 0, 200, 25), MasterServer.ipAddress + "  " + MasterServer.port);
		GUI.TextField(new Rect(250, 000, 200, 25), OperatingSystem);

		if (!Network.isClient && !Network.isServer)
		{
			if (GUI.Button(new Rect(100, 100, 200, 50), "Start Unity Server"))
				StartUnityServer();
			if (GUI.Button(new Rect(100, 150, 200, 50), "Start User Server"))
				StartUserServer();
			if (GUI.Button(new Rect(100, 200, 200, 50), "Refresh Hosts"))
				RefreshHostList();
			if (GUI.Button(new Rect(100, 250, 200, 50), "Refresh LANs"))
				RefreshLANList();




			GUI.TextField(new Rect(100, 50, 200, 25), path);

			
			if (hostList != null)
			{
				GUI.TextField(new Rect(400, 50, 100, 25), "Games");
				for (int i = 0; i < hostList.Length; i++)
				{
					if (GUI.Button(new Rect(400, 100 + (110 * i), 300, 100), hostList[i].gameName))
						JoinServer(hostList[i]);
				}
			}
		}
	}


	
	private void RefreshHostList()
	{	
		Network.Disconnect ();

		MasterServer.ipAddress = "67.225.180.24";
		MasterServer.port = 23466;
		MasterServer.RequestHostList(typeName);
	}

	private void RefreshLANList()
	{
		Network.Disconnect ();

		MasterServer.ipAddress = "127.0.0.1";
		MasterServer.port = 23467;
		MasterServer.RequestHostList(typeName);
	}

	void OnMasterServerEvent(MasterServerEvent msEvent)
	{
		if (msEvent == MasterServerEvent.HostListReceived)
			hostList = MasterServer.PollHostList();
	}

	private void JoinServer(HostData hostData)
	{
		Network.Connect(hostData);
	}
	
	void OnConnectedToServer()
	{
		SpawnPlayer();
	}

	private void SpawnPlayer()
	{
		Network.Instantiate(playerPrefab1, new Vector3(0f, 4f, 0f), Quaternion.identity, 0);
	}

	// Use this for initialization
	void Start () {	
		hostList = new HostData[0];
		Network.sendRate = 100;
		path = Application.dataPath;
		Application.runInBackground = true;
		if(Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
			OperatingSystem = "Windows";	
	}
	
	// Update is called once per frame
	void Update () {

	}

	void OnDestroy()
	{
		if(ServerProcess != null)
			ServerProcess.Kill ();
	}
}
