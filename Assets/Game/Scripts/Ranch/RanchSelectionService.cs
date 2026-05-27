using NekogamiRanch.Animals;

namespace NekogamiRanch.Ranch
{
    public class RanchSelectionService
    {
        private readonly System.Action stateChanged;

        public RanchSelectionService(System.Action stateChanged)
        {
            this.stateChanged = stateChanged;
        }

        public MapCell SelectedCell { get; private set; }

        public void SelectCell(MapCell cell)
        {
            if (SelectedCell != null)
            {
                SelectedCell.SetSelected(false);
            }

            SelectedCell = cell;
            if (SelectedCell != null)
            {
                SelectedCell.SetSelected(true);
            }

            stateChanged?.Invoke();
        }

        public bool ClearIfSelectionLostAnimal()
        {
            if (SelectedCell == null || SelectedCell.Animal != null)
            {
                return false;
            }

            SelectCell(null);
            return true;
        }

        public bool IsSelectedAnimal(Animal animal)
        {
            return animal != null && SelectedCell != null && SelectedCell.Animal == animal;
        }

        public string GetSelectedCellText()
        {
            return RanchTextFormatter.GetSelectedCellText(SelectedCell);
        }
    }
}
