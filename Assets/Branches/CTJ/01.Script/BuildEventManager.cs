using System.Collections;
using TMPro;
using UnityEngine;

public class BuildEventManager : MonoSingleton<BuildEventManager>
{
    [SerializeField] GameObject messageBox;
    [SerializeField] TextMeshProUGUI messageText;
    [SerializeField] float messageTime = 3.0f;

    protected override void Awake()
    {
        base.Awake();
        messageBox.SetActive(false);
    }

    public IEnumerator MessageOn(string message)
    {
        messageBox.SetActive(true);
        messageText.text = message;
        yield return new WaitForSeconds(messageTime);
        messageBox.SetActive(false);
    }
}
