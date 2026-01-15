# Testing Framework - 80% Code Coverage Guide

This document provides a comprehensive framework for writing tests to achieve 80% code/line coverage. Each section outlines what needs to be tested for controllers and services.

## Test Coverage Strategy

### Test Types
1. **Happy Path Tests** - Normal, expected behavior
2. **Sad Path Tests** - Error conditions, invalid inputs
3. **Edge Case Tests** - Boundary conditions, null values, empty strings
4. **Authorization Tests** - Role-based access control (RBAC)
5. **Integration Tests** - Controller + Service + Database interactions
6. **Unit Tests** - Service layer with mocked dependencies

### Test Categories Per Endpoint
- ✅ **Happy Path**: Valid input, expected success response
- ❌ **Sad Path**: Invalid input, expected error response
- 🔒 **Authorization**: Unauthorized access, forbidden access
- 📊 **Edge Cases**: Null, empty, boundary values, invalid formats
- 🔄 **State Changes**: Create → Read → Update → Delete flows

---

## 1. Test_Authentication.cs (C_Auth Controller)

### Current Coverage: ~40%
### Target Coverage: 80%+

### Missing Tests:

#### Login Endpoint (`POST /api/v2/auth/login`)
- ✅ Login_Should_Return_Token_For_Valid_Credentials (EXISTS)
- ✅ Login_With_Invalid_Credentials_Should_Return_401 (EXISTS)
- ❌ **MISSING**: Login_With_Null_Username_Should_Return_400
- ❌ **MISSING**: Login_With_Empty_Username_Should_Return_400
- ❌ **MISSING**: Login_With_Null_Password_Should_Return_400
- ❌ **MISSING**: Login_With_Empty_Password_Should_Return_400
- ❌ **MISSING**: Login_With_Whitespace_Only_Should_Return_400
- ❌ **MISSING**: Login_With_Inactive_User_Should_Return_401
- ❌ **MISSING**: Login_With_User_No_Password_Should_Return_401
- ❌ **MISSING**: Login_With_Legacy_Hash_Should_Upgrade_To_BCrypt
- ❌ **MISSING**: Login_Should_Return_Token_With_ExpiresAt

#### Me Endpoint (`GET /api/v2/auth/me`)
- ✅ Get_Me_Endpoint_Without_Token_Should_Return_401 (EXISTS)
- ❌ **MISSING**: Get_Me_Endpoint_With_Valid_Token_Should_Return_User_Info
- ❌ **MISSING**: Get_Me_Endpoint_With_Expired_Token_Should_Return_401
- ❌ **MISSING**: Get_Me_Endpoint_With_Invalid_Token_Should_Return_401
- ❌ **MISSING**: Get_Me_Endpoint_Should_Return_Correct_Claims

#### Register Endpoint (`POST /api/v2/auth/register`)
- ❌ **MISSING**: Register_With_Valid_Data_Should_Return_200
- ❌ **MISSING**: Register_With_Null_Username_Should_Return_400
- ❌ **MISSING**: Register_With_Empty_Username_Should_Return_400
- ❌ **MISSING**: Register_With_Null_Password_Should_Return_400
- ❌ **MISSING**: Register_With_Password_Mismatch_Should_Return_400
- ❌ **MISSING**: Register_With_Invalid_Email_Should_Return_400
- ❌ **MISSING**: Register_With_Invalid_Phone_Should_Return_400
- ❌ **MISSING**: Register_With_Duplicate_Username_Should_Return_400
- ❌ **MISSING**: Register_With_Duplicate_Email_Should_Return_400
- ❌ **MISSING**: Register_Should_Create_User_With_ParkingUser_Role
- ❌ **MISSING**: Register_With_Null_BirthYear_Should_Use_Current_Date

#### Logout Endpoint (`POST /api/v2/auth/logout`)
- ❌ **MISSING**: Logout_With_Valid_Token_Should_Return_200
- ❌ **MISSING**: Logout_Without_Token_Should_Return_401
- ❌ **MISSING**: Logout_With_Invalid_Token_Should_Return_401
- ❌ **MISSING**: Logout_Should_Revoke_Token
- ❌ **MISSING**: Logout_With_Missing_UserId_Claim_Should_Return_401
- ❌ **MISSING**: Logout_Should_Handle_Exceptions_Gracefully

---

## 2. Test_Parkinglots.cs (C_Parkinglots Controller)

### Current Coverage: ~60%
### Target Coverage: 80%+

### Missing Tests:

#### GetAll Endpoint (`GET /api/v2/parkinglots`)
- ✅ Test_GetAllParkinglots_ShouldReturnOk (EXISTS)
- ❌ **MISSING**: GetAll_Without_Token_Should_Return_401
- ❌ **MISSING**: GetAll_With_Empty_List_Should_Return_200_With_Empty_Array

#### GetById Endpoint (`GET /api/v2/parkinglots/{id}`)
- ✅ Test_GetParkinglotById_ShouldReturnOk (EXISTS)
- ✅ Test_GetById_NotFound (EXISTS)
- ❌ **MISSING**: GetById_Without_Token_Should_Return_401
- ❌ **MISSING**: GetById_With_Invalid_Guid_Should_Return_400
- ❌ **MISSING**: GetById_With_Empty_Guid_Should_Return_400

