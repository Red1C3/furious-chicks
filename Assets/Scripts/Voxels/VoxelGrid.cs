using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityMeshDecimation;

public class VoxelGrid : MonoBehaviour
{
    public GameObject voxelPrefab;
    private Vector3 origin;
    private float density;
    private float length, width, depth;
    [SerializeField]
    private float totalMass;
    private float voxelLen;
    private GameObject[][][] voxels;
    private Vector3[] vertices;

    private int[] indices;
    private static readonly float VOLUME_FACTOR = 50.0f;
    private static readonly float FACE_COUNT_FACTOR = 100.0f;

    public List<Voxel> surfaceVoxels, interiorVoxels, exteriorVoxels;
    [SerializeField]
    private bool displayDecimated = false;

    void Awake()
    {
        surfaceVoxels = new List<Voxel>();
        interiorVoxels = new List<Voxel>();
        exteriorVoxels = new List<Voxel>();
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Renderer renderer = GetComponent<Renderer>();
        origin = renderer.bounds.center;
        var bounds = renderer.bounds; //Mesh bounds in world space
        length = bounds.extents.x * 2;
        width = bounds.extents.y * 2;
        depth = bounds.extents.z * 2;


        density = (mesh.triangles.Length / 3.0f) * math.sqrt(getApproxAABBVolume());
        mesh = decimate(mesh);
        vertices = mesh.vertices;
        indices = mesh.triangles;
        if (displayDecimated)
        {
            GetComponent<MeshFilter>().mesh = mesh;
        }

        buildGrid();
        centerVoxels();
        markSurfaceVoxels();
        markNonSurface();
        //Calculate inertia tensor
        var b = merge(voxels[2][6][8].GetComponent<Voxel>()); //2 6 8
        Debug.Log(b.center);
        Debug.Log(b.extents);
        removeOfType(Voxel.Type.EXTERIOR);
        //removeOfType(Voxel.Type.SURFACE);
        //removeOfType(Voxel.Type.INTERIOR);


    }

    private Mesh decimate(Mesh mesh)
    {
        int facesCount = (int)math.round(mesh.triangles.Length / 3.0f);
        var conditions = new TargetConditions();
        //conditions.faceCount = 100;
        conditions.maxMetrix = VOLUME_FACTOR * (1.0f / math.sqrt(getApproxAABBVolume())) +
                                FACE_COUNT_FACTOR * (1.0f / math.sqrt(facesCount));

        var parameter = new EdgeCollapseParameter();
        parameter.UsedProperty = UnityMeshDecimation.Internal.VertexProperty.UV0;
        parameter.PreserveBoundary = true;

        var meshDecimation = new UnityMeshDecimation.UnityMeshDecimation();
        meshDecimation.Execute(mesh, parameter, conditions);
        return meshDecimation.ToMesh();
    }

    private Bounds merge(Voxel seed)
    {
        bool[,,] visited = new bool[voxels.Length, voxels[0].Length, voxels[0][0].Length];

        var bounds = new Bounds();

        Queue<Voxel> queue = new Queue<Voxel>();
        queue.Enqueue(seed);

        while (queue.Count != 0)
        {
            Voxel head = queue.Dequeue();
            var coords = head.coords;

            if (visited[coords.x, coords.y, coords.z]) continue;

            visited[coords.x, coords.y, coords.z] = true;

            var newBounds = bounds;

            //Probably voxel get bounds is not right
            newBounds.Encapsulate(head.getBounds());

            Debug.Log(newBounds.center);
            Debug.Log(newBounds.extents);

            bool intersectsExterior = false;

            foreach (Voxel exterior in exteriorVoxels)
            {
                if (newBounds.Intersects(exterior.getBounds()))
                {
                    intersectsExterior = true;
                    break;
                }
            }

            if (!intersectsExterior)
            {
                bounds = newBounds;
                var neighbours = getNeighbours(head, (int)Voxel.Type.SURFACE | (int)Voxel.Type.INTERIOR);

                foreach (Voxel v in neighbours)
                {
                    if (!visited[v.coords.x, v.coords.y, v.coords.z])
                    {
                        queue.Enqueue(v);
                    }
                }
            }
        }

        return bounds;
    }

    private float getApproxAABBVolume()
    {
        var extents = GetComponent<Renderer>().bounds.extents;
        return extents.x * extents.y * extents.z * 8.0f;
    }

    private void centerVoxels()
    {
        Vector3 avg = Vector3.zero;
        int count = 0;
        for (int i = 0; i < voxels.Length; i++)
        {
            for (int j = 0; j < voxels[i].Length; j++)
            {
                for (int k = 0; k < voxels[i][j].Length; k++)
                {
                    avg += voxels[i][j][k].transform.position;
                    count++;
                }
            }
        }
        avg /= count;
        for (int i = 0; i < voxels.Length; i++)
        {
            for (int j = 0; j < voxels[i].Length; j++)
            {
                for (int k = 0; k < voxels[i][j].Length; k++)
                {
                    voxels[i][j][k].transform.Translate(-avg + origin, Space.World);
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
                    voxels[i][j][k].transform.localScale = new Vector3(voxelLen / transform.lossyScale.x,
                    voxelLen / transform.lossyScale.y, voxelLen / transform.lossyScale.z);
                    voxels[i][j][k].GetComponent<Voxel>().coords = new Vector3Int(i, j, k);
                    voxels[i][j][k].GetComponent<Voxel>().grid = this;
                    voxels[i][j][k].GetComponent<Voxel>().init();
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

            Voxel[] neighbours = getNeighbours(head, (int)Voxel.Type.NEUTRAL);
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
            if (isExterior)
            {
                visited[i].type = Voxel.Type.EXTERIOR;
                exteriorVoxels.Add(visited[i]);
            }
            else
            {
                visited[i].type = Voxel.Type.INTERIOR;
                interiorVoxels.Add(visited[i]);
            }
        }
        //Exhaust all voxels in the grid that are not surface
        markNonSurface();
    }
    private Voxel[] getNeighbours(Voxel v, int typeMask)
    {
        List<Voxel> list = new List<Voxel>();

        for (int i = v.coords.x - 1; i <= v.coords.x + 1; i++)
        {
            if (i < 0 || i >= voxels.Length || i == v.coords.x) continue;
            Voxel voxel = voxels[i][v.coords.y][v.coords.z].GetComponent<Voxel>();
            if ((((int)voxel.type) & typeMask) != 0)
            {
                list.Add(voxel);
            }
        }

        for (int i = v.coords.y - 1; i <= v.coords.y + 1; i++)
        {
            if (i < 0 || i >= voxels[0].Length || i == v.coords.y) continue;
            Voxel voxel = voxels[v.coords.x][i][v.coords.z].GetComponent<Voxel>();
            if ((((int)voxel.type) & typeMask) != 0) list.Add(voxel);
        }

        for (int i = v.coords.z - 1; i <= v.coords.z + 1; i++)
        {
            if (i < 0 || i >= voxels[0][0].Length || i == v.coords.z) continue;
            Voxel voxel = voxels[v.coords.x][v.coords.y][i].GetComponent<Voxel>();
            if ((((int)voxel.type) & typeMask) != 0) list.Add(voxel);
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


    private bool voxelExists(Vector3Int coords)
    {
        return coords.x >= 0 && coords.y >= 0 && coords.z >= 0
            && coords.x < voxels.Length && coords.y < voxels[0].Length && coords.z < voxels[0][0].Length
            && voxels[coords.x][coords.y][coords.z] != null;
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
