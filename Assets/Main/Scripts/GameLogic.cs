using UnityEngine;
using System.Collections;
using System;

public class GameLogic : MonoBehaviour
{
    
    [Serializable]
    public class PlayerInitArg
    {
        public int         CurrentCirclesCount;
        public NetManager.PlayerType  PlayerType;
        public int         Scores;
        public int         RandomSeed;
    };
    [Serializable]
    public class PlayerArg
    {
        public int              CurrentCirclesCount;
        public NetManager.PlayerType PlayerType;
        public int              Scores;
        public int              RandomSeed;
        public float            FinishHPosition;
        public Action<CircleObj.CircleID, int>          FinishMove_CallBack;
        public Action<CircleObj.CircleID, int, float>   OnClick_CallBack;
        public System.Random    Random;
        public Vector3          CircleSpawnStartPositionLeft;   
        public Vector3          CircleSpawnStartPositionRight; 
    };
    //--------------------------------------------
    public PlayerArg   Player1;
    public PlayerArg   Player2;
    private NetManager.PlayerType currentPlayer;

    public Vector3     Player1CircleSpawnStartPositionLeft;   
    public Vector3     Player1CircleSpawnStartPositionRight; 
    public Vector3     Player2CircleSpawnStartPositionLeft;   
    public Vector3     Player2CircleSpawnStartPositionRight; 
    public float       CircleMoveFinishHPosition;
    public float       MinCircleSize       = 0.1f;
    public float       MaxCircleSize       = 1f;
    public float       MinCircleSpeed      = 0.1f;
    public float       MaxCircleSpeed      = 1f;
    public float       SpeedGeneration     = 2f;
    public int         CircleLayer         = 8;
    public AudioClip   BubbleSound;
    public AudioSource BubbleSource;

    public delegate void OnClickDelegate( CircleObj.CircleID id );
    public event OnClickDelegate OnClickCircleEvent;

    private const string SOUND_BUNDLE_URL = @"dl.dropboxusercontent.com/u/36321526/Circles/BubbleSound.unity3d";

    private bool                isGameStarted = false;

    private ResManager.TextureSize  _textSize;
    private CircleObj               _circle;
    private Ray                     __ray;
    private RaycastHit              __hit;
    private CircleObj.CircleArg     _circleArg;

