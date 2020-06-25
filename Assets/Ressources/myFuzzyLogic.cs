
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class myFuzzyLogic : MonoBehaviour
{

    //Declare Fuzzy Membership Curves
    public AnimationCurve FuzzyDir_TooIntoRight;
    public AnimationCurve FuzzyDir_Right;
    public AnimationCurve FuzzyDir_BarelyRight;

    public AnimationCurve FuzzyDir_TooIntoLeft;
    public AnimationCurve FuzzyDir_Left;
    public AnimationCurve FuzzyDir_BarelyLeft;

    public AnimationCurve FuzzyOutput_Right;
    public AnimationCurve FuzzyOutput_Middle;
    public AnimationCurve FuzzyOutputLeft;

    //Tables to store the membership of a given value to each fuzzy set
    private Dictionary<string, float> Membership_Right;
    private Dictionary<string, float> Membership_Left;
    private Dictionary<string, float> Membership_Output;

    //Table with the Fuzzy Rules to be Evaluated
    public Dictionary<string, string> RuleBase;

    //Defuzzified output
    private float Output;

    //Crips input values
    public float InputValueRight;
    public float InputValueLeft;

    //Fuctions to fuzzify each linguistic variable
    public void FuzzifyRight(float inputValue)
    {
        Membership_Right["Good"] = FuzzyDir_TooIntoRight.Evaluate(inputValue);
        Membership_Right["Neutral"] = FuzzyDir_Right.Evaluate(inputValue);
        Membership_Right["Evil"] = FuzzyDir_BarelyRight.Evaluate(inputValue);
    }

    public void FuzzifyLeft(float inputValue)
    {
        Membership_Left["Celebrity"] = FuzzyDir_TooIntoLeft.Evaluate(inputValue);
        Membership_Left["Famous"] = FuzzyDir_Left.Evaluate(inputValue);
        Membership_Left["Unknown"] = FuzzyDir_BarelyLeft.Evaluate(inputValue);
    }

    //Auxiliary function to set the membership curves inflexion points
    public void SetFuzzify(Vector2[] keyframes, AnimationCurve Curve)
    {
        int frame = 0;
        foreach (var key in keyframes)
        {
            Curve.AddKey(key.x, key.y);
        }

        foreach (var key in keyframes)
        {
            AnimationUtility.SetKeyLeftTangentMode(Curve, frame, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyRightTangentMode(Curve, frame, AnimationUtility.TangentMode.Linear);
            frame += 1;
        }
    }

    //Clean any trash membership values that might be left from a previous operation
    public void ZeroMemberships()
    {

        Membership_Output["Hefty"] = 0f;
        Membership_Output["Moderate"] = 0f;
        Membership_Output["Pocketchange"] = 0f;

        Membership_Right["Good"] = 0f;
        Membership_Right["Neutral"] = 0f;
        Membership_Right["Evil"] = 0f;

        Membership_Left["Celebrity"] = 0f;
        Membership_Left["Famous"] = 0f;
        Membership_Left["Unknown"] = 0f;
    }

    //Iterate over all possible memberships and process the rules present
    public void EvaluateRuleBase()
    {
        foreach (var keyA in Membership_Right.Keys)
        {
            foreach (var keyF in Membership_Left.Keys)
            {
                if (Membership_Left[keyF] > -600 && Membership_Right[keyA] > -600)
                {
                    if(Membership_Left[keyF] < 10 && Membership_Right[keyA] < 10)
                    {
                        string PayGroup = RuleBase[keyF + keyA];
                        Membership_Output[PayGroup] = Mathf.Max(Mathf.Min(Membership_Right[keyA], Membership_Left[keyF]), Membership_Output[PayGroup]);

                    }
                }
            }
        }

    }


    //Membership values are defined in isoceles triangles. This function calcuates the are of half of this triangle (a rectangle triangle)
    private float CalculateHalfArea(float x0, float y0, float x1, float y1, float u)
    {
        // Function is of shape y = mx + b
        if (y0 == y1)
            return 0;

        float m = (y0 - y1) / (x0 - x1);
        float b = y1 - m * x1;
        float xu = (u - b) / m;
        float area = 0;
        if (m < 0)
            area = u * (x1 - x0 + xu - x0) / 2;
        else
            area = u * (x1 - x0 + x1 - xu) / 2;

        return area;

    }

    //Divide the isoceles membership into two rectangle triangles and sum up their areas
    private float CalculateTrapezoidArea(AnimationCurve function, float U)
    {
        //Split in two segments, first half and second half
        //Split in two segments, first half and second half

        float areaA = CalculateHalfArea(function.keys[0].time, function.keys[0].value, function.keys[1].time, function.keys[1].value, U);
        float areaB = CalculateHalfArea(function.keys[1].time, function.keys[1].value, function.keys[2].time, function.keys[2].value, U);

        return areaA + areaB;
    }


    //Return the maximum point of the Curve
    private float CalculateCenter(AnimationCurve function, float U)
    {
        return (function.keys[1].time);
    }

    //Defuzzify the fuzzy output into a single value using the Sum of Centers method
    public void DefuzzifyPay()
    {
        EvaluateRuleBase();

        //Log fuzzy output values
        //Debug.Log("Output: " + Membership_Output["Pocketchange"].ToString() + " " + Membership_Output["Moderate"].ToString() + " " + Membership_Output["Hefty"].ToString());

        List<float> Areas = new List<float>();
        List<float> Centers = new List<float>();

        // For each possible membership, calculate the area and its center
        foreach (var keyP in Membership_Output.Keys)
        {
            float U_a = Membership_Output[keyP];

            float area = 0;
            float center = 0;
            if (keyP == "Hefty")
            {
                area = CalculateTrapezoidArea(FuzzyOutput_Right, U_a);
                center = CalculateCenter(FuzzyOutput_Right, U_a);
            }
            else if (keyP == "Moderate")
            {
                area = CalculateTrapezoidArea(FuzzyOutput_Middle, U_a);
                center = CalculateCenter(FuzzyOutput_Middle, U_a);
            }
            else
            {
                area = CalculateTrapezoidArea(FuzzyOutputLeft, U_a);
                center = CalculateCenter(FuzzyOutputLeft, U_a);
            }
            Areas.Add(area);
            Centers.Add(center);
        }

        float numerator = 0;
        float den = 0;

        //Perform weighted average given area size of each membership
        for (int i = 0; i < Areas.Count; i++)
        {
            numerator += Areas[i] * Centers[i];
            den += Areas[i];
        }

        Output = numerator / den;
    }

    public float DistanceHit = 7.5f; //alterado pelo sizeScale do carro
    public float limitMaximum = 1600f;
    // Start is called before the first frame update
    void Awake()
    {

        // Set values for each membership function
        SetFuzzify(new[] { new Vector2(0f, 1f), new Vector2((DistanceHit*0.8f), 0.8f), new Vector2(DistanceHit, 0f) }, FuzzyDir_TooIntoRight);
        SetFuzzify(new[] { new Vector2(0f, 0f), new Vector2(DistanceHit, 1f), new Vector2((DistanceHit*2), 0) }, FuzzyDir_Right);
        SetFuzzify(new[] { new Vector2(0f,0f), new Vector2(DistanceHit, 1f) }, FuzzyDir_BarelyRight);

        SetFuzzify(new[] { new Vector2(0f, 1f), new Vector2((DistanceHit * 0.8f), 0.8f), new Vector2(DistanceHit, 0f) }, FuzzyDir_TooIntoLeft);
        SetFuzzify(new[] { new Vector2(0f, 0f), new Vector2(DistanceHit, 1f), new Vector2((DistanceHit*2), 0) }, FuzzyDir_Left);
        SetFuzzify(new[] { new Vector2(0f, 0f), new Vector2(DistanceHit, 1f) }, FuzzyDir_BarelyLeft);

        SetFuzzify(new[] { new Vector2(-limitMaximum, 0f), new Vector2(-(limitMaximum*0.625f), 1f), new Vector2(0f, 0f) }, FuzzyOutputLeft);
        SetFuzzify(new[] { new Vector2(-(limitMaximum * 0.375f), 0f), new Vector2(0, 1f), new Vector2((limitMaximum * 0.375f), 0) }, FuzzyOutput_Middle);
        SetFuzzify(new[] { new Vector2(0f, 0f), new Vector2((limitMaximum * 0.625f), 1f), new Vector2(limitMaximum, 0f) }, FuzzyOutput_Right);


        //Initialize RuleBase dictionary and fill in the rules
        RuleBase = new Dictionary<string, string>();

        RuleBase.Add("CelebrityGood", "Moderate");//Celebrity = toomuchontheright
        RuleBase.Add("CelebrityNeutral", "Hefty");
        RuleBase.Add("CelebrityEvil", "Hefty");

        RuleBase.Add("FamousGood", "Pocketchange");
        RuleBase.Add("FamousNeutral", "Moderate");
        RuleBase.Add("FamousEvil", "Hefty");

        RuleBase.Add("UnknownGood", "Pocketchange");
        RuleBase.Add("UnknownNeutral", "Pocketchange");
        RuleBase.Add("UnknownEvil", "Moderate");

        //Intialize membership values
        Membership_Right = new Dictionary<string, float>();
        Membership_Left = new Dictionary<string, float>();
        Membership_Output = new Dictionary<string, float>();

        Membership_Output.Add("Hefty", 0f);
        Membership_Output.Add("Moderate", 0f);
        Membership_Output.Add("Pocketchange", 0f);

        Membership_Right.Add("Good", 0f);
        Membership_Right.Add("Neutral", 0f);
        Membership_Right.Add("Evil", 0f);

        Membership_Left.Add("Celebrity", 0f);
        Membership_Left.Add("Famous", 0f);
        Membership_Left.Add("Unknown", 0f);

        //Intilialize input values
        InputValueRight = 0;
        InputValueLeft = 0;

    }
    public float SetLEFTinput(float inputAlignmentLEFT)
    {
        InputValueLeft = inputAlignmentLEFT;
        return InitFuzzify();
    }
    public float SetRIGHTinput(float inputAligmentRIGHT)
    {
        InputValueRight = inputAligmentRIGHT;
        return InitFuzzify();
    }
    float InitFuzzify( )
    {
        //Clean Inputs
        ZeroMemberships();
        
        //Fuzzify inputs
        FuzzifyRight(InputValueRight);
        FuzzifyLeft(InputValueLeft);

        //Log fuzzy input values
        //Debug.Log("Right: " + Membership_Right["Good"].ToString() + " " + Membership_Right["Neutral"].ToString() + " " + Membership_Right["Evil"].ToString());
        //Debug.Log("Left: " + Membership_Left["Celebrity"] + " " + Membership_Left["Famous"] + " " + Membership_Left["Unknown"]);

        //Perform Inference over rules
        EvaluateRuleBase();

        //Defuzzify output awnser
        DefuzzifyPay();

        //Log output
        //Debug.Log(Output);
        //Debug.Log("------");
        return Output;
    }

    public float CrossRuleOf3_Math(float fullv, float piecev)//multiplies crossed to find a vlue of some percentual (rule of 3 in math)
    {
        float fullvalue;  float fullpercent = 100f;
        float piecevalue; float xvalue;

        fullvalue = fullv;
        piecevalue = piecev;

        xvalue = piecevalue* fullpercent;
        xvalue = xvalue / fullvalue;

        return xvalue;
    }
}

