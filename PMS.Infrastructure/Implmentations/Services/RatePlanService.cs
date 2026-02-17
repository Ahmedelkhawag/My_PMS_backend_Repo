using Microsoft.EntityFrameworkCore;
using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Configuration;
using PMS.Application.Interfaces.Services;
using PMS.Application.Interfaces.UOF;
using PMS.Domain.Entities.Configuration;
using PMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PMS.Infrastructure.Implmentations.Services
{
    public class RatePlanService : IRatePlanService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RatePlanService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseObjectDto<List<RatePlanDto>>> GetAllAsync(bool? isPublicOnly = null)
        {
            var query = _unitOfWork.RatePlans.GetQueryable()
                .Where(rp => !rp.IsDeleted);

            if (isPublicOnly == true)
            {
                query = query.Where(rp => rp.IsPublic && rp.IsActive);
            }

            var items = await query
                .OrderBy(rp => rp.Name)
                .Select(rp => new RatePlanDto
                {
                    Id = rp.Id,
                    Code = rp.Code,
                    Name = rp.Name,
                    Description = rp.Description,
                    RateType = rp.RateType,
                    RateValue = rp.RateValue,
                    IsPublic = rp.IsPublic,
                    IsActive = rp.IsActive
                })
                .ToListAsync();

            return new ResponseObjectDto<List<RatePlanDto>>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Rate plans retrieved successfully",
                Data = items
            };
        }

        public async Task<ResponseObjectDto<RatePlanDto>> GetByIdAsync(int id)
        {
            var entity = await _unitOfWork.RatePlans.GetQueryable()
                .FirstOrDefaultAsync(rp => rp.Id == id && !rp.IsDeleted);

            if (entity == null)
            {
                return new ResponseObjectDto<RatePlanDto>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = "Rate plan not found"
                };
            }

            return new ResponseObjectDto<RatePlanDto>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Rate plan retrieved successfully",
                Data = MapToDto(entity)
            };
        }

        public async Task<ResponseObjectDto<RatePlanDto>> CreateAsync(CreateRatePlanDto dto)
        {
            var validation = ValidateRateValue(dto.RateType, dto.RateValue);
            if (!validation.IsSuccess)
            {
                return new ResponseObjectDto<RatePlanDto>
                {
                    IsSuccess = false,
                    StatusCode = validation.StatusCode,
                    Message = validation.Message
                };
            }

            var existing = await _unitOfWork.RatePlans.GetQueryable()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(rp => rp.Code.ToLower() == dto.Code.ToLower());

            if (existing != null && !existing.IsDeleted)
            {
                return new ResponseObjectDto<RatePlanDto>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Rate plan code must be unique"
                };
            }

            var entity = new RatePlan
            {
                Code = dto.Code.Trim(),
                Name = dto.Name.Trim(),
                Description = dto.Description,
                RateType = dto.RateType,
                RateValue = dto.RateValue,
                IsPublic = dto.IsPublic,
                IsActive = dto.IsActive
            };

            await _unitOfWork.RatePlans.AddAsync(entity);
            await _unitOfWork.CompleteAsync();

            return new ResponseObjectDto<RatePlanDto>
            {
                IsSuccess = true,
                StatusCode = 201,
                Message = "Rate plan created successfully",
                Data = MapToDto(entity)
            };
        }

        public async Task<ResponseObjectDto<RatePlanDto>> UpdateAsync(UpdateRatePlanDto dto)
        {
            var entity = await _unitOfWork.RatePlans.GetQueryable()
                .FirstOrDefaultAsync(rp => rp.Id == dto.Id && !rp.IsDeleted);

            if (entity == null)
            {
                return new ResponseObjectDto<RatePlanDto>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = "Rate plan not found"
                };
            }

            var validation = ValidateRateValue(dto.RateType, dto.RateValue);
            if (!validation.IsSuccess)
            {
                return new ResponseObjectDto<RatePlanDto>
                {
                    IsSuccess = false,
                    StatusCode = validation.StatusCode,
                    Message = validation.Message
                };
            }

            entity.Name = dto.Name.Trim();
            entity.Description = dto.Description;
            entity.RateType = dto.RateType;
            entity.RateValue = dto.RateValue;
            entity.IsPublic = dto.IsPublic;
            entity.IsActive = dto.IsActive;

            _unitOfWork.RatePlans.Update(entity);
            await _unitOfWork.CompleteAsync();

            return new ResponseObjectDto<RatePlanDto>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Rate plan updated successfully",
                Data = MapToDto(entity)
            };
        }

        public async Task<ResponseObjectDto<bool>> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.RatePlans.GetQueryable()
                .FirstOrDefaultAsync(rp => rp.Id == id && !rp.IsDeleted);

            if (entity == null)
            {
                return new ResponseObjectDto<bool>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = "Rate plan not found",
                    Data = false
                };
            }

            var hasActiveReservations = await _unitOfWork.Reservations.GetQueryable()
                .AnyAsync(r => r.RatePlanId == id && !r.IsDeleted && r.Status != ReservationStatus.Cancelled);

            if (hasActiveReservations)
            {
                return new ResponseObjectDto<bool>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Cannot delete rate plan because it is linked to active reservations.",
                    Data = false
                };
            }

            var hasCompanies = await _unitOfWork.CompanyProfiles.GetQueryable()
                .AnyAsync(c => c.RatePlanId == id && !c.IsDeleted);

            if (hasCompanies)
            {
                return new ResponseObjectDto<bool>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Cannot delete rate plan because it is linked to active companies.",
                    Data = false
                };
            }

            entity.IsDeleted = true;
            entity.IsActive = false;

            _unitOfWork.RatePlans.Update(entity);
            await _unitOfWork.CompleteAsync();

            return new ResponseObjectDto<bool>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Rate plan deleted successfully",
                Data = true
            };
        }

        private static ResponseObjectDto<bool> ValidateRateValue(RateType rateType, decimal rateValue)
        {
            if (rateValue < 0)
            {
                return new ResponseObjectDto<bool>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Rate value cannot be negative"
                };
            }

            if (rateType == RateType.PercentageDiscount && rateValue > 100)
            {
                return new ResponseObjectDto<bool>
                {
                    IsSuccess = false,
                    StatusCode = 400,
                    Message = "Percentage discount cannot exceed 100%"
                };
            }

            return new ResponseObjectDto<bool>
            {
                IsSuccess = true,
                StatusCode = 200,
                Message = "Valid"
            };
        }

        private static RatePlanDto MapToDto(RatePlan entity)
        {
            return new RatePlanDto
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                Description = entity.Description,
                RateType = entity.RateType,
                RateValue = entity.RateValue,
                IsPublic = entity.IsPublic,
                IsActive = entity.IsActive
            };
        }
    }
}

