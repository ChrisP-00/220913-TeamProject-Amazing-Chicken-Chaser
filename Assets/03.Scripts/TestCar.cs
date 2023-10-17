using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class TestCar : MonoBehaviour
{
    [Header("Car Info")]
    [SerializeField] public float Acceleration = 1000f;       // 자동차 속도
    [SerializeField] public float BrakingForce = 1000f;      // 브레이크 
    [SerializeField] public float MaxTurnAngle = 45f;        // 회전 각
    [SerializeField] public float MaxSpeed = 100f;
    public float stiffness;


    public float MyextremumSlip;
    public float MyextremumValue;
    public float MyasymptoteSlip;
    public float MYasymptoteValue;



    [Header("Wheel Info")]
    [SerializeField] GameObject[] wheelMeshes = new GameObject[4];          // wheel mesh
    [SerializeField] WheelCollider[] wheelColliders = new WheelCollider[4]; // wheel collider
    [SerializeField] TrailRenderer[] trailrenderers = new TrailRenderer[4]; // wheel trailrenderer



    [SerializeField] TextMeshProUGUI speed;

    int CarSpeed;

    float xAxis;
    float zAxis;

    // 플레이어 Rigidbody 
    Rigidbody playerRigid;
    // 플레이어 무게 중심 
    public GameObject mycg;

    // 현재 속도
    float currSpeed;

    //플레이어 sideslipe 
    WheelFrictionCurve myFriction = new WheelFrictionCurve();

    //============================================================
    #region 아이템 관련 변수
    [SerializeField] public bool shield = false;
    [SerializeField] public bool booster = false;
    [SerializeField] GameObject PlayerSlot;
    [SerializeField] GameObject Slot1;
    [SerializeField] GameObject Slot2;
    [SerializeField] int[] Slot = new int[2];

    #endregion
    //============================================================


    //============================================================

    private void Awake()
    {

        playerRigid = GetComponent<Rigidbody>();

    }





    //==============================================      UPDATE      ===========================================
    private void Update()
    {

        // speed 
        speed.text = $"{CarSpeed} km/h";


        //input keys 
        xAxis = Input.GetAxis("Horizontal");
        zAxis = Input.GetAxis("Vertical");


        CarSpeed = (int)(playerRigid.velocity.magnitude * 3.6f);


        // Apply brakes
        if (Input.GetKey(KeyCode.Space))
        {
            // Rear Wheel Brake
            wheelColliders[2].brakeTorque = BrakingForce;
            wheelColliders[3].brakeTorque = BrakingForce;
            //start making skidmarks
            SkidMark(0, true);
        }

        // release brakes 
        if (Input.GetKeyUp(KeyCode.Space))
        {
            wheelColliders[0].brakeTorque = 0f;
            wheelColliders[1].brakeTorque = 0f;
            wheelColliders[2].brakeTorque = 0f;
            wheelColliders[3].brakeTorque = 0f;
            //start making skidmarks
            SkidMark(0, false);

        }

        // drift key "shift"
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Drift(stiffness);        // decrease wheel stiffness for drifting
            SkidMark(2, true);  // skidmark on

        }

        // drift key "shift"
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            Drift(5f);          // increase wheel stiffness to stop drifting
            SkidMark(2, false); // skidmark off
        }

        // if wheel is off ground, no skidmark
        EraseSkidMark();





    }



    private void FixedUpdate()
    {

        // move C.G of vehicle
        playerRigid.centerOfMass = mycg.transform.localPosition;



        // currentspeed of car 
        currSpeed = (float)(playerRigid.velocity.magnitude * 3.6f);

        //if has horizontal key input, Steer wheels 
        if (xAxis != 0f)
        {
            // 전방 휠 움직임 
            wheelColliders[0].steerAngle = xAxis * MaxTurnAngle;    // front left wheel 
            wheelColliders[1].steerAngle = xAxis * MaxTurnAngle;    // front right wheel
        }

        else
        {
            // 전방 휠 움직임 
            wheelColliders[0].steerAngle = 0f;    // front left wheel 
            wheelColliders[1].steerAngle = 0f;    // front right wheel
        }

        if (zAxis != 0f && currSpeed <= MaxSpeed)
        {
            // 토크를 주는 휠 
            // FWD 
            wheelColliders[0].motorTorque = zAxis * Acceleration;
            wheelColliders[1].motorTorque = zAxis * Acceleration;

            // RWD
            wheelColliders[2].motorTorque = zAxis * Acceleration;
            wheelColliders[3].motorTorque = zAxis * Acceleration;
        }


        //if no input, set all the motor torque to zero 
        else
        {
            wheelColliders[0].motorTorque = 0;
            wheelColliders[1].motorTorque = 0;
            wheelColliders[2].motorTorque = 0;
            wheelColliders[3].motorTorque = 0;
        }


        //set all the wheelmash position & rotation as same as wheelCollider
        for (int i = 0; i < wheelColliders.Length; i++)
        {
            // wheel collider에 맞춰 wheel mesh를 움직일 수 있도록 함 
            UpdateWheelPos(wheelColliders[i], wheelMeshes[i].transform);
        }

    }




    // to move wheelmash as wheelCollider moves  
    void UpdateWheelPos(WheelCollider collider, Transform transform)
    {
        Vector3 position;
        Quaternion rotation;

        // GetWorldPos is the function of wheelcollider, you can get position and rotation of the wheel collider 
        collider.GetWorldPose(out position, out rotation);

        //transform wheel mash position and rotation as same as wheel collider 
        transform.position = position;
        transform.rotation = rotation;
    }

    void Drift(float stiffness)
    {
        // change only rear wheels 
        for (int i = 2; i < 4; i++)
        {
            // increase sideways slip value for drifting
            myFriction.extremumSlip = wheelColliders[i].sidewaysFriction.extremumSlip;
            myFriction.extremumValue = wheelColliders[i].sidewaysFriction.extremumValue;
            myFriction.asymptoteSlip = wheelColliders[i].sidewaysFriction.asymptoteSlip;
            myFriction.asymptoteValue = wheelColliders[i].sidewaysFriction.asymptoteValue;

            //change stiffness Value 
            myFriction.stiffness = stiffness;

            wheelColliders[i].sidewaysFriction = myFriction;

        }
    }


    void EndDrift()
    {
        myFriction.extremumSlip = 0.4f;
        myFriction.extremumValue = 1f;
        myFriction.asymptoteSlip = 0.8f;
        myFriction.asymptoteValue = 0.75f;


    }


    // which wheel to be emitted, and how many wheels? 
    void SkidMark(int wheel, bool emitting)
    {

        // set trailrenderer emitting
        for (int i = wheel; i < trailrenderers.Length; i++)
        {
            if (wheelColliders[i].isGrounded)
                trailrenderers[i].emitting = emitting;      // selected wheel only
        }
    }

    // if wheel is not grounded, turn trailrenderer emitting off
    void EraseSkidMark()
    {

        for (int i = 0; i < trailrenderers.Length; i++)
        {
            if (!wheelColliders[i].isGrounded)
                trailrenderers[i].emitting = false;
        }
    }



    public void GetItem(int num) //아이템 획득시
    {
        if ((Slot[0] == 0 && Slot[1] != 0) || (Slot[0] == 0 && Slot[1] == 0)) // 첫번째만 비었거나 둘 다 비어있으면 첫번째부터 넣는다.
        {
           
            Slot[0] = num;
            Slot1.transform.GetChild(num).gameObject.SetActive(true);
        }
        else if (Slot[0] != 0 && Slot[1] == 0) // 두번째 칸이 비어있다면
        {
           
            Slot[1] = num;
            Slot2.transform.GetChild(num).gameObject.SetActive(true);
        }
        else
        {
            return; // 둘 다 차있는 경우
        }
    }

    public void GetKeyDownControl(bool ctrl) //아이템 사용키 눌렀을 때 함수
    {
        if (ctrl) //Ctrl 키 눌렀을 때
        {
            if (Slot[0] != 0)
                UseItem(Slot[0], 0);
        }

        else //Alt 키 눌렀을 때
        {
            if (Slot[1] != 0)
                UseItem(Slot[1], 1);
        }
    }

    void UseItem(int num, int i) //사용되는 아이템 정보
    {
        Slot[i] = 0; //사용한 슬롯은 일단 비운다
        PlayerSlot.transform.GetChild(i).transform.GetChild(num).gameObject.SetActive(false);
        switch (num)
        {
            case 1: //부스터
               
     
                return;
            case 2: //미사일
              
                GameObject myMissile = PhotonNetwork.Instantiate("Missile", transform.position + new Vector3(0f, 0.4f, 0f), transform.rotation);
                myMissile.AddComponent<Missile>();
                myMissile.transform.position = transform.position + new Vector3(0, 0.4f, 0f);
                myMissile.transform.rotation = Quaternion.LookRotation(transform.forward);
                return;
            case 3: //방어막
              
             
                return;
            case 4: //바나나
            
                GameObject myBanana = PhotonNetwork.Instantiate("Banana", transform.position + (-transform.forward * 5f), Quaternion.Euler(90f, 0f, 0f));
                myBanana.AddComponent<Banana>();
                return;
            case 5: //안개
               
                PhotonNetwork.Instantiate("Smoke", transform.position, Quaternion.Euler(0f, 0f, 0f));
                return;
            case 6: //얼음탄
              
                GameObject myFreeze = PhotonNetwork.Instantiate("Freeze", transform.position + new Vector3(0f, 0.4f, 0f), transform.rotation * Quaternion.Euler(-50f, 0f, 0f));
                myFreeze.AddComponent<Freeze>();
                myFreeze.transform.position = transform.position + new Vector3(0, 0.4f, 0f);
                myFreeze.transform.rotation = Quaternion.LookRotation(transform.forward);
                return;
        }
    }
}




