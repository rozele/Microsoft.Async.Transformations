# Contributing Guide

## System Requirements
  * Visual Studio 2015 with Universal Tools
  
## Style Guide

## Pull Requests

### Style Guide
Err on the side of consistency, but where there doesn't seem to be a pattern to follow, consult the [Framework Design Guidelines](https://msdn.microsoft.com/en-us/library/ms229042(v=vs.110).aspx) or [C# Coding Conventions](https://msdn.microsoft.com/en-us/library/ff926074.aspx).

### Unit Tests
We use the Visual Studio Unit Test Framework, try to keep the code coverage up, and obviously test any new features rigorously.

### Don't Break the Build
We use AppVeyor to run the unit tests created thus far. No pull requests will be accepted if they break the unit tests.

## Cleaning-up History

Please make sure to squash commits and rebase if appropriate.
