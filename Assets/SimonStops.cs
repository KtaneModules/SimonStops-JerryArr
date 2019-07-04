using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using KModkit;


public class SimonStops : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBombModule Module;

    private static int _moduleIdCounter = 1;
    private int _moduleId;

    public KMSelectable[] button;
    public Light[] lights;

    //public KMRuleSeedable RuleSeedable;


    string[] colors = new string[6]
    {
        "Red", "Orange", "Yellow",
        "Green", "Blue", "Violet",
    };

    string[] inputRainbow = new string[6]
{
        "r", "o", "y",
        "g", "b", "v",
};

    string[] alphabet = new string[26]
    { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
      "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"};

    /*
    int[] values = new int[26] 
    { 1, 5, 5, 5, 1, 5, 5, 6, 1, 3, 6, 4, 6,
      4, 1, 6, 3, 4, 4, 4, 1, 6, 2, 3, 2, 3 };
  
         */

    string[] outputSequence = new string[5];

    bool pressedAllowed = false;

    // TWITCH PLAYS SUPPORT
    //int tpStages; This one is not needed for this module
    // TWITCH PLAYS SUPPORT

    int stageNum;

    int currentStagePresses;

    int expectedControl;

    int currentLightCycle;

    int currentState;
    // 0 = idle, 1 = normal input, 2 = control input needed, 3 = solved

    double timeUntilNewSection;
    double controlTimeLimit;
    double controlTimeLeft;
    double resetInputTimeLeft;
    double lightPressTime;

    bool hasFocus;

    string expectedInput;
    string expectedInputNormal;
    string expectedInputNumbers;
    string expectedColor;
    string rawOutput;
    string fullCycle;

    string letterString;

    bool isSolved = false;
    bool tpActive = false;

    void Start()
    {
        _moduleId = _moduleIdCounter++;
        //colorblindModeEnabled = colorblindMode.ColorblindModeActive;
        Init();
        pressedAllowed = true;
    }

    void Init()
    {
        delegationZone();
        float scalar = transform.lossyScale.x;
        for (int sbn = 0; sbn < 5; sbn++)
        {

            lights[sbn].range *= scalar;
        }
        controlTimeLimit = 3f;
        stageNum = 1;
        resetInputTimeLeft = 10f;
        lightPressTime = .35f;
        currentStagePresses = 0;
        for (int lightOff = 0; lightOff < 6; lightOff++)
        {
            lights[lightOff].enabled = false;
        }
        for (int pNum = 0; pNum < 5; pNum++)
        {
            rawOutput = rawOutput + UnityEngine.Random.Range(0, 6).ToString();

        }
        doCycle();
        currentLightCycle = fullCycle.Length - 2;
        var testString = colors[Int16.Parse(rawOutput.Substring(0, 1))] + ", " + colors[Int16.Parse(rawOutput.Substring(1, 1))] + ", " + colors[Int16.Parse(rawOutput.Substring(2, 1))];
        Debug.LogFormat("[Simon Stops #{0}] Output for stage {1}: {2}", _moduleId, stageNum, testString);

        figureInput();

        Debug.LogFormat("[Simon Stops #{0}] Expected Normal Input for stage {1}: {2} (upper case means control input expected after that input)", _moduleId, stageNum, expectedInputNormal);
        Debug.LogFormat("[Simon Stops #{0}] Expected Total Input for stage {1}: {2} (upper case means control input expected after that input)", _moduleId, stageNum, expectedInput);
        //Debug.LogFormat("[Simon Stops #{0}] Expected Control Input for stage {1}: {2}", _moduleId, stageNum, testString);
        //currentAccessory = UnityEngine.Random.Range(0, 6);
        //accessoryPart.material.mainTexture = accessoryImages[currentAccessory];
        //pickedName = UnityEngine.Random.Range(0, gryphNames.Count());
        //IDBox.GetComponentInChildren<TextMesh>().text = gryphNames[pickedName] + " (" + age + ")";
        pressedAllowed = true;
        for (int stg = 0; stg < 5; stg++)
        {
            outputSequence[stg] = colors[Int16.Parse(rawOutput.Substring(stg, 1))];
            //Debug.Log(outputSequence[stg]);
        }
        
    }


    void OnHold()
    {

    }

    void OnRelease()
    {
    }

    void doCycle()
    {
        for (int cS = 0; cS < stageNum + 2; cS++)
        {
            fullCycle = fullCycle + rawOutput.Substring(cS, 1) + "9";
        }
        fullCycle = fullCycle + "99";
        //Debug.LogFormat("[Simon Stops #{0}] Full cycle is: {1} ", _moduleId, fullCycle);
    }

    void figureInput()
    {
        expectedInput = "";
        expectedInputNormal = "";
        expectedInputNumbers = "";
        expectedControl = UnityEngine.Random.Range(0, stageNum + 1);
        expectedColor = "";
        var xDigit = "";
        for (int cs = 0; cs < stageNum + 2; cs++)
        {
            switch (stageNum)
            {
                case 1:
                    if (rawOutput.Substring(cs, 1) == "0")
                    {
                        expectedColor = "b";
                    }
                    else if (rawOutput.Substring(cs, 1) == "1")
                    {
                        expectedColor = colors[(1 + Bomb.GetBatteryCount()) % 6].Substring(0, 1).ToLowerInvariant();
                    }
                    else if (rawOutput.Substring(cs, 1) == "2")
                    {
                        expectedColor = "y";
                    }
                    else if (rawOutput.Substring(cs, 1) == "3")
                    {
                        expectedColor = "r";
                    }
                    else if (rawOutput.Substring(cs, 1) == "4")
                    {
                        expectedColor = "v";
                    }
                    else if (rawOutput.Substring(cs, 1) == "5")
                    {
                        expectedColor = colors[(5 + Bomb.GetBatteryCount()) % 6].Substring(0, 1).ToLowerInvariant();
                    }
                    break;
                case 2:
                    if (rawOutput.Substring(cs, 1) == "0")
                    {
                        expectedColor = colors[Bomb.GetBatteryCount() % 6].Substring(0, 1).ToLowerInvariant();
                    }
                    else if (rawOutput.Substring(cs, 1) == "1")
                    {
                        expectedColor = "y";
                    }
                    else if (rawOutput.Substring(cs, 1) == "2")
                    {
                        expectedColor = colors[(2 + Bomb.GetBatteryCount()) % 6].Substring(0, 1).ToLowerInvariant();
                    }
                    else if (rawOutput.Substring(cs, 1) == "3")
                    {
                        expectedColor = "v";
                    }
                    else if (rawOutput.Substring(cs, 1) == "4")
                    {
                        expectedColor = "o";
                    }
                    else if (rawOutput.Substring(cs, 1) == "5")
                    {
                        expectedColor = "b";
                    }
                    break;
                case 3:
                    if (rawOutput.Substring(cs, 1) == "0")
                    {
                        expectedColor = "y";
                    }
                    else if (rawOutput.Substring(cs, 1) == "1")
                    {
                        expectedColor = "o";
                    }
                    else if (rawOutput.Substring(cs, 1) == "2")
                    {
                        expectedColor = "g";
                    }
                    else if (rawOutput.Substring(cs, 1) == "3")
                    {
                        expectedColor = colors[(3 + Bomb.GetBatteryCount()) % 6].Substring(0, 1).ToLowerInvariant();
                    }
                    else if (rawOutput.Substring(cs, 1) == "4")
                    {
                        expectedColor = colors[(4 + Bomb.GetBatteryCount()) % 6].Substring(0, 1).ToLowerInvariant();
                    }
                    else if (rawOutput.Substring(cs, 1) == "5")
                    {
                        expectedColor = "r";
                    }
                    break;
                default:
                    break;
                    
            }
            if (expectedColor == "r")
            {
                xDigit = "0";
            }
            else if (expectedColor == "o")
            {
                xDigit = "1";
            }
            else if (expectedColor == "y")
            {
                xDigit = "2";
            }
            else if (expectedColor == "g")
            {
                xDigit = "3";
            }
            else if (expectedColor == "b")
            {
                xDigit = "4";
            }
            else if (expectedColor == "v")
            {
                xDigit = "5";
            }
            expectedInputNumbers = expectedInputNumbers + xDigit;

            if (cs == expectedControl)
            {
                expectedColor = expectedColor.ToUpperInvariant();
                expectedInputNormal = expectedInputNormal + expectedColor.ToUpperInvariant();
                expectedColor = expectedColor + figureControlInput();
            }
            else
            {

                expectedInputNormal = expectedInputNormal + expectedColor;
            }

            expectedInput = expectedInput + expectedColor;
        }

    }

    string figureControlInput()
    {
        var ciNumber = 0;
        var explainer = "";
        for (int snNum = 5; snNum >= 0; snNum--)
        {
            if (Bomb.GetSerialNumber().Substring(snNum, 1).TryParseInt() > -1)
            {
                ciNumber = Int16.Parse(Bomb.GetSerialNumber().Substring(snNum, 1));
                explainer = "The last SN digit is " + ciNumber + ". ";
                snNum = -1;
            }
        }
        var ciColor = "";
        var lastColor = expectedColor.Substring(expectedColor.Length - 1, 1).ToLowerInvariant();
        var lastNumber = expectedInputNumbers.Substring(expectedInputNumbers.Length - 1, 1);
        var rainbow = "roygbv";
        switch (stageNum)
        {
            case 1:
                var snCons = 0;
                for (int snNum = 5; snNum >= 0; snNum--)
                {
                    
                    if (Bomb.GetSerialNumber().Substring(snNum, 1) == "A" || Bomb.GetSerialNumber().Substring(snNum, 1) == "E" ||
                        Bomb.GetSerialNumber().Substring(snNum, 1) == "I" || Bomb.GetSerialNumber().Substring(snNum, 1) == "O" || Bomb.GetSerialNumber().Substring(snNum, 1) == "U" || 
                        Bomb.GetSerialNumber().Substring(snNum, 1).TryParseInt() >= 0)
                    {
                        
                    }
                    else
                    {
                        snCons++;
                    }

                }
                explainer = explainer + "Batteries (" + Bomb.GetBatteryCount() + ") times SN consonants (" + snCons + ") is " + (snCons * Bomb.GetBatteryCount()) + ". Add them together to get ";
                ciNumber = ciNumber + (snCons * Bomb.GetBatteryCount());
                explainer = explainer + ciNumber + ". ";
                ciNumber = ciNumber % 10;
                explainer = explainer + "That modulo 10 is " + ciNumber + ". The button that will require Control Input is press number " + expectedInputNumbers.Length + " which will be " + lastColor + ".";
                Debug.LogFormat("[Simon Stops #{0}] Stage 1. {1} ", _moduleId, explainer);
                switch (ciNumber)
                {
                    case 0: //SC, Same color
                        ciColor = lastColor;
                        Debug.LogFormat("[Simon Stops #{0}] You need the same color as this, or {1}.", _moduleId, ciColor);
                        break;
                    case 1: //1N, Next color
                        ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 1) % 6, 1);
                        Debug.LogFormat("[Simon Stops #{0}] You need the color 1 clockwise from this, or {1}.", _moduleId, ciColor);
                        break;
                    case 2: //PS, Previous secondary
                        if (lastColor == "r" || lastColor == "b" || lastColor == "y")
                        {
                            ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 5) % 6, 1);
                        }
                        else
                        {
                            ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 4) % 6, 1);
                        }
                        Debug.LogFormat("[Simon Stops #{0}] You need the secondary color counterclockwise from this, or {1}.", _moduleId, ciColor);
                        break;
                    case 3: //1P, Previous color
                        ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 5) % 6, 1);
                        Debug.LogFormat("[Simon Stops #{0}] You need the color counterclockwise from this, or {1}.", _moduleId, ciColor);
                        break;
                    case 4: //2N, Color + 2
                        ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 2) % 6, 1);
                        Debug.LogFormat("[Simon Stops #{0}] You need the color two clockwise from this, or {1}.", _moduleId, ciColor);
                        break;
                    case 5: //OC, Opposite color
                        ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 3) % 6, 1);
                        Debug.LogFormat("[Simon Stops #{0}] You need the opposite color, or {1}.", _moduleId, ciColor);
                        break;
                    case 6: //NS, Next secondary
                        if (lastColor == "r" || lastColor == "b" || lastColor == "y")
                        {
                            ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 1) % 6, 1);
                        }
                        else
                        {
                            ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 2) % 6, 1);
                        }
                        Debug.LogFormat("[Simon Stops #{0}] You need the secondary color clockwise from this, or {1}.", _moduleId, ciColor);
                        break;
                    case 7: //2P, Color - 2
                        ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 4) % 6, 1);
                        Debug.LogFormat("[Simon Stops #{0}] You need the color two counterclockwise from this one, or {1}.", _moduleId, ciColor);
                        break;
                    case 8: //PP, Previous primary
                        if (lastColor == "r" || lastColor == "b" || lastColor == "y")
                        {
                            ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 4) % 6, 1);
                        }
                        else
                        {
                            ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 5) % 6, 1);
                        }
                        Debug.LogFormat("[Simon Stops #{0}] You need the primary color counterclockwise from this, or {1}.", _moduleId, ciColor);
                        break;
                    default: //NP, Next primary
                        if (lastColor == "r" || lastColor == "b" || lastColor == "y")
                        {
                            ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 2) % 6, 1);
                        }
                        else
                        {
                            ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 1) % 6, 1);
                        }
                        Debug.LogFormat("[Simon Stops #{0}] You need the primary color clockwise from this, or {1}.", _moduleId, ciColor);
                        break;
                }
                break;
            case 2:
                ciNumber = ciNumber + (Bomb.GetPortCount() * 2) + Bomb.GetBatteryHolderCount();
                explainer = explainer + "Ports times 2 (" + (Bomb.GetPortCount() * 2) + ") plus battery holders (" + Bomb.GetBatteryHolderCount() + ") is " + ((Bomb.GetPortCount() * 2) + Bomb.GetBatteryHolderCount()) + ". Add them together to get " + ciNumber;
                ciNumber = ciNumber % 10;
                explainer = explainer + ". That modulo 10 is " + ciNumber + ". The button that will require Control Input is press number " + expectedInputNumbers.Length + " which will be " + lastColor + ".";
                Debug.LogFormat("[Simon Stops #{0}] Stage 2. {1} ", _moduleId, explainer);
                switch (ciNumber)
                {
                    case 0: //1P, Previous color
                        ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 5) % 6, 1);
                        Debug.LogFormat("[Simon Stops #{0}] You need the color counterclockwise from this, or {1}.", _moduleId, ciColor);
                        break;
                    case 1: //NP, Next primary
                        if (lastColor == "r" || lastColor == "b" || lastColor == "y")
                        {
                            ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 2) % 6, 1);
                        }
                        else
                        {
                            ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 1) % 6, 1);
                        }
                        Debug.LogFormat("[Simon Stops #{0}] You need the primary color clockwise from this, or {1}.", _moduleId, ciColor);
                        break;
                    case 2: //PP, Previous primary
                        if (lastColor == "r" || lastColor == "b" || lastColor == "y")
                        {
                            ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 4) % 6, 1);
                        }
                        else
                        {
                            ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 5) % 6, 1);
                        }
                        Debug.LogFormat("[Simon Stops #{0}] You need the primary color counterclockwise from this, or {1}.", _moduleId, ciColor);
                        break;
                    case 3: //SC, Same color
                        ciColor = lastColor;
                        Debug.LogFormat("[Simon Stops #{0}] You need the same color as this, or {1}.", _moduleId, ciColor);
                        break;
                    case 4: //OC, Opposite color
                        ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 3) % 6, 1);
                        Debug.LogFormat("[Simon Stops #{0}] You need the opposite color, or {1}.", _moduleId, ciColor);
                        break;
                    case 5: //PS, Previous secondary
                        if (lastColor == "r" || lastColor == "b" || lastColor == "y")
                        {
                            ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 5) % 6, 1);
                        }
                        else
                        {
                            ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 4) % 6, 1);
                        }
                        Debug.LogFormat("[Simon Stops #{0}] You need the secondary color counterclockwise from this, or {1}.", _moduleId, ciColor);
                        break;
                    case 6: //2P, Color - 2
                        ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 4) % 6, 1);
                        Debug.LogFormat("[Simon Stops #{0}] You need the color two counterclockwise from this one, or {1}.", _moduleId, ciColor);
                        break;
                    case 7: //1N, Next color
                        ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 1) % 6, 1);
                        Debug.LogFormat("[Simon Stops #{0}] You need the color 1 clockwise from this, or {1}.", _moduleId, ciColor);
                        break;
                    case 8: //NS, Next secondary
                        if (lastColor == "r" || lastColor == "b" || lastColor == "y")
                        {
                            ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 1) % 6, 1);
                        }
                        else
                        {
                            ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 2) % 6, 1);
                        }
                        Debug.LogFormat("[Simon Stops #{0}] You need the secondary color clockwise from this, or {1}.", _moduleId, ciColor);
                        break;
                    default: //2N, Color + 2
                        ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 2) % 6, 1);
                        Debug.LogFormat("[Simon Stops #{0}] You need the color two clockwise from this, or {1}.", _moduleId, ciColor);
                        break;
                }
                break;
            case 3:
                ciNumber = ciNumber + 2 + (Bomb.GetOnIndicators().Count() * 3) + Bomb.GetOffIndicators().Count();
                explainer = explainer + "2 (2), plus the indicator count with lit indicators counting as triple (" + ((Bomb.GetOnIndicators().Count() * 3) + Bomb.GetOffIndicators().Count()) + ") is " + (2 + (Bomb.GetOnIndicators().Count() * 3) + Bomb.GetOffIndicators().Count()) + ". Add them together to get " + ciNumber;
                ciNumber = ciNumber % 10;
                explainer = explainer + ". That modulo 10 is " + ciNumber + ". The button that will require Control Input is press number " + expectedInputNumbers.Length + " which will be " + lastColor + ".";
                Debug.LogFormat("[Simon Stops #{0}] Stage 3. {1} ", _moduleId, explainer);
                switch (ciNumber)
                {
                    case 0: //OC, Opposite color
                        ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 3) % 6, 1);
                        Debug.LogFormat("[Simon Stops #{0}] You need the opposite color, or {1}.", _moduleId, ciColor);
                        break;
                    case 1: //1N, Next color
                        ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 1) % 6, 1);
                        Debug.LogFormat("[Simon Stops #{0}] You need the color 1 clockwise from this, or {1}.", _moduleId, ciColor);
                        break;
                    case 2: //1P, Previous color
                        ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 5) % 6, 1);
                        Debug.LogFormat("[Simon Stops #{0}] You need the color counterclockwise from this, or {1}.", _moduleId, ciColor);
                        break;
                    case 3: //NS, Next secondary
                        if (lastColor == "r" || lastColor == "b" || lastColor == "y")
                        {
                            ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 1) % 6, 1);
                        }
                        else
                        {
                            ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 2) % 6, 1);
                        }
                        Debug.LogFormat("[Simon Stops #{0}] You need the secondary color clockwise from this, or {1}.", _moduleId, ciColor);
                        break;
                    case 4: //2P, Color - 2
                        ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 4) % 6, 1);
                        Debug.LogFormat("[Simon Stops #{0}] You need the color two counterclockwise from this one, or {1}.", _moduleId, ciColor);
                        break;
                    case 5: //PP, Previous primary
                        if (lastColor == "r" || lastColor == "b" || lastColor == "y")
                        {
                            ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 4) % 6, 1);
                        }
                        else
                        {
                            ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 5) % 6, 1);
                        }
                        Debug.LogFormat("[Simon Stops #{0}] You need the primary color counterclockwise from this, or {1}.", _moduleId, ciColor);
                        break;
                    case 6: //PS, Previous secondary
                        if (lastColor == "r" || lastColor == "b" || lastColor == "y")
                        {
                            ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 5) % 6, 1);
                        }
                        else
                        {
                            ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 4) % 6, 1);
                        }
                        Debug.LogFormat("[Simon Stops #{0}] You need the secondary color counterclockwise from this, or {1}.", _moduleId, ciColor);
                        break;
                    case 7: //SC, Same color
                        ciColor = lastColor;
                        Debug.LogFormat("[Simon Stops #{0}] You need the same color as this, or {1}.", _moduleId, ciColor);
                        break;
                    case 8: //NP, Next primary
                        if (lastColor == "r" || lastColor == "b" || lastColor == "y")
                        {
                            ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 2) % 6, 1);
                        }
                        else
                        {
                            ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 1) % 6, 1);
                        }
                        Debug.LogFormat("[Simon Stops #{0}] You need the primary color clockwise from this, or {1}.", _moduleId, ciColor);
                        break;
                    default: //2N, Color + 2
                        ciColor = rainbow.Substring((Int16.Parse(lastNumber) + 2) % 6, 1);
                        Debug.LogFormat("[Simon Stops #{0}] You need the color two clockwise from this, or {1}.", _moduleId, ciColor);
                        break;
                }
                break;
            default:
                break;
        }
        if (ciColor == "r")
        {
            expectedInputNumbers = expectedInputNumbers + "0";
        }
        else if (ciColor == "o")
        {
            expectedInputNumbers = expectedInputNumbers + "1";
        }
        else if (ciColor == "y")
        {
            expectedInputNumbers = expectedInputNumbers + "2";
        }
        else if (ciColor == "g")
        {
            expectedInputNumbers = expectedInputNumbers + "3";
        }
        else if (ciColor == "b")
        {
            expectedInputNumbers = expectedInputNumbers + "4";
        }
        else if (ciColor == "v")
        {
            expectedInputNumbers = expectedInputNumbers + "5";
        }
        Debug.LogFormat("[Simon Stops #{0}] Expected Control Input for stage {1}: {2}", _moduleId, stageNum, ciColor);
        return ciColor;
    }


    private void FixedUpdate()
    {
        if (isSolved)
        {
		
        }
        else
        {
            switch (currentState)
            {
                case 0:
                    if (timeUntilNewSection > 0)
                    {
                        timeUntilNewSection -= Time.fixedDeltaTime;
                    }
                    else if (timeUntilNewSection < 0)
                    {
                        timeUntilNewSection = 0;
                    }
                    else
                    {
                        timeUntilNewSection = .35f;
                        currentLightCycle++;
                        currentLightCycle = currentLightCycle % fullCycle.Length;
                        for (int lo = 0; lo < 6; lo++)
                        {
                            lights[lo].enabled = false;
                        }
                        if (fullCycle.Substring(currentLightCycle, 1).TryParseInt() != 9)
                        {
                            lights[Int16.Parse(fullCycle.Substring(currentLightCycle, 1))].enabled = true;
                            if (hasFocus)
                            {
                                GetComponent<KMAudio>().PlaySoundAtTransform("tone" + (Int16.Parse(fullCycle.Substring(currentLightCycle, 1)) + 1), transform);
                            }
                        }

                        //Debug.LogFormat("[Simon Stops #{0}] Now flashing: {1} ", _moduleId, fullCycle.Substring(currentLightCycle, 1));
                    }
                    break;
                case 1:
                    lightPressTime -= Time.fixedDeltaTime;
                    if (lightPressTime <= 0)
                    {
                        for (int lo = 0; lo < 6; lo++)
                        {
                            lights[lo].enabled = false;
                        }
                        lightPressTime = 0;
                    }
                    if (resetInputTimeLeft > 0)
                    {
                        resetInputTimeLeft -= Time.fixedDeltaTime;
                    }
                    else if (resetInputTimeLeft < 0)
                    {
                        resetInputTimeLeft = 0;
                    }
                    else
                    {
                        resetInputTimeLeft = 10f;
                        currentLightCycle = 0;
                        currentState = 0;
                        for (int lo = 0; lo < 6; lo++)
                        {
                            lights[lo].enabled = false;
                        }
                        if (fullCycle.Substring(currentLightCycle, 1).TryParseInt() != 9)
                        {
                            lights[Int16.Parse(fullCycle.Substring(currentLightCycle, 1))].enabled = true;
                        }
                        currentStagePresses = 0;
                        //Debug.LogFormat("[Simon Stops #{0}] Now flashing: {1} ", _moduleId, fullCycle.Substring(currentLightCycle, 1));
                    }
                    break;
                case 2:
                    if (controlTimeLeft > 0)
                    {
                        controlTimeLeft -= Time.fixedDeltaTime;
                    }
                    else if (controlTimeLeft < 0)
                    {
                        controlTimeLeft = 0;
                    }
                    else
                    {
                        controlTimeLeft = controlTimeLimit;
                        currentLightCycle = 0;
                        
                        for (int lo = 0; lo < 6; lo++)
                        {
                            lights[lo].enabled = false;
                        }
                        if (fullCycle.Substring(currentLightCycle, 1).TryParseInt() != 9)
                        {
                            lights[Int16.Parse(fullCycle.Substring(currentLightCycle, 1))].enabled = true;
                        }
                        currentStagePresses = 0;
                        Debug.LogFormat("[Simon Stops #{0}] Control input not given in time. Strike given!", _moduleId);
                        currentStagePresses = 0;
                        Module.HandleStrike();
                        currentState = 0;
                        //Debug.LogFormat("[Simon Stops #{0}] Now flashing: {1} ", _moduleId, fullCycle.Substring(currentLightCycle, 1));
                    }
                    break;
                case 4:

                default:
                    break;
            }


        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} roygbv/r o y g b v to input a Normal Input sequence. Please do not use full color names, and make sure the command consists only of the colors' first letters. Your input will stop once a Control Input is needed; input the Control Input along with the rest of the sequence when prompted. On Twitch Plays, the time limit to input the Control Input is 20 seconds.";
    private readonly bool TwitchShouldCancelCommand = false;
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        tpActive = true;
        var piecesRaw = command.ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        controlTimeLimit = 20;

        var tpStages = 0;
        string theError;
        theError = "";
        if (piecesRaw.Count() == 0)
        {
            theError = "sendtochaterror No arguments! You need to use roygbv/r o y g b v to presss those buttons";
            yield return theError;
        }
        else
        {

            //Debug.Log(piecesRaw[0]);
            letterString = piecesRaw[0];
            tpStages = piecesRaw[0].Length;
            //Debug.Log(tpStages);

                letterString = "";
                for (int pn = 0; pn < piecesRaw.Count(); pn++)
                {

                    letterString = letterString + piecesRaw[pn];
                }
            tpStages = letterString.Length;
        }
        if (currentState == 2)
        {
            if (letterString.Length != stageNum + 3 - currentStagePresses)
            {
                theError = "sendtochaterror Not enough arguments! You require " + (stageNum + 3 - currentStagePresses) + " more input(s): the Control Input and the rest of your Normal Input sequence.";
                yield return theError;
            }
        }
        else
        {
            if (letterString.Length < stageNum + 2)
            {
                theError = "sendtochaterror Not enough arguments! Stage " + stageNum + " requires " + (stageNum + 2) + " inputs (but you'll be stopped when prompted for the Control Input).";
                yield return theError;
            }
        }
        ///////////////////
        ///// check if all stages are ok here
        ///////////////////
        for (int tbn = 0; tbn < letterString.Length; tbn++)
        {
            if (letterString.Substring(tbn, 1) != "r" && letterString.Substring(tbn, 1) != "o" && letterString.Substring(tbn, 1) != "y" &&
                letterString.Substring(tbn, 1) != "g" && letterString.Substring(tbn, 1) != "b" && letterString.Substring(tbn, 1) != "v" )
            {
                theError = "sendtochaterror Invalid argument! " + letterString.Substring(tbn, 1) + " is not a valid initial letter of a color. Please use R, O, Y, G, B, or V.";
                if (tbn == 0)
                {
                    theError = theError + " Remember: Only use initial letters of the colors you're inputting; commands like 'press' aren't used.";
                }
                tbn = letterString.Length;
				tpStages = 0;
                yield return theError;
                break;
            }
        }
        while (tpStages > 0 && theError == "")
        {
            var awaitingControl = false;
            if (currentState == 2)
            {
                awaitingControl = true;
            }
            yield return null;
            yield return new WaitForSeconds(.175f);
            var pickNum = 0;
            while (pickNum < 6 && inputRainbow[pickNum] != letterString.Substring(letterString.Length - tpStages, 1))
            {
                pickNum++;
            }
            if (pickNum == 6)
            {
                theError = "sendtochaterror Invalid arguments! " + letterString.Substring(letterString.Length - tpStages, 1) + " is not a valid color or initial letter of a color even though we just tested this.";
                yield return theError;
            }
            var buttonPicked = pickNum;
            yield return null;
            //Debug.Log("PRessed " + buttonPicked + " which is " + letterString.Substring(letterString.Length - tpStages, 1));
            button[buttonPicked].OnInteract();
            tpStages--;
            if (!awaitingControl && currentState == 2)
            {
                tpStages = 0;
                yield return "sendtochat It's time for Control Input! So far this stage you've pressed " + currentStagePresses + " button(s).";
            }
        }
    }
	
	void TwitchHandleForcedSolve()
	{
		Debug.LogFormat("[Simon Stops #{0}] Twitch Plays demands an auto-solve, skipping all stages.", _moduleId);
        string[] tpFS = new string[7] { "t", "p", "s", "o", "l", "v", "e" };
        outputSequence = tpFS;	//Make sure Souvenir doesn't ask about an auto-solved Simon Stops.			
		stageNum = 4;
		currentStagePresses = 0;
		for (int lo = 0; lo < 6; lo++)
		{
			lights[lo].enabled = false;
		}
		currentState = 3;
		pressedAllowed = false;
		isSolved = true;
		Module.HandlePass();
	}
	

    void pressButton(int b)
    {
        hasFocus = true;
        if (pressedAllowed)
        {
            if (expectedInput.Substring(currentStagePresses, 1).ToUpperInvariant() == colors[b].Substring(0, 1)) //color matches what is expected, now check if it's a control
            {
                currentState = 1;
                lightPressTime = .35f;
                resetInputTimeLeft = 10f;
                if (expectedInput.Substring(currentStagePresses, 1) == expectedInput.Substring(currentStagePresses, 1).ToUpperInvariant())
                {
                    currentState = 2;
                    controlTimeLeft = controlTimeLimit;

                }
                currentStagePresses++;
                if (currentStagePresses == stageNum + 3)
                {
                    Debug.LogFormat("[Simon Stops #{0}] Stage {1} clear.", _moduleId, stageNum);
                    currentStagePresses = 0;
                    currentState = 0;
                    stageNum++;
                    if (stageNum >= 4)
                    {
                        Debug.LogFormat("[Simon Stops #{0}] All stages clear, module disarmed!", _moduleId);
                        for (int lo = 0; lo < 6; lo++)
                        {
                            lights[lo].enabled = false;
                        }
                        currentState = 3;
                        pressedAllowed = false;
                        isSolved = true;
                        Module.HandlePass();
                    }
                    else if (stageNum == 2)
                    {
                        for (int lightOff = 0; lightOff < 6; lightOff++)
                        {
                            lights[lightOff].enabled = false;
                        }
                        expectedInputNumbers = "";
                        fullCycle = "";
                        doCycle();
                        currentLightCycle = fullCycle.Length - 4;
                        var testString = colors[Int16.Parse(rawOutput.Substring(0, 1))] + ", " + colors[Int16.Parse(rawOutput.Substring(1, 1))] + ", " + 
                            colors[Int16.Parse(rawOutput.Substring(2, 1))] + ", " + colors[Int16.Parse(rawOutput.Substring(3, 1))];
                        Debug.LogFormat("[Simon Stops #{0}] Output for stage {1}: {2}", _moduleId, stageNum, testString);

                        figureInput();

                        Debug.LogFormat("[Simon Stops #{0}] Expected Normal Input for stage {1}: {2} (upper case means control input expected after that input)", _moduleId, stageNum, expectedInputNormal);
                        Debug.LogFormat("[Simon Stops #{0}] Expected Total Input for stage {1}: {2} (upper case means control input expected after that input)", _moduleId, stageNum, expectedInput);
                    }
                    else
                    {
                        for (int lightOff = 0; lightOff < 6; lightOff++)
                        {
                            lights[lightOff].enabled = false;
                        }
                        expectedInputNumbers = "";
                        fullCycle = "";
                        doCycle();
                        currentLightCycle = fullCycle.Length - 4;
                        var testString = colors[Int16.Parse(rawOutput.Substring(0, 1))] + ", " + colors[Int16.Parse(rawOutput.Substring(1, 1))] + ", " +
                            colors[Int16.Parse(rawOutput.Substring(2, 1))] + ", " + colors[Int16.Parse(rawOutput.Substring(3, 1))] + ", " + colors[Int16.Parse(rawOutput.Substring(4, 1))];
                        Debug.LogFormat("[Simon Stops #{0}] Output for stage {1}: {2}", _moduleId, stageNum, testString);

                        figureInput();

                        Debug.LogFormat("[Simon Stops #{0}] Expected Normal Input for stage {1}: {2} (upper case means control input expected after that input)", _moduleId, stageNum, expectedInputNormal);
                        Debug.LogFormat("[Simon Stops #{0}] Expected Total Input for stage {1}: {2} (upper case means control input expected after that input)", _moduleId, stageNum, expectedInput);
                    }
                }
            }
            else
            {
                Debug.LogFormat("[Simon Stops #{0}] {1} was pressed when we expected {2}. Strike given!", _moduleId, colors[b], colors[ Int16.Parse(expectedInputNumbers.Substring(currentStagePresses, 1)) ]);
                currentStagePresses = 0;
                currentState = 0;
                Module.HandleStrike();
            }
        }
        if (!isSolved)
        {
            for (int lo = 0; lo < 6; lo++)
            {
                lights[lo].enabled = false;
            }
            lights[b].enabled = true;
        }
        else
        {
            lights[b].enabled = !lights[b].enabled;
        }
        if (currentState != 2)
        {
            GetComponent<KMAudio>().PlaySoundAtTransform("tone" + (b + 1), transform);
        }
    }

    void doSubmit()
    {

    }

    void delegationZone()
    {
        //Module.

        button[0].OnInteract += delegate () { OnHold(); pressButton(0); return false; };
        button[0].OnInteractEnded += delegate () { OnRelease(); };

        button[1].OnInteract += delegate () { OnHold(); pressButton(1); return false; };
        button[1].OnInteractEnded += delegate () { OnRelease(); };

        button[2].OnInteract += delegate () { OnHold(); pressButton(2); return false; };
        button[2].OnInteractEnded += delegate () { OnRelease(); };

        button[3].OnInteract += delegate () { OnHold(); pressButton(3); return false; };
        button[3].OnInteractEnded += delegate () { OnRelease(); };

        button[4].OnInteract += delegate () { OnHold(); pressButton(4); return false; };
        button[4].OnInteractEnded += delegate () { OnRelease(); };

        button[5].OnInteract += delegate () { OnHold(); pressButton(5); return false; };
        button[5].OnInteractEnded += delegate () { OnRelease(); };
    }

    

}
