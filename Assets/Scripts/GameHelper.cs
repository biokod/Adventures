using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameHelper : MonoBehaviour {

    public Transform player;

    public int KillCounter { get; set; }
    public Text Counter { get; set; }

    int slimeNum;
	void Start ()
    {
        Invoke("CreateSlime", 10.0F);

        slimeNum = 1;
        KillCounter = 0;
        Counter = FindObjectOfType<Text>();
    }

    void Update()
    {
        slimeNum = FindObjectsOfType<SlimeHelper>().Length;

        Counter.text = KillCounter.ToString();
    }

    void CreateSlime()
    {
        if (slimeNum == 7)
        {
            Invoke("CreateSlime", 1.0F);
            return;
        }

        Vector3 spawnPosition;
        float distance;
        do
        {
            spawnPosition = new Vector3(Random.Range(-9.0F, 10.0F), 0.0F, Random.Range(-9.0F, 10.0F));
            distance = Vector3.Distance(player.position, spawnPosition);
        } while (Mathf.Abs(spawnPosition.x) <= 2.0F && Mathf.Abs(spawnPosition.z) <= 2.0F && distance <= 3.0F);
        
        Instantiate(Resources.Load<GameObject>("Slime"), spawnPosition, Quaternion.identity);

        Invoke("CreateSlime", 5.0F);
    }
}