#### Create Endpoint (`POST /api/v2/parkinglots`)
- ✅ Test_CreateParkinglot_ShouldReturnCreated (EXISTS)
- ✅ Test_CreateParkinglot_BadData (EXISTS)
- ❌ **MISSING**: Create_Without_Token_Should_Return_401
- ❌ **MISSING**: Create_With_User_Token_Should_Return_403
- ❌ **MISSING**: Create_With_Null_Body_Should_Return_400
- ❌ **MISSING**: Create_With_Null_Name_Should_Return_400
- ❌ **MISSING**: Create_With_Empty_Name_Should_Return_400
- ❌ **MISSING**: Create_With_Null_Location_Should_Return_400
- ❌ **MISSING**: Create_With_Null_Coordinates_Should_Return_400
- ❌ **MISSING**: Create_With_Invalid_Coordinates_Should_Return_400
- ❌ **MISSING**: Create_Should_Set_Id_And_CreatedAt

#### Update Endpoint (`PUT /api/v2/parkinglots/{id}`)
- ✅ Test_UpdateParkinglot_ShouldReturnNoContent (EXISTS)
- ✅ Test_UpdateParkinglot_NotFound (EXISTS)
- ❌ **MISSING**: Update_Without_Token_Should_Return_401
- ❌ **MISSING**: Update_With_User_Token_Should_Return_403
- ❌ **MISSING**: Update_With_Invalid_Guid_Should_Return_400
- ❌ **MISSING**: Update_With_Null_Body_Should_Return_400
- ❌ **MISSING**: Update_Should_Update_All_Fields

#### Delete Endpoint (`DELETE /api/v2/parkinglots/{id}`)
- ✅ Test_DeleteParkinglot_ShouldReturnNoContent (EXISTS)
- ✅ Test_DeleteParkinglot_NotFound (EXISTS)
- ❌ **MISSING**: Delete_Without_Token_Should_Return_401
- ❌ **MISSING**: Delete_With_User_Token_Should_Return_403
- ❌ **MISSING**: Delete_With_LotAdmin_Token_Should_Return_403
- ❌ **MISSING**: Delete_With_SuperAdmin_Token_Should_Return_204
- ❌ **MISSING**: Delete_With_Invalid_Guid_Should_Return_400

#### SearchNearby Endpoint (`GET /api/v2/parkinglots/search`)
- ✅ Test_SearchNearbyParkinglots_ShouldReturnOk (EXISTS)
- ❌ **MISSING**: SearchNearby_Without_Token_Should_Return_401
- ❌ **MISSING**: SearchNearby_With_No_Query_Params_Should_Return_400
- ❌ **MISSING**: SearchNearby_With_Invalid_Lat_Should_Return_400
- ❌ **MISSING**: SearchNearby_With_Invalid_Lng_Should_Return_400
- ❌ **MISSING**: SearchNearby_With_Invalid_Radius_Should_Return_400
- ❌ **MISSING**: SearchNearby_With_Zero_Radius_Should_Return_Empty
- ❌ **MISSING**: SearchNearby_With_Large_Radius_Should_Return_All
- ❌ **MISSING**: SearchNearby_Should_Filter_By_Bounding_Box

---

## 3. Test_Payments.cs (C_Payments Controller)

### Current Coverage: ~50%
### Target Coverage: 80%+

### Missing Tests:

#### GetAllPayments Endpoint (`GET /api/v2/payments/all`)
- ✅ GetAllPayments_WithoutToken_Returns401 (EXISTS)
- ✅ GetAllPayments_WithUserToken_Returns403 (EXISTS)
- ✅ GetAllPayments_WithLotAdminToken_Returns200 (EXISTS)
- ✅ GetAllPayments_WithSuperAdminToken_Returns200 (EXISTS)
- ❌ **MISSING**: GetAllPayments_With_Negative_Page_Should_Return_400
- ❌ **MISSING**: GetAllPayments_With_Page_Exceeding_Total_Should_Return_400
- ❌ **MISSING**: GetAllPayments_Should_Return_Paginated_Response
- ❌ **MISSING**: GetAllPayments_With_Page_0_Should_Return_First_Page
- ❌ **MISSING**: GetAllPayments_Should_Return_Correct_PageSize

#### GetPaymentByID Endpoint (`GET /api/v2/payments/{id}`)
- ❌ **MISSING**: GetPaymentByID_Without_Token_Should_Return_401
- ❌ **MISSING**: GetPaymentByID_With_Valid_Id_Should_Return_200
- ❌ **MISSING**: GetPaymentByID_With_Invalid_Guid_Should_Return_400
- ❌ **MISSING**: GetPaymentByID_With_NonExistent_Id_Should_Return_404
- ❌ **MISSING**: GetPaymentByID_With_Empty_Guid_Should_Return_400

