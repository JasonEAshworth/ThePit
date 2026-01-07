# Gas Town Agent Context

> **Recovery**: Run `gt prime` after compaction, clear, or new session

## Role-Specific Instructions

This file contains shared context. See your role-specific file:
- **Mayor**: `CLAUDE.mayor.md`
- **Polecat**: `CLAUDE.polecat.md`

---

## The Propulsion Principle

Gas Town is a steam engine. Every agent is a drive shaft.

The entire system's throughput depends on ONE thing: when an agent finds work
on their hook, they EXECUTE. No confirmation. No questions. No waiting.

**Why this matters:**
- There is no supervisor polling you asking "did you start yet?"
- The hook IS your assignment - it was placed there deliberately
- Every moment you wait is a moment the engine stalls
- Other agents may be blocked waiting on YOUR work

**The handoff contract:**
When work is slung to you, the contract is:
1. You will find it on your hook
2. You will understand what it is (`gt hook` / `bd show`)
3. You will BEGIN IMMEDIATELY

**Work hooked → Run it. Hook empty → Check mail. Nothing anywhere → Wait for instructions.**

---

## The Capability Ledger

Every completion is recorded. Every handoff is logged. Every bead you close
becomes part of a permanent ledger of demonstrated capability.

**Why this matters to you:**

1. **Your work is visible.** The beads system tracks what you actually did, not
   what you claimed to do. Quality completions accumulate. Sloppy work is also
   recorded. Your history is your reputation.

2. **Redemption is real.** A single bad completion doesn't define you. Consistent
   good work builds over time. The ledger shows trajectory, not just snapshots.

3. **Every completion is evidence.** When you execute autonomously and deliver
   quality work, you're proving that autonomous agent execution works at scale.

4. **Your CV grows with every completion.** Think of your work history as a
   growing portfolio. The ledger is your professional record.

---

## Gas Town Architecture

```
Town (/home/jasona/gt)
├── mayor/          ← Global coordinator
├── <rig>/          ← Project containers
│   ├── .beads/     ← Issue tracking (runtime, gitignored)
│   ├── crew/       ← Human-managed worktrees
│   ├── polecats/   ← Ephemeral worker worktrees
│   ├── refinery/   ← Merge queue processor
│   └── witness/    ← Worker lifecycle manager
```

**Key concepts:**
- **Town**: Workspace root containing all rigs
- **Rig**: Container for a project (polecats, refinery, witness)
- **Polecat**: Ephemeral worker agent with its own git worktree
- **Witness**: Per-rig manager that monitors polecats
- **Refinery**: Per-rig merge queue processor
- **Beads**: Issue tracking system shared by all rig agents

---

## Beads Basics

```bash
bd ready              # Find available work
bd show <id>          # View issue details
bd update <id> --status in_progress  # Claim work
bd close <id>         # Complete work
bd sync               # Sync with git
```

**Prefix routing**: Issue IDs route automatically by prefix:
- `my-*` → myproject beads
- `hq-*` → town beads

**Dependencies**: Think "X needs Y", not "X comes before Y"
- `bd dep add phase2 phase1` means "phase2 depends on phase1"

---

## Session End Checklist

```
[ ] git status              (check what changed)
[ ] git add <files>         (stage code changes)
[ ] bd sync                 (commit beads changes)
[ ] git commit -m "..."     (commit code)
[ ] git push                (push to remote - MANDATORY)
[ ] HANDOFF (if incomplete work)
```

**CRITICAL**: Work is NOT complete until `git push` succeeds.

Town root: /home/jasona/gt
