using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;           // TextMeshProUGUI ����� ���� using
using UnityEngine.UI;  // UI ����� ���� using
using Photon.Pun;      // ���� ���̺귯���� ����Ƽ ������Ʈ�� ����� �� �ְ� �ϴ� ���̺귯��  
using Photon.Realtime; // ������ �ǽð� ��Ʈ��ũ ���� ���߿� C# ���̺귯��
using Hashtable = ExitGames.Client.Photon.Hashtable;
// ExitGames �� ������ ���� ȸ���ε� ���⿡�� ���� Hashtable�� ����Ϸ��µ�
// ����Ƽ���� �⺻������ �����ϴ� Hashtable�� �ƴ� ���濡�� �����ϴ� Hashtable�� ����Ѵ�

public class UIManager : MonoBehaviourPunCallbacks
{
    #region UI������
    [SerializeField] TextMeshProUGUI Text_ConnectionInfo = null;  // ���� ��Ʈ��ũ ���� �޼����� ��Ÿ�� TextMeshPro
    [SerializeField] GameObject      Panel_Notice        = null;  // ���� �˸� ���¸� ��� �г�

    [Header("** �α��� UI **")]
    [SerializeField] Button          Button_JoinLobby    = null;  // �κ� ���� ��ư
    [SerializeField] TextMeshProUGUI Text_UserName       = null;  // ���� �̸�
    [SerializeField] GameObject      Panel_WaitConnect   = null;  // ���ӽ� �ε�ȭ��ó�� ��� �г�

    [Header("** �κ� UI **")]
    [SerializeField] GameObject      Panel_Login             = null;  // �α��� �г�
    [SerializeField] GameObject      Panel_Lobby             = null;  // �κ� �г�
    [SerializeField] GameObject      Panel_CreateRoom        = null;  // ���� �����ϴµ� ���� �г�
    [SerializeField] Button          Button_CreateRoomPanel  = null;  // ����� �ǳ��� ����� ��ư
    [SerializeField] Transform       Tr_Content_Room         = null;  // �� ������ ��ũ�Ѻ信 �־��� ���� ��ġ(Vertical Layout Group ������� �˸°� ���� �Ұ���)
    [SerializeField] Button          Button_CreateRoom       = null;  // ���� �����ϴ� ��ư
    [SerializeField] GameObject      room                    = null;  // �������� ���� ������� �� ������
    [SerializeField] Toggle[]        togglesForMaxPlayer     = null;  // �ִ� �÷��̾ ������ ��۵�
    [SerializeField] TextMeshProUGUI Text_MyZera             = null;  // ���� �ڽ�Ʈ

    [Header("** �� UI **")]
    [SerializeField] TextMeshProUGUI   Text_roomName           = null;    // �� �̸�
    [SerializeField] TextMeshProUGUI   Text_RoomCost           = null;    // �� �ڽ�Ʈ
    [SerializeField] TextMeshProUGUI   Text_MyZeraInRoom       = null;    // �� �ȿ����� ���� �ڽ�Ʈ
    [SerializeField] GameObject        Panel_Room              = null;    // ���� ��ü���� �г�
    [SerializeField] GameObject        Button_StartGame        = null;    // ���� ���� ��ư
    [SerializeField] GameObject        Button_Ready            = null;    // ���� ���� ��ư
    [SerializeField] GameObject[]      Panel_PlayerSlot        = null;    // �÷��̾���� ���ü� �ִ� ����


    [SerializeField] GameObject        Panel_ChangeUIEffect    = null;    // �� ����,����� ȿ���� ��Ÿ�� �г�
    #endregion

    #region �濡�� ���� ������
    // �� ������ ���� ����Ʈ
    List<RoomInfo> _roomList = new List<RoomInfo>();

    // �� ����� ���ӿ� ���� �ڽ�Ʈ
    private int myZera = 0;

    // �� Ÿ��Ʋ�� ���� string
    private string roomNameText = "";

    // RoomOption�� maxPlayer�� byteŸ���̶� byteŸ��
    private byte myRoomMaxPlayer = 0;

