using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextUpdater : MonoBehaviour
{
    [SerializeField] private EncoderCounter encoderCounter;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (encoderCounter == null)
        {
            return;
        }
        if (encoderCounter.IsAvailable)
        {
            string text = $"Count: {encoderCounter.Count}, Revolution: {encoderCounter.NumberOfRevolution}";
            GetComponent<Text>().text = text;
        }
        else
        {
            GetComponent<Text>().text = "Encoder is not available";
        }
    }
}
