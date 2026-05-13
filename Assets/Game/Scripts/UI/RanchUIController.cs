using NekogamiRanch.Ranch;
using UnityEngine;

namespace NekogamiRanch.UI
{
    public class RanchUIController : MonoBehaviour
    {
        [SerializeField] private RanchHUD hud;
        [SerializeField] private AnimalOfferPanel offerPanel;

        private RanchManager manager;

        public void Initialize(RanchManager ranchManager)
        {
            if (manager != null)
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

            manager.StateChanged += Refresh;
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
                    manager.GetSelectedCellText(),
                    manager.LastSettlementReport,
                    manager.IsWaitingForOfferSelection,
                    manager.IsWaitingToEnterNextDay);
            }

            if (offerPanel != null)
            {
                offerPanel.Refresh(manager.CurrentOffers, manager.IsWaitingForOfferSelection);
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

        private void OnDestroy()
        {
            if (manager != null)
            {
                manager.StateChanged -= Refresh;
            }
        }
    }
}
