using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SessionManager : MonoBehaviour
{
    public TextMeshProUGUI textTitle;
    public TextMeshProUGUI textSubTitle;
    public TextMeshProUGUI textInstruction;
    public TextMeshProUGUI textNumber;

    public GameObject buttonNextSession;
    public GameObject buttonStartCompeting;
    public GameObject buttonValidate;
    public GameObject[] sessionCakes;
    public Transform targetCake;

    public int numberOfTraining = 5;
    public int numberOfCompeting = 10;

    private string instruction = "- {0} the cake to the target. \n- Be as precise and fast as you can. \n- Press the button below to validate.";
    private int sessionId = -1;
    private int trialId = 0;
    private bool isTraining;

    void Start()
    {
        NextSession();
    }

    void Update()
    {
        
    }

    public void NextSession()
    {
        buttonNextSession.SetActive(false);
        trialId = 0;
        sessionId += 1;
        if (sessionId < sessionCakes.Length)
        {
            buttonValidate.SetActive(true);
            isTraining = true;
            textTitle.text = "TRAINING";
            string sessionName = sessionCakes[sessionId].name;
            textSubTitle.text = sessionName.Remove(sessionName.Length - 1).ToUpper() + "ING";
            textInstruction.text = string.Format(instruction, sessionName);
            textNumber.text = trialId + "/" + numberOfTraining;
        }
        else
        {
            buttonValidate.SetActive(false);
            textTitle.text = "FINISH";
            textSubTitle.text = "THANK YOU";
            textInstruction.text = "- Remove the virtual realit headset. \n- Answer the questionnaire.";
            textNumber.text = "oo";
        }
    }

    public void StartCompeting()
    {
        trialId = 0;
        isTraining = false;
        textTitle.text = "COMPETING";
        textNumber.text = trialId + "/" + numberOfCompeting;
        buttonStartCompeting.SetActive(false);
        buttonValidate.SetActive(true);
    }

    public void Validate()
    {
        trialId += 1;
        if (isTraining)
        {
            textNumber.text = trialId + "/" + numberOfTraining;
            if (trialId >= numberOfTraining)
            {
                buttonStartCompeting.SetActive(true);
                buttonValidate.SetActive(false);
            }
        }
        else 
        {
            textNumber.text = trialId + "/" + numberOfCompeting;
            if (trialId >= numberOfCompeting)
            {
                buttonNextSession.SetActive(true);
                buttonValidate.SetActive(false);
            }
        }
    }

    private void MoveTarget()
    {
        targetCake.localPosition = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
    }

    private void RotateTarget()
    {
        targetCake.localRotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
    }

    private void ScaleTarget()
    {
        targetCake.localScale = new Vector3(Random.Range(0.25f, 0.75f), Random.Range(0.10f, 0.30f), Random.Range(0.25f, 0.75f));
    }
}