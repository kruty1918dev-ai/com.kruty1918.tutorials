# Changelog

## 0.1.2 — 2026-10-04

- Include stable Unity metadata for package documentation and license.
- Use the package manifest importer; clean immutable UPM installs need no generated metadata.

## 0.1.1 — 2026-10-04

- Move standalone .NET tooling under `Tools~` so Unity excludes it from asset import.
- Keep UPM imports free from immutable-folder missing-meta warnings.

## 0.1.0 — 2026-10-04

- Pure C# tutorial flow with stable step IDs, evaluated predicates and serializable progress.
- Skip/resume preserves earned steps; skipping never grants the completion reward.
- Replaceable presenters, dynamic target registry and idempotent reward boundary.
- Unity adapter, sample, onboarding documentation, EditMode tests and standalone core checks.
