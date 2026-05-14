using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;
using NekogamiRanch.Ranch;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NekogamiRanch.Testing
{
    public class RanchTestController : MonoBehaviour
    {
        [SerializeField] private RanchManager manager;
        [SerializeField] private Button enterTestButton;
        [SerializeField] private Button exitTestButton;
        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private TMP_Text selectedCellText;
        [SerializeField] private TMP_Dropdown animalDropdown;
        [SerializeField] private Button setAnimalButton;
        [SerializeField] private Button deleteAnimalButton;
        [SerializeField] private Button resetTestButton;
        [SerializeField] private List<AnimalData> animalCatalog = new List<AnimalData>();

        private static TMP_FontAsset uiFont;
        private readonly List<AnimalData> dropdownAnimals = new List<AnimalData>();

        private void Start()
        {
            if (manager == null)
            {
                manager = FindObjectOfType<RanchManager>();
            }

            LoadUiFont();
            LoadAnimalCatalog();
            ApplyFontToPanel();
            BindUi();
            SetPanelVisible(false);
            Refresh();

            if (manager != null)
            {
                manager.StateChanged += Refresh;
            }
        }

        private void OnDestroy()
        {
            if (manager != null)
            {
                manager.StateChanged -= Refresh;
            }
        }

        private static void LoadUiFont()
        {
#if UNITY_EDITOR
            if (uiFont == null)
            {
                uiFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/GenSenRounded2-B SDF.asset");
            }
#endif
        }

        private void LoadAnimalCatalog()
        {
#if UNITY_EDITOR
            if (animalCatalog.Count == 0)
            {
                animalCatalog = AssetDatabase.FindAssets("t:AnimalData", new[] { "Assets/Game/Data/Animals" })
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<AnimalData>)
                    .Where(data => data != null)
                    .OrderBy(data => data.Family)
                    .ThenBy(data => data.Rarity)
                    .ThenBy(data => data.DisplayName)
                    .ToList();
            }
#endif
        }

        private void ApplyFontToPanel()
        {
            if (uiFont == null || panelRoot == null)
            {
                return;
            }

            foreach (var text in panelRoot.GetComponentsInChildren<TMP_Text>(true))
            {
                text.font = uiFont;
                text.fontSize = Mathf.Max(text.fontSize, 22f);
            }
        }

        private void BindUi()
        {
            dropdownAnimals.Clear();
            dropdownAnimals.AddRange(animalCatalog.Where(data => data != null));

            if (animalDropdown != null)
            {
                animalDropdown.ClearOptions();
                animalDropdown.AddOptions(dropdownAnimals
                    .Select(data => $"{data.DisplayName} ({data.Id})")
                    .ToList());
                animalDropdown.RefreshShownValue();
            }

            if (enterTestButton != null)
            {
                enterTestButton.onClick.RemoveListener(EnterTestMode);
                enterTestButton.onClick.AddListener(EnterTestMode);
            }

            if (exitTestButton != null)
            {
                exitTestButton.onClick.RemoveListener(ExitTestMode);
                exitTestButton.onClick.AddListener(ExitTestMode);
            }

            if (setAnimalButton != null)
            {
                setAnimalButton.onClick.RemoveListener(SetSelectedAnimal);
                setAnimalButton.onClick.AddListener(SetSelectedAnimal);
            }

            if (deleteAnimalButton != null)
            {
                deleteAnimalButton.onClick.RemoveListener(DeleteSelectedAnimal);
                deleteAnimalButton.onClick.AddListener(DeleteSelectedAnimal);
            }

            if (resetTestButton != null)
            {
                resetTestButton.onClick.RemoveListener(ResetTestMode);
                resetTestButton.onClick.AddListener(ResetTestMode);
            }

            if (enterTestButton == null || panelRoot == null || selectedCellText == null ||
                animalDropdown == null || setAnimalButton == null || deleteAnimalButton == null || resetTestButton == null)
            {
                Debug.LogWarning("[RanchTestController] Test UI references are not fully assigned. Build the test UI in SampleScene and bind all fields.");
            }
        }

        private void Refresh()
        {
            var selectedCell = manager != null ? manager.SelectedCell : null;
            if (selectedCellText != null)
            {
                if (selectedCell == null)
                {
                    selectedCellText.text = "\u5f53\u524d\u5730\u5757\uff1a\u672a\u9009\u62e9";
                }
                else
                {
                    var animalName = selectedCell.Animal != null ? selectedCell.Animal.DisplayName : "\u7a7a";
                    selectedCellText.text = $"\u5f53\u524d\u5730\u5757\uff1a({selectedCell.Coords.x},{selectedCell.Coords.y})  {animalName}";
                }
            }

            var hasCell = selectedCell != null;
            var hasAnimal = selectedCell != null && selectedCell.Animal != null;
            if (setAnimalButton != null)
            {
                setAnimalButton.interactable = manager != null && manager.IsTestMode && hasCell && dropdownAnimals.Count > 0;
            }

            if (deleteAnimalButton != null)
            {
                deleteAnimalButton.interactable = manager != null && manager.IsTestMode && hasAnimal;
            }
        }

        private void EnterTestMode()
        {
            manager?.EnterTestMode();
            SetPanelVisible(true);
        }

        private void ExitTestMode()
        {
            manager?.ExitTestMode();
            SetPanelVisible(false);
        }

        private void SetSelectedAnimal()
        {
            if (manager == null || manager.SelectedCell == null || animalDropdown == null)
            {
                return;
            }

            var index = animalDropdown.value;
            if (index < 0 || index >= dropdownAnimals.Count)
            {
                return;
            }

            manager.TrySetAnimalAt(manager.SelectedCell.Coords, dropdownAnimals[index]);
        }

        private void DeleteSelectedAnimal()
        {
            if (manager == null || manager.SelectedCell == null)
            {
                return;
            }

            manager.TryClearAnimalAt(manager.SelectedCell.Coords);
        }

        private void ResetTestMode()
        {
            manager?.EnterTestMode();
            SetPanelVisible(true);
        }

        private void SetPanelVisible(bool visible)
        {
            if (panelRoot != null)
            {
                panelRoot.gameObject.SetActive(visible);
            }
        }
    }
}
