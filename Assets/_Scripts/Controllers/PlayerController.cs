﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;



public class PlayerController : MonoBehaviour
{

    public Sprite touchDisabled;
    public Sprite touchActivated;
    public bool gameStarted = false;

    private Sound_Manager sound_manager;
    //**********************************
    //sounds_evolution
    //
    // 0. Heat evolve
    // 1. Cold Evolve
    // 2. Acid Evolve
    // 3. Alkali Evolve 
    // 4. Nerve Evolve
    //*********************************
    //sounds_attacks
    //
    // 
    //*********************************
    //sounds_miscellaneous
    // 0. Unit Select
    // 1. Movement conformation
    // 2. Unit Splits
    // 3. Unit Dies

    private GameObject winScreen;
    private GameObject loseScreen;
    private GameObject Hud_Canvas;
    public static bool isOverUI = false;

    public GameObject friendlySelector;
    public GameObject targetSelector;


    public void TurnOnOverUI()
    {
        isOverUI = true;
    }
    public void TurnOffOverUI()
    {
        isOverUI = false;
    }



    private int terrainLayer;

    public const int MAX_CAP = 20;
    public static int cap = 0;
    public GameObject movePin;
    public GameObject attackPin;
    public bool isSelecting = true;
    GameObject touchButton;

    public int NumStemCells = 0;
    public int NumHeatCells = 0;
    public int NumColdCells = 0;
    public int NumAcidicCells = 0;
    public int NumAlkaliCells = 0;
    public int NumNerveCells = 0;
    public int NumTierTwoCold = 0;
    public int NumTierTwoHeat = 0;
    public int NumEnemiesLeft = 0;

    private int NumStemCells_counter = 0;
    private int NumHeatCells_counter = 0;
    private int NumColdCells_counter = 0;
    private int NumAcidicCells_counter = 0;
    private int NumAlkaliCells_counter = 0;
    private int NumNerveCells_counter = 0;
    private int NumTierTwoCold_counter = 0;
    private int NumTierTwoHeat_counter = 0;
    private int NumEnemiesLeft_counter = 0;

    private int NumCells_counter = 0;

    public List<BaseCell> allSelectableUnits;
    public List<BaseCell> selectedUnits;
    public List<GameObject> allSelectableTargets;
    public List<GameObject> selectedTargets;
    //    List<BaseCell>[] groups;
    public Texture selector;


    private Camera minimapCamera;

    public GUIStyle style_for_badges; // makes the GUI boxes transparent for badges
    public Texture2D badge_Stem;
    public Texture2D badge_Heat;
    public Texture2D badge_Cold;
    public Texture2D badge_Acidic;
    public Texture2D badge_Alkali;
    public Texture2D badge_Heat2;
    public Texture2D badge_Cold2;
    public Texture2D badge_Nerve;

    public int Icon_Spacing;// controls the distance between badges 
    private float badge_scale = .9f; // size of badges
    float fps;
    float initTouchTime;
    float delay;

    Rect GUISelectRect;

    Vector2 origin = new Vector2();

    private Button button_split;
    private Button button_evolve;
    private Button button_merge;
    private Button button_devolve;

    private int moreThanOne_Heat = 0;
    private int moreThanOne_Cold = 0;
    private int moreThanOne_AcidAlkali = 0;

    void Awake()
    {
        Time.timeScale = 1.0f;

        sound_manager = GameObject.FindGameObjectWithTag("Sound_Manager").GetComponent<Sound_Manager>(); // gets the sound sources
        winScreen = GameObject.FindGameObjectWithTag("Win_Screen");
        loseScreen = GameObject.FindGameObjectWithTag("Lose_Screen");
        Hud_Canvas = GameObject.FindGameObjectWithTag("HUD_Canvas");

        winScreen.SetActive(false);
        loseScreen.SetActive(false);

        button_split = GameObject.FindGameObjectWithTag("Button_Split").GetComponent<Button>();
        button_devolve = GameObject.FindGameObjectWithTag("Button_Devolve").GetComponent<Button>();
        button_merge = GameObject.FindGameObjectWithTag("Button_Merge").GetComponent<Button>();
        button_evolve = GameObject.FindGameObjectWithTag("Button_Evolve").GetComponent<Button>();

        button_split.interactable = false;
        button_devolve.interactable = false;
        button_merge.interactable = false;
        button_evolve.interactable = false;

        minimapCamera = GameObject.FindGameObjectWithTag("Minimap_Camera").GetComponent<Camera>();

        touchButton = GameObject.Find("Touch");
        if (!Input.touchSupported)
        {
            GameObject.Find("RIGHT_CLICK_Panel").SetActive(false);
            isSelecting = false;
        }

        terrainLayer = 1 << LayerMask.NameToLayer("Terrain");  // Layer masking for raycast clicking
        // ----------
    }


    public void ToggleSelecting()
    {
        isSelecting = !isSelecting;
        if (isSelecting)
        {
            touchButton.GetComponent<Button>().image.sprite = touchActivated;
        }
        else
        {
            touchButton.GetComponent<Button>().image.sprite = touchDisabled;
        }

    }

