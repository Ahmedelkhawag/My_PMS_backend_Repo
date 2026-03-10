# Functional Specification Document (FSD) - GL Module (Full Version: Phase 1-3)

## 1. Overview
This document defines the technical structure of the General Ledger (GL) module, covering foundation, governance, and analytical reporting.

## 2. Data Models (Entities)

### 2.1 Fiscal & Currency (Phase 1)
- **FiscalYear & AccountingPeriod**: To control posting dates and lock closed months.
- **JournalEntryLine (Multi-Currency)**: Includes `CurrencyId`, `ExchangeRate`, `ForeignAmount`, and `BaseAmount`.

### 2.2 Governance & Status (Phase 2)
- **JournalEntry (Status Engine)**:
    - `Status` (Enum): `Draft`, `PendingApproval`, `Posted`, `Rejected`.
    - `ApprovedById`, `ApprovedDate`, `RejectionReason`.
- **JournalEntryMapping**: Supports dynamic splitting based on `Percentage`.

### 2.3 Analysis & Reporting (Phase 3 - NEW)
- **CostCenter (Tree Structure)**:
    - `Id` (int, PK)
    - `Code` (string, Unique)
    - `Name` (string)
    - `ParentCostCenterId` (int, Nullable FK)
    - `IsGroup` (bool)
- **JournalEntryLine (Update)**:
    - `CostCenterId` (int, Nullable FK): Mandatory for Expense and Revenue accounts.

## 3. Core Business Logic

### 3.1 Financial Reporting Engine (Phase 3)
- **Recursive Summation**: Reports (P&L, Balance Sheet) must aggregate balances from child accounts to parent accounts based on the COA hierarchy.
- **Posted-Only Rule**: Financial statements and Trial Balance must only include lines from Journal Entries with `Status == Posted`.
- **Date Range Filters**: All reports must support dynamic date ranges (Start/End).

### 3.2 Year-End Closing Logic
- **Process**: Automates the transfer of net income (Revenues - Expenses) to the "Retained Earnings" account and zeros out all temporary P&L accounts for the new fiscal year.

## 4. API Endpoints
- `GET /api/accounting/reports/pnl`: Profit & Loss statement.
- `GET /api/accounting/reports/balance-sheet`: Balance Sheet.
- `GET /api/accounting/cost-centers/tree`: Hierarchical view of cost centers.