# Test Coverage Checklist - 80% Target

This checklist tracks all tests that need to be implemented to reach 80% code coverage.

## Legend
- ✅ = Test exists
- ❌ = Test missing
- 🔄 = Test needs improvement/expansion

---

## 1. C_Auth Controller (Test_Authentication.cs)

### Login Endpoint
- ✅ Login_Should_Return_Token_For_Valid_Credentials
- ✅ Login_With_Invalid_Credentials_Should_Return_401
- ❌ Login_With_Null_Username_Should_Return_400
- ❌ Login_With_Empty_Username_Should_Return_400
- ❌ Login_With_Null_Password_Should_Return_400
- ❌ Login_With_Empty_Password_Should_Return_400
- ❌ Login_With_Whitespace_Only_Should_Return_400
- ❌ Login_With_Inactive_User_Should_Return_401
- ❌ Login_With_User_No_Password_Should_Return_401
- ❌ Login_With_Legacy_Hash_Should_Upgrade_To_BCrypt
- ❌ Login_Should_Return_Token_With_ExpiresAt

### Me Endpoint
- ✅ Get_Me_Endpoint_Without_Token_Should_Return_401
- ❌ Get_Me_Endpoint_With_Valid_Token_Should_Return_User_Info
- ❌ Get_Me_Endpoint_With_Expired_Token_Should_Return_401
- ❌ Get_Me_Endpoint_With_Invalid_Token_Should_Return_401
- ❌ Get_Me_Endpoint_Should_Return_Correct_Claims

### Register Endpoint
- ❌ Register_With_Valid_Data_Should_Return_200
- ❌ Register_With_Null_Username_Should_Return_400
- ❌ Register_With_Empty_Username_Should_Return_400
- ❌ Register_With_Null_Password_Should_Return_400
- ❌ Register_With_Password_Mismatch_Should_Return_400
- ❌ Register_With_Invalid_Email_Should_Return_400
- ❌ Register_With_Invalid_Phone_Should_Return_400
- ❌ Register_With_Duplicate_Username_Should_Return_400
- ❌ Register_With_Duplicate_Email_Should_Return_400
- ❌ Register_Should_Create_User_With_ParkingUser_Role
- ❌ Register_With_Null_BirthYear_Should_Use_Current_Date

### Logout Endpoint
- ❌ Logout_With_Valid_Token_Should_Return_200
- ❌ Logout_Without_Token_Should_Return_401
- ❌ Logout_With_Invalid_Token_Should_Return_401
- ❌ Logout_Should_Revoke_Token
- ❌ Logout_With_Missing_UserId_Claim_Should_Return_401
- ❌ Logout_Should_Handle_Exceptions_Gracefully

**Total: 6/32 (19%)**

---

## 2. C_Parkinglots Controller (Test_Parkinglots.cs)

### GetAll Endpoint
- ✅ Test_GetAllParkinglots_ShouldReturnOk
- ❌ GetAll_Without_Token_Should_Return_401
- ❌ GetAll_With_Empty_List_Should_Return_200_With_Empty_Array

### GetById Endpoint
- ✅ Test_GetParkinglotById_ShouldReturnOk
- ✅ Test_GetById_NotFound
- ❌ GetById_Without_Token_Should_Return_401
- ❌ GetById_With_Invalid_Guid_Should_Return_400
- ❌ GetById_With_Empty_Guid_Should_Return_400

### Create Endpoint
- ✅ Test_CreateParkinglot_ShouldReturnCreated
- ✅ Test_CreateParkinglot_BadData
- ❌ Create_Without_Token_Should_Return_401
- ❌ Create_With_User_Token_Should_Return_403
- ❌ Create_With_Null_Body_Should_Return_400
- ❌ Create_With_Null_Name_Should_Return_400
- ❌ Create_With_Empty_Name_Should_Return_400
- ❌ Create_With_Null_Location_Should_Return_400
- ❌ Create_With_Null_Coordinates_Should_Return_400
- ❌ Create_With_Invalid_Coordinates_Should_Return_400
- ❌ Create_Should_Set_Id_And_CreatedAt

### Update Endpoint
- ✅ Test_UpdateParkinglot_ShouldReturnNoContent
- ✅ Test_UpdateParkinglot_NotFound
- ❌ Update_Without_Token_Should_Return_401
- ❌ Update_With_User_Token_Should_Return_403
- ❌ Update_With_Invalid_Guid_Should_Return_400
- ❌ Update_With_Null_Body_Should_Return_400
- ❌ Update_Should_Update_All_Fields

