using UnityEngine;

public struct float12x12{
    public float[,] floats;
    public float12x12(float firstThreeDiag,float secondThreeDiag,float thirdThreeDiag,float fourthThreeDiag){
        floats=new float[12,12];
        for(int i=0;i<3;i++){
            floats[i,i]=firstThreeDiag;
        }
        for(int i=3;i<6;i++){
            floats[i,i]=secondThreeDiag;
        }
        for(int i=6;i<9;i++){
            floats[i,i]=thirdThreeDiag;
        }
        for(int i=9;i<12;i++){
            floats[i,i]=fourthThreeDiag;
        }
    }

    public static float12 operator*(float12 vec,float12x12 mat){
        //TODO
        return new float12(0,0,0,0);
    }
}