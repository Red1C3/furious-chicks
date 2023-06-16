using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RandLvlGenUI : MonoBehaviour
{
    [SerializeField]
    private GameObject[] dontDestroy;
    [SerializeField]
    private Text groundScale,stoneProb,woodProb,iceProb,objNum,pigNum;
    [SerializeField]
    private Toggle scaleX,scaleY;
    private void Start(){
        foreach(GameObject gameObject in dontDestroy){
            DontDestroyOnLoad(gameObject);
        }
    }

    private void generate(){
        RandomGen randomGen=FindObjectOfType<RandomGen>();
        randomGen.groundScale=float.Parse(groundScale.text);

        randomGen.blocksPros[0]=int.Parse(iceProb.text);
    
    }


}
