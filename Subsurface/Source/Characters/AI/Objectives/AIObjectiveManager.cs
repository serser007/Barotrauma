﻿using Barotrauma.Items.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Barotrauma
{
    class AIObjectiveManager
    {
        const float OrderPriority = 50.0f;

        private List<AIObjective> objectives;

        private Character character;

        private AIObjective currentOrder;
        
        public AIObjective CurrentObjective
        {
            get
            {
                if (currentOrder != null) return currentOrder;
                return objectives.Any() ? objectives[0] : null;
            }
        }

        public AIObjectiveManager(Character character)
        {
            this.character = character;

            objectives = new List<AIObjective>();
        }

        public void AddObjective(AIObjective objective)
        {
            if (objectives.Find(o => o.IsDuplicate(objective)) != null) return;

            objectives.Add(objective);
        }

        public float GetCurrentPriority(Character character)
        {
            if (currentOrder != null) return OrderPriority;
            return (CurrentObjective == null) ? 0.0f : CurrentObjective.GetPriority(character);
        }

        public void UpdateObjectives()
        {
            if (!objectives.Any()) return;

            //remove completed objectives
            objectives = objectives.FindAll(o => !o.IsCompleted());

            //sort objectives according to priority
            objectives.Sort((x, y) => y.GetPriority(character).CompareTo(x.GetPriority(character)));

            if (character.AnimController.CurrentHull!=null)
            {
                var gaps = character.AnimController.CurrentHull.FindGaps();

                foreach (Gap gap in gaps)
                {
                    if (gap.linkedTo.Count > 1) continue;
                    AddObjective(new AIObjectiveFixLeak(gap, character));
                }
            }
        }

        public void DoCurrentObjective(float deltaTime)
        {
            if (currentOrder != null && (!objectives.Any() || objectives[0].GetPriority(character)<OrderPriority))
            {
                currentOrder.TryComplete(deltaTime);
                return;
            }

            if (!objectives.Any()) return;
            objectives[0].TryComplete(deltaTime);
        }

        public void SetOrder(Order order, string option)
        {
            currentOrder = null;

            switch (order.Name.ToLower())
            {
                case "follow":
                    currentOrder = new AIObjectiveGoTo(Character.Controlled, character, true);
                    break;
                default:
                    if (order.TargetItem == null) return;

                    currentOrder = new AIObjectiveOperateItem(order.TargetItem, character, option);

                    break;
            }
        }
    }
}
