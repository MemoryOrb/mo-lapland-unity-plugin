using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

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

    private string pathFileName = "logs.txt";

    void Start()
    {
        NextSession();
    }

    void Update()
    {
        
    }

    public void NextSession()
    {
        SaveLog("NEXT-SESSION");
        ResetCake(targetCake);
        buttonNextSession.SetActive(false);
        trialId = 0;
        sessionId += 1;
        ClearCake();
        sessionCakes[sessionId].SetActive(true);
        if (sessionId < sessionCakes.Length)
        {
            SetTarget();
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
        SaveLog("START-COMPETING");
        SetTarget();
        trialId = 0;
        isTraining = false;
        textTitle.text = "COMPETING";
        textNumber.text = trialId + "/" + numberOfCompeting;
        buttonStartCompeting.SetActive(false);
        buttonValidate.SetActive(true);
    }

    public void Validate()
    {
        SaveLog(string.Format("VALIDATE,{0},{1}", GameObjectToString(sessionCakes[sessionId]), TransformToString(targetCake)));
        SetTarget();
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

    private void SetTarget() 
    {
        ResetCake(sessionCakes[sessionId].transform);
        
        string sessionName = sessionCakes[sessionId].name;
        switch (sessionName)
        {
            case "Move":
                MoveTarget();
            break;
            case "Rotate":
                RotateTarget();
            break;
            case "Scale":
                ScaleTarget();
            break;
            case "Manipulate":
            default:
                MoveTarget();
                RotateTarget();
                ScaleTarget();
            break;
        }
    }

    private void ClearCake()
    {
        for (int i = 0; i < sessionCakes.Length; i++)
        {
            sessionCakes[i].SetActive(false);
        }
    }

    private void ResetCake(Transform t)
    {
        t.localPosition = new Vector3(0f, 0f, 0f);
        t.localRotation = Quaternion.Euler(0f, 0f, 0f);
        t.localScale = new Vector3(0.5f, 0.2f, 0.5f);
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


    /*
    *   LOGS
    */

    string GameObjectToString(GameObject g)
    {
        return g.name + "," + TransformToString(g.transform);
    }

    string TransformToString(Transform t)
    {
        return  t.localPosition.x + "," + 
                t.localPosition.y + "," + 
                t.localPosition.z + "," + 
                t.localRotation.x + "," + 
                t.localRotation.y + "," + 
                t.localRotation.z + "," +
                t.localScale.x + "," +
                t.localScale.y + "," +
                t.localScale.z;
    }

    void SaveLog(string data)
    {
        File.AppendAllText(pathFileName, string.Format("{0},{1},{2},{3},{4}\n", System.DateTime.Now.ToString("HH:mm:ss.fff"), sessionId, isTraining, trialId, data));
    }

    public void TranslateStart(GameObject g)
    {
        SaveLog("TRANSLATE-START," + GameObjectToString(g));
    }

    public void TranslateStop(GameObject g)
    {
        SaveLog("TRANSLATE-STOP," + GameObjectToString(g));
    }

    public void RotateStart(GameObject g)
    {
        SaveLog("ROTATE-START," + GameObjectToString(g));
    }

    public void RotateStop(GameObject g)
    {
        SaveLog("ROTATE-STOP," + GameObjectToString(g));
    }

    public void ScaleStart(GameObject g)
    {
        SaveLog("SCALE-START," + GameObjectToString(g));
    }

    public void ScaleStop(GameObject g)
    {
        SaveLog("SCALE-STOP," + GameObjectToString(g));
    }

    public void ManipulationStarted(GameObject g)
    {
        SaveLog("MANIPULATION-START," + GameObjectToString(g));
    }

    public void ManipulationEnded(GameObject g)
    {
        SaveLog("MANIPULATION-END," + GameObjectToString(g));
    }
}