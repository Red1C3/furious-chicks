using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomGen : MonoBehaviour
{
    [SerializeField]
    public float groundScale;
    public GameObject ground;
    public GameObject[] blocksPrefabs;
    public float[] blocksPros;
    public int objectNumber;
    
    Vector3 scale;

    List<List<float>> heights;
    void Awake()
    {
        ground.transform.localScale = new Vector3(groundScale,0.1f,groundScale);
        ground.transform.position = new Vector3(0,0,groundScale*3.0f/4.0f);
        heights = new List<List<float>>();
        for (int i = 0; i < groundScale; i++)
        {
            heights.Add(new List<float>());
            for (int j = 0; j < groundScale; j++){
                heights[i].Add(0);
            }
        }
        float sum=0;
        for (int i = 0; i < blocksPros.Length; i++)
        {
            sum+=blocksPros[i];
            blocksPros[i]=sum;
        }
        for (int i = 0; i < blocksPros.Length; i++)
        {
            blocksPros[i]/=sum;
        }
        for (int i = 0; i < objectNumber; i++)
        {
            // scale=new Vector3(Random.Range(1, 6),Random.Range(1, 6),Random.Range(1, 6));
            scale=new Vector3(1,Random.Range(1, 6),1);

            GameObject randPrefab = Instantiate(blocksPrefabs[randomPrefabIndex()], getCenterVector(), Quaternion.identity);
            randPrefab.transform.localScale = scale;
            
        }
        
    }

    public int randomPrefabIndex(){
        float r = Random.value;
        int i=0;
        for(;i<blocksPrefabs.Length;i++){
            if(blocksPros[i]>=r)
                break;
        }
        return i;
    }

    public Vector3 getCenterVector(){
        while (true)
        {
            int x = (int) Random.Range(0,groundScale);
            int z = (int) Random.Range(0,groundScale);
            if(scale.x==scale.z && scale.x==1){
                heights[x][z]+=scale.y;
                return new Vector3(x-(int) (groundScale/2.0f), heights[x][z]-scale.y, z-(int) (groundScale/2.0f))+0.5f*scale+ground.transform.position;
            }

            
        }
    }
}
