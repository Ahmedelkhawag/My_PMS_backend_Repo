# Business Requirements Document (BRD) - GL Module (Full Version: Phase 1-3)

## 1. Objective
To provide a secure, automated, and highly analytical financial system that supports hotel management decision-making through multi-dimensional reporting.

## 2. Functional Requirements

### 2.1 Analytical Accounting (Cost Centers - Phase 3)
- **Goal:** Track profitability per department (F&B, Rooms, Spa).
- **Rule:** Every expense or revenue transaction MUST be tagged with a `CostCenterId`.
- **Hierarchy:** Cost centers follow a parent-child structure for aggregate reporting (e.g., "Main Kitchen" is under "F&B Department").

### 2.2 Financial Output (Phase 3)
- **Profit & Loss (P&L):** Dynamic report showing income vs. expenses to calculate Net Profit/Loss.
- **Balance Sheet:** Real-time view of Assets, Liabilities, and Equity.
- **Maker-Checker Integrity:** Only approved (Posted) transactions affect these final financial statements.

### 2.3 Integration & Control (Phase 1 & 2)
- **FO Integration:** Automated posting from Front Office transactions with proper tax splitting.
- **Fiscal Control:** Posting is restricted to "Open" periods only.
- **Currency:** Support for transactions in USD/EUR with historical rate locking.

## 3. Financial Integrity & Compliance
- **Double-Entry:** The system strictly enforces `Debit = Credit` on the Base Amount.
- **Audit Trail:** Full transparency on who created, edited, or approved any financial entry.