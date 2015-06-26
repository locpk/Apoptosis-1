using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Instructions_ImageSwap_Controller : MonoBehaviour
{


    public Sprite Controls_Sprite;
    public Sprite EvolutionTree_Sprite;

    public int starting_page;
    public GameObject canvas;


    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {







    }

    public void NextPage()
    {
        starting_page++;

        switch (starting_page)
        {
    
            case 0:
                // case empty for default strite               

                break;

            case 1:
                canvas.GetComponent<Image>().sprite = EvolutionTree_Sprite;
                break;

            case 2:
                canvas.GetComponent<Image>().sprite = Controls_Sprite;
                break;


            default:
                starting_page = 0;
                Application.LoadLevel("MainMenu");
                break;
        }
    }
}