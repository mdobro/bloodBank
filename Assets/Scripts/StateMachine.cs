using UnityEngine;
using UnityEngine.UI;

// State Machines are responsible for processing states, notifying them when they're about to begin or conclude, etc.
public class StateMachine {
    private State _current_state;
    
    public void ChangeState(State new_state) {
        if (_current_state != null) {
            _current_state.OnFinish();
        }

        _current_state = new_state;
        // States sometimes need to reset their machine. 
        // This reference makes that possible.
        _current_state.state_machine = this;
        _current_state.OnStart();
    }

    public void Reset() {
        if (_current_state != null)
            _current_state.OnFinish();
        _current_state = null;
    }

    public void Update() {
        if (_current_state != null) {
            float time_delta_fraction = Time.deltaTime / (1.0f / Application.targetFrameRate);
            _current_state.OnUpdate(time_delta_fraction);
        }
    }

    public bool IsFinished() {
        return _current_state == null;
    }
}

// A State is merely a bundle of behavior listening to specific events, such as...
// OnUpdate -- Fired every frame of the game.
// OnStart -- Fired once when the state is transitioned to.
// OnFinish -- Fired as the state concludes.
// State Constructors often store data that will be used during the execution of the State.
public class State {
    // A reference to the State Machine processing the state.
    public StateMachine state_machine;

    public virtual void OnStart() { }
    public virtual void OnUpdate(float time_delta_fraction) { } // time_delta_fraction is a float near 1.0 indicating how much more / less time this frame took than expected.
    public virtual void OnFinish() { }

    // States may call ConcludeState on themselves to end their processing.
    public void ConcludeState() { state_machine.Reset(); }
}

// A State that takes a renderer and a sprite, and implements idling behavior.
// The state is capable of transitioning to a walking state upon key press.
public class StateIdleWithSprite : State {
    Vampire vampire;
    SpriteRenderer renderer;
    Sprite sprite;

    public StateIdleWithSprite(Vampire vampire, SpriteRenderer renderer, Sprite sprite) {
        this.vampire = vampire;
        this.renderer = renderer;
        this.sprite = sprite;
    }

    public override void OnStart() {
        renderer.sprite = sprite;
    }

    public override void OnUpdate(float time_delta_fraction) {
        // Transition to walking animations on key press.
        if (Input.GetKeyDown(KeyCode.DownArrow))
            state_machine.ChangeState(new StatePlayAnimationForHeldKey(vampire, renderer, vampire.move, 6, KeyCode.DownArrow));
        if (Input.GetKeyDown(KeyCode.UpArrow))
            state_machine.ChangeState(new StatePlayAnimationForHeldKey(vampire, renderer, vampire.move, 6, KeyCode.UpArrow));
        if (Input.GetKeyDown(KeyCode.RightArrow))
            state_machine.ChangeState(new StatePlayAnimationForHeldKey(vampire, renderer, vampire.move, 6, KeyCode.RightArrow));
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            state_machine.ChangeState(new StatePlayAnimationForHeldKey(vampire, renderer, vampire.move, 6, KeyCode.LeftArrow));
    }
}

// A State for playing an animation until a particular key is released.
// Good for animations such as walking.
public class StatePlayAnimationForHeldKey : State {
    Vampire vampire;
    SpriteRenderer renderer;
    KeyCode key;
    Sprite[] animation;
    int animation_length;
    float animation_progression;
    float animation_start_time;
    int fps;

    public StatePlayAnimationForHeldKey(Vampire vampire, SpriteRenderer renderer, Sprite[] animation, int fps, KeyCode key) {
        this.vampire = vampire;
        this.renderer = renderer;
        this.key = key;
        this.animation = animation;
        this.animation_length = animation.Length;
        this.fps = fps;

    }

    public override void OnStart() {
        animation_start_time = Time.time;
    }

