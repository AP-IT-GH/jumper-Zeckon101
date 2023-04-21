using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingObject : MonoBehaviour
{
    private void Update()
    {
        //transform.Translate(Vector3.back * speed * Time.deltaTime);
        transform.Rotate(new Vector3(0.5f, 0, 0));
    }
}

