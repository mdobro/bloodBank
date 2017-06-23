using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hunter : MonoBehaviour {

    public StateMachine animation_state_machine;
    public Sprite[] idle_animation;
    public Sprite[] movement_animation;
    public Sprite[] stake_animation;
    public Sprite[] hide_animation;

    public Transform[] targets;
    public GameObject exclamation;
    public SpriteRenderer rend;

    public float detectionRange;
    public float idleTimer;
    public float hideTime;
    public float speed;


    public bool ______________________;

    private AILerp pathfinder;
    private bool chasingDracula;
    private float _idleTimer;
    private float _cooldownTimer;
    public bool hiding = false;

	// Use this for initialization
	void Start () {
        animation_state_machine = new StateMachine();
        animation_state_machine.ChangeState(new AI_PlayAnimation(rend, movement_animation, 10));

        pathfinder = GetComponent<AILerp>();
        chasingDracula = false;
        _idleTimer = idleTimer;
        //set random point to travel to
        pathfinder.target = targets[Random.Range(0, targets.Length)];
    }

    void OnDrawGizmosSelected() {
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    void FixedUpdate() {

        _cooldownTimer += Time.fixedDeltaTime;

        if (hiding) {
            if (pathfinder.targetReached) {
                animation_state_machine.ChangeState(new AI_PlayAnimation(rend, hide_animation, 10));    
            }
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, 1 << 9);
            if (hits.Length > 0) {
                //GET THAT VAMPIRE
                pathfinder.target = hits[0].gameObject.transform;
                chasingDracula = true;
                if (_idleTimer != idleTimer) {
                    //coming from idle, change to movement
                    animation_state_machine.ChangeState(new AI_PlayAnimation(rend, movement_animation, 10));
                    //reset idle
                    _idleTimer = idleTimer;
                }
            }
            else if (chasingDracula) {
                //Dracula has gone outside the radius to chase him, go to a random point
                pathfinder.target = targets[Random.Range(0, targets.Length)];
                chasingDracula = false;
            }
            else if (pathfinder.targetReached && _cooldownTimer > 1) {
                //idle for some time
                if (_idleTimer == idleTimer) {
                    //idle is just beginning, switch states
                    animation_state_machine.ChangeState(new AI_PlayAnimation(rend, idle_animation, 10));
                }
                _idleTimer -= Time.fixedDeltaTime;

                if (_idleTimer <= 0) {
                    // randomly pick a new point to travel to
                    pathfinder.target = targets[Random.Range(0, targets.Length)];
                    _idleTimer = idleTimer;
                    animation_state_machine.ChangeState(new AI_PlayAnimation(rend, movement_animation, 10));
                    _cooldownTimer = 0;
                }
            }

            exclamation.SetActive(chasingDracula);
    }
	
	// Update is called once per frame
	void Update () {
        animation_state_machine.Update();
	}
    
    void OnTriggerEnter(Collider col) {
        if (!hiding) {
            if (col.gameObject.tag == "Dracula") {
                animation_state_machine.ChangeState(new AI_PlayAnimation(rend, stake_animation, 10));
            }
        }
    }

    void OnTriggerExit(Collider col) {
        if (!hiding) {
            if (col.gameObject.tag == "Dracula") {
                animation_state_machine.ChangeState(new AI_PlayAnimation(rend, movement_animation, 10));
                pathfinder.target = targets[Random.Range(0, targets.Length)];
            }
        }
    }
    
    //go to corners of map, hard coded because I'm lazy
    public void RunToCorner() {
        if (gameObject.name == "Hunter") {
            pathfinder.target = targets[0];
        } else {
            pathfinder.target = targets[5];
        }
        hiding = true;
        animation_state_machine.ChangeState(new AI_PlayAnimation(rend, movement_animation, 15));
        exclamation.SetActive(false);
        pathfinder.speed = speed+3;
        Invoke("ResumeNormalMovement", hideTime);
    }
    
    public void ResumeNormalMovement() {
        hiding = false;
        pathfinder.speed = speed;
    }  
}
