using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public static LevelLoader instance;

    private void Awake()
    {
        if (instance != null) Destroy(gameObject);
        instance = this;
    }

    public void loadScene(string name)
    {
        StartCoroutine(LoadLevel(name));
    }
   
    IEnumerator LoadLevel(string name)
    {
        animator.Play("effect");
        yield return new WaitForSecondsRealtime(1.5f);

        Time.timeScale = 1;
        Addressables.LoadSceneAsync(name);
    }
}
