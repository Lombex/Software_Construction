using CSharpAPI.Controllers.Utils;
using FluentAssertions;
using System;
using Xunit;

namespace CSharpAPI.Tests.APITests
{
    public class Test_C_Utils
    {
        [Fact]
        public void HashPassword_With_Valid_Password_Should_Return_Hash()
        {
            var password = "TestPassword123!";
            var hash = C_Utils.HashPassword(password);
            hash.Should().NotBeNullOrEmpty();
            hash.Should().StartWith("$2");
        }

        [Fact]
        public void HashPassword_With_Empty_Password_Should_Throw_Exception()
        {
            Assert.Throws<ArgumentException>(() => C_Utils.HashPassword(""));
        }

        [Fact]
        public void HashPassword_With_Whitespace_Password_Should_Throw_Exception()
        {
            Assert.Throws<ArgumentException>(() => C_Utils.HashPassword("   "));
        }

        [Fact]
        public void HashPassword_With_Null_Password_Should_Throw_Exception()
        {
            Assert.Throws<ArgumentException>(() => C_Utils.HashPassword(null!));
        }

        [Fact]
        public void VerifyPassword_With_Valid_BCrypt_Password_Should_Return_True()
        {
            var password = "TestPassword123!";
            var hash = C_Utils.HashPassword(password);
            var result = C_Utils.VerifyPassword(password, hash);
            result.Should().BeTrue();
        }

        [Fact]
        public void VerifyPassword_With_Invalid_Password_Should_Return_False()
        {
            var password = "TestPassword123!";
            var hash = C_Utils.HashPassword(password);
            var result = C_Utils.VerifyPassword("WrongPassword", hash);
            result.Should().BeFalse();
        }

        [Fact]
        public void VerifyPassword_With_Empty_Password_Should_Return_False()
        {
            var result = C_Utils.VerifyPassword("", "somehash");
            result.Should().BeFalse();
        }

        [Fact]
        public void VerifyPassword_With_Empty_Hash_Should_Return_False()
        {
            var result = C_Utils.VerifyPassword("password", "");
            result.Should().BeFalse();
        }

        [Fact]
        public void VerifyPassword_With_Null_Password_Should_Return_False()
        {
            var result = C_Utils.VerifyPassword(null!, "somehash");
            result.Should().BeFalse();
        }

        [Fact]
        public void VerifyPassword_With_Null_Hash_Should_Return_False()
        {
            var result = C_Utils.VerifyPassword("password", null!);
            result.Should().BeFalse();
        }

        [Fact]
        public void VerifyPassword_With_Invalid_BCrypt_Hash_Should_Return_False()
        {
            var result = C_Utils.VerifyPassword("password", "$2a$invalidhash");
            result.Should().BeFalse();
        }

        [Fact]
        public void VerifyPassword_With_SHA256_Legacy_Hash_Should_Work()
        {
            // Create a legacy SHA256 hash manually for testing
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes("legacyPassword");
                var hash = sha256.ComputeHash(bytes);
                var hashString = Convert.ToBase64String(hash);

                var result = C_Utils.VerifyPassword("legacyPassword", hashString);
                result.Should().BeTrue();
            }
        }

        [Fact]
        public void IsLegacyHash_With_BCrypt_Hash_Should_Return_False()
        {
            var password = "TestPassword123!";
            var hash = C_Utils.HashPassword(password);
            var result = C_Utils.IsLegacyHash(hash);
            result.Should().BeFalse();
        }

        [Fact]
        public void IsLegacyHash_With_SHA256_Hash_Should_Return_True()
        {
            // Create a SHA256 hash manually
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes("password");
                var hash = sha256.ComputeHash(bytes);
                var hashString = Convert.ToBase64String(hash);

                var result = C_Utils.IsLegacyHash(hashString);
                result.Should().BeTrue();
            }
        }

        [Fact]
        public void IsValidEmail_With_Valid_Email_Should_Return_True()
        {
            var result = C_Utils.IsValidEmail("test@example.com");
            result.Should().BeTrue();
        }

        [Fact]
        public void IsValidEmail_With_Invalid_Email_Should_Return_False()
        {
            var result = C_Utils.IsValidEmail("invalid-email");
            result.Should().BeFalse();
        }

        [Fact]
        public void IsValidPhoneNumber_With_Valid_Phone_Should_Return_True()
        {
            var result = C_Utils.IsValidPhoneNumber("0612345678");
            result.Should().BeTrue();
        }

        [Fact]
        public void IsValidPhoneNumber_With_Invalid_Phone_Should_Return_False()
        {
            var result = C_Utils.IsValidPhoneNumber("invalid");
            result.Should().BeFalse();
        }

        [Fact]
        public void IsValidPhoneNumber_With_Phone_With_Plus_Should_Return_False()
        {
            var result = C_Utils.IsValidPhoneNumber("+31612345678");
            result.Should().BeFalse();
        }

        [Fact]
        public void IsValidPhoneNumber_With_Too_Short_Phone_Should_Return_False()
        {
            var result = C_Utils.IsValidPhoneNumber("123");
            result.Should().BeFalse();
        }

        [Fact]
        public void IsValidPhoneNumber_With_Too_Long_Phone_Should_Return_False()
        {
            var result = C_Utils.IsValidPhoneNumber("12345678901234567890");
            result.Should().BeFalse();
        }
    }
}