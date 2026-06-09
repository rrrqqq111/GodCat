using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public UIAnimalSequentialEnter animalEnterController;
    public MainMenuButtonController mainMenuButtonController;

    void Start()
    {
        animalEnterController.Play();
    }
}