    public override void OnUpdate(float time_delta_fraction) {

        if (this.animation_length <= 0) {
            return;
        }

        // Modulus is necessary so we don't overshoot the length of the animation.
        int current_frame_index = ((int)((Time.time - animation_start_time) / (1.0 / fps)) % animation_length);
        renderer.sprite = animation[current_frame_index];

        //IMPLIMENT IF NEEDED!!!!
        // If another key is pressed and Link is not paused, we need to transition to a different walking animation.
        //if (Input.GetKeyDown(KeyCode.DownArrow))
        //    state_machine.ChangeState(new StatePlayAnimationForHeldKey(vampire, renderer, vampire.link_run_down, 6, KeyCode.DownArrow));
        //else if (Input.GetKeyDown(KeyCode.UpArrow))
        //   state_machine.ChangeState(new StatePlayAnimationForHeldKey(vampire, renderer, vampire.link_run_up, 6, KeyCode.UpArrow));
        //else if (Input.GetKeyDown(KeyCode.RightArrow))
        //    state_machine.ChangeState(new StatePlayAnimationForHeldKey(vampire, renderer, vampire.link_run_right, 6, KeyCode.RightArrow));
        //else if (Input.GetKeyDown(KeyCode.LeftArrow))
        //    state_machine.ChangeState(new StatePlayAnimationForHeldKey(vampire, renderer, vampire.link_run_left, 6, KeyCode.LeftArrow));

        // If we detect the specified key has been released, return to the idle state.
        if (!Input.GetKey(key))
            state_machine.ChangeState(new StateIdleWithSprite(vampire, renderer, animation[0]));
    }
}

public class StateVampireNormalMovement : State {
    Vampire vampire;

    public StateVampireNormalMovement(Vampire vampire) {
        this.vampire = vampire;
    }

    public override void OnUpdate(float time_delta_fraction) {
        float horizontal_input = Input.GetAxis("Horizontal");
        float vertical_input = Input.GetAxis("Vertical");

        vampire.GetComponent<Rigidbody>().velocity = new Vector3(horizontal_input, vertical_input, 0) * -vampire.walking_velocity * time_delta_fraction;

        //only allow abilites when not morphing
        if (!vampire.isBat) {
            //ability / actions
            if (Input.GetKeyDown(KeyCode.A)) {
                //Bat
                if (vampire.batPoints > 0) {
                    vampire.animation_state_machine.ChangeState(new StateVampireMetamorphosis(vampire, vampire.GetComponent<SpriteRenderer>(), vampire.toBat, 10, !vampire.isBat));
                    vampire.isBat = !vampire.isBat;
                }
            }
            if (Input.GetKeyDown(KeyCode.S)) {
                //children of the night 
                if (vampire._wolfCooldownTimer >= vampire.wolfCooldown) {
                    vampire.GetComponent<AudioSource>().Play();
                    vampire._wolfCooldownTimer = 0;
                    GameObject.Find("Hunter").GetComponent<Hunter>().RunToCorner();
                    GameObject.Find("Hunter_1").GetComponent<Hunter>().RunToCorner();
                }
            }
            if (Input.GetKeyDown(KeyCode.D)) {
                //Suck blood
                vampire.animation_state_machine.ChangeState(new StateVampireAttack(vampire, vampire.GetComponent<SpriteRenderer>(), vampire.attack, 10));
                vampire.Attack();
            }
        }
    }
}

public class StateVampireMetamorphosis : State {
    Vampire vampire;
    SpriteRenderer renderer;
    Sprite[] animation;
    int animation_length;
    float animation_progression;
    float animation_start_time;
    int fps;
    bool toBat;

    public StateVampireMetamorphosis(Vampire vampire, SpriteRenderer renderer, Sprite[] animation, int fps, bool toBat) {
        this.vampire = vampire;
        this.renderer = renderer;
        this.animation = animation;
        this.animation_length = animation.Length;
        this.fps = fps;
        this.toBat = toBat;

    }

    public override void OnStart() {
        base.OnStart();
        animation_start_time = Time.time;

        if (!toBat) {
            System.Array.Reverse(animation);
        }
    }

