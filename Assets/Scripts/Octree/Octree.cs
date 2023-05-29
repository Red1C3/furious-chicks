using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Octree
{
    public OctreeNode rootNode;

    public Octree(List<GameObject> world,float minNodeSize){
        Update(world,minNodeSize);
    }

    public Octree(List<GameObject> world){
        Update(world);
    }

    public void AddObj(List<GameObject> world){
        foreach (GameObject go in world)
        {
            rootNode.AddObj(go);
        }
    }

    public void search(GameObject mainGo,Solver solver){
        rootNode.search(mainGo,solver);
    }

    public void Update(List<GameObject> world,float minNodeSize){
        Bounds bounds = new Bounds(world[0].GetComponent<Cullider>().getBounds().center,world[0].GetComponent<Cullider>().getBounds().size);
        foreach (GameObject go in world)
        {
            bounds.Encapsulate(go.GetComponent<Cullider>().getBounds());
        }
        float maxSize = Mathf.Max(new float[] {bounds.size.x,bounds.size.y,bounds.size.z});
        Vector3 sizeVector = new Vector3(maxSize,maxSize,maxSize) * 0.5f;
        bounds.SetMinMax(bounds.center - sizeVector,bounds.center+sizeVector);
        rootNode = new OctreeNode(bounds,minNodeSize);
        AddObj(world);
    }

    public void Update(List<GameObject> world){
        Bounds bounds = new Bounds(world[0].GetComponent<Cullider>().getBounds().center,world[0].GetComponent<Cullider>().getBounds().size);
        foreach (GameObject go in world)
        {
            bounds.Encapsulate(go.GetComponent<Cullider>().getBounds());
        }
        float maxSize = Mathf.Max(new float[] {bounds.size.x,bounds.size.y,bounds.size.z});
        Vector3 sizeVector = new Vector3(maxSize,maxSize,maxSize) * 0.5f;
        bounds.SetMinMax(bounds.center - sizeVector,bounds.center+sizeVector);
        CreateOctree.nodeMinSize = (int)(maxSize/2.0f) + 1;
        rootNode = new OctreeNode(bounds,CreateOctree.nodeMinSize);
        AddObj(world);
    }
}
