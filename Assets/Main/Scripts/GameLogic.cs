using UnityEngine;
using System.Collections;

public class GameLogic : MonoBehaviour
{
    public enum PlayerType
    {
        Server,
        Client,
        Spectator
    };

    public struct CircleID
    {
        public int          ID;
        public PlayerType   PlayerType;
    };

    public  Vector3     StartPositionLeft   = new Vector3(0, 0, 0);
    public  Vector3     StartPositionRight  = new Vector3(10f, 0, 0);
    public  float       FinishHPosition     = 10f;
    public  float       MinCircleSize       = 0.1f;
    public  float       MaxCircleSize       = 1f;
    public  float       MinCircleSpeed      = 0.1f;
    public  float       MaxCircleSpeed      = 1f;
    public  float       SpeedGeneration     = 2f;
    public  int         CircleLayer         = 8;

    private CircleID            MyPlayer;
    private int                 player1RandomSeed;
    private int                 Player2RandomSeed;
    private System.Random       random;
    private CircleObj.CircleArg circleArg;
    private bool                isGameStarted = false;

    private ResManager.TextureSize  _textSize;
    private CircleObj               _circle;
    private Ray                     __ray;
    private RaycastHit              __hit;
    //==================================
    public void Start()
    {
        MyPlayer.ID                     = 0;
        MyPlayer.PlayerType             = PlayerType.Server;
        player1RandomSeed               = 100;
        random                          = new System.Random(player1RandomSeed);
        circleArg.FinishHPosition       = FinishHPosition;
        circleArg.FinishMove_CallBack   = FinishMove_CallBack;
        circleArg.OnClick_CallBack      = OnClick_CallBack;
        isGameStarted                   = true;
        StartCoroutine( IE_CircleGeneration() );
    }
    //---------------------------------
    private IEnumerator IE_CircleGeneration()
    {
        while( isGameStarted )
        {
            InstantiateCircle( MyPlayer );
            MyPlayer.ID++;
            yield return new WaitForSeconds(1f/SpeedGeneration);
        }
    }
    //--------------------------------
    private void InstantiateCircle( CircleID circle)
    {
        circleArg.ID = circle;
        float _diameter = (float)random.NextDouble();
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

        float _pos = (float)random.NextDouble();
        _pos *= (StartPositionRight.x-StartPositionLeft.x);
        _pos += StartPositionLeft.x;
        
        _circle.transform.position = new Vector3(_pos, StartPositionLeft.y, StartPositionLeft.z-_diameter/2);
        circleArg.Radius = _diameter/2;
        circleArg.Speed  = MinCircleSize+(MaxCircleSize-_diameter)*((MaxCircleSpeed-MinCircleSpeed)/(MaxCircleSize-MinCircleSize));
        
        _circle.Init( circleArg );

    }
    //------------------------------------
    public void OnClick_CallBack( CircleID ID, int _pool_ID )
    {
        if( ID.PlayerType == MyPlayer.PlayerType )
        {
            
        }
        ResManager.Instance.PutBackCircle( _pool_ID );
    }

    public void FinishMove_CallBack( CircleID ID, int _pool_ID )
    {
        ResManager.Instance.PutBackCircle( _pool_ID );
    }
    //---------------------------------------
    private void Update()
    {
        if( isGameStarted )
        {
            if(Input.GetMouseButtonUp(0))
            {
                __ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if(Physics.Raycast(__ray, out __hit, 100, (1<<CircleLayer)))
                {
                    __hit.transform.gameObject.SendMessage( "OnClick", MyPlayer.PlayerType );
                }
            }
        }
    }
}