    public override void OnUpdate(float time_delta_fraction) {
        base.OnUpdate(time_delta_fraction);
        if (this.animation_length <= 0) {
            return;
        }

        // Modulus is necessary so we don't overshoot the length of the animation.
        int current_frame_index = ((int)((Time.time - animation_start_time) / (1.0 / fps)) % animation_length);
        renderer.sprite = animation[current_frame_index];

        if (current_frame_index == animation_length-1) {
            if (toBat) {
                //end state on bat state
                vampire.animation_state_machine.ChangeState(new StateBatAnimation(vampire, renderer, vampire.bat, 10));
                vampire.movement_state_machine.ChangeState(new StateBatMovement(vampire));
            }
            else {
                //end state on normal movement state
                vampire.animation_state_machine.ChangeState(new StateIdleWithSprite(vampire, renderer, vampire.move[0]));
                vampire.movement_state_machine.ChangeState(new StateVampireNormalMovement(vampire));
                //reverse the array again for the next pass into bat state
                System.Array.Reverse(animation);
            }
        }
    }
}

public class StateBatMovement : State {
    Vampire vampire;

    public StateBatMovement(Vampire vampire) {
        this.vampire = vampire;
    }

    public override void OnUpdate(float time_delta_fraction) {
        float horizontal_input = Input.GetAxis("Horizontal");
        float vertical_input = Input.GetAxis("Vertical");

        vampire.GetComponent<Rigidbody>().velocity = new Vector3(horizontal_input, vertical_input, 0) * -vampire.flying_velocity * time_delta_fraction;

        //ability / actions
        if (Input.GetKeyDown(KeyCode.A)) {
            //Bat
            if (vampire.isBat) {
                vampire.animation_state_machine.ChangeState(new StateVampireMetamorphosis(vampire, vampire.GetComponent<SpriteRenderer>(), vampire.toBat, 10, !vampire.isBat));
                vampire.isBat = !vampire.isBat;
            }
        }
    }
}

public class StateBatAnimation : State {
    Vampire vampire;
    SpriteRenderer renderer;
    Sprite[] animation;
    int animation_length;
    float animation_progression;
    float animation_start_time;
    int fps;

    public StateBatAnimation(Vampire vampire, SpriteRenderer renderer, Sprite[] animation, int fps) {
        this.vampire = vampire;
        this.renderer = renderer;
        this.animation = animation;
        this.animation_length = animation.Length;
        this.fps = fps;

    }

    public override void OnStart() {
        base.OnStart();
        animation_start_time = Time.time;
    }

    public override void OnUpdate(float time_delta_fraction) {
        base.OnUpdate(time_delta_fraction);
        if (this.animation_length <= 0) {
            return;
        }

        // Modulus is necessary so we don't overshoot the length of the animation.
        int current_frame_index = ((int)((Time.time - animation_start_time) / (1.0 / fps)) % animation_length);
        renderer.sprite = animation[current_frame_index];
    }
}

public class StateVampireAttack : State {

    Vampire vampire;
    SpriteRenderer renderer;
    Sprite[] animation;
    int animation_length;
    float animation_progression;
    float animation_start_time;
    int fps;

    public StateVampireAttack(Vampire vampire, SpriteRenderer renderer, Sprite[] animation, int fps) {
        this.vampire = vampire;
        this.renderer = renderer;
        this.animation = animation;
        this.animation_length = animation.Length;
        this.fps = fps;
    }

    public override void OnStart() {
        base.OnStart();
        animation_start_time = Time.time;
    }

    public override void OnUpdate(float time_delta_fraction) {
        base.OnUpdate(time_delta_fraction);
        if (this.animation_length <= 0) {
            return;
        }

        // Modulus is necessary so we don't overshoot the length of the animation.
        int current_frame_index = ((int)((Time.time - animation_start_time) / (1.0 / fps)) % animation_length);
        renderer.sprite = animation[current_frame_index];

        if (current_frame_index == animation_length-1) {
            //last frame, go back to idle
            vampire.animation_state_machine.ChangeState(new StateIdleWithSprite(vampire, renderer, vampire.move[0]));
        }
    }
}

public class StateVampireDamaged : State {

    Vampire vampire;
    Vector3 contactPoint;
    Vector3 linkCenter;
    float vel = 10;
    float aniTime = 0.2f;

    public StateVampireDamaged(Vampire vampire, Vector3 contactPoint) {
        this.vampire = vampire;
        this.linkCenter = vampire.GetComponent<Collider>().bounds.center;
        this.contactPoint = contactPoint;
    }

    public override void OnStart() {
        
    }

    public override void OnUpdate(float time_delta_fraction) {
        aniTime -= Time.fixedDeltaTime;
        if (aniTime < 0) {
            ConcludeState();
        }
    }

