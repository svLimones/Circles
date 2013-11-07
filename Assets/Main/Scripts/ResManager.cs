using UnityEngine;
using System.Collections.Generic;

public class ResManager : MonoBehaviour
{
    public enum TextureSize
    {
        s32     = 32,
        s64     = 64,
        s128    = 128,
        s256    = 256
    };

    public  int                 CountCircles  = 100;
    public  GameObject          CircleModel;
    public  int                 CircleLayer   = 8;

    private GameObject[]        circlesPool;
    private Dictionary<TextureSize, Texture2D[]> texturesPool;
    private int                 numCircles      = 0;

    private Vector3 PoolPos = new Vector3(100, 100, 100);
    private static ResManager instance;
    public  static ResManager Instance
    {
        get
        {
            if (instance == null)
            {
                #if DEBUG
                Debug.Log("Instance of ResManager wasn't found. Add one to the scene.");
                #endif
                return null;
            }
            return instance;
        }
    }
    //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    public void Awake()
    {
        if (instance != null)
        {
            #if DEBUG
            Debug.Log("There are more than one ResManager in the scene. Remove extra ones");
            #endif
            return;
        }
        instance = this;
        CreatPool();
    }
    //------------------------------------
    private void CreatPool()
    {
        GameObject _circleModel;
        Texture2D  _texture;
        circlesPool     = new GameObject[CountCircles];
        texturesPool    = new Dictionary<TextureSize, Texture2D[]>();
        texturesPool.Add( TextureSize.s32,  new Texture2D[CountCircles] );
        texturesPool.Add( TextureSize.s64,  new Texture2D[CountCircles] );
        texturesPool.Add( TextureSize.s128, new Texture2D[CountCircles] );
        texturesPool.Add( TextureSize.s256, new Texture2D[CountCircles] );

        for (int i = 0; i < CountCircles; i++)
        {
            _circleModel = Instantiate( CircleModel, PoolPos, Quaternion.identity ) as GameObject;
            //_circleModel.layer = CircleLayer;
            //_circleModel.AddComponent<BoxCollider>();
            //_circleModel.AddComponent<Rigidbody>();
            //_circleModel.rigidbody.isKinematic = true;
            //_circleModel.rigidbody.useGravity  = false;
            _circleModel.AddComponent<CircleObj>();
            _circleModel.GetComponent<CircleObj>().Model    = _circleModel.GetComponentInChildren<Renderer>().gameObject;
            _circleModel.GetComponent<CircleObj>().Pool_ID  = i;
            circlesPool[i] = _circleModel;

            _texture = new Texture2D(32, 32);
            texturesPool[TextureSize.s32][i] = _texture;
            _texture = new Texture2D(64, 64);
            texturesPool[TextureSize.s64][i] = _texture;
            _texture = new Texture2D(128, 128);
            texturesPool[TextureSize.s128][i] = _texture;
            _texture = new Texture2D(256, 256);
            texturesPool[TextureSize.s256][i] = _texture;
        }
    }
    //-----------------------------------------
    private void GenerateTexture( TextureSize size, int num )
    {
        
    }
    //------------------------------------------
    public CircleObj GetCircle( TextureSize textureSize )
    {
        CircleObj obj;

        if( ++numCircles >= CountCircles ) numCircles = 0;
        GenerateTexture( textureSize, numCircles );
        obj = circlesPool[numCircles].GetComponent<CircleObj>();
        obj.Model.renderer.material.mainTexture = texturesPool[textureSize][numCircles];
        
        return obj;
    }

    public void PutBackCircle( int _pool_ID )
    {
        circlesPool[_pool_ID].transform.position = PoolPos;
    }
}
