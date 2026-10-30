using CarConfigurator.Domain.Entities;

namespace CarConfigurator.Domain.Rules;

public record SelectionResult(HashSet<int> SelectedIds, List<int> AutoAdded, List<int> Removed);

public static class RuleEngine
{
    public static HashSet<int> GetBlockedOptionIds(
        IEnumerable<int> selected, IEnumerable<OptionRule> rules)
    {
        var selectedSet = selected.ToHashSet();
        var blocked = new HashSet<int>();

        foreach (var rule in rules.Where(r => r.RuleType == RuleType.Excludes))
        {
            if (selectedSet.Contains(rule.OptionId)) blocked.Add(rule.RelatedOptionId);
            if (selectedSet.Contains(rule.RelatedOptionId)) blocked.Add(rule.OptionId);
        }

        return blocked;
    }

    public static SelectionResult Select(
        IEnumerable<int> currentSelection, int optionId, IEnumerable<OptionRule> rules)
    {
        var ruleList = rules.ToList();
        var selected = currentSelection.ToHashSet();
        var autoAdded = new List<int>();
        var removed = new List<int>();

        foreach (var rule in ruleList.Where(r => r.RuleType == RuleType.Excludes))
        {
            if (rule.OptionId == optionId && selected.Remove(rule.RelatedOptionId))
                removed.Add(rule.RelatedOptionId);
            if (rule.RelatedOptionId == optionId && selected.Remove(rule.OptionId))
                removed.Add(rule.OptionId);
        }

        selected.Add(optionId);

        var queue = new Queue<int>();
        queue.Enqueue(optionId);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var rule in ruleList.Where(r => r.RuleType == RuleType.Requires && r.OptionId == current))
            {
                if (selected.Add(rule.RelatedOptionId))
                {
                    autoAdded.Add(rule.RelatedOptionId);
                    queue.Enqueue(rule.RelatedOptionId);
                }
            }
        }

        return new SelectionResult(selected, autoAdded, removed);
    }

    public static SelectionResult Deselect(
        IEnumerable<int> currentSelection, int optionId, IEnumerable<OptionRule> rules)
    {
        var requiresRules = rules.Where(r => r.RuleType == RuleType.Requires).ToList();
        var selected = currentSelection.ToHashSet();
        var removed = new List<int>();

        var queue = new Queue<int>();
        queue.Enqueue(optionId);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!selected.Remove(current)) continue;
            if (current != optionId) removed.Add(current);

            foreach (var rule in requiresRules.Where(r => r.RelatedOptionId == current))
                queue.Enqueue(rule.OptionId);
        }

        return new SelectionResult(selected, new List<int>(), removed);
    }
        public static List<OptionRule> FindViolations(
        IEnumerable<int> selected, IEnumerable<OptionRule> rules)
    {
        var selectedSet = selected.ToHashSet();
        var violations = new List<OptionRule>();

        foreach (var rule in rules)
        {
            if (rule.RuleType == RuleType.Requires)
            {
                if (selectedSet.Contains(rule.OptionId) && !selectedSet.Contains(rule.RelatedOptionId))
                    violations.Add(rule);
            }
            else if (rule.RuleType == RuleType.Excludes)
            {
                if (selectedSet.Contains(rule.OptionId) && selectedSet.Contains(rule.RelatedOptionId))
                    violations.Add(rule);
            }
        }

        return violations;
    }
}