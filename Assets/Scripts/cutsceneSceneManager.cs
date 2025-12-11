using UnityEngine;
using UnityEngine.SceneManagement;

public class cutsceneSceneManager : MonoBehaviour
{
    public string nextSceneName;
    public float delayBeforeLoad;

    // Update is called once per frame
    void Update()
    {
        delayBeforeLoad -= Time.deltaTime;
        if (delayBeforeLoad <= 0f) 
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
