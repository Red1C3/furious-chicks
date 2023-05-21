using UnityEngine;

public struct float12x12
{
    public float[,] floats;
    private static float12 medium;
    public float12x12(float firstThreeDiag, float secondThreeDiag, float thirdThreeDiag, float fourthThreeDiag)
    {
        if (medium.floats == null) medium.floats = new float[12];
        floats = new float[12, 12];
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

    public static float12 operator *(float12 vec, float12x12 mat)
    {
        float12 res = new float12();
        res.floats = new float[12];

        for (int i = 0; i < 12; i++)
        {
            res.floats[i] = rowColMult(vec, mat.column(i));
        }

        return res;
    }

    public static float12 operator *(float12x12 mat, float12 vec)
    {
        float12 res = new float12();
        res.floats = new float[12];
        for (int i = 0; i < 12; i++)
        {
            res.floats[i] = rowColMult(mat.row(i), vec);
        }
        return res;
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