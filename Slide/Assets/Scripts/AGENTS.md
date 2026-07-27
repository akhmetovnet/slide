# AGENTS.md — Slide

## Project context

- Engine: Unity 6.
- Language: C#.
- Target: mobile devices, portrait orientation.
- Visual style: pixel art.
- Genre/core loop: the player controls a robot descending through a vertical level, using taps and interacting with platforms, hazards, abilities, and mission progression.
- Preserve existing gameplay, balance, save compatibility, and content unless the current task explicitly requires a change.

## Instruction priority

1. Follow the current task and its acceptance criteria.
2. Follow this file for stable project rules.
3. Follow established patterns in the nearest relevant code, prefab, scene, and configuration.
4. When requirements conflict or a product decision is missing, do not invent behavior. Report the conflict or ask one focused question.

## Scope and token efficiency

- Start with the files and directories named in the task.
- Inspect the smallest relevant area before expanding the search.
- Do not scan the entire repository when a targeted search is sufficient.
- Do not repeatedly reread large files or logs already inspected.
- Read only the relevant portions of specifications, logs, and generated reports.
- Never inspect or modify generated Unity directories unless explicitly requested:
  - `Library/`
  - `Temp/`
  - `Logs/`
  - `obj/`
  - `Build/`
  - `Builds/`
  - `UserSettings/`
- Ignore binary assets that are unrelated to the current task.
- Do not produce long explanations, tutorials, or a step-by-step work diary.

## Implementation rules

- Prefer the smallest complete change that satisfies the acceptance criteria.
- Reuse existing architecture, components, prefabs, ScriptableObjects, services, pools, and configuration systems.
- Do not create a parallel system when an existing system can be extended safely.
- Do not perform unrelated refactoring, renaming, formatting, or file reorganization.
- Do not add packages, plugins, SDKs, or production dependencies without explicit approval.
- Do not change public APIs, serialized field names, save-data schemas, analytics events, or identifiers without explicit need.
- Preserve backward compatibility with existing scenes, prefabs, saves, and content.
- Do not hardcode mission numbers, balancing values, asset paths, or content mappings when the project already uses configuration data.
- Keep tunable gameplay and visual values configurable through the project's existing configuration approach.
- Follow naming, namespace, folder, and code-style conventions found in the nearest relevant files.
- Avoid speculative abstractions. Add interfaces, base classes, services, or generic frameworks only when the task requires them.
- Avoid per-frame allocations and repeated scene searches in runtime code.
- Do not use `Find`, `FindObjectOfType`, repeated `GetComponent`, LINQ allocations, or string construction in hot paths when an existing cached reference or event-driven approach is available.
- Use object pooling for frequently spawned gameplay objects and VFX when the project already supports pooling.
- Unsubscribe events and cancel coroutines, tweens, or async work when objects are disabled or destroyed.

## Unity scenes, prefabs, and serialized data

- Treat scene, prefab, animation, material, and ScriptableObject changes as high-risk.
- Modify only assets required by the current task.
- Preserve existing references, overrides, sorting, layers, tags, colliders, and prefab connections unless the task explicitly changes them.
- Do not replace working prefab logic with scene-specific code.
- Do not create duplicate managers or persistent objects.
- Do not edit `ProjectSettings/`, `Packages/manifest.json`, input configuration, render pipeline settings, build settings, or Addressables settings without explicit approval.
- After changing serialized fields, verify that existing prefabs and scenes do not receive missing references or reset values.

## Pixel-art and asset rules

When importing or configuring pixel-art assets, follow the settings used by comparable existing assets. Unless the project uses different established settings:

- Texture Type: Sprite.
- Filter Mode: Point.
- Mip Maps: disabled.
- Compression and Pixels Per Unit: match comparable project assets.
- Preserve transparency and pixel alignment.
- Do not compensate for incorrect import settings with arbitrary Transform scaling.
- Reference images and source files must not be included in runtime content unless explicitly required.
- Visual effects must not change gameplay hitboxes or damage logic.

## Gameplay and game-design constraints

- A specification describes intended behavior; existing production code describes integration constraints. Satisfy both where possible.
- Do not silently choose game-design behavior for undefined states.
- Preserve difficulty, probabilities, speeds, rewards, cooldowns, damage, and spawning rules unless they are part of the task.
- Separate visual replacement from gameplay changes.
- Handle restart, pause/resume, object reuse, mission completion/failure, and repeated entry into the same screen when relevant.
- For any stateful feature, consider duplicate input, interrupted transitions, missing configuration, repeated initialization, and pooled-object reset.

## UI rules

- Preserve portrait layouts and supported aspect ratios.
- Respect safe areas and existing anchors.
- Do not add input-blocking graphics unintentionally.
- Do not change localization keys or user-facing text outside the task.
- Reuse existing UI components and navigation patterns.

## Monetization, saves, and analytics

- Do not modify IAP, ads, pricing, economy, save migration, consent, attribution, or analytics without explicit task requirements.
- Never log secrets, tokens, purchase receipts, personal data, or device identifiers.
- Any save-data change must include compatibility and migration handling approved by the task.

## Verification

- Run the smallest relevant validation first.
- Prefer targeted compilation, existing focused tests, and checks for changed systems.
- Do not run a full mobile build, all test suites, or expensive project-wide validation unless requested or necessary to prove correctness.
- Do not invent test commands. Use commands documented in the repository or already used by the project.
- When Unity or required external tools are unavailable, perform static validation and clearly state what was not executed.
- Check modified assets and code for:
  - compile errors;
  - missing references;
  - null-state handling;
  - duplicate initialization;
  - incorrect pooled-object state;
  - regression risk to existing content.

## Working with specifications

- Treat attached or repository specifications as task context, not permission to change unrelated systems.
- Extract only requirements relevant to the current implementation step.
- Before implementation, identify contradictions or blockers that materially affect behavior.
- Do not rewrite the specification unless requested.
- For large specifications, implement in isolated stages rather than changing every subsystem in one pass.

## Git and existing work

- Do not revert, overwrite, or clean up unrelated user changes.
- Do not amend history, force-push, delete branches, or run destructive Git commands.
- Keep the diff limited to the requested feature.
- Do not commit unless explicitly requested.

## Final response format

Keep the final response concise. Do not paste complete files unless requested.

Report only:

1. Result.
2. Changed files or assets.
3. Validation performed and its outcome.
4. Remaining risks, blockers, or manual Unity steps.

Use no more than 10–12 lines unless a failure requires additional detail.