### Delete Endpoint
- ✅ Test_DeleteParkinglot_ShouldReturnNoContent
- ✅ Test_DeleteParkinglot_NotFound
- ❌ Delete_Without_Token_Should_Return_401
- ❌ Delete_With_User_Token_Should_Return_403
- ❌ Delete_With_LotAdmin_Token_Should_Return_403
- ❌ Delete_With_SuperAdmin_Token_Should_Return_204
- ❌ Delete_With_Invalid_Guid_Should_Return_400

### SearchNearby Endpoint
- ✅ Test_SearchNearbyParkinglots_ShouldReturnOk
- ❌ SearchNearby_Without_Token_Should_Return_401
- ❌ SearchNearby_With_No_Query_Params_Should_Return_400
- ❌ SearchNearby_With_Invalid_Lat_Should_Return_400
- ❌ SearchNearby_With_Invalid_Lng_Should_Return_400
- ❌ SearchNearby_With_Invalid_Radius_Should_Return_400
- ❌ SearchNearby_With_Zero_Radius_Should_Return_Empty
- ❌ SearchNearby_With_Large_Radius_Should_Return_All
- ❌ SearchNearby_Should_Filter_By_Bounding_Box

**Total: 8/40 (20%)**

---

## 3. C_Payments Controller (Test_Payments.cs)

### GetAllPayments Endpoint
- ✅ GetAllPayments_WithoutToken_Returns401
- ✅ GetAllPayments_WithUserToken_Returns403
- ✅ GetAllPayments_WithLotAdminToken_Returns200
- ✅ GetAllPayments_WithSuperAdminToken_Returns200
- ❌ GetAllPayments_With_Negative_Page_Should_Return_400
- ❌ GetAllPayments_With_Page_Exceeding_Total_Should_Return_400
- ❌ GetAllPayments_Should_Return_Paginated_Response
- ❌ GetAllPayments_With_Page_0_Should_Return_First_Page
- ❌ GetAllPayments_Should_Return_Correct_PageSize

### GetPaymentByID Endpoint
- ❌ GetPaymentByID_Without_Token_Should_Return_401
- ❌ GetPaymentByID_With_Valid_Id_Should_Return_200
- ❌ GetPaymentByID_With_Invalid_Guid_Should_Return_400
- ❌ GetPaymentByID_With_NonExistent_Id_Should_Return_404
- ❌ GetPaymentByID_With_Empty_Guid_Should_Return_400

### CreatePayment Endpoint
- ✅ CreatePayment_WithUserToken_Returns200
- ❌ CreatePayment_Without_Token_Should_Return_401
- ❌ CreatePayment_With_Null_Body_Should_Return_400
- ❌ CreatePayment_With_Invalid_Data_Should_Return_400
- ❌ CreatePayment_Should_Set_CreatedAt_If_Not_Provided
- ❌ CreatePayment_Should_Set_Completed_If_Not_Provided
- ❌ CreatePayment_Should_Generate_Hash_If_Not_Provided

### UpdatePayment Endpoint
- ✅ UpdatePayment_WithUserToken_Returns403
- ✅ UpdatePayment_WithLotAdminToken_Returns200
- ❌ UpdatePayment_Without_Token_Should_Return_401
- ❌ UpdatePayment_With_Invalid_Guid_Should_Return_400
- ❌ UpdatePayment_With_NonExistent_Id_Should_Return_404
- ❌ UpdatePayment_With_Null_Body_Should_Return_400
- ❌ UpdatePayment_With_SuperAdmin_Token_Should_Return_200

### DeletePayment Endpoint
- ✅ DeletePayment_WithUserToken_Returns403
- ✅ DeletePayment_WithLotAdminToken_Returns403
- ✅ DeletePayment_WithSuperAdminToken_Returns200
- ❌ DeletePayment_Without_Token_Should_Return_401
- ❌ DeletePayment_With_Invalid_Guid_Should_Return_400
- ❌ DeletePayment_With_NonExistent_Id_Should_Return_404

### RefundPayment Endpoint
- ❌ RefundPayment_Without_Token_Should_Return_401
- ❌ RefundPayment_With_User_Token_Should_Return_403
- ❌ RefundPayment_With_Valid_Data_Should_Return_200
- ❌ RefundPayment_With_Empty_Guid_Should_Return_400
- ❌ RefundPayment_With_Invalid_Guid_Should_Return_400
- ❌ RefundPayment_With_Null_Reason_Should_Return_400
- ❌ RefundPayment_With_Empty_Reason_Should_Return_400
- ❌ RefundPayment_With_NonExistent_Payment_Should_Return_404
- ❌ RefundPayment_Should_Create_Billing_Entry
- ❌ RefundPayment_With_Missing_UserId_Claim_Should_Return_401
- ❌ RefundPayment_With_Invalid_Operation_Should_Return_400
- ❌ RefundPayment_Should_Handle_Exceptions_Gracefully

