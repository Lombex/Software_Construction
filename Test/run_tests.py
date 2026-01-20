#!/usr/bin/env python3
"""
Simple test runner for integration tests without pytest dependency
"""
import sys
import os
import traceback

# Add the current directory to Python path
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

# Import the test module
try:
    import test_integration
    print("✅ Successfully imported test_integration module")
except ImportError as e:
    print(f"❌ Failed to import test_integration: {e}")
    sys.exit(1)

def run_test(test_func, test_name):
    """Run a single test function and report results"""
    try:
        print(f"\n🧪 Running {test_name}...")
        test_func()
        print(f"✅ {test_name} PASSED")
        return True
    except Exception as e:
        print(f"❌ {test_name} FAILED: {e}")
        traceback.print_exc()
        return False

def main():
    """Run all integration tests"""
    print("🚀 Starting Integration Tests")
    print("=" * 50)
    
    # List of tests to run - comprehensive integration testing
    tests = [
        # Authentication tests (working endpoints)
        (test_integration.test_auth_protection_profile_requires_token, "test_auth_protection_profile_requires_token"),
        (test_integration.test_auth_protection_vehicles_requires_token, "test_auth_protection_vehicles_requires_token"),
        (test_integration.test_auth_protection_payments_requires_token, "test_auth_protection_payments_requires_token"),
        (test_integration.test_auth_protection_billing_requires_token, "test_auth_protection_billing_requires_token"),
        (test_integration.test_logout_without_token_returns_400, "test_logout_without_token_returns_400"),
        
        # Error handling tests
        (test_integration.test_invalid_parking_lot_id_returns_404, "test_invalid_parking_lot_id_returns_404"),
        (test_integration.test_parking_lot_by_id_requires_auth, "test_parking_lot_by_id_requires_auth"),
        
        # Critical bug detection tests (these are valuable integration tests!)
        (test_integration.test_parking_lots_endpoint_crashes_server, "test_parking_lots_endpoint_crashes_server"),
        (test_integration.test_parking_lots_with_auth_token_still_crashes, "test_parking_lots_with_auth_token_still_crashes"),
        (test_integration.test_server_stability_after_crash, "test_server_stability_after_crash"),
        
        # Comprehensive integration tests
        (test_integration.test_authentication_flow_integration, "test_authentication_flow_integration"),
        (test_integration.test_error_response_consistency, "test_error_response_consistency"),
        (test_integration.test_http_method_support, "test_http_method_support"),
    ]
    
    passed = 0
    failed = 0
    skipped = 0
    
    for test_func, test_name in tests:
        try:
            if run_test(test_func, test_name):
                passed += 1
            else:
                failed += 1
        except Exception as e:
            if "skip" in str(e).lower():
                print(f"⏭️  {test_name} SKIPPED: {e}")
                skipped += 1
            else:
                print(f"❌ {test_name} FAILED: {e}")
                failed += 1
    
    print("\n" + "=" * 50)
    print("📊 TEST RESULTS SUMMARY")
    print("=" * 50)
    print(f"✅ Passed: {passed}")
    print(f"❌ Failed: {failed}")
    print(f"⏭️  Skipped: {skipped}")
    print(f"📈 Total: {passed + failed + skipped}")
    
    if failed == 0:
        print("\n🎉 ALL TESTS PASSED!")
        return 0
    else:
        print(f"\n⚠️  {failed} TESTS FAILED")
        return 1

if __name__ == "__main__":
    sys.exit(main())
