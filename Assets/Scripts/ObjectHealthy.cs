using Photon.Chat.UtilityScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectHealthy : MonoBehaviour
{
    public float healthy = 20f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public float takeDamage(float amount, float playerHealth)
    {
        float cur_health = 0f;

        if (healthy > 0f)
        {
            healthy -= amount;
            cur_health = playerHealth;
            Debug.Log("Mushroom not dead");
        }


        if (healthy <= 0f)
        {
            Destroy(this.gameObject);
            cur_health = playerHealth + 20;
            Text txt = GameObject.Find("MushroomSurprise").GetComponent<Text>();
            txt.text = "Mushroom Surprise: Your HP is up!";

            if (cur_health > 100f)
            {
                cur_health = 100f;
                Debug.Log("Health back to 100");
            }
        }

        return cur_health;

    }
}
