using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
public class Controls : MonoBehaviour
{
    //Variables inicializadas en el editor
    [SerializeField] Camera m_camera;
    [SerializeField] TMP_Dropdown dropdown;
    [SerializeField] RectTransform dropdownRect;
    [SerializeField] Button startButton;
    //Al ser un struct que manipulo en codigo no lo serializo
    Vector3 clickedPos;
    bool isGameInitiated;
    private void Start()
    {
        PopulateTurretDropdown();
        startButton.onClick.AddListener(OnStart);
    }
    // Lleno el dropdown de torretas
    void PopulateTurretDropdown()
    {
        List<string> items = new List<string>();
        foreach (TurretType item in Enum.GetValues(typeof(TurretType)))
        {
            string itemName = item.ToString();
            items.Add(itemName);
        }
        dropdown.AddOptions(items);
        dropdown.onValueChanged.AddListener((x) => CastTurretInSandbox((TurretType)x));
    }

    //Inputs de usuario
    void Update()
    {
        if (isGameInitiated)
        {
            if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                RaycastHit hit;
                Vector3 inputMouse = Input.mousePosition;
                Ray ray = m_camera.ScreenPointToRay(inputMouse);
                //Debug.DrawRay(ray.origin, ray.direction, Color.green); //motivos de debug
                if (Physics.Raycast(ray, out hit, 1000f))
                {
                    clickedPos = hit.point;
                    CastTurretDropdown(inputMouse);
                }
            }
        }
    }
    void OnStart()
    {
        startButton.gameObject.SetActive(false);
        GameCore.Instance.StartGame();
        isGameInitiated = true;
    }
    void CastTurretInSandbox(TurretType selectdItem)
    {
        var gamecore = GameCore.Instance;
        if (gamecore)
        {
            DropdownStatus(false);
            gamecore.CastTurret(selectdItem, clickedPos);
            //Para regresar el dropdown a SelectTurret
            dropdown.value = 0;
        }
    }
    // Obtener la posicion del mouse y mover la UI dropdown ahi. TODO: Modificar el pivot en editor
    void CastTurretDropdown(Vector3 input)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(dropdownRect.parent.transform as RectTransform, input, null, out pos);
        dropdownRect.localPosition = pos;
        DropdownStatus(true);
    }
    void DropdownStatus(bool value) => dropdown.gameObject.SetActive(value);
}