#### CreatePayment Endpoint (`POST /api/v2/payments/create`)
- ✅ CreatePayment_WithUserToken_Returns200 (EXISTS)
- ❌ **MISSING**: CreatePayment_Without_Token_Should_Return_401
- ❌ **MISSING**: CreatePayment_With_Null_Body_Should_Return_400
- ❌ **MISSING**: CreatePayment_With_Invalid_Data_Should_Return_400
- ❌ **MISSING**: CreatePayment_Should_Set_CreatedAt_If_Not_Provided
- ❌ **MISSING**: CreatePayment_Should_Set_Completed_If_Not_Provided
- ❌ **MISSING**: CreatePayment_Should_Generate_Hash_If_Not_Provided

#### UpdatePayment Endpoint (`PUT /api/v2/payments/update/{id}`)
- ✅ UpdatePayment_WithUserToken_Returns403 (EXISTS)
- ✅ UpdatePayment_WithLotAdminToken_Returns200 (EXISTS)
- ❌ **MISSING**: UpdatePayment_Without_Token_Should_Return_401
- ❌ **MISSING**: UpdatePayment_With_Invalid_Guid_Should_Return_400
- ❌ **MISSING**: UpdatePayment_With_NonExistent_Id_Should_Return_404
- ❌ **MISSING**: UpdatePayment_With_Null_Body_Should_Return_400
- ❌ **MISSING**: UpdatePayment_With_SuperAdmin_Token_Should_Return_200

#### DeletePayment Endpoint (`DELETE /api/v2/payments/delete/{id}`)
- ✅ DeletePayment_WithUserToken_Returns403 (EXISTS)
- ✅ DeletePayment_WithLotAdminToken_Returns403 (EXISTS)
- ✅ DeletePayment_WithSuperAdminToken_Returns200 (EXISTS)
- ❌ **MISSING**: DeletePayment_Without_Token_Should_Return_401
- ❌ **MISSING**: DeletePayment_With_Invalid_Guid_Should_Return_400
- ❌ **MISSING**: DeletePayment_With_NonExistent_Id_Should_Return_404

#### RefundPayment Endpoint (`POST /api/v2/payments/{id}/refund`)
- ❌ **MISSING**: RefundPayment_Without_Token_Should_Return_401
- ❌ **MISSING**: RefundPayment_With_User_Token_Should_Return_403
- ❌ **MISSING**: RefundPayment_With_Valid_Data_Should_Return_200
- ❌ **MISSING**: RefundPayment_With_Empty_Guid_Should_Return_400
- ❌ **MISSING**: RefundPayment_With_Invalid_Guid_Should_Return_400
- ❌ **MISSING**: RefundPayment_With_Null_Reason_Should_Return_400
- ❌ **MISSING**: RefundPayment_With_Empty_Reason_Should_Return_400
- ❌ **MISSING**: RefundPayment_With_NonExistent_Payment_Should_Return_404
- ❌ **MISSING**: RefundPayment_Should_Create_Billing_Entry
- ❌ **MISSING**: RefundPayment_With_Missing_UserId_Claim_Should_Return_401
- ❌ **MISSING**: RefundPayment_With_Invalid_Operation_Should_Return_400
- ❌ **MISSING**: RefundPayment_Should_Handle_Exceptions_Gracefully

---

## 4. Test_Reservations.cs & Test_Reservation.cs (C_Reservations Controller)

### Current Coverage: ~55%
### Target Coverage: 80%+

### Missing Tests:

#### GetAllReservations Endpoint (`GET /api/v2/reservations/all`)
- ✅ GetReservations_ReturnsOk (EXISTS)
- ✅ GetAllReservations_WithoutToken_Returns401 (EXISTS)
- ✅ GetAllReservations_WithUserToken_Returns403 (EXISTS)
- ✅ GetAllReservations_WithLotAdminToken_Returns200 (EXISTS)
- ✅ GetAllReservations_WithSuperAdminToken_Returns200 (EXISTS)
- ✅ GetAllReservations_PageBeyondTotal_ReturnsBadRequest (EXISTS)
- ❌ **MISSING**: GetAllReservations_With_Negative_Page_Should_Return_400
- ❌ **MISSING**: GetAllReservations_Should_Return_Paginated_Response
- ❌ **MISSING**: GetAllReservations_With_No_Reservations_Should_Return_Empty

#### CreateReservation Endpoint (`POST /api/v2/reservations/create`)
- ✅ CreateReservation_ReturnsOk_WithBody (EXISTS)
- ✅ CreateReservation_ValidData_Returns200 (EXISTS)
- ✅ CreateReservation_BadData_ReturnsBadRequest (EXISTS)
- ✅ CreateReservation_InvalidTimeRange_ReturnsBadRequest (EXISTS)
- ✅ CreateReservation_MissingData_Returns400 (EXISTS)
- ❌ **MISSING**: CreateReservation_Without_Token_Should_Return_401
- ❌ **MISSING**: CreateReservation_With_Null_Body_Should_Return_400
- ❌ **MISSING**: CreateReservation_With_Invalid_ModelState_Should_Return_400
- ❌ **MISSING**: CreateReservation_With_StartTime_Equals_EndTime_Should_Return_400
- ❌ **MISSING**: CreateReservation_With_Empty_Guid_Should_Return_400
- ❌ **MISSING**: CreateReservation_Should_Set_Status_To_Active
- ❌ **MISSING**: CreateReservation_Should_Set_CreatedAt

