using UnityEngine;
using System.Collections;
using System;

public class CircleObj : MonoBehaviour
{
    public struct CircleArg
    {
        public GameLogic.CircleID           ID;
        public float                        FinishHPosition;
        public float                        Speed;
        public float                        Radius;
        public Action<GameLogic.CircleID, int>   FinishMove_CallBack;
        public Action<GameLogic.CircleID, int>   OnClick_CallBack;
    };

    private int pool_ID = -1;
    public  int Pool_ID
    {
        get { return pool_ID; }
        set { if( pool_ID == -1 ) pool_ID = value; }
    }
    public  GameObject  Model;
    private CircleArg   args;
    private bool        isMoving = false;
    private Transform   _transform;
    //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    public void Init( CircleArg _args )
    {
        args            = _args;
        isMoving        = true;
        _transform      = transform;
        StartCoroutine( IE_Moving() );
    }
    //----------------------------------
    IEnumerator IE_Moving()
    {
        while( isMoving )
        {
            _transform.Translate( Vector3.back * args.Speed * Time.deltaTime );
            if( _transform.position.z - args.Radius <= args.FinishHPosition )
            {
                isMoving = false;
                if( args.FinishMove_CallBack!=null ) args.FinishMove_CallBack(args.ID, pool_ID);
            }
            yield return null;
        } 
    }
    //------------------------------------
    public void OnClick( GameLogic.PlayerType type )
    {
        if(!isMoving) return;
        if( type != args.ID.PlayerType ) return;
        args.OnClick_CallBack( args.ID, pool_ID );
    }
}
