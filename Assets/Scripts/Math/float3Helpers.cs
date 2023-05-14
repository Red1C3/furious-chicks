using Unity.Mathematics;
using UnityEngine;

public static class float3Helpers{
    public static float3x3 crossProductTensor(float3 u,float3 v){
        float3x3 tensor=float3x3.identity;

        for(int i=0;i<3;i++){
            for(int j=0;j<3;j++){
                tensor[i][j]=u[i]*v[j];
            }
        }

        return tensor;
    }
}