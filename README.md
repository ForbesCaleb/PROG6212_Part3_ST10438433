# PROG6212\_Part3\_ST10438433



\# CMCS – Claim Management \& Coordination System  

\### README Documentation



\## Overview

The Claim Management \& Coordination System (CMCS) is an ASP.NET Core MVC web application designed to manage lecturer claims, automate calculations, enforce approval workflows, and provide dashboard tools for different roles including Lecturers, Programme Coordinators (PC), Academic Managers (AM), and HR.



The system integrates fully with SQL Server using Entity Framework Core and includes automatic hourly‑rate calculations, two‑step claim approval, invoice generation for HR, and secure file storage.



---



\## Application Roles



\### \*\*1. Lecturer\*\*

Lecturers can:

\- Submit claims (hours, activity, notes, documents)

\- View their submitted, approved, and rejected claims

\- See automated hourly rate and total calculation

\- Track claim statuses: Submitted → Under Review → Approved/Rejected



\### \*\*2. Programme Coordinator (PC)\*\*

PCs can:

\- Review lecturer claims

\- Approve or reject the first stage of the workflow

\- Their approval changes claim status to \*\*Under Review\*\*



\### \*\*3. Academic Manager (AM)\*\*

AMs can:

\- Provide the second approval

\- A claim only becomes \*\*Approved\*\* when PC \*\*AND\*\* AM approve



\### \*\*4. HR\*\*

HR has full administrative control:

\- Manage users (create, edit, delete)

\- View system metrics (total users, claims, lecturers)

\- Access invoice summary (grouped totals per lecturer)

\- Drill down into each lecturer’s claim history



---



\## Controllers Overview



\### \*\*DashboardController\*\*

Handles:

\- Role‑based dashboards

\- Submit Claim (GET/POST)

\- My Claims page

\- Auto‑loading hourly rate from user claims

\- Ensures correct total calculation: Hours × HourlyRate

\- Prevents exceeding 180 hours per month



\### \*\*HrController\*\*

Handles:

\- User management (CRUD)

\- Invoice summary

\- Lecturer claim drill‑down

\- Displays grouped totals for each lecturer



\### \*\*AuthController\*\*

Handles:

\- Login and logout

\- Authentication cookie creation

\- Attaches user claims (Id, Role, HourlyRate)



\### \*\*ManagementController / PC/AM logic\*\*

Handles:

\- Two‑step approval

\- Coordinators mark claim as CoordinatorApproved

\- Managers mark claim as ManagerApproved

\- System automatically updates final status when both approve



---



\## Database Models



\### \*\*AppUser\*\*

Represents system users. Fields include:

\- Id, Username, FullName

\- Role (Lecturer, PC, AM, HR)

\- HourlyRate (for lecturers)

\- Password (plain text for demo)

\- Navigation property: Claims



\### \*\*Claim\*\*

Represents lecturer claim submissions:

\- DateWorked, HoursWorked, Activity

\- HourlyRate (loaded from user)

\- TotalAmount auto‑calculated

\- Status: Submitted, UnderReview, Approved, Rejected

\- CoordinatorApproved + ManagerApproved determine final approval

\- Navigation: List<ClaimDocument>



\### \*\*ClaimDocument\*\*

Stores document metadata:

\- FileName

\- RelativePath

\- UploadDate



\### \*\*LecturerInvoiceSummary\*\*

ViewModel used for HR invoice pages:

\- LecturerName, TotalHours, TotalAmount



---



\## File Storage

The system uses `IFileStorage` + `LocalFileStorage` to store uploaded files.



Features:

\- Saves documents into a safe upload directory

\- Returns safe file paths

\- Used when attaching documents during claim submission



---



\## Two‑Step Claim Approval Logic



1\. Lecturer submits claim → Status = \*\*Submitted\*\*  

2\. PC approves → Status changes to \*\*Under Review\*\*  

3\. AM approves → Status changes to \*\*Approved\*\*  

4\. If either rejects → Status = \*\*Rejected\*\*



This ensures no claim is approved by only one authority.



---



\## Invoice Generation for HR



Uses LINQ grouping to calculate totals:



```csharp

var result = await \_db.Claims

&nbsp;   .Where(c => c.Status == ClaimStatus.Approved)

&nbsp;   .GroupBy(c => c.UserId)

&nbsp;   .Select(g => new LecturerInvoiceSummary

&nbsp;   {

&nbsp;       UserId = g.Key,

&nbsp;       LecturerName = g.First().User.FullName,

&nbsp;       TotalHours = g.Sum(c => c.HoursWorked),

&nbsp;       TotalAmount = g.Sum(c => c.TotalAmount)

&nbsp;   })

&nbsp;   .OrderBy(r => r.LecturerName)

&nbsp;   .ToListAsync();

```



HR can:

\- View totals per lecturer

\- Drill‑down into each lecturer’s claims

\- Use results for payroll purposes



---



\## Error Handling



The system includes:

\- ModelState validation

\- Try/catch wrappers for file upload and DB operations

\- Validation for maximum hours per month (180)

\- Safe redirects when invalid IDs are accessed

\- Friendly error messages shown using TempData



---



\## SQL Server Integration



The system:

\- Uses Entity Framework Core

\- Applies migrations to update schema

\- Uses foreign keys between Users, Claims, and Documents

\- Ensures relational integrity

\- Stores numeric values using decimal(18,2)



---



\## Authentication \& Authorization



\- Cookie authentication with login/logout  

\- Claims include:

&nbsp; - User Id

&nbsp; - Role

&nbsp; - HourlyRate  

\- Role-based access for Dashboard, HR, PC, AM  



---



\## How to Run



1\. Open the solution in Visual Studio  

2\. Configure connection string in \*\*appsettings.json\*\*  

3\. Run migrations:  

&nbsp;  ```

&nbsp;  Add-Migration Init

&nbsp;  Update-Database

&nbsp;  ```

4\. Press \*\*F5\*\* to run the application  

5\. Login using seeded accounts in `ApplicationDbSeeder`



---



\## Conclusion



This system modernizes the lecturer claim process by:  

✔ Automating critical calculations  

✔ Enforcing multi‑step approvals  

✔ Improving HR oversight  

✔ Providing dashboards for all roles  

✔ Ensuring reliable data storage via SQL Server  



It is structured, scalable, and aligned with real‑world institutional workflows.



