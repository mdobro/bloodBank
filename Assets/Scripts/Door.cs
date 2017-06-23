using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour {

    public static Door D;

    public StateMachine animation_state_machine;
    public Sprite[] door_animation;

    public bool _________________________;
 
    float animation_start_time;
    int fps = 6;
    bool doorOpening;
    bool doorClosing;
    bool spawnPatient;

    // Use this for initialization
    void Start () {
        D = this;
        doorOpening = false;
        doorClosing = false;
    }
	
	// Update is called once per frame
	void Update () {
        if (doorOpening || doorClosing) {

            // Modulus is necessary so we don't overshoot the length of the animation.
            int current_frame_index = ((int)((Time.time - animation_start_time) / (1.0 / fps)) % door_animation.Length);
            GetComponent<SpriteRenderer>().sprite = door_animation[current_frame_index];

            if (current_frame_index == door_animation.Length-1) {
                //end of animation
                if (doorOpening) {
                    spawnPatient = true;
                }
                doorOpening = false;
                doorClosing = false;
                //reverse animation for next call
                System.Array.Reverse(door_animation);
            }
        }

    }

    void LateUpdate() {
        if (spawnPatient) {
            spawnPatient = false;
            MainGameController.controller.SpawnPatient();
        }
    }

    public void OpenDoor() {
        if (!doorOpening && !doorClosing) {
            animation_start_time = Time.time;
            //open door
            doorOpening = true;
        }
    }

    public void OnTriggerExit(Collider col) {
        if (col.gameObject.tag == "Patient" && !doorOpening && !doorClosing) {
            //close door
            doorClosing = true;
            animation_start_time = Time.time;
        }
    }
}