    public override void OnFinish() {

    }
}

public class StateVampireDeath : State {

    Vampire vampire;
    SpriteRenderer rend;
    Sprite[] animation;
    int animation_length;
    float animation_progression;
    float animation_start_time;
    int fps;

    public StateVampireDeath(Vampire vampire, Sprite[] animation, int fps) {
        this.vampire = vampire;
        this.rend = vampire.GetComponent<SpriteRenderer>();
        this.animation = animation;
        this.animation_length = animation.Length;
        this.fps = fps;

    }

    public override void OnStart() {
        base.OnStart();
        this.animation_start_time = Time.time;
        vampire.GetComponent<Rigidbody>().velocity = Vector3.zero;

    }

    public override void OnUpdate(float time_delta_fraction) {
        if (this.animation_length <= 0) {
            return;
        }

        // Modulus is necessary so we don't overshoot the length of the animation.
        int current_frame_index = ((int)((Time.time - animation_start_time) / (1.0 / fps)) % animation_length);
        rend.sprite = animation[current_frame_index];

        if (current_frame_index == animation.Length-1) {
            MonoBehaviour.Destroy(vampire.gameObject);
            //show game over / restart
            MainGameController.controller.ShowRestartPanel();
        }
    }
}

public class StateVampireStunned : State {
    //mainly used to keep playing from moving during death : empty state
    public StateVampireStunned() {

    }
}

// Additional recommended states:
// StateDeath
// StateDamaged
// StateWeaponSwing
// StateVictory

// Additional control states:
// LinkNormalMovement.
// LinkStunnedState.

public class StatePlayMovementAnimation : State {
    SpriteRenderer renderer;
    Sprite[] animation;
    int animation_length;
    float animation_progression;
    float animation_start_time;
    int fps;

    public StatePlayMovementAnimation(SpriteRenderer renderer, Sprite[] animation, int fps) {
        this.renderer = renderer;
        this.animation = animation;
        this.animation_length = animation.Length;
        this.fps = fps;

    }

    public override void OnStart() {
        animation_start_time = Time.time;
    }

    public override void OnUpdate(float time_delta_fraction) {
        if (this.animation_length <= 0) {
            return;
        }

        // Modulus is necessary so we don't overshoot the length of the animation.
        int current_frame_index = ((int)((Time.time - animation_start_time) / (1.0 / fps)) % animation_length);
        renderer.sprite = animation[current_frame_index];
    }
}

public class AI_PlayAnimation : State {
    SpriteRenderer renderer;
    Sprite[] animation;
    int animation_length;
    float animation_progression;
    float animation_start_time;
    int fps;

    public AI_PlayAnimation(SpriteRenderer renderer, Sprite[] animation, int fps) {
        this.renderer = renderer;
        this.animation = animation;
        this.animation_length = animation.Length;
        this.fps = fps;

    }

    public override void OnStart() {
        base.OnStart();
        animation_start_time = Time.time;
    }

    public override void OnUpdate(float time_delta_fraction) {
        base.OnUpdate(time_delta_fraction);
        if (this.animation_length <= 0) {
            return;
        }

        // Modulus is necessary so we don't overshoot the length of the animation.
        int current_frame_index = ((int)((Time.time - animation_start_time) / (1.0 / fps)) % animation_length);
        renderer.sprite = animation[current_frame_index];
    }
}

public class TitleAnimationState : State {
    Image renderer;
    Sprite[] animation;
    int animation_length;
    float animation_progression;
    float animation_start_time;
    int fps;

    public TitleAnimationState(Image renderer, Sprite[] animation, int fps) {
        this.renderer = renderer;
        this.animation = animation;
        this.animation_length = animation.Length;
        this.fps = fps;

    }

    public override void OnStart() {
        base.OnStart();
        animation_start_time = Time.time;
    }

    public override void OnUpdate(float time_delta_fraction) {
        base.OnUpdate(time_delta_fraction);
        if (this.animation_length <= 0) {
            return;
        }

        // Modulus is necessary so we don't overshoot the length of the animation.
        int current_frame_index = ((int)((Time.time - animation_start_time) / (1.0 / fps)) % animation_length);
        renderer.sprite = animation[current_frame_index];
    }
}