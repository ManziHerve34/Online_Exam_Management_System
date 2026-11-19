# Online Professional Exams Scheduling and Management System

**Developer Name:** Manzi Herve  
**Student ID:** 24605  
**Course:** DotNet  
**Group:** GROUP A  
**Platform:** C# Windows Forms (.NET Framework 4.8), Visual Studio IDE  
**Database:** XAMPP MySQL / MariaDB (Port 3306, Database: `online_exams_db`)  

---

## 1. Project Overview & System Scope
The **Online Professional Exams Scheduling and Management System** is a desktop Windows Forms application engineered to streamline the registration, scheduling, and administrative oversight of professional certification and licensure examinations.

Focusing on the pre-exam lifecycle, the application allows candidates to register accounts, explore a searchable exam catalog, schedule examination sessions with real-time seat tracking, and download printable official Admission Tickets with unique booking reference codes. Administrative staff retain strict oversight over testing centers, seat capacities, proctors, and manual on-site payment verification.

---

## 2. Requirements Compliance & Functional Modules

### A. User & Student Management Module
* **FR-1.1 (Candidate Registration):** Users can register by providing Full Legal Name, Email Address, Phone Number, Educational Background, Professional Credentials, and a secure password.
* **FR-1.2 (Email Address Verification):** Generates a 6-digit verification code/token to validate candidate emails. In the desktop interface, the active token is displayed for instant verification and testing.
* **FR-1.3 (Secure Authentication):** Secure login using email and hashed passwords with cryptographic salt verification.
* **FR-1.4 (Password Recovery via OTP):** Provides 2-step password reset via a 6-digit email OTP (One-Time Password) with 15-minute expiration and secure password updating.
* **FR-1.5 (Profile Management):** Candidates can view and update their contact information, university/educational background, credentials, and update passwords.

### B. Exam Catalog & Browsing Module
* **FR-2.1 (Searchable Catalog):** Displays official certification exams with Exam Code, Certification Title, Category, Discipline, Level, Duration (minutes), Minimum Passing Score (%), Prerequisites, and Examination Fee ($).
* **FR-2.2 (Multi-Criteria Filtering):** Real-time filtering by Category (Cloud & Infrastructure, Cybersecurity, Project Management, Finance & Accounting, Software Engineering), Certification Level (Associate, Professional, Expert, Specialty), Discipline, and live keyword search.
* **Direct Booking Shortcut:** Candidates can inspect syllabus overviews and click *"Book This Exam Now"* to transition directly into slot selection.

### C. Exam Scheduling & Booking Module
* **FR-3.1 (Seat Selection & Real-Time Availability):** Candidates select an available date, time slot, and testing center/venue based on real-time calculated seat availability (`Available = MaxSeats - BookedSeats`).
* **FR-3.2 (Double-Booking Validation Check):**
  1. Prevents candidates from scheduling two exams at overlapping time windows on the same date.
  2. Prohibits re-booking the same exam if an active booking is already pending or confirmed.
  3. Prohibits booking any slot that has reached maximum capacity.
* **FR-3.3 (Unique Ticket Reference & Printable Summary):** Generates unique Exam Booking Reference numbers (format: `EXAM-2026-XXXXX`). Automatically renders an official **Admission Ticket & Scheduling Summary** with candidate details, exam specifications, reporting guidelines, barcode visualization, and options to **Print** or **Export to HTML**.
* **FR-3.4 (Candidate Dashboard):** Categorized tabs to monitor *Upcoming Exams*, *Past / Completed Exams*, and *Cancelled Bookings*, with clearance status badges.
* **FR-3.5 (Reschedule & Cancel Cutoff Policy):** Candidates can reschedule to an alternate slot or cancel a booking up to a defined policy window (24 hours prior to the scheduled exam start time).

### D. Administrative & Payment Verification Module
* **FR-4.1 (Admin CRUD Operations):** Full dashboard CRUD operations for:
  - **Exam Offerings:** Codes, titles, prerequisites, passing standards, durations, fees, and active status.
  - **Testing Venues / Centers:** Center name, city, room/hall, address, total capacity, contact person, phone.
  - **Schedules & Sessions:** Exam, venue, date, start time, end time, maximum seat capacity, assigned proctor, session status.
