using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreloadingShaders : MonoBehaviour
{
    [SerializeField] GameObject Real;
    [SerializeField] GameObject Fake;
    [SerializeField] Text textspot;

    private int I;
    private Coroutine _waitingCoroutine;

    void Start()
    {
        Real.SetActive(false);
        Fake.SetActive(true);
        _waitingCoroutine = StartCoroutine(WaitingForEver());
    }

    IEnumerator WaitingForEver()
    { 

        Real.SetActive(false);
        Fake.SetActive(true);
        I = 0;
        textspot.text = "Preloading shaders..." + I.ToString() + "%";

        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1, 7));
            I = I + Random.Range(-1, 15);
            if (I < 0) I = 0;
            if (I > 100) I = 100;
            Debug.Log(I);
            textspot.text = "Preloading shaders..." + I.ToString() + "%";
            if (I == 100)

            {   Real.SetActive(true);
                Fake.SetActive(false);
                textspot.text = I.ToString() + "% - Done loading";

                break;
            }
        }
    }

    void OnDestroy()
    {
        if (_waitingCoroutine != null)
            StopCoroutine(_waitingCoroutine);
    }
}