**Total: 9/38 (24%)**

---

## 4. C_Reservations Controller (Test_Reservations.cs + Test_Reservation.cs)

### GetAllReservations Endpoint
- ✅ GetReservations_ReturnsOk
- ✅ GetAllReservations_WithoutToken_Returns401
- ✅ GetAllReservations_WithUserToken_Returns403
- ✅ GetAllReservations_WithLotAdminToken_Returns200
- ✅ GetAllReservations_WithSuperAdminToken_Returns200
- ✅ GetAllReservations_PageBeyondTotal_ReturnsBadRequest
- ❌ GetAllReservations_With_Negative_Page_Should_Return_400
- ❌ GetAllReservations_Should_Return_Paginated_Response
- ❌ GetAllReservations_With_No_Reservations_Should_Return_Empty

### CreateReservation Endpoint
- ✅ CreateReservation_ReturnsOk_WithBody
- ✅ CreateReservation_ValidData_Returns200
- ✅ CreateReservation_BadData_ReturnsBadRequest
- ✅ CreateReservation_InvalidTimeRange_ReturnsBadRequest
- ✅ CreateReservation_MissingData_Returns400
- ❌ CreateReservation_Without_Token_Should_Return_401
- ❌ CreateReservation_With_Null_Body_Should_Return_400
- ❌ CreateReservation_With_Invalid_ModelState_Should_Return_400
- ❌ CreateReservation_With_StartTime_Equals_EndTime_Should_Return_400
- ❌ CreateReservation_With_Empty_Guid_Should_Return_400
- ❌ CreateReservation_Should_Set_Status_To_Active
- ❌ CreateReservation_Should_Set_CreatedAt

### GetReservationById Endpoint
- ✅ GetReservationById_ReturnsOkAndBody
- ✅ GetReservationById_UnknownId_Returns404
- ✅ GetReservationById_EmptyId_ReturnsBadRequest
- ❌ GetReservationById_Without_Token_Should_Return_401
- ❌ GetReservationById_With_User_Viewing_Other_User_Reservation_Should_Return_403
- ❌ GetReservationById_With_Admin_Should_Return_200
- ❌ GetReservationById_With_Invalid_Guid_Should_Return_400

### CancelReservation Endpoint
- ✅ CreateAndCancelReservation_ReturnsOkAndThenNotFound
- ✅ CancelReservation_InvalidId_ReturnsBadRequest
- ✅ CancelReservation_NotFound_Returns404
- ✅ CancelReservation_HappyFlow_ReturnsOk
- ❌ CancelReservation_Without_Token_Should_Return_401
- ❌ CancelReservation_With_User_Cancelling_Other_User_Reservation_Should_Return_403
- ❌ CancelReservation_With_Admin_Should_Return_200
- ❌ CancelReservation_With_Invalid_Guid_Should_Return_400
- ❌ CancelReservation_Should_Handle_Exceptions_Gracefully

### ListReservationsByUser Endpoint
- ✅ ListReservationsByUser_InvalidUserId_ReturnsBadRequest
- ✅ ListReservationsByUser_HappyFlow_ReturnsOk
- ❌ ListReservationsByUser_Without_Token_Should_Return_401
- ❌ ListReservationsByUser_With_User_Viewing_Other_User_Reservations_Should_Return_403
- ❌ ListReservationsByUser_With_Admin_Should_Return_200
- ❌ ListReservationsByUser_With_Empty_Guid_Should_Return_400
- ❌ ListReservationsByUser_With_Status_Filter_Should_Return_Filtered

### CheckAvailability Endpoint
- ✅ CheckAvailability_InvalidRange_ReturnsBadRequest
- ✅ CheckAvailability_InvalidLotId_ReturnsBadRequest
- ✅ CheckAvailability_HappyFlow_ReturnsOk
- ❌ CheckAvailability_Without_Token_Should_Return_401
- ❌ CheckAvailability_With_StartTime_Equals_EndTime_Should_Return_400
- ❌ CheckAvailability_With_Empty_Guid_Should_Return_400
- ❌ CheckAvailability_With_Overlapping_Reservation_Should_Return_NotAvailable
- ❌ CheckAvailability_With_No_Overlapping_Reservation_Should_Return_Available

