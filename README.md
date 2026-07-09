# 🚗 RoadReady - Car Rental Management System

RoadReady is a full-stack Car Rental Management System developed using **ASP.NET Core Microservices** and **React**. The application allows customers to rent vehicles online, agents to manage vehicle check-in/check-out, and administrators to manage the entire platform.

RoadReady is built using a **Microservice Architecture**, separating authentication, vehicle management, and booking operations into independent services connected through an Ocelot API Gateway. The system provides a complete car rental experience with secure authentication, online bookings, digital payments, and role-based management for customers, rental agents, and administrators.

---

# 🏗️ Architecture

The backend is built using a **Microservice Architecture**.

```
React Frontend
        │
        ▼
Ocelot API Gateway (Port 5000)
        │
 ┌──────┼─────────┐
 │      │         │
 ▼      ▼         ▼
Auth   Car     Booking
Service Service Service
5001    5002     5003
        │
        ▼
    SQL Server
```

### Services

- **API Gateway (Ocelot)** – Central entry point for the application. Routes incoming client requests to the appropriate microservice while providing a single API endpoint for the frontend.

- **Authentication Service** – Handles user registration, login, JWT authentication, refresh tokens, role-based authorization, password reset, and account security.

- **Car Service** – Manages vehicle inventory, brands, vehicle availability, car images, promo code management, search and filtering, and other vehicle-related operations.

- **Booking Service** – Handles vehicle bookings, booking lifecycle management, Razorpay payment integration, payment webhook processing, booking history, receipts, rental agent check-in/check-out operations, and email notifications.

---

# 💻 Tech Stack

## Backend

- ASP.NET Core
- ASP.NET Core Web API
- C#
- Entity Framework Core
- SQL Server
- JWT Authentication
- Ocelot API Gateway
- Microservice Architecture

## Frontend

- React
- React Router DOM
- Axios
- Context API
- HTML5
- CSS3
- JavaScript

## Payment & Email

- Razorpay Payment Links
- Razorpay Webhooks
- Brevo SMTP

## DevOps

- Git
- GitHub
- Jenkins
- SonarQube

## Testing

- NUnit
- Moq Framework

---

# ✨ Key Features

### Customer

- User Registration & Login
- JWT Authentication
- Browse & Search Cars
- View Car Details
- Online Booking
- Promo Code Support
- Razorpay Payment
- Booking History
- Booking Receipt
- Email Confirmation

### Rental Agent

- Agent Dashboard
- Vehicle Check-In
- Vehicle Check-Out
- Booking Verification

### Administrator

- Dashboard
- User Management
- Vehicle Management
- Brand Management
- Booking Management
- Promo Code Management

---

# 🧪 Testing

Unit testing was implemented using:

- NUnit
- Moq Framework

Tests cover the service layer and business logic using mocked repositories.

---

# 🚀 CI/CD

Implemented using **Jenkins**

Pipeline includes:

- Source Code Checkout
- Build
- Automated Tests
- SonarQube Analysis

---

# 📊 Code Quality

Code quality analysis performed using **SonarQube**

Includes:

- Bugs
- Vulnerabilities
- Code Smells
- Maintainability Analysis
- Security Hotspots

---

# 📸 Screenshots

## 👤 Customer

| Page | Screenshot |
|------|------------|
| Home | ![](Screenshots/HomePage.png) |
| Home (Section 2) | ![](Screenshots/HomePage2.png) |
| Login | ![](Screenshots/Login.png) |
| Cars | ![](Screenshots/Cars.png) |
| Car Details | ![](Screenshots/CarDetails.png) |
| Booking | ![](Screenshots/CarBooking.png) |
| Payment | ![](Screenshots/Payment.png) |
| Booking History | ![](Screenshots/BookingHistory.png) |
| Booking Receipt | ![](Screenshots/Bookingreceipt.png) |
| Booking Email | ![](Screenshots/CarBookingEmail.png) |

---

## 🚘 Rental Agent

| Page | Screenshot |
|------|------------|
| Dashboard | ![](Screenshots/AgentDash.png) |
| Check-In / Check-Out | ![](Screenshots/AgentCheckoutCheckIn.png) |
| Vehicle Check-Out | ![](Screenshots/CarCheckOutAgent.png) |

---

## 👨‍💼 Administrator

| Page | Screenshot |
|------|------------|
| Dashboard | ![](Screenshots/AdminDash.png) |
| Users | ![](Screenshots/UsersAdmin.png) |
| Cars | ![](Screenshots/CarsAdmin.png) |
| Brands | ![](Screenshots/BrandsAdmin.png) |
| Bookings | ![](Screenshots/BookingsAdmin.png) |
| Promo Codes | ![](Screenshots/PromocodeAdmin.png) |

---

## ⚙️ DevOps

| Tool | Screenshot |
|------|------------|
| Jenkins | ![](Screenshots/Jenkins.png) |
| SonarQube | ![](Screenshots/SonarQube.png) |

---

# ▶️ Running the Project

## Prerequisites

Install:

- .NET 8 SDK
- Node.js (LTS)
- SQL Server
- Visual Studio 2022 / VS Code
- Jenkins (Optional)
- SonarQube (Optional)

---

## Configure Required Services

Before running the project, configure the following:

### Razorpay

Update:

- Key ID
- Key Secret
- Webhook Secret

---

### Brevo

Configure:

- SMTP Host
- SMTP Port
- Username
- Password
- Sender Email


### Google Auth

- Key ID
- Key Secret


---

### SQL Server

Update the connection strings inside each microservice's `appsettings.json`.

Run Entity Framework migrations.

---

## Start the Backend Services

| Service | Port |
|---------|------|
| API Gateway (Ocelot) | **5000** |
| Authentication Service | **5001** |
| Car Service | **5002** |
| Booking Service | **5003** |

Start all four services.

---

## Start React Frontend

```bash
npm install
npm run dev
```

---

# 👨‍💻 Author

**William Giftson S**

Full Stack .NET Developer Trainee