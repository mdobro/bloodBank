using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patient : MonoBehaviour {

    public StateMachine animation_state_machine;
    public Sprite[] idle_animation;
    public Sprite[] movement_animation;
    public Sprite[] bitten_sprite;
    public float remove_time;

    public SpriteRenderer rend;

    public bool ______________________;

    private AILerp pathfinder;
    public int bed_index; // used to make bed availble after bitten, assigned by MainGameController
    public bool walking = true;

    // Use this for initialization
    void Awake() {
        animation_state_machine = new StateMachine();
        animation_state_machine.ChangeState(new AI_PlayAnimation(rend, movement_animation, 6));

        pathfinder = GetComponent<AILerp>();
    }

    void FixedUpdate() {

        if (pathfinder.targetReached && walking) {
            //TODO: next to bed, sleep in it
            //stop moving
            walking = false;
            pathfinder.enabled = false; //keeping this enabled makes the character snap to the wrong position
            animation_state_machine.ChangeState(new AI_PlayAnimation(rend, idle_animation, 1));
            Vector3 pos = transform.position;
            pos.x += 1f;
            float rot_dir = 1;
            if (bed_index > 2) {
                //on the right, have to do things a bit different
                rot_dir = -1;
                rend.flipX = true;
                pos.x -= 1.5f;
                pos.y -= .5f;
            }
            transform.Rotate(new Vector3(0, 0, 90*rot_dir));
            transform.position = pos;
            //GetComponent<BoxCollider>().isTrigger = false;
        }
    }

    // Update is called once per frame
    void Update() {
        animation_state_machine.Update();
    }

    public void Bitten() {
        //this function is called when Dracula bites this patient.
        GetComponent<BoxCollider>().enabled = false;
        animation_state_machine.ChangeState(new AI_PlayAnimation(rend, bitten_sprite, 1));
        Invoke("RemoveFromBed", remove_time);
        GetComponent<AudioSource>().enabled = true;
    }

    public void SetTarget(Transform target, int bed_index) {
        pathfinder.target = target;
        this.bed_index = bed_index;
    }

    private void RemoveFromBed() {
        Destroy(this.gameObject);
        MainGameController.controller.bed_availible[bed_index] = true;
    }
}
