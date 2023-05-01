using UnityEngine;

public struct float12{
    public float[] floats;
    public float12(float firstThree,float secondThree,float thirdThree,float fourthThree){
        floats=new float[12];
        for(int i=0;i<3;i++){
            floats[i]=firstThree;
        }
        for(int i=3;i<6;i++){
            floats[i]=secondThree;
        }
        for(int i=6;i<9;i++){
            floats[i]=thirdThree;
        }
        for(int i=9;i<12;i++){
            floats[i]=fourthThree;
        }
    }

    public float12(Vector3 firstThree,Vector3 secondThree,Vector3 thirdThree,Vector3 fourthThree){
        floats=new float[12];
        for(int i=0;i<3;i++){
            floats[i]=firstThree[i%3];
        }
        for(int i=3;i<6;i++){
            floats[i]=secondThree[i%3];
        }
        for(int i=6;i<9;i++){
            floats[i]=thirdThree[i%3];
        }
        for(int i=9;i<12;i++){
            floats[i]=fourthThree[i%3];
        }
    }
}