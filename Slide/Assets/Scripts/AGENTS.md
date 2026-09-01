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

## Character Menu preview generation

Generate premium Character Menu preview assets for SLIDE by matching the current project style and the existing gameplay skins.

Stable identity and gameplay binding:

- Use `heroIndex` as the stable link between menu preview and gameplay skin.
- Gameplay continues to use `CurrentSkin`, `HeroController.skins`, and the matching `HeroController{heroIndex}.controller`.
- Do not change gameplay binding logic when generating or importing menu preview assets.
- Do not write generation prompts for `hero_00`, `hero_01`, or `hero_02` unless the user explicitly requests those heroes later.

Use gameplay sprites as identity references:

- `Assets/Sprites/NewSprites/player/skin_{heroIndex}_idle.png`
- `Assets/Sprites/NewSprites/player/skin_{heroIndex}_fall.png`
- `Assets/Sprites/NewSprites/player/skin_{heroIndex}_slide.png`

The menu preview must preserve:

- main silhouette;
- armor color palette;
- helmet/head shape;
- shoulder shape;
- arm and leg proportions;
- accent/emissive color;
- theme implied by fall/slide animation frames.

The menu preview should improve:

- readability;
- premium feeling;
- pose;
- material detail;
- lighting;
- purchase appeal;
- unique animated effect identity.

For every new menu hero create:

- `Assets/Sprites/CharacterMenu/Hero_XX/hero_XX_preview.png`
- `Assets/Sprites/CharacterMenu/Hero_XX/hero_XX_fire_pair.png`
- optional future animation sheet: `Assets/Sprites/CharacterMenu/Hero_XX/hero_XX_effect_sheet.png`

Use `XX` as two digits matching gameplay skin index, for example `Hero_03/hero_03_preview.png`.

`hero_XX_preview.png` requirements:

- transparent background;
- full-body character;
- no cropped feet, head, shoulders, or effects;
- no UI, podium, arrows, text, background ring, or menu scene;
- centered composition;
- front-facing or slight 3/4 front-facing pose;
- pixel-art style matching the current Character Menu;
- sharp pixel edges;
- no blur, painterly gradients, 3D render, anime proportions, or sticker style;
- high readability at mobile size;
- larger and more premium than gameplay sprite;
- enough transparent padding around the character;
- whole preview sprite, not separate body parts;
- recommended canvas: square transparent PNG, `1254x1254` or larger;
- import as Unity `Sprite Mode: Single`.

Every hero must have a unique animated effect. The compatibility asset name is `hero_XX_fire_pair.png`, but the effect does not have to be literal fire. It can be flame, plasma, electric arcs, coolant vapor, toxic mist, leaf energy, ember sparks, thruster glow, shadow smoke, crystal particles, or another hero-specific visual.

Effect requirements:

- visually belongs to the hero;
- sits behind or around the character;
- readable when mirrored left/right;
- supports procedural Unity animation: sway, flicker, alpha pulse, squash/stretch, and vertical drift;
- does not cover the face, torso identity, or feet.

Future full animation sheet requirements:

- transparent background;
- 6-8 frames or a 4x4 sheet;
- looping motion;
- consistent silhouette and anchor;
- no background;
- no UI;
- no character body unless explicitly requested.

Generation style rules:

- sci-fi pixel-art mech/robot characters;
- bold black/dark outline;
- controlled limited palette;
- bright emissive accents;
- clean readable armor plates;
- slightly exaggerated shoulders, hands, helmet, and boots;
- premium collectible character look;
- no realistic human faces;
- no soft airbrush art;
- no modern 3D render;
- no cartoon sticker style;
- no anime proportions;
- no background scene.

Quality checklist before accepting a generated character:

- Full body visible, feet included.
- Silhouette matches gameplay `skin_{heroIndex}`.
- Palette matches gameplay skin but is richer.
- Effect is unique to this hero.
- Character reads clearly at small menu scale.
- Transparent background is clean.
- Asset can be imported as Unity `Sprite Mode: Single`.
- `heroIndex` matches folder and filename.

