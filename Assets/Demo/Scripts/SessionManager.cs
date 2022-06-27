using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.UI;

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

    private System.DateTime firstActionTime;
    private System.DateTime lastActionTime;
    private bool hasStarted = false;

    public MemoryOrbManager memoryOrbManager;
    public GameObject tracker;

    public bool memoryOrbActivated = true;

    void Awake()
    {
        pathFileName = System.DateTime.Now.ToString("ddMM") + "_logs.txt";
    }
    
    void Start()
    {
        firstActionTime = System.DateTime.Now;
        lastActionTime = System.DateTime.Now;
        
        if (memoryOrbActivated)
        {
            foreach (GameObject g in sessionCakes)
            {
                var b = g.GetComponent<BoundsControl>();
                b.enabled = false;
                var o = g.GetComponent<ObjectManipulator>();
                o.enabled = false;
                
                var m = g.GetComponent<MemoryOrbManipulator>();
                m.enabled = true;
            }
        }
        else {
            tracker.SetActive(false);
        }

        NextSession();
    }

    void Update()
    {
        
    }

    public void NextSession()
    {
        hasStarted = false;
        SaveLog("NEXT-SESSION");
        ResetCake(targetCake);
        buttonNextSession.SetActive(false);
        trialId = 0;
        sessionId += 1;
        ClearCake();
        if (sessionId < sessionCakes.Length)
        {
            sessionCakes[sessionId].SetActive(true);
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
        sessionCakes[sessionId].SetActive(true);
        hasStarted = false;
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
        hasStarted = false;
        SaveLog(string.Format("VALIDATE,{0},{1},{2},{3},{4}", 
            GameObjectToString(sessionCakes[sessionId]), 
            TransformToString(targetCake),
            DifferenceTransformToString(targetCake, sessionCakes[sessionId].transform),
            (lastActionTime - firstActionTime).TotalMilliseconds,
            (System.DateTime.Now - firstActionTime).TotalMilliseconds
        ));
        
        SetTarget();
        trialId += 1;
        if (isTraining)
        {
            textNumber.text = trialId + "/" + numberOfTraining;
            if (trialId >= numberOfTraining)
            {
                ClearCake();
                buttonStartCompeting.SetActive(true);
                buttonValidate.SetActive(false);
            }
        }
        else 
        {
            textNumber.text = trialId + "/" + numberOfCompeting;
            if (trialId >= numberOfCompeting)
            {
                ClearCake();
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
        targetCake.localPosition = new Vector3((Random.Range(-50, 51)/100f), (Random.Range(-50, 51)/100f), (Random.Range(-50, 51)/100f));
    }

    private void RotateTarget()
    {
        targetCake.localRotation = Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
    }

    private void ScaleTarget()
    {
        targetCake.localScale = new Vector3((Random.Range(25, 76)/100f), (Random.Range(10, 31)/100f), (Random.Range(25, 76)/100f));
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
                t.localEulerAngles.x + "," + 
                t.localEulerAngles.y + "," + 
                t.localEulerAngles.z + "," +
                t.localScale.x + "," +
                t.localScale.y + "," +
                t.localScale.z;
    }

    string DifferenceTransformToString(Transform t1, Transform t2)
    {
        return  (t1.localPosition.x - t2.localPosition.x) + "," +
                (t1.localPosition.y - t2.localPosition.y) + "," +
                (t1.localPosition.z - t2.localPosition.z) + "," +
                Mathf.DeltaAngle(t1.localEulerAngles.x, t2.localEulerAngles.x) + "," +
                Mathf.DeltaAngle(t1.localEulerAngles.x, t2.localEulerAngles.x) + "," +
                Mathf.DeltaAngle(t1.localEulerAngles.x, t2.localEulerAngles.x) + "," +
                (t1.localScale.x - t2.localScale.x) + "," +
                (t1.localScale.y - t2.localScale.y) + "," +
                (t1.localScale.z - t2.localScale.z);       
    }

    void SaveLog(string data)
    {
        File.AppendAllText(pathFileName, string.Format("{0},{1},{2},{3},{4},{5}\n", System.DateTime.Now.ToString("HH:mm:ss.fff"), memoryOrbActivated, sessionId, isTraining, trialId, data));
    }

    public void TranslateStart(GameObject g)
    {
        if (!hasStarted)
        {
            firstActionTime = System.DateTime.Now;
            hasStarted = true;
        }
        SaveLog("TRANSLATE-START," + GameObjectToString(g));
    }

    public void TranslateStop(GameObject g)
    {
        lastActionTime = System.DateTime.Now;
        SaveLog("TRANSLATE-STOP," + GameObjectToString(g));
    }

    public void RotateStart(GameObject g)
    {
        if (!hasStarted)
        {
            firstActionTime = System.DateTime.Now;
            hasStarted = true;
        }
        SaveLog("ROTATE-START," + GameObjectToString(g));
    }

    public void RotateStop(GameObject g)
    {
        lastActionTime = System.DateTime.Now;
        SaveLog("ROTATE-STOP," + GameObjectToString(g));
    }

    public void ScaleStart(GameObject g)
    {
        if (!hasStarted)
        {
            firstActionTime = System.DateTime.Now;
            hasStarted = true;
        }
        SaveLog("SCALE-START," + GameObjectToString(g));
    }

    public void ScaleStop(GameObject g)
    {
        lastActionTime = System.DateTime.Now;
        SaveLog("SCALE-STOP," + GameObjectToString(g));
    }

    public void ManipulationStarted(GameObject g)
    {
        if (!hasStarted)
        {
            firstActionTime = System.DateTime.Now;
            hasStarted = true;
        }
        SaveLog("MANIPULATION-START," + GameObjectToString(g));
    }

    public void ManipulationEnded(GameObject g)
    {
        lastActionTime = System.DateTime.Now;
        SaveLog("MANIPULATION-END," + GameObjectToString(g));
    }

    /*
    Memory Orb Log
    */

    private string MOPointersToString()
    {
        string s = memoryOrbManager.mrtkPointer ? "ON," : "OFF,";
        return s + memoryOrbManager.currentPointer + ",";
    }

    public void MOManipulationStarted(GameObject g)
    {
        if (!hasStarted)
        {
            firstActionTime = System.DateTime.Now;
            hasStarted = true;
        }
        SaveLog("MO-MANIPULATION-START," + MOPointersToString() + GameObjectToString(g));
    }

    public void MOManipulationEnded(GameObject g)
    {
        lastActionTime = System.DateTime.Now;
        SaveLog("MO-MANIPULATION-END," + MOPointersToString() + GameObjectToString(g));
    }

    public void MOManipulationRotating(GameObject g)
    {
        if (!hasStarted)
        {
            firstActionTime = System.DateTime.Now;
            hasStarted = true;
        }
        lastActionTime = System.DateTime.Now;
        SaveLog("MO-MANIPULATION-ROTATING," + MOPointersToString() + GameObjectToString(g));
    }

    public void MOManipulationScaling(GameObject g)
    {
        if (!hasStarted)
        {
            firstActionTime = System.DateTime.Now;
            hasStarted = true;
        }
        lastActionTime = System.DateTime.Now;
        SaveLog("MO-MANIPULATION-SCALING," + MOPointersToString() + GameObjectToString(g));
    }

    public void MORotating(GameObject g)
    {
        if (!hasStarted)
        {
            firstActionTime = System.DateTime.Now;
            hasStarted = true;
        }
        lastActionTime = System.DateTime.Now;
        SaveLog("MO-ROTATING," + MOPointersToString() + GameObjectToString(g));
    }

    public void MOScaling(GameObject g)
    {
        if (!hasStarted)
        {
            firstActionTime = System.DateTime.Now;
            hasStarted = true;
        }
        lastActionTime = System.DateTime.Now;
        SaveLog("MO-SCALING," + MOPointersToString() + GameObjectToString(g));
    }    
}