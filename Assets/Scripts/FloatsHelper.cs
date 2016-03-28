using UnityEngine;
using System.Collections;

public class FloatsHelper : MonoBehaviour {

    float rot;

    void Start()
    {
        do
        {
            rot = Random.Range(-0.04F, 0.041F);
        } while (Mathf.Abs(rot) <= 0.02F);
    }

    void Update()
    {
        transform.Rotate(transform.up * rot);
    }
}
