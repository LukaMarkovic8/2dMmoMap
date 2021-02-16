using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FpsCounter : MonoBehaviour
{
    public TextMeshProUGUI text;

    void Update()
    {
        int fps = (int)(1f / Time.unscaledDeltaTime);
        text.text = "FPS:" + fps;

  
    }
}
