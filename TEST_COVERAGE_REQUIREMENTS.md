# Test Coverage Requirements - 80% Minimum

This document lists all controllers and services that need test coverage to achieve at least 80% code coverage.

## Controllers (14 total)

### ✅ Controllers WITH Tests (6)
1. **C_Auth** (`Controller_Auth.cs`) - ✅ Test_Authentication.cs
   - Status: Has basic tests
   - Coverage needed: Additional edge cases, logout, token refresh

2. **C_Parkinglots** (`Controller_Parkinglots.cs`) - ✅ Test_Parkinglots.cs
   - Status: Has comprehensive tests
   - Coverage needed: Additional authorization edge cases

3. **C_Payments** (`Controller_Payments.cs`) - ✅ Test_Payments.cs
   - Status: Has authorization and CRUD tests
   - Coverage needed: Additional edge cases, error scenarios

4. **C_Reservations** (`Controller_Reservations.cs`) - ✅ Test_Reservations.cs, Test_Reservation.cs
   - Status: Has comprehensive tests
   - Coverage needed: Additional edge cases

5. **C_Users** (`Controller_Users.cs`) - ✅ Test_Users.cs
   - Status: Has comprehensive tests
   - Coverage needed: Additional edge cases

6. **C_Vehicles** (`Controller_Vehicles.cs`) - ✅ Test_Vehicles.cs
   - Status: Has comprehensive tests
   - Coverage needed: Additional edge cases

### ❌ Controllers WITHOUT Tests (8) - **PRIORITY**
1. **C_Balance** (`Controller_Balance.cs`)
   - Endpoints: GET /me, GET /user/{userId}, POST /add, POST /deduct, POST /transfer
   - Tests needed: Authorization, CRUD operations, edge cases, error handling

2. **C_Billing** (`Controller_Billing.cs`)
   - Endpoints: GET /all, GET /mine, GET /{id}, POST /create, PUT /update/{id}, DELETE /delete/{id}
   - Tests needed: Authorization, CRUD operations, edge cases, error handling

3. **C_Company** (`Controller_Company.cs`)
   - Endpoints: GET /, GET /{id}, POST /create, PUT /update/{id}, DELETE /delete/{id}
   - Tests needed: Authorization, CRUD operations, edge cases, error handling

4. **C_Gate** (`Controller_Gate.cs`)
   - Endpoints: POST /open, POST /close, GET /status
   - Tests needed: Basic functionality, error handling, logging

5. **C_Hotel** (`Controller_Hotel.cs`)
   - Endpoints: GET /, GET /{id}, POST /create, PUT /update/{id}, DELETE /delete/{id}
   - Tests needed: Authorization, CRUD operations, edge cases, error handling

6. **C_NFC** (`Controller_NFC.cs`)
   - Endpoints: POST /verify-and-pay
   - Tests needed: Balance verification, payment processing, gate opening, error handling

7. **C_Profile** (`Controller_Profile.cs`)
   - Endpoints: GET /, GET /{id}, PUT /update/{id}
   - Tests needed: Authorization (users can only view/edit own profile), CRUD operations, edge cases

8. **C_Sessions** (`Controller_Sessions.cs`)
   - Endpoints: POST /start, POST /end/{id}, GET /all, GET /{id}, GET /user/{userId}, GET /active
   - Tests needed: Authorization, CRUD operations, session lifecycle, edge cases, error handling

---

## Services (13 total)

### ❌ Services WITHOUT Tests (13) - **ALL PRIORITY**

1. **Service_Billing** (`Service_Billing.cs`)
   - Methods: GetAll, GetById, GetForUser, Create, Update, Delete
   - Tests needed: Unit tests for all business logic, edge cases, error handling

2. **Service_Company** (`Service_Company.cs`)
   - Methods: GetAll, GetById, Create, Update, Delete
   - Tests needed: Unit tests for all business logic, edge cases, error handling

3. **Service_Hotel** (`Service_Hotel.cs`)
   - Methods: GetAll, GetById, Create, Update, Delete
   - Tests needed: Unit tests for all business logic, edge cases, error handling

4. **Service_Parkinglots** (`Service_Parkinglots.cs`)
   - Methods: Various parking lot operations
   - Tests needed: Unit tests for all business logic (controller tests exist but service layer needs unit tests)

