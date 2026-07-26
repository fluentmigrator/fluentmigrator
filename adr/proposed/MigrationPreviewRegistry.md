# ADR: Migration Preview Registry and Approval Workflows

## Status
Proposed

## Context
The reusable `migration-preview.yml` workflow introduces pull-request schema previews that generate and publish SQL diffs. This improves visibility for migration changes, but review and approval remain tied to pull request comments in a single repository.

Teams running large, distributed database programs need stronger coordination primitives:
- Central visibility across many repositories and services
- Explicit approval gates for risky migration plans
- Safe rollout planning that combines schema diffs with online migration tooling

## Decision
Adopt a roadmap that evolves migration preview output into a cloud registry workflow with approval states and external execution integrations.

The first phase keeps today’s workflow intact and adds metadata that can later be consumed by a registry service. Future phases introduce approval workflows and then optional execution adapters for online migration and virtualization systems.

## Next Steps / Future Work
1. **Preview metadata contract**
   - Define a stable preview payload format (processor, base/head refs, SQL diff, checksums, migration assembly metadata).
   - Version the payload to support backward-compatible evolution.

2. **Cloud registry integration**
   - Publish preview payloads to a registry service keyed by repository, branch, and pull request.
   - Store approval state transitions (`draft` → `reviewed` → `approved`/`rejected`) and audit history.
   - Expose webhook or API callbacks so GitHub checks can enforce approval policies before merge.

3. **Large-team workflow controls**
   - Add role-based approval rules (for example: DBA, service owner, platform owner).
   - Support environment-specific gates (staging vs production) and change windows.
   - Provide registry-level dashboards for cross-repository migration planning.

4. **Execution ecosystem adapters**
   - Evaluate integration with binary log stream replay tools such as [gh-ost](https://github.com/github/gh-ost) for online schema changes.
   - Evaluate integration with data virtualization tools such as [pgroll](https://github.com/xataio/pgroll) for staged migration rollouts.
   - Map preview artifacts to execution plans so the same reviewed plan can be promoted through environments.

## Consequences
### Positive
- Establishes a path from PR-local previews to organization-wide migration governance.
- Enables auditable approval workflows suitable for large engineering organizations.
- Creates clear seams for integrating external online migration and virtualization tools.

### Trade-offs
- Adds architectural complexity (service APIs, storage, auth, policy management).
- Requires schema/version governance for preview payload contracts.
- Introduces operational overhead for teams that do not need centralized approvals.
