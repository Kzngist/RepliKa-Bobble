using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeButtonUIHandler : MonoBehaviour
{
    public void OnHomeButton()
    {
        SceneLoadManager.Instance.LoadScene("MainMenu");
    }
}
