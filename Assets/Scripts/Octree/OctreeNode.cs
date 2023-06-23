using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctreeNode
{
    Bounds nodeBounds;
    float minSize;
    Bounds[] childBounds;
    OctreeNode[] child = null;
    List<GameObject> gos = new List<GameObject>();

    public OctreeNode(Bounds b, float m)
    {
        nodeBounds = b;
        minSize = m;

        float quarter = nodeBounds.size.y / 4.0f;
        float childLength = nodeBounds.size.y / 2;
        Vector3 childSize = new Vector3(childLength, childLength, childLength);
        childBounds = new Bounds[8];

        childBounds[0] = new Bounds(nodeBounds.center + new Vector3(-quarter, quarter, -quarter), childSize);
        childBounds[1] = new Bounds(nodeBounds.center + new Vector3(quarter, quarter, -quarter), childSize);
        childBounds[2] = new Bounds(nodeBounds.center + new Vector3(-quarter, quarter, quarter), childSize);
        childBounds[3] = new Bounds(nodeBounds.center + new Vector3(quarter, quarter, quarter), childSize);
        childBounds[4] = new Bounds(nodeBounds.center + new Vector3(-quarter, -quarter, -quarter), childSize);
        childBounds[5] = new Bounds(nodeBounds.center + new Vector3(quarter, -quarter, -quarter), childSize);
        childBounds[6] = new Bounds(nodeBounds.center + new Vector3(-quarter, -quarter, quarter), childSize);
        childBounds[7] = new Bounds(nodeBounds.center + new Vector3(quarter, -quarter, quarter), childSize);

    }

    public void AddObj(GameObject go)
    {
        DivideAndAdd(go);
    }

    public void DivideAndAdd(GameObject go)
    {
        if (nodeBounds.size.y <= minSize)
        {
            gos.Add(go);
            Engine.allObjectsN += 1;
            Engine.maxNodeObjectN = Math.Max(Engine.maxNodeObjectN, gos.Count);
            return;
        }
        if (child == null)
            child = new OctreeNode[8];

        bool dividing = false;
        for (int i = 0; i < 8; i++)
        {
            if (child[i] == null)
            {
                child[i] = new OctreeNode(childBounds[i], minSize);
            }
            if (childBounds[i].Intersects(go.GetComponent<Cullider>().getBounds()))
            {
                dividing = true;
                child[i].DivideAndAdd(go);
            }
        }
        if (!dividing)
        {
            child = null;
        }
    }

    public bool Intersects(Bounds bounds)
    {
        return nodeBounds.Intersects(bounds);
    }

    public bool isNull(CullisionInfo c)
    {
        return (c.first == null || c.second == null);
    }

    public void search(GameObject mainGo, Solver solver)
    {
        Bounds bounds = mainGo.GetComponent<Cullider>().getBounds();
        if (!Intersects(bounds))
        {
            return;
        }
        if (child == null)
        {
            Cullider firstGo = mainGo.GetComponent<Cullider>();
            foreach (GameObject go in gos)
            {
                if (go == mainGo || bothAreVoxelsBelongingToSameGrid(go, mainGo)) continue;
                Cullider secondGo = go.GetComponent<Cullider>();

                checkCulliding(firstGo, secondGo);
            }
        }
        else
        {
            for (int i = 0; i < 8; i++)
            {
                child[i].search(mainGo, solver);
            }
        }
    }

    public void checkCulliding(Cullider firstGo, Cullider secondGo)
    {
        if (!firstGo.getBounds().Intersects(secondGo.getBounds()) || !secondGo.getBounds().Intersects(firstGo.getBounds()))
            return;
        if (isNull(Engine.culls.Find(x => (x.first == secondGo && firstGo == x.second)))
        && isNull(Engine.culls.Find(x => (x.first == firstGo && secondGo == x.second))))
        {
            CullisionInfo returned = firstGo.cullideWith(secondGo);
            if (!isNull(returned))
            {
                Engine.culls.Add(returned);
                if (!returned.first.getRigidbodyDriver().psudoFreeze ||
                            !returned.second.getRigidbodyDriver().psudoFreeze)
                {
                    returned.first.getRigidbodyDriver().psudoUnfreeze();
                    returned.second.getRigidbodyDriver().psudoUnfreeze();
                }
            }
        }
    }

    private bool bothAreVoxelsBelongingToSameGrid(GameObject v0, GameObject v1)
    {
        Voxel vc0, vc1;
        if (v0.TryGetComponent<Voxel>(out vc0) && v1.TryGetComponent<Voxel>(out vc1))
        {
            if (vc0.grid == vc1.grid)
            {
                return true;
            }
        }
        return false;
    }

    public void Draw()
    {
        Gizmos.color = new Color(1, 0, 0);
        Gizmos.DrawWireCube(nodeBounds.center, nodeBounds.size);
        if (child != null)
        {
            for (int i = 0; i < 8; i++)
            {
                if (child[i] != null)
                {
                    child[i].Draw();
                }
            }
        }
    }
}
