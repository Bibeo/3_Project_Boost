using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    Rigidbody rocketRigidbody;
    AudioSource rocketAudio;

    bool collisionActive;

    enum State { Alive, Dying, Transcending }
    State state = State.Alive;

    [SerializeField] float rcsThrust = 200f;
    [SerializeField] float pwrThrust = 1000f;
    [SerializeField] float deathDelay = 1f;

    [SerializeField] AudioClip mainEngine;
    [SerializeField] AudioClip deathSound;
    [SerializeField] AudioClip successSound;

    [SerializeField] ParticleSystem mainEngineParticles;
    [SerializeField] ParticleSystem deathParticles;
    [SerializeField] ParticleSystem successParticles;

    // Start is called before the first frame update
    void Start()
    {
        rocketRigidbody = GetComponent<Rigidbody>();
        rocketAudio = GetComponent<AudioSource>();
        collisionActive = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.Alive)
        {
            RepondToThrustInput();
            RespondToRotate();
        }
        if (Debug.isDebugBuild)
        {
            RespondToDebugKeys();
        }
    }

    private void RespondToDebugKeys()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadNextLevel();
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            collisionActive = !collisionActive; // Boolean toogle
        }
    }

    void OnCollisionEnter(Collision collision)
        {
            if (collisionActive == true)
            {
                if (state != State.Alive) // ignore collisions when dead after the first collision
                {
                    return;
                }

                switch (collision.gameObject.tag)
                {
                    case "Friendly":
                        //do nothing
                        break;
                    case "Finish":
                        StartSuccessSequence();
                        break;
                    default:
                        StartDeathSequence();
                        break;
                }
            }
        }

    private void StartSuccessSequence()
    {
        state = State.Transcending;
        rocketAudio.Stop();
        rocketAudio.PlayOneShot(successSound);
        successParticles.Play();
        Invoke("LoadNextLevel", deathDelay);
    }

    private void StartDeathSequence()
    {
        state = State.Dying;
        rocketAudio.Stop();
        rocketAudio.PlayOneShot(deathSound);
        deathParticles.Play();
        Invoke("LoadFirstLevel", deathDelay);
    }

    private void LoadNextLevel()
    {
        SceneManager.LoadScene(1);
        rocketAudio.PlayOneShot(deathSound);
    }

    private void LoadFirstLevel()
    {
        SceneManager.LoadScene(0);
    }


    private void RepondToThrustInput()
    {
        float mainThrust = pwrThrust * Time.deltaTime;
        if (Input.GetKey(KeyCode.Space))
        {
            ApplyThrust(mainThrust);
        }
        else
        {
            rocketAudio.Stop();
            mainEngineParticles.Stop();
        }
    }

    private void ApplyThrust(float mainThrust)
    {
        rocketRigidbody.AddRelativeForce(Vector3.up * mainThrust);
        if (!rocketAudio.isPlaying)
        {
            rocketAudio.PlayOneShot(mainEngine);
        }
        mainEngineParticles.Play();
    }

    private void RespondToRotate()
    {
        rocketRigidbody.freezeRotation = true; //Take manual control of the rotation
        float rotationThisFrame = rcsThrust * Time.deltaTime;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(Vector3.forward * rotationThisFrame);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(Vector3.back * rotationThisFrame);
        }
        rocketRigidbody.freezeRotation = false; //Resume physics control of rotation
    }
}