### CreateReservationForUser Endpoint
- ❌ CreateReservationForUser_Without_Token_Should_Return_401
- ❌ CreateReservationForUser_With_User_Token_Should_Return_403
- ❌ CreateReservationForUser_With_Admin_Token_Should_Return_200
- ❌ CreateReservationForUser_With_Null_Body_Should_Return_400
- ❌ CreateReservationForUser_With_Empty_Guid_Should_Return_400
- ❌ CreateReservationForUser_With_Invalid_TimeRange_Should_Return_400
- ❌ CreateReservationForUser_Should_Create_Reservation_With_Specified_Status

**Total: 12/44 (27%)**

---

## 5. C_Users Controller (Test_Users.cs)

### GetAllUsers Endpoint
- ✅ Test_Pagination_HappyFlow
- ✅ Test_Pagination_NegativePage
- ✅ Test_Pagination_EmptyPage
- ❌ GetAllUsers_Without_Token_Should_Return_401
- ❌ GetAllUsers_With_User_Token_Should_Return_403
- ❌ GetAllUsers_With_LotAdmin_Token_Should_Return_403
- ❌ GetAllUsers_With_SuperAdmin_Token_Should_Return_200
- ❌ GetAllUsers_With_Page_Exceeding_Total_Should_Return_400
- ❌ GetAllUsers_Should_Return_Paginated_Response

### GetUserByID Endpoint
- ✅ Test_GetById_UnknownID
- ❌ GetUserByID_Without_Token_Should_Return_401
- ❌ GetUserByID_With_Valid_Id_Should_Return_200
- ❌ GetUserByID_With_Invalid_Guid_Should_Return_400
- ❌ GetUserByID_With_Empty_Guid_Should_Return_400

### CreateUser Endpoint
- ✅ Test_Create_HappyFlow
- ✅ Test_Create_NotPossible_BirthDate
- ✅ Test_Create_WrongData
- ✅ Test_Invalid_Email
- ❌ CreateUser_Without_Token_Should_Return_401
- ❌ CreateUser_With_User_Token_Should_Return_403
- ❌ CreateUser_With_LotAdmin_Token_Should_Return_403
- ❌ CreateUser_With_SuperAdmin_Token_Should_Return_201
- ❌ CreateUser_With_Null_Body_Should_Return_400
- ❌ CreateUser_With_Empty_Username_Should_Return_400
- ❌ CreateUser_With_Empty_Password_Should_Return_400
- ❌ CreateUser_With_Empty_Name_Should_Return_400
- ❌ CreateUser_With_Empty_Email_Should_Return_400
- ❌ CreateUser_With_Empty_Phone_Should_Return_400
- ❌ CreateUser_Should_Hash_Password
- ❌ CreateUser_With_Invalid_Role_Should_Return_400

### UpdateUser Endpoint
- ✅ Test_Update_GoodData_WrongID
- ✅ Test_Update_WrongData
- ✅ Test_User_HappyFlow (includes update)
- ❌ UpdateUser_Without_Token_Should_Return_401
- ❌ UpdateUser_With_User_Token_Should_Return_403
- ❌ UpdateUser_With_LotAdmin_Token_Should_Return_403
- ❌ UpdateUser_With_SuperAdmin_Token_Should_Return_204
- ❌ UpdateUser_With_Invalid_Guid_Should_Return_400
- ❌ UpdateUser_With_Null_Body_Should_Return_400
- ❌ UpdateUser_Should_Update_All_Fields

### DeleteUser Endpoint
- ✅ Test_Delete_WrongID
- ✅ Test_User_HappyFlow (includes delete)
- ❌ DeleteUser_Without_Token_Should_Return_401
- ❌ DeleteUser_With_User_Token_Should_Return_403
- ❌ DeleteUser_With_LotAdmin_Token_Should_Return_403
- ❌ DeleteUser_With_SuperAdmin_Token_Should_Return_204
- ❌ DeleteUser_With_Invalid_Guid_Should_Return_400

**Total: 7/40 (18%)**

---

## 6. C_Vehicles Controller (Test_Vehicles.cs)

### GetAllVehicles Endpoint
- ✅ Test_Pagination_HappyFlow
- ✅ Test_Pagination_InvalidPageNumber
- ✅ Test_Pagination_NonIntegerPageNumber
- ✅ Test_Pagination_MissingPageNumber
- ❌ GetAllVehicles_Without_Token_Should_Return_401
- ❌ GetAllVehicles_With_User_Token_Should_Return_403
- ❌ GetAllVehicles_With_LotAdmin_Token_Should_Return_200
- ❌ GetAllVehicles_With_SuperAdmin_Token_Should_Return_200
- ❌ GetAllVehicles_With_Page_Exceeding_Total_Should_Return_400