#### GetReservationById Endpoint (`GET /api/v2/reservations/{id}`)
- ✅ GetReservationById_ReturnsOkAndBody (EXISTS)
- ✅ GetReservationById_UnknownId_Returns404 (EXISTS)
- ✅ GetReservationById_EmptyId_ReturnsBadRequest (EXISTS)
- ❌ **MISSING**: GetReservationById_Without_Token_Should_Return_401
- ❌ **MISSING**: GetReservationById_With_User_Viewing_Other_User_Reservation_Should_Return_403
- ❌ **MISSING**: GetReservationById_With_Admin_Should_Return_200
- ❌ **MISSING**: GetReservationById_With_Invalid_Guid_Should_Return_400

#### CancelReservation Endpoint (`POST /api/v2/reservations/cancel/{id}`)
- ✅ CreateAndCancelReservation_ReturnsOkAndThenNotFound (EXISTS)
- ✅ CancelReservation_InvalidId_ReturnsBadRequest (EXISTS)
- ✅ CancelReservation_NotFound_Returns404 (EXISTS)
- ✅ CancelReservation_HappyFlow_ReturnsOk (EXISTS)
- ❌ **MISSING**: CancelReservation_Without_Token_Should_Return_401
- ❌ **MISSING**: CancelReservation_With_User_Cancelling_Other_User_Reservation_Should_Return_403
- ❌ **MISSING**: CancelReservation_With_Admin_Should_Return_200
- ❌ **MISSING**: CancelReservation_With_Invalid_Guid_Should_Return_400
- ❌ **MISSING**: CancelReservation_Should_Handle_Exceptions_Gracefully

#### ListReservationsByUser Endpoint (`GET /api/v2/reservations/user/{userId}`)
- ✅ ListReservationsByUser_InvalidUserId_ReturnsBadRequest (EXISTS)
- ✅ ListReservationsByUser_HappyFlow_ReturnsOk (EXISTS)
- ❌ **MISSING**: ListReservationsByUser_Without_Token_Should_Return_401
- ❌ **MISSING**: ListReservationsByUser_With_User_Viewing_Other_User_Reservations_Should_Return_403
- ❌ **MISSING**: ListReservationsByUser_With_Admin_Should_Return_200
- ❌ **MISSING**: ListReservationsByUser_With_Empty_Guid_Should_Return_400
- ❌ **MISSING**: ListReservationsByUser_With_Status_Filter_Should_Return_Filtered

#### CheckAvailability Endpoint (`GET /api/v2/reservations/check-availability/parking-lots/{parkingLotId}`)
- ✅ CheckAvailability_InvalidRange_ReturnsBadRequest (EXISTS)
- ✅ CheckAvailability_InvalidLotId_ReturnsBadRequest (EXISTS)
- ✅ CheckAvailability_HappyFlow_ReturnsOk (EXISTS)
- ❌ **MISSING**: CheckAvailability_Without_Token_Should_Return_401
- ❌ **MISSING**: CheckAvailability_With_StartTime_Equals_EndTime_Should_Return_400
- ❌ **MISSING**: CheckAvailability_With_Empty_Guid_Should_Return_400
- ❌ **MISSING**: CheckAvailability_With_Overlapping_Reservation_Should_Return_NotAvailable
- ❌ **MISSING**: CheckAvailability_With_No_Overlapping_Reservation_Should_Return_Available

#### CreateReservationForUser Endpoint (`POST /api/v2/reservations/admin/create-for-user`)
- ❌ **MISSING**: CreateReservationForUser_Without_Token_Should_Return_401
- ❌ **MISSING**: CreateReservationForUser_With_User_Token_Should_Return_403
- ❌ **MISSING**: CreateReservationForUser_With_Admin_Token_Should_Return_200
- ❌ **MISSING**: CreateReservationForUser_With_Null_Body_Should_Return_400
- ❌ **MISSING**: CreateReservationForUser_With_Empty_Guid_Should_Return_400
- ❌ **MISSING**: CreateReservationForUser_With_Invalid_TimeRange_Should_Return_400
- ❌ **MISSING**: CreateReservationForUser_Should_Create_Reservation_With_Specified_Status

---

## 5. Test_Users.cs (C_Users Controller)

### Current Coverage: ~65%
### Target Coverage: 80%+

### Missing Tests:

#### GetAllUsers Endpoint (`GET /api/v2/users/all`)
- ✅ Test_Pagination_HappyFlow (EXISTS)
- ✅ Test_Pagination_NegativePage (EXISTS)
- ✅ Test_Pagination_EmptyPage (EXISTS)
- ❌ **MISSING**: GetAllUsers_Without_Token_Should_Return_401
- ❌ **MISSING**: GetAllUsers_With_User_Token_Should_Return_403
- ❌ **MISSING**: GetAllUsers_With_LotAdmin_Token_Should_Return_403
- ❌ **MISSING**: GetAllUsers_With_SuperAdmin_Token_Should_Return_200
- ❌ **MISSING**: GetAllUsers_With_Page_Exceeding_Total_Should_Return_400
- ❌ **MISSING**: GetAllUsers_Should_Return_Paginated_Response

