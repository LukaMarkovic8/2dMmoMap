using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    public float time = 3f;
    // Start is called before the first frame update
    void Start()
    {
        //destroying the name indicator so it won't stay there forever
        Destroy(this.gameObject, time);
    }

    
}
