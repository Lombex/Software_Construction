using CSharpAPI.Models;
using CSharpAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace CSharpAPI.Controllers
{
    [ApiController]
    [Route("api/v2/company")]
    [Authorize]
    public class C_Company : ControllerBase
    {
        private readonly ICompanyService _companyService;
        private readonly ILogger<C_Company> _logger;

        public C_Company(ICompanyService companyService, ILogger<C_Company> logger)
        {
            _companyService = companyService;
            _logger = logger;
        }

        // Get all companies (admin only)
        [HttpGet]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var companies = await _companyService.GetAll();
                return Ok(companies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all companies");
                return StatusCode(500, "Error retrieving companies.");
            }
        }

        // Get company by ID
        [HttpGet("{id}")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var company = await _companyService.GetById(id);
                if (company == null)
                    return NotFound();
                return Ok(company);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting company {CompanyId}", id);
                return StatusCode(500, "Error retrieving company.");
            }
        }

        // Create company (admin only)
        [HttpPost]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> Create([FromBody] M_Company company)
        {
            try
            {
                var created = await _companyService.Create(company);
                return CreatedAtAction(nameof(GetById), new { id = created.id }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating company");
                return StatusCode(500, "Error creating company.");
            }
        }

        // Update company (admin only)
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> Update(Guid id, [FromBody] M_Company company)
        {
            try
            {
                company.id = id;
                var updated = await _companyService.Update(company);
                if (!updated)
                    return NotFound();
                return Ok(company);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating company {CompanyId}", id);
                return StatusCode(500, "Error updating company.");
            }
        }

        // Delete company (super admin only)
        [HttpDelete("{id}")]
        [Authorize(Policy = "SuperAdminOnly")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleted = await _companyService.Delete(id);
                if (!deleted)
                    return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting company {CompanyId}", id);
                return StatusCode(500, "Error deleting company.");
            }
        }

        // Get company vehicles
        [HttpGet("{id}/vehicles")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> GetCompanyVehicles(Guid id)
        {
            try
            {
                var vehicles = await _companyService.GetCompanyVehicles(id);
                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicles for company {CompanyId}", id);
                return StatusCode(500, "Error retrieving vehicles.");
            }
        }

        // Add user to company
        [HttpPost("{id}/users")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> AddUserToCompany(Guid id, [FromBody] AddUserToCompanyRequest request)
        {
            try
            {
                var added = await _companyService.AddUserToCompany(id, request.UserId, request.Role);
                if (!added)
                    return BadRequest("User is already in company or invalid request.");
                return Ok(new { message = "User added to company successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user to company {CompanyId}", id);
                return StatusCode(500, "Error adding user to company.");
            }
        }

        // Get company users
        [HttpGet("{id}/users")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> GetCompanyUsers(Guid id)
        {
            try
            {
                var users = await _companyService.GetCompanyUsers(id);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users for company {CompanyId}", id);
                return StatusCode(500, "Error retrieving users.");
            }
        }

        // Generate monthly bundle invoice
        [HttpPost("{id}/billing/monthly")]
        [Authorize(Policy = "AdminOrAbove")]
        public async Task<IActionResult> GenerateMonthlyBundle(Guid id, [FromBody] GenerateBundleRequest request)
        {
            try
            {
                var invoices = await _companyService.GenerateMonthlyBundle(id, request.Month);
                return Ok(invoices);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating monthly bundle for company {CompanyId}", id);
                return StatusCode(500, "Error generating monthly bundle.");
            }
        }
    }

    public class AddUserToCompanyRequest
    {
        public Guid UserId { get; set; }
        public CompanyUserRole Role { get; set; }
    }

    public class GenerateBundleRequest
    {
        public DateTime Month { get; set; }
    }
}

