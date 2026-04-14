# Landmine Soft College Student Portal

A complete college management portal backend built with **ASP.NET Core Web API + Entity Framework Core + PostgreSQL**.

## Features

- **JWT Authentication** with role-based access control (RBAC)
- **3 User Types**: Student, Faculty, Admin — each with separate registration/login
- **Course Management**: Subjects, courses, faculty assignments
- **Enrollment Management**: Student enrollment with status tracking
- **Attendance Tracking**: Single and bulk marking, attendance summaries
- **Marks Management**: Theory + practical marks with auto-grading
- **Fee Management**: Fee structures by branch, payment processing with receipts
- **Announcements**: Targeted announcements with expiry
- **Admin Dashboard**: Statistics and reports
- **Swagger/OpenAPI** documentation
- **Global Error Handling** with proper HTTP status codes

## Tech Stack

| Component | Technology |
|-----------|-----------|
| Backend | ASP.NET Core (.NET 10) |
| ORM | Entity Framework Core |
| Database | PostgreSQL |
| Auth | JWT Bearer Tokens + BCrypt |
| Docs | Swagger / Swashbuckle |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/) (running on port 5432)

## Setup & Run

### 1. Clone the repository
```bash
git clone <repo-url>
cd Assig
```

### 2. Configure the database
Edit `CollegePortal.API/appsettings.json` — update the connection string:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=college_portal;Username=postgres;Password=YOUR_PASSWORD"
}
```

### 3. Create the PostgreSQL database
```sql
CREATE DATABASE college_portal;
```

### 4. Run the application
```bash
cd CollegePortal.API
dotnet run
```
The database tables will be auto-created on startup.

### 5. Open Swagger UI
Navigate to: `https://localhost:5001/swagger` (or `http://localhost:5000/swagger`)

## API Endpoints

### Auth (`/api/auth`)
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/auth/student/register` | Register student | Public |
| POST | `/api/auth/faculty/register` | Register faculty | Public |
| POST | `/api/auth/admin/register` | Register admin | ADMIN |
| POST | `/api/auth/student/login` | Student login | Public |
| POST | `/api/auth/faculty/login` | Faculty login | Public |
| POST | `/api/auth/admin/login` | Admin login | Public |
| POST | `/api/auth/forgot-password` | Request reset token | Public |
| POST | `/api/auth/reset-password` | Reset password | Public |
| POST | `/api/auth/change-password` | Change password | Any |

### Student (`/api/student`)
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/student/profile` | Get profile | STUDENT |
| PUT | `/api/student/profile` | Update profile | STUDENT |
| GET | `/api/student/enrollments` | View enrollments | STUDENT |
| GET | `/api/student/attendance` | View attendance | STUDENT |
| GET | `/api/student/attendance/summary` | Attendance summary | STUDENT |
| GET | `/api/student/marks` | View marks | STUDENT |
| GET | `/api/student/fees` | View fee payments | STUDENT |
| GET | `/api/student/fee-structure` | View fee structure | STUDENT |

### Faculty (`/api/faculty`)
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/faculty/profile` | Get profile | FACULTY |
| PUT | `/api/faculty/profile` | Update profile | FACULTY |
| GET | `/api/faculty/courses` | View assigned courses | FACULTY |
| GET | `/api/faculty/courses/{id}/students` | Students in course | FACULTY |
| POST | `/api/faculty/attendance` | Mark attendance | FACULTY |
| POST | `/api/faculty/attendance/bulk` | Bulk attendance | FACULTY |
| GET | `/api/faculty/courses/{id}/attendance` | View attendance | FACULTY |
| POST | `/api/faculty/marks` | Enter marks | FACULTY |
| GET | `/api/faculty/courses/{id}/marks` | View marks | FACULTY |

### Admin (`/api/admin`)
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/admin/dashboard` | Dashboard stats | ADMIN |
| GET | `/api/admin/students` | List students | ADMIN |
| GET | `/api/admin/students/{id}` | Student detail | ADMIN |
| DELETE | `/api/admin/students/{id}` | Delete student | ADMIN |
| GET | `/api/admin/faculty` | List faculty | ADMIN |
| DELETE | `/api/admin/faculty/{id}` | Delete faculty | ADMIN |
| GET | `/api/admin/admins` | List admins | ADMIN |
| GET | `/api/admin/reports/enrollment` | Enrollment report | ADMIN |
| GET | `/api/admin/reports/fees` | Fee report | ADMIN |
| GET | `/api/admin/reports/attendance` | Attendance report | ADMIN |

### Subjects, Courses, Enrollments, Fees, Announcements
Full CRUD endpoints — see Swagger UI for details.

## Environment Variables

For production, set these environment variables:
```
ConnectionStrings__DefaultConnection=Host=...;Database=college_portal;...
Jwt__SecretKey=YourProductionSecretKey
Jwt__Issuer=CollegePortal.API
Jwt__Audience=CollegePortal.Client
```

## Deployment

### Render
1. Create a new Web Service on [Render](https://render.com)
2. Set build command: `dotnet publish -c Release -o out`
3. Set start command: `dotnet out/CollegePortal.API.dll`
4. Set environment variables
5. Add a PostgreSQL database
6. Update `ConnectionStrings__DefaultConnection`

## Testing with Postman

1. Register a student via `POST /api/auth/student/register`
2. Login via `POST /api/auth/student/login` — copy the JWT token
3. Set the `Authorization` header to `Bearer <token>` for subsequent requests
4. Test protected endpoints

---

**Team Landmine Soft** | Landmine Soft College Student Portal | 2024-25
