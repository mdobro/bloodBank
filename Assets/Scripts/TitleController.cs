using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour {

    public GameObject mainPanel;
    public GameObject howToPanel;
    public GameObject creditsPanel;

    public void StartGame() {  
        SceneManager.LoadScene("Scene_0");
    }

    public void HowToPlay() {
        howToPanel.SetActive(true);
        mainPanel.SetActive(false);
    }

    public void Credits() {
        creditsPanel.SetActive(true);
        mainPanel.SetActive(false);
    }

    public void ReturnToMain() {
        howToPanel.SetActive(false);
        creditsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void ExitGame() {
        Application.Quit();
    }
}
