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
    private Toggle drag, angDrag, fRX, fRY, fRZ, fPX, fPY, fPZ, resting, gravity, isForce;
    [SerializeField]
    private Slider friction, bounciness;
    [SerializeField]
    private TMP_InputField mass, forceX, forceY, forceZ, veloX, veloY, veloZ, angVeloX, angVeloY, angVeloZ,
    health;
    private CreateOctree engine;
    [SerializeField]
    private TMP_Text minNodeSize, 
    maxNodeObjectsNum, collidingObjectsNum, generatedCollisionsNum, timeStep, fps,
    velocity, angularVelocity, voxelsCount;
    private RigidbodyDriver selectedRb;
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
        engine = FindObjectOfType<CreateOctree>();
        StartCoroutine(updateOctreeVals());
        StartCoroutine(updateTimeVals());
    }
    IEnumerator updateOctreeVals()
    {
        while (true)
        {
            minNodeSize.text = CreateOctree.nodeMinSize.ToString();
            maxNodeObjectsNum.text = CreateOctree.maxNodeObjectN.ToString();
            collidingObjectsNum.text = engine.getCullidingObjectsNum().ToString();
            generatedCollisionsNum.text = CreateOctree.culls.Count.ToString();
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
            { //Let's hope this is not a data race
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
        selected.GetComponent<RigidbodyDriver>().setDrag(val);
    }
    public void onAngDragChange(bool val)
    {
        selected.GetComponent<RigidbodyDriver>().setAngDrag(val);
    }
    public void onFPX(bool val)
    {
        selected.GetComponent<RigidbodyDriver>().setFreezePX(val);
    }
    public void onFPY(bool val)
    {
        selected.GetComponent<RigidbodyDriver>().setFreezePY(val);
    }
    public void onFPZ(bool val)
    {
        selected.GetComponent<RigidbodyDriver>().setFreezePZ(val);
    }
    public void onFRX(bool val)
    {
        selected.GetComponent<RigidbodyDriver>().setFreezeRX(val);
    }
    public void onFRY(bool val)
    {
        selected.GetComponent<RigidbodyDriver>().setFreezeRY(val);
    }
    public void onFRZ(bool val)
    {
        selected.GetComponent<RigidbodyDriver>().setFreezeRZ(val);
    }
    public void onResting(bool val)
    {
        selected.GetComponent<RigidbodyDriver>().setPsudoFreeze(val);
    }
    public void onGravity(bool val)
    {
        selected.GetComponent<RigidbodyDriver>().setGravity(val);
    }
    public void onMass(string massString)
    {
        selected.GetComponent<RigidbodyDriver>().setMass(float.Parse(massString));
    }
    public void onFriction(float friction)
    {
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
        selected.GetComponent<RigidbodyDriver>().addLinearVelocity(new Vector3(float.Parse(veloX.text), float.Parse(veloY.text), float.Parse(veloZ.text)));
    }
    public void addAngularVelocity()
    {
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
            FindObjectOfType<RandLvlGenUI>()?.unload();
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
}
