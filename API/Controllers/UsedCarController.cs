using API.Requests.UsedCar;
using API.Responses.Auth;
using API.Responses.UsedCar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Context;
using System.Security.Claims;
using System.Text.Json;

namespace API.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class UsedCarController : ControllerBase
    {
        private readonly CarFinderDbContext _context;
        private readonly ILogger<UsedCarController> _logger;
        private readonly IWebHostEnvironment _environment;
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };

        public UsedCarController(CarFinderDbContext context, ILogger<UsedCarController> logger, IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        [HttpPost("AddYourUsedCar")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> AddYourUsedCar([FromForm] AddUsedCarRequest request)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                
                if (userId == 0)
                {
                    _logger.LogWarning("Unauthorized add used car attempt - no valid user ID in claims");
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid user context"));
                }

                // Validate images
                if (request.Images == null || request.Images.Count == 0)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("At least one image is required"));
                }

                if (request.Images.Count > 10)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Maximum 10 images allowed"));
                }

                var validationErrors = new List<string>();
                foreach (var image in request.Images)
                {
                    if (image.Length > MaxFileSize)
                    {
                        validationErrors.Add($"Image {image.FileName} exceeds maximum size of 5 MB");
                    }

                    if (!image.ContentType.StartsWith("image/"))
                    {
                        validationErrors.Add($"File {image.FileName} is not a valid image");
                    }

                    var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
                    if (!AllowedExtensions.Contains(extension))
                    {
                        validationErrors.Add($"Image {image.FileName} has invalid extension. Allowed: .jpg, .jpeg, .png");
                    }
                }

                if (validationErrors.Any())
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(string.Join("; ", validationErrors)));
                }

                // Save images to wwwroot/used-cars
                var uploadPath = Path.Combine(_environment.WebRootPath, "used-cars");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var savedImagePaths = new List<string>();
                foreach (var image in request.Images)
                {
                    var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                    var filePath = Path.Combine(uploadPath, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    // Store relative path
                    savedImagePaths.Add($"/used-cars/{uniqueFileName}");
                }

                var createdAtYear = request.CreatedAt ?? DateTime.UtcNow.Year;
                
                if (request.CreatedAt.HasValue && (request.CreatedAt.Value < 1900 || request.CreatedAt.Value > DateTime.UtcNow.Year + 1))
                {
                    createdAtYear = DateTime.UtcNow.Year;
                }

                var usedCar = new Core.Entities.UsedCar.UsedCar
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Name = request.Name,
                    Images = JsonSerializer.Serialize(savedImagePaths),
                    Price = request.Price,
                    Description = request.Description,
                    City = request.City,
                    BuyerPhoneNumber = request.BuyerPhoneNumber,
                    CreatedAtYear = createdAtYear,
                    CreatedAt = DateTime.UtcNow
                };

                _context.UsedCars.Add(usedCar);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Used car added successfully - UserId:{UserId}, CarId:{CarId}, Name:{Name}, ImagesCount:{ImagesCount}", 
                    userId, usedCar.Id, usedCar.Name, savedImagePaths.Count);

                return Ok(ApiResponse<object>.SuccessResponse(new { id = usedCar.Id }, "Used car added successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding used car - UserId:{UserId}", GetUserIdFromClaims());
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while adding the used car"));
            }
        }

        [HttpGet("GetAllUsedCars")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllUsedCars([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                var totalCount = await _context.UsedCars.CountAsync();
                
                var usedCars = await _context.UsedCars
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var response = usedCars.Select(car => new UsedCarResponse
                {
                    Id = car.Id,
                    Name = car.Name,
                    Price = car.Price,
                    Description = car.Description,
                    City = car.City,
                    BuyerPhoneNumber = car.BuyerPhoneNumber,
                    CreatedAtYear = car.CreatedAtYear,
                    Images = ParseImages(car.Images)
                }).ToList();

                var result = new
                {
                    usedCars = response,
                    totalCount,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                };

                _logger.LogInformation("Retrieved {Count} used cars - Page:{Page}, PageSize:{PageSize}", 
                    response.Count, page, pageSize);

                return Ok(ApiResponse<object>.SuccessResponse(result, "Used cars retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving used cars");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving used cars"));
            }
        }

        private List<string> ParseImages(string imagesJson)
        {
            if (string.IsNullOrWhiteSpace(imagesJson))
            {
                return new List<string>();
            }

            try
            {
                var images = JsonSerializer.Deserialize<List<string>>(imagesJson);
                return images ?? new List<string>();
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse images JSON: {ImagesJson}", imagesJson);
                return new List<string>();
            }
        }

        private int GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                              User.FindFirst("user_id")?.Value;
            
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return 0;
        }
    }
}
