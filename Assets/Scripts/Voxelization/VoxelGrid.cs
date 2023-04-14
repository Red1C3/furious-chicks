using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelGrid : MonoBehaviour
{
    public GameObject voxel;
    private Vector3 origin;
    private float length, width, depth;
    [SerializeField]
    private float density; //Density is an approx of voxels in the grid
    private float voxelLen; //each voxel side length
    private GameObject[][][] voxels;
    // Start is called before the first frame update
    void Start()
    {
        Renderer renderer=GetComponent<Renderer>();
        var bounds = renderer.bounds;
        length=bounds.extents.x*2;
        width=bounds.extents.y*2;
        depth=bounds.extents.z*2;
        origin=bounds.center-bounds.extents;
        buildGrid();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void buildGrid()
    {
        voxelLen = Mathf.Pow(((length * width * depth) / density), 1.0f / 3.0f);
        //voxels array init
        voxels = new GameObject[Mathf.CeilToInt(length / voxelLen)][][];
        for (int i = 0; i < voxels.Length; i++) voxels[i] = new GameObject[Mathf.CeilToInt(width / voxelLen)][];
        for (int i = 0; i < voxels.Length; i++)
            for (int j = 0; j < voxels[i].Length; j++) voxels[i][j] = new GameObject[Mathf.CeilToInt(depth / voxelLen)];

        //create and place voxels in the grid
        for (int i = 0; i < voxels.Length; i++)
        {
            for (int j = 0; j < voxels[i].Length; j++)
            {
                for (int k = 0; k < voxels[i][j].Length; k++)
                {
                    voxels[i][j][k] = Instantiate(voxel,
                     new Vector3((i + 0.5f) * voxelLen, (j + 0.5f) * voxelLen, (k + 0.5f) * voxelLen) + origin
                    , Quaternion.identity, transform);
                    voxels[i][j][k].transform.localScale = new Vector3(voxelLen, voxelLen, voxelLen);
                }
            }
        }
    }
}
