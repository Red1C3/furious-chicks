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
    List<GameObject> gos=new List<GameObject>();

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
            gos.Add(go);
            CreateOctree.allObjectsN+=1;
            CreateOctree.maxNodeObjectN=Math.Max(CreateOctree.maxNodeObjectN,gos.Count);
            return;
        }
        if(child==null)
            child = new OctreeNode[8];

        bool dividing = false;
        for(int i=0;i<8;i++){
            if(child[i]==null){
                child[i]=new OctreeNode(childBounds[i],minSize);
            }
            if(childBounds[i].Intersects(go.GetComponent<Cullider>().getBounds())){
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

    public bool notNull(CullisionInfo c){
        return (c.first!=null && c.second!=null);
    }

    public void search(GameObject player,Solver solver){        
        Bounds bounds = player.GetComponent<Cullider>().getBounds();  
        if(!Intersects(bounds)){
            return;
        }      
        if(child==null){
            foreach(GameObject go in gos){
                if(go == player)
                    continue;
                if(bothAreVoxelsBelongingToSameGrid(go,player)) continue;

                
                CullisionInfo returned = player.GetComponent<Cullider>().cullideWith(go.GetComponent<Cullider>());
                CullisionInfo swaped = CreateOctree.culls.Find(x => (x.first==returned.second && returned.first==x.second));
                CullisionInfo duplicated = CreateOctree.culls.Find(x => (x.first==returned.first && returned.second==x.second));
                if(notNull(returned) && !notNull(swaped)&& !notNull(duplicated))
                    CreateOctree.culls.Add(returned);
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

    private bool bothAreVoxelsBelongingToSameGrid(GameObject v0,GameObject v1){
        Voxel vc0,vc1;
        if(v0.TryGetComponent<Voxel>(out vc0) && v1.TryGetComponent<Voxel>(out vc1)){
            if(vc0.grid==vc1.grid){
                return true;
            }
        }
        return false;
    }

    public void Draw(){
        Gizmos.color = new Color(1,0,0);
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
