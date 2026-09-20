---
name: unity-shader-validation
description: Validate Unity game changes by running the game briefly, taking three screenshots, and checking for magenta/pink shader failures; repair shader problems and repeat until the view is clean.
metadata:
  short-description: Test Unity changes for broken pink shaders
---

# Unity shader validation

Use this skill for Unity project changes that can affect rendering, materials, prefabs, scenes, shaders, render-pipeline settings, or runtime-generated geometry/materials.

## Required post-change loop

After every material, shader, scene, prefab, rendering, or code change that may affect the game:

1. Start the project's configured game/Play Mode.
2. Let it run for approximately 3 seconds so runtime-created objects and materials appear.
3. During those 3 seconds, capture three screenshots at different moments or camera states. Use the available Unity/editor screenshot mechanism; do not treat a single static editor view as sufficient.
4. Inspect all three screenshots for Unity's magenta/pink error rendering, missing textures, invisible geometry, broken lighting, or other obvious shader failures. Give special attention to large surfaces and objects created at runtime.
5. If substantial pink/magenta or another shader failure is present, stop the run, identify the responsible material/shader/render-pipeline mismatch, make the smallest safe fix, and repeat the full 3-second/three-screenshot loop.
6. Continue until all three screenshots are free of substantial pink/magenta shader failure, or report the exact blocker when the game cannot be run or the problem cannot be fixed safely.

Do not declare the rendering change complete based only on a successful compile. A clean console is not proof that runtime materials render correctly.

## Diagnosis priorities

- Confirm the active render pipeline and quality-level pipeline assignment.
- Check whether the affected material uses a missing, invalid, Built-in, legacy, or incompatible custom shader.
- For URP projects, prefer compatible URP shaders such as `Universal Render Pipeline/Lit` or `Universal Render Pipeline/Unlit`, or convert the material through Unity's URP material conversion workflow.
- Preserve textures, colors, and intended visual behavior when replacing a shader; do not blindly overwrite unrelated materials.
- Inspect the Console and relevant Unity logs for shader compilation and render-pipeline errors.

## Reporting

In the final handoff, state whether the repeated runtime validation passed, how many validation rounds were needed, and mention any remaining limitation or unverified platform.
