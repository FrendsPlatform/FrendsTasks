# Changelog

## [1.0.0] - GeneratedDate

### Added

- Template for Frends Toolkit, 
which will contain common code for system-wide tasks. The initial implementation contains:
  - Interfaces for common classes (`Result`, `Options`, `Error`) that should be implemented in all tasks. (In the future it can be required by FrendsTaskAnalyzer to use them)
  - Validation mechanism that can be used to validate input parameters based on attributes.
  - Commonly used attrubutes for input parameters (`NotEmptyString` and `RequiredIf`).