5. **Service_Payments** (`Service_Payments.cs`)
   - Methods: GetAllPayments, GetPaymentById, CreatePayment, UpdatePayment, DeletePayment
   - Tests needed: Unit tests for all business logic (controller tests exist but service layer needs unit tests)

6. **Service_Profile** (`Service_Profile.cs`)
   - Methods: GetById, Update
   - Tests needed: Unit tests for all business logic, edge cases, error handling

7. **Service_Reservations** (`Service_Reservations.cs`)
   - Methods: Various reservation operations
   - Tests needed: Unit tests for all business logic (controller tests exist but service layer needs unit tests)

8. **Service_Sessions** (`Service_Sessions.cs`)
   - Methods: StartSession, EndSession, GetSessionById, GetSessionsForUser, GetActiveSessions
   - Tests needed: Unit tests for all business logic, session lifecycle, edge cases, error handling

9. **Service_Token** (`Service_Token.cs`)
   - Methods: GenerateToken, ValidateToken, RefreshToken (if exists)
   - Tests needed: Unit tests for token generation, validation, expiration, edge cases

10. **Service_TokenRevocation** (`Service_TokenRevocation.cs`)
    - Methods: RevokeToken, IsTokenRevoked
    - Tests needed: Unit tests for token revocation logic, edge cases, error handling

11. **Service_UserBalance** (`Service_UserBalance.cs`)
    - Methods: GetBalanceForUser, CreateBalance, AddBalance, DeductBalance, TransferBalance
    - Tests needed: Unit tests for all balance operations, edge cases (negative balance, insufficient funds), error handling

12. **Service_Users** (`Service_Users.cs`)
    - Methods: GetAllUsers, GetUserById, CreateUser, UpdateUser, DeleteUser
    - Tests needed: Unit tests for all business logic (controller tests exist but service layer needs unit tests)

13. **Service_Vehicles** (`Service_Vehicles.cs`)
    - Methods: GetAllVehicles, GetVehicleById, CreateVehicle, UpdateVehicle, DeleteVehicle
    - Tests needed: Unit tests for all business logic (controller tests exist but service layer needs unit tests)

---

## Summary

### Controllers
- **Total**: 14
- **With Tests**: 6 (43%)
- **Without Tests**: 8 (57%) ⚠️

### Services
- **Total**: 13
- **With Tests**: 0 (0%) ⚠️
- **Without Tests**: 13 (100%) ⚠️

### Priority Order for Test Creation

#### High Priority (No tests exist)
1. **C_Balance** - Financial operations, critical
2. **C_Billing** - Financial operations, critical
3. **C_Sessions** - Core functionality
4. **C_Profile** - User data access
5. **C_NFC** - Payment processing
6. **C_Company** - Business entity management
7. **C_Hotel** - Business entity management
8. **C_Gate** - Hardware integration

#### Service Layer (All High Priority)
1. **Service_UserBalance** - Financial operations, critical
2. **Service_Billing** - Financial operations, critical
3. **Service_Token** - Security, critical
4. **Service_TokenRevocation** - Security, critical
5. **Service_Sessions** - Core functionality
6. **Service_Reservations** - Core functionality
7. **Service_Payments** - Financial operations
8. **Service_Profile** - User data
9. **Service_Users** - User management
10. **Service_Vehicles** - Vehicle management
11. **Service_Parkinglots** - Parking lot management
12. **Service_Company** - Business entity
13. **Service_Hotel** - Business entity

---

## Test Coverage Goals

To achieve 80% coverage, focus on:

1. **All HTTP endpoints** - Test all routes, methods, and status codes
2. **Authorization** - Test role-based access (SuperAdmin, ParkingLotAdmin, ParkingUser)
3. **Validation** - Test input validation, edge cases, boundary conditions
4. **Error handling** - Test error scenarios, exceptions, invalid data
5. **Business logic** - Test service layer methods independently
6. **Integration** - Test controller-service-database interactions
7. **Edge cases** - Test null values, empty strings, invalid GUIDs, etc.

---

## Notes

- Controller tests exist for 6 controllers but may need expansion for edge cases
- **NO service layer unit tests exist** - this is a critical gap
- Service layer tests should be unit tests (mocked dependencies)
- Controller tests can be integration tests (using test database)
- Consider using Moq for service mocking in controller tests
- Consider using xUnit, FluentAssertions (already in use), and potentially AutoFixture for test data generation
