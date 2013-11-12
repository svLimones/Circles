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

    public  int                 PoolLength  = 100;//Max number of circles on screen
    public  GameObject          CircleModel;
    public  Material            CircleMaterial;
    public  int                 CircleLayer = 8;

    private CircleObj[]         circlesPool;
    private Material[]          materialsPool;
    private Dictionary<TextureSize, Texture2D[]> texturesPool;
    private Vector3             PoolPos         = new Vector3(100, 100, 100);
    private int                 numCircles      = 0;//The current number of circles

    private Color[] _pix32;//spike 
    private Color[] _pix64;
    private Color[] _pix128;
    private Color[] _pix256;

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
        circlesPool     = new CircleObj[PoolLength];
        texturesPool    = new Dictionary<TextureSize, Texture2D[]>();
        texturesPool.Add( TextureSize.s32,  new Texture2D[PoolLength] );
        texturesPool.Add( TextureSize.s64,  new Texture2D[PoolLength] );
        texturesPool.Add( TextureSize.s128, new Texture2D[PoolLength] );
        texturesPool.Add( TextureSize.s256, new Texture2D[PoolLength] );

        _pix32  = new Color[32*32];
        _pix64  = new Color[64*64];
        _pix128 = new Color[128*128];
        _pix256 = new Color[256*256];

        materialsPool = new Material[PoolLength];

        for (int i = 0; i < PoolLength; i++)
        {
            _circleModel = Instantiate( CircleModel, PoolPos, Quaternion.identity ) as GameObject;
            _circleModel.AddComponent<CircleObj>();
            _circleModel.GetComponent<CircleObj>().Model    = _circleModel.GetComponentInChildren<Renderer>().gameObject;
            _circleModel.GetComponent<CircleObj>().Pool_ID  = i;
            circlesPool[i] = _circleModel.GetComponent<CircleObj>();

            _texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            texturesPool[TextureSize.s32][i] = _texture;
            _texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            texturesPool[TextureSize.s64][i] = _texture;
            _texture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            texturesPool[TextureSize.s128][i] = _texture;
            _texture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            texturesPool[TextureSize.s256][i] = _texture;

            materialsPool[i] = CircleMaterial;
        }
    }
    //-----------------------------------------
    private void GenerateTexture( TextureSize size, int num )
    {
        Color _color = new Color(0f,0f,0f,1f);
        Color[] _pix = new Color[0];
        int y = 0;
        int x = 0;
        int h = texturesPool[size][num].height;
        int w = texturesPool[size][num].width;

        switch( size )
        {
            case TextureSize.s32:
                _pix = _pix32;
                break;
            case TextureSize.s64:
                _pix = _pix64;
                break;
            case TextureSize.s128:
                _pix = _pix128;
                break;
            case TextureSize.s256:
                _pix = _pix256;
                break;
        }

        while (y < h) {
            x = 0;
            while (x < w)
            {
                _color.r = Random.Range( 0f, 1f );
                _color.g = Random.Range( 0f, 1f );
                _color.b = Random.Range( 0f, 1f );
                _pix[y*w + x] = _color;
                x++;
            }
            y++;
        }
        texturesPool[size][num].SetPixels(_pix);
        texturesPool[size][num].Apply();
        
    }
    //------------------------------------------
    public CircleObj GetCircle( TextureSize textureSize )
    {
        CircleObj obj;
        Color color1 = new Color(0f,0f,0f,0.6f);
        Color color2 = new Color(0f,0f,0f,0.6f);
        Color color3 = new Color(0f,0f,0f,0.6f);

        if( ++numCircles >= PoolLength ) numCircles = 0;
        obj = circlesPool[numCircles];
        obj.Model.renderer.material = materialsPool[numCircles];
        obj.Model.renderer.material.mainTexture = texturesPool[textureSize][numCircles];
        GenerateTexture( textureSize, numCircles );

        color1.r = Random.Range( 0f, 1f );
        color1.g = Random.Range( 0f, 1f );
        color1.b = Random.Range( 0f, 1f );

        color2.r = Random.Range( 0f, 1f );
        color2.g = Random.Range( 0f, 1f );
        color2.b = Random.Range( 0f, 1f );

        color3.r = Random.Range( 0f, 1f );
        color3.g = Random.Range( 0f, 1f );
        color3.b = Random.Range( 0f, 1f );

        obj.Model.renderer.material.SetColor( "_Color1", color1);
        obj.Model.renderer.material.SetColor( "_Color2", color2);
        obj.Model.renderer.material.SetColor( "_Color3", color3);

        return obj;
    }

    public void PutBackCircle( int _pool_ID )
    {
        circlesPool[_pool_ID].StopMoving();
        circlesPool[_pool_ID].transform.position = PoolPos;
    }

    public void PutBackCircle( CircleObj.CircleID ID )
    {
        bool b = false;
        for( int i = 0; i < PoolLength && !b; i++ )
        {
            if( circlesPool[i].Args.CircleID.PlayerType == ID.PlayerType &&  circlesPool[i].Args.CircleID.ID == ID.ID )
            {
                b = true;
                PutBackCircle( i );
            }
        }
    }
}
