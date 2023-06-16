using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RandLvlGenUI : MonoBehaviour
{
    [SerializeField]
    private GameObject[] dontDestroy;
    [SerializeField]
    private TMP_InputField groundScale, stoneProb, woodProb, iceProb, objNum, pigNum;
    [SerializeField]
    private Toggle scaleX, scaleZ;
    private RandomGen randomGen;
    private void Start()
    {
        foreach (GameObject gameObject in dontDestroy)
        {
            DontDestroyOnLoad(gameObject);
        }
        randomGen = FindObjectOfType<RandomGen>();
        SceneManager.sceneLoaded += onSceneLoad;
    }

    public void generate()
    {
        randomGen.groundScale = float.Parse(groundScale.text);

        randomGen.blocksPros[0] = int.Parse(iceProb.text);
        randomGen.blocksPros[1] = int.Parse(stoneProb.text);
        randomGen.blocksPros[2] = int.Parse(woodProb.text);

        randomGen.objectNumber = int.Parse(objNum.text);
        randomGen.pigsNumber = int.Parse(pigNum.text);

        randomGen.scaleX = scaleX.isOn;
        randomGen.scaleZ = scaleZ.isOn;

        SceneManager.LoadScene("empty");
    }

    private void onSceneLoad(Scene scene, LoadSceneMode mode)
    {
        randomGen.Awake();
    }


}
