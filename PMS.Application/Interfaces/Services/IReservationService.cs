using PMS.Application.DTOs.Common;
using PMS.Application.DTOs.Reservations;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.Interfaces.Services
{
    public interface IReservationService
    {
		Task<ResponseObjectDto<ReservationDto>> CreateReservationAsync(CreateReservationDto dto);
		Task<ResponseObjectDto<PagedResult<ReservationListDto>>> GetAllReservationsAsync(string? search, string? status, int pageNumber, int pageSize);
		Task<ResponseObjectDto<bool>> ChangeStatusAsync(ChangeReservationStatusDto dto);
		Task<ResponseObjectDto<ReservationDto>> GetReservationByIdAsync(int id);

		Task<ResponseObjectDto<bool>> DeleteReservationAsync(int id);

		Task<ResponseObjectDto<bool>> RestoreReservationAsync(int id);

		Task<ResponseObjectDto<ReservationDto>> UpdateReservationAsync(UpdateReservationDto dto);
	}
}
