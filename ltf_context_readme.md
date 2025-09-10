# LTFCatalog Context Repository

This repository contains documented solutions and patterns for the LTFCatalog Blazor Server application to prevent rework and maintain institutional knowledge.

## Solutions

### [Subsite Routing - Complete Solution](./Subsite%20Routing%20-%20Complete%20Solution.md)
**Problem:** Authentication redirects and navigation going to root domain instead of subsite  
**Solution:** Systematic fix for absolute paths, middleware configuration, and IIS setup  
**Date:** September 2025  

## Repository Structure

```
/Solutions/          # Major problems solved with step-by-step solutions
/Patterns/           # Reusable code templates and configurations  
/Architecture/       # High-level design decisions and rationale
/Gotchas/           # Common mistakes and how to avoid them
```

## Usage Guidelines

### When to Add Content
- After solving any significant technical problem
- When establishing new architectural patterns
- After discovering configuration "gotchas"
- When creating reusable code templates

### Content Format
- Use descriptive filenames with spaces (GitHub handles them fine)
- Include problem summary, solution steps, and prevention guidance
- Add date and context for future reference
- Include code examples and configuration snippets

### Maintenance
- Update existing documents when solutions evolve
- Add cross-references between related solutions
- Keep a running list of common issues in this README

## Quick Reference

### Current Tech Stack
- **Framework:** ASP.NET Core Blazor Server
- **Database:** SQL Server with Entity Framework Core
- **Authentication:** ASP.NET Core Identity
- **Hosting:** IIS subsite configuration
- **Languages:** C#, SQL Server T-SQL, PowerFX (for future PowerApps integration)

### Key Configuration Patterns
- Subsite routing requires systematic elimination of absolute paths
- Middleware order is critical for authentication
- IIS rewrite rules needed for subsite deployments

---

*This repository implements the Context Repo pattern - capturing solutions to prevent rework and build institutional knowledge for single-developer and small-team projects.*