#### GetUserByID Endpoint (`GET /api/v2/users/{id}`)
- ✅ Test_GetById_UnknownID (EXISTS)
- ❌ **MISSING**: GetUserByID_Without_Token_Should_Return_401
- ❌ **MISSING**: GetUserByID_With_Valid_Id_Should_Return_200
- ❌ **MISSING**: GetUserByID_With_Invalid_Guid_Should_Return_400
- ❌ **MISSING**: GetUserByID_With_Empty_Guid_Should_Return_400

#### CreateUser Endpoint (`POST /api/v2/users/create`)
- ✅ Test_Create_HappyFlow (EXISTS)
- ✅ Test_Create_NotPossible_BirthDate (EXISTS)
- ✅ Test_Create_WrongData (EXISTS)
- ✅ Test_Invalid_Email (EXISTS)
- ❌ **MISSING**: CreateUser_Without_Token_Should_Return_401
- ❌ **MISSING**: CreateUser_With_User_Token_Should_Return_403
- ❌ **MISSING**: CreateUser_With_LotAdmin_Token_Should_Return_403
- ❌ **MISSING**: CreateUser_With_SuperAdmin_Token_Should_Return_201
- ❌ **MISSING**: CreateUser_With_Null_Body_Should_Return_400
- ❌ **MISSING**: CreateUser_With_Empty_Username_Should_Return_400
- ❌ **MISSING**: CreateUser_With_Empty_Password_Should_Return_400
- ❌ **MISSING**: CreateUser_With_Empty_Name_Should_Return_400
- ❌ **MISSING**: CreateUser_With_Empty_Email_Should_Return_400
- ❌ **MISSING**: CreateUser_With_Empty_Phone_Should_Return_400
- ❌ **MISSING**: CreateUser_Should_Hash_Password
- ❌ **MISSING**: CreateUser_With_Invalid_Role_Should_Return_400

#### UpdateUser Endpoint (`PUT /api/v2/users/update/{id}`)
- ✅ Test_Update_GoodData_WrongID (EXISTS)
- ✅ Test_Update_WrongData (EXISTS)
- ✅ Test_User_HappyFlow (EXISTS - includes update)
- ❌ **MISSING**: UpdateUser_Without_Token_Should_Return_401
- ❌ **MISSING**: UpdateUser_With_User_Token_Should_Return_403
- ❌ **MISSING**: UpdateUser_With_LotAdmin_Token_Should_Return_403
- ❌ **MISSING**: UpdateUser_With_SuperAdmin_Token_Should_Return_204
- ❌ **MISSING**: UpdateUser_With_Invalid_Guid_Should_Return_400
- ❌ **MISSING**: UpdateUser_With_Null_Body_Should_Return_400
- ❌ **MISSING**: UpdateUser_Should_Update_All_Fields

#### DeleteUser Endpoint (`DELETE /api/v2/users/delete/{id}`)
- ✅ Test_Delete_WrongID (EXISTS)
- ✅ Test_User_HappyFlow (EXISTS - includes delete)
- ❌ **MISSING**: DeleteUser_Without_Token_Should_Return_401
- ❌ **MISSING**: DeleteUser_With_User_Token_Should_Return_403
- ❌ **MISSING**: DeleteUser_With_LotAdmin_Token_Should_Return_403
- ❌ **MISSING**: DeleteUser_With_SuperAdmin_Token_Should_Return_204
- ❌ **MISSING**: DeleteUser_With_Invalid_Guid_Should_Return_400

---

## 6. Test_Vehicles.cs (C_Vehicles Controller)

### Current Coverage: ~60%
### Target Coverage: 80%+

### Missing Tests:

#### GetAllVehicles Endpoint (`GET /api/v2/vehicles/all`)
- ✅ Test_Pagination_HappyFlow (EXISTS)
- ✅ Test_Pagination_InvalidPageNumber (EXISTS)
- ✅ Test_Pagination_NonIntegerPageNumber (EXISTS)
- ✅ Test_Pagination_MissingPageNumber (EXISTS)
- ❌ **MISSING**: GetAllVehicles_Without_Token_Should_Return_401
- ❌ **MISSING**: GetAllVehicles_With_User_Token_Should_Return_403
- ❌ **MISSING**: GetAllVehicles_With_LotAdmin_Token_Should_Return_200
- ❌ **MISSING**: GetAllVehicles_With_SuperAdmin_Token_Should_Return_200
- ❌ **MISSING**: GetAllVehicles_With_Page_Exceeding_Total_Should_Return_400

