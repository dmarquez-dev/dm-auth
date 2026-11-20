<!-- source: sir-sdlc disciplines/test.md -->

# Test discipline

Rules are binding. Where one conflicts with a frozen contract, the contract wins.

## Behavior over implementation

- A test asserts what the unit does, not how it does it. A test that breaks on a rename that changed no behavior was testing the wrong thing.
- Assert on returned values and observable effects, never on private state.
- A mock stands in for a boundary the test cannot cross. Mocking the thing under test asserts only that the mock was configured.
- A test that would still pass with the implementation deleted is not a test.

## Naming

- A test name states the condition and the expected outcome, so a failure is legible without opening the file.
- A name that says "works" or "is correct" names nothing.
- The name must not claim more than the test checks. A name broader than its assertions misleads every later reader.

## Isolation

- Tests do not share mutable state, and they pass in any order.
- Each test creates what it needs and leaves nothing behind. A test depending on another's leftovers fails alone.
- No test depends on the wall clock, the ambient time zone, the network, or a fixed port.

## Determinism

- No sleeping. Wait on the condition, with a timeout, or make the operation synchronous for the test.
- Randomness is seeded, and the seed is reported on failure.
- A test that fails intermittently is treated as a failing test, not as noise to re-run.

## Coverage

- Every acceptance criterion has a test that would fail if the criterion were unmet.
- A test is shown able to fail — seen failing before it is made to pass, or shown to catch the defect it exists for when that defect is injected. Either establishes it; a test never observed failing has not been shown to test anything, and one whose failing input cannot occur tests nothing at all.
- The failure path is tested, not only the success path.
- A bug fix arrives with the test that reproduces it.
