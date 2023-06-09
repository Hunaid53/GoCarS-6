using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarControllers : MonoBehaviour
{
    public Rigidbody RB;

    public float maxSpeed;

    public float forwardAccel = 8f, reverseAccel = 4f;

    private float speedInput;


    public float turnStrength = 180f;
    private float turnInput;

    private bool grounded;

    public Transform groundRayPoint, groundRayPoint2;
    public LayerMask whatIsGround;
    public float groundRayLength = .75f;

    private float dragOnGround;
    public float gravityMod = 10f;

    public Transform leftFrontWheel, rightFrontWheel;
    public float maxWheelTurn = 25f;

    public ParticleSystem[] dustTrail;
    public float maxEmission = 25f, emissionFadeSpeed = 20f;
    private float emissionRate;

    public AudioSource engineSound, skidSound;
    public float skidFadeSpeed;

    private int nextCheckpoint;
    public int currentLap;

    public float lapTime, bestLapTime;

    // Start is called before the first frame update
    void Start()
    {
        RB.transform.parent = null;

        dragOnGround = RB.drag;   

        UIManager.instance.lapCounterText.text = currentLap + "/" + RaceManager.instance.totalLaps;

    }

    // Update is called once per frame
    void Update()
    {
        lapTime += Time.deltaTime;
        
        var ts = System.TimeSpan.FromSeconds(lapTime);
        UIManager.instance.currentLapTimeText.text = string.Format("{0:00}m{1:00}.{2:000}s", ts.Minutes, ts.Seconds, ts.Milliseconds);

        speedInput = 0f;
        if(Input.GetAxis("Vertical") > 0)
        {
            speedInput = Input.GetAxis("Vertical") * forwardAccel;
        }
        else if(Input.GetAxis("Vertical") < 0)
        {
            speedInput = Input.GetAxis("Vertical") * reverseAccel;
        }

        turnInput = Input.GetAxis("Horizontal");

        /*if(grounded && Input.GetAxis("Vertical") != 0)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * turnStrength *Time.deltaTime * Mathf.Sign(speedInput)  * (RB.velocity.magnitude / maxSpeed), 0f));
        }   */    


        //turning the wheels
        leftFrontWheel.localRotation =  Quaternion.Euler(leftFrontWheel.localRotation.x, (turnInput * maxWheelTurn) - 180,leftFrontWheel.localRotation.z);
        rightFrontWheel.localRotation =  Quaternion.Euler(rightFrontWheel.localRotation.x, (turnInput * maxWheelTurn) ,rightFrontWheel.localRotation.z);

        //transform.position = RB.position;

        //control particle emission
        emissionRate = Mathf.MoveTowards(emissionRate, 0f, emissionFadeSpeed * Time.deltaTime);

        if(grounded && (Mathf.Abs (turnInput) > .5f || (RB.velocity.magnitude < maxSpeed * .5f && RB.velocity.magnitude != 0)))
        {
            emissionRate = maxEmission;
        }

        if(RB.velocity.magnitude <= .5f)
        {
            emissionRate = 0;
        }

        for(int i = 0; i < dustTrail.Length; i++)
        {
            var emissionModule = dustTrail[i].emission;

            emissionModule.rateOverTime = emissionRate;
        }

        if(engineSound != null)
        {
         engineSound.pitch = 1f + ((RB.velocity.magnitude / maxSpeed) * 2f );   
        }

        if(skidSound != null)
        {
            if(Mathf.Abs(turnInput) > 0.5f)
            {
                skidSound.volume = 1f;
            }
            else
            {
                skidSound.volume = Mathf.MoveTowards(skidSound.volume, 0f, skidFadeSpeed * Time.deltaTime);
            }
        }
    }

    private void FixedUpdate()
    {

        grounded = false;

        RaycastHit hit;
        Vector3 normalTarget = Vector3.zero;

        if(Physics.Raycast(groundRayPoint.position, -transform.up, out hit, groundRayLength, whatIsGround))
        {
            grounded = true;

            normalTarget = hit.normal;

        }

        if(Physics.Raycast(groundRayPoint2.position, -transform.up, out hit, groundRayLength, whatIsGround))
        {
            grounded = true;

            normalTarget = (normalTarget + hit.normal / 2f);
        }

        //when on ground rotates to match the normal
        if(grounded)
        {
            transform.rotation = Quaternion.FromToRotation(transform.up, normalTarget) * transform.rotation;
        }

        //accelarates the car
        if(grounded)
        {
            RB.drag = dragOnGround;

            RB.AddForce(transform.forward * speedInput * 1000f);
        }else
        {
            RB.drag = .1f;

            RB.AddForce(-Vector3.up * gravityMod * 100f);
        }


        if(RB.velocity.magnitude > maxSpeed)
        {
            RB.velocity = RB.velocity.normalized * maxSpeed;
        }

        //Debug.Log(RB.velocity.magnitude);

        transform.position = RB.position;

        if(grounded && Input.GetAxis("Vertical") != 0)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * turnStrength *Time.deltaTime * Mathf.Sign(speedInput)  * (RB.velocity.magnitude / maxSpeed), 0f));
        }  
    }
    public void CheckpointHit(int cpNumber)
    {
        if(cpNumber == nextCheckpoint)
        {
            nextCheckpoint++;

            if(nextCheckpoint == RaceManager.instance.allCheckpoints.Length)
            {
                nextCheckpoint = 0;
                LapCompleted();
            }
        }
    }
    public void LapCompleted()
    {
        currentLap++;

        if(lapTime < bestLapTime || bestLapTime == 0)
        {
            bestLapTime = lapTime;
        }

        lapTime = 0f;

        var ts = System.TimeSpan.FromSeconds(bestLapTime);
        UIManager.instance.bestLapTimeText.text = string.Format("{0:00}m{1:00}.{2:000}s", ts.Minutes, ts.Seconds, ts.Milliseconds);

        UIManager.instance.lapCounterText.text = currentLap + "/" + RaceManager.instance.totalLaps;
    }
}
