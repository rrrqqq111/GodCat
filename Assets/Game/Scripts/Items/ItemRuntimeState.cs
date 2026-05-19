using System;
using System.Collections.Generic;

namespace NekogamiRanch.Items
{
    [Serializable]
    public class ItemRuntimeState
    {
        private readonly Dictionary<string, int> counters = new Dictionary<string, int>();

        public ItemRuntimeState(ItemData data, int count = 1)
        {
            Data = data;
            Count = count;
        }

        public ItemData Data { get; }
        public int Count { get; private set; }
        public int Stack { get; private set; }
        public int Tick { get; private set; }

        public void AddCount(int amount)
        {
            Count += amount;
        }

        public bool TryConsume(int amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            if (Count < amount)
            {
                return false;
            }

            Count -= amount;
            return true;
        }

        public void AddStack(int amount)
        {
            Stack += amount;
        }

        public void SetStack(int value)
        {
            Stack = value;
        }

        public void AddTick(int amount = 1)
        {
            Tick += amount;
        }

        public void ResetTick()
        {
            Tick = 0;
        }

        public int GetCounter(string key)
        {
            return !string.IsNullOrWhiteSpace(key) && counters.TryGetValue(key, out var value) ? value : 0;
        }

        public void SetCounter(string key, int value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            counters[key] = value;
        }

        public void AddCounter(string key, int amount)
        {
            SetCounter(key, GetCounter(key) + amount);
        }
    }
}
