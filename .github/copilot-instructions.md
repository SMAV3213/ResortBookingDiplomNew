# Copilot Instructions

## General Guidelines
- API controller responses should return clear text messages instead of structured ApiResponse.
- Prefer using nullable reference types on DTOs for request binding so FluentValidation can run and enforce NotEmpty rules.