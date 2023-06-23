using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LvlUI : MonoBehaviour
{
    private RigidbodyDriver[] rigidbodies;
    private List<TMP_Dropdown.OptionData> optionDatas;
    [SerializeField]
    private TMP_Dropdown rbDropdown;
    private GameObject selected;


    [SerializeField]
    private Toggle drag, angDrag, fRX, fRY, fRZ, fPX, fPY, fPZ, resting, gravity, isForce,
    physicsPanelToggle, infoPanelToggle, healthPanelToggle, randomPanelToggle;
    [SerializeField]
    private Slider friction, bounciness;
    [SerializeField]
    private TMP_InputField mass, forceX, forceY, forceZ, veloX, veloY, veloZ, angVeloX, angVeloY, angVeloZ,
    health;
    private Engine engine;
    [SerializeField]
    private TMP_Text minNodeSize,
    maxNodeObjectsNum, collidingObjectsNum, generatedCollisionsNum, timeStep, fps,
    velocity, angularVelocity, voxelsCount;
    private RigidbodyDriver selectedRb;
    [SerializeField]
    private RectTransform rbPanel, infoPanel, healthPanel, rbdd;
    private void Start()
    {
        updateOptionData();
        rbDropdown.onValueChanged.AddListener(onDropdownValueChange);
        drag.onValueChanged.AddListener(onDragChange);
        angDrag.onValueChanged.AddListener(onAngDragChange);
        fRX.onValueChanged.AddListener(onFRX);
        fRY.onValueChanged.AddListener(onFRY);
        fRZ.onValueChanged.AddListener(onFRZ);
        fPX.onValueChanged.AddListener(onFPX);
        fPY.onValueChanged.AddListener(onFPY);
        fPZ.onValueChanged.AddListener(onFPZ);
        resting.onValueChanged.AddListener(onResting);
        gravity.onValueChanged.AddListener(onGravity);
        mass.onSubmit.AddListener(onMass);
        health.onSubmit.AddListener(onHealth);
        friction.onValueChanged.AddListener(onFriction);
        bounciness.onValueChanged.AddListener(onBounciness);


        physicsPanelToggle.onValueChanged.AddListener(togglePhysicsPanel);
        togglePhysicsPanel(physicsPanelToggle.isOn);
        infoPanelToggle.onValueChanged.AddListener(toggleInfoPanel);
        toggleInfoPanel(infoPanelToggle.isOn);
        healthPanelToggle.onValueChanged.AddListener(toggleHealthPanel);
        toggleHealthPanel(healthPanelToggle.isOn);
        if (FindObjectOfType<RandLvlGenUI>() != null)
        {
            randomPanelToggle.onValueChanged.AddListener(toggleRandomPanel);
            toggleRandomPanel(randomPanelToggle.isOn);
        }
        else
        {
            randomPanelToggle.gameObject.SetActive(false);
        }
        engine = FindObjectOfType<Engine>();
        StartCoroutine(updateOctreeVals());
        StartCoroutine(updateTimeVals());
    }
    IEnumerator updateOctreeVals()
    {
        while (true)
        {
            minNodeSize.text = Engine.nodeMinSize.ToString();
            maxNodeObjectsNum.text = Engine.maxNodeObjectN.ToString();
            collidingObjectsNum.text = engine.getCullidingObjectsNum().ToString();
            generatedCollisionsNum.text = Engine.culls.Count.ToString();
            yield return new WaitForSeconds(1);
        }
    }
    IEnumerator updateTimeVals()
    {
        while (true)
        {
            timeStep.text = Time.fixedDeltaTime.ToString();
            fps.text = (1.0f / Time.deltaTime).ToString();
            if (selectedRb != null)
            {
                velocity.text = "V :" + selectedRb.velocity.x.ToString("0.00") + " ," +
                selectedRb.velocity.y.ToString("0.00") + " ,"
                + selectedRb.velocity.z.ToString("0.00");
                Vector3 angVel = selectedRb.getAngularVelocity();
                angularVelocity.text = "AngV :" + angVel.x.ToString("0.00") + " ," +
                angVel.y.ToString("0.00") + " ,"
                + angVel.z.ToString("0.00");

            }
            yield return new WaitForSeconds(0.2f);
        }
    }
    private void updateOptionData()
    {
        rigidbodies = FindObjectsOfType<RigidbodyDriver>();
        optionDatas = new List<TMP_Dropdown.OptionData>(rigidbodies.Length);
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            optionDatas.Add(new TMP_Dropdown.OptionData(rigidbodies[i].name.Replace("(Clone)", "")));
        }
        rbDropdown.options = optionDatas;
    }

    public void onDropdownValueChange(int idx)
    {
        if (selected != null) unselectItem(selected);
        if (rigidbodies[idx] != null)
        {
            selectItem(rigidbodies[idx].gameObject);
        }
        updateOptionData();
    }

    private void selectItem(GameObject selected)
    {
        this.selected = selected;
        Renderer renderer;
        if (selected.TryGetComponent<Renderer>(out renderer))
        {
            renderer.material.SetColor("_Color", Color.red);
        }
        VoxelGrid voxelGrid;
        if (selected.TryGetComponent<VoxelGrid>(out voxelGrid))
        {
            foreach (Renderer renderer1 in selected.GetComponentsInChildren<Renderer>())
            {
                renderer1.material.SetColor("_Color", Color.red);
            }
        }
        RigidbodyDriver rb = selected.GetComponent<RigidbodyDriver>();
        drag.isOn = rb.getDrag();
        angDrag.isOn = rb.getAngularDrag();
        fPX.isOn = rb.getFreezePX();
        fPY.isOn = rb.getFreezePY();
        fPZ.isOn = rb.getFreezePZ();
        fRX.isOn = rb.getFreezeRX();
        fRY.isOn = rb.getFreezeRY();
        fRZ.isOn = rb.getFreezeRZ();
        resting.isOn = rb.psudoFreeze;
        gravity.isOn = rb.useGravity;

        if (rb.voxelGrid != null)
        {
            mass.text = rb.mass.ToString();
            mass.interactable = false;
            friction.value = rb.voxelGrid.getFriction();
            bounciness.value = rb.voxelGrid.getBounciness();
            voxelsCount.text = "Voxels #: " + rb.transform.childCount.ToString();
        }
        else
        {
            mass.interactable = true;
            mass.text = rb.mass.ToString();
            Cullider cullider = rb.GetComponent<Cullider>();
            friction.value = cullider.getFrictionCo();
            bounciness.value = cullider.getBouncinessCo();
            voxelsCount.text = "Voxels #: 0";
        }
        selectedRb = rb;
        int healthVal = getSelectedHealth();
        if (healthVal != -1)
        {
            health.interactable = true;
            health.text = healthVal.ToString();
        }
        else
        {
            health.interactable = false;
            health.text = "N/A";
        }
    }
    private void unselectItem(GameObject selected)
    {
        Renderer renderer;
        if (selected.TryGetComponent<Renderer>(out renderer))
        {
            renderer.material.SetColor("_Color", Color.white);
        }
        VoxelGrid voxelGrid;
        if (selected.TryGetComponent<VoxelGrid>(out voxelGrid))
        {
            foreach (Renderer renderer1 in selected.GetComponentsInChildren<Renderer>())
            {
                renderer1.material.SetColor("_Color", Color.white);
            }
        }
    }
    public void onDragChange(bool val)
    {
        if (selected == null) return;
        selected.GetComponent<RigidbodyDriver>().setDrag(val);
    }
    public void onAngDragChange(bool val)
    {
        if (selected == null) return;
        selected.GetComponent<RigidbodyDriver>().setAngDrag(val);
    }
    public void onFPX(bool val)
    {
        if (selected == null) return;
        selected.GetComponent<RigidbodyDriver>().setFreezePX(val);
    }
    public void onFPY(bool val)
    {
        if (selected == null) return;
        selected.GetComponent<RigidbodyDriver>().setFreezePY(val);
    }
    public void onFPZ(bool val)
    {
        if (selected == null) return;
        selected.GetComponent<RigidbodyDriver>().setFreezePZ(val);
    }
    public void onFRX(bool val)
    {
        if (selected == null) return;
        selected.GetComponent<RigidbodyDriver>().setFreezeRX(val);
    }
    public void onFRY(bool val)
    {
        if (selected == null) return;
        selected.GetComponent<RigidbodyDriver>().setFreezeRY(val);
    }
    public void onFRZ(bool val)
    {
        if (selected == null) return;
        selected.GetComponent<RigidbodyDriver>().setFreezeRZ(val);
    }
    public void onResting(bool val)
    {
        if (selected == null) return;
        selected.GetComponent<RigidbodyDriver>().setPsudoFreeze(val);
    }
    public void onGravity(bool val)
    {
        if (selected == null) return;
        selected.GetComponent<RigidbodyDriver>().setGravity(val);
    }
    public void onMass(string massString)
    {
        if (selected == null) return;
        float massFloat = float.Parse(massString);
        massFloat = Mathf.Clamp(massFloat, 1e-5f, 1e7f);
        selected.GetComponent<RigidbodyDriver>().setMass(massFloat);
    }
    public void onFriction(float friction)
    {
        if (selected == null) return;
        if (selected.GetComponent<VoxelGrid>() == null)
        {
            BoxCullider box;
            SphereCullider sphere;
            if (selected.TryGetComponent<BoxCullider>(out box))
            {
                box.setFrictionCo(friction);
            }
            if (selected.TryGetComponent<SphereCullider>(out sphere))
            {
                sphere.setFrictionCo(friction);
            }
        }
        else
        {
            VoxelCullider[] culliders = selected.GetComponentsInChildren<VoxelCullider>();
            foreach (VoxelCullider cullider in culliders)
            {
                cullider.setFrictionCo(friction);
            }
        }
    }
    public void onBounciness(float bounciness)
    {
        if (selected == null) return;
        if (selected.GetComponent<VoxelGrid>() == null)
        {
            BoxCullider box;
            SphereCullider sphere;
            if (selected.TryGetComponent<BoxCullider>(out box))
            {
                box.setBouncinessCo(bounciness);
            }
            if (selected.TryGetComponent<SphereCullider>(out sphere))
            {
                sphere.setBouncinessCo(bounciness);
            }
        }
        else
        {
            VoxelCullider[] culliders = selected.GetComponentsInChildren<VoxelCullider>();
            foreach (VoxelCullider cullider in culliders)
            {
                cullider.setBouncinessCo(bounciness);
            }
        }
    }
    public void addForce()
    {
        if (selected == null) return;
        if (isForce.isOn)
        {
            selected.GetComponent<RigidbodyDriver>().addForce(new Vector3(float.Parse(forceX.text), float.Parse(forceY.text), float.Parse(forceZ.text)), ForceMode.Force);
        }
        else
        {
            selected.GetComponent<RigidbodyDriver>().addForce(new Vector3(float.Parse(forceX.text), float.Parse(forceY.text), float.Parse(forceZ.text)), ForceMode.Impulse);
        }
    }
    public void addVeloicty()
    {
        if (selected == null) return;
        selected.GetComponent<RigidbodyDriver>().addLinearVelocity(new Vector3(float.Parse(veloX.text), float.Parse(veloY.text), float.Parse(veloZ.text)));
    }
    public void addAngularVelocity()
    {
        if (selected == null) return;
        selected.GetComponent<RigidbodyDriver>().addAngularVelocity(new Vector3(float.Parse(angVeloX.text), float.Parse(angVeloY.text), float.Parse(angVeloZ.text)));
    }
    private void OnDestroy()
    {
        StopAllCoroutines();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            FindObjectOfType<RandLvlGenUI>(true)?.unload();
            SceneManager.LoadScene("main menu");
        }
    }
    private void onHealth(string val)
    {
        if (selected == null) return;
        SingleObstacleBase singleObstacleBase;
        PigBase pigBase;
        MultiObstacleBase multiObstacleBase;
        if (selected.TryGetComponent<SingleObstacleBase>(out singleObstacleBase))
        {
            singleObstacleBase.setHealth(int.Parse(val));
        }
        if (selected.TryGetComponent<PigBase>(out pigBase))
        {
            pigBase.setHealth(int.Parse(val));
        }
        if (selected.TryGetComponent<MultiObstacleBase>(out multiObstacleBase))
        {
            multiObstacleBase.setHealth(int.Parse(val));
        }
    }
    private int getSelectedHealth()
    {
        if (selected == null) return -1;
        SingleObstacleBase singleObstacleBase;
        PigBase pigBase;
        MultiObstacleBase multiObstacleBase;
        if (selected.TryGetComponent<SingleObstacleBase>(out singleObstacleBase))
        {
            return singleObstacleBase.getHealth();
        }
        if (selected.TryGetComponent<PigBase>(out pigBase))
        {
            return pigBase.getHealth();
        }
        if (selected.TryGetComponent<MultiObstacleBase>(out multiObstacleBase))
        {
            return multiObstacleBase.getHealth();
        }
        return -1;
    }
    public void togglePhysicsPanel(bool val)
    {
        GetComponent<Image>().enabled = val;
        rbdd.gameObject.SetActive(val);
        rbPanel.gameObject.SetActive(val);
    }
    public void toggleInfoPanel(bool val)
    {
        infoPanel.gameObject.SetActive(val);
    }
    public void toggleHealthPanel(bool val)
    {
        healthPanel.gameObject.SetActive(val);
    }
    public void toggleRandomPanel(bool val)
    {
        FindObjectOfType<RandLvlGenUI>(true).gameObject.SetActive(val);
    }
    public Toggle getRbPanelToggle(){
        return physicsPanelToggle;
    }
    public Toggle getInfoPanelToggle(){
        return infoPanelToggle;
    }
    public Toggle getHealthPanelToggle(){
        return healthPanelToggle;
    }
    public Toggle getRandomPanelToggle(){
        return randomPanelToggle;
    }
}
