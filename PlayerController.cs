using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CharacterController controller;
    public Camera playerCamera;

    public float speed = 5f;
    public float mouseSensitivity = 2f;
    public float gravity = -9.81f;

    private float xRotation = 0f;
    private Vector3 velocity;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        MovePlayer();
        LookAround();
    }

    void MovePlayer()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(Vector3.up * mouseX);
    }
}using UnityEngine;

public class GrabHand : MonoBehaviour
{
    public Camera cam;
    public Transform handPoint;
    public float grabDistance = 15f;
    public LineRenderer line;

    private Rigidbody grabbedObject;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryGrab();
        }

        if (Input.GetMouseButton(0) && grabbedObject != null)
        {
            HoldObject();
        }

        if (Input.GetMouseButtonUp(0))
        {
            ReleaseObject();
        }
    }

    void TryGrab()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance))
        {
            if (hit.collider.CompareTag("Grabbable"))
            {
                grabbedObject = hit.collider.GetComponent<Rigidbody>();
                grabbedObject.useGravity = false;

                line.enabled = true;
            }
        }
    }

    void HoldObject()
    {
        grabbedObject.MovePosition(handPoint.position);

        line.SetPosition(0, transform.position);
        line.SetPosition(1, grabbedObject.position);
    }

    void ReleaseObject()
    {
        if (grabbedObject != null)
        {
            grabbedObject.useGravity = true;
            grabbedObject = null;
        }

        line.enabled = false;
    }
}using UnityEngine;

public class DoorController : MonoBehaviour
{
    public bool needsKeycard = true;
    public bool playerHasKeycard = false;
    public float openAngle = 90f;
    public float openSpeed = 3f;

    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));
    }

    void Update()
    {
        if (isOpen)
            transform.rotation = Quaternion.Slerp(transform.rotation, openRotation, Time.deltaTime * openSpeed);
        else
            transform.rotation = Quaternion.Slerp(transform.rotation, closedRotation, Time.deltaTime * openSpeed);
    }

    public void TryOpen()
    {
        if (!needsKeycard || playerHasKeycard)
        {
            isOpen = true;
        }
        else
        {
            Debug.Log("Door locked. Find the keycard.");
        }
    }
}using UnityEngine;

public class KeycardPickup : MonoBehaviour
{
    public DoorController door;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            door.playerHasKeycard = true;
            Destroy(gameObject);
            Debug.Log("Keycard collected.");
        }
    }
}using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    public Transform player;
    public float detectionRange = 15f;
    public float attackRange = 2f;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRange)
        {
            agent.SetDestination(player.position);
        }

        if (distance <= attackRange)
        {
            Debug.Log("Game Over");
        }
    }
}using UnityEngine;

public class Interact : MonoBehaviour
{
    public Camera cam;
    public float interactDistance = 4f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
            {
                DoorController door = hit.collider.GetComponent<DoorController>();

                if (door != null)
                {
                    door.TryOpen();
                }
            }
        }
    }
}
