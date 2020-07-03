using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FastShootTimer : MonoBehaviour
{
    private Text txt;
    public float second = 1.0f;
    // Start is called before the first frame update
    void Start()
    {
        txt = this.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKey(KeyCode.B))
        {
            this.second = 1f;
        }
        if (second > 0)
        {
            second -= Time.deltaTime;
        }
        if (second <= 0)
        {
            txt.text = "";
        }
    }
}
