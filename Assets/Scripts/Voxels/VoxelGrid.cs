using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelGrid : MonoBehaviour
{
    public GameObject voxelPrefab;
    private Vector3 origin;
    [SerializeField]
    private float density, length, width, depth;
    [SerializeField]
    private float totalMass;
    private float voxelLen;
    private GameObject[][][] voxels;
    public List<Voxel> surfaceVoxels, interiorVoxels;

    void Awake()
    {
        surfaceVoxels = new List<Voxel>();
        interiorVoxels = new List<Voxel>();
        origin = transform.position;
        buildGrid();
        typeVoxels();
        centerVoxels();
        //connectVoxels();
        //distributeMass();
    }

    private void centerVoxels(){
        Vector3 avg=Vector3.zero;
        int count=0;
        for (int i = 0; i < voxels.Length; i++)
        {
            for (int j = 0; j < voxels[i].Length; j++)
            {
                for (int k = 0; k < voxels[i][j].Length; k++)
                {
                    avg+=voxels[i][j][k].transform.position;
                    count++;
                }
            }
        }
        avg/=count;
        for (int i = 0; i < voxels.Length; i++)
        {
            for (int j = 0; j < voxels[i].Length; j++)
            {
                for (int k = 0; k < voxels[i][j].Length; k++)
                {
                    voxels[i][j][k].transform.Translate(-avg+transform.position,Space.World);
                }
            }
        }
    }
    private void buildGrid()
    {
        voxelLen = Mathf.Pow(((length * width * depth) / density), 1.0f / 3.0f);
        voxels = new GameObject[Mathf.CeilToInt(length / voxelLen)][][];
        for (int i = 0; i < voxels.Length; i++) voxels[i] = new GameObject[Mathf.CeilToInt(width / voxelLen)][];
        for (int i = 0; i < voxels.Length; i++)
            for (int j = 0; j < voxels[i].Length; j++)
                voxels[i][j] = new GameObject[Mathf.CeilToInt(depth / voxelLen)];

        //create and place voxels in the grid
        for (int i = 0; i < voxels.Length; i++)
        {
            for (int j = 0; j < voxels[i].Length; j++)
            {
                for (int k = 0; k < voxels[i][j].Length; k++)
                {
                    voxels[i][j][k] = Instantiate(voxelPrefab,
                     new Vector3((i + 0.5f) * voxelLen, (j + 0.5f) * voxelLen, (k + 0.5f) * voxelLen) + origin
                    , Quaternion.identity, transform);
                    voxels[i][j][k].transform.localScale = new Vector3(voxelLen, voxelLen, voxelLen);
                    voxels[i][j][k].GetComponent<Voxel>().coords = new Vector3Int(i, j, k);
                    voxels[i][j][k].GetComponent<Voxel>().grid = this;
                    voxels[i][j][k].GetComponent<Voxel>().init();
                }
            }
        }
    }

    private void typeVoxels()
    {
        for (int i = 0; i < voxels.Length; i++)
        {
            for (int j = 0; j < voxels[i].Length; j++)
            {
                for (int k = 0; k < voxels[i][j].Length; k++)
                {
                    Vector3 c = voxels[i][j][k].GetComponent<Voxel>().coords;
                    if (c.x == 0 || c.x == voxels.Length - 1 || c.y == 0 || c.y == voxels[i].Length - 1 ||
                    c.z == 0 || c.z == voxels[i][j].Length - 1)
                    {
                        voxels[i][j][k].GetComponent<Voxel>().type = Voxel.Type.SURFACE;
                        voxels[i][j][k].AddComponent<BoxCullider>().updateBoundaries();
                        surfaceVoxels.Add(voxels[i][j][k].GetComponent<Voxel>());
                    }
                    else
                    {
                        voxels[i][j][k].GetComponent<Voxel>().type = Voxel.Type.INTERIOR;
                        interiorVoxels.Add(voxels[i][j][k].GetComponent<Voxel>());
                    }
                }
            }
        }
    }

    public Bounds getBounds()
    {
        Bounds bounds = new Bounds();
        foreach (Voxel v in surfaceVoxels)
        {
            BoxCullider cullider = v.GetComponent<BoxCullider>();
            bounds.Encapsulate(cullider.getBounds());
        }
        return bounds;
    }

    private void connectVoxels()
    {
        Vector3 ruf = new Vector3(0.5f, 0.5f, 0.5f), rdf = new Vector3(0.5f, -0.5f, 0.5f), luf = new Vector3(-0.5f, 0.5f, 0.5f),
        ldf = new Vector3(-0.5f, -0.5f, 0.5f), rub = new Vector3(0.5f, 0.5f, -0.5f), rdb = new Vector3(0.5f, -0.5f, -0.5f),
        lub = new Vector3(-0.5f, 0.5f, -0.5f), ldb = new Vector3(-0.5f, -0.5f, -0.5f);

        for (int i = 0; i < voxels.Length; i++)
        {
            for (int j = 0; j < voxels[i].Length; j++)
            {
                for (int k = 0; k < voxels[i][j].Length; k++)
                {
                    Vector3 c = voxels[i][j][k].GetComponent<Voxel>().coords;
                    Voxel v = voxels[i][j][k].GetComponent<Voxel>();
                    //check if voxel exists and not connected then add spring
                    if (voxelExists(new Vector3Int(i + 1, j, k)) && !v.isConnected(voxels[i + 1][j][k].GetComponent<Voxel>()))
                    {
                        //Connect right voxel
                        addSprings(v.gameObject, voxels[i + 1][j][k], new[] { ruf, rdf, rub, rdb });

                    }
                    if (voxelExists(new Vector3Int(i - 1, j, k)) && !v.isConnected(voxels[i - 1][j][k].GetComponent<Voxel>()))
                    {
                        //Connect left voxel
                        addSprings(v.gameObject, voxels[i - 1][j][k], new[] { luf, ldf, lub, ldb });
                    }
                    if (voxelExists(new Vector3Int(i, j + 1, k)) && !v.isConnected(voxels[i][j + 1][k].GetComponent<Voxel>()))
                    {
                        //Connect above voxel
                        addSprings(v.gameObject, voxels[i][j + 1][k], new[] { ruf, rub, luf, lub });
                    }
                    if (voxelExists(new Vector3Int(i, j - 1, k)) && !v.isConnected(voxels[i][j - 1][k].GetComponent<Voxel>()))
                    {
                        //Connect below voxel
                        addSprings(v.gameObject, voxels[i][j - 1][k], new[] { rdf, rdb, ldf, ldb });
                    }
                    if (voxelExists(new Vector3Int(i, j, k + 1)) && !v.isConnected(voxels[i][j][k + 1].GetComponent<Voxel>()))
                    {
                        //Connect forward voxel
                        addSprings(v.gameObject, voxels[i][j][k + 1], new[] { ruf, rdf, luf, ldf });
                    }
                    if (voxelExists(new Vector3Int(i, j, k - 1)) && !v.isConnected(voxels[i][j][k - 1].GetComponent<Voxel>()))
                    {
                        //Connect backward voxel
                        addSprings(v.gameObject, voxels[i][j][k - 1], new[] { rub, rdb, lub, ldb });
                    }

                    if (voxelExists(new Vector3Int(i + 1, j + 1, k)) && !v.isConnected(voxels[i + 1][j + 1][k].GetComponent<Voxel>()))
                    {
                        //Connect up-right voxel
                        addSprings(v.gameObject, voxels[i + 1][j + 1][k], new[] { ruf, rub });
                    }
                    if (voxelExists(new Vector3Int(i + 1, j - 1, k)) && !v.isConnected(voxels[i + 1][j - 1][k].GetComponent<Voxel>()))
                    {
                        //Connect down-right voxel
                        addSprings(v.gameObject, voxels[i + 1][j - 1][k], new[] { rdf, rdb });
                    }
                    if (voxelExists(new Vector3Int(i + 1, j, k + 1)) && !v.isConnected(voxels[i + 1][j][k + 1].GetComponent<Voxel>()))
                    {
                        //Connnect right-forward voxel
                        addSprings(v.gameObject, voxels[i + 1][j][k + 1], new[] { ruf, rdf });
                    }
                    if (voxelExists(new Vector3Int(i + 1, j, k - 1)) && !v.isConnected(voxels[i + 1][j][k - 1].GetComponent<Voxel>()))
                    {
                        //Connect right-back voxel
                        addSprings(v.gameObject, voxels[i + 1][j][k - 1], new[] { rub, rdb });
                    }

                    if (voxelExists(new Vector3Int(i - 1, j + 1, k)) && !v.isConnected(voxels[i - 1][j + 1][k].GetComponent<Voxel>()))
                    {
                        //Connect up-left voxel
                        addSprings(v.gameObject, voxels[i - 1][j + 1][k], new[] { luf, lub });
                    }
                    if (voxelExists(new Vector3Int(i - 1, j - 1, k)) && !v.isConnected(voxels[i - 1][j - 1][k].GetComponent<Voxel>()))
                    {
                        //Connect down-left voxel
                        addSprings(v.gameObject, voxels[i - 1][j - 1][k], new[] { ldf, ldb });
                    }
                    if (voxelExists(new Vector3Int(i - 1, j, k + 1)) && !v.isConnected(voxels[i - 1][j][k + 1].GetComponent<Voxel>()))
                    {
                        //Connnect left-forward voxel
                        addSprings(v.gameObject, voxels[i - 1][j][k + 1], new[] { luf, ldf });
                    }
                    if (voxelExists(new Vector3Int(i - 1, j, k - 1)) && !v.isConnected(voxels[i - 1][j][k - 1].GetComponent<Voxel>()))
                    {
                        //Connect left-back voxel
                        addSprings(v.gameObject, voxels[i - 1][j][k - 1], new[] { lub, ldb });
                    }

                    if (voxelExists(new Vector3Int(i, j + 1, k + 1)) && !v.isConnected(voxels[i][j + 1][k + 1].GetComponent<Voxel>()))
                    {
                        //Connect up-forward
                        addSprings(v.gameObject, voxels[i][j + 1][k + 1], new[] { ruf, luf });
                    }
                    if (voxelExists(new Vector3Int(i, j + 1, k - 1)) && !v.isConnected(voxels[i][j + 1][k - 1].GetComponent<Voxel>()))
                    {
                        //Connect up-backward
                        addSprings(v.gameObject, voxels[i][j + 1][k - 1], new[] { rub, lub });
                    }
                    if (voxelExists(new Vector3Int(i, j - 1, k + 1)) && !v.isConnected(voxels[i][j - 1][k + 1].GetComponent<Voxel>()))
                    {
                        //Connect down-forward
                        addSprings(v.gameObject, voxels[i][j - 1][k + 1], new[] { rdf, ldf });
                    }
                    if (voxelExists(new Vector3Int(i, j - 1, k - 1)) && !v.isConnected(voxels[i][j - 1][k - 1].GetComponent<Voxel>()))
                    {
                        //Connect down-backward
                        addSprings(v.gameObject, voxels[i][j - 1][k - 1], new[] { rdb, ldb });
                    }

                    if (voxelExists(new Vector3Int(i + 1, j + 1, k + 1)) && !v.isConnected(voxels[i + 1][j + 1][k + 1].GetComponent<Voxel>()))
                    {
                        //Connect up-right-forward
                        addSprings(v.gameObject, voxels[i + 1][j + 1][k + 1], new[] { ruf });
                    }
                    if (voxelExists(new Vector3Int(i - 1, j + 1, k + 1)) && !v.isConnected(voxels[i - 1][j + 1][k + 1].GetComponent<Voxel>()))
                    {
                        //Connect left-up-forward
                        addSprings(v.gameObject, voxels[i - 1][j + 1][k + 1], new[] { luf });
                    }
                    if (voxelExists(new Vector3Int(i + 1, j - 1, k + 1)) && !v.isConnected(voxels[i + 1][j - 1][k + 1].GetComponent<Voxel>()))
                    {
                        //Connect right-down-forward
                        addSprings(v.gameObject, voxels[i + 1][j - 1][k + 1], new[] { rdf });
                    }
                    if (voxelExists(new Vector3Int(i - 1, j - 1, k + 1)) && !v.isConnected(voxels[i - 1][j - 1][k + 1].GetComponent<Voxel>()))
                    {
                        //Connect left-down-forward
                        addSprings(v.gameObject, voxels[i - 1][j - 1][k + 1], new[] { ldf });
                    }

                    if (voxelExists(new Vector3Int(i + 1, j + 1, k - 1)) && !v.isConnected(voxels[i + 1][j + 1][k - 1].GetComponent<Voxel>()))
                    {
                        //Connect up-right-back
                        addSprings(v.gameObject, voxels[i + 1][j + 1][k - 1], new[] { rub });
                    }
                    if (voxelExists(new Vector3Int(i - 1, j + 1, k - 1)) && !v.isConnected(voxels[i - 1][j + 1][k - 1].GetComponent<Voxel>()))
                    {
                        //Connect left-up-back
                        addSprings(v.gameObject, voxels[i - 1][j + 1][k - 1], new[] { lub });
                    }
                    if (voxelExists(new Vector3Int(i + 1, j - 1, k - 1)) && !v.isConnected(voxels[i + 1][j - 1][k - 1].GetComponent<Voxel>()))
                    {
                        //Connect right-down-back
                        addSprings(v.gameObject, voxels[i + 1][j - 1][k - 1], new[] { rdb });
                    }
                    if (voxelExists(new Vector3Int(i - 1, j - 1, k - 1)) && !v.isConnected(voxels[i - 1][j - 1][k - 1].GetComponent<Voxel>()))
                    {
                        //Connect left-down-back
                        addSprings(v.gameObject, voxels[i - 1][j - 1][k - 1], new[] { ldb });
                    }

                }
            }
        }
    }

    private bool voxelExists(Vector3Int coords)
    {
        return coords.x >= 0 && coords.y >= 0 && coords.z >= 0
            && coords.x < voxels.Length && coords.y < voxels[0].Length && coords.z < voxels[0][0].Length
            && voxels[coords.x][coords.y][coords.z] != null;
    }

    private void addSprings(GameObject voxel, GameObject other, Vector3[] localConnectingPoints)
    {
        foreach (Vector3 localConnectingPoint in localConnectingPoints)
        {
            Rigidbody otherRb = other.GetComponent<Rigidbody>();
            SpringJoint spring = voxel.AddComponent<SpringJoint>();
            spring.connectedBody = otherRb;
            spring.anchor = localConnectingPoint;
            spring.tolerance=0;
            spring.spring=250;//Increase to reduce gap between voxels

            //change other properties as well here


            voxel.GetComponent<Voxel>().addSpring(spring);
        }
    }

    private void distributeMass(){
        int voxelsCount=surfaceVoxels.Count+interiorVoxels.Count;
        float massPerVoxel=totalMass/(float)voxelsCount;
        foreach(Voxel v in surfaceVoxels){
            v.rb.mass=massPerVoxel;
        }
        foreach(Voxel v in interiorVoxels){
            v.rb.mass=massPerVoxel;
        }
    }
}