#### GetVehicleByID Endpoint (`GET /api/v2/vehicles/{id}`)
- ✅ Test_GetById_UnknownID (EXISTS)
- ✅ Test_GetById_InvalidIDFormat (EXISTS)
- ❌ **MISSING**: GetVehicleByID_Without_Token_Should_Return_401
- ❌ **MISSING**: GetVehicleByID_With_User_Viewing_Other_User_Vehicle_Should_Return_403
- ❌ **MISSING**: GetVehicleByID_With_User_Viewing_Own_Vehicle_Should_Return_200
- ❌ **MISSING**: GetVehicleByID_With_Admin_Should_Return_200
- ❌ **MISSING**: GetVehicleByID_With_Empty_Guid_Should_Return_400

#### CreateVehicle Endpoint (`POST /api/v2/vehicles/create`)
- ✅ Test_Create_HappyFlow (EXISTS)
- ✅ Test_Create_BadData (EXISTS)
- ✅ Test_Create_EmptyData (EXISTS)
- ✅ Test_Create_NotPossible_Year (EXISTS)
- ✅ Test_Create_NotPossible_LicensePlate (EXISTS)
- ❌ **MISSING**: CreateVehicle_Without_Token_Should_Return_401
- ❌ **MISSING**: CreateVehicle_With_Null_Body_Should_Return_400
- ❌ **MISSING**: CreateVehicle_With_User_Should_Set_User_Id_To_Current_User
- ❌ **MISSING**: CreateVehicle_With_Admin_Should_Allow_Any_User_Id
- ❌ **MISSING**: CreateVehicle_With_Admin_And_Empty_User_Id_Should_Use_Current_User
- ❌ **MISSING**: CreateVehicle_With_Whitespace_Only_Fields_Should_Return_400
- ❌ **MISSING**: CreateVehicle_With_Year_Equals_Current_Year_Should_Return_200

#### UpdateVehicle Endpoint (`PUT /api/v2/vehicles/update/{id}`)
- ✅ Test_Update_WrongID (EXISTS)
- ✅ Test_FullFlow_HappyPath (EXISTS - includes update)
- ❌ **MISSING**: UpdateVehicle_Without_Token_Should_Return_401
- ❌ **MISSING**: UpdateVehicle_With_User_Updating_Other_User_Vehicle_Should_Return_403
- ❌ **MISSING**: UpdateVehicle_With_User_Updating_Own_Vehicle_Should_Return_204
- ❌ **MISSING**: UpdateVehicle_With_Admin_Should_Return_204
- ❌ **MISSING**: UpdateVehicle_With_User_Should_Prevent_Ownership_Change
- ❌ **MISSING**: UpdateVehicle_With_Admin_Should_Allow_Ownership_Change
- ❌ **MISSING**: UpdateVehicle_With_Null_Body_Should_Return_400
- ❌ **MISSING**: UpdateVehicle_With_Invalid_Guid_Should_Return_400

#### DeleteVehicle Endpoint (`DELETE /api/v2/vehicles/delete/{id}`)
- ✅ Test_FullFlow_HappyPath (EXISTS - includes delete)
- ❌ **MISSING**: DeleteVehicle_Without_Token_Should_Return_401
- ❌ **MISSING**: DeleteVehicle_With_User_Deleting_Other_User_Vehicle_Should_Return_403
- ❌ **MISSING**: DeleteVehicle_With_User_Deleting_Own_Vehicle_Should_Return_204
- ❌ **MISSING**: DeleteVehicle_With_Admin_Should_Return_204
- ❌ **MISSING**: DeleteVehicle_With_Invalid_Guid_Should_Return_400
- ❌ **MISSING**: DeleteVehicle_With_NonExistent_Id_Should_Return_404

---

## 7. Service Layer Tests (ALL MISSING - CRITICAL)

### Service Tests Should Be Unit Tests with Mocked Dependencies

#### Test_Service_Payments.cs
- ❌ **MISSING**: GetAllPayments_Should_Return_All_Payments
- ❌ **MISSING**: GetByID_With_Valid_Id_Should_Return_Payment
- ❌ **MISSING**: GetByID_With_Invalid_Id_Should_Throw_Exception
- ❌ **MISSING**: CreatePayment_With_Valid_Data_Should_Create_Payment
- ❌ **MISSING**: CreatePayment_With_Null_Model_Should_Throw_ArgumentNullException
- ❌ **MISSING**: CreatePayment_Should_Set_CreatedAt_If_Not_Provided
- ❌ **MISSING**: CreatePayment_Should_Set_Completed_If_Not_Provided
- ❌ **MISSING**: CreatePayment_Should_Generate_Hash_If_Not_Provided
- ❌ **MISSING**: CreatePayment_Should_Validate_Hash_If_Provided
- ❌ **MISSING**: UpdatePayment_With_Valid_Data_Should_Update_Payment
- ❌ **MISSING**: UpdatePayment_With_Invalid_Id_Should_Throw_Exception
- ❌ **MISSING**: DeletePayment_With_Valid_Id_Should_Delete_Payment
- ❌ **MISSING**: DeletePayment_With_Invalid_Id_Should_Throw_Exception
- ❌ **MISSING**: RefundPayment_With_Valid_Data_Should_Refund_Payment
- ❌ **MISSING**: RefundPayment_With_Invalid_Id_Should_Throw_Exception
- ❌ **MISSING**: RefundPayment_Should_Create_Refund_Payment_Record