    public bool iSpectractor = false;
    //==================================
    public IEnumerator Start()
    {
        Player1 = new PlayerArg();
        Player2 = new PlayerArg();

        using( WWW www = new WWW( SOUND_BUNDLE_URL ) )
        {
            yield return www;
            BubbleSound = Instantiate(www.assetBundle.mainAsset) as AudioClip;
        }

        
    }
    //---------------------------------
    public void Init( PlayerInitArg _Player1Arg, PlayerInitArg _Player2Arg, NetManager.PlayerType _currentPlayer)
    {
        Player1 = new PlayerArg();
        Player2 = new PlayerArg();
        currentPlayer = _currentPlayer;

        Player1.CurrentCirclesCount              = _Player1Arg.CurrentCirclesCount;
        Player1.PlayerType                       = _Player1Arg.PlayerType;
        Player1.Scores                           = _Player1Arg.Scores;
        Player1.RandomSeed                       = _Player1Arg.RandomSeed;
        Player1.FinishHPosition                  = CircleMoveFinishHPosition;
        Player1.FinishMove_CallBack              = OnFinishMoveCircle;
        Player1.OnClick_CallBack                 = OnClickCircle;
        Player1.Random                           = new System.Random(Player1.RandomSeed);
        Player1.CircleSpawnStartPositionLeft     = Player1CircleSpawnStartPositionLeft;
        Player1.CircleSpawnStartPositionRight    = Player1CircleSpawnStartPositionRight;

        Player2.CurrentCirclesCount              = _Player2Arg.CurrentCirclesCount;
        Player2.PlayerType                       = _Player2Arg.PlayerType;
        Player2.Scores                           = _Player2Arg.Scores;
        Player2.RandomSeed                       = _Player2Arg.RandomSeed;
        Player2.FinishHPosition                  = CircleMoveFinishHPosition;
        Player2.FinishMove_CallBack              = OnFinishMoveCircle;
        Player2.OnClick_CallBack                 = OnClickCircle;
        Player2.Random                           = new System.Random(Player2.RandomSeed);
        Player2.CircleSpawnStartPositionLeft     = Player2CircleSpawnStartPositionLeft;
        Player2.CircleSpawnStartPositionRight    = Player2CircleSpawnStartPositionRight;
        
        int i = 0;
        if( Player1.CurrentCirclesCount > 0 )
        {
            i = 0;
            while( i++ < Player1.CurrentCirclesCount )
            {
                Player1.Random.NextDouble();
                Player1.Random.NextDouble();
            }
        }
        if( Player2.CurrentCirclesCount > 0 )
        {
            i = 0;
            while( i++ < Player2.CurrentCirclesCount )
            {
                Player2.Random.NextDouble();
                Player2.Random.NextDouble();
            }
        }

        isGameStarted = true;
        StartCoroutine( IE_CircleGeneration() );
    }
    //-----------------------------------------
    private IEnumerator IE_CircleGeneration()
    {
        while( isGameStarted )
        {
            InstantiateCircle( Player1 );
            Player1.CurrentCirclesCount++;
            InstantiateCircle( Player2 );
            Player2.CurrentCirclesCount++;
            yield return new WaitForSeconds(1f/SpeedGeneration);
        }
    }
    //--------------------------------
    private void InstantiateCircle( PlayerArg player)
    {
        _circleArg.CircleID.ID          = player.CurrentCirclesCount;
        _circleArg.CircleID.PlayerType  = player.PlayerType;
        _circleArg.FinishMove_CallBack  = player.FinishMove_CallBack;
        _circleArg.OnClick_CallBack     = player.OnClick_CallBack;

        float _diameter = (float)player.Random.NextDouble();
        _diameter *= (MaxCircleSize-MinCircleSize);
        _diameter += MinCircleSize;
        
        if( MaxCircleSize/_diameter <= 1f/4f ) _textSize = ResManager.TextureSize.s32; 
        else
        if( MaxCircleSize/_diameter <= 2f/4f ) _textSize = ResManager.TextureSize.s64;
        else
        if( MaxCircleSize/_diameter <= 3f/4f ) _textSize = ResManager.TextureSize.s128;
        else
        _textSize = ResManager.TextureSize.s256;

        _circle = ResManager.Instance.GetCircle( _textSize );
        _circle.transform.localScale = new Vector3(_diameter, _diameter, _diameter);

        float _pos = (float)player.Random.NextDouble();
        _pos *= (player.CircleSpawnStartPositionRight.x-player.CircleSpawnStartPositionLeft.x);
        _pos += player.CircleSpawnStartPositionLeft.x;
        
        _circle.transform.position = new Vector3(_pos, player.CircleSpawnStartPositionLeft.y, player.CircleSpawnStartPositionLeft.z-_diameter/2);
        _circleArg.Radius = _diameter/2;
        _circleArg.Speed  = MinCircleSize+(MaxCircleSize-_diameter)*((MaxCircleSpeed-MinCircleSpeed)/(MaxCircleSize-MinCircleSize));
        
        _circle.Init( _circleArg );

    }
    //------------------------------------
    public void DeleteCircle(CircleObj.CircleID ID)
    {
        if( ID.PlayerType != currentPlayer )
        {
            ResManager.Instance.PutBackCircle( ID );
        }
    }

    public void OnClickCircle( CircleObj.CircleID ID, int poolID, float circleSize )
    {
        if( ID.PlayerType == currentPlayer )
        {
            if(currentPlayer == Player1.PlayerType) Player1.Scores += (int)( 10f/circleSize );
                                               else Player2.Scores += (int)( 10f/circleSize );
            if(BubbleSound!=null) BubbleSource.PlayOneShot( BubbleSound );
            if( OnClickCircleEvent != null )
            {
                OnClickCircleEvent(ID);
            }
        }
        ResManager.Instance.PutBackCircle( poolID );
    }

    public void OnFinishMoveCircle( CircleObj.CircleID ID, int poolID )
    {
        ResManager.Instance.PutBackCircle( poolID );
    }
    //---------------------------------------
    private Rect rect1 = new Rect(10, 5, 150, 25);
    private Rect rect2 = new Rect(Screen.width-160, 5, 150, 25);

    void OnGUI()
    {
        GUI.Label( rect1, "Player1 Scores:"+Player1.Scores );
        GUI.Label( new Rect(10, 35, 150, 25), "Seed1="+Player1.RandomSeed+" Seed2="+Player2.RandomSeed );
        GUI.Label( rect2, "Player2 Scores:"+Player2.Scores );
    }
    //-----------------------------------------
    private void Update()
    {
        if( isGameStarted && !iSpectractor)
        {
            if(Input.GetMouseButtonUp(0))
            {
                __ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if(Physics.Raycast(__ray, out __hit, 100, (1<<CircleLayer)))
                {
                    __hit.transform.gameObject.SendMessage( "OnClick", currentPlayer );       
                }
            }
        }
    }
}