    public void AddNewCell(BaseCell _in)
    {
        if (PhotonNetwork.connected && !_in.gameObject.GetPhotonView().isMine)
        {
            allSelectableTargets.Add(_in.gameObject);
        }
        else if (_in.isSinglePlayer && !_in.isMine && !allSelectableTargets.Contains(_in.gameObject))
        {
            allSelectableTargets.Add(_in.gameObject);
        }
        else if (!allSelectableUnits.Contains(_in) && _in.isMine)
        {

            _in.isSelected = true;
            if (!_in.transform.FindChild("FriendlySelector(Clone)"))
            {
                GameObject tFriendlySelector = GameObject.Instantiate(friendlySelector, _in.transform.position, Quaternion.Euler(90.0f, 0.0f, 0.0f)) as GameObject;
                tFriendlySelector.transform.parent = _in.transform;
            }
            allSelectableUnits.Add(_in);
            selectedUnits.Add(_in);
            CheckSelectedUnits();
        }
    }

    public void AddNewProtein(Protein _in)
    {
        allSelectableTargets.Add(_in.gameObject);
        // selectedTargets.Add(_in.gameObject);
        CheckSelectedUnits();
    }

    public void RemoveDeadCell(BaseCell _in)
    {
        _in.isSelected = false;
        allSelectableUnits.Remove(_in);
        selectedUnits.Remove(_in);
        CheckSelectedUnits();
    }
    public void DeselectCell(BaseCell _in)
    {
        _in.isSelected = false;
        selectedUnits.Remove(_in);
    }

    public void DeselectCells()
    {
        foreach (BaseCell item in selectedUnits)
        {
            item.isSelected = false;
        }
        selectedUnits.Clear();
    }


    public void RemoveTarget(GameObject _in)
    {
        allSelectableTargets.Remove(_in);
        selectedTargets.Remove(_in);
    }

    public List<GameObject> GetAllSelectableUnits()
    {
        List<GameObject> allSelectableObjects = new List<GameObject>(); // Initialize a list of GameObjects
        foreach (BaseCell item in allSelectableUnits) // For each of the player's controllable cells
        {
            allSelectableObjects.Add(item.gameObject); // Add the cell's GameObject to the list
        }
        return allSelectableObjects; // Return the list
    }

    public List<GameObject> GetAllSelectableTargets()
    {
        List<GameObject> allSelectableObjects = new List<GameObject>(); // Initialize a list of GameObjects
        foreach (GameObject item in allSelectableTargets) // For each of the player's controllable cells
        {
            allSelectableObjects.Add(item.gameObject); // Add the cell's GameObject to the list
        }
        return allSelectableObjects; // Return the list
    }

    public List<GameObject> GetSelectedUnits()
    {
        List<GameObject> allSelectedUnits = new List<GameObject>();
        foreach (BaseCell item in selectedUnits)
        {
            allSelectedUnits.Add(item.gameObject);
        }
        return allSelectedUnits;
    }

    public void UnitSelection(Vector2 origin)
    {
        if (!minimapCamera.pixelRect.Contains(Input.mousePosition))
        {
            if (Input.mousePosition.x >= origin.x)
            {
                GUISelectRect.xMax = Input.mousePosition.x;
            }
            else
            {
                GUISelectRect.xMin = Input.mousePosition.x;
            }

            if (-Input.mousePosition.y + Screen.height >= origin.y)
            { GUISelectRect.yMax = -Input.mousePosition.y + Screen.height; }
            else
            { GUISelectRect.yMin = -Input.mousePosition.y + Screen.height; }

            foreach (BaseCell item in selectedUnits)
            {
                item.isSelected = false;
            }
            selectedUnits.Clear();
            foreach (BaseCell item in allSelectableUnits)
            {
                Vector3 itemPos = Camera.main.WorldToScreenPoint(item.transform.position);
                itemPos.y = -itemPos.y + Screen.height;
                if (GUISelectRect.Contains(itemPos))
                {
                    selectedUnits.Add(item);
                    item.isSelected = true;


                    if (!item.transform.FindChild("FriendlySelector(Clone)"))
                    {
                        GameObject tFriendlySelector = GameObject.Instantiate(friendlySelector, item.transform.position, Quaternion.Euler(90.0f, 0.0f, 0.0f)) as GameObject;
                        tFriendlySelector.transform.parent = item.transform;
                    }

                    if (!sound_manager.sounds_miscellaneous[0].isPlaying)
                    {
                        sound_manager.sounds_miscellaneous[0].Play();
                    }

                }
            }
        }
    }


    public void TouchUnitSelection(Vector2 origin)
    {

        if (Input.GetTouch(0).position.x >= origin.x)
        {
            GUISelectRect.xMax = Input.GetTouch(0).position.x;
        }
        else
        {
            GUISelectRect.xMin = Input.GetTouch(0).position.x;
        }

        if (-Input.GetTouch(0).position.y + Screen.height >= origin.y)
        { GUISelectRect.yMax = -Input.GetTouch(0).position.y + Screen.height; }
        else
        { GUISelectRect.yMin = -Input.GetTouch(0).position.y + Screen.height; }

        if (selectedUnits.Count > 0)
        {
            DeselectCells();
        }


        foreach (BaseCell item in allSelectableUnits)
        {
            Vector3 itemPos = Camera.main.WorldToScreenPoint(item.transform.position);
            itemPos.y = -itemPos.y + Screen.height;
            if (GUISelectRect.Contains(itemPos))
            {
                selectedUnits.Add(item);
                item.isSelected = true;

                //draw selector
                if (!item.transform.FindChild("FriendlySelector(Clone)"))
                {
                    GameObject tFriendlySelector = GameObject.Instantiate(friendlySelector, item.transform.position, Quaternion.Euler(90.0f, 0.0f, 0.0f)) as GameObject;
                    tFriendlySelector.transform.parent = item.transform;
                }

                if (!sound_manager.sounds_miscellaneous[0].isPlaying)
                {
                    sound_manager.sounds_miscellaneous[0].Play();
                }
            }
        }
    }

