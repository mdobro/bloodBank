using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainGameController : MonoBehaviour {

    public static MainGameController controller;

    public float spawnTime; //time between spawns
    public Transform[] bed_locations; //list of all beds
    public GameObject patient_prefab;
    public Slider healthSlider;
    public Text scoreText;
    public GameObject restartPanel;
    public GameObject inGamePanel; 

    public bool ________________________________;

    public bool[] bed_availible = new bool[6] { true, true, true, true, true, true };
    public int score {
        get {
            return _score;
        }
        set {
            _score = value;
            scoreText.text = _score.ToString();
            print(_score);
        }
    }
    private int _score = 0;

    // Use this for initialization
    void Start() {
        controller = this;
        InvokeRepeating("OpenDoorIfBedAvailible", 1, spawnTime); 
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey("escape")) {
            Application.Quit();
        }
            		
	}

    private void OpenDoorIfBedAvailible() {
        foreach (bool open in bed_availible) {
            if (open) {
                Door.D.OpenDoor();
                break;
            }
        } 
    }

    //Spawns patient if there is a bed availble, called by Door
    public void SpawnPatient() {
        List<int> randomBeds = new List<int>();
        for (int i = 0; i < bed_availible.Length; i++) {
            if (bed_availible[i])
                randomBeds.Add(i);
        }
        int index = randomBeds[Random.Range(0, randomBeds.Count)];
        bed_availible[index] = false;
        Vector3 pos = Door.D.transform.position;
        pos.y -= 1;
        GameObject patient = MonoBehaviour.Instantiate(patient_prefab, pos, Quaternion.identity);
        patient.GetComponent<Patient>().SetTarget(bed_locations[index], index);
    }

    public void ShowRestartPanel() {
        inGamePanel.SetActive(false);
        restartPanel.SetActive(true);

        restartPanel.transform.Find("Score").GetComponent<Text>().text = "Score " + score;

        
    }

    public void RestartGame() {
        SceneManager.LoadScene("Scene_0");
    }

    public void MainMenu() {
        SceneManager.LoadScene("Title_Screen");
    }
}
