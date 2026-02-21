# Account Report Template Placeholders

## Overview
The Account Report template system allows users to customize the layout of account statements while data is automatically loaded from the database.

## Available Placeholders

### Account Information
- `#AccountNo#` - Account ID/Number
- `#Guardian#` - Guardian/Account Name (ولي الأمر)
- `#CreatedDate#` - Account creation date (formatted)

### Financial Summary
- `#TotalDebit#` - Total debt amount (إجمالي المديونية)
- `#TotalCredit#` - Total payments/credits (إجمالي المدفوعات)
- `#Balance#` - Current balance (الرصيد)

### School Information
- `#SchoolName#` - School name
- `#SchoolAddress#` - School address
- `#SchoolPhone#` - School phone number
- `#SchoolLogo#` - School logo URL

### Transactions Table
- `#TransactionsTable#` - Complete HTML table rows for all transactions
  - This placeholder will be replaced with formatted table rows
  - Each row includes: #, Description (البند), Type (النوع), Amount (المبلغ), Date (التاريخ)

### Savings/Deposits Table
- `#SavingsTable#` - Complete HTML table rows for all savings/deposits
  - This placeholder will be replaced with formatted table rows
  - Each row includes: #, Description (الوصف), Type (النوع), Amount (المبلغ), Created Date (تاريخ الإنشاء)
- `#TotalSavings#` - Total amount of all savings/deposits (إجمالي المدخرات)

## Example Template

```html
<div dir="rtl" style="padding:20px; line-height:1.8; font-family:Arial;">
  <div style="display:flex; justify-content:space-between; margin-bottom:20px; border-bottom:2px solid #ccc; padding-bottom:20px;">
    <div style="flex:1; text-align:right;">
      <h2 style="font-size:1.5em; margin-bottom:10px;">رقم الحساب: #AccountNo#</h2>
      <h3 style="font-size:1.2em; margin-bottom:5px;">#Guardian#</h3>
      <p style="color:#666; margin-bottom:10px;">تاريخ الإنشاء: <strong>#CreatedDate#</strong></p>
      
      <div style="margin-top:15px;">
        <div style="display:flex; justify-content:space-between; border-bottom:1px solid #ddd; padding:5px 0;">
          <span>إجمالي المديونية</span>
          <span>YR #TotalDebit#</span>
        </div>
        <div style="display:flex; justify-content:space-between; border-bottom:1px solid #ddd; padding:5px 0;">
          <span>إجمالي المدفوعات</span>
          <span>YR #TotalCredit#</span>
        </div>
        <div style="display:flex; justify-content:space-between; font-weight:bold; padding:5px 0; color:#f97316;">
          <span>الرصيد</span>
          <span>YR #Balance#</span>
        </div>
      </div>
    </div>
    
    <div style="min-width:200px; text-align:left;">
      <img src="#SchoolLogo#" alt="شعار المدرسة" style="height:48px; margin-bottom:10px;" />
      <div style="font-weight:bold; font-size:1.1em; margin-bottom:5px;">#SchoolName#</div>
      <div style="color:#666;">#SchoolAddress#</div>
      <div style="color:#666;">Tel: #SchoolPhone#</div>
    </div>
  </div>
  
  <div style="margin-top:20px;">
    <table style="width:100%; border-collapse:collapse; margin-top:20px;" border="1">
      <thead style="background-color:#f3f4f6;">
        <tr>
          <th style="padding:8px; border:1px solid #ddd; text-align:center;">#</th>
          <th style="padding:8px; border:1px solid #ddd; text-align:center;">البند</th>
          <th style="padding:8px; border:1px solid #ddd; text-align:center;">النوع</th>
          <th style="padding:8px; border:1px solid #ddd; text-align:center;">المبلغ</th>
          <th style="padding:8px; border:1px solid #ddd; text-align:center;">التاريخ</th>
        </tr>
      </thead>
      <tbody>
        #TransactionsTable#
        <tr style="font-weight:bold; background-color:#fef3c7;">
          <td colspan="4" style="text-align:right; padding:8px; border:1px solid #ddd;">إجمالي المديونية:</td>
          <td style="padding:8px; border:1px solid #ddd;">YR #TotalDebit#</td>
        </tr>
      </tbody>
    </table>
  </div>
</div>
```

## How to Customize

1. **Go to Report Customization Page**: Navigate to the "تخصيص التقارير" (Customize Reports) page
2. **Select "Account Report"** from the dropdown
3. **Edit the template** using the rich text editor
4. **Use placeholders** like `#AccountNo#`, `#Guardian#`, etc. where you want data to appear
5. **Save the template** - it will be stored in the database
6. **View the report** - when you open an account report, it will use your customized template

## Data Loading

- Account data is loaded from the database when viewing a report
- The component automatically replaces placeholders with actual data
- Transactions table is generated from account transactions/fees

## Notes

- Use `display:flex` for layouts (supported via CSS)
- All inline styles are preserved
- Spaces are preserved correctly
- The template supports RTL (right-to-left) layout
