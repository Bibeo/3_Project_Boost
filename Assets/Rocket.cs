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

    bool isTransitionning = false;

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
        if (!isTransitionning)
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
        if (Input.GetKeyDown(KeyCode.N))
        {
            LoadNextLevel();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            LoadPreviousLevel();
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
                if (isTransitionning) // ignore collisions when dead after the first collision
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
        isTransitionning = true;
        rocketAudio.Stop();
        rocketAudio.PlayOneShot(successSound);
        successParticles.Play();
        Invoke("LoadNextLevel", deathDelay);
    }

    private void StartDeathSequence()
    {
        isTransitionning = true;
        rocketAudio.Stop();
        rocketAudio.PlayOneShot(deathSound);
        deathParticles.Play();
        Invoke("LoadFirstLevel", deathDelay);
    }

    private void LoadPreviousLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (currentSceneIndex > 0)
        {
            int previousSceneIndex = currentSceneIndex - 1;
            SceneManager.LoadScene(previousSceneIndex);
        }
    }

    private void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;
        if (nextSceneIndex == SceneManager.sceneCountInBuildSettings)
        {
            nextSceneIndex = 0;
        }
            SceneManager.LoadScene(nextSceneIndex);
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
            StopApplyingThrust();
        }
    }

    private void StopApplyingThrust()
    {
        rocketAudio.Stop();
        mainEngineParticles.Stop();
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
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            RotateManually(rcsThrust * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            RotateManually(-rcsThrust * Time.deltaTime);
        } 
    }

    private void RotateManually(float rotationThisFrame)
    {
        rocketRigidbody.freezeRotation = true; //Take manual control of the rotation
        transform.Rotate(Vector3.forward * rotationThisFrame);
        rocketRigidbody.freezeRotation = false; //Resume physics control of rotation
    }
}