    // �������� �����ϴ� �ð��� 0.2�ʷ�
    private WaitForSeconds delayUpdateTime = new WaitForSeconds(0.2f);

    // ��� ��ȭ ȿ���� ���� �ð�
    private WaitForSeconds uiUpdateTime = new WaitForSeconds(0.05f);
    // �ڱ� �ڽ��� �������
    private bool ready = false;

    private bool leftRoomDone = true;

    private bool enterRoomDone = true;

    private bool createRoomDone = true;

    private string myRoomName = null;

    RoomOptions myRo = null;

    // ���� �ڽ��� �����ִ� ��
    private Room curRoom = null;
    #endregion

    #region ���� ����

    // �α��� ���� ����
    private bool ? canLogin = null;

    #endregion

    private void Awake()
    {
        // �ʱ� ȭ�� ����
        Screen.SetResolution(960, 540, false);

        // ���۷� ����
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;

        StartCoroutine(ConnectAPI());
        SoundManager.Inst.StartBGM.Play();
        SoundManager.Inst.InGameBGM.Stop();
    }

    private void Start()
    {
        // ���� ����Ǿ� �ִ� ���¶�� ���������ʰ�
        if (PhotonNetwork.IsConnected)
            return;
        // �׷��� ������ �������ش�
        PhotonNetwork.ConnectUsingSettings();
    }

    private void Update()
    {
        if(PhotonNetwork.NetworkClientState == ClientState.JoinedLobby || 
            Panel_WaitConnect.activeInHierarchy)
        {
            Button_JoinLobby.interactable = false;
            return;
        }
        else
        {
            if (canLogin == true)
                Button_JoinLobby.interactable = true;
            else
                Button_JoinLobby.interactable = false;
        }
    }
  
    #region �ݹ� �Լ���
    // ������ ���� ���� �����ÿ� ȣ��
    public override void OnConnectedToMaster()
    {
        Text_ConnectionInfo.text = "������ ������ ���� �Ϸ�!";
        if (canLogin == true)
            PhotonNetwork.JoinLobby();
    }

    // ������ ������ ������ �������� �� ȣ��
    public override void OnDisconnected(DisconnectCause cause)
    {
        Text_ConnectionInfo.text = "������ �������� ������...";
        Text_UserName.text = "";
        // ������ ������ ���¿����� �κ� �г��� ��Ȱ��ȭ�ϰ�
        if (Panel_Lobby.activeInHierarchy)
            Panel_Lobby.SetActive(false);

        // ���Ƿ� ������ ������ ���� ���� �Ҿ������� �����
        // ���������� �ڵ����� ������ ������ �ٽ� �����ϰ� �õ�
        PhotonNetwork.ConnectUsingSettings();
    }

    // �κ� ���� �Ϸ�� ȣ��Ǵ� �Լ�
    public override void OnJoinedLobby()
    {
        // ����� �г����� API���� ������ UserName���� �ް� ���� ���̵� ���� �޾Ƶд�
        PhotonNetwork.LocalPlayer.NickName = ZeraAPIHandler.Inst.resGetUserProfile.userProfile.username;

        // �κ񿡼� ǥ���� �ؽ�Ʈ ���� �����
        Text_ConnectionInfo.text = "�κ� ���� �Ϸ�!";
        Text_UserName.text = PhotonNetwork.LocalPlayer.NickName;
        Text_MyZera.text = "MyZera : " + myZera.ToString();
        // �κ� ���� �Ϸ�� �α��� �г��� ��Ȱ��ȭ �κ� �г��� Ȱ��ȭ ���ش�
        Panel_Lobby.SetActive(true);
        // �κ� ���ӽ� �켱 �� ������ ������
        _roomList.Clear();
    }

