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
    private List<Voxel> surfaceVoxels, interiorVoxels;
    // Start is called before the first frame update
    void Start()
    {
        surfaceVoxels = new List<Voxel>();
        interiorVoxels = new List<Voxel>();
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
        markNonSurface();
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
                    voxels[i][j][k].GetComponent<Voxel>().coords = new Vector3Int(i, j, k);
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
    private void markNonSurface()
    {
        //Transverse the voxel grid starting from a neutral voxel
        Voxel seed = null;
        for (int i = 0; i < voxels.Length; i++)
        {
            for (int j = 0; j < voxels[i].Length; j++)
            {
                for (int k = 0; k < voxels[i][j].Length; k++)
                {
                    if (voxels[i][j][k].GetComponent<Voxel>().type == Voxel.Type.NEUTRAL)
                    {
                        seed = voxels[i][j][k].GetComponent<Voxel>();
                    }
                }
            }
        }
        //All voxels are now set
        if (seed == null) return;

        Stack<Voxel> stack = new Stack<Voxel>();
        List<Voxel> visited = new List<Voxel>();
        stack.Push(seed);

        bool isExterior = false;

        while (stack.Count > 0)
        {
            Voxel head = stack.Pop();
            visited.Add(head);
            if (isVoxelOnOutline(head))
            {
                isExterior = true;
            }

            Voxel[] neighbours = getNeutralNeighbours(head);
            for (int i = 0; i < neighbours.Length; i++)
            {
                if (!visited.Contains(neighbours[i]))
                {
                    stack.Push(neighbours[i]);
                }
            }
        }

        for (int i = 0; i < visited.Count; i++)
        {
            if (isExterior) visited[i].type = Voxel.Type.EXTERIOR;
            else
            {
                visited[i].type = Voxel.Type.INTERIOR;
                interiorVoxels.Add(visited[i]);
            }
        }
        //Exhaust all voxels in the grid that are not surface
        markNonSurface();
    }
    //Get neutral neighbours in 6-neighbours style (8-style caused a bug)
    private Voxel[] getNeutralNeighbours(Voxel v)
    {
        List<Voxel> list = new List<Voxel>();

        for (int i = v.coords.x - 1; i <= v.coords.x + 1; i++)
        {
            if (i < 0 || i >= voxels.Length || i == v.coords.x) continue;
            Voxel voxel = voxels[i][v.coords.y][v.coords.z].GetComponent<Voxel>();
            if (voxel.type == Voxel.Type.NEUTRAL)
            {
                list.Add(voxel);
            }
        }

        for (int i = v.coords.y - 1; i <= v.coords.y + 1; i++)
        {
            if (i < 0 || i >= voxels[0].Length || i == v.coords.y) continue;
            Voxel voxel = voxels[v.coords.x][i][v.coords.z].GetComponent<Voxel>();
            if (voxel.type == Voxel.Type.NEUTRAL) list.Add(voxel);
        }

        for (int i = v.coords.z - 1; i <= v.coords.z + 1; i++)
        {
            if (i < 0 || i >= voxels[0][0].Length || i == v.coords.z) continue;
            Voxel voxel = voxels[v.coords.x][v.coords.y][i].GetComponent<Voxel>();
            if (voxel.type == Voxel.Type.NEUTRAL) list.Add(voxel);
        }

        return list.ToArray();
    }
    private bool isVoxelOnOutline(Voxel v)
    {
        int i = v.coords.x;
        int j = v.coords.y;
        int k = v.coords.z;
        return i == 0 || j == 0 || k == 0 || i == voxels.Length - 1 || j == voxels[0].Length - 1 || k == voxels[0][0].Length - 1;
    }
    private void removeOfType(Voxel.Type type)
    {
        for (int i = 0; i < voxels.Length; i++)
        {
            for (int j = 0; j < voxels[i].Length; j++)
            {
                for (int k = 0; k < voxels[i][j].Length; k++)
                {
                    GameObject voxel = voxels[i][j][k];
                    if (voxel.GetComponent<Voxel>().type == type)
                    {
                        Destroy(voxel);
                    }
                }
            }
        }
    }
}
