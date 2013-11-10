using UnityEngine;
using System.Collections;
using System;

public class GameLogic : MonoBehaviour
{
    public enum PlayerType
    {
        Server,
        Client,
        Spectator
    };
    [Serializable]
    public class PlayerInitArg
    {
        public int         CurrentCirclesCount;
        public PlayerType  PlayerType;
        public int         Scores;
        public int         RandomSeed;
    };
    [Serializable]
    public class PlayerArg
    {
        public int              CurrentCirclesCount;
        public PlayerType       PlayerType;
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
    private PlayerArg   Player1;
    private PlayerArg   Player2;

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

    private const string SOUND_BUNDLE_URL = @"dl.dropboxusercontent.com/u/36321526/Circles/BubbleSound.unity3d";

    private bool                isGameStarted = false;

    private ResManager.TextureSize  _textSize;
    private CircleObj               _circle;
    private Ray                     __ray;
    private RaycastHit              __hit;
    private CircleObj.CircleArg     _circleArg;
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

        Init( new PlayerInitArg {PlayerType = PlayerType.Server, CurrentCirclesCount = 0, RandomSeed = 100, Scores = 0},
              new PlayerInitArg {PlayerType = PlayerType.Client, CurrentCirclesCount = 0, RandomSeed = 100, Scores = 0} );
    }
    //---------------------------------
    public void Init( PlayerInitArg _Player1Arg, PlayerInitArg _Player2Arg)
    {
        Player1 = new PlayerArg();
        Player2 = new PlayerArg();

        Player1.CurrentCirclesCount              = _Player1Arg.CurrentCirclesCount;
        Player1.PlayerType                       = _Player1Arg.PlayerType;
        Player1.Scores                           = _Player1Arg.Scores;
        Player1.RandomSeed                       = _Player1Arg.RandomSeed;
        Player1.FinishHPosition                  = CircleMoveFinishHPosition;
        Player1.FinishMove_CallBack              = FinishMove_CallBack;
        Player1.OnClick_CallBack                 = OnClick_CallBack;
        Player1.Random                           = new System.Random(Player1.RandomSeed);
        Player1.CircleSpawnStartPositionLeft     = Player1CircleSpawnStartPositionLeft;
        Player1.CircleSpawnStartPositionRight    = Player1CircleSpawnStartPositionRight;

        Player2.CurrentCirclesCount              = _Player2Arg.CurrentCirclesCount;
        Player2.PlayerType                       = _Player2Arg.PlayerType;
        Player2.Scores                           = _Player2Arg.Scores;
        Player2.RandomSeed                       = _Player2Arg.RandomSeed;
        Player2.FinishHPosition                  = CircleMoveFinishHPosition;
        Player2.FinishMove_CallBack              = FinishMove_CallBack;
        Player2.OnClick_CallBack                 = OnClick_CallBack;
        Player2.Random                           = new System.Random(Player2.RandomSeed);
        Player2.CircleSpawnStartPositionLeft     = Player2CircleSpawnStartPositionLeft;
        Player2.CircleSpawnStartPositionRight    = Player2CircleSpawnStartPositionRight;
        
        isGameStarted = true;
        StartCoroutine( IE_CircleGeneration() );
    }
    //------------------------------------
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
    public void OnClick_CallBack( CircleObj.CircleID ID, int poolID, float circleSize )
    {
        if( ID.PlayerType == Player1.PlayerType )
        {
            Player1.Scores += (int)( 10f/circleSize );
            if(BubbleSound!=null) BubbleSource.PlayOneShot( BubbleSound );
        }
        ResManager.Instance.PutBackCircle( poolID );
    }

    public void FinishMove_CallBack( CircleObj.CircleID ID, int poolID )
    {
        ResManager.Instance.PutBackCircle( poolID );
    }
    //---------------------------------------
    private Rect rect1 = new Rect(10, 5, 150, 50);
    private Rect rect2 = new Rect(Screen.width-160, 5, 150, 50);

    void OnGUI()
    {
        GUI.Label( rect1, "Player1 Scores:"+Player1.Scores );
        GUI.Label( rect2, "Player2 Scores:"+Player2.Scores );
    }
    //-----------------------------------------
    private void Update()
    {
        if( isGameStarted )
        {
            if(Input.GetMouseButtonUp(0))
            {
                __ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if(Physics.Raycast(__ray, out __hit, 100, (1<<CircleLayer)))
                {
                    __hit.transform.gameObject.SendMessage( "OnClick", Player1.PlayerType );       
                }
            }
        }
    }
}
