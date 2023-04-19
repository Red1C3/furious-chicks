using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelGrid : MonoBehaviour
{
    public GameObject voxelPrefab;
    private Vector3 origin;
    [SerializeField]
    private float density, length, width, depth;
    private float voxelLen;
    private GameObject[][][] voxels;
    private List<Voxel> surfaceVoxels, interiorVoxels;

    void Start()
    {
        surfaceVoxels = new List<Voxel>();
        interiorVoxels = new List<Voxel>();
        origin = transform.position;
        buildGrid();
        typeVoxels();
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
                    voxels[i][j][k].GetComponent<Voxel>().grid=this;
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
                    }
                    else
                    {
                        voxels[i][j][k].GetComponent<Voxel>().type = Voxel.Type.INTERIOR;
                    }
                }
            }
        }
    }
}
