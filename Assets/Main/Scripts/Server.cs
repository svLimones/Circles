using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server
{
    private string      playerName;
    private int         PORT_NUM = 3333;
    private Hashtable   clients = new Hashtable();
    private TcpListener listener;

    private NetManager netManager;
    private bool isTwoPlayers = false;
    //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    public Server(NetManager _netManager)
    {
        netManager = _netManager;
        isTwoPlayers = false;
    }

    ~Server()
    {
        if (listener != null)
        {
            listener.Stop();
        }
    }
    //---------------------------------
    private void Broadcast(string strMessage)
	{
		UserConnection client;

        foreach(DictionaryEntry entry in clients)
		{
            client = (UserConnection) entry.Value;
            client.SendData(strMessage);
        }
    }
    //-----------------------------------------------
    private void ConnectUser(string userName, UserConnection sender)
	{
		if (clients.Contains(userName))
		{
			ReplyToSender("REFUSE", sender);
		}
		else 
		{
			sender.Name = userName;
			UpdateStatus(userName + " has joined the chat.");
			clients.Add(userName, sender);

            if( !isTwoPlayers )
            {
                ReplyToSender("JOIN|"+netManager.GetPlayersArgs(), sender);
                isTwoPlayers = true;
                netManager.StartGame();
            }
            else
            {
                ReplyToSender("JOIN_AS_SPECTRACTOR|"+netManager.GetPlayersArgs(), sender);
            }
		}
    }
    //------------------------------------------
    private void DisconnectUser(UserConnection sender)
	{
        UpdateStatus(sender.Name + " has left the chat.");
        clients.Remove(sender.Name);
    }
    //---------------------------------------------
    private void DoListen()
	{
        try {
            listener = new TcpListener(IPAddress.Any, PORT_NUM);
            listener.Start();

            AcceptConnection();

        } 
		catch(Exception ex){
			Debug.Log("NO Connect " + ex.ToString());
        }
    }

    private void AcceptConnection()
    {
        listener.BeginAcceptTcpClient( 
                    new AsyncCallback(AcceptCallback),
                    listener );
    }
    //--------------------------------------------------
    private void AcceptCallback(IAsyncResult ar)
    {
        AcceptConnection();
        TcpListener listener = (TcpListener) ar.AsyncState;
        UserConnection client = new UserConnection(listener.EndAcceptTcpClient(ar));
		client.LineReceived += new LineReceive(OnLineReceived);
    }
    //---------------------------------
    public void StopServer()
    {
        if(listener!=null) listener.Stop();
    }
    //----------------------------------
    public void StartServer(int port, string _playerName)
    {
        playerName      = _playerName;
        PORT_NUM        = port;
        DoListen();
        UpdateStatus("Listener started");
    }
    //-----------------------------------------------
    private void ListUsers(UserConnection sender)
	{
        UserConnection client;       
        string strUserList;
        UpdateStatus("Sending " + sender.Name + " a list of users online.");
        strUserList = "LISTUSERS";
 
        foreach(DictionaryEntry entry in clients)
		{
            client = (UserConnection) entry.Value;
            strUserList = strUserList + "|" + client.Name;
        }

        ReplyToSender(strUserList, sender);
    }
 
    //-------------------------------------------------
    private void OnLineReceived(UserConnection sender, string data)
	{
        string[] dataArray;
		dataArray = data.Split((char) 13);
        dataArray = dataArray[0].Split((char) 124);
 
        switch( dataArray[0])
		{
            case "CONNECT":
                ConnectUser(dataArray[1], sender);
				break;
            case "CLICK":
		        netManager.OnCliclCircle_NetReport(dataArray[1]);
                SendChat(dataArray[1], sender);
				break;
            case "DISCONNECT":
                DisconnectUser(sender);
				break;
            case "REQUESTUSERS":
                ListUsers(sender);
				break;
            default: 

				break;
        }
    }
    //--------------------------------------------
    private void ReplyToSender(string strMessage, UserConnection sender)
	{
        sender.SendData(strMessage);
    }
    //-------------------------------------------
    private void SendChat(string message, UserConnection sender)
	{
        UpdateStatus(sender.Name + ": " + message);
        SendToClients("CLICK|" + message, sender);
    }
    //----------------------------------------
    private void SendToClients(string strMessage, UserConnection sender)
	{
        UserConnection client;

        foreach(DictionaryEntry entry in clients)
		{
            client = (UserConnection) entry.Value;
            
            if (client.Name != sender.Name)
			{
                client.SendData(strMessage);
            }
        }
    }
    //---------------------------------------------
    public void SendClickToAll(string message)
    {
        Broadcast("CLICK|"+message);
    }
    //----------------------------------------
    private void UpdateStatus(string statusMessage)
	{
        Debug.Log(statusMessage);
    }
}