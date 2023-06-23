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
    private GameObject lvlUi;
    private GameObject lvUiInstance;
    [SerializeField]
    private TMP_InputField groundScale, stoneProb, woodProb, iceProb, objNum, pigNum;
    [SerializeField]
    private Toggle scaleX, scaleZ;
    private RandomGen randomGen;
    private bool physicsPanel, infoPanel, healthPanel;
    private void Start()
    {
        foreach (GameObject gameObject in dontDestroy)
        {
            DontDestroyOnLoad(gameObject);
        }
        randomGen = FindObjectOfType<RandomGen>();
        lvUiInstance = Instantiate(lvlUi, dontDestroy[5].transform);
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

        if (lvUiInstance != null)
        {
            physicsPanel = lvUiInstance.GetComponent<LvlUI>().getRbPanelToggle().isOn;
            infoPanel = lvUiInstance.GetComponent<LvlUI>().getInfoPanelToggle().isOn;
            healthPanel = lvUiInstance.GetComponent<LvlUI>().getHealthPanelToggle().isOn;
            Destroy(lvUiInstance);
        }
        SceneManager.LoadScene("empty");
    }

    private void onSceneLoad(Scene scene, LoadSceneMode mode)
    {
        randomGen.Awake();
        lvUiInstance = Instantiate(lvlUi, dontDestroy[5].transform);
        lvUiInstance.GetComponent<LvlUI>().getHealthPanelToggle().isOn = healthPanel;
        lvUiInstance.GetComponent<LvlUI>().getInfoPanelToggle().isOn = infoPanel;
        lvUiInstance.GetComponent<LvlUI>().getRbPanelToggle().isOn = physicsPanel;
        lvUiInstance.GetComponent<LvlUI>().getRandomPanelToggle().isOn=true;
    }
    public void unload()
    {
        SceneManager.sceneLoaded -= onSceneLoad;
        foreach (GameObject go in dontDestroy)
        {
            Destroy(go);
        }
    }
}