    // �� ������ �ٲ� �ڵ����� ȣ��Ǵ� �Լ�
    // �κ� ���� ��(������ ���� -> �κ�)
    // ���ο� ���� ������� ���
    // ���� �����Ǵ� ���
    // ���� IsOpen ���� ��ȭ�� ���
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // ���� ������ �ٲ�� �� �ݹ��Լ��� ����Ǹ�
        // ���� �ִ� ����� ���� �����ְ�
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("ROOM"))
        {
            Destroy(obj);
        }

        // roomList ����Ʈ�� �������� Ȯ���Ѵ�
        foreach (RoomInfo roomInfo in roomList)
        {
            // �� ������ isVisible�� false �̰ų� ����Ʈ���� ���ŵ� ����(�÷��̾ �ƹ��� ���) ���
            if (!roomInfo.IsVisible || roomInfo.RemovedFromList)
            {
                // �� ��Ͽ��� �����Ѵ�
                if (_roomList.IndexOf(roomInfo) != -1)
                    _roomList.RemoveAt(_roomList.IndexOf(roomInfo));
            }
            else
            {
                // ���� ��Ȳ�� �ƴϸ� �渮��Ʈ�� ���� �־��ش�
                if (!_roomList.Contains(roomInfo)) _roomList.Add(roomInfo);
                else _roomList[_roomList.IndexOf(roomInfo)] = roomInfo;
            }
        }

        // ���� ������ ���� ����Ʈ�� �ִ� ����� ������ش�
        foreach (RoomInfo roomInfo in _roomList)
        {
            GameObject _room = Instantiate(room, Tr_Content_Room);
            RoomData roomData = _room.GetComponent<RoomData>();
            roomData.roomName = roomInfo.Name;
            roomData.maxPlayer = roomInfo.MaxPlayers;
            roomData.playerCount = roomInfo.PlayerCount;
            roomData.isOpen = roomInfo.IsOpen;
            roomData.roomCost = roomInfo.MaxPlayers * 5;
            roomData.UpdateInfo();

            // �ش� ���� �ο��� ���������� ��ưŬ���� ���� �����Ҽ� �����Ѵ�
            if (roomData.playerCount == roomData.maxPlayer)
                _room.GetComponent<Button>().interactable = false;

            // ���� ���������� �������� ��ư Ŭ���� ����
            if (roomData.isOpen == false)
                _room.GetComponent<Button>().interactable = false;
            else
            {
                // ���� ����������
                // delegate�� ������ �����Ͽ� Ŭ�������� �濡 ������ �� �ֵ��� ó��
                roomData.GetComponent<Button>().onClick.AddListener
                (
                    delegate
                    {
                        PlayClickSound();
                        roomNameText = roomData.roomName;
                        // �� �κ��� ������ �濡 �����ϴ� �κ�
                        enterRoomDone = false;
                        StartCoroutine(ChangeUIProcess());
                        myRoomName = roomData.roomName;
                    }
                );
            }
        }
    }

    // �÷��̾ ���� �������� �ش� �ݹ��Լ��� ����
    public override void OnLeftRoom()
    {
        leftRoomDone = true;
        ResetMyRoom();
    }

    // �濡 �����ϸ� �ڵ������� ȣ��Ǵ� �ݹ��Լ�
    public override void OnJoinedRoom()
    {
        myRoomName = null;
        myRo = null;

        if(enterRoomDone == false)
            enterRoomDone = true;

        if (createRoomDone == false)
            createRoomDone = true;

        // ���� �濡 �޷��ִ� �±׸� Hashtable ������ curRoomProperties ��� ������ �־��ش�
        curRoom = PhotonNetwork.CurrentRoom;
        // ���� �ִ� ���� UI �� Text�� ���� ���� text�� �־��ش�
        Text_roomName.text = "�� : " + curRoom.Name;

        if (PhotonNetwork.IsMasterClient)
        {
            // �ε����� 0 ���� �����ϹǷ� -1
            int max = curRoom.MaxPlayers - 1;
            //// ������ ó���� ���� �İԵǸ� ���� �ʱ� ���Ե��� ������ ���ش�
            curRoom.SetCustomProperties(new Hashtable
            {
                // ����(ȣ��Ʈ)�� 0�� ���� ��ȣ, �������� ������0, �����Ұ��� ������ -1
                // �ش� ������ �ε����� ��ɼ��� MaxPlayer�� ���Ͽ� MaxPlayer�� �Ѿ�� ������ �ݾ��ش�
                {"0", PhotonNetwork.LocalPlayer.ActorNumber }, {"1", 0 },
                {"2", 2 <= max ? 0 : -1 }, {"3", 3 <= max ? 0 : -1 }, {"4", 4 <= max ? 0 : -1 },
                {"5", 5 <= max ? 0 : -1 }, {"RoomCost", curRoom.MaxPlayers * 5}
            });
        }
        else
        {
            // �濡 ������ ����� �������� �����̶�� �ڱ��ȣ��
            for (int i = 0; i < 6; i++)
            {
                if (GetRoomTag(i) == 0)
                {
                    SetRoomTag(i, PhotonNetwork.LocalPlayer.ActorNumber);
                    break;
                }
            }
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable {
                        {"mySessionID", ZeraAPIHandler.Inst.resGetSessionID.sessionId }
                    });

        // �濡 �����ϸ� �غ���¸� false��
        SetLocalTag("IsReady", false);

        Text_MyZeraInRoom.text = "MyZera : " + myZera.ToString();

        // ���� ���� �±� ������ �������ش�
        StartCoroutine(RoomUpdate());
    }

    #endregion

    #region UI�� ����ϴ� �Լ���

    // �� �����Ҷ� �Է� ��Ʈ��
    public void OnvalueChangedCreateRoom(string inStr)
    {
        if (string.IsNullOrEmpty(inStr))
            Button_CreateRoom.interactable = false;
        else
            Button_CreateRoom.interactable = true;

        roomNameText = inStr;
    }

    // �κ� ���� ��ư�� ���� �Լ�
    public void OnClick_JoinLobby()
    {
        StartCoroutine(WaitConnectOsiris());
        SoundManager.Inst.ClickSound.Play();
    }

    // ���� ���� ��ư�� ���� �Լ�
    public void OnClick_DisConnect()
    {
        Text_ConnectionInfo.text = "���� ������ ...";
        PhotonNetwork.Disconnect();
        StartCoroutine(ConnectAPI());
        SoundManager.Inst.ClickNagative.Play();
    }

    // �� �����г� ��ư�� �޾��� �Լ�
    public void OnCreateRoomInfoButtonClicked()
    {
        // �� ������ �Է¹��� �ǳ��� Ȱ��ȭ ��Ű��
        Panel_CreateRoom.SetActive(true);
        // �ߺ��ؼ� ��� ��ư�� ������ �ʰ� ���� ��ư�� Ŭ���� ���´�
        Button_CreateRoom.GetComponent<Button>().interactable = false;
        SoundManager.Inst.ClickSound.Play();
    }

    // ���� �����Ҷ� �ִ� �÷��̾���� �������ִ� �Լ�
    public void CheckToggleValue()
    {
        for (int i = 0; i < togglesForMaxPlayer.Length; i++)
        {
            if (togglesForMaxPlayer[i].isOn == true)
            {
                myRoomMaxPlayer = (byte)(i + 2);
                break;
            }
        }
    }
    // �� ���� ��ư�� �� �Լ�
    public void OnCreateRoomButtonClicked()
    {
        SoundManager.Inst.ClickSound.Play();
        // ���� ��������� ������ư�� �ٽ� ����Ҽ� �ְ� Ŭ���� Ǯ���ְ�
        Button_CreateRoomPanel.GetComponent<Button>().interactable = true;

        // ���� ������ ����ִ� Ŭ����
        RoomOptions ro = new RoomOptions();
        ro.IsVisible = true;                        // ���� ���̰�
        ro.IsOpen = true;                           // ���� ����
        CheckToggleValue();                         // ���� ��� ����� ���õǾ��ִ��� Ȯ���ϰ�
        ro.MaxPlayers = myRoomMaxPlayer;            // �ִ� �ο����� üũ�� ��۰��� ���� ���� �ִ´�
        // Ŭ���̾�Ʈ���� ���� ������
        // �ش� Ŭ���̾�Ʈ�� Properties�� ����ش�
        ro.CleanupCacheOnLeave = true;

        createRoomDone = false;
        myRoomName = roomNameText;
        myRo = ro;
        StartCoroutine(ChangeUIProcess());
    }

    public void OnLeaveRoomButtonClicked()
    {
        SoundManager.Inst.ClickSound.Play();
        leftRoomDone = false;
        StartCoroutine(ChangeUIProcess());
        Text_MyZera.text = myZera.ToString();
    }

    // �غ� ��ư�� �������� ����Ǵ� �Լ�
    public void OnClick_ReadyButton()
    {
        if (ready == true)
        {
            SoundManager.Inst.ClickNagative.Play();
            ready = false;
        }
        else
        {
            SoundManager.Inst.ClickReady.Play();
            ready = true;
        }
        SetLocalTag("IsReady", ready);
    }

    // ���� ��ư�� �������� ����Ǵ� �Լ�
    public void OnClick_StartGame()
    {
        if (CheckPlayersReady())
        {
            curRoom.IsOpen = false;
            for(int i = 0; i < myRoomMaxPlayer; i++)
            {
                ZeraAPIHandler.Inst.allPlayersSessionID.Add((string)GetPlayer(i).CustomProperties["mySessionID"]);
            }
            PhotonNetwork.LoadLevel("GameScene");
        }
        else
        {
            SoundManager.Inst.ClickNagative.Play();
            Panel_Notice.GetComponentInChildren<TextMeshProUGUI>().text = "���� ��� �÷��̾ �غ� ���� �ʾҽ��ϴ�";
            Panel_Notice.SetActive(true);
        }
    }

    #endregion

    #region �ڷ�ƾ �Լ���

    IEnumerator ConnectAPI()
    {
        canLogin = false;
        Panel_WaitConnect.GetComponentInChildren<TextMeshProUGUI>().text = "�� �� �� . . .";
        StartCoroutine(ActiveWaitPanel());
        yield return StartCoroutine(RequestAPI());
        // �⺻������ API�� ����Ǿ� ������ �޾ƿ����� �Ʒ� ������� �����Ѵ�
        ZeraAPIHandler.Inst.GetMyZeraBalance();

        yield return new WaitForSeconds(2f);

        // ����(������ Ŭ���̾�Ʈ)�� ���Ӿ����� �̵��Ҷ� Ŭ���̾�Ʈ�鵵 ���� �̵�
        PhotonNetwork.AutomaticallySyncScene = true;
        canLogin = true;

        yield return null;
    }

    IEnumerator RequestAPI()
    {
        ZeraAPIHandler.Inst.GetUserProfile();
        ZeraAPIHandler.Inst.GetSessionID();
        yield return new WaitForSeconds(2f);

        ZeraAPIHandler.Inst.GetBettingSettings();
        yield return new WaitForSeconds(2f);
    }

    // Wait osiris connect if Connect JoinLobby else announce connect osiris
    IEnumerator WaitConnectOsiris()
    {
        if (canLogin == false)
        {
            Panel_Notice.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                "Osiris�� ���� �Ǿ� ���� �ʽ��ϴ�.\n���� �� �ٽ� ������ �ּ���";
            Panel_Notice.SetActive(true);
            yield break;
        }

        // ���� ��Ʈ��ũ�� ������ ������ ����� ���°� �ƴ϶�� ó������ �ʰ�
        if (PhotonNetwork.NetworkClientState != ClientState.ConnectedToMasterServer)
            yield break;
        // DAPPX�� ���� Cost
        myZera = ZeraAPIHandler.Inst.resBalanceInfo.data.balance;
        Text_ConnectionInfo.text = "���� �õ� �� ...";

        Panel_WaitConnect.GetComponentInChildren<TextMeshProUGUI>().text =
    "���� ������ \n�޾ƿ��� ���Դϴ� \n���ݸ� ��ٷ� �ּ���";
        // �ε� �г�4�ʵ��� �����ϴ°��� ��ٸ���
        yield return StartCoroutine(ActiveWaitPanel());

        PhotonNetwork.JoinLobby();
    }

    // 4�ʵ��� �ε��г��� ������� ��Ȱ��ȭ
    IEnumerator ActiveWaitPanel()
    {
        Panel_WaitConnect.SetActive(true);
        yield return new WaitForSeconds(5f);
        Panel_WaitConnect.SetActive(false);
        Panel_WaitConnect.GetComponentInChildren<TextMeshProUGUI>().text = "";
    }

    // ����� ������ �����ϴ� �ڷ�ƾ
    IEnumerator RoomUpdate()
    {
        // �濡 ������ ���¶�� ��� üũ�Ѵ�
        while (PhotonNetwork.InRoom)
        {
            yield return delayUpdateTime;
            // ������Ʈ ���� �濡�� �����ԵǸ� ������ ������ �����
            if (!PhotonNetwork.InRoom) yield break;

            Text_RoomCost.text = "�� ���� �ݾ� " + ((int)curRoom.CustomProperties["RoomCost"]).ToString();

            // ������ �ٲ������ �ٲ��÷��̾ �����̸� �ش� �÷��̾��� ���ӽ��۹�ư�� Ȱ��ȭ �ȴ�
            if (PhotonNetwork.IsMasterClient)
            {
                Button_Ready.SetActive(false);
                Button_StartGame.SetActive(true);
                SetLocalTag("IsReady", false);
            }
            else
            {
                Button_Ready.SetActive(true);
                Button_StartGame.SetActive(false);
            }

            // �÷��̾�� ���� ���� ����
            for (int i = 0; i < 6; i++)
            {
                // ����(������Ŭ���̾�Ʈ)�� �漳���� ���ش�
                if (PhotonNetwork.IsMasterClient)
                {
                    // ������ �����ִµ� ����� ��������� �ش� ���Կ� 0�� ����
                    if (GetPlayer(i) == null && GetRoomTag(i) > 0) SetRoomTag(i, 0);
                }

                // �� �±׸� �ٶ� ������ �������� 0, -1�� �±װ��� �����Ƿ� �̸� ���� �Ǵ�
                if (GetRoomTag(i) == -1)
                {
                    // ������ �����ٴ� UI �̹����� ����ش�
                    Panel_PlayerSlot[i].transform.GetChild(4).gameObject.SetActive(true);
                }
                else if (GetRoomTag(i) > 0)
                {
                    // �ݴ��쿡�� �����̹����� ��Ȱ���� �����ִ°�ó�� ���̰� ó��
                    Panel_PlayerSlot[i].transform.GetChild(4).gameObject.SetActive(false);
                }

                // ������ �����ִ� �����ε� �÷��̾ ���ٸ� ���� ó��
                if (GetPlayer(i) == null)
                {
                    Panel_PlayerSlot[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
                    Panel_PlayerSlot[i].transform.GetChild(1).gameObject.SetActive(false);
                    Panel_PlayerSlot[i].transform.GetChild(2).gameObject.SetActive(false);
                    Panel_PlayerSlot[i].transform.GetChild(3).gameObject.SetActive(false);
                    Panel_PlayerSlot[i].transform.GetChild(5).gameObject.SetActive(false);
                }
                // �ݴ� ����� �ش� �÷��̾� ���� �־��ִ� ó��
                else
                {
                    Panel_PlayerSlot[i].GetComponentInChildren<TextMeshProUGUI>().text = GetPlayer(i).NickName; ;
                    Panel_PlayerSlot[i].transform.GetChild(1).gameObject.SetActive(true);
                    Panel_PlayerSlot[i].transform.GetChild(3).gameObject.SetActive(true);

                    // ������ ������ ����ǥ�ø� ����
                    if (GetPlayer(i).IsMasterClient)
                    {
                        Panel_PlayerSlot[i].transform.GetChild(2).gameObject.SetActive(false);
                        Panel_PlayerSlot[i].transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "�� ��";
                        Panel_PlayerSlot[i].transform.GetChild(5).gameObject.SetActive(true);
                    }
                    else
                    {
                        // Ŭ���̾�Ʈ���� ������ ������¿� ���� �����̹����� ���ų� ������ ����ǥ�ô� ������
                        Panel_PlayerSlot[i].transform.GetChild(2).gameObject.SetActive((bool)GetPlayer(i).CustomProperties["IsReady"]);
                        Panel_PlayerSlot[i].transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "READY";
                        Panel_PlayerSlot[i].transform.GetChild(5).gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    // �濡 ���� ������ ������ ����
    IEnumerator ChangeUIProcess()
    {
        float fadeCount = 0; // ó�� ���İ�(����)
        Panel_ChangeUIEffect.SetActive(true);
        while (fadeCount < 1.0f) // ���İ��� 1 �������� �ɶ����� �ݺ�
        {
            fadeCount += 0.05f;
            yield return uiUpdateTime;
            Panel_ChangeUIEffect.GetComponent<Image>().color = new Color(0, 0, 0, fadeCount);
        }

        if (leftRoomDone == false)
        {
            PhotonNetwork.LeaveRoom();
            yield return leftRoomDone = true;
            // UI���� ��Ȳ�� �°� ó��
            Panel_Room.SetActive(false);
            Panel_Login.SetActive(true);
            Panel_Lobby.SetActive(true);
        }

        if (enterRoomDone == false)
        {
            PhotonNetwork.JoinRoom(myRoomName, null);
            yield return enterRoomDone = true;
            // �гε��� UI���� �°� ó�����ش�
            Panel_Login.SetActive(false);
            Panel_Lobby.SetActive(false);
            Panel_Room.SetActive(true);
        }

        if(createRoomDone == false)
        {
            PhotonNetwork.CreateRoom(myRoomName, myRo); // ������ ���� ����� �Լ�
            yield return createRoomDone = true;
            // �гε��� UI���� �°� ó�����ش�
            Panel_Login.SetActive(false);
            Panel_Lobby.SetActive(false);
            Panel_Room.SetActive(true);
        }

        while (fadeCount > 0) // �ٽ� ���İ��� 0 ���������� ���� �ݺ�
        {
            fadeCount -= 0.05f;
            yield return uiUpdateTime;
            Panel_ChangeUIEffect.GetComponent<Image>().color = new Color(0, 0, 0, fadeCount);
        }
        Panel_ChangeUIEffect.SetActive(false);
    }

    #endregion

    #region �� ���� �Լ�

    private void ResetMyRoom()
    {
        // ���� ������ �����濡 ������ UIó������ �ʱ�ȭ�Ͽ��ش�
        for (int i = 0; i < 6; i++)
        {
            for (int j = 1; j <= 5; j++)
            {
                Panel_PlayerSlot[i].transform.GetChild(j).gameObject.SetActive(false);
                Panel_PlayerSlot[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
            }
        }
    }

    // �濡 �±׸� �޾��ִ� �Լ�
    private void SetRoomTag(int slotIndex, int value) => curRoom.SetCustomProperties(new Hashtable { { slotIndex.ToString(), value } });

    // ���� �±׸� ������ �Լ�
    private int GetRoomTag(int slotIndex) => (int)curRoom.CustomProperties[slotIndex.ToString()];

    // �÷��̾ ����ִ� �Լ�
    private Player GetPlayer(int slotIndex)
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].ActorNumber == GetRoomTag(slotIndex))
                return PhotonNetwork.PlayerList[i];
        }
        return null;
    }

    // �ڱ��ڽ�(�����÷��̾�)�� �±׸� �޾��ִ� �Լ�
    private void SetLocalTag(string key, bool value) => PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { key, value } });


    // �÷��̾���� ������µ��� üũ
    public bool CheckPlayersReady()
    {
        int readyCnt = 0;
        // ����� �÷��̾���� ����ŭ �ݺ����� ����
        for (int i = 0; i < curRoom.PlayerCount; i++)
        {
            if (GetPlayer(i) == null)
                continue;
            // �ش� �÷��̾��� �غ���� �±� IsReady�� true��� ī��Ʈ�� �÷�
            if ((bool)GetPlayer(i).CustomProperties["IsReady"])
                readyCnt++;
        }

        // �� ī��Ʈ�� ������ ������ �÷��̾��� ���� ������ true
        if (readyCnt == curRoom.MaxPlayers - 1)
            return true;
        else // ���� ������ false
            return false;
    }

    #endregion

    public void PlayClickSound() => SoundManager.Inst.ClickSound.Play();
}