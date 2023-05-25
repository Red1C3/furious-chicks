using UnityEngine;

public struct float12x12
{
    public float[,] floats;
    private static float12 medium, result0, result1;
    public float12x12(float firstThreeDiag, float secondThreeDiag, float thirdThreeDiag, float fourthThreeDiag)
    {
        if (medium.floats == null) medium.floats = new float[12];
        if (result0.floats == null) result0.floats = new float[12];
        if (result1.floats == null) result1.floats = new float[12];

        floats = new float[12, 12];
        set(firstThreeDiag, secondThreeDiag, thirdThreeDiag, fourthThreeDiag);
    }
    public void set(float firstThreeDiag, float secondThreeDiag, float thirdThreeDiag, float fourthThreeDiag)
    {
        if (floats == null) return;
        for (int i = 0; i < 3; i++)
        {
            floats[i, i] = firstThreeDiag;
        }
        for (int i = 3; i < 6; i++)
        {
            floats[i, i] = secondThreeDiag;
        }
        for (int i = 6; i < 9; i++)
        {
            floats[i, i] = thirdThreeDiag;
        }
        for (int i = 9; i < 12; i++)
        {
            floats[i, i] = fourthThreeDiag;
        }
    }
    public void set(Vector3 firstDiag, Vector3 secondDiag, Vector3 thirdDiag, Vector3 fourthDiag)
    {
        if (floats == null) return;
        floats[0, 0] = firstDiag.x;
        floats[1, 1] = firstDiag.y;
        floats[2, 2] = firstDiag.z;
        floats[3, 3] = secondDiag.x;
        floats[4, 4] = secondDiag.y;
        floats[5, 5] = secondDiag.z;
        floats[6, 6] = thirdDiag.x;
        floats[7, 7] = thirdDiag.y;
        floats[8, 8] = thirdDiag.z;
        floats[9, 9] = fourthDiag.x;
        floats[10, 10] = fourthDiag.y;
        floats[11, 11] = fourthDiag.z;
    }

    public static float12 operator *(float12 vec, float12x12 mat)
    {
        for (int i = 0; i < 12; i++)
        {
            result0.floats[i] = rowColMult(vec, mat.column(i));
        }

        return result0;
    }

    public static float12 operator *(float12x12 mat, float12 vec)
    {
        for (int i = 0; i < 12; i++)
        {
            result1.floats[i] = rowColMult(mat.row(i), vec);
        }
        return result1;
    }
    //Uses static medium, may not be used with another expression that uses it
    private float12 column(int i)
    {
        for (int j = 0; j < 12; j++)
        {
            medium.floats[j] = floats[j, i];
        }
        return medium;
    }
    //Uses static medium, may not be used with another expression that uses it
    private float12 row(int i)
    {
        for (int j = 0; j < 12; j++)
        {
            medium.floats[j] = floats[i, j];
        }
        return medium;
    }

    public static float rowColMult(float12 row, float12 col)
    {
        float res = 0;
        for (int i = 0; i < 12; i++)
        {
            res += row.floats[i] * col.floats[i];
        }
        return res;
    }
    public override string ToString()
    {
        string str = "";
        for (int i = 0; i < 12; i++)
        {
            for (int j = 0; j < 12; j++)
            {
                str += floats[i, j] + ", ";
            }
            str += "\n";
        }
        return str;
    }
}