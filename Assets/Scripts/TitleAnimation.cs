using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleAnimation : MonoBehaviour {

    StateMachine animationMachine;
    public int animation_speed;
    public Sprite[] animation_sprites;

    // Use this for initialization
    void Start() {
        animationMachine = new StateMachine();
        animationMachine.ChangeState(new TitleAnimationState(GetComponent<Image>(), animation_sprites, animation_speed));
    }

    // Update is called once per frame
    void Update() {
        animationMachine.Update();

    }
}
