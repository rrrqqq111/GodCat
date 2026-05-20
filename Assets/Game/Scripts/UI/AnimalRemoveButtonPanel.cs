using NekogamiRanch.Animals;
using NekogamiRanch.Ranch;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NekogamiRanch.UI
{
    public class AnimalRemoveButtonPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private Button removeButton;
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private Vector2 screenOffset = new Vector2(0f, 64f);

        private RanchManager manager;
        private Canvas canvas;
        private RectTransform canvasRect;
        private Camera worldCamera;
        private MapCell targetCell;
        private Animal targetAnimal;

        private void Awake()
        {
            EnsureView();
            Hide();
        }

        private void LateUpdate()
        {
            UpdatePosition();
        }

        public void Initialize(RanchManager ranchManager)
        {
            manager = ranchManager;
            EnsureView();

            if (removeButton != null)
            {
                removeButton.onClick.RemoveListener(RemoveTargetAnimal);
                removeButton.onClick.AddListener(RemoveTargetAnimal);
            }

            Refresh(null);
        }

        public void Refresh(MapCell selectedCell)
        {
            EnsureView();
            targetCell = selectedCell != null && selectedCell.Animal != null ? selectedCell : null;
            targetAnimal = targetCell != null ? targetCell.Animal : null;

            if (targetAnimal == null)
            {
                Hide();
                return;
            }

            var cost = manager != null ? manager.RemoveAnimalCansCost : 1;
            if (labelText != null)
            {
                labelText.text = cost > 0 ? $"\u79fb\u9664 -{cost}\u7f50\u5934" : "\u79fb\u9664";
            }

            if (removeButton != null)
            {
                removeButton.interactable = manager != null && manager.Cans >= cost;
            }

            gameObject.SetActive(true);
            UpdatePosition();
        }

        private void RemoveTargetAnimal()
        {
            if (manager == null || targetAnimal == null)
            {
                Hide();
                return;
            }

            if (manager.TryRemoveAnimalWithCans(targetAnimal))
            {
                Hide();
            }
        }

        private void UpdatePosition()
        {
            if (!gameObject.activeSelf || panelRoot == null || targetCell == null)
            {
                return;
            }

            if (canvas == null || canvasRect == null)
            {
                ResolveCanvas();
            }

            var camera = ResolveWorldCamera();
            if (camera == null || canvasRect == null)
            {
                return;
            }

            var screenPoint = camera.WorldToScreenPoint(targetCell.transform.position);
            if (screenPoint.z < 0f)
            {
                return;
            }

            var screenPosition = (Vector2)screenPoint + screenOffset;
            var canvasCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera != null ? canvas.worldCamera : camera
                : null;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, canvasCamera, out var anchoredPosition))
            {
                panelRoot.anchoredPosition = anchoredPosition;
            }
        }

        private void Hide()
        {
            targetCell = null;
            targetAnimal = null;
            gameObject.SetActive(false);
        }

        private void EnsureView()
        {
            if (panelRoot == null)
            {
                panelRoot = transform as RectTransform;
            }

            if (removeButton == null)
            {
                removeButton = GetComponent<Button>();
            }

            if (labelText == null)
            {
                labelText = GetComponentInChildren<TMP_Text>(true);
            }

            ResolveCanvas();
        }

        private void ResolveCanvas()
        {
            canvas = GetComponentInParent<Canvas>();
            canvasRect = canvas != null ? canvas.transform as RectTransform : null;
        }

        private Camera ResolveWorldCamera()
        {
            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }

            return worldCamera;
        }

        private void OnDestroy()
        {
            if (removeButton != null)
            {
                removeButton.onClick.RemoveListener(RemoveTargetAnimal);
            }
        }
    }
}
