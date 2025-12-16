using UnityEngine;

public class animation_functions : MonoBehaviour
{
    private void hide()
    {
        print("hide");
        gameObject.SetActive(false);
    }
    private void destroy()
    {
        Destroy(gameObject);
    }
}