    public void TouchTargetSelection(Vector2 origin)
    {
        if (Input.GetTouch(0).position.x >= origin.x)
        {
            GUISelectRect.xMax = Input.GetTouch(0).position.x;
        }
        else
        {
            GUISelectRect.xMin = Input.GetTouch(0).position.x;
        }

        if (-Input.GetTouch(0).position.y + Screen.height >= origin.y)
        { GUISelectRect.yMax = -Input.GetTouch(0).position.y + Screen.height; }
        else
        { GUISelectRect.yMin = -Input.GetTouch(0).position.y + Screen.height; }

        selectedTargets.Clear();
        foreach (GameObject item in allSelectableTargets)
        {
            Vector3 itemPos = Camera.main.WorldToScreenPoint(item.transform.position);
            itemPos.y = -itemPos.y + Screen.height;
            if (GUISelectRect.Contains(itemPos))
            {
                if (item.GetComponent<FogOfWarHider>().isVisible)
                {
                    selectedTargets.Add(item);
                    if (!item.transform.FindChild("TargetSelector(Clone)"))
                    {
                        GameObject tTargetSelector = GameObject.Instantiate(targetSelector, item.transform.position, Quaternion.Euler(90.0f, 0.0f, 0.0f)) as GameObject;
                        tTargetSelector.transform.parent = item.transform;
                    }
                }
            }
        }
    }

    public void TargetSelection(Vector2 origin)
    {
        if (Input.mousePosition.x >= origin.x)
        {
            GUISelectRect.xMax = Input.mousePosition.x;
        }
        else
        {
            GUISelectRect.xMin = Input.mousePosition.x;
        }

        if (-Input.mousePosition.y + Screen.height >= origin.y)
        { GUISelectRect.yMax = -Input.mousePosition.y + Screen.height; }
        else
        { GUISelectRect.yMin = -Input.mousePosition.y + Screen.height; }

        selectedTargets.Clear();
        foreach (GameObject item in allSelectableTargets)
        {
            Vector3 itemPos = Camera.main.WorldToScreenPoint(item.transform.position);
            itemPos.y = -itemPos.y + Screen.height;
            if (GUISelectRect.Contains(itemPos))
            {
                if (item.GetComponent<FogOfWarHider>().isVisible)
                {
                    selectedTargets.Add(item);

                    if (!item.transform.FindChild("TargetSelector(Clone)"))
                    {
                        GameObject tTargetSelector = GameObject.Instantiate(targetSelector, item.transform.position, Quaternion.Euler(90.0f, 0.0f, 0.0f)) as GameObject;
                        tTargetSelector.transform.parent = item.transform;
                    }
                }

            }
        }
    }

