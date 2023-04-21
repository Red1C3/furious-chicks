using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Octree
{
    public OctreeNode rootNode;

    public Octree(GameObject[] world,float minNodeSize){
        Update(world,minNodeSize);
    }

    public void AddObj(GameObject[] world){
        foreach (GameObject go in world)
        {
            rootNode.AddObj(go);
        }
    }

    public void search(GameObject player,SimpleSolver solver){
        rootNode.search(player,solver);
    }

    public void Update(GameObject[] world,float minNodeSize){
        Bounds bounds = new Bounds();
        foreach (GameObject go in world)
        {
            if(go.tag=="VoxelGrid"){
                bounds.Encapsulate(go.GetComponent<VoxelGrid>().getBounds());
            }
            else{
                bounds.Encapsulate(go.GetComponent<Cullider>().getBounds());
            }
        }
        float maxSize = Mathf.Max(new float[] {bounds.size.x,bounds.size.y,bounds.size.z});
        Vector3 sizeVector = new Vector3(maxSize,maxSize,maxSize) * 0.5f;
        bounds.SetMinMax(bounds.center - sizeVector,bounds.center+sizeVector);
        rootNode = new OctreeNode(bounds,minNodeSize);
        AddObj(world);
    }
}