#### Test_Service_Reservations.cs
- ❌ **MISSING**: Create_With_Valid_Reservation_Should_Create_Reservation
- ❌ **MISSING**: Create_With_Null_Reservation_Should_Throw_ArgumentNullException
- ❌ **MISSING**: Cancel_With_Valid_Id_Should_Cancel_Reservation
- ❌ **MISSING**: Cancel_With_Invalid_Id_Should_Throw_Exception
- ❌ **MISSING**: GetById_With_Valid_Id_Should_Return_Reservation
- ❌ **MISSING**: GetById_With_Invalid_Id_Should_Return_Null
- ❌ **MISSING**: ListByUser_With_Valid_UserId_Should_Return_Reservations
- ❌ **MISSING**: ListByUser_With_Status_Filter_Should_Return_Filtered
- ❌ **MISSING**: CheckAvailability_With_No_Overlap_Should_Return_Available
- ❌ **MISSING**: CheckAvailability_With_Overlap_Should_Return_NotAvailable
- ❌ **MISSING**: CheckAvailability_With_Invalid_ParkingLotId_Should_Return_Available
- ❌ **MISSING**: GetAllReservations_Should_Return_All_Reservations

#### Test_Service_Users.cs
- ❌ **MISSING**: GetAllUsers_Should_Return_All_Users
- ❌ **MISSING**: GetByID_With_Valid_Id_Should_Return_User
- ❌ **MISSING**: GetByID_With_Invalid_Id_Should_Return_Null
- ❌ **MISSING**: CreateUser_With_Valid_Data_Should_Create_User
- ❌ **MISSING**: CreateUser_With_Null_User_Should_Throw_Exception
- ❌ **MISSING**: UpdateProfile_With_Valid_Data_Should_Update_User
- ❌ **MISSING**: UpdateProfile_With_Invalid_Id_Should_Throw_Exception
- ❌ **MISSING**: DeleteUser_With_Valid_Id_Should_Delete_User
- ❌ **MISSING**: DeleteUser_With_Invalid_Id_Should_Throw_Exception

#### Test_Service_Vehicles.cs
- ❌ **MISSING**: GetAllVehicles_Should_Return_All_Vehicles
- ❌ **MISSING**: GetByID_With_Valid_Id_Should_Return_Vehicle
- ❌ **MISSING**: GetByID_With_Invalid_Id_Should_Return_Null
- ❌ **MISSING**: CreateVehicle_With_Valid_Data_Should_Create_Vehicle
- ❌ **MISSING**: CreateVehicle_With_Null_Vehicle_Should_Throw_Exception
- ❌ **MISSING**: UpdateVehicle_With_Valid_Data_Should_Update_Vehicle
- ❌ **MISSING**: UpdateVehicle_With_Invalid_Id_Should_Throw_Exception
- ❌ **MISSING**: DeleteVehicle_With_Valid_Id_Should_Delete_Vehicle
- ❌ **MISSING**: DeleteVehicle_With_Invalid_Id_Should_Throw_Exception

#### Test_Service_UserBalance.cs
- ❌ **MISSING**: GetBalanceForUser_With_Valid_UserId_Should_Return_Balance
- ❌ **MISSING**: GetBalanceForUser_With_No_Balance_Should_Return_Null
- ❌ **MISSING**: CreateBalance_With_Valid_UserId_Should_Create_Balance
- ❌ **MISSING**: CreateBalance_With_Existing_Balance_Should_Throw_Exception
- ❌ **MISSING**: AddToBalance_With_Valid_Amount_Should_Increase_Balance
- ❌ **MISSING**: AddToBalance_With_Negative_Amount_Should_Throw_Exception
- ❌ **MISSING**: DeductFromBalance_With_Sufficient_Balance_Should_Decrease_Balance
- ❌ **MISSING**: DeductFromBalance_With_Insufficient_Balance_Should_Throw_Exception
- ❌ **MISSING**: HasSufficientBalance_With_Sufficient_Balance_Should_Return_True
- ❌ **MISSING**: HasSufficientBalance_With_Insufficient_Balance_Should_Return_False
- ❌ **MISSING**: GetTransactionHistory_Should_Return_Transactions
- ❌ **MISSING**: RecordTransaction_Should_Create_Transaction_Record

#### Test_Service_Sessions.cs
- ❌ **MISSING**: Start_With_Valid_Session_Should_Create_Session
- ❌ **MISSING**: Start_With_Active_Session_Exists_Should_Throw_Exception
- ❌ **MISSING**: Start_With_Exceeded_Capacity_Should_Throw_Exception
- ❌ **MISSING**: Stop_With_Valid_Id_Should_Stop_Session
- ❌ **MISSING**: Stop_With_Invalid_Id_Should_Return_Null
- ❌ **MISSING**: GetSessionById_Should_Return_Sessions
- ❌ **MISSING**: GetAllSessions_Should_Return_All_Sessions
- ❌ **MISSING**: Pay_With_Valid_Id_Should_Mark_As_Paid