    public void UnitMove()
    {
        if (selectedUnits.Count <= 0)
        {
            return;
        }
        // Modified by using raycast
        RaycastHit hitInfo;
        Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(screenRay, out hitInfo, 1000.0f, terrainLayer))
        {
            EventManager.Move(hitInfo.point);
            GameObject.Instantiate(movePin, hitInfo.point, Quaternion.Euler(90.0f, 0.0f, 0.0f));


            if (!sound_manager.sounds_miscellaneous[1].isPlaying)
            {
                sound_manager.sounds_miscellaneous[1].Play();
            }
        }

    }


    public void UnitAttackMove()
    {
        // Modified by using raycast
        RaycastHit hitInfo;
        Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(screenRay, out hitInfo, 1000.0f, terrainLayer))
        {
            EventManager.AttackMove(hitInfo.point);
        }
    }

    public void UnitAttack()
    {
        EventManager.Attack(selectedTargets[0]);

    }


    public void UnitSplit()
    {
        EventManager.Split();


        if (!sound_manager.sounds_miscellaneous[2].isPlaying)
        {
            sound_manager.sounds_miscellaneous[2].Play();


        }
    }

    public void UnitEvolve(int cellNum)
    {
        switch (cellNum)
        {
            case 0: //turn into heat cell
                EventManager.Evolve(CellType.HEAT_CELL);
                sound_manager.sounds_evolution[cellNum].Play();
                break;
            case 1: //turn into cold cell
                EventManager.Evolve(CellType.COLD_CELL);
                sound_manager.sounds_evolution[cellNum].Play();
                break;
            case 2: //turn into acidic cell
                EventManager.Evolve(CellType.ACIDIC_CELL);
                sound_manager.sounds_evolution[cellNum].Play();
                break;
            case 3: //turn into alkali cell
                EventManager.Evolve(CellType.ALKALI_CELL);
                sound_manager.sounds_evolution[cellNum].Play();
                break;
            default:
                break;
        }

    }

    public void UnitHarvest()
    {
        EventManager.Consume(selectedTargets[0]);
    }

    public void UnitIncubation()
    {

    }

    public void DoubleClick()
    {
        BaseCell selectedCell = selectedUnits[0].GetComponent<BaseCell>(); // Get double-clicked cell;
        CellType selectedType = selectedCell.celltype; // Get the type of cell it is
        selectedUnits.Clear(); // Remove control of any currently selected units
        foreach (BaseCell item in allSelectableUnits) // For each of the player's selectable units
        {
            if (item.celltype == selectedType) // If the type matches the double-clicked cell
            {
                selectedUnits.Add(item); // Add the cell to the players selected units
                item.isSelected = true;

            }
        }
    }

    public void Grouping()
    {
    }

    public void DrawPin()
    {
    }

    public void OnGUI()
    {
        if (Time.timeScale > 0.0f)
        {
#if UNITY_EDITOR
            GUI.Label(Rect.MinMaxRect(0, 0, Screen.width, Screen.height), fps.ToString());
#endif
            if (GUISelectRect.height != 0 && GUISelectRect.width != 0)
            {
                if (!isOverUI)
                {
                    if (isSelecting || Input.GetMouseButton(0))
                    {
                        GUI.color = new Color(0.0f, 1.0f, 0.0f, 0.5f);
                    }
                    else if (!isSelecting || Input.GetMouseButton(1))
                    {
                        GUI.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
                    }

                    GUI.DrawTexture(GUISelectRect, selector, ScaleMode.StretchToFill, true);
                }

            }



            if (Time.timeScale > 0.0f)
            {
                button_split.interactable = false;
                button_devolve.interactable = false;
                button_merge.interactable = false;
                button_evolve.interactable = false;

                GUI.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                GUI.BeginGroup(new Rect(Screen.width * 0.18f, 20 * badge_scale, Screen.width * 0.8f, 512));

                moreThanOne_Heat = 0;
                moreThanOne_Cold = 0;
                moreThanOne_AcidAlkali = 0;

                float health_ratio = 0;// = item.currentProtein / item.MAX_PROTEIN;

                foreach (BaseCell item in selectedUnits)
                {
                    NumCells_counter++;
           
                   
                    if (item.celltype == CellType.STEM_CELL)
                    {
                        GUI.Box(new Rect(0 + NumCells_counter * Icon_Spacing * badge_scale, 0, 86 * badge_scale, 86), badge_Stem, style_for_badges);
                        button_split.interactable = true;
                        button_evolve.interactable = true;
                        health_ratio = item.gameObject.GetComponent<StemCell>().currentProtein / item.gameObject.GetComponent<StemCell>().MAX_PROTEIN;
                    }
                    else if (item.celltype == CellType.HEAT_CELL)
                    {
                        GUI.Box(new Rect(0 + NumCells_counter * Icon_Spacing * badge_scale, 0, 86 * badge_scale, 86), badge_Heat, style_for_badges);
                       // button_split.interactable = true;
                        moreThanOne_Heat++;
                        health_ratio = item.gameObject.GetComponent<HeatCell>().currentProtein / item.gameObject.GetComponent<HeatCell>().MAX_PROTEIN;
                        if (moreThanOne_Heat > 1 && item.gameObject.GetComponent<HeatCell>().Inheat == true)
                        {
                            button_merge.interactable = true;
                        }
                    }
                    else if (item.celltype == CellType.COLD_CELL)
                    {
                        GUI.Box(new Rect(0 + NumCells_counter * Icon_Spacing * badge_scale, 0, 86 * badge_scale, 86), badge_Cold, style_for_badges);
                     //   button_split.interactable = true;
                        moreThanOne_Cold++;
                        health_ratio = item.gameObject.GetComponent<ColdCell>().currentProtein / item.gameObject.GetComponent<ColdCell>().MAX_PROTEIN;
                        if (moreThanOne_Cold > 1 && item.gameObject.GetComponent<ColdCell>().InCold == true)
                        {
                            button_merge.interactable = true;
                        }
                    }
                    else if (item.celltype == CellType.COLD_CELL_TIRE2)
                    {
                        GUI.Box(new Rect(0 + NumCells_counter * Icon_Spacing * badge_scale, 0, 86 * badge_scale, 86), badge_Cold2, style_for_badges);
                        button_devolve.interactable = true;
                        health_ratio = item.gameObject.GetComponent<Tier2ColdCell>().currentProtein / item.gameObject.GetComponent<Tier2ColdCell>().MAX_PROTEIN;
                    }
                    else if (item.celltype == CellType.HEAT_CELL_TIRE2)
                    {
                        GUI.Box(new Rect(0 + NumCells_counter * Icon_Spacing * badge_scale, 0, 86 * badge_scale, 86), badge_Heat2, style_for_badges);
                        button_devolve.interactable = true;
                        health_ratio = item.gameObject.GetComponent<Tier2HeatCell>().currentProtein / item.gameObject.GetComponent<Tier2HeatCell>().MAX_PROTEIN;
                    }
                    else if (item.celltype == CellType.ACIDIC_CELL)
                    {
                        GUI.Box(new Rect(0 + NumCells_counter * Icon_Spacing * badge_scale, 0, 86 * badge_scale, 86), badge_Acidic, style_for_badges);
                        moreThanOne_AcidAlkali++;
                        health_ratio = item.gameObject.GetComponent<AcidicCell>().currentProtein / item.gameObject.GetComponent<AcidicCell>().MAX_PROTEIN;
                        if (moreThanOne_AcidAlkali > 1)
                        {
                            button_merge.interactable = true;
                        }
                    }
                    else if (item.celltype == CellType.ALKALI_CELL)
                    {
                        moreThanOne_AcidAlkali++;
                        GUI.Box(new Rect(0 + NumCells_counter * Icon_Spacing * badge_scale, 0, 86 * badge_scale, 86), badge_Alkali, style_for_badges);
                        health_ratio = item.gameObject.GetComponent<AlkaliCell>().currentProtein / item.gameObject.GetComponent<AlkaliCell>().MAX_PROTEIN;
                        if (moreThanOne_AcidAlkali > 1)
                        {
                            button_merge.interactable = true;
                        }
                    }
                    else if (item.celltype == CellType.NERVE_CELL)
                    {
                        button_devolve.interactable = true;
                        health_ratio = item.gameObject.GetComponent<NerveCell>().currentProtein / item.gameObject.GetComponent<NerveCell>().MAX_PROTEIN;
                        GUI.Box(new Rect(0 + NumCells_counter * Icon_Spacing * badge_scale, 0, 86 * badge_scale, 86), badge_Nerve, style_for_badges);
                    }
                 
                    Texture2D texture = new Texture2D(1, 1);
                    texture.SetPixel(0, 0, Color.green);
                    if (health_ratio <= 0.2f)
                        texture.SetPixel(0, 0, Color.red);
                    else if (health_ratio <= 0.4f)
                        texture.SetPixel(0, 0, Color.yellow);
                    texture.Apply();
                    GUI.DrawTexture(new Rect(15 * badge_scale + NumCells_counter * Icon_Spacing * badge_scale, 4 * badge_scale, 58 * health_ratio * badge_scale, 4), texture);



                    //   switch (item.celltype)
                    //   {
                    //       case CellType.STEM_CELL:
                    //           {
                    //               GUI.Box(new Rect(0 + NumCells_counter * Icon_Spacing * badge_scale, 0, 86 * badge_scale, 86), badge_Stem, style_for_badges);
                    //
                    //               break;
                    //           }
                    //       case CellType.HEAT_CELL:
                    //           {
                    //               GUI.Box(new Rect(0 + NumCells_counter * Icon_Spacing * badge_scale, 0, 86 * badge_scale, 86), badge_Heat, style_for_badges);
                    //               break;
                    //           }
                    //       case CellType.COLD_CELL: GUI.Box(new Rect(0 + NumCells_counter * Icon_Spacing * badge_scale, 0, 86 * badge_scale, 86), badge_Cold, style_for_badges);
                    //           break;
                    //       case CellType.HEAT_CELL_TIRE2:GUI.Box(new Rect(0 + NumCells_counter * Icon_Spacing * badge_scale, 0, 86 * badge_scale, 86), badge_Heat2, style_for_badges);
                    //           break;
                    //       case CellType.COLD_CELL_TIRE2:GUI.Box(new Rect(0 + NumCells_counter * Icon_Spacing * badge_scale, 0, 86 * badge_scale, 86), badge_Cold2, style_for_badges);
                    //           break;
                    //       case CellType.ACIDIC_CELL:GUI.Box(new Rect(0 + NumCells_counter * Icon_Spacing * badge_scale, 0, 86 * badge_scale, 86), badge_Acidic, style_for_badges);
                    //           break;
                    //       case CellType.ALKALI_CELL:GUI.Box(new Rect(0 + NumCells_counter * Icon_Spacing * badge_scale, 0, 86 * badge_scale, 86), badge_Alkali, style_for_badges);
                    //           break;
                    //       case CellType.CANCER_CELL:GUI.Box(new Rect(0 + NumCells_counter * Icon_Spacing * badge_scale, 0, 86 * badge_scale, 86), badge_Heat, style_for_badges);
                    //           break;
                    //       case CellType.NERVE_CELL:GUI.Box(new Rect(0 + NumCells_counter * Icon_Spacing * badge_scale, 0, 86 * badge_scale, 86), badge_Nerve, style_for_badges);
                    //           break;
                    //       default:
                    //           break;
                    //   }


                    // drwas the life bar
       
                }

                if (NumCells_counter >= selectedUnits.Count)
                {
                    NumCells_counter = 0;
                }

                // NumHeatCells = selectedUnits.FindAll(item => (item != null) && (item.celltype == CellType.HEAT_CELL)).Count;





                //       GUI.Box(new Rect(160, 0, 75, 60), "Cold Cells: ");
                //       GUI.Label(new Rect(195, 35, 50, 50), NumColdCells.ToString());
                //
                //       GUI.Box(new Rect(240, 0, 75, 60), "Acidic Cells: ");
                //       GUI.Label(new Rect(275, 35, 50, 50), NumAcidicCells.ToString());
                //
                //       GUI.Box(new Rect(320, 0, 75, 60), "Alkali Cells: ");
                //       GUI.Label(new Rect(355, 35, 50, 50), NumAlkaliCells.ToString());
                //
                //       GUI.Box(new Rect(400, 0, 75, 60), "Nerve Cells: ");
                //       GUI.Label(new Rect(435, 35, 50, 50), NumNerveCells.ToString());
                //
                //       GUI.Box(new Rect(480, 0, 75, 60), "Tier 2\nHeat Cells: ");
                //       GUI.Label(new Rect(515, 35, 50, 50), NumTierTwoHeat.ToString());
                //
                //       GUI.Box(new Rect(560, 0, 75, 60), "Tier 2\nCold Cells: ");
                //       GUI.Label(new Rect(595, 35, 50, 50), NumTierTwoCold.ToString());
                //
                //       GUI.Box(new Rect(640, 0, 75, 60), "Enemies\nLeft: ");
                //       GUI.Label(new Rect(675, 35, 50, 50), NumEnemiesLeft.ToString());
                //
                //       GUI.Box(new Rect(720, 0, 75, 60), "Cap: ");
                //       GUI.Label(new Rect(755, 35, 50, 50), cap.ToString());
                GUI.EndGroup();
            }



        }
    }

    public void FixedUpdate()
    {


        if (isSelecting && Input.touchSupported)
        {
            touchButton.GetComponent<Button>().image.sprite = touchButton.GetComponent<Button>().spriteState.pressedSprite;

        }
        else if (Input.touchSupported)
        {
            touchButton.GetComponent<Button>().image.sprite = touchButton.GetComponent<Button>().spriteState.disabledSprite;
        }





    }

    public void UnitStop()
    {
        selectedTargets.Clear();
        EventManager.Stop();
    }

    public void UnitMerge()
    {
        EventManager.Merge();
    }

    public void UnitRevert()
    {
        EventManager.Revert();
    }



    void TouchUpdate()
    {

        if (!isOverUI && Time.timeScale > 0.0f)
        {
            if (Input.touchCount == 1)
            {
                Touch oneTouch = Input.GetTouch(0);
                if (oneTouch.phase == TouchPhase.Began)
                {
                    if (oneTouch.tapCount >= 2)
                    {
                        DeselectCells();
                        return;
                    }
                }

            }

            if (Input.touchCount == 1 && isSelecting && !minimapCamera.pixelRect.Contains(Input.GetTouch(0).position))
            {
                Touch oneTouch = Input.GetTouch(0);
                switch (oneTouch.phase)
                {
                    case TouchPhase.Began:

                        GUISelectRect.xMax = oneTouch.position.x;
                        GUISelectRect.yMax = oneTouch.position.y;
                        GUISelectRect.xMin = oneTouch.position.x;
                        GUISelectRect.yMin = -oneTouch.position.y + Screen.height;

                        origin = oneTouch.position;
                        origin.y = -origin.y + Screen.height;
                        break;
                    case TouchPhase.Canceled:
                        break;
                    case TouchPhase.Ended:
                        TouchUnitSelection(origin);
                        GUISelectRect.xMax = GUISelectRect.xMin;
                        GUISelectRect.yMax = GUISelectRect.yMin;
                        break;
                    case TouchPhase.Moved:
                        TouchUnitSelection(origin);
                        break;
                    case TouchPhase.Stationary:
                        break;
                    default:
                        break;
                }
            }

            if (Input.touchCount == 1 && !isSelecting && !isOverUI && !minimapCamera.pixelRect.Contains(Input.GetTouch(0).position))
            {
                Touch touchOne = Input.GetTouch(0);
                switch (touchOne.phase)
                {
                    case TouchPhase.Began:


                        GUISelectRect.xMax = touchOne.position.x;
                        GUISelectRect.yMax = touchOne.position.y;
                        GUISelectRect.xMin = touchOne.position.x;
                        GUISelectRect.yMin = -touchOne.position.y + Screen.height;

                        origin = touchOne.position;
                        origin.y = -origin.y + Screen.height;

                        break;
                    case TouchPhase.Canceled:
                        break;
                    case TouchPhase.Ended:
                        GUISelectRect.xMax = GUISelectRect.xMin;
                        GUISelectRect.yMax = GUISelectRect.yMin;

                        if (selectedUnits.Count == 0)
                        {
                            return;
                        }


                        //single tap commands
                        RaycastHit hitInfo;
                        Ray screenRay = Camera.main.ScreenPointToRay(touchOne.position);

                        if (Physics.Raycast(screenRay, out hitInfo, 1000.0f, terrainLayer))
                        {
                            if (hitInfo.collider.tag == "Unit" || hitInfo.collider.tag == "Protein")
                            {
                                if (hitInfo.collider.GetComponent<FogOfWarHider>().isVisible)
                                {
                                    selectedTargets.Add(hitInfo.collider.gameObject);
                                    if (!hitInfo.transform.FindChild("TargetSelector(Clone)"))
                                    {
                                        GameObject tTargetSelector = GameObject.Instantiate(targetSelector, hitInfo.transform.position, Quaternion.Euler(90.0f, 0.0f, 0.0f)) as GameObject;
                                        tTargetSelector.transform.parent = hitInfo.transform;
                                    }
                                }
                            }
                            else if (selectedTargets.Count == 0)
                            {
                                EventManager.Move(hitInfo.point);
                                GameObject.Instantiate(movePin, hitInfo.point, Quaternion.Euler(90.0f, 0.0f, 0.0f));
                                if (!sound_manager.sounds_miscellaneous[1].isPlaying)
                                {
                                    sound_manager.sounds_miscellaneous[1].Play();
                                }
                            }


                        }

                        break;
                    case TouchPhase.Moved:
                        TouchTargetSelection(origin);
                        break;
                    case TouchPhase.Stationary:
                        TouchTargetSelection(origin);
                        break;
                    default:
                        break;
                }

                if (selectedTargets.Count > 0)
                {
                    foreach (BaseCell item in selectedUnits)
                    {
                        item.SetTargets(selectedTargets);
                        item.SetPrimaryTarget(selectedTargets[0]);
                    }
                    if (selectedTargets[0].tag == "Protein")
                    {
                        UnitHarvest();
                    }
                    else
                        UnitAttack();
                }

            }
        }
        else
        {
            GUISelectRect.xMax = GUISelectRect.xMin;
            GUISelectRect.yMax = GUISelectRect.yMin;
        }
    }

    void MouseKeyBoardUpdate()
    {
        if (!isOverUI && Time.timeScale > 0.0f)
        {
            //Vector3 topleft = new Vector3(GUISelectRect.xMin, GUISelectRect.yMin, Camera.main.transform.position.z);
            //Vector3 bottomright = new Vector3(GUISelectRect.xMax, GUISelectRect.yMin, Camera.main.transform.position.z);

            if (Input.GetKeyDown(KeyCode.D)) // If the player presses D
            {
                UnitSplit();

            }

            if (Input.GetKeyDown(KeyCode.Alpha1)) // If the player presses 1
            {
                UnitRevert();

            }

            if (Input.GetKeyDown(KeyCode.Q)) // If the player presses Q
            {
                UnitMerge();

            }

            if (Input.GetKeyDown(KeyCode.S)) // If the player presses S
            {
                UnitStop();
            }

            if (Input.GetKeyDown(KeyCode.C)) // If the player presses C
            {
                EventManager.Evolve(CellType.ACIDIC_CELL);


            }

            if (Input.GetKeyDown(KeyCode.V)) // If the player presses V
            {
                EventManager.Evolve(CellType.ALKALI_CELL);

            }

            if (Input.GetKeyDown(KeyCode.X)) // If the player presses X
            {
                EventManager.Evolve(CellType.HEAT_CELL);


            }

            if (Input.GetKeyDown(KeyCode.Z)) // If the player presses Z
            {
                EventManager.Evolve(CellType.COLD_CELL);


            }

            if (!isOverUI && Time.timeScale > 0.0f)
            {

                if (Input.GetMouseButtonDown(0) && !minimapCamera.pixelRect.Contains(Input.mousePosition)) // If the player left-clicks
                {
                    GUISelectRect.xMin = Input.mousePosition.x;
                    GUISelectRect.yMin = Input.mousePosition.y;
                    GUISelectRect.xMax = Input.mousePosition.x;
                    GUISelectRect.yMax = Input.mousePosition.y;

                    GUISelectRect.xMin = Input.mousePosition.x;
                    GUISelectRect.yMin = -Input.mousePosition.y + Screen.height;
                    origin = Input.mousePosition;
                    origin.y = -origin.y + Screen.height;

                }
                else if (Input.GetMouseButtonUp(0) && !minimapCamera.pixelRect.Contains(Input.mousePosition)) // When the player releases left-click
                {
                    GUISelectRect.yMax = GUISelectRect.yMin;
                    GUISelectRect.xMax = GUISelectRect.xMin;
                    if (selectedUnits.Count == 0)
                    {
                        RaycastHit hitInfo;
                        Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                        if (Physics.Raycast(screenRay, out hitInfo, 1000.0f))
                        {
                            BaseCell hitCell = hitInfo.collider.gameObject.GetComponentInParent<BaseCell>();
                            if (allSelectableUnits.Contains(hitCell))
                            {
                                hitInfo.collider.gameObject.GetComponentInParent<BaseCell>().isSelected = true;
                                selectedUnits.Add(hitInfo.collider.gameObject.GetComponentInParent<BaseCell>());
                                if (!hitInfo.transform.FindChild("FriendlySelector(Clone)"))
                                {
                                    GameObject tFriendlySelector = GameObject.Instantiate(friendlySelector, hitInfo.transform.position, Quaternion.Euler(90.0f, 0.0f, 0.0f)) as GameObject;
                                    tFriendlySelector.transform.parent = hitInfo.transform;
                                }
                            }
                        }
                    }

                }
                else if (Input.GetMouseButton(0) && !minimapCamera.pixelRect.Contains(Input.mousePosition)) // If the player has left-click held down
                {

                    UnitSelection(origin);

                }


                if (Input.GetMouseButtonDown(1) && !minimapCamera.pixelRect.Contains(Input.mousePosition)) // If the player right-clicks
                {

                    GUISelectRect.xMin = Input.mousePosition.x;
                    GUISelectRect.yMin = Input.mousePosition.y;
                    GUISelectRect.xMax = Input.mousePosition.x;
                    GUISelectRect.yMax = Input.mousePosition.y;

                    GUISelectRect.xMin = Input.mousePosition.x;
                    GUISelectRect.yMin = -Input.mousePosition.y + Screen.height;
                    origin = Input.mousePosition;
                    origin.y = -origin.y + Screen.height;
                }
                else if (Input.GetMouseButtonUp(1) && !minimapCamera.pixelRect.Contains(Input.mousePosition)) // When the player releases right-click
                {



                    GUISelectRect.yMax = GUISelectRect.yMin;
                    GUISelectRect.xMax = GUISelectRect.xMin;

                    RaycastHit hitInfo;
                    Ray screenRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(screenRay, out hitInfo, 1000.0f))
                    {
                        GameObject hitObject = hitInfo.collider.gameObject;
                        if (allSelectableTargets.Contains(hitObject))
                        {
                            if (hitObject.GetComponent<FogOfWarHider>().isVisible)
                            {
                                selectedTargets.Add(hitObject);
                                if (!hitObject.transform.FindChild("TargetSelector(Clone)"))
                                {
                                    GameObject tTargetSelector = GameObject.Instantiate(targetSelector, hitObject.transform.position, Quaternion.Euler(90.0f, 0.0f, 0.0f)) as GameObject;
                                    tTargetSelector.transform.parent = hitObject.transform;
                                }
                            }
                        }
                    }


                    if (selectedTargets.Count > 0)
                    {
                        foreach (BaseCell item in selectedUnits)
                        {
                            item.SetTargets(selectedTargets);
                            item.SetPrimaryTarget(selectedTargets[0]);
                        }
                        if (selectedTargets[0].tag == "Protein")
                        {
                            UnitHarvest();
                        }
                        else
                            UnitAttack();
                    }
                    else
                        UnitMove();

                }
                else if (Input.GetMouseButton(1) && !minimapCamera.pixelRect.Contains(Input.mousePosition)) // If the player has right-click held down
                {
                    TargetSelection(origin);
                }
                if (Input.GetMouseButtonDown(1) && minimapCamera.pixelRect.Contains(Input.mousePosition))
                {
                    RaycastHit hitPosition;
                    Ray ray = minimapCamera.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out hitPosition))
                    {
                        //move selected units to that position
                        foreach (BaseCell item in selectedUnits)
                        {
                            item.Move(hitPosition.point);
                        }
                    }
                }



            }
            if (Input.GetMouseButton(2) && !minimapCamera.pixelRect.Contains(Input.mousePosition))
            {
                UnitAttackMove();
            }
        }
        else
        {
            GUISelectRect.xMax = GUISelectRect.xMin;
            GUISelectRect.yMax = GUISelectRect.yMin;
        }
    }
    // Update is called once per frame
    void LateUpdate()
    {
#if UNITY_EDITOR
        fps = 1.0f / Time.deltaTime;
#endif
        cap = allSelectableUnits.Count;

        selectedUnits.RemoveAll(item => item == null);
        selectedTargets.RemoveAll(item => item == null);
        allSelectableTargets.RemoveAll(item => item == null);


        if (Input.touchSupported)
        {
            TouchUpdate();
        }
        else
        {
            MouseKeyBoardUpdate();
        }

        CheckSelectedUnits();
        CheckEnemiesLeft();

        if (allSelectableUnits.FindAll(item => item.celltype == CellType.STEM_CELL).Count == 0 && gameStarted)
        {
            Show_LoseScreen();
        }


    }

    public void CheckSelectedUnits()
    {
        NumStemCells = selectedUnits.FindAll(item => (item != null) && (item.celltype == CellType.STEM_CELL)).Count;
        NumHeatCells = selectedUnits.FindAll(item => (item != null) && (item.celltype == CellType.HEAT_CELL)).Count;
        NumColdCells = selectedUnits.FindAll(item => (item != null) && (item.celltype == CellType.COLD_CELL)).Count;
        NumAcidicCells = selectedUnits.FindAll(item => (item != null) && (item.celltype == CellType.ACIDIC_CELL)).Count;
        NumAlkaliCells = selectedUnits.FindAll(item => (item != null) && (item.celltype == CellType.ALKALI_CELL)).Count;
        NumTierTwoCold = selectedUnits.FindAll(item => (item != null) && (item.celltype == CellType.COLD_CELL_TIRE2)).Count;
        NumTierTwoHeat = selectedUnits.FindAll(item => (item != null) && (item.celltype == CellType.HEAT_CELL_TIRE2)).Count;
        NumNerveCells = selectedUnits.FindAll(item => (item != null) && (item.celltype == CellType.NERVE_CELL)).Count;
    }

    public void CheckEnemiesLeft()
    {

        List<GameObject> enemies = new List<GameObject>(allSelectableTargets);

        enemies.RemoveAll(item => (item != null) && (item.tag == "Protein") );
       
        NumEnemiesLeft = enemies.Count;

        if (enemies.Count == 0 && gameStarted) // if there are no enemies left, the player has won the game
        {

            Show_WinningScreen();
        }


    }

    void Show_WinningScreen()
    {
        winScreen.SetActive(true);

        if (!sound_manager.win_music.isPlaying)
        {
            sound_manager.win_music.Play();
        }
        winScreen.GetComponentInChildren<Image>().enabled = true;
        Image[] test = winScreen.GetComponentsInChildren<Image>();

        foreach (Image img in test)
        {
            img.enabled = true;

        }

        Time.timeScale = 0.0f;
        this.gameObject.SetActive(false);
        Invoke("NextLevel", 5.0f);
    }
    void Show_LoseScreen()
    {

        loseScreen.SetActive(true);
        if (!sound_manager.lose_music.isPlaying)
        {
            sound_manager.lose_music.Play();
        }
        loseScreen.GetComponentInChildren<Image>().enabled = true;

        Image[] test = loseScreen.GetComponentsInChildren<Image>();

        foreach (Image img in test)
        {
            img.enabled = true;

        }

        this.gameObject.SetActive(false);
        Invoke("GoBackToMainMenu", 5.0f);

    }

    public void NextLevel()
    {
        if (PhotonNetwork.connected)
        {
            PhotonNetwork.LeaveRoom();
            Application.LoadLevel("Mulitplayer_Lobby");
        }
        else
        {
            if (Application.loadedLevelName == "Singleplayer_Level3")
            {
                Application.LoadLevel("Singleplayer_Level2");
            }
            else if (Application.loadedLevelName == "Singleplayer_Level2")
            {
                Application.LoadLevel("Singleplayer_Level1");
            }
            else if (Application.loadedLevelName == "Singleplayer_Level1")
            {
                Application.LoadLevel("Credits");
            }
            else if (Application.loadedLevelName == "Multiplayer_Level")
            {
                Application.LoadLevel("MainMenu");
            }
        }
    }


    public void GoBackToMainMenu()
    {
        if (PhotonNetwork.connected)
        {
            PhotonNetwork.LeaveRoom();
            Application.LoadLevel("Mulitplayer_Lobby");
        }
        else
            Application.LoadLevel("MainMenu");
    }

}
