-- =============================================================================
-- Online Professional Exams Scheduling and Management System
-- Database Schema for MySQL / XAMPP (MariaDB)
-- Developer: Manzi Herve (ID: 24605, Course: DotNet, Group: A)
-- =============================================================================

CREATE DATABASE IF NOT EXISTS `online_exams_db` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE `online_exams_db`;

-- Drop existing tables in reverse dependency order
SET FOREIGN_KEY_CHECKS = 0;
DROP TABLE IF EXISTS `audit_logs`;
DROP TABLE IF EXISTS `payments`;
DROP TABLE IF EXISTS `bookings`;
DROP TABLE IF EXISTS `exam_schedules`;
DROP TABLE IF EXISTS `venues`;
DROP TABLE IF EXISTS `exams`;
DROP TABLE IF EXISTS `categories`;
DROP TABLE IF EXISTS `users`;
DROP TABLE IF EXISTS `roles`;
SET FOREIGN_KEY_CHECKS = 1;

-- 1. Roles Table
CREATE TABLE `roles` (
    `role_id` INT AUTO_INCREMENT PRIMARY KEY,
    `role_name` VARCHAR(50) NOT NULL UNIQUE,
    `description` VARCHAR(255) NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO `roles` (`role_id`, `role_name`, `description`) VALUES
(1, 'System Administrator', 'Full administrative privileges across the entire exam scheduling platform'),
(2, 'Admin Staff', 'Authorized staff for venue scheduling, booking oversight, and manual on-site payment verification'),
(3, 'Candidate', 'Registered candidate eligible to browse exams, book seats, manage registrations, and print admission tickets');

-- 2. Users Table
CREATE TABLE `users` (
    `user_id` INT AUTO_INCREMENT PRIMARY KEY,
    `role_id` INT NOT NULL,
    `full_name` VARCHAR(150) NOT NULL,
    `email` VARCHAR(150) NOT NULL UNIQUE,
    `password_hash` VARCHAR(255) NOT NULL,
    `password_salt` VARCHAR(255) NOT NULL,
    `phone_number` VARCHAR(50) NULL,
    `educational_background` VARCHAR(255) NULL,
    `professional_credentials` VARCHAR(255) NULL,
    `is_email_verified` TINYINT(1) NOT NULL DEFAULT 1,
    `email_verification_token` VARCHAR(100) NULL,
    `password_reset_otp` VARCHAR(10) NULL,
    `password_reset_expiry` DATETIME NULL,
    `is_active` TINYINT(1) NOT NULL DEFAULT 1,
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT `fk_users_role` FOREIGN KEY (`role_id`) REFERENCES `roles` (`role_id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 3. Categories Table
CREATE TABLE `categories` (
    `category_id` INT AUTO_INCREMENT PRIMARY KEY,
    `category_name` VARCHAR(100) NOT NULL UNIQUE,
    `description` VARCHAR(255) NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO `categories` (`category_id`, `category_name`, `description`) VALUES
(1, 'Cloud & Infrastructure', 'Cloud architecture, DevOps, virtualization, and enterprise networking certifications'),
(2, 'Cybersecurity & Governance', 'Information security, ethical hacking, compliance, and cyber risk management'),
(3, 'Project & IT Service Management', 'Agile leadership, PMP, Scrum, and ITIL framework certifications'),
(4, 'Accounting, Finance & Audit', 'Chartered accounting, financial analysis, internal audit, and forensic accounting'),
(5, 'Software Engineering & Data', 'Full-stack development, database architecture, AI, and big data engineering');

-- 4. Exams Table
CREATE TABLE `exams` (
    `exam_id` INT AUTO_INCREMENT PRIMARY KEY,
    `category_id` INT NOT NULL,
    `exam_code` VARCHAR(50) NOT NULL UNIQUE,
    `title` VARCHAR(200) NOT NULL,
    `discipline` VARCHAR(100) NOT NULL,
    `certification_level` VARCHAR(50) NOT NULL,
    `prerequisites` TEXT NULL,
    `duration_minutes` INT NOT NULL DEFAULT 90,
    `passing_score` INT NOT NULL DEFAULT 70,
    `fee` DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    `description` TEXT NULL,
    `is_active` TINYINT(1) NOT NULL DEFAULT 1,
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT `fk_exams_category` FOREIGN KEY (`category_id`) REFERENCES `categories` (`category_id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO `exams` (`exam_id`, `category_id`, `exam_code`, `title`, `discipline`, `certification_level`, `prerequisites`, `duration_minutes`, `passing_score`, `fee`, `description`) VALUES
(1, 1, 'AWS-SAA-C03', 'AWS Certified Solutions Architect - Associate', 'Cloud Architecture', 'Associate', 'Basic knowledge of distributed systems and AWS core services', 130, 72, 150.00, 'Validates ability to design and implement distributed systems on Amazon Web Services.'),
(2, 1, 'AZ-104', 'Microsoft Azure Administrator', 'Cloud Systems Administration', 'Associate', '6+ months of hands-on experience administering Azure', 120, 70, 165.00, 'Demonstrates expertise in implementing, managing, and monitoring Microsoft Azure environments.'),
(3, 2, 'CISSP-2026', 'Certified Information Systems Security Professional', 'Information Security', 'Expert', 'Minimum 5 years of cumulative paid work experience in 2+ cybersecurity domains', 180, 70, 749.00, 'Globally recognized credential for senior cyber defense leaders, engineers, and risk officers.'),
(4, 2, 'CEH-V12', 'Certified Ethical Hacker', 'Penetration Testing', 'Professional', '2 years of IT security work experience or official EC-Council training', 150, 70, 450.00, 'Master penetration testing methodologies, vulnerability analysis, and modern threat defense.'),
(5, 3, 'PMP-2026', 'Project Management Professional', 'Project Leadership', 'Professional', 'Secondary degree with 60 months project management experience or 4-year degree with 36 months', 230, 65, 405.00, 'The gold standard in project leadership across Agile, Predictive, and Hybrid methodologies.'),
(6, 3, 'ITIL-F-V4', 'ITIL 4 Foundation in IT Service Management', 'Service Management', 'Associate', 'None', 60, 65, 299.00, 'End-to-end operating model for the creation, delivery and continual improvement of tech-enabled products.'),
(7, 4, 'CPA-FAR', 'Certified Public Accountant - Financial Accounting & Reporting', 'Accounting', 'Professional', 'Bachelor degree in accounting/finance with 150 credit hours', 240, 75, 250.00, 'Rigorous assessment of standard US GAAP financial reporting standards for enterprises and non-profits.'),
(8, 5, 'CKA-2026', 'Certified Kubernetes Administrator', 'DevOps & Containers', 'Professional', 'Strong Linux CLI skills and Docker containerization fundamentals', 120, 66, 395.00, 'Hands-on performance-based exam validating Kubernetes production cluster management skills.');

-- 5. Venues Table
CREATE TABLE `venues` (
    `venue_id` INT AUTO_INCREMENT PRIMARY KEY,
    `venue_name` VARCHAR(150) NOT NULL,
    `address` VARCHAR(255) NOT NULL,
    `city` VARCHAR(100) NOT NULL,
    `room_number` VARCHAR(50) NULL,
    `total_capacity` INT NOT NULL DEFAULT 30,
    `contact_person` VARCHAR(100) NULL,
    `contact_phone` VARCHAR(50) NULL,
    `is_active` TINYINT(1) NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO `venues` (`venue_id`, `venue_name`, `address`, `city`, `room_number`, `total_capacity`, `contact_person`, `contact_phone`) VALUES
(1, 'AUCA Tech Examination Center', 'Gishushu Campus, KG 543 St', 'Kigali', 'Lab 101 - Prometric Suite', 25, 'Jean de Dieu Habimana', '+250 788 123 456'),
(2, 'Kigali ICT Innovation Hub', 'Telecom House, 4th Floor', 'Kigali', 'Certification Hall B', 30, 'Alice Umutoni', '+250 789 234 567'),
(3, 'Rwanda Online Assessment Lab', 'Boulevard de l\'Umuganda, Kacyiru', 'Kigali', 'Testing Room 3A', 20, 'Patrick Ndayisaba', '+250 783 345 678'),
(4, 'Rubavu Regional Testing Annex', 'Avenue de la Frontiere', 'Rubavu', 'Annex Lab 2', 15, 'Marie Claire Mukamana', '+250 785 456 789');

-- 6. Exam Schedules Table
CREATE TABLE `exam_schedules` (
    `schedule_id` INT AUTO_INCREMENT PRIMARY KEY,
    `exam_id` INT NOT NULL,
    `venue_id` INT NOT NULL,
    `exam_date` DATE NOT NULL,
    `start_time` TIME NOT NULL,
    `end_time` TIME NOT NULL,
    `max_seats` INT NOT NULL,
    `assigned_proctor` VARCHAR(100) NULL,
    `status` VARCHAR(50) NOT NULL DEFAULT 'Scheduled',
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT `fk_schedules_exam` FOREIGN KEY (`exam_id`) REFERENCES `exams` (`exam_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT `fk_schedules_venue` FOREIGN KEY (`venue_id`) REFERENCES `venues` (`venue_id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO `exam_schedules` (`schedule_id`, `exam_id`, `venue_id`, `exam_date`, `start_time`, `end_time`, `max_seats`, `assigned_proctor`, `status`) VALUES
(1, 1, 1, '2026-11-25', '09:00:00', '11:30:00', 25, 'Eric Mugisha (Lead Proctor)', 'Scheduled'),
(2, 1, 2, '2026-11-26', '14:00:00', '16:30:00', 30, 'Diane Uwera (Proctor)', 'Scheduled'),
(3, 2, 1, '2026-11-25', '13:00:00', '15:30:00', 25, 'Eric Mugisha (Lead Proctor)', 'Scheduled'),
(4, 3, 2, '2026-11-27', '08:30:00', '12:00:00', 20, 'Alice Umutoni (Chief Proctor)', 'Scheduled'),
(5, 4, 3, '2026-11-28', '10:00:00', '13:00:00', 20, 'Patrick Ndayisaba (Proctor)', 'Scheduled'),
(6, 5, 1, '2026-11-30', '09:00:00', '13:30:00', 25, 'Jean de Dieu Habimana', 'Scheduled'),
(7, 6, 2, '2026-12-02', '10:00:00', '11:30:00', 30, 'Diane Uwera', 'Scheduled'),
(8, 7, 3, '2026-12-05', '08:00:00', '12:30:00', 20, 'Patrick Ndayisaba', 'Scheduled'),
(9, 8, 1, '2026-12-08', '13:30:00', '16:00:00', 25, 'Eric Mugisha', 'Scheduled');

-- 7. Bookings Table
CREATE TABLE `bookings` (
    `booking_id` INT AUTO_INCREMENT PRIMARY KEY,
    `booking_reference` VARCHAR(50) NOT NULL UNIQUE,
    `candidate_id` INT NOT NULL,
    `schedule_id` INT NOT NULL,
    `booking_date` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `booking_status` VARCHAR(50) NOT NULL DEFAULT 'Pending Payment',
    `notes` TEXT NULL,
    `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT `fk_bookings_candidate` FOREIGN KEY (`candidate_id`) REFERENCES `users` (`user_id`) ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT `fk_bookings_schedule` FOREIGN KEY (`schedule_id`) REFERENCES `exam_schedules` (`schedule_id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 8. Payments Table
CREATE TABLE `payments` (
    `payment_id` INT AUTO_INCREMENT PRIMARY KEY,
    `booking_id` INT NOT NULL UNIQUE,
    `amount_paid` DECIMAL(10,2) NOT NULL,
    `payment_method` VARCHAR(50) NOT NULL,
    `receipt_number` VARCHAR(100) NOT NULL UNIQUE,
    `payment_status` VARCHAR(50) NOT NULL DEFAULT 'Verified',
    `verified_by_user_id` INT NOT NULL,
    `verified_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `remarks` TEXT NULL,
    CONSTRAINT `fk_payments_booking` FOREIGN KEY (`booking_id`) REFERENCES `bookings` (`booking_id`) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT `fk_payments_verifier` FOREIGN KEY (`verified_by_user_id`) REFERENCES `users` (`user_id`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 9. Audit Logs Table
CREATE TABLE `audit_logs` (
    `log_id` INT AUTO_INCREMENT PRIMARY KEY,
    `user_id` INT NULL,
    `action` VARCHAR(100) NOT NULL,
    `details` TEXT NULL,
    `ip_address` VARCHAR(50) DEFAULT '127.0.0.1',
    `timestamp` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT `fk_audit_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Default sample users:
-- Passwords will be populated with secure SHA256 hashed salt & hash
-- Admin: admin@exams.com / Admin@123
-- Staff: staff@exams.com / Staff@123
-- Candidate: candidate@exams.com / Candidate@123
-- Student: manzi.herve@auca.ac.rw / Herve@123

-- Insert Default Sample Users
INSERT INTO `users` (`user_id`, `role_id`, `full_name`, `email`, `password_hash`, `password_salt`, `phone_number`, `educational_background`, `professional_credentials`, `is_email_verified`, `is_active`) VALUES
(1, 1, 'System Administrator', 'admin@exams.com', 'vYCDZN6fMUICIEVOjh3LoXIpnsJzcjw7bCIcwScPHlI=', 'c2FsdF9BZG1pbkAxMjM=', '+250 788 111 222', 'M.Sc. Information Security & Systems', 'CISSP, CISM, AWS-SA Pro', 1, 1),
(2, 2, 'Claire Mukamana (Staff)', 'staff@exams.com', 'QtgAIoUtLJHnVWKgGmZ0baWkRjp4fskiVSMb9bvTdBQ=', 'c2FsdF9TdGFmZkAxMjM=', '+250 788 333 444', 'B.Sc. Computer Science', 'Exam Operations Specialist', 1, 1),
(3, 3, 'David Mugabo (Candidate)', 'candidate@exams.com', 'ghY1/ReWR6szUvOER5MqVODH2uO2dRp1O6Anpjj5XNw=', 'c2FsdF9DYW5kaWRhdGVAMTIz', '+250 788 555 666', 'B.Sc. Software Engineering', 'Junior Cloud Engineer', 1, 1),
(4, 3, 'Manzi Herve', 'manzi.herve@auca.ac.rw', 'BCDifN/vVJFKGhiPIO03mgxutLvaB61iuRxj1F2DOsQ=', 'c2FsdF9IZXJ2ZUAxMjM=', '+250 788 777 888', 'Bachelor of Science in Information Technology (AUCA)', 'DotNet Specialist, Software Architect', 1, 1);

-- Insert Sample Bookings
INSERT INTO `bookings` (`booking_id`, `booking_reference`, `candidate_id`, `schedule_id`, `booking_date`, `booking_status`, `notes`) VALUES
(1, 'EXAM-2026-94812', 3, 1, '2026-11-15 10:30:00', 'Confirmed / Cleared for Exam', 'Standard accommodation requested. Approved.'),
(2, 'EXAM-2026-10492', 4, 1, '2026-11-16 14:15:00', 'Pending Payment', 'Candidate planning on-site payment at Kigali Center.'),
(3, 'EXAM-2026-58291', 4, 3, '2026-11-17 09:45:00', 'Confirmed / Cleared for Exam', 'Onsite cash payment verified.');

-- Insert Sample Payment Records
INSERT INTO `payments` (`payment_id`, `booking_id`, `amount_paid`, `payment_method`, `receipt_number`, `payment_status`, `verified_by_user_id`, `verified_at`, `remarks`) VALUES
(1, 1, 150.00, 'Cash at Center', 'REC-2026-0041', 'Verified', 2, '2026-11-15 11:00:00', 'Paid full examination fee in cash at testing center.'),
(2, 3, 165.00, 'Mobile Money', 'MOMO-RW-98214', 'Verified', 2, '2026-11-17 10:05:00', 'MTN MoMo transaction confirmed.');

-- Insert Sample Audit Logs
INSERT INTO `audit_logs` (`user_id`, `action`, `details`) VALUES
(1, 'DATABASE_INITIALIZATION', 'Online Professional Exams Scheduling and Management System database initialized successfully.'),
(2, 'PAYMENT_VERIFICATION', 'Verified payment of $150.00 for Booking Ref EXAM-2026-94812 (Candidate: David Mugabo).'),
(2, 'PAYMENT_VERIFICATION', 'Verified MoMo payment of $165.00 for Booking Ref EXAM-2026-58291 (Candidate: Manzi Herve).');
