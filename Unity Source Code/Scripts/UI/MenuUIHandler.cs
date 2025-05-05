using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuUIHandler : MonoBehaviour
{
    [SerializeField] internal MenuUIHandler previousMenu;
    
    internal void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
}
