using NekogamiRanch.Ranch;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NekogamiRanch.UI
{
    public class RanchUIController : MonoBehaviour
    {
        [SerializeField] private RanchHUD hud;
        [SerializeField] private AnimalOfferPanel offerPanel;
        [SerializeField] private AnimalDetailPanel animalDetailPanel;
        [SerializeField] private AnimalRemoveButtonPanel animalRemoveButtonPanel;
        [SerializeField] private RanchManager manager;

        private bool initialized;

        private void Start()
        {
            if (manager == null)
            {
                manager = FindObjectOfType<RanchManager>();
            }

            Initialize(manager);
        }

        private void Update()
        {
            if (manager == null || animalDetailPanel == null || !animalDetailPanel.IsShowing)
            {
                return;
            }

            if (animalDetailPanel.LastShownFrame == Time.frameCount)
            {
                return;
            }

            if (TryGetPrimaryPointerDownPosition(out var screenPosition) && !IsPointerOverUi() && !IsPointerOverOccupiedCell(screenPosition))
            {
                manager.SelectCell(null);
            }
        }

        public void Initialize(RanchManager ranchManager)
        {
            if (initialized && manager == ranchManager)
            {
                return;
            }

            if (manager != null && initialized)
            {
                manager.StateChanged -= Refresh;
            }

            manager = ranchManager;
            if (manager == null)
            {
                return;
            }

            ResolveReferences();

            if (hud != null)
            {
                hud.Initialize(manager.NextDay);
            }

            if (offerPanel != null)
            {
                offerPanel.Initialize(OnOfferSelected);
            }

            if (animalRemoveButtonPanel != null)
            {
                animalRemoveButtonPanel.Initialize(manager);
            }

            manager.StateChanged += Refresh;
            initialized = true;
            Refresh();
        }

        private void ResolveReferences()
        {
            if (hud == null)
            {
                hud = GetComponentInChildren<RanchHUD>(true);
            }

            if (offerPanel == null)
            {
                offerPanel = GetComponentInChildren<AnimalOfferPanel>(true);
            }

            if (animalDetailPanel == null)
            {
                animalDetailPanel = GetComponentInChildren<AnimalDetailPanel>(true);
            }

            if (animalRemoveButtonPanel == null)
            {
                animalRemoveButtonPanel = GetComponentInChildren<AnimalRemoveButtonPanel>(true);
            }
        }

        private void Refresh()
        {
            if (manager == null)
            {
                return;
            }

            if (hud != null)
            {
                hud.Refresh(
                    manager.Day,
                    manager.Money,
                    manager.Cans,
                    manager.GetSelectedCellText(),
                    manager.LastSettlementReport,
                    manager.IsWaitingForOfferSelection,
                    manager.IsWaitingToEnterNextDay);
            }

            if (offerPanel != null)
            {
                offerPanel.Refresh(manager.CurrentOffers, manager.IsWaitingForOfferSelection);
            }

            if (animalDetailPanel != null)
            {
                animalDetailPanel.Refresh(manager.SelectedCell != null ? manager.SelectedCell.Animal : null);
            }

            if (animalRemoveButtonPanel != null)
            {
                animalRemoveButtonPanel.Refresh(manager.SelectedCell);
            }
        }

        private void OnOfferSelected(int index)
        {
            if (manager == null)
            {
                return;
            }

            manager.SelectOffer(index);
        }

        private static bool TryGetPrimaryPointerDownPosition(out Vector2 screenPosition)
        {
            if (Input.GetMouseButtonDown(0))
            {
                screenPosition = Input.mousePosition;
                return true;
            }

            for (var i = 0; i < Input.touchCount; i++)
            {
                var touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began)
                {
                    screenPosition = touch.position;
                    return true;
                }
            }

            screenPosition = default;
            return false;
        }

        private static bool IsPointerOverUi()
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            if (EventSystem.current.IsPointerOverGameObject())
            {
                return true;
            }

            for (var i = 0; i < Input.touchCount; i++)
            {
                if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsPointerOverOccupiedCell(Vector2 screenPosition)
        {
            var camera = Camera.main;
            if (camera == null)
            {
                return false;
            }

            var worldPosition = camera.ScreenToWorldPoint(screenPosition);
            var hit = Physics2D.OverlapPoint(worldPosition);
            if (hit == null)
            {
                return false;
            }

            var cell = hit.GetComponent<MapCell>();
            return cell != null && cell.Animal != null;
        }

        private void OnDestroy()
        {
            if (manager != null)
            {
                manager.StateChanged -= Refresh;
            }
        }
    }
}
