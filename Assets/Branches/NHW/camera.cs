using JetBrains.Annotations;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class camera : MonoBehaviour
{
    [SerializeField] private CinemachineCamera[] cameras;
    [SerializeField] private int activePriority = 20;
    [SerializeField] private int inactivePriority = 0;
    public float MoveSpeed;
    private CinemachineCamera _current;
    public Transform LeftLimitMark;
    public Transform RightLimitMark;
    public GameObject ColdVFX;
    public GameObject SnowSlide;
    public SpriteRenderer sr;
    public void SwitchTo(int index)
    {
        if (cameras == null || cameras.Length == 0) return;
        if (index < 0 || index >= cameras.Length) return;

        for (int i = 0; i < cameras.Length; i++)
            cameras[i].Priority = inactivePriority;

        _current = cameras[index];
        if (index == 1)
        {
            ColdSetting();
        }
        else
        {
            ColdVFX.SetActive(false);
            SnowSlide.SetActive(false);
        }
        _current.Priority = activePriority;
    }
    public void MoveCamera(int a)
    {
        if(_current == cameras[1])
        {
            Debug.Log("Cannot move this camera");
            return;
        }
        if (a==1)
        {
            Debug.Log("Left");
            if(_current.gameObject.transform.position.x < LeftLimitMark.position.x)
            {
                Debug.Log("Reached Left Limit");
                return;
            }
            _current.gameObject.transform.position+= Vector3.left  * MoveSpeed * Time.deltaTime;
        }
        if(a==2)
        {
            Debug.Log("Right");
            if (_current.gameObject.transform.position.x > RightLimitMark.position.x)
            {
                Debug.Log("Reached Right Limit");
                return;
            }
            _current.gameObject.transform.position += Vector3.right * MoveSpeed*Time.deltaTime;
        }
    }
    public void ColdSetting()
    {
        ColdVFX.SetActive(true);
        SnowSlide.SetActive(true);
        sr.color = new Color(1f, 1f, 1f, 0f);
    }
}
