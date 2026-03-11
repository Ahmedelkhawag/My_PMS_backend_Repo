# Functional Specification Document (FSD): AR Module - Stage 1
**Project:** Hotel Property Management System (PMS)
**Module:** Accounts Receivable (AR) - Phase 1: Advance Deposits & Credit Control

---

## 1. Introduction
The objective of this module is to safeguard the hotel's cash flow by managing company debts, enforcing credit limits, and handling advance payments that are not initially tied to specific invoices.

## 2. Functional Requirements

### 2.1. Advance Deposits (Unallocated Payments)
- **Description:** Allow accountants to post payments from Travel Agents or Corporate clients before any stay occurs or invoices are generated.
- **Workflow:**
    1. Accountant selects a Company and enters payment details (Amount, Method, Date).
    2. The system flags the payment as an "Advance Deposit" (InvoiceId is null).
    3. The amount is added to the company's "Unallocated Balance."
- **Business Rules:**
    - Advance deposits must reflect as a Credit balance in the company's statement.
    - Payments can be partially or fully allocated to one or multiple invoices later.

### 2.2. Credit Management & Control
- **Description:** A defensive mechanism to prevent excessive debt by checking company eligibility before allowing "Transfer to AR" during checkout.
- **Key Features:**
    - **Credit Limit:** Maximum allowable debt for a specific company.
    - **Credit Days:** Grace period before an invoice is considered "Overdue."
- **Validation Logic:**
    - System blocks any transfer if:
        1. (Current Debt + New Invoice) > Credit Limit.
        2. Any existing invoice has passed its "Credit Days" limit (Aging Check).

### 2.3. Payment Allocation
- **Description:** A dedicated interface to link "Unallocated Payments" to "Open Invoices."
- **Rules:**
    - Allocation cannot exceed the available payment amount.
    - Allocation cannot exceed the remaining invoice balance.
    - Voiding a payment must automatically roll back all associated allocations.

## 3. User Personas & Permissions
- **Accountant:** Can post payments and perform allocations.
- **FO Receptionist:** Receives "Credit Limit Exceeded" alerts; cannot override blocks.
- **Finance Manager:** Can set credit limits and provide "Override Tokens" to bypass blocks.

## 4. Expected UI Fields
- **Company Profile:** `Credit Limit (Decimal)`, `Credit Days (Int)`, `Is Credit Allowed (Bool)`.
- **Payment Screen:** `Is Deposit (Checkbox)`, `Unallocated Amount (Display Only)`.