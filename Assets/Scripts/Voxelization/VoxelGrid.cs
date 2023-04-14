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
    private Vector3[] vertices;
    private int[] indices;
    private List<Voxel> surfaceVoxels;
    // Start is called before the first frame update
    void Start()
    {
        surfaceVoxels = new List<Voxel>();
        Renderer renderer = GetComponent<Renderer>();
        var bounds = renderer.bounds; //Mesh bounds in world space
        length = bounds.extents.x * 2;
        width = bounds.extents.y * 2;
        depth = bounds.extents.z * 2;
        origin = bounds.center - bounds.extents;
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        indices = mesh.triangles;
        buildGrid();
        markSurfaceVoxels();
        //removeNonSurface();// for demo
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
    private void markSurfaceVoxels()
    {
        for (int i = 0; i < voxels.Length; i++)
        {
            for (int j = 0; j < voxels[i].Length; j++)
            {
                for (int k = 0; k < voxels[i][j].Length; k++)
                {
                    Voxel voxel = voxels[i][j][k].GetComponent<Voxel>();

                    for (int l = 0; l < indices.Length; l += 3)
                    {
                        Vector3[] tri = new Vector3[3];
                        tri[0] = vertices[indices[l]];
                        tri[1] = vertices[indices[l + 1]];
                        tri[2] = vertices[indices[l + 2]];
                        for (int m = 0; m < 3; m++) tri[m] = transform.TransformPoint(tri[m]);

                        //if tri intersects voxel, switch to surface and break
                        if (TriCubeIntersection.triCubeIntersection(tri, voxel.transform) == (ulong)TriCubeIntersection.InOut.INSIDE)
                        {
                            voxel.type = Voxel.Type.SURFACE;
                            surfaceVoxels.Add(voxel);
                            break;
                        }
                    }
                }
            }
        }
    }
    private void removeNonSurface()
    {
        for (int i = 0; i < voxels.Length; i++)
        {
            for (int j = 0; j < voxels[i].Length; j++)
            {
                for (int k = 0; k < voxels[i][j].Length; k++)
                {
                    GameObject voxel = voxels[i][j][k];
                    if (voxel.GetComponent<Voxel>().type != Voxel.Type.SURFACE)
                    {
                        Destroy(voxel);
                    }
                }
            }
        }
    }
}
