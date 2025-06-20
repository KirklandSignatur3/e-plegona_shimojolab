using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CirclePattern : MonoBehaviour
{
    [SerializeField] GameObject circleOrigin;
    // Start is called before the first frame update
    int count = 0;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        count++;
        if (count > 15)
        {
            var circle = Instantiate(circleOrigin, Vector3.zero, Quaternion.identity);
            circle.transform.SetParent(this.transform);
            circle.transform.localPosition = UnityEngine.Random.onUnitSphere * 0.75f;
            circle.GetComponent<Circle>().Show();
            count = 0;
        }
    }
}
