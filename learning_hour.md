---
theme: refactoring
title: Never resolve state you could delegate
kata: shopping_basket
difficulty: 2
author: strudso
tags: refactoring code_smells tell_dont_ask
---

# Never resolve state you could delegate

This learning hour uses Emily Bache's [Shopping Basket Kata](https://github.com/emilybache/ShoppingBasket-TDD-Kata) with a pre-built anti-pattern branch. The starting code is a working implementation where all 11 tests are green — but some tests pass with objects that could never exist in production.

Starting code: [anti_pattern_lacking_delegation](https://github.com/STRUDSO/ShoppingBasket-TDD-Kata/tree/anti_pattern_lacking_delegation) branch (C#/NUnit, ShoppingBasketA project).

## Learning Objectives

* Recognize resolved-property objects — objects that copy state from a collaborator without adding behavior
* Spot tests that pass with "illegal" objects — internally inconsistent state that the real system could never produce
* Practice lifting tests to a behavior boundary before refactoring code

## Session Outline

* 5 min connect: Find the illegal objects
* 10 min concept: The resolve vs delegate smell
* 35 min concrete: Refactor the shopping basket
* 5 min conclusions: When are the tests the blocker?

### Connect - Find the illegal objects

Show `CheckoutServiceTest` from the starting code. All 8 tests are green. Ask people to read them and find the two tests where `BasketSummary` contains values that are **mathematically impossible** — values that `PricingService` could never produce.

Give people two minutes to hunt, then reveal:

```csharp
// Subtotal is 80 but Total is 120 — discount can only reduce, never increase
var summary = new BasketSummary
{
    Subtotal = 80m, Total = 120m, ...
};

// DiscountAmount is 10.00 but 159.94 * 0.05 = 7.997
var summary = new BasketSummary
{
    Subtotal = 159.94m, DiscountPercentage = 0.05m, DiscountAmount = 10.00m, ...
};
```

Both tests are green. Ask: why doesn't anything catch this?

### Concept - The resolve vs delegate smell

Show the architecture on a whiteboard:

```
[ShoppingBasket] --GetItems()--> [PricingService] --resolves--> [BasketSummary] --passed to--> [CheckoutService]
     has data                    has logic                       has neither                    uses derived values
```

Point out three problems:

1. **PricingService resolves state into BasketSummary.** Every property is calculated from the basket and copied into a flat DTO. The DTO has no way to enforce consistency between its own properties.

2. **CheckoutService uses derived values it can't validate.** It receives `BasketSummary` and trusts `Total`, `DiscountPercentage`, etc. It has no access to the basket that produced them. It can't check if they're consistent.

3. **Tests exploit the gap.** Because `CheckoutService` takes a `BasketSummary`, tests construct one directly — bypassing `PricingService` entirely. The test objects can contain any values, including impossible ones. The tests are green, but they prove nothing about the real system.

If you already know Fowler's **Replace Temp with Query** — eliminating a local variable that caches a derived value in favor of calling a method — this is the same smell scaled up across class boundaries. Instead of a temp variable holding a derived value, you have a whole DTO holding five derived values, passed to a consumer that can't validate any of them. `BasketSummary` is the cross-class temp. The fix is the cross-class query: ask the basket directly.

The rule: **if a service receives pre-calculated values that it can't validate, the boundary is in the wrong place.** Push the behavior into the object that owns the data.

### Concrete - Refactor the shopping basket

Clone the [anti_pattern_lacking_delegation](https://github.com/STRUDSO/ShoppingBasket-TDD-Kata/tree/anti_pattern_lacking_delegation) branch. Open `ShoppingBasketA` in your IDE. All 11 tests are green.

Work in pairs. Follow these phases in order — the order matters.

**Phase 1: Lift tests to the behavior boundary** (15 min)

Write new tests that go through the full path — add items to a basket, then assert on what the caller actually cares about. No hand-crafted `BasketSummary` objects.

Test list to work from:

- Empty basket generates a receipt with "Total: $0.00"
- Basket with items totaling $50 does not qualify for free shipping
- Basket with items totaling $160 qualifies for free shipping (total after 5% discount is $151.94)
- Basket with items totaling $250 gets Gold discount tier
- Basket with items totaling $160 gets Silver discount tier
- Basket with items totaling $50 gets no discount tier
- Receipt for a discounted basket shows the discount line

These tests should pass against the current code — you're testing existing behavior, not changing it. Do not change any production code yet.

**Phase 2: Push behavior into ShoppingBasket** (15 min)

1. Add `CalculateTotal()` to `ShoppingBasket` — move the subtotal and discount logic from `PricingService`.
2. Change `CheckoutService` to take a `ShoppingBasket` instead of a `BasketSummary`. It asks the basket for its total, discount tier, etc.
3. Run your Phase 1 tests — they should still pass.
4. Delete the `CheckoutServiceTest` tests that constructed `BasketSummary` directly — especially the two with illegal objects.
5. Delete `PricingService` and `BasketSummary`.

**Phase 3: Clean up** (5 min)

Remove `GetItems()` from `ShoppingBasket` — nothing needs it anymore. Run all tests. Only your Phase 1 behavior tests remain, and they're all green. No illegal objects are possible.

### Conclusions - When are the tests the blocker?

Discuss in pairs:

- Why could the old tests pass with impossible values? What's the structural cause?
- Why did we write new tests before changing any code?
- How do you spot this pattern in your own codebase? What's the smell?

Key takeaway: when a service receives a DTO of pre-resolved values, tests can construct that DTO with any state — including states the real system can never produce. The fix is to push behavior into the object that owns the data, so the impossible states become unrepresentable. But the precondition is: lift the tests first.

## Related Patterns

### Related Learning Hours (sammancoaching.org)

- [Law of Demeter](https://sammancoaching.org/learning_hours/refactoring/law_of_demeter.html) — `basket.GetItems().Sum(x => x.Item.Price * x.Quantity)` is a classic Demeter violation. This learning hour teaches spotting the chain; ours teaches what happens when tests cement it.
- [Example-guided design](https://sammancoaching.org/learning_hours/small_steps/example_guided_design.html) — uses the same Shopping Basket kata but approaches it greenfield with TDD. Good follow-up to show how delegation emerges naturally when you design usage-first.

### Code Smells (sammancoaching.org)

- [Data Class](https://sammancoaching.org/reference/code_smells/data_class.html) — `BasketSummary` is a textbook data class: public getters/setters, no behavior. It exists to hold values that belong elsewhere.
- [Feature Envy](https://sammancoaching.org/reference/code_smells/feature_envy.html) — `PricingService.Calculate()` is envious of `ShoppingBasket`'s data. It calls `GetItems()`, iterates, sums, and applies discounts — all logic that belongs on the basket.
- [Middle Man](https://sammancoaching.org/reference/code_smells/middle_man.html) — `BasketSummary` is a middleman. It adds no value between `PricingService` (which produces the values) and `CheckoutService` (which consumes them). Remove it and let the consumer talk to the source.
- [Shotgun Surgery](https://sammancoaching.org/reference/code_smells/shotgun_surgery.html) — adding a new derived value (e.g., tax) requires changes in `PricingService`, `BasketSummary`, `CheckoutService`, and every test that constructs a summary.
- [Insider Trading](https://sammancoaching.org/reference/code_smells/insider_trading.html) — `PricingService` reaches into the basket's internal list via `GetItems()`. The basket's implementation details leak across the boundary.

### Refactorings (sammancoaching.org)

- [Move Function](https://sammancoaching.org/reference/refactorings/move_function.html) — the core fix: move `CalculateTotal()` from `PricingService` into `ShoppingBasket`, where the data lives.
- [Inline Function](https://sammancoaching.org/reference/refactorings/inline_function.html) — once `BasketSummary` is empty, inline it: callers talk to the basket directly.
- [Encapsulate Variable](https://sammancoaching.org/reference/refactorings/encapsulate_variable.html) — replace `GetItems()` (raw list exposure) with behavior methods on the basket.

### Fowler's Refactoring Catalog ([refactoring.com/catalog](https://refactoring.com/catalog/))

The online catalog gives concise summaries. The book *Refactoring* (2nd edition) is more elaborate — with worked examples, motivation, and mechanics for each refactoring.

- **Replace Temp with Query** — the single-method version of this smell. `BasketSummary` is the cross-class variant: instead of a temp variable caching a derived value, a whole DTO caches five derived values across a class boundary.
- **Inline Class** — the final step: `BasketSummary` has no remaining reason to exist, so absorb it into `ShoppingBasket`.
- **Move Field** — each resolved property (`Subtotal`, `Total`, etc.) moves back to the object that can compute it.
- **Remove Middle Man** — delete `PricingService` once its logic lives on the basket.

### Other References

- [Never resolve state you could delegate](../research/never-resolve-state-you-could-delegate.md) — full chapter in Five Lines of Code style
- [Secondary Adapter Fakes: Modeling Storage Semantics](../secondary-adapter-fakes.md) — the same pattern applied to repository test doubles
- Freeman & Pryce, *GOOS* — Tell Don't Ask, adapter boundaries
- Pragmatic Programmers — Tell, Don't Ask principle
