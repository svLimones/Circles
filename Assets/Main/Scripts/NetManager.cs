using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class NetManager : MonoBehaviour
{
    public enum PlayerType : int
    {
        Server = 0,
        Client = 1,
        Spectator = 2
    };
    //-------------------------
    public GameLogic GameLogic;

    private const int GAME_VERSION = 1;

    private  string      PlayerName  = "Player";
    private  PlayerType  Type        = PlayerType.Server;
    private  string      IP          = "127.0.0.1";
    private  int         Port        = 3333;

    private Server server;
    private Client client;

    private bool isShowMenu = true;
    public GameLogic GameLogicScr;

    private static NetManager instance;
    public  static NetManager Instance
    {
        get
        {
            if( instance == null )
            {
                #if DEBUG
                Debug.Log( "Instance of ResManager wasn't found. Add one to the scene." );
                #endif
                return null;
            }
            return instance;
        }
    }

    private GameLogic.PlayerInitArg arg1;
    private GameLogic.PlayerInitArg arg2;

    private string clickMsg;
    //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    public void Awake()
    {
        if( instance != null )
        {
            #if DEBUG
            Debug.Log( "There are more than one ResManager in the scene. Remove extra ones" );
            #endif
            return;
        }
        instance = this;
    }
    //------------------------------------
    void Start()
    {
        server = new Server(this);
        client = new Client(this);
        isShowMenu = true;
        GameLogicScr.OnClickCircleEvent += OnClickCircle;
        arg1 = new GameLogic.PlayerInitArg();
        arg2 = new GameLogic.PlayerInitArg();
        PlayerName += UnityEngine.Random.Range( 0, 10000 );
        //GameLogicScr.Init( arg1, arg2 );
    }
    //----------------------------
    private void StartGame( GameLogic.PlayerInitArg _Player1Arg, GameLogic.PlayerInitArg _Player2Arg )
    {
        GameLogicScr.Init( _Player1Arg, _Player2Arg, Type );
    }

    public void OnStartGame(string message)
    {
        string[] str;
        str = message.Split(':');
        arg1.RandomSeed = Convert.ToInt32( str[0] );
        arg1.PlayerType = PlayerType.Server;
        arg1.Scores     = Convert.ToInt32( str[1] );
        arg1.CurrentCirclesCount = Convert.ToInt32( str[2] );

        arg2.RandomSeed = Convert.ToInt32( str[3] );
        arg2.PlayerType = PlayerType.Client;
        arg2.Scores     = Convert.ToInt32( str[4] );
        arg2.CurrentCirclesCount = Convert.ToInt32( str[5] );

        StartGame(arg1, arg2);
    }

    public void StartGame()
    {
        arg1.RandomSeed = 1000;
        arg1.PlayerType = PlayerType.Server;
        arg1.Scores     = 0;
        arg1.CurrentCirclesCount = 0;

        arg2.RandomSeed = 2222;
        arg2.PlayerType = PlayerType.Client;
        arg2.Scores     = 0;
        arg2.CurrentCirclesCount = 0;

        StartGame(arg1, arg2);

        Debug.Log("Game is started| Seed1="+arg1.RandomSeed+" Seed2="+arg2.RandomSeed);
    }

    public void StartSpectractor(string message)
    {
        GameLogicScr.iSpectractor = true;
        Type = PlayerType.Spectator;
        OnStartGame(message);
    }

    public string GetPlayersArgs()
    {
        lock(this)
        {
            string str;
            arg1.RandomSeed             = 1000;
            arg1.Scores                 = 0;
            arg1.CurrentCirclesCount    = 0;

            arg2.RandomSeed             = 2222;
            arg2.Scores                 = 0;
            arg2.CurrentCirclesCount    = 0;

            str = arg1.RandomSeed.ToString();
            str += ":"+GameLogicScr.Player1.Scores.ToString();
            str += ":"+GameLogicScr.Player1.CurrentCirclesCount;

            str += ":"+arg2.RandomSeed.ToString();
            str += ":"+GameLogicScr.Player2.Scores.ToString();
            str += ":"+GameLogicScr.Player2.CurrentCirclesCount;

            return str;
        }
    }
    //----------------------------
    private void OnClickCircle( CircleObj.CircleID ID )
    {
        string str;
        str = ((int)(ID.PlayerType)).ToString()+":"+(ID.ID).ToString()+":";

        switch( Type )
            {
                case PlayerType.Server:
                    str += GameLogicScr.Player1.Scores.ToString();
                    server.SendClickToAll( str );
                    break;
                case PlayerType.Client:
                    str += GameLogicScr.Player2.Scores.ToString();
                    client.SendClick( str );
                    break;
            }
    }
    //--------------------------------------
    public void OnCliclCircle_NetReport( string message )
    {
        //clickMsg = message;
        CircleObj.CircleID Circle;
        string[] str;
        str = message.Split(':');

        Circle.PlayerType   = (PlayerType) (Convert.ToInt32(str[0]));
        Circle.ID           = Convert.ToInt32(str[1]);

        if(Circle.PlayerType == PlayerType.Server) GameLogicScr.Player1.Scores = Convert.ToInt32(str[2]); else
        if(Circle.PlayerType == PlayerType.Client) GameLogicScr.Player2.Scores = Convert.ToInt32(str[2]);

        GameLogicScr.DeleteCircle( Circle );
    }
    //-----------------------------------
    private int toolbarInt = 0;
	private string[] toolbarStrings = {"Server", "Client"};

    void OnGUI()
    {
        if( isShowMenu )
        {
            GUI.Window(0, new Rect(250, 50, 400, 400), WindowFunction, "Menu");
        }
        //GUI.Label( new Rect(10, 60, 200, 25), "Click="+clickMsg );
    }
    void WindowFunction (int windowID) 
    {
		GUI.Label( new Rect(50, 80, 80, 25), "Player Name:" );
        PlayerName = GUI.TextField( new Rect(130, 80, 100, 25), PlayerName );
 
		toolbarInt = GUI.Toolbar (new Rect (50, 120, 280, 25), toolbarInt, toolbarStrings);

        GUI.Label( new Rect(50, 150, 80, 30), "Server IP:" );
        IP = GUI.TextField( new Rect(130, 150, 100, 25), IP );

        GUI.Label( new Rect(50, 180, 80, 30), "Port:" );
        Port = Convert.ToInt32(GUI.TextField( new Rect(130, 180, 100, 25), Port.ToString() ));

        if( GUI.Button( new Rect( 130, 350, 80, 25 ), "Start" ) )
        {
            isShowMenu = false;
            Type = (PlayerType)(toolbarInt);
            Debug.Log( "PlayerType="+ Type );
            switch( Type )
            {
                case PlayerType.Server:
                    server.StartServer(Port, PlayerName);
                    break;
                case PlayerType.Client:
                    client.Connect(IP, Port, PlayerName);
                    break;
            }
        }
	}
    //-----------------------------------
    public void Disconnect()
    {
        switch( Type )
        {
            case PlayerType.Server:
                server.StopServer();
                break;
            case PlayerType.Client: 
                client.Disconnect();
                break;
            case PlayerType.Spectator: 
                client.Disconnect();
                break;
        }
    }
    //-----------------------------------------
    void OnApplicationQuit()
    {
        GameLogicScr.OnClickCircleEvent -= OnClickCircle;
        Disconnect();
    }
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private class DelegateSrtArg
    {
        public string Arg;
        public Anonymous2 Delegate;
    };

    public delegate void Anonymous ();
    public delegate void Anonymous2 (string str);
    List<Anonymous> callbacks = new List<Anonymous> ();
    List<DelegateSrtArg> callbacks2 = new List<DelegateSrtArg> ();

    public void Update ()
    {
    Anonymous[] c;
    DelegateSrtArg[] c2;
    lock(this) {
    c = callbacks.ToArray();
    callbacks.Clear();  
    c2 = callbacks2.ToArray();
    callbacks.Clear(); 
    callbacks2.Clear();  
    }
    foreach(var i in c) i();
    foreach(var i in c2) i.Delegate(i.Arg);
    }
 
    public void ScheduleCallback(Anonymous fn) {
        lock(this) {
            callbacks.Add(fn);
        }
    }

    public void ScheduleCallback2(Anonymous2 fn, string str) {
        lock(this) {
            callbacks2.Add(new DelegateSrtArg(){ Arg = str, Delegate = fn});
        }
    }
}