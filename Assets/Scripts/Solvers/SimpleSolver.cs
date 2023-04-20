using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSolver : MonoBehaviour
{
    private List<GameObject> cullidingObject;
    void Start()
    {
        cullidingObject = new List<GameObject>();
        GameObject[] gameObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject gameObject in gameObjects)
        {
            Cullider cullider;
            if (gameObject.TryGetComponent<Cullider>(out cullider))
            {
                cullidingObject.Add(gameObject);
            }
        }
    }

    void FixedUpdate()
    {
        for (int i = 0; i < cullidingObject.Count; i++)
        {
            for (int j = i + 1; j < cullidingObject.Count; j++)
            {
                resolveCullision(cullidingObject[i].GetComponent<Cullider>().cullideWith(cullidingObject[j].GetComponent<Cullider>()),
                cullidingObject[i].GetComponent<Rigidbody>(), cullidingObject[j].GetComponent<Rigidbody>());
            }
        }
    }

    void resolveCullision(CullisionInfo cullision, Rigidbody A, Rigidbody B)
    {
        if (!cullision.cullided) return;

        cullision.depth*=10;

        if (cullision.hasContactPointA)
        {
            A.AddForceAtPosition(cullision.normal.normalized * cullision.depth, cullision.contactPointA, ForceMode.Impulse);
        }
        else
        {
            A.AddForce(cullision.normal.normalized * cullision.depth, ForceMode.Impulse);
        }

        if (cullision.hasContactPointB)
        {
            B.AddForceAtPosition(-cullision.normal.normalized * cullision.depth, cullision.contactPointB, ForceMode.Impulse);
        }
        else
        {
            B.AddForce(-cullision.normal.normalized * cullision.depth, ForceMode.Impulse);
        }
    }
}
