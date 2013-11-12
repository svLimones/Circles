using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Collections;
using System.IO;

public class Client
{
    const   int         READ_BUFFER_SIZE    = 255;
	private TcpClient   client;
	private byte[]      readBuffer          = new byte[READ_BUFFER_SIZE];
	public  ArrayList   lstUsers            = new ArrayList();
	public  string      strMessage          = string.Empty;
	public  string      res                 = String.Empty;
	private string      pUserName;

    private NetManager netManager;
    //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    public Client(NetManager _netManager)
    {
        netManager = _netManager;
    }
    //------------------------------------------
	public string Connect(string sNetIP, int iPORT_NUM, string sUserName)
	{
		try 
		{
			pUserName=sUserName;
			client = new TcpClient();
            client.Connect(new IPEndPoint(IPAddress.Parse(sNetIP), iPORT_NUM));
			client.GetStream().BeginRead(readBuffer, 0, READ_BUFFER_SIZE, new AsyncCallback(DoRead), null);
			AttemptLogin(sUserName);
            Debug.Log("Connect  Succeeded");

			return "Connection Succeeded";
		} 
		catch(Exception ex)
		{
			Debug.Log("NO Connect" + ex.ToString());
            return "Server is not active.  Please start server and try again.      " + ex.ToString();
		}
	}
    //--------------------------------
	public void AttemptLogin(string user)
	{
		SendData("CONNECT|"+ user);
	}
 
	public void SendClick(string sInfo)
	{
		SendData("CLICK|" + sInfo);
	}
 
	public void Disconnect()
	{
		SendData("DISCONNECT");
	}
 
	public void ListUsers()
	{
		SendData("REQUESTUSERS");
	}
    //--------------------------------
	private void DoRead(IAsyncResult ar)
	{ 
		int BytesRead;
		try
		{
			// Finish asynchronous read into readBuffer and return number of bytes read.
			BytesRead = client.GetStream().EndRead(ar);
			if (BytesRead < 1) 
			{
				// if no bytes were read server has close.  
				res="Disconnected";
				return;
			}
			// Convert the byte array the message was saved into, minus two for the
			// Chr(13) and Chr(10)
			strMessage = Encoding.ASCII.GetString(readBuffer, 0, BytesRead - 2);
			ProcessCommands(strMessage);
			// Start a new asynchronous read into readBuffer.
			client.GetStream().BeginRead(readBuffer, 0, READ_BUFFER_SIZE, new AsyncCallback(DoRead), null);
 
		} 
		catch
		{
			res="Disconnected";
		}
	}
	//-------------------------------------------------------
	private void ProcessCommands(string strMessage)
	{
		string[] dataArray;
 
		// Message parts are divided by "|"  Break the string into an array accordingly.
        dataArray = strMessage.Split((char) 13);
		dataArray = dataArray[0].Split((char) 124);
		
		switch( dataArray[0])
		{
			case "JOIN":
				// Server acknowledged login.
		        netManager.OnStartGame( dataArray[1] );
				res= "You have joined the chat";
				break;
            case "JOIN_AS_SPECTRACTOR":
				// Server acknowledged login.
		        netManager.StartSpectractor( dataArray[1] );
				res= "You have joined as Spectractor";
				break;
			case "CLICK":
				// Received chat message, display it.
		        netManager.OnCliclCircle_NetReport(dataArray[1]);
				res=  dataArray[1].ToString();
				break;
			case "REFUSE":
				// Server refused login with this user name, try to log in with another.
				AttemptLogin(pUserName);
				res=  "Attempted Re-Login";
				break;
			case "LISTUSERS":
				// Server sent a list of users.
				ListUsers(dataArray);
				break;
			case "BROAD":
				// Server sent a broadcast message
				res=  "ServerMessage: " + dataArray[1].ToString();
				break;
		}
	}
	//---------------------------------------
	private void SendData(string data)
	{
		StreamWriter writer = new StreamWriter(client.GetStream());
		writer.Write(data + (char) 13 + (char) 10);
		writer.Flush();
	}
    //----------------------------------------
	private void ListUsers(string[] users)
	{
		int I;
		lstUsers.Clear();
		for (I = 1; I <= (users.Length - 1); I++)
		{
			lstUsers.Add(users[I]);	
		}
	}
    //----------------------------------------

    
}