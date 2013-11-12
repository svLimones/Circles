using UnityEngine;
using System.Collections;
using System;

public class CircleObj : MonoBehaviour
{
    public struct CircleID
    {
        public int                  ID; //local id for player
        public NetManager.PlayerType PlayerType;
    };

    public struct CircleArg
    {
        public CircleID             CircleID;
        public float                FinishHPosition;
        public float                Speed;
        public float                Radius;
        public Action<CircleID, int>          FinishMove_CallBack;
        public Action<CircleID, int, float>   OnClick_CallBack;
    };

    private int pool_ID = -1; //global pool id
    public  int Pool_ID
    {
        get { return pool_ID; }
        set { if( pool_ID == -1 ) pool_ID = value; }
    }
    public  GameObject  Model;
    public  CircleArg   Args;
    private bool        isMoving = false;
    private Transform   _transform;
    //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    public void Init( CircleArg _args )
    {
        Args            = _args;
        isMoving        = true;
        _transform      = transform;
        StartCoroutine( IE_Moving() );
    }
    //----------------------------------
    IEnumerator IE_Moving()
    {
        while( isMoving )
        {
            _transform.Translate( Vector3.back * Args.Speed * Time.deltaTime );
            if( _transform.position.z - Args.Radius <= Args.FinishHPosition )
            {
                StopMoving();
                if( Args.FinishMove_CallBack!=null ) Args.FinishMove_CallBack(Args.CircleID, pool_ID);
            }
            yield return null;
        } 
    }
    //------------------------------------
    public void StopMoving()
    {
        isMoving = false;
    }
    //------------------------------------
    public void OnClick( NetManager.PlayerType type )
    {
        if(!isMoving) return;
        if( type != Args.CircleID.PlayerType ) return;
        Args.OnClick_CallBack( Args.CircleID, pool_ID, transform.localScale.x);
    }
}
