using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FuzzyCar_Agent : MonoBehaviour
{
    public myFuzzyLogic HorizFuzzyController;
    public myFuzzyLogic VertlFuzzyController;

    public Transform LeftPointer, CenterPointer, RightPointer, BackwardPointer;
    public Transform LP_Child, CP_Child, RP_Child, BK_Child;


    public float Scale_Laterals = 10f;//17.5f
    public float Scale_Front = 16f;//17.5f
    public float carMaxSpeed = 140f;
    public float carMinSpeed = 80f;
    public float carSpeed = 50f;
    float constScale = 17.5f;// scale that multiplied with raycast gives the exact size of the draw line
    float turnSpeed = 600f;

    float scaleRealRaycastLaterals = 10; float scaleDrawLineRC_Laterals = 175;//17.5f //size = tamanho do raycast, size2 = tamanho do draw line referente ao raycast
    float scaleRealRaycastFront = 16; float scaleDrawLineRC_Front = 245;


    RaycastHit hit_RU; RaycastHit hit_LU;
    RaycastHit hit_BK; RaycastHit hit_CT;


    // Start is called before the first frame update
    void Start()
    {
        scaleRealRaycastLaterals = Scale_Laterals;
        scaleDrawLineRC_Laterals = scaleDrawLineRC_Laterals * constScale;

        HorizFuzzyController.DistanceHit = Scale_Front;
        HorizFuzzyController.limitMaximum = 1600f;

        VertlFuzzyController.DistanceHit = Scale_Laterals;
        VertlFuzzyController.limitMaximum = 70;

    }
    // Update is called once per frame

    void FixedUpdate()//-- middle -> (-0.7, 0.0, 0.0, 0.7), -- left-> (-0.4, 0.6, 0.6, 0.4), -- right-> (-0.2, -0.7, -0.7, 0.2). Backwards -> (-0.2, 0.7, 0.7, 0.2)
    {
        scaleRealRaycastLaterals = Scale_Laterals;                      scaleRealRaycastFront = Scale_Front;
        scaleDrawLineRC_Laterals = Scale_Laterals * constScale;         scaleDrawLineRC_Front = Scale_Front * constScale;
        HorizFuzzyController.DistanceHit = scaleRealRaycastLaterals;
        
        
        transform.Translate(-Vector3.up * Time.fixedDeltaTime * carSpeed);


        if ( hit_RU.distance > 0)
        {
            HorizFuzzyController.DistanceHit = scaleRealRaycastLaterals;
            turnSpeed = HorizFuzzyController.SetRIGHTinput(hit_RU.distance); 
            transform.Rotate(new Vector3(0,0,turnSpeed * Time.deltaTime )) ; // negativo rot esque
        }
        if ( hit_LU.distance > 0)
        {
            HorizFuzzyController.DistanceHit = scaleRealRaycastLaterals;
            turnSpeed = HorizFuzzyController.SetLEFTinput(hit_LU.distance);
            transform.Rotate(new Vector3(0, 0, turnSpeed * Time.deltaTime)); // negativo rot direita
        }
        if (carSpeed < carMinSpeed * 0.5f) carSpeed = carMinSpeed * 0.5f;
        if (hit_CT.distance > 0)
        {
            VertlFuzzyController.DistanceHit = scaleRealRaycastFront;
            float temp = VertlFuzzyController.CrossRuleOf3_Math(carMaxSpeed, VertlFuzzyController.SetRIGHTinput(hit_CT.distance));
            temp = temp / 1000;
            float prediction = carSpeed * temp;
            if(prediction >= carMinSpeed) carSpeed *=  temp * Time.fixedDeltaTime;

        }
        else if (hit_CT.distance == 0)
        {
            VertlFuzzyController.DistanceHit = scaleRealRaycastFront;
            float temp = VertlFuzzyController.CrossRuleOf3_Math(carMaxSpeed, VertlFuzzyController.SetLEFTinput(hit_BK.distance));
            temp = temp / 1000; if (temp < 0) { temp *= -1; }
            float prediction = carSpeed * temp;
            if (prediction + carSpeed <= carMaxSpeed) carSpeed += prediction * Time.fixedDeltaTime;
        }
        if (hit_BK.distance > 0 || hit_CT.distance == 0)
        {
           /* HorizFuzzyController.DistanceHit = scaleRealRaycastFront;
            float temp = HorizFuzzyController.CrossRuleOf3_Math(carMaxSpeed, HorizFuzzyController.SetLEFTinput(hit_BK.distance));
            temp = temp / 1000;
            float prediction = carSpeed * temp;
            if (prediction <= carMaxSpeed) carSpeed = prediction += carSpeed ;
            //VertlFuzzyController.distance*/
        }


        #region DiagonalLeftUp_Raycast
        if (Physics.Raycast(LeftPointer.position, transform.TransformDirection(LP_Child.localPosition - LeftPointer.localPosition), out hit_LU,scaleRealRaycastLaterals))
        {
            Debug.DrawRay(LeftPointer.position, transform.TransformDirection(LP_Child.localPosition - LeftPointer.localPosition)* scaleDrawLineRC_Laterals , Color.red);
        }
        else
        {
            Debug.DrawRay(LeftPointer.position, transform.TransformDirection(LP_Child.localPosition - LeftPointer.localPosition)* scaleDrawLineRC_Laterals, Color.green);
        }
        #endregion 

        #region DiagonalRightUp_Raycast
        if (Physics.Raycast(RightPointer.position, transform.TransformDirection(RP_Child.localPosition - RightPointer.localPosition), out hit_RU, scaleRealRaycastLaterals))
        {
            Debug.DrawRay(RightPointer.position, transform.TransformDirection(RP_Child.localPosition - RightPointer.localPosition) * scaleDrawLineRC_Laterals, Color.red);
        }
        else
        {
            Debug.DrawRay(RightPointer.position, transform.TransformDirection(RP_Child.localPosition - RightPointer.localPosition) * scaleDrawLineRC_Laterals, Color.green);
        }
        #endregion



        #region ForwardUp_Raycast
        if (Physics.Raycast(CenterPointer.position, transform.TransformDirection(CP_Child.localPosition - CenterPointer.localPosition), out hit_CT, scaleRealRaycastFront))
        {
            Debug.DrawRay(CenterPointer.position, transform.TransformDirection(CP_Child.localPosition - CenterPointer.localPosition) * scaleDrawLineRC_Front, Color.red);
        }
        else
        {
            Debug.DrawRay(CenterPointer.position, transform.TransformDirection(CP_Child.localPosition - CenterPointer.localPosition) * scaleDrawLineRC_Front, Color.green);
        }
        #endregion

        #region Backward_Raycast
        if (Physics.Raycast(BackwardPointer.position, transform.TransformDirection(BK_Child.localPosition - BackwardPointer.localPosition), out hit_BK, scaleRealRaycastFront))
        {
            Debug.DrawRay(BackwardPointer.position, transform.TransformDirection(BK_Child.localPosition - BackwardPointer.localPosition) * scaleDrawLineRC_Front, Color.red);
        }
        else
        {
            Debug.DrawRay(BackwardPointer.position, transform.TransformDirection(BK_Child.localPosition - BackwardPointer.localPosition) * scaleDrawLineRC_Front, Color.green);
        }
        #endregion
    }

}

