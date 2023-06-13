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
    public bool scaleX=true,scaleZ=true;
    public GameObject pigPrefab;
    public int pigsNumber;
    
    private int copyNumber;
    Vector3 scale;

    List<List<float>> heights;
    void Awake()
    {
        copyNumber=objectNumber;

        initGround();

        initHeights();

        calcPros();
        
        for (int i = 0; i < copyNumber; i++)
        {
            int x = scaleX?getRandOdd():1;
            int y = getRandOdd();
            int z = scaleZ?getRandOdd():1;

            scale=new Vector3(x,y,z);

            Vector3 pos = getCenterVector();
            if(copyNumber<=0)
                break;
            GameObject randPrefab = Instantiate(blocksPrefabs[randomPrefabIndex()], pos , Quaternion.identity);
            randPrefab.transform.localScale = scale;      
            randPrefab.GetComponent<RigidbodyDriver>().mass = calcMass(scale);

        }
        for(int i=0;i<pigsNumber;i++){
            if(i>=groundScale*groundScale)
                break;
            int x = (int) Random.Range(0,groundScale);
            int z = (int) Random.Range(0,groundScale);
            float y = heights[x][z];
            if(y<0){
                i--;
                continue;
            }
            Vector3 pos = new Vector3(x-(int) (groundScale/2.0f), heights[x][z]+0.5f, z-(int) (groundScale/2.0f))+ground.transform.position;
            GameObject pig = Instantiate(pigPrefab, pos , Quaternion.Euler(0,180,0));
            heights[x][z]=-1;
        }
        
    }

    private int getRandOdd(){
        return 2*Random.Range(1, 3)+1;
    }

    private Vector3 getCenterVector(){        
        int x = (int) Random.Range((int)scale.x/2.0f,groundScale-((int)scale.x/2.0f));
        int z = (int) Random.Range((int)scale.z/2.0f,groundScale-((int)scale.z/2.0f));
        
        if(scale.x==scale.z && scale.x==1){
            heights[x][z]+=scale.y;
            return new Vector3(x-(int) (groundScale/2.0f), heights[x][z]-0.5f*scale.y, z-(int) (groundScale/2.0f))+ground.transform.position;
        }

        else if(scale.x==1){
            int dz = (int) Mathf.Abs(scale.z/2.0f);
            int z1=(int)Mathf.Max(z-dz,0),z2=(int)Mathf.Min(z+dz,groundScale-1);

            float newH = heights[x][z];
            for(int i=z1;i<=z2;i++)
                newH = Mathf.Max(newH,heights[x][i]);
            makePilier(newH,x,z1);
            makePilier(newH,x,z2);
            for(int i=z1;i<=z2;i++)
                heights[x][i]=newH+scale.y;
            return new Vector3(x-(int) (groundScale/2.0f), heights[x][z]-0.5f*scale.y, z-(int) (groundScale/2.0f))+ground.transform.position;
        }

        else if(scale.z==1){
            int dx =(int) Mathf.Abs(scale.x/2.0f);
            int x1=(int)Mathf.Max(x-dx,0),x2=(int)Mathf.Min(x+dx,groundScale-1);

            float newH = heights[x][z];
            for(int j=x1;j<=x2;j++)
                newH = Mathf.Max(newH,heights[j][z]);
            makePilier(newH,x1,z);
            makePilier(newH,x2,z);
            for(int j=x1;j<=x2;j++)
                heights[j][z]=newH+scale.y;
            return new Vector3(x-(int) (groundScale/2.0f), heights[x][z]-0.5f*scale.y, z-(int) (groundScale/2.0f))+ground.transform.position;
        }
        else{
            int dx =(int) Mathf.Abs(scale.x/2.0f);
            int x1=(int)Mathf.Max(x-dx,0),x2=(int)Mathf.Min(x+dx,groundScale-1);
            
            int dz =(int) Mathf.Abs(scale.z/2.0f);
            int z1=(int)Mathf.Max(z-dz,0),z2=(int)Mathf.Min(z+dz,groundScale-1);
            
            float newH = heights[x][z];
            for(int i=z1;i<=z2;i++)
                for(int j=x1;j<=x2;j++)
                    newH = Mathf.Max(newH,heights[j][i]);
            makePilier(newH,x1,z1);
            makePilier(newH,x1,z2);
            makePilier(newH,x2,z1);
            makePilier(newH,x2,z2);
            for(int i=z1;i<=z2;i++)
                for(int j=x1;j<=x2;j++)
                    heights[j][i]=newH+scale.y;            
            return new Vector3(x-(int) (groundScale/2.0f), heights[x][z]-0.5f*scale.y, z-(int) (groundScale/2.0f))+ground.transform.position;
        }
    }

    private void makePilier(float maxH, int x, int z){
        float y = maxH - heights[x][z];
        if (y<=1e-8 || copyNumber<=0){
            return;
        }
        Vector3 pilierScale = new Vector3(1,y,1);
        heights[x][z]+=pilierScale.y;
        Vector3 pos = new Vector3(x-(int) (groundScale/2.0f), heights[x][z]-0.5f*pilierScale.y, z-(int) (groundScale/2.0f))+ground.transform.position;
        
        GameObject pilier = Instantiate(blocksPrefabs[randomPrefabIndex()],pos , Quaternion.identity);
        pilier.transform.localScale = pilierScale;
        pilier.GetComponent<RigidbodyDriver>().mass = calcMass(pilierScale);

        pilier.tag = "Pilier";
        copyNumber--;
        return;
    }

    private void initHeights(){
        heights = new List<List<float>>();
        for (int i = 0; i < groundScale; i++)
        {
            heights.Add(new List<float>());
            for (int j = 0; j < groundScale; j++){
                heights[i].Add(0);
            }
        }
    }

    private void initGround(){
        ground.transform.localScale = new Vector3(2*groundScale,0.1f,2*groundScale);
        ground.transform.position = new Vector3(0,-1.0f,groundScale*3.0f/2.0f);
    }

    private void calcPros(){
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
    }

    private int randomPrefabIndex(){
        float r = Random.value;
        int i=0;
        for(;i<blocksPrefabs.Length;i++){
            if(blocksPros[i]>=r)
                break;
        }
        return i;
    }

    private float calcMass(Vector3 scale){
        float newMass = 1;
        newMass*=scale.x;
        newMass*=scale.y;
        newMass*=scale.z;
        return newMass;
    }    
}
