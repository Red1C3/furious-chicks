using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LvlUI : MonoBehaviour
{
    private RigidbodyDriver[] rigidbodies;
    private List<TMP_Dropdown.OptionData> optionDatas;
    [SerializeField]
    private TMP_Dropdown rbDropdown;
    private GameObject selected;


    [SerializeField]
    private Toggle drag, angDrag, fRX, fRY, fRZ, fPX, fPY, fPZ, resting, gravity;
    [SerializeField]
    private Slider friction, bounciness;
    [SerializeField]
    private TMP_InputField mass;

    private void Start()
    {
        updateOptionData();
        rbDropdown.onValueChanged.AddListener(onDropdownValueChange);
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
            mass.DeactivateInputField(false);
            friction.value = rb.voxelGrid.getFriction();
            bounciness.value = rb.voxelGrid.getBounciness();
        }
        else
        {
            mass.ActivateInputField();
            mass.text = rb.mass.ToString();
            Cullider cullider = rb.GetComponent<Cullider>();
            friction.value = cullider.getFrictionCo();
            bounciness.value = cullider.getBouncinessCo();
        }
    }
    private void unselectItem(GameObject selected)
    {
        Renderer renderer;
        if (selected.TryGetComponent<Renderer>(out renderer))
        {
            renderer.material.SetColor("_Color", Color.white);
        }
    }

}
