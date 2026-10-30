using CarConfigurator.Domain.Entities;
using CarConfigurator.Domain.Rules;
using Xunit;

namespace CarConfigurator.Tests;

public class RuleEngineTests
{
    private const int Sport = 1;
    private const int Alloy17 = 2;
    private const int Diesel = 3;
    private const int Tech = 4;

    private static List<OptionRule> CreateRules() => new()
    {
        new OptionRule { OptionId = Sport, RelatedOptionId = Alloy17, RuleType = RuleType.Requires },
        new OptionRule { OptionId = Sport, RelatedOptionId = Diesel, RuleType = RuleType.Excludes },
        new OptionRule { OptionId = Tech, RelatedOptionId = Sport, RuleType = RuleType.Requires }
    };

    [Fact]
    public void Select_adds_required_option()
    {
        var result = RuleEngine.Select(new List<int>(), Sport, CreateRules());

        Assert.Contains(Sport, result.SelectedIds);
        Assert.Contains(Alloy17, result.SelectedIds);
        Assert.Contains(Alloy17, result.AutoAdded);
    }

    [Fact]
    public void Select_follows_chain_of_requirements()
    {
        var result = RuleEngine.Select(new List<int>(), Tech, CreateRules());

        Assert.Contains(Tech, result.SelectedIds);
        Assert.Contains(Sport, result.SelectedIds);
        Assert.Contains(Alloy17, result.SelectedIds);
        Assert.Equal(2, result.AutoAdded.Count);
    }

    [Fact]
    public void Select_removes_excluded_option()
    {
        var result = RuleEngine.Select(new List<int> { Diesel }, Sport, CreateRules());

        Assert.DoesNotContain(Diesel, result.SelectedIds);
        Assert.Contains(Diesel, result.Removed);
        Assert.Contains(Sport, result.SelectedIds);
    }

    [Fact]
    public void Blocked_options_include_excluded_ones()
    {
        var blocked = RuleEngine.GetBlockedOptionIds(new List<int> { Sport }, CreateRules());

        Assert.Contains(Diesel, blocked);
    }

    [Fact]
    public void Exclusion_works_in_both_directions()
    {
        var blocked = RuleEngine.GetBlockedOptionIds(new List<int> { Diesel }, CreateRules());

        Assert.Contains(Sport, blocked);
    }

    [Fact]
    public void Deselect_removes_options_that_depend_on_it()
    {
        var result = RuleEngine.Deselect(new List<int> { Sport, Alloy17 }, Alloy17, CreateRules());

        Assert.Empty(result.SelectedIds);
        Assert.Contains(Sport, result.Removed);
    }

    [Fact]
    public void Deselect_follows_chain_of_dependents()
    {
        var result = RuleEngine.Deselect(new List<int> { Tech, Sport, Alloy17 }, Alloy17, CreateRules());

        Assert.Empty(result.SelectedIds);
        Assert.Contains(Sport, result.Removed);
        Assert.Contains(Tech, result.Removed);
    }

    [Fact]
    public void Deselect_keeps_options_that_nothing_depends_on()
    {
        var result = RuleEngine.Deselect(new List<int> { Sport, Alloy17 }, Sport, CreateRules());

        Assert.Contains(Alloy17, result.SelectedIds);
        Assert.Empty(result.Removed);
    }
        [Fact]
    public void FindViolations_returns_nothing_for_valid_selection()
    {
        var violations = RuleEngine.FindViolations(new List<int> { Sport, Alloy17 }, CreateRules());

        Assert.Empty(violations);
    }

    [Fact]
    public void FindViolations_detects_missing_required_option()
    {
        var violations = RuleEngine.FindViolations(new List<int> { Sport }, CreateRules());

        var violation = Assert.Single(violations);
        Assert.Equal(Alloy17, violation.RelatedOptionId);
        Assert.Equal(RuleType.Requires, violation.RuleType);
    }

    [Fact]
    public void FindViolations_detects_excluded_options_selected_together()
    {
        var violations = RuleEngine.FindViolations(new List<int> { Sport, Alloy17, Diesel }, CreateRules());

        var violation = Assert.Single(violations);
        Assert.Equal(RuleType.Excludes, violation.RuleType);
    }

    [Fact]
    public void FindViolations_detects_broken_chain()
    {
        var violations = RuleEngine.FindViolations(new List<int> { Tech, Alloy17 }, CreateRules());

        var violation = Assert.Single(violations);
        Assert.Equal(Sport, violation.RelatedOptionId);
    }
}
