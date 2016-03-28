using UnityEngine;
using System.Collections;

public class IslandHelper : MonoBehaviour {
    float rot;

    void Start()
    {
        GetComponent<Animation>()["Floating"].speed = Random.Range(0.1F, 0.31F);
        float scale = Random.Range(0.35F, 1.0F);
        transform.localScale = new Vector3(scale, scale * Random.Range(0.8F, 2.0F), scale);

        do
        {
            rot = Random.Range(-0.4F, 0.41F);
        } while (Mathf.Abs(rot) < 0.15F);
    }

    void Update()
    {
        transform.Rotate(transform.up * rot);
    }
}
