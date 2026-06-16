using NekogamiRanch.Animals;
using UnityEngine;

namespace NekogamiRanch.Presentation
{
    public readonly struct RanchPreyAnimationRequest
    {
        public RanchPreyAnimationRequest(Animal predator, Vector2Int predatorCoords, Vector2Int targetCoords)
        {
            Predator = predator;
            PredatorCoords = predatorCoords;
            TargetCoords = targetCoords;
        }

        public Animal Predator { get; }
        public Vector2Int PredatorCoords { get; }
        public Vector2Int TargetCoords { get; }
    }
}