General generation prompt:

```text
Create a premium full-body pixel-art sci-fi robot hero preview for the Unity mobile game SLIDE.

Use the provided gameplay sprites `skin_{INDEX}_idle`, `skin_{INDEX}_fall`, and `skin_{INDEX}_slide` only as identity references. Preserve the small gameplay character's silhouette, armor color palette, helmet/head shape, shoulder/arm/leg identity, and emissive accent color, but redesign it as a larger, cleaner, more desirable Character Menu preview that feels worth buying.

Style: sharp high-quality pixel art, sci-fi mech/robot armor, bold dark outline, readable armor plates, crisp edges, no blur, no painterly rendering, no 3D render, no anime, no background, no UI, no podium, no text.

Composition: centered full-body character, front-facing or slight 3/4 front pose, heroic floating menu pose, complete feet visible, complete head visible, complete shoulders/effects visible, enough transparent padding on all sides.

Output: transparent PNG, square canvas, full character only.

Also create a separate transparent effect asset named `hero_{INDEX}_fire_pair.png`. It does not need to be literal fire, but it must be a unique animated-looking energy effect for this hero. It should work behind or around the character and support Unity procedural animation: flicker, sway, alpha pulse, squash/stretch, vertical drift. Do not include the character in the effect asset.
```

Hero-specific generation prompts:

```text
Hero index: 03.

Reference identity: `skin_3_idle/fall/slide`. The tiny gameplay hero is a cold blue/cyan armored mech with a rounded helmet visor, bulky glowing fists, compact torso, dark inner armor, and strong icy-blue energy accents.

Generate a premium Character Menu preview: an elite cryo-runner mech, compact but powerful, with polished dark steel armor, bright cyan visor, glowing cyan chest core, heavy round energy gauntlets, reinforced boots, and small icy light strips across shoulders and limbs. Keep the silhouette close to the gameplay skin: broad shoulders, glowing fists, helmet dome/visor, dark blue body.

Unique effect: cryo plasma flame. Create a separate `hero_03_fire_pair.png` with blue-white cold flame, frost sparks, small electric ice wisps, vertical flicker, and vapor-like tongues. It should feel like freezing energy, not normal fire.
```

```text
Hero index: 04.

Reference identity: `skin_4_idle/fall/slide`. The gameplay hero is bronze/copper, industrial, with warm orange highlights, heavy arms, square helmet/visor, and rugged armor.

Generate a premium Character Menu preview: a copper industrial mining mech with bronze armor plates, dark joints, warm amber visor, heavy forearms, reinforced shoulders, piston details, heat vents, and orange glowing cracks. Preserve the stocky silhouette and copper palette.

Unique effect: furnace ember aura. Create `hero_04_fire_pair.png` as orange ember-fire with sparks, molten dust, tiny forge particles, and pulsing heat shimmer. It should animate like a furnace breathing behind the character.
```

```text
Hero index: 05.

Reference identity: `skin_5_idle/fall/slide`. The gameplay hero is sleek blue/cyan with bright shoulder pads, narrow limbs, dark navy body, and electric cyan highlights.

Generate a premium Character Menu preview: a fast neon courier mech with slim aerodynamic armor, bright cyan shoulder shells, glowing blue visor, dark navy torso, thin mechanical arms, sharp boots, and clean high-tech panel lines. Make it feel agile and expensive.

Unique effect: neon ion stream. Create `hero_05_fire_pair.png` with cyan ion trails, small electric streaks, sharp blue particles, and looping thruster-like flicker. It should feel fast, clean, and high-voltage.
```