* **FR-4.2 (Booking Oversight by Venue & Date):** Staff can view and filter all exam registrations per test center, date, and status.
* **FR-4.3 (Manual On-Site Payment Clearance):** Authorized staff record on-site payments (Cash at Center, Bank Slip, Mobile Money / MoMo, Credit Card), generate official receipt numbers (`REC-2026-XXXX`), and update booking status from *"Pending Payment"* to *"Confirmed / Cleared for Exam"*.
* **FR-4.4 (Administrator Reports):**
  1. **Daily Registrations Report:** Date-range breakdown of total bookings, confirmed clearances, pending payments, and revenue per day.
  2. **Fee Collections Report:** Detailed transaction log of verified fees, payment methods, receipts, verifiers, and candidate details.
  3. **Venue Occupancy Report:** Real-time seating utilization and capacity percentage analysis across all authorized centers.
  4. **Export:** One-click export to CSV for all administrative reports.

---

## 3. Non-Functional & Security Architecture
* **NFR-1.1 (Cryptographic Password Hashing):** All passwords are encrypted with SHA-256 and unique per-user cryptographic salts (`PasswordHelper.cs`) before database persistence.
* **NFR-1.2 (Role-Based Access Control - RBAC):** Strictly isolated interfaces and operations for:
  - **Candidate (Role ID: 3):** Catalog browsing, slot scheduling, ticket printing, booking management, profile editing.
  - **Admin Staff (Role ID: 2):** Booking oversight, venue schedules, manual on-site payment clearance.
  - **System Administrator (Role ID: 1):** Full access to CRUD exams, venues, schedules, user rosters, system reports, and audit logs.
* **NFR-2.1 & NFR-2.2 (Performance & Scalability):** Indexed database queries and relational constraints handle high-volume scheduling without performance degradation.
* **NFR-4.1 & NFR-4.2 (Usability & Responsiveness):** Corporate Navy/Slate UI palette (`UiTheme.cs`), intuitive sidebar navigation, visual status badges, and responsive TableLayoutPanels.

---

## 4. Database Setup in XAMPP

1. Open **XAMPP Control Panel** and ensure the **MySQL** module is running on port **3306**.
2. The database name is **`online_exams_db`**.
3. The SQL schema and sample data file is located at `database.sql` in the project root.
4. If needed, you can import `database.sql` in **phpMyAdmin** (`http://localhost/phpmyadmin`) or via the command line:
   ```cmd
   mysql -u root < database.sql
   ```
5. The application includes an automated initialization check (`DatabaseHelper.EnsureDatabaseAndTables()`) that ensures tables and default records exist on startup.

### Database Tables Overview
| Table Name | Description |
| :--- | :--- |
| `roles` | RBAC role definitions (System Administrator, Admin Staff, Candidate). |
| `users` | Candidate, Staff, and Admin profiles with hashed passwords and salts. |
| `categories` | Professional certification categories (Cloud, Security, Project Mgmt, etc.). |
| `exams` | Exam offerings with codes, prerequisites, duration, fee, passing score. |
| `venues` | Authorized testing centers with address, room number, total capacity. |
| `exam_schedules` | Scheduled session windows with date, time slots, max seats, proctor. |
| `bookings` | Candidate registrations with unique reference codes (`EXAM-2026-XXXXX`). |
| `payments` | On-site payment receipts, verified amounts, methods, staff verifier ID. |
| `audit_logs` | Immutable security and operational audit trail with timestamps and client IPs. |

---

## 5. Pre-Configured Test Accounts

For rapid grading and demonstration, the login window includes 1-click test credential fill buttons:

| Role | Email | Password | Access Level |
| :--- | :--- | :--- | :--- |
| **System Administrator** | `admin@exams.com` | `Admin@123` | Full administrative control (Exams, Venues, Schedules, Reports, Users, Audit) |
| **Admin Staff** | `staff@exams.com` | `Staff@123` | Venue schedules, booking oversight, on-site payment clearance |
| **Candidate (Student)** | `manzi.herve@auca.ac.rw` | `Herve@123` | Candidate booking, admission ticket, reschedule/cancel, profile |
| **Candidate (General)** | `candidate@exams.com` | `Candidate@123` | Candidate exam booking, tickets, and profile management |

---

## 6. How to Open and Run in Visual Studio

1. Open **Visual Studio**.
2. Click **Open a project or solution**.
3. Select `OnlineExamsSystem.sln` from this folder:
   ```
   c:\Users\Herve\Desktop\Dotnet\Online Professional Exams Scheduling and Management System\OnlineExamsSystem.sln
   ```
4. Verify that MySQL is running in XAMPP.
5. Set the build configuration to **Debug | Any CPU**.
6. Press **F5** or click **Start** to run the application.
7. All dependencies (`MySqlConnector.dll`, etc.) are bundled directly in the `lib\` directory with `<HintPath>` references, guaranteeing a build with **0 errors and 0 missing dependencies**.
