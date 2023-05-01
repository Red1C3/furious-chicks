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
        float12 res=new float12();
        res.floats=new float[12];
        
        for(int i=0;i<12;i++){
            res.floats[i]=rowColMult(vec,mat.column(i));
        }

        return res;
    }

    public float12 column(int i){
        float12 col=new float12();
        col.floats=new float[12];

        for(int j=0;j<12;j++){
            col.floats[j]=floats[j,i];
        }
        return col;
    }

    private static float rowColMult(float12 row,float12 col){
        float res=0;
        for(int i=0;i<12;i++){
            res+=row.floats[i]*col.floats[i];
        }
        return res;
    }
}