#### Test_Service_Token.cs
- ❌ **MISSING**: GenerateToken_Should_Return_Valid_Token
- ❌ **MISSING**: GenerateToken_Should_Include_User_Claims
- ❌ **MISSING**: GenerateToken_Should_Set_Expiration
- ❌ **MISSING**: ValidateToken_With_Valid_Token_Should_Return_True
- ❌ **MISSING**: ValidateToken_With_Expired_Token_Should_Return_False
- ❌ **MISSING**: ValidateToken_With_Invalid_Token_Should_Return_False

#### Test_Service_TokenRevocation.cs
- ❌ **MISSING**: RevokeToken_Should_Add_Token_To_Revoked_List
- ❌ **MISSING**: IsTokenRevoked_With_Revoked_Token_Should_Return_True
- ❌ **MISSING**: IsTokenRevoked_With_NonRevoked_Token_Should_Return_False
- ❌ **MISSING**: CleanupExpiredTokens_Should_Remove_Expired_Tokens

---

## Test Template Structure

### For Each Test File:

```csharp
using CSharpAPI.Tests.Utillities;
using CSharpAPI.Models;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace CSharpAPI.Tests.APITests
{
    public class Test_[ControllerName] : IClassFixture<CSharpAPITests>
    {
        private readonly CSharpAPITests _factory;
        
        public Test_[ControllerName](CSharpAPITests factory) => _factory = factory;

        // ========== HAPPY PATH TESTS ==========
        
        [Fact]
        public async Task [Endpoint]_With_Valid_Data_Should_Return_[ExpectedStatus]()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            
            // Act
            var response = await client.[Method]Async("[endpoint]", [data]);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.[ExpectedStatus]);
            // Additional assertions
        }

        // ========== SAD PATH TESTS ==========
        
        [Fact]
        public async Task [Endpoint]_With_Invalid_Data_Should_Return_BadRequest()
        {
            // Test invalid inputs
        }

        [Fact]
        public async Task [Endpoint]_With_NonExistent_Id_Should_Return_NotFound()
        {
            // Test not found scenarios
        }

        // ========== AUTHORIZATION TESTS ==========
        
        [Fact]
        public async Task [Endpoint]_Without_Token_Should_Return_401()
        {
            // Test unauthorized access
        }

        [Fact]
        public async Task [Endpoint]_With_User_Token_Should_Return_403()
        {
            // Test forbidden access for regular users
        }

        [Fact]
        public async Task [Endpoint]_With_Admin_Token_Should_Return_200()
        {
            // Test admin access
        }

        // ========== EDGE CASE TESTS ==========
        
        [Fact]
        public async Task [Endpoint]_With_Null_Body_Should_Return_400()
        {
            // Test null inputs
        }

        [Fact]
        public async Task [Endpoint]_With_Empty_Guid_Should_Return_400()
        {
            // Test empty GUIDs
        }

        [Fact]
        public async Task [Endpoint]_With_Invalid_Guid_Should_Return_400()
        {
            // Test invalid GUID formats
        }

        // ========== INTEGRATION TESTS ==========
        
        [Fact]
        public async Task [FullFlow]_Should_Work_Correctly()
        {
            // Test complete CRUD flow
        }
    }
}
```

---

## Priority Order for Implementation

### Phase 1: Critical Missing Tests (Highest Impact)
1. **Service Layer Tests** - All 13 services (0% coverage currently)
2. **C_Auth** - Register and Logout endpoints (completely missing)
3. **C_Payments** - RefundPayment endpoint (completely missing)
4. **C_Reservations** - CreateReservationForUser endpoint (completely missing)

### Phase 2: Authorization Gaps
5. All controllers - Missing authorization tests for various roles
6. All controllers - Missing unauthorized access tests

### Phase 3: Edge Cases
7. All controllers - Missing null/empty/invalid input tests
8. All controllers - Missing boundary condition tests

### Phase 4: Integration Flows
9. All controllers - Missing full CRUD flow tests
10. All controllers - Missing pagination edge cases

---

## Coverage Calculation

To reach 80% coverage:
- **Current Estimated Coverage**: ~45-50%
- **Missing Coverage**: ~30-35%
- **Target**: 80%

### Breakdown:
- **Controllers**: Need ~200+ additional test cases
- **Services**: Need ~150+ additional test cases
- **Total**: ~350+ test cases needed

### Focus Areas:
1. **Service Layer** (0% → 80%): ~150 tests
2. **Controller Edge Cases** (50% → 80%): ~100 tests
3. **Authorization** (40% → 80%): ~50 tests
4. **Integration Flows** (60% → 80%): ~50 tests

---

## Notes

- Use **Moq** for mocking dependencies in service tests
- Use **FluentAssertions** for readable assertions (already in use)
- Use **xUnit** for test framework (already in use)
- Consider **AutoFixture** for generating test data
- Run coverage reports after each batch of tests
- Aim for at least 3-5 tests per endpoint (happy, sad, auth, edge cases)
