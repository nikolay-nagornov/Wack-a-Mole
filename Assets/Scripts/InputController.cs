using UnityEngine;
using UnityEngine.EventSystems;

public class InputController : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;

    private Camera cam;
    private Ray ray;
    private RaycastHit hit;
    LevelManager levelMng;

    void Start()
    {
        cam = Camera.main;
        levelMng = LevelManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            ray = cam.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out hit, 100f, layerMask.value))
            {
                //Debug.Log(hit.collider.name);
                switch (hit.collider.tag)
                {
                    case "Mole":
                        hit.collider.GetComponent<Enemy>().Defeate();
                        break;
                    case "Armor":
                        hit.collider.GetComponent<EnemiesArmor>().Defeate();
                        break;
                }
            }
        }
    }
}