```text
Hero index: 06.

Reference identity: `skin_6_idle/fall/slide`. The gameplay hero has a dark green insect/forest-tech identity, green armor pieces, claw-like hands, organic helmet shape, and brown/copper torso elements.

Generate a premium Character Menu preview: a bio-mechanical jungle hunter robot with dark graphite armor, emerald green plates, insect-like helmet crest, segmented limbs, claw hands, copper-brown core armor, and glowing green organic circuits. Preserve the green insect silhouette and aggressive stance.

Unique effect: toxic bioflame. Create `hero_06_fire_pair.png` with green toxic flame, spores, small leaf-like particles, acid glow, and uneven organic flicker. It should move like living energy, not clean plasma.
```

```text
Hero index: 07.

Reference identity: `skin_7_idle/fall/slide`. The gameplay hero is red, gray, and dark blue, with a skull-like helmet/face area, red shoulder armor, red hands, and segmented mechanical legs.

Generate a premium Character Menu preview: a tactical assault mech with red shoulder armor, dark steel limbs, skull-like visor mask, compact chest plating, red gauntlets, and angular military armor. Make it look dangerous, collectible, and combat-ready while keeping the gameplay palette.

Unique effect: red combat sparks. Create `hero_07_fire_pair.png` with red-orange muzzle-flash sparks, hot exhaust wisps, small explosive embers, and sharp flickering pulses. It should feel aggressive and mechanical.
```

```text
Hero index: 08.

Reference identity: `skin_8_idle/fall/slide`. The gameplay hero has dark purple/navy armor, green shoulders and hands, a visor/helmet with green glow, and a stealthy toxic-tech look.

Generate a premium Character Menu preview: a stealth toxin operative mech with dark violet armor, green shoulder pods, glowing green visor, lean limbs, small poison vials or vents integrated into armor, and neon green hand emitters. Preserve the dark-purple plus green identity.

Unique effect: poison shadow flame. Create `hero_08_fire_pair.png` with green toxic smoke, violet shadow wisps, small bubbles/particles, and slow serpentine flicker. It should feel stealthy and poisonous.
```

```text
Hero index: 09.

Reference identity: `skin_9_idle/fall/slide`. The gameplay hero is red/orange with a bright fiery helmet/core, dark blue limbs, and hot glowing armor accents.

Generate a premium Character Menu preview: a volcanic striker mech with red armor, orange molten visor, bright chest core, dark navy mechanical limbs, heat-resistant plates, and glowing magma seams. Preserve the hot red-orange identity and compact armored body.

Unique effect: magma fire. Create `hero_09_fire_pair.png` with orange-red flames, lava sparks, molten droplets, and strong heat flicker. This should be the most classic fire animation, intense and sellable.
```

```text
Hero index: 10.

Reference identity: `skin_10_idle/fall/slide`. The gameplay hero has a bright green vertical helmet/crest, dark body, orange shoulder/torso accents, and green hands.

Generate a premium Character Menu preview: a neon reactor mech with tall green crystal-like helmet crest, black armor, orange-gold shoulder plates, green glowing hands, compact reactor core, and sharp sci-fi paneling. Keep the green crest and orange accent identity very clear.

Unique effect: reactor plasma. Create `hero_10_fire_pair.png` with green plasma tongues, orange reactor sparks, small unstable energy particles, and rhythmic pulsing. It should feel radioactive and high-value.
```

```text
Hero index: 11.

Reference identity: `skin_11_idle/fall/slide`. The gameplay hero is dark blue/gray with a bright cyan horned or crown-like helmet, blue hands, and icy/electric highlights.

Generate a premium Character Menu preview: an elite storm sentinel mech with dark slate armor, bright cyan crown-like helmet fins, glowing blue visor, silver mechanical forearms, blue energy hands, and refined angular armor plates. Preserve the horned cyan head silhouette.

Unique effect: storm lightning aura. Create `hero_11_fire_pair.png` with blue electric arcs, cold plasma wisps, small lightning particles, and fast flickering branches. It should animate like electricity wrapping behind the character.
```

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
