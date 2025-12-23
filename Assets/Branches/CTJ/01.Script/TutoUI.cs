using UnityEngine;

public class TutoUI : MonoBehaviour
{
    [SerializeField] GameObject tutoPanel;
    bool isOpened = false;

    private void Awake()
    {
        tutoPanel.SetActive(false);
    }

    public void Inter()
    {
        tutoPanel.SetActive(!isOpened);
        isOpened = !isOpened;
    }
}
