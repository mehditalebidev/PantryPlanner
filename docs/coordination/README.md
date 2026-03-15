# Coordination Docs

This folder is the handoff layer between planning and implementation.

## Files

- `BOARD.md` tracks what is ready, active, blocked, in review, and done
- `FEATURES.md` stores feature briefs for implementation slices
- `ROADMAP.md` groups work into stages
- `BUGFIXES.md` tracks recurring bug or hardening work
- `OPERATING_MODEL.md` defines how planners and implementers should use these docs

## Rules

- planners refine work here before implementation starts
- implementers should update status as work moves forward
- keep feature IDs stable once they are referenced in branches, PRs, or the worklog
- if a slice changes the product contract, update shared docs first or alongside the code
