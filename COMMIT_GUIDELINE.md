# Commit Guideline for NCC ERP Workflow W2 Backend

This project follows the **Conventional Commits** standard for commit messages. Follow this guide to ensure your commits are consistent and meaningful.

---

## 1. Commit Message Format

Commit messages must follow the [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) standard:

```
<type>(W2-<ticket-number>): <description>
```

### Examples:
- `feat(W2-123): add workflow approval logic`
- `fix(W2-456): resolve database connection issue`
- `docs(W2-789): update backend setup instructions`
- `refactor(W2-101): improve query performance`
- `test(W2-112): add unit tests for WorkflowInstanceAppService`

### Allowed Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, missing semi-colons, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks
- `revert`: Reverting a previous commit
- `BREAKING CHANGE`: Introduces a breaking change

### Rules:
- **Scope**: Must always follow the format `W2-<ticket-number>` (e.g., `W2-123`).
- **Subject**: Must be in lowercase (e.g., `add workflow approval logic`).

---

## 2. How to Commit

1. **Stage your changes**:
   ```bash
   git add .
   ```

2. **Commit with a proper message**:
   ```bash
   git commit -m "feat(W2-123): add workflow approval logic"
   ```

3. **If commit fails due to message format**:
   - Amend the commit message:
     ```bash
     git commit --amend -m "fix(W2-456): resolve database connection issue"
     ```
   - Push with `--force` if necessary:
     ```bash
     git push --force
     ```

---

## 3. Notes

- Always follow the commit message format to avoid confusion in the commit history.
- Ensure your commit message is meaningful and reflects the changes made.
- If you are unsure about the ticket number, check the issue tracker or ask your team.

---

**Following these guidelines ensures clean commit history and smooth collaboration!**