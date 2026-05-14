using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void EasyLevel()
    {
        SceneManager.LoadScene(1);
    }
    public void MediumLevel()
    {
        SceneManager.LoadScene(2);
    }
    public void HardLevel()
    {
        SceneManager.LoadScene(3);
    }
}
