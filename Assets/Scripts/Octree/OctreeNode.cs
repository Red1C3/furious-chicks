using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctreeNode
{
    Bounds nodeBounds;
    float minSize;
    Bounds[] childBounds;
    OctreeNode[] child = null;
    List<GameObject> gos = null;

    public OctreeNode(Bounds b, float m){
        nodeBounds=b;
        minSize=m;

        float quarter = nodeBounds.size.y/4.0f;
        float childLength = nodeBounds.size.y/2;
        Vector3 childSize = new Vector3(childLength,childLength,childLength);
        childBounds = new Bounds[8];

        childBounds[0] = new Bounds(nodeBounds.center + new Vector3(-quarter,quarter,-quarter),childSize);
        childBounds[1] = new Bounds(nodeBounds.center + new Vector3(quarter,quarter,-quarter),childSize);
        childBounds[2] = new Bounds(nodeBounds.center + new Vector3(-quarter,quarter,quarter),childSize);
        childBounds[3] = new Bounds(nodeBounds.center + new Vector3(quarter,quarter,quarter),childSize);
        childBounds[4] = new Bounds(nodeBounds.center + new Vector3(-quarter,-quarter,-quarter),childSize);
        childBounds[5] = new Bounds(nodeBounds.center + new Vector3(quarter,-quarter,-quarter),childSize);
        childBounds[6] = new Bounds(nodeBounds.center + new Vector3(-quarter,-quarter,quarter),childSize);
        childBounds[7] = new Bounds(nodeBounds.center + new Vector3(quarter,-quarter,quarter),childSize);

    }

    public void AddObj(GameObject go){
        DivideAndAdd(go);
    }

    public void DivideAndAdd(GameObject go){
        if(nodeBounds.size.y <= minSize){
            if(gos == null){
                gos = new List<GameObject>();
            }
            gos.Add(go);
            return;
        }
        if(child==null)
            child = new OctreeNode[8];

        bool dividing = false;
        for(int i=0;i<8;i++){
            if(child[i]==null){
                child[i]=new OctreeNode(childBounds[i],minSize);
            }
            if(go.tag=="VoxelGrid" && childBounds[i].Intersects(go.GetComponent<VoxelGrid>().getBounds())){
                dividing=true;
                child[i].DivideAndAdd(go);
            }
            else if(go.tag!="VoxelGrid" && childBounds[i].Intersects(go.GetComponent<Cullider>().getBounds())){
                dividing=true;
                child[i].DivideAndAdd(go);
            }
        }
        if(!dividing){
            child=null;
        }
    }

    public bool Intersects(Bounds bounds){
        return nodeBounds.Intersects(bounds);
    }

    public void search(GameObject player,SimpleSolver solver){
        Bounds bounds = player.GetComponent<Cullider>().getBounds();  
        if(!Intersects(bounds)){
            return;
        }      
        if(child==null){
            foreach(GameObject go in gos){
                solver.resolveCullision(player.GetComponent<Cullider>().cullideWith(go.GetComponent<Cullider>()),
                player.GetComponent<Rigidbody>(), go.GetComponent<Rigidbody>());
            } 
        }
        else
        {
            for (int i = 0; i < 8; i++)
            {
                child[i].search(player,solver);
            }       
        }
    }

    public void Draw(){
        Gizmos.color = new Color(0,1,0);
        Gizmos.DrawWireCube(nodeBounds.center,nodeBounds.size);
        if(child!=null){
            for(int i=0;i<8;i++){
                if(child[i]!= null){
                    child[i].Draw();
                }
            }
        }
    }
}
