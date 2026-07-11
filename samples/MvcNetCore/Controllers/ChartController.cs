using MapIdeaHub.BirSign.SharedKernel.Dtos;
using MapIdeaHub.BirSign.SharedKernel.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcNetCore.Data;
using MvcNetCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcNetCore.Controllers
{
    public class ChartController(
        ApplicationDbContext dbContext,
        IdsService idsService,
        UserManager<ApplicationUser> userManager) : Controller
    {
        private readonly ApplicationDbContext _dbContext = dbContext;
        private readonly IdsService _idsService = idsService;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        public IActionResult Index()
        {
            return View();
        }

        #region Chart API Data

        [HttpGet]
        public async Task<IActionResult> GetChartData()
        {
            try
            {
                var departments = await _dbContext.Departments
                    .AsNoTracking()
                    .ToListAsync();

                var positions = await _dbContext.Positions
                    .AsNoTracking()
                    .ToListAsync();

                var userPositions = await _dbContext.UserPositions
                    .Include(up => up.User)
                    .AsNoTracking()
                    .ToListAsync();

                var nodes = new List<object>();

                // Add Department Nodes
                foreach (var dept in departments)
                {
                    nodes.Add(new
                    {
                        id = $"dept_{dept.Id}",
                        parentId = dept.ParentDepartmentId.HasValue ? $"dept_{dept.ParentDepartmentId}" : null,
                        title = dept.Title,
                        fullName = dept.FullName ?? dept.Title,
                        level = dept.Level,
                        nodeType = "department",
                        color = "#3b82f6" // Blue
                    });
                }

                // Add Position Nodes (as children of their Departments)
                foreach (var pos in positions)
                {
                    nodes.Add(new
                    {
                        id = $"pos_{pos.Id}",
                        parentId = $"dept_{pos.DepartmentId}",
                        title = pos.Title,
                        description = pos.Description ?? "",
                        qualification = pos.QualificationCriteria ?? "",
                        positionType = pos.PositionType.ToString(),
                        row = pos.Row,
                        nodeType = "position",
                        color = pos.PositionType == MapIdeaHub.BirSign.SharedKernel.Enums.PositionType.Chief ? "#10b981" : "#84cc16" // Emerald or Lime
                    });
                }

                // Add User Nodes (as children of their Positions)
                foreach (var up in userPositions)
                {
                    if (up.User == null) continue;

                    nodes.Add(new
                    {
                        id = $"userpos_{up.Id}",
                        parentId = $"pos_{up.PositionId}",
                        title = $"{up.User.Name} {up.User.Family}".Trim(),
                        userName = up.User.UserName,
                        email = up.User.Email ?? "",
                        phone = up.User.PhoneNumber ?? "",
                        birthday = up.User.BirthDay ?? "",
                        gender = up.User.Gender.ToString(),
                        isActive = up.IsActive,
                        startDate = up.StartDate?.ToString("yyyy/MM/dd") ?? "",
                        endDate = up.EndDate?.ToString("yyyy/MM/dd") ?? "",
                        nodeType = "user",
                        color = up.IsActive ? "#f59e0b" : "#6b7280" // Amber (active) or Gray (inactive)
                    });
                }

                return Json(nodes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Sync()
        {
            try
            {
                var response = await _idsService.GetDepartmentTreeWithDetailsAsync();
                if (response == null || response.Data == null)
                {
                    return Json(new { success = false, message = "Could not fetch data from Identity Server API." });
                }

                var details = response.Data;

                using (var transaction = await _dbContext.Database.BeginTransactionAsync())
                {
                    // Clear existing local chart data to do a clean overwrite from identity service
                    _dbContext.UserPositions.RemoveRange(_dbContext.UserPositions);
                    _dbContext.Positions.RemoveRange(_dbContext.Positions);
                    _dbContext.Departments.RemoveRange(_dbContext.Departments);
                    await _dbContext.SaveChangesAsync();

                    // Save departments recursively starting from root
                    if (details.Department != null && details.Department.Id != Guid.Empty)
                    {
                        await SaveDepartmentTreeAsync(details.Department, null);
                    }

                    // Save positions
                    if (details.Positions != null)
                    {
                        foreach (var posDto in details.Positions)
                        {
                            var deptExists = await _dbContext.Departments.AnyAsync(d => d.Id == posDto.DepartmentId);
                            if (!deptExists) continue;

                            var pos = new Position
                            {
                                Id = posDto.Id,
                                Title = posDto.Title ?? "Unnamed Position",
                                DepartmentId = posDto.DepartmentId,
                                Description = posDto.Description,
                                QualificationCriteria = posDto.QualificationCriteria,
                                PositionType = posDto.PositionType,
                                Row = posDto.Row
                            };
                            _dbContext.Positions.Add(pos);
                        }
                        await _dbContext.SaveChangesAsync();
                    }

                    // Save users & user positions
                    if (details.UserPositions != null)
                    {
                        foreach (var upDto in details.UserPositions)
                        {
                            if (string.IsNullOrEmpty(upDto.UserId)) continue;

                            var user = await _dbContext.Users.FindAsync(upDto.UserId);
                            if (user == null)
                            {
                                user = new ApplicationUser
                                {
                                    Id = upDto.UserId,
                                    UserName = upDto.UserName ?? upDto.UserEmail ?? upDto.UserId,
                                    Email = upDto.UserEmail,
                                    PhoneNumber = upDto.UserPhoneNumber,
                                    Name = upDto.UserFirstName ?? "",
                                    Family = upDto.UserLastName ?? "",
                                    BirthDay = upDto.UserBirthday,
                                    NationalCode = upDto.UserId,
                                    EmailConfirmed = upDto.EmailConfirmed,
                                    PhoneNumberConfirmed = upDto.PhoneNumberConfirmed,
                                    Gender = upDto.Gender
                                };
                                user.SecurityStamp = Guid.NewGuid().ToString();
                                _dbContext.Users.Add(user);
                                await _dbContext.SaveChangesAsync();
                            }
                            else
                            {
                                user.Name = upDto.UserFirstName ?? user.Name;
                                user.Family = upDto.UserLastName ?? user.Family;
                                user.BirthDay = upDto.UserBirthday ?? user.BirthDay;
                                user.Gender = upDto.Gender;
                                _dbContext.Users.Update(user);
                            }

                            var posExists = await _dbContext.Positions.AnyAsync(p => p.Id == upDto.PositionId);
                            if (!posExists) continue;

                            var up = new UserPosition
                            {
                                Id = upDto.Id,
                                UserId = upDto.UserId,
                                PositionId = upDto.PositionId,
                                StartDate = upDto.StartDate,
                                EndDate = upDto.EndDate,
                                IsActive = upDto.IsActive
                            };
                            _dbContext.UserPositions.Add(up);
                        }
                        await _dbContext.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();
                }

                return Json(new { success = true, message = "Successfully synchronized and populated local chart data." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Sync failed: {ex.Message}" });
            }
        }

        private async Task SaveDepartmentTreeAsync(DepartmentApiDto deptDto, Guid? parentId)
        {
            var dept = new Department
            {
                Id = deptDto.Id,
                Title = deptDto.Title ?? "Unnamed Department",
                ParentDepartmentId = parentId,
                FullName = deptDto.FullName,
                Level = deptDto.Level
            };
            _dbContext.Departments.Add(dept);
            await _dbContext.SaveChangesAsync();

            if (deptDto.Children != null)
            {
                foreach (var child in deptDto.Children)
                {
                    await SaveDepartmentTreeAsync(child, dept.Id);
                }
            }
        }

        #endregion

        #region Departments CRUD

        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            var list = await _dbContext.Departments
                .Select(d => new { d.Id, d.Title, d.ParentDepartmentId, d.FullName, d.Level })
                .ToListAsync();
            return Json(list);
        }

        [HttpPost]
        public async Task<IActionResult> SaveDepartment([FromBody] Department model)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                return Json(new { success = false, message = "Title is required." });
            }

            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
                _dbContext.Departments.Add(model);
            }
            else
            {
                var existing = await _dbContext.Departments.FindAsync(model.Id);
                if (existing == null) return Json(new { success = false, message = "Department not found." });
                existing.Title = model.Title;
                existing.ParentDepartmentId = model.ParentDepartmentId;
                existing.FullName = model.FullName;
                existing.Level = model.Level;
                _dbContext.Departments.Update(existing);
            }

            await _dbContext.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDepartment(Guid id)
        {
            var existing = await _dbContext.Departments.FindAsync(id);
            if (existing == null) return Json(new { success = false, message = "Department not found." });

            var hasChildren = await _dbContext.Departments.AnyAsync(d => d.ParentDepartmentId == id);
            if (hasChildren) return Json(new { success = false, message = "Cannot delete department because it has child departments." });

            var hasPositions = await _dbContext.Positions.AnyAsync(p => p.DepartmentId == id);
            if (hasPositions) return Json(new { success = false, message = "Cannot delete department because positions are assigned to it." });

            _dbContext.Departments.Remove(existing);
            await _dbContext.SaveChangesAsync();
            return Json(new { success = true });
        }

        #endregion

        #region Positions CRUD

        [HttpGet]
        public async Task<IActionResult> GetPositions()
        {
            var list = await _dbContext.Positions
                .Include(p => p.Department)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.DepartmentId,
                    DepartmentTitle = p.Department != null ? p.Department.Title : "Unknown",
                    p.Description,
                    p.QualificationCriteria,
                    PositionType = (int)p.PositionType,
                    PositionTypeName = p.PositionType.ToString(),
                    p.Row
                })
                .ToListAsync();
            return Json(list);
        }

        [HttpPost]
        public async Task<IActionResult> SavePosition([FromBody] Position model)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                return Json(new { success = false, message = "Title is required." });
            }

            if (model.DepartmentId == Guid.Empty)
            {
                return Json(new { success = false, message = "Department is required." });
            }

            var deptExists = await _dbContext.Departments.AnyAsync(d => d.Id == model.DepartmentId);
            if (!deptExists)
            {
                return Json(new { success = false, message = "Target Department does not exist." });
            }

            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
                _dbContext.Positions.Add(model);
            }
            else
            {
                var existing = await _dbContext.Positions.FindAsync(model.Id);
                if (existing == null) return Json(new { success = false, message = "Position not found." });
                existing.Title = model.Title;
                existing.DepartmentId = model.DepartmentId;
                existing.Description = model.Description;
                existing.QualificationCriteria = model.QualificationCriteria;
                existing.PositionType = model.PositionType;
                existing.Row = model.Row;
                _dbContext.Positions.Update(existing);
            }

            await _dbContext.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeletePosition(Guid id)
        {
            var existing = await _dbContext.Positions.FindAsync(id);
            if (existing == null) return Json(new { success = false, message = "Position not found." });

            var hasUsers = await _dbContext.UserPositions.AnyAsync(up => up.PositionId == id);
            if (hasUsers) return Json(new { success = false, message = "Cannot delete position because users are assigned to it." });

            _dbContext.Positions.Remove(existing);
            await _dbContext.SaveChangesAsync();
            return Json(new { success = true });
        }

        #endregion

        #region Users CRUD

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var list = await _dbContext.Users
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Name,
                    u.Family,
                    u.Email,
                    u.PhoneNumber,
                    u.BirthDay,
                    Gender = (int)u.Gender,
                    GenderName = u.Gender.ToString()
                })
                .ToListAsync();
            return Json(list);
        }

        [HttpPost]
        public async Task<IActionResult> SaveUser([FromBody] ApplicationUser model)
        {
            if (string.IsNullOrWhiteSpace(model.UserName))
            {
                return Json(new { success = false, message = "Username is required." });
            }

            if (string.IsNullOrEmpty(model.Id))
            {
                model.Id = Guid.NewGuid().ToString();
                model.SecurityStamp = Guid.NewGuid().ToString();
                var result = await _userManager.CreateAsync(model);
                if (!result.Succeeded)
                {
                    return Json(new { success = false, message = string.Join(", ", result.Errors.Select(e => e.Description)) });
                }
            }
            else
            {
                var existing = await _userManager.FindByIdAsync(model.Id);
                if (existing == null) return Json(new { success = false, message = "User not found." });
                existing.UserName = model.UserName;
                existing.Name = model.Name;
                existing.Family = model.Family;
                existing.Email = model.Email;
                existing.PhoneNumber = model.PhoneNumber;
                existing.BirthDay = model.BirthDay;
                existing.Gender = model.Gender;

                var result = await _userManager.UpdateAsync(existing);
                if (!result.Succeeded)
                {
                    return Json(new { success = false, message = string.Join(", ", result.Errors.Select(e => e.Description)) });
                }
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var existing = await _userManager.FindByIdAsync(id);
            if (existing == null) return Json(new { success = false, message = "User not found." });

            var hasPositions = await _dbContext.UserPositions.AnyAsync(up => up.UserId == id);
            if (hasPositions) return Json(new { success = false, message = "Cannot delete user because they hold position assignments." });

            var result = await _userManager.DeleteAsync(existing);
            if (!result.Succeeded)
            {
                return Json(new { success = false, message = string.Join(", ", result.Errors.Select(e => e.Description)) });
            }

            return Json(new { success = true });
        }

        #endregion

        #region User Positions CRUD

        [HttpGet]
        public async Task<IActionResult> GetUserPositions()
        {
            var list = await _dbContext.UserPositions
                .Include(up => up.User)
                .Include(up => up.Position)
                .Select(up => new
                {
                    up.Id,
                    up.UserId,
                    UserName = up.User != null ? $"{up.User.Name} {up.User.Family}" : "Unknown",
                    up.PositionId,
                    PositionTitle = up.Position != null ? up.Position.Title : "Unknown",
                    StartDate = up.StartDate.HasValue ? up.StartDate.Value.ToString("yyyy-MM-dd") : null,
                    EndDate = up.EndDate.HasValue ? up.EndDate.Value.ToString("yyyy-MM-dd") : null,
                    up.IsActive
                })
                .ToListAsync();
            return Json(list);
        }

        [HttpPost]
        public async Task<IActionResult> SaveUserPosition([FromBody] UserPosition model)
        {
            if (string.IsNullOrEmpty(model.UserId))
            {
                return Json(new { success = false, message = "User is required." });
            }

            if (model.PositionId == Guid.Empty)
            {
                return Json(new { success = false, message = "Position is required." });
            }

            var userExists = await _dbContext.Users.AnyAsync(u => u.Id == model.UserId);
            if (!userExists) return Json(new { success = false, message = "Target User does not exist." });

            var posExists = await _dbContext.Positions.AnyAsync(p => p.Id == model.PositionId);
            if (!posExists) return Json(new { success = false, message = "Target Position does not exist." });

            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
                _dbContext.UserPositions.Add(model);
            }
            else
            {
                var existing = await _dbContext.UserPositions.FindAsync(model.Id);
                if (existing == null) return Json(new { success = false, message = "Assignment not found." });
                existing.UserId = model.UserId;
                existing.PositionId = model.PositionId;
                existing.StartDate = model.StartDate;
                existing.EndDate = model.EndDate;
                existing.IsActive = model.IsActive;
                _dbContext.UserPositions.Update(existing);
            }

            await _dbContext.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUserPosition(Guid id)
        {
            var existing = await _dbContext.UserPositions.FindAsync(id);
            if (existing == null) return Json(new { success = false, message = "Assignment not found." });

            _dbContext.UserPositions.Remove(existing);
            await _dbContext.SaveChangesAsync();
            return Json(new { success = true });
        }

        #endregion
    }
}