### GetVehicleByID Endpoint
- ✅ Test_GetById_UnknownID
- ✅ Test_GetById_InvalidIDFormat
- ❌ GetVehicleByID_Without_Token_Should_Return_401
- ❌ GetVehicleByID_With_User_Viewing_Other_User_Vehicle_Should_Return_403
- ❌ GetVehicleByID_With_User_Viewing_Own_Vehicle_Should_Return_200
- ❌ GetVehicleByID_With_Admin_Should_Return_200
- ❌ GetVehicleByID_With_Empty_Guid_Should_Return_400

### CreateVehicle Endpoint
- ✅ Test_Create_HappyFlow
- ✅ Test_Create_BadData
- ✅ Test_Create_EmptyData
- ✅ Test_Create_NotPossible_Year
- ✅ Test_Create_NotPossible_LicensePlate
- ❌ CreateVehicle_Without_Token_Should_Return_401
- ❌ CreateVehicle_With_Null_Body_Should_Return_400
- ❌ CreateVehicle_With_User_Should_Set_User_Id_To_Current_User
- ❌ CreateVehicle_With_Admin_Should_Allow_Any_User_Id
- ❌ CreateVehicle_With_Admin_And_Empty_User_Id_Should_Use_Current_User
- ❌ CreateVehicle_With_Whitespace_Only_Fields_Should_Return_400
- ❌ CreateVehicle_With_Year_Equals_Current_Year_Should_Return_200

### UpdateVehicle Endpoint
- ✅ Test_Update_WrongID
- ✅ Test_FullFlow_HappyPath (includes update)
- ❌ UpdateVehicle_Without_Token_Should_Return_401
- ❌ UpdateVehicle_With_User_Updating_Other_User_Vehicle_Should_Return_403
- ❌ UpdateVehicle_With_User_Updating_Own_Vehicle_Should_Return_204
- ❌ UpdateVehicle_With_Admin_Should_Return_204
- ❌ UpdateVehicle_With_User_Should_Prevent_Ownership_Change
- ❌ UpdateVehicle_With_Admin_Should_Allow_Ownership_Change
- ❌ UpdateVehicle_With_Null_Body_Should_Return_400
- ❌ UpdateVehicle_With_Invalid_Guid_Should_Return_400

### DeleteVehicle Endpoint
- ✅ Test_FullFlow_HappyPath (includes delete)
- ❌ DeleteVehicle_Without_Token_Should_Return_401
- ❌ DeleteVehicle_With_User_Deleting_Other_User_Vehicle_Should_Return_403
- ❌ DeleteVehicle_With_User_Deleting_Own_Vehicle_Should_Return_204
- ❌ DeleteVehicle_With_Admin_Should_Return_204
- ❌ DeleteVehicle_With_Invalid_Guid_Should_Return_400
- ❌ DeleteVehicle_With_NonExistent_Id_Should_Return_404

**Total: 8/38 (21%)**

---

## 7. Service Layer Tests (ALL MISSING)

### Service_Payments
- ❌ All 15+ service method tests

### Service_Reservations
- ❌ All 12+ service method tests

### Service_Users
- ❌ All 9+ service method tests

### Service_Vehicles
- ❌ All 9+ service method tests

### Service_UserBalance
- ❌ All 12+ service method tests

### Service_Sessions
- ❌ All 8+ service method tests

### Service_Token
- ❌ All 6+ service method tests

### Service_TokenRevocation
- ❌ All 4+ service method tests

### Service_Billing
- ❌ All 6+ service method tests

### Service_Company
- ❌ All 5+ service method tests

### Service_Hotel
- ❌ All 5+ service method tests

### Service_Parkinglots
- ❌ All 5+ service method tests

### Service_Profile
- ❌ All 3+ service method tests

**Total: 0/100+ (0%)**

---

## Summary Statistics

### Controllers
- **Total Tests Needed**: ~250
- **Tests Existing**: ~50
- **Tests Missing**: ~200
- **Current Coverage**: ~20%

### Services
- **Total Tests Needed**: ~100
- **Tests Existing**: 0
- **Tests Missing**: ~100
- **Current Coverage**: 0%

### Overall
- **Total Tests Needed**: ~350
- **Tests Existing**: ~50
- **Tests Missing**: ~300
- **Current Coverage**: ~14%
- **Target Coverage**: 80%
- **Gap**: ~66%

---

## Quick Wins (High Impact, Low Effort)

1. **Add authorization tests** to all existing test files (~50 tests)
2. **Add null/empty validation tests** to all endpoints (~50 tests)
3. **Add service layer unit tests** for critical services (~50 tests)

These 150 tests alone would bring coverage from ~14% to ~50%